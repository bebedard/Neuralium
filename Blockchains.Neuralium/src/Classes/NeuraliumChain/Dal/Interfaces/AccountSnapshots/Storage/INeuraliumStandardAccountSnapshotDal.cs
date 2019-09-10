using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage.Base;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage {

	public interface INeuraliumStandardAccountSnapshotDal : IStandardAccountSnapshotDal, INeuraliumAccountSnapshotDal {
	}

	public interface INeuraliumStandardAccountSnapshotDal<ACCOUNT_SNAPSHOT_CONTEXT, ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT> : INeuraliumAccountSnapshotDal<ACCOUNT_SNAPSHOT_CONTEXT, ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, IStandardAccountSnapshotDal<ACCOUNT_SNAPSHOT_CONTEXT, ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT>, INeuraliumStandardAccountSnapshotDal
		where ACCOUNT_SNAPSHOT_CONTEXT : INeuraliumStandardAccountSnapshotContext<ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>
		where ACCOUNT_SNAPSHOT : class, INeuraliumStandardAccountSnapshotEntry<ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, new()
		where ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new() {
	}
}