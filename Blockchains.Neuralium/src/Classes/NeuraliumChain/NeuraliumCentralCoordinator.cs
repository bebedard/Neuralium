using System.IO.Abstractions;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core;

namespace Blockchains.Neuralium.Classes.NeuraliumChain {

	public interface INeuraliumCentralCoordinator : ICentralCoordinator<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumCentralCoordinator : CentralCoordinator<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumCentralCoordinator {

		public NeuraliumCentralCoordinator(BlockchainServiceSet serviceSet, ChainRuntimeConfiguration chainRuntimeConfiguration, IFileSystem fileSystem) : base(NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium, serviceSet, chainRuntimeConfiguration, fileSystem) {

			// make sure we can name our blockchain
			BlockchainTypes.Instance.AddBlockchainTypeNameProvider(new NeuraliumBlockchainTypeNameProvider());
		}
	}

}