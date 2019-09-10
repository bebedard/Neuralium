using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots {

	public interface INeuraliumStandardAccountKeysSnapshotSqliteEntry : INeuraliumStandardAccountKeysSnapshotEntry, IStandardAccountKeysSnapshotSqliteEntry {
	}

	public class NeuraliumStandardAccountKeysSnapshotSqliteEntry : AccountKeysSnapshotSqliteEntry, INeuraliumStandardAccountKeysSnapshotSqliteEntry {
	}
}