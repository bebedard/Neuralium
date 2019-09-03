using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Keys;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Keys {

	public interface INeuraliumSecretComboWalletKey : ISecretComboWalletKey, INeuraliumSecretWalletKey {
	}

	public class NeuraliumSecretComboWalletKey : SecretComboWalletKey, INeuraliumSecretComboWalletKey {
	}
}