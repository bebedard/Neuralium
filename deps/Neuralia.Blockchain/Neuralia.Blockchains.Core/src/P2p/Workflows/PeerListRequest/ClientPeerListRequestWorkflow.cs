using System.Linq;
using Neuralia.Blockchains.Core.P2p.Connections;
using Neuralia.Blockchains.Core.P2p.Messages.MessageSets;
using Neuralia.Blockchains.Core.P2p.Workflows.Base;
using Neuralia.Blockchains.Core.P2p.Workflows.PeerListRequest.Messages;
using Neuralia.Blockchains.Core.P2p.Workflows.PeerListRequest.Messages.V1;
using Neuralia.Blockchains.Core.Tools;
using Neuralia.Blockchains.Core.Workflows.Base;
using Serilog;

namespace Neuralia.Blockchains.Core.P2p.Workflows.PeerListRequest {
	public class ClientPeerListRequestWorkflow<R> : ClientWorkflow<PeerListRequestMessageFactory<R>, R>
		where R : IRehydrationFactory {
		public readonly PeerConnection peerConnection;

		public ClientPeerListRequestWorkflow(PeerConnection peerConnection, ServiceSet<R> serviceSet) : base(serviceSet) {
			this.peerConnection = peerConnection;

			// allow only one per peer at a time
			this.ExecutionMode = Workflow.ExecutingMode.Single;

			this.PeerUnique = true;
		}

		protected override void PerformWork() {
			this.CheckShouldCancel();

			var peerListRequestTrigger = this.MessageFactory.CreatePeerListRequestWorkflowTriggerSet(this.CorrelationId);

			Log.Verbose($"Sending peer list request to peer {this.peerConnection.ScopedAdjustedIp}");

			if(!this.SendMessage(this.peerConnection, peerListRequestTrigger)) {
				Log.Verbose($"Connection with peer  {this.peerConnection.ScopedAdjustedIp} was terminated");

				return;
			}

			var serverPeerListRequest = this.WaitSingleNetworkMessage<PeerListRequestServerReply<R>, TargettedMessageSet<PeerListRequestServerReply<R>, R>, R>();

			// take the peer nodes and update our system
			this.networkingService.ConnectionStore.UpdatePeerNodes(this.peerConnection, serverPeerListRequest.Message.nodes);

			Log.Verbose($"Received {serverPeerListRequest.Message.nodes.Select(n => n.Value.Nodes.Count).Sum()} peers from peer {this.peerConnection.ScopedAdjustedIp}");
		}

		protected override PeerListRequestMessageFactory<R> CreateMessageFactory() {
			return new PeerListRequestMessageFactory<R>(this.serviceSet);
		}

		protected override bool CompareOtherPeerId(IWorkflow other) {
			if(other is ClientPeerListRequestWorkflow<R> otherWorkflow) {
				return this.peerConnection.ClientUuid == otherWorkflow.peerConnection.ClientUuid;
			}

			return base.CompareOtherPeerId(other);
		}
	}
}