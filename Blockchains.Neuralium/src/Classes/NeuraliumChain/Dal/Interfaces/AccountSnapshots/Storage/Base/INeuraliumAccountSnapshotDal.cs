using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage.Bases;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage.Base {
	public interface INeuraliumAccountSnapshotDal : IAccountSnapshotDal {
	}

	public interface INeuraliumAccountSnapshotDal<ACCOUNT_SNAPSHOT_CONTEXT, ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT> : INeuraliumAccountSnapshotDal
		where ACCOUNT_SNAPSHOT_CONTEXT : INeuraliumAccountSnapshotContext<ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>
		where ACCOUNT_SNAPSHOT : class, INeuraliumAccountSnapshotEntry<ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, new()
		where ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new() {
	}
}