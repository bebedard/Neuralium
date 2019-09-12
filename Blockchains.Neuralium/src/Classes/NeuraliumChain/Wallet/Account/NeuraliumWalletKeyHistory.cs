using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account {
	public interface INeuraliumWalletKeyHistory : IWalletKeyHistory {
	}

	public class NeuraliumWalletKeyHistory : WalletKeyHistory, INeuraliumWalletKeyHistory {
	}
}