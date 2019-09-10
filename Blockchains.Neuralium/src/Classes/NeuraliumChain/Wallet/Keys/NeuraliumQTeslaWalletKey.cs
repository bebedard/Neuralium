using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Keys;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Keys {

	public interface INeuraliumQTeslaWalletKey : IQTeslaWalletKey {
	}

	public class NeuraliumQTeslaWalletKey : QTeslaWalletKey, INeuraliumQTeslaWalletKey {
	}
}