using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account {

	public interface INeuraliumWalletAccountChainState : IWalletAccountChainState {
	}

	public class NeuraliumWalletAccountChainState : WalletAccountChainState, INeuraliumWalletAccountChainState {
	}
}