using Blockchains.Neuralium.Classes.NeuraliumChain.Managers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Tasks;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Tasks {
	public interface INeuraliumWorkflowTaskFactory : IWorkflowTaskFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {

		INeuraliumValidationTask<K> CreateNeuraliumValidationTask<K>();
		INeuraliumSerializationTask<K> CreateNeuraliumSerializationTask<K>();
		INeuraliumBlockchainTask<K> CreateNeuraliumBlockchainTask<K>();

		INeuraliumValidationTask<INeuraliumValidationManager, K> CreateNeuraliumValidationTaskSpecialized<K>();
		INeuraliumSerializationTask<INeuraliumSerializationManager, K> CreateNeuraliumSerializationTaskSpecialized<K>();
		INeuraliumBlockchainTask<INeuraliumBlockchainManager, K> CreateNeuraliumBlockchainTaskSpecialized<K>();
	}

	public class NeuraliumWorkflowTaskFactory : WorkflowTaskFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumWorkflowTaskFactory {

		public NeuraliumWorkflowTaskFactory(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}

		public virtual INeuraliumValidationTask<K> CreateNeuraliumValidationTask<K>() {
			return this.CreateNeuraliumValidationTaskSpecialized<K>();
		}

		public INeuraliumValidationTask<INeuraliumValidationManager, K> CreateNeuraliumValidationTaskSpecialized<K>() {
			return new NeuraliumValidationTask<K>();
		}

		public virtual INeuraliumSerializationTask<K> CreateNeuraliumSerializationTask<K>() {
			return this.CreateNeuraliumSerializationTaskSpecialized<K>();
		}

		public INeuraliumSerializationTask<INeuraliumSerializationManager, K> CreateNeuraliumSerializationTaskSpecialized<K>() {
			return new NeuraliumSerializationTask<K>();
		}

		public virtual INeuraliumBlockchainTask<K> CreateNeuraliumBlockchainTask<K>() {
			return this.CreateNeuraliumBlockchainTaskSpecialized<K>();
		}

		public INeuraliumBlockchainTask<INeuraliumBlockchainManager, K> CreateNeuraliumBlockchainTaskSpecialized<K>() {
			return new NeuraliumBlockchainTask<K>();
		}
	}
}