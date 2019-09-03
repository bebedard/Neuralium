using Blockchains.Neuralium.Classes.Configuration;

namespace Neuralium.Shell.Classes.Configuration {

	public interface IAppSettings : INeuraliumBlockchainAppSettings {
	}

	public class AppSettings : NeuraliumBlockchainAppSettings, IAppSettings {
	}

}