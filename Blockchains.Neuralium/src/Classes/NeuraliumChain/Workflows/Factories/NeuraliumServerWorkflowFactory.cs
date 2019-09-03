using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Chain.ChainSync;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Chain.ChainSync.Messages.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Factories;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Messages;
using Neuralia.Blockchains.Core.P2p.Connections;
using Neuralia.Blockchains.Core.Workflows;
using Neuralia.Blockchains.Core.Workflows.Base;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Factories {
	public interface INeuraliumServerWorkflowFactory : IServerChainWorkflowFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumServerWorkflowFactory : ServerChainWorkflowFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumServerWorkflowFactory {
		public NeuraliumServerWorkflowFactory(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}

		public override INetworkingWorkflow<IBlockchainEventsRehydrationFactory> CreateResponseWorkflow(IBlockchainTriggerMessageSet messageSet, PeerConnection peerConnectionn) {
			this.ValidateTrigger<NeuraliumChainSyncTrigger>(messageSet);

			if((messageSet.BaseMessage.WorkflowType == WorkflowIDs.CHAIN_SYNC) && (messageSet.BaseMessage != null) && messageSet.BaseMessage is NeuraliumChainSyncTrigger) {
				return new NeuraliumServerChainSyncWorkflow(messageSet as BlockchainTriggerMessageSet<NeuraliumChainSyncTrigger>, peerConnectionn, this.centralCoordinator);
			}

			return null;
		}
	}
}