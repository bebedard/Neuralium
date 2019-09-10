using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Factories;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Factories {

	public interface INeuraliumChainComponentsInjection : IChainComponentsInjection<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumChainComponentsInjection : ChainComponentsInjection<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumChainComponentsInjection {
	}
}