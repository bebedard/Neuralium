using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots {

	public interface INeuraliumAccountSnapshotSqliteEntry : IAccountSnapshotSqliteEntry, INeuraliumAccountSnapshotEntry {
	}

	public interface INeuraliumAccountSnapshotSqliteEntry<ACCOUNT_FEATURE, ACCOUNT_FREEZE> : IAccountSnapshotSqliteEntry<ACCOUNT_FEATURE>, INeuraliumAccountSnapshotEntry<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, INeuraliumAccountSnapshotSqliteEntry
		where ACCOUNT_FEATURE : INeuraliumAccountFeatureSqliteEntry
		where ACCOUNT_FREEZE : INeuraliumAccountFreezeSqlite {
	}
}