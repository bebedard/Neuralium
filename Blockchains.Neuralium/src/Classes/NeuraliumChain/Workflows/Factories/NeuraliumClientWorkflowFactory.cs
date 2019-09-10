using System.IO.Abstractions;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Chain.ChainSync;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Chain.WalletSync;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.ChainSync;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.WalletSync;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Factories;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Factories {
	public interface INeuraliumClientWorkflowFactory : IClientChainWorkflowFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumClientWorkflowFactory : ClientChainWorkflowFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumClientWorkflowFactory {
		public NeuraliumClientWorkflowFactory(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}

		public override IClientChainSyncWorkflow CreateChainSynchWorkflow(IFileSystem fileSystem) {
			return new NeuraliumClientChainSyncWorkflow(this.centralCoordinator, fileSystem);
		}

		public override ISyncWalletWorkflow CreateSyncWalletWorkflow() {
			return new NeuraliumSyncWalletWorkflow(this.centralCoordinator);
		}

		public NeuraliumClientChainSyncWorkflow CreateNeuraliumChainSynchWorkflow(IFileSystem fileSystem) {
			return (NeuraliumClientChainSyncWorkflow) this.CreateChainSynchWorkflow(fileSystem);
		}

		public NeuraliumSyncWalletWorkflow CreateNeuraliumSyncWalletWorkflow() {
			return (NeuraliumSyncWalletWorkflow) this.CreateSyncWalletWorkflow();
		}
	}
}