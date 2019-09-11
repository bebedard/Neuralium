using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Network;
using Neuralia.Blockchains.Core.P2p.Connections;
using Neuralia.Blockchains.Core.P2p.Messages;
using Neuralia.Blockchains.Core.P2p.Messages.Base;
using Neuralia.Blockchains.Core.P2p.Messages.MessageSets;
using Neuralia.Blockchains.Core.P2p.Messages.RoutingHeaders;
using Neuralia.Blockchains.Core.P2p.Workflows.Handshake.Messages.V1;
using Neuralia.Blockchains.Core.Tools;
using Neuralia.Blockchains.Core.Workflows;
using Neuralia.Blockchains.Core.Workflows.Base;
using Neuralia.Blockchains.Tools;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.General.ExclusiveOptions;
using Serilog;

namespace Neuralia.Blockchains.Core.Services {
	public interface INetworkingService : IDisposable2 {

		bool IsStarted { get; }
		IConnectionStore ConnectionStore { get; }

		IConnectionListener ConnectionListener { get; }

		IMainMessageFactory MessageFactoryBase { get; }

		IConnectionsManager ConnectionsManagerBase { get; }

		int CurrentPeerCount { get; }

		Dictionary<BlockchainType, ChainSettings> ChainSettings { get; }

		List<BlockchainType> SupportsChains { get; }

		void Start();

		void Stop();

		void Initialize();

		void PostNetworkMessage(IByteArray data, PeerConnection connection);

		void ForwardValidGossipMessage(IGossipMessageSet gossipMessageSet, PeerConnection connection);

		void PostNewGossipMessage(IGossipMessageSet gossipMessageSet);

		event Action<int> PeerConnectionsCountUpdated;

		bool SupportsChain(BlockchainType blockchainType);

		bool IsChainVersionValid(BlockchainType blockchainType, SoftwareVersion version);

		event Action Started;
	}

	public interface INetworkingService<R> : INetworkingService
		where R : IRehydrationFactory {

		IWorkflowCoordinator<IWorkflow<R>, R> WorkflowCoordinator { get; }

		IMainMessageFactory<R> MessageFactory { get; }
		ServiceSet<R> ServiceSet { get; }

		Dictionary<BlockchainType, R> ChainRehydrationFactories { get; }

		IConnectionsManager<R> ConnectionsManager { get; }

		void RegisterChain(BlockchainType chainType, ChainSettings chainSettings, INetworkRouter transactionchainNetworkRouting, R rehydrationFactory, IGossipMessageFactory<R> mainChainMessageFactory, Func<SoftwareVersion, bool> versionValidationCallback);
	}

	public class NetworkingService<R> : INetworkingService<R>
		where R : IRehydrationFactory {

		protected readonly IDataAccessService dataAccessService;
		protected readonly IFileFetchService fileFetchService;
		protected readonly IGlobalsService globalsService;
		protected readonly IGuidService guidService;
		protected readonly IHttpService httpService;

		protected readonly IInstantiationService<R> instantiationService;

		protected readonly ByteExclusiveOption<RoutingHeader.Options> optionsInterpreter = new ByteExclusiveOption<RoutingHeader.Options>();

		protected readonly Dictionary<BlockchainType, ChainInfo<R>> supportedChains = new Dictionary<BlockchainType, ChainInfo<R>>();

		protected readonly ITimeService timeService;

		public NetworkingService(IGuidService guidService, IHttpService httpService, IFileFetchService fileFetchService, IDataAccessService dataAccessService, IInstantiationService<R> instantiationService, IGlobalsService globalsService, ITimeService timeService) {
			this.instantiationService = instantiationService;
			this.globalsService = globalsService;
			this.timeService = timeService;
			this.guidService = guidService;
			this.httpService = httpService;
			this.fileFetchService = fileFetchService;
			this.dataAccessService = dataAccessService;

			this.ServiceSet = this.CreateServiceSet();
		}

		public event Action Started;
		public event Action<int> PeerConnectionsCountUpdated;

		public ServiceSet<R> ServiceSet { get; }

		public Dictionary<BlockchainType, R> ChainRehydrationFactories => this.supportedChains.ToDictionary(e => e.Key, e => e.Value.rehydrationFactory);

		public Dictionary<BlockchainType, ChainSettings> ChainSettings {
			get { return this.supportedChains.ToDictionary(t => t.Key, t => t.Value.ChainSettings); }
		}

		/// <summary>
		///     Return the list of confirmed and active peer connections we have
		/// </summary>
		public int CurrentPeerCount => this.IsStarted ? this.ConnectionStore.ActiveConnectionsCount : 0;

		public virtual void Initialize() {
			if(GlobalSettings.Instance.NetworkId == 0) {
				throw new InvalidOperationException("The network Id is not set.");
			}

			this.InitializeComponents();

			this.connectionStore.DataReceived += this.HandleDataReceivedEvent<WorkflowTriggerMessage<R>>;

			this.connectionStore.PeerConnectionsCountUpdated += count => {
				this.PeerConnectionsCountUpdated?.Invoke(count);
			};

			this.connectionListener.NewConnectionReceived += this.ConnectionListenerOnNewConnectionReceived;

			this.connectionListener.NewConnectionRequestReceived += connection => {

				// when the server gets a new connection, register for this event to check their uuid
				NodeAddressInfo nodeAddressInfo = ConnectionStore<R>.GetEndpointInfoNode(connection.EndPoint, Enums.PeerTypes.Unknown);
				this.connectionStore.SetConnectionUuidExistsCheck(connection, nodeAddressInfo);
			};
		}

		public bool IsStarted { get; private set; }

		public void Start() {
			if(GlobalSettings.ApplicationSettings.P2pEnabled) {
				this.connectionListener.Start();

				this.StartWorkers();

				this.IsStarted = true;

				this.Started?.Invoke();
			} else {

				Log.Information("Peer to peer network disabled");
			}
		}

		public void Stop() {
			try {
				this.connectionListener?.Dispose();

				this.IsStarted = false;
			} catch(Exception ex) {
				Log.Error(ex, "failed to stop connetion listener");

				throw;
			} finally {
				this.StopWorkers();
			}
		}

		public void PostNetworkMessage(IByteArray data, PeerConnection connection) {
			MessagingManager<R>.MessageReceivedTask messageTask = new MessagingManager<R>.MessageReceivedTask(data, connection);
			this.PostNetworkMessage(messageTask);
		}

		/// <summary>
		///     here we ensure a gossip message will be forwarded to our peers who may want it
		/// </summary>
		/// <param name="gossipMessageSet"></param>
		public void ForwardValidGossipMessage(IGossipMessageSet gossipMessageSet, PeerConnection connection) {
			// redirect the received message into the message manager worker, who will know what to do with it in its own time
			MessagingManager<R>.ForwardGossipMessageTask forwardTask = new MessagingManager<R>.ForwardGossipMessageTask(gossipMessageSet, connection);
			this.messagingManager.ReceiveTask(forwardTask);
		}

		/// <summary>
		///     here we ensure a gossip message will be forwarded to our peers who may want it
		/// </summary>
		/// <param name="gossipMessageSet"></param>
		public void PostNewGossipMessage(IGossipMessageSet gossipMessageSet) {
			// redirect the received message into the message manager worker, who will know what to do with it in its own time
			MessagingManager<R>.PostNewGossipMessageTask forwardTask = new MessagingManager<R>.PostNewGossipMessageTask(gossipMessageSet);
			this.messagingManager.ReceiveTask(forwardTask);
		}

		/// <summary>
		///     Register a new available transactionchain for the networking and routing purposes
		/// </summary>
		/// <param name="chainTypes"></param>
		/// <param name="transactionchainNetworkRouting"></param>
		public virtual void RegisterChain(BlockchainType chainType, ChainSettings chainSettings, INetworkRouter transactionchainNetworkRouting, R rehydrationFactory, IGossipMessageFactory<R> mainChainMessageFactory, Func<SoftwareVersion, bool> versionValidationCallback) {

			// make sure we support this chain
			var chainInfo = new ChainInfo<R>();

			chainInfo.ChainSettings = chainSettings;

			// now register the rehydration factories
			chainInfo.rehydrationFactory = rehydrationFactory;

			// add the chain for routing
			chainInfo.router = transactionchainNetworkRouting;

			// and the ability to confirm chain versions
			chainInfo.versionValidationCallback = versionValidationCallback;

			this.supportedChains.Add(chainType, chainInfo);
			this.messageFactory.RegisterChainMessageFactory(chainType, mainChainMessageFactory);
		}

		public bool SupportsChain(BlockchainType blockchainType) {
			return this.supportedChains.ContainsKey(blockchainType);
		}

		public List<BlockchainType> SupportsChains => this.supportedChains.Keys.ToList();

		public bool IsChainVersionValid(BlockchainType blockchainType, SoftwareVersion version) {
			if(!this.SupportsChain(blockchainType)) {
				return false;
			}

			return this.supportedChains[blockchainType].versionValidationCallback(version);
		}

		protected virtual ServiceSet<R> CreateServiceSet() {
			return new ServiceSet<R>(BlockchainTypes.Instance.None);
		}

		protected virtual void ConnectionListenerOnNewConnectionReceived(TcpServer listener, ITcpConnection connection, IByteArray buffer) {

			try {
				if((buffer == null) || buffer.IsEmpty) {
					//TODO: handle the evil peer
					throw new ApplicationException("Invalid data");
				}

				this.optionsInterpreter.Value = buffer[0];

				if(this.optionsInterpreter.HasOption(RoutingHeader.Options.IPConfirmation)) {
					// ok, this is a VERY special case. if we are contacted by an IP Validator, we must respond very quickly, and this special workflow allows us to do that
					this.HandleIpValidatorRequest(buffer, connection);
				} else {
					PeerConnection peerConnection = this.connectionStore.AddNewIncomingConnection(connection);
					this.HandleDataReceivedEvent<HandshakeTrigger<R>>(buffer, peerConnection);
				}

			} catch(Exception exception) {
				Log.Error(exception, "Invalid connection attempt");

				throw;
			}
		}

		protected void PostNetworkMessage(MessagingManager<R>.MessageReceivedTask messageTask) {
			this.messagingManager.ReceiveTask(messageTask);
		}

		protected virtual void InitializeComponents() {

			this.connectionStore = new ConnectionStore<R>(this.ServiceSet);
			this.connectionListener = new ConnectionListener(GlobalSettings.ApplicationSettings.port, this.ServiceSet);
			this.workflowCoordinator = new WorkflowCoordinator<IWorkflow<R>, R>(this.ServiceSet);
			this.messageFactory = new MainMessageFactory<R>(this.ServiceSet);
		}

		protected virtual void StartWorkers() {
			//TODO: perhaps we should attempt a restart if it fails

			this.ConnectionsManager = this.instantiationService.GetInstantiationFactory(this.ServiceSet).CreateConnectionsManager(this.ServiceSet);
			this.InitializeConnectionsManager();
			this.ConnectionsManager.Start();

			this.ConnectionsManager.Error2 += (sender, exception) => {
				if(sender.Task.Status == TaskStatus.Faulted) {
					if(exception is AggregateException ae) {
						Log.Error(ae.Flatten(), "Failed to run connections coordinator");

						throw ae.Flatten();
					}

					throw exception;
				}
			};

			this.messagingManager = this.instantiationService.GetInstantiationFactory(this.ServiceSet).CreateMessagingManager(this.ServiceSet);
			this.InitializeMessagingManager();
			this.messagingManager.Start();

			this.messagingManager.Error2 += (sender, exception) => {
				if(sender.Task.Status == TaskStatus.Faulted) {
					if(exception is AggregateException ae) {
						Log.Error(ae.Flatten(), "Failed to run messaging coordinator");

						throw ae.Flatten();
					}

					throw exception;
				}
			};
		}

		protected virtual void StopWorkers() {
			// lets cancel the coordinator
			try {
				this.ConnectionsManager?.Stop();
			} catch {
			}

			try {
				this.messagingManager?.Stop();
			} catch {
			}
		}

		protected virtual void InitializeConnectionsManager() {

		}

		protected virtual void InitializeMessagingManager() {

		}

		/// <summary>
		///     Rehydrate the message and route it
		///     TODO: should this really be in this class, or some kind of router?
		/// </summary>
		/// <param name="data"></param>
		/// <param name="connection"></param>
		public void HandleDataReceivedEvent<TRIGGER>(IByteArray data, PeerConnection connection, IEnumerable<Type> acceptedTriggers = null) {

			// redirect the received message into the message manager worker, who will know what to do with it in its own time
			var acceptedTriggerTypes = acceptedTriggers != null ? acceptedTriggers.ToList() : new List<Type>();

			acceptedTriggerTypes.Add(typeof(TRIGGER));

			MessagingManager<R>.MessageReceivedTask messageTask = new MessagingManager<R>.MessageReceivedTask(data, connection, acceptedTriggerTypes);
			this.PostNetworkMessage(messageTask);

		}

		/// <summary>
		///     This is a very special use case where an IP Validator is contacting us. We need to respond as quickly a possible,
		///     so its all done here in top priority
		/// </summary>
		/// <param name="buffer"></param>
		protected virtual void HandleIpValidatorRequest(IByteArray buffer, ITcpConnection connection) {

			//TODO: what should happen by default here?
			// we dont know what to do with this

		}

		/// <summary>
		///     here we ensure to route a message to the proper registered chain
		/// </summary>
		/// <param name="header"></param>
		/// <param name="data"></param>
		/// <param name="connection"></param>
		public void RouteNetworkGossipMessage(IGossipMessageSet gossipMessageSet, PeerConnection connection) {
			if(!this.SupportsChain(gossipMessageSet.BaseHeader.ChainId)) {
				throw new ApplicationException("A message was received that targets a transactionchain that we do not support.");
			}

			// ok, now we route this message to the chain
			this.supportedChains[gossipMessageSet.BaseHeader.ChainId].router.RouteNetworkGossipMessage(gossipMessageSet, connection);
		}

		/// <summary>
		///     here we ensure to route a message to the proper registered chain
		/// </summary>
		/// <param name="header"></param>
		/// <param name="data"></param>
		/// <param name="connection"></param>
		public void RouteNetworkMessage(IRoutingHeader header, IByteArray data, PeerConnection connection) {
			if(!this.SupportsChain(header.ChainId)) {
				throw new ApplicationException("A message was received that targets a transactionchain that we do not support.");
			}

			// ok, now we route this message to the chain
			this.supportedChains[header.ChainId].router.RouteNetworkMessage(header, data, connection);
		}

		public class ChainInfo<R>
			where R : IRehydrationFactory {
			public ChainSettings ChainSettings;
			public R rehydrationFactory;
			public INetworkRouter router;
			public Func<SoftwareVersion, bool> versionValidationCallback;
		}

	#region components

		protected IConnectionStore connectionStore;

		protected IConnectionListener connectionListener;

		protected IWorkflowCoordinator<IWorkflow<R>, R> workflowCoordinator;

		protected IMainMessageFactory<R> messageFactory;

		public IConnectionStore ConnectionStore => this.connectionStore;

		public IWorkflowCoordinator<IWorkflow<R>, R> WorkflowCoordinator => this.workflowCoordinator;

		public IConnectionListener ConnectionListener => this.connectionListener;

		public IMainMessageFactory MessageFactoryBase => this.MessageFactory;
		public IMainMessageFactory<R> MessageFactory => this.messageFactory;

		protected Task connectionsManagerTask;

		/// <summary>
		///     the service that will manage connections to our peers
		/// </summary>
		public IConnectionsManager<R> ConnectionsManager { get; protected set; }

		public IConnectionsManager ConnectionsManagerBase => this.ConnectionsManager;

		/// <summary>
		///     the service that will manage all netowrk messaging
		/// </summary>
		protected IMessagingManager<R> messagingManager;

	#endregion

	#region dispose

		protected virtual void Dispose(bool disposing) {
			if(disposing && !this.IsDisposed) {
				try {
					try {
						this.Stop();
					} catch(Exception ex) {
						Log.Error(ex, "Failed to stop");
					}

					try {
						this.ConnectionsManager?.Dispose();
					} catch(Exception ex) {
						Log.Error(ex, "Failed to dispose of connections coordinator");
					}

					try {
						this.messagingManager?.Dispose();
					} catch(Exception ex) {
						Log.Error(ex, "Failed to dispose of connections coordinator");
					}

					try {
						this.workflowCoordinator?.Dispose();
					} catch(Exception ex) {
						Log.Error(ex, "Failed to dispose of workflow coordinator");
					}

					try {
						this.connectionListener?.Dispose();
					} catch(Exception ex) {
						Log.Error(ex, "Failed to dispose of connection listener");
					}

					try {
						this.connectionStore?.Dispose();
					} catch(Exception ex) {
						Log.Error(ex, "Failed to dispose of connection manager");
					}
				} finally {
					this.IsDisposed = true;
				}
			}
		}

		~NetworkingService() {
			this.Dispose(false);
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}