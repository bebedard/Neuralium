using System.Collections.Generic;
using Neuralia.Blockchains.Core.Network;
using Neuralia.Blockchains.Core.P2p.Connections;
using Neuralia.Blockchains.Core.P2p.Messages.Base;
using Neuralia.Blockchains.Core.P2p.Messages.MessageSets;
using Neuralia.Blockchains.Core.P2p.Workflows.Handshake;
using Neuralia.Blockchains.Core.P2p.Workflows.MessageGroupManifest;
using Neuralia.Blockchains.Core.P2p.Workflows.PeerListRequest;
using Neuralia.Blockchains.Core.Tools;

namespace Neuralia.Blockchains.Core.P2p.Workflows {
	public interface IClientWorkflowFactory<R>
		where R : IRehydrationFactory {

		ClientHandshakeWorkflow<R> CreateRequestHandshakeWorkflow(NetworkEndPoint endpoint);
		ClientPeerListRequestWorkflow<R> CreatePeerListRequest(PeerConnection peerConnection);
		ClientMessageGroupManifestWorkflow<R> CreateMessageGroupManifest(List<INetworkMessageSet> messages, PeerConnection peerConnection);
	}

	public class ClientWorkflowFactory<R> : WorkflowFactory<R>, IClientWorkflowFactory<R>
		where R : IRehydrationFactory {

		public ClientWorkflowFactory(ServiceSet<R> serviceSet) : base(serviceSet) {
		}

		public virtual ClientHandshakeWorkflow<R> CreateRequestHandshakeWorkflow(NetworkEndPoint endpoint) {
			return new ClientHandshakeWorkflow<R>(endpoint, this.serviceSet);
		}

		public virtual ClientPeerListRequestWorkflow<R> CreatePeerListRequest(PeerConnection peerConnection) {
			return new ClientPeerListRequestWorkflow<R>(peerConnection, this.serviceSet);
		}

		public virtual ClientMessageGroupManifestWorkflow<R> CreateMessageGroupManifest(List<INetworkMessageSet> messages, PeerConnection peerConnection) {
			return new ClientMessageGroupManifestWorkflow<R>(messages, peerConnection, this.serviceSet);
		}
	}
}