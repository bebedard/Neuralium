using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots {

	public interface INeuraliumStandardAccountSnapshotEntry : INeuraliumStandardAccountSnapshot, INeuraliumAccountSnapshotEntry, IStandardAccountSnapshotEntry {
	}

	public interface INeuraliumStandardAccountSnapshotEntry<ACCOUNT_FEATURE, ACCOUNT_FREEZE> : INeuraliumStandardAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, INeuraliumAccountSnapshotEntry<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, IStandardAccountSnapshotEntry<ACCOUNT_FEATURE>, INeuraliumStandardAccountSnapshotEntry
		where ACCOUNT_FEATURE : INeuraliumAccountFeatureEntry
		where ACCOUNT_FREEZE : INeuraliumAccountFreezeEntry {
	}

}