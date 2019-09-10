using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots {

	public interface INeuraliumAccountSnapshotEntry : IAccountSnapshotEntry, INeuraliumAccountSnapshot {
	}

	public interface INeuraliumAccountSnapshotEntry<ACCOUNT_FEATURE, ACCOUNT_FREEZE> : IAccountSnapshotEntry<ACCOUNT_FEATURE>, INeuraliumAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, INeuraliumAccountSnapshotEntry
		where ACCOUNT_FEATURE : INeuraliumAccountFeatureEntry
		where ACCOUNT_FREEZE : INeuraliumAccountFreezeEntry {
	}

}