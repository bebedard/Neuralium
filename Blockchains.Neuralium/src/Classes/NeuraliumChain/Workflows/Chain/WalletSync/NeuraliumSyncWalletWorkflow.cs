using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Base;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Chain.HandleReceivedGossipMessage;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.WalletSync;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Chain.WalletSync {
	public interface INeuraliumSyncWalletWorkflow : ISyncWalletWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumSyncWalletWorkflow : SyncWalletWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumChainWorkflow, INeuraliumReceiveGossipMessageWorkflow {
		public NeuraliumSyncWalletWorkflow(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}
	}
}