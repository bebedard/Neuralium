using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account {
	public interface INeuraliumWalletAccountChainStateKey : IWalletAccountChainStateKey {
	}

	public class NeuraliumWalletAccountChainStateKey : WalletAccountChainStateKey, INeuraliumWalletAccountChainStateKey {
	}
}