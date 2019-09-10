using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.Cryptography;
using Neuralia.Blockchains.Core.P2p.Messages.Base;
using Neuralia.Blockchains.Core.P2p.Messages.MessageSets;
using Neuralia.Blockchains.Core.P2p.Messages.RoutingHeaders;
using Neuralia.Blockchains.Core.P2p.Workflows;
using Neuralia.Blockchains.Core.P2p.Workflows.Handshake;
using Neuralia.Blockchains.Core.Services;
using Neuralia.Blockchains.Core.Tools;
using Neuralia.Blockchains.Core.Workflows;
using Neuralia.Blockchains.Core.Workflows.Base;
using Neuralia.Blockchains.Core.Workflows.Tasks;
using Neuralia.Blockchains.Core.Workflows.Tasks.Receivers;
using Neuralia.Blockchains.Core.Workflows.Tasks.Routing;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.General.ExclusiveOptions;
using Neuralia.Blockchains.Tools.Threading;
using Serilog;

namespace Neuralia.Blockchains.Core.P2p.Connections {

	public interface IMessagingManager<R> : ILoopThread<MessagingManager<R>>, IColoredRoutedTaskHandler
		where R : IRehydrationFactory {
	}

	/// <summary>
	///     A special coordinator thread that is responsible for managing various aspects of the networking stack
	/// </summary>
	public class MessagingManager<R> : LoopThread<MessagingManager<R>>, IMessagingManager<R>
		where R : IRehydrationFactory {
		private const int MAX_SECONDS_BEFORE_NEXT_PEER_LIST_REQUEST = 40; //3*60;
		private const int MAX_SECONDS_BEFORE_NEXT_CONNECTION_ATTEMPT = 20; //1*60;

		/// <summary>
		///     our message rehydrator for messages that belong to no chain
		/// </summary>
		protected readonly R chainlessBlockchainEventsRehydrationFactory;

		protected readonly IClientWorkflowFactory<R> clientWorkflowFactory;

		private readonly IDataAccessService dataAccessService;

		protected readonly DataDispatcher dataDispatcher;

		protected readonly IGlobalsService globalsService;

		protected readonly INetworkingService<R> networkingService;

		/// <summary>
		///     gossip messages that are ready to go out
		/// </summary>
		/// <returns></returns>
		protected readonly Dictionary<string, PeerMessageQueue> outgoingMessageQueue = new Dictionary<string, PeerMessageQueue>();

		/// <summary>
		///     The receiver that allows us to act as a task endpoint mailbox
		/// </summary>
		protected readonly ColoredRoutedTaskReceiver RoutedTaskReceiver;

		protected readonly IServerWorkflowFactory<R> serverWorkflowFactory;

		protected readonly ServiceSet<R> serviceSet;

		protected readonly ITimeService timeService;

		private DateTime? nextAction;
		private DateTime? nextDatabaseClean;

		public MessagingManager(ServiceSet<R> serviceSet) : base(10) {

			this.globalsService = serviceSet.GlobalsService;
			this.networkingService = (INetworkingService<R>) DIService.Instance.GetService<INetworkingService>();
			this.timeService = serviceSet.TimeService;
			this.dataAccessService = serviceSet.DataAccessService;

			this.clientWorkflowFactory = serviceSet.InstantiationService.GetClientWorkflowFactory(serviceSet);
			this.serverWorkflowFactory = serviceSet.InstantiationService.GetServerWorkflowFactory(serviceSet);

			this.serviceSet = serviceSet;

			this.RoutedTaskReceiver = new ColoredRoutedTaskReceiver(this.HandleTask);

			this.dataDispatcher = new DataDispatcher(serviceSet.TimeService);
		}

		/// <summary>
		///     interface method to receive tasks into our mailbox
		/// </summary>
		/// <param name="task"></param>
		public void ReceiveTask(IColoredTask task) {
			this.RoutedTaskReceiver.ReceiveTask(task);
		}

		/// <summary>
		///     handle any message (task) that we may have recived
		/// </summary>
		/// <param name="task"></param>
		protected virtual void HandleTask(IColoredTask task) {
			try {
				if(task is MessageReceivedTask messageReceivedTask) {
					this.HandleMessageReceived(messageReceivedTask);
				} else if(task is ForwardGossipMessageTask forwardGossipMessageTask) {
					this.HandleForwardGossipMessageTask(forwardGossipMessageTask);
				} else if(task is PostNewGossipMessageTask postNewGossipMessageTask) {
					this.HandlePostNewGossipMessageTask(postNewGossipMessageTask);
				}

			} catch(Exception ex) {
				Log.Error(ex, "failed to handle task");
			}
		}

		protected virtual void CleanMesageCache() {
			try {
				this.dataAccessService.CreateMessageRegistryDal(this.globalsService.GetSystemStorageDirectoryPath(), this.serviceSet).CleanMessageCache();
			} catch(Exception ex) {
				Log.Error(ex, "failed to clean the message cache.");
			}
		}

		/// <summary>
		///     here we handle the forwarding of a valid gossip message we have received, and will move ahead
		/// </summary>
		/// <param name="forwardGossipMessageTask"></param>
		protected void HandleForwardGossipMessageTask(ForwardGossipMessageTask forwardGossipMessageTask) {

			// ok, this is a valid message, it went through our hoops. so lets be nice and forward it to whoever will want it
			this.ForwardValidGossipMessage(forwardGossipMessageTask.gossipMessageSet);

		}

		/// <summary>
		///     This method allows us to post a brand new gossip message to our peers.
		/// </summary>
		/// <remarks>USE WITH CAUTION!! our peers can blacklist us if we abuse it.</remarks>
		/// <param name="postNewGossipMessageTask"></param>
		protected void HandlePostNewGossipMessageTask(PostNewGossipMessageTask sendGossipMessageTask) {
			if(sendGossipMessageTask == null) {
				throw new ApplicationException("Cannot send null gossip message");
			}

			// ok, lets send it out. first we prepare it

			// lets hash it if it was not already
			if(sendGossipMessageTask.gossipMessageSet.BaseHeader.Hash == 0) {
				HashingUtils.HashGossipMessageSet(sendGossipMessageTask.gossipMessageSet);
			}

			// now we add it to our database as already received, we dont need to get it back from other peers. we set it as valid, since this is our own message
			this.dataAccessService.CreateMessageRegistryDal(this.globalsService.GetSystemFilesDirectoryPath(), this.serviceSet).AddMessageToCache(sendGossipMessageTask.gossipMessageSet.BaseHeader.Hash, true, true);

			// ok, now we can forward it to our peers
			this.ForwardValidGossipMessage(sendGossipMessageTask.gossipMessageSet);
		}

		/// <summary>
		///     this method ensures the verification of a received gossip message and if necessary, its forwarding to other peers.
		///     we also determine if we should process it, or ignore it
		/// </summary>
		/// <param name="gossipHeader"></param>
		/// <param name="task"></param>
		/// <returns>result true if we should process the </returns>
		protected (bool messageInCache, bool messageValid, IGossipMessageSet gossipMessageSet) ProcessReceivedGossipMessage(GossipHeader gossipHeader, MessageReceivedTask task) {
			// ok, a gossip message, these are special, we must forward them if they are new

			bool returnMessageToSender = false;

			R chainFactory = ((NetworkingService<R>) this.networkingService).ChainRehydrationFactories[gossipHeader.chainId];

			if(chainFactory == null) {
				throw new ApplicationException("Failed to obtain the chain's registered rehydration factory. we can not inspect the chain specific gossip message and validate it");
			}

			IGossipMessageSet gossipMessageSet = this.networkingService.MessageFactory.RehydrateGossipMessage(task.data, gossipHeader, chainFactory);

			if(gossipMessageSet == null) {
				throw new ApplicationException("Failed to rehydrate the chain gossip message");
			}

			// first step, validate the hash

			if(!HashingUtils.ValidateGossipMessageSetHash(gossipMessageSet)) {
				throw new ApplicationException("Invalid gossip message hash");
			}

			// ok at this point, the hash is valid, lets keep going

			// lets take in the potential optionsBase in the message
			if(gossipMessageSet.BaseHeader.NetworkOptions.HasOption((byte) GossipHeader.GossipNetworkMessageOptions.ReturnMeMessage)) {
				// ok, they want us to resend the message to them, if its valid. we do this so that when we send a new transaction to our peers, we will get the message from them
				// if they consider it to be valid. we can know that they agreed about our message
				returnMessageToSender = true;

				// for sure, we remove this option, since WE dont pass it on other peers.
				gossipMessageSet.BaseHeader.NetworkOptions.RemoveOption((byte) GossipHeader.GossipNetworkMessageOptions.ReturnMeMessage);

				// also remove it from the deserialized version, since odds are high we will forward it.
				// we can do this as the network optionsBase is the only byte that is not part of the message hash. hence, it is designed to be changed.
				if(gossipMessageSet.HasDeserializedData) {
					// its always the first byte

					ByteExclusiveOption options = gossipMessageSet.DeserializedData[0];
					options.RemoveOption((byte) GossipHeader.GossipNetworkMessageOptions.ReturnMeMessage);
					gossipMessageSet.DeserializedData[0] = options;
				}
			}

			// next lets confirm we have not processed this message before, and record the gossip message so we dont process it again
			(bool messageInCache, bool messageValid) = this.dataAccessService.CreateMessageRegistryDal(this.globalsService.GetSystemFilesDirectoryPath(), this.serviceSet).CheckRecordMessageInCache(gossipMessageSet.BaseHeader.Hash, task, returnMessageToSender);

			if(messageInCache) {

			}

			if(messageValid) {
				// if we get here, its because we had already processed it before, and it was valid. lets forward it to any peer that may not have received it since
				this.ForwardValidGossipMessage(gossipMessageSet);
			}

			return (messageInCache, messageValid, gossipMessageSet);

		}

		/// <summary>
		///     this method will forward a gossip message to any connected peer that our cache indicates has never received it and
		///     update the cache to reflect so
		/// </summary>
		private void ForwardValidGossipMessage(IGossipMessageSet gossipMessageSet) {

			// gossip messages are only sent to nodes that support them
			var gossipConnections = this.networkingService.ConnectionStore.BasicGossipConnectionsList;

			// if its a block, then we send it only to the full types of nodes
			if(gossipMessageSet.MinimumNodeTypeSupport.HasFlag(Enums.PeerTypeSupport.FullGossip)) {
				gossipConnections = this.networkingService.ConnectionStore.FullGossipConnectionsList;
			}

			this.dataAccessService.CreateMessageRegistryDal(this.globalsService.GetSystemFilesDirectoryPath(), this.serviceSet).ForwardValidGossipMessage(gossipMessageSet.BaseHeader.Hash, gossipConnections.Select(c => c.ScopedIp).ToList(), peerNotReceived => {
				// first update our peers to match any new connection since
				this.UpdatePeerConnections();

				var sentIps = new List<string>();

				// and send the message to those who have not received it (as far as we know)
				foreach(string sendpeer in peerNotReceived) // thats it, now we add the outbound message to this peer
				{
					try {
						if(this.outgoingMessageQueue.ContainsKey(sendpeer)) {
							this.outgoingMessageQueue[sendpeer].outboundMessages.Add(gossipMessageSet);
							sentIps.Add(sendpeer);
						}
					} catch(Exception ex) {
						//not much to do here, just eat it up. what matters is that we send it to others
						Log.Error(ex, "Failed to forward valid gossip message");
					}
				}

				// return the list of peers we forwarded it to
				return sentIps;
			});
		}

		/// <summary>
		///     make sure that we update our peer message queues to reflect any new peer connection we may have now
		/// </summary>
		private void UpdatePeerConnections() {
			var connections = this.networkingService.ConnectionStore.AllConnections.Select(c => (c.Key, c.Value.ScopedIp)).ToList();

			foreach((Guid Key, string ScopedIp) conn in connections) {
				if(!this.outgoingMessageQueue.ContainsKey(conn.ScopedIp)) {
					PeerMessageQueue peerMessageQueue = new PeerMessageQueue();
					peerMessageQueue.Connection = this.networkingService.ConnectionStore.AllConnections[conn.Key];

					this.outgoingMessageQueue.Add(conn.ScopedIp, peerMessageQueue);
				}
			}

			// remove obsolete ones
			var connectionIps = connections.Select(c => c.ScopedIp).ToList();

			foreach(var uuid in this.outgoingMessageQueue.Where(c => !connectionIps.Contains(c.Key)).ToArray()) {
				this.outgoingMessageQueue.Remove(uuid.Key);
			}
		}

		protected virtual IRoutingHeader RehydrateHeader(MessageReceivedTask task) {

			IRoutingHeader header = null;

			try {
				header = this.networkingService.MessageFactory.RehydrateMessageHeader(task.data);

				if(header == null) {
					throw new ApplicationException("Null message header");
				}

				return header;
			} catch(Exception ex) {
				Log.Error(ex, "Fail to rehydrate message set header.");

				throw;
			}
		}

		/// <summary>
		///     lets handle the message and redirect it where it needs to go
		/// </summary>
		/// <param name="task"></param>
		protected virtual void HandleMessageReceived(MessageReceivedTask task) {

			// lets see what we just received
			IRoutingHeader header = this.RehydrateHeader(task);

			// set the client Scope of the client who sent us this message
			header.ClientId = task.Connection.ClientUuid;

			IGossipMessageSet gossipMessageSet = null;

			// first, for gossip messages, we must forward them to other peers, so lets do that
			if(header is GossipHeader gossipHeader) {

				if(!task.Connection.IsConfirmed) {
					throw new ApplicationException("An unconfirmed connection cannot send us a gossip message");
				}

				if(header.ChainId == BlockchainTypes.Instance.None) {
					// we do not allow null chain gossip messages, so lets end here
					throw new ApplicationException("A null chain gossip message is not allowed");
				}

				if(!header.IsWorkflowTrigger) {
					// we do not allow null chain gossip messages, so lets end here
					throw new ApplicationException("A gossip message is not marked as a workflow trigger, which is not allowed");
				}

				bool messageInCache = false;
				bool messageValid = false;
				(messageInCache, messageValid, gossipMessageSet) = this.ProcessReceivedGossipMessage(gossipHeader, task);

				if(messageInCache) {
					return; // we do not process any further
				}

				// now the message will be sent to the chains for validation, and if valid, will come back for a forward
				((NetworkingService<R>) this.networkingService).RouteNetworkGossipMessage(gossipMessageSet, task.Connection);

				return;
			}

			// if we get any further, then they are targeted messages
			if(header.ChainId == BlockchainTypes.Instance.None) {
				// this is a null chain, this is our message

				if(header is TargettedHeader targettedHeader) {
					// this is a targeted header, its meant only for us

					var messageSet = this.networkingService.MessageFactory.RehydrateMessage(task.data, targettedHeader, this.chainlessBlockchainEventsRehydrationFactory);

					var workflowTracker = new WorkflowTracker<IWorkflow<R>, R>(task.Connection, messageSet.Header.WorkflowCorrelationId, messageSet.Header.originatorId, this.networkingService.ConnectionStore.MyClientUuid, this.networkingService.WorkflowCoordinator);

					if(messageSet.Header.IsWorkflowTrigger && messageSet is ITriggerMessageSet<R> triggeMessageSet) {
						// route the message
						Type messageType = triggeMessageSet.BaseMessage.GetType();

						if(task.acceptedTriggers.Any(t => t.IsInstanceOfType(triggeMessageSet.BaseMessage) != t.IsAssignableFrom(messageType))) {
							throw new ApplicationException("Jetbrains refactoring error, the suggested fix is not equal to the previous");
						}

						if(task.acceptedTriggers.Any(t => t.IsInstanceOfType(triggeMessageSet.BaseMessage))) {

							// let's check if a workflow already exists for this trigger
							if(!workflowTracker.WorkflowExists()) {
								// create a new workflow
								var workflow = (ITargettedNetworkingWorkflow<R>) this.serverWorkflowFactory.CreateResponseWorkflow(triggeMessageSet, task.Connection);

								if(!task.Connection.IsConfirmed && !(workflow is IServerHandshakeWorkflow)) {
									throw new ApplicationException("An unconfirmed connection must initiate a handshake");
								}

								this.networkingService.WorkflowCoordinator.AddWorkflow(workflow);
							}
						} else {
							if(triggeMessageSet.BaseMessage is WorkflowTriggerMessage<R>) {
								// this means we did not pass the trigger filter above, it could be an evil trigger and we default
								throw new ApplicationException("An invalid trigger was sent");
							}
						}
					} else {

						if(messageSet.BaseMessage is WorkflowTriggerMessage<R>) {
							throw new ApplicationException("We have a cognitive dissonance here. The trigger flag is not set, but the message type is a workflow trigger");
						}

						if(messageSet.Header.IsWorkflowTrigger) {
							throw new ApplicationException("We have a cognitive dissonance here. The trigger flag is set, but the message type is not a workflow trigger");
						}

						// forward the message to the right correlated workflow
						// this method will ensure we get the right workflow id for our connection

						//----------------------------------------------------

						if(workflowTracker.GetActiveWorkflow() is ITargettedNetworkingWorkflow<R> workflow) {

							if(!task.Connection.IsConfirmed && !(workflow is IHandshakeWorkflow)) {
								throw new ApplicationException("An unconfirmed connection must initiate a handshake");
							}

							workflow.ReceiveNetworkMessage(messageSet);
						} else {
							Log.Verbose($"The message references a workflow correlation ID '{messageSet.Header.WorkflowCorrelationId}' which does not exist");
						}
					}
				}
			} else {
				if(!task.Connection.IsConfirmed) {
					throw new ApplicationException("An unconfirmed connection cannot send us a chain scoped targeted message");
				}

				// this message is targeted at a specific chain, so we route it over there
				// first confirm that we support this chain
				((NetworkingService<R>) this.networkingService).RouteNetworkMessage(header, task.data, task.Connection);
			}
		}

		protected override void ProcessLoop() {
			try {
				this.CheckShouldCancel();

				// first thing, lets check if we have any tasks received to process
				this.CheckTasks();

				this.CheckShouldCancel();

				if(this.ShouldAct(ref this.nextAction)) {
					this.CheckShouldCancel();

					// ok, its time to act
					//TODO: set this to about 10 seconds. in debug, we make it faster
					int secondsToWait = 10; // default next action time in seconds. we can play on this

					// send any messages we have in the outbound queue
					this.SendMessageGroupManifest();

					//---------------------------------------------------------------
					// done, lets sleep for a while

					// lets act again in X seconds
					this.nextAction = DateTime.Now.AddSeconds(secondsToWait);
				}

				if(this.ShouldAct(ref this.nextDatabaseClean)) {
					this.CheckShouldCancel();

					// lets keep our database clean
					this.CleanMesageCache();

					this.CheckShouldCancel();

					// ok, its time to act
					int secondsToWait = 5 * 60; // default next action time in seconds. we can play on this

					//---------------------------------------------------------------
					// done, lets sleep for a while

					// lets act again in X seconds
					this.nextDatabaseClean = DateTime.Now.AddSeconds(secondsToWait);
				}
			} catch(OperationCanceledException) {
				throw;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to process connections");
				this.nextAction = DateTime.Now.AddSeconds(10);
			}
		}

		private void SendMessageGroupManifest() {
			var actions = new List<Action>();
			KeyValuePair<string, PeerMessageQueue>[] entries = default;

			lock(this.locker) {
				entries = this.outgoingMessageQueue.Where(q => (q.Value.manifestSentTime == null) && q.Value.outboundMessages.Any()).ToArray();
			}

			foreach(var messageGroup in entries) {

				actions.Add(() => {
					PeerMessageQueue queue = messageGroup.Value;
					queue.sendAttempts++;

					if(queue.sendAttempts == 4) {
						// thats it, we will drop our messages for this peer
						this.outgoingMessageQueue.Remove(messageGroup.Key);

						Log.Warning($"Client {queue.Connection.ClientUuid} seems to have disconnected and has messages to be sent. we retried a few times and now we give up.");

						return; // this it, we are done with this peer and its messages, we give up and delete it all
					}

					if(!queue.Connection.connection.CheckConnected()) {
						Log.Warning($"Client {queue.Connection.ClientUuid} seems to have disconnected and has messages to be sent. A retry will be attempted");

						return;
					}

					// lets mark it all as we have a manifest in progress
					List<INetworkMessageSet> messages = null;

					lock(this.locker) {
						messages = queue.outboundMessages.ToList();
						queue.outboundMessages.Clear();

						queue.manifestSentMessages.AddRange(messages);
						queue.manifestSentTime = this.timeService.CurrentRealTime;
					}

					if(messages.All(m => m is ITargettedMessageSet)) {
						// ok, we only have workflow triggers, no gossips. so no mneed to wate time by creating a workflow, let's just send the mesasges directly

						var sendActions = new List<Action>();

						foreach(INetworkMessageSet message in messages) {

							sendActions.Add(() => {
								if(!this.dataDispatcher.SendMessage(queue.Connection, message)) {
									Log.Verbose($"Connection with peer  {queue.Connection.ScopedAdjustedIp} was terminated");

									return;
								}

								queue.manifestSentMessages.Remove(message);
							});
						}

						try {
							IndependentActionRunner.Run(sendActions.ToArray());
						} catch {
							queue.outboundMessages.AddRange(queue.manifestSentMessages); // lets return the messages, to try again later
						} finally {
							queue.manifestSentMessages.Clear();
						}
					} else {
						// ok, we need the full workflow here
						var manifestWorkflow = this.clientWorkflowFactory.CreateMessageGroupManifest(messages, queue.Connection);

						manifestWorkflow.Success += w => {
							lock(this.locker) {
								queue.manifestSentTime = null;
								queue.manifestSentMessages.Clear();
								queue.sendAttempts = 0;
							}
						};

						manifestWorkflow.Error += (e, ex) => {
							// lets make sure it will be attempted again
							lock(this.locker) {
								queue.manifestSentTime = null;
								queue.outboundMessages.AddRange(queue.manifestSentMessages); // lets return the messages, to try again later
								queue.manifestSentMessages.Clear();
							}
						};

						this.networkingService.WorkflowCoordinator.AddWorkflow(manifestWorkflow);
					}
				});
			}

			IndependentActionRunner.Run(actions.ToArray());
		}

		/// <summary>
		///     Check if we received any tasks and process them
		/// </summary>
		/// <param name="Process">returns true if satisfied to end the loop, false if it still needs to wait</param>
		/// <returns></returns>
		protected List<Guid> CheckTasks() {
			return this.RoutedTaskReceiver.CheckTasks(() => {
				// check this every loop, for responsiveness
				this.CheckShouldCancel();
			});
		}

		protected override void Initialize() {
			base.Initialize();

			if(!NetworkInterface.GetIsNetworkAvailable() && !GlobalSettings.ApplicationSettings.UndocumentedDebugConfigurations.localhostOnly) {
				throw new NetworkInformationException();
			}
		}

		protected override void DisposeAll(bool disposing) {
			base.DisposeAll(disposing);

		}

		public class MessageReceivedTask : ColoredTask {
			public readonly List<Type> acceptedTriggers;
			public readonly PeerConnection Connection;
			public readonly IByteArray data;

			public MessageReceivedTask(IByteArray data, PeerConnection connection, List<Type> acceptedTriggers) {
				this.data = data;
				this.Connection = connection;
				this.acceptedTriggers = acceptedTriggers;
			}

			public MessageReceivedTask(IByteArray data, PeerConnection connection) : this(data, connection, new List<Type>(new[] {typeof(WorkflowTriggerMessage<R>)})) {

			}

			public MessageReceivedTask(MessageReceivedTask task, PeerConnection connection) : this(task.data, connection, task.acceptedTriggers.ToList()) {

			}
		}

		public class ForwardGossipMessageTask : ColoredTask {
			public readonly PeerConnection Connection;
			public readonly IGossipMessageSet gossipMessageSet;

			public ForwardGossipMessageTask(IGossipMessageSet gossipMessageSet, PeerConnection connection) {
				this.gossipMessageSet = gossipMessageSet;
				this.Connection = connection;
			}
		}

		public class PostNewGossipMessageTask : ColoredTask {

			public readonly IGossipMessageSet gossipMessageSet;

			public PostNewGossipMessageTask(IGossipMessageSet gossipMessageSet) {
				this.gossipMessageSet = gossipMessageSet;
			}
		}

		public class PeerMessageQueue {
			public PeerConnection Connection;

			/// <summary>
			///     messages for which a manifest was sent to the peer. we are awaiting the acceptance to continue
			/// </summary>
			/// <returns></returns>
			public List<INetworkMessageSet> manifestSentMessages = new List<INetworkMessageSet>();

			/// <summary>
			///     when we sent the manifest, in case we need to timeout
			/// </summary>
			public DateTime? manifestSentTime;

			/// <summary>
			///     messages that are waiting to be processed and sent out
			/// </summary>
			/// <returns></returns>
			public List<INetworkMessageSet> outboundMessages = new List<INetworkMessageSet>();

			public int sendAttempts;
		}
	}
}