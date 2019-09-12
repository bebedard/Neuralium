using Neuralia.Blockchains.Common.Classes.Configuration;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.Configuration;

namespace Blockchains.Neuralium.Classes.Configuration {

	public interface INeuraliumBlockchainAppSettings : IBlockchainAppSettings {
	}

	public class NeuraliumBlockchainAppSettings : BlockchainAppSettings, INeuraliumBlockchainAppSettings {
		public NeuraliumBlockChainConfigurations NeuraliumChainConfiguration { get; set; } = new NeuraliumBlockChainConfigurations();

		public override ChainConfigurations GetChainConfiguration(BlockchainType chaintype) {
			if(chaintype.Value == NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium.Value) {
				return this.NeuraliumChainConfiguration;
			}

			return null;
		}
	}
}