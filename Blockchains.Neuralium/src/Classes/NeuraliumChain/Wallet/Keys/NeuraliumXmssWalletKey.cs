using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Keys;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Keys {
	public interface INeuraliumXmssWalletKey : IXmssWalletKey {
	}

	public class NeuraliumXmssWalletKey : XmssWalletKey, INeuraliumXmssWalletKey {
	}
}