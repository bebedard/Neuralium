using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage.Bases;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage.Base {

	public interface INeuraliumAccountSnapshotContext : IAccountSnapshotContext {
	}

	public interface INeuraliumAccountSnapshotContext<ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT> : INeuraliumAccountSnapshotContext
		where ACCOUNT_SNAPSHOT : class, INeuraliumAccountSnapshotEntry<ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, new()
		where ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new() {
	}

}