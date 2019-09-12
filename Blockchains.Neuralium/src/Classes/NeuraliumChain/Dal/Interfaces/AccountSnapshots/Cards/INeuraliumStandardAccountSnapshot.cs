using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Cards;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards {
	public interface INeuraliumStandardAccountSnapshot : IStandardAccountSnapshot, INeuraliumAccountSnapshot {
	}

	public interface INeuraliumStandardAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE> : IStandardAccountSnapshot<ACCOUNT_FEATURE>, INeuraliumAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, INeuraliumStandardAccountSnapshot
		where ACCOUNT_FEATURE : INeuraliumAccountFeature
		where ACCOUNT_FREEZE : INeuraliumAccountFreeze {
	}
}