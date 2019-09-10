using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Managers;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Managers {
	public interface INeuraliumSerializationManager : ISerializationManager<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumSerializationManager : SerializationManager<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumSerializationManager {
		public NeuraliumSerializationManager(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}
	}
}