using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Keys;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Keys {

	public interface INeuraliumSecretWalletKey : ISecretWalletKey, INeuraliumQTeslaWalletKey {
	}

	public class NeuraliumSecretWalletKey : SecretWalletKey, INeuraliumSecretWalletKey {
	}
}