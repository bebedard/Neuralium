using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards.Implementations;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account.Snapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account.Snapshots {
	public interface INeuraliumWalletAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE> : IWalletAccountSnapshot<ACCOUNT_FEATURE>, INeuraliumAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE>
		where ACCOUNT_FEATURE : INeuraliumAccountFeature
		where ACCOUNT_FREEZE : INeuraliumAccountFreeze {
	}

	public interface INeuraliumWalletAccountSnapshot : INeuraliumWalletAccountSnapshot<NeuraliumAccountFeature, NeuraliumAccountFreeze> {
	}
}