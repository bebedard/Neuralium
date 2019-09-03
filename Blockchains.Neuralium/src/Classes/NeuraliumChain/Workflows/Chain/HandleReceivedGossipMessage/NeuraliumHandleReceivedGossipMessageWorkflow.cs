using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Base;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.HandleReceivedGossipMessage;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Messages;
using Neuralia.Blockchains.Core.P2p.Connections;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Chain.HandleReceivedGossipMessage {
	public interface INeuraliumReceiveGossipMessageWorkflow {
	}

	public class NeuraliumHandleReceivedGossipMessageWorkflow : HandleReceivedGossipMessageWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumChainWorkflow, INeuraliumReceiveGossipMessageWorkflow {
		public NeuraliumHandleReceivedGossipMessageWorkflow(INeuraliumCentralCoordinator centralCoordinator, IBlockchainGossipMessageSet gossipMessageSet, PeerConnection connection) : base(centralCoordinator, gossipMessageSet, connection) {
		}
	}
}