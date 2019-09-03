using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage.Base;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage {

	public interface INeuraliumJointAccountSnapshotDal : IJointAccountSnapshotDal, INeuraliumAccountSnapshotDal {
	}

	public interface INeuraliumJointAccountSnapshotDal<ACCOUNT_SNAPSHOT_CONTEXT, ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_MEMBERS_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT> : INeuraliumAccountSnapshotDal<ACCOUNT_SNAPSHOT_CONTEXT, ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, IJointAccountSnapshotDal<ACCOUNT_SNAPSHOT_CONTEXT, ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_MEMBERS_SNAPSHOT>, INeuraliumJointAccountSnapshotDal
		where ACCOUNT_SNAPSHOT_CONTEXT : INeuraliumJointAccountSnapshotContext<ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_MEMBERS_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>
		where ACCOUNT_SNAPSHOT : class, INeuraliumJointAccountSnapshotEntry<ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_MEMBERS_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, new()
		where ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where ACCOUNT_MEMBERS_SNAPSHOT : class, INeuraliumJointMemberAccountEntry, new()
		where ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new() {
	}
}