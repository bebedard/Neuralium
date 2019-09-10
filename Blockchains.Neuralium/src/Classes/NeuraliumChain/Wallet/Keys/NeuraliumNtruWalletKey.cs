using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Keys;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Keys {

	public interface INeuraliumNtruWalletKey : INtruWalletKey {
	}

	public class NeuraliumNtruWalletKey : NtruWalletKey, INeuraliumNtruWalletKey {
	}
}