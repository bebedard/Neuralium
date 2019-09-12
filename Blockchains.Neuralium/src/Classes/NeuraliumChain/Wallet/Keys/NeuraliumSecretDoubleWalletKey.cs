using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Keys;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Keys {

	public interface INeuraliumSecretDoubleWalletKey : ISecretDoubleWalletKey, INeuraliumSecretComboWalletKey {
	}

	public class NeuraliumSecretDoubleWalletKey : SecretDoubleWalletKey, INeuraliumSecretDoubleWalletKey {
	}
}