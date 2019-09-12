using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Envelopes;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.Base;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.BlockchainMessageReceivedGossip;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.BlockReceivedGossip;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.TransactionReceivedGossip;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Messages;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Factories;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Gossip;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Messages;
using Neuralia.Blockchains.Core.P2p.Connections;
using Neuralia.Blockchains.Core.Workflows.Base;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Factories {
	public interface INeuraliumGossipWorkflowFactory : IGossipChainWorkflowFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumGossipWorkflowFactory : GossipChainWorkflowFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumGossipWorkflowFactory {
		public NeuraliumGossipWorkflowFactory(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}

		public override INetworkingWorkflow<IBlockchainEventsRehydrationFactory> CreateGossipResponseWorkflow(IBlockchainGossipMessageSet messageSet, PeerConnection peerConnectionn) {
			this.ValidateTrigger(messageSet);

			// template:
			// if((messageSet.BaseMessage.WorkflowType == GossipWorkflowIDs.TRANSACTION_RECEIVED) && (messageSet.BaseMessage != null) && messageSet.BaseMessage is INeuraliumGossipWorkflowTriggerMessage) {
			// 	return new NeuraliumNewTransactionReceivedGossipWorkflow(this.centralCoordinator, (INeuraliumGossipMessageSet<INeuraliumGossipWorkflowTriggerMessage<INeuraliumTransactionEnvelope>, INeuraliumTransactionEnvelope>) messageSet, peerConnectionn);
			// }

			return null;
		}
	}
}