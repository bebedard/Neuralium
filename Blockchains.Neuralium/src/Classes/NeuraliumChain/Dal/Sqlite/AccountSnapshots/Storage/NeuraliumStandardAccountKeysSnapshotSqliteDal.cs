using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage;
using Blockchains.Neuralium.Classes.NeuraliumChain.Factories;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots.Storage;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots.Storage.Base;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core.Configuration;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots.Storage {

	public interface INeuraliumStandardAccountKeysSnapshotSqliteDal : INeuraliumStandardAccountKeysSnapshotDal<NeuraliumStandardAccountKeysSnapshotSqliteContext, NeuraliumStandardAccountKeysSnapshotSqliteEntry>, IAccountKeysSnapshotSqliteDal<NeuraliumStandardAccountKeysSnapshotSqliteContext, NeuraliumStandardAccountKeysSnapshotSqliteEntry> {
	}

	public class NeuraliumStandardAccountKeysSnapshotSqliteDal : StandardAccountKeysSnapshotSqliteDal<NeuraliumStandardAccountKeysSnapshotSqliteContext, NeuraliumStandardAccountKeysSnapshotSqliteEntry>, INeuraliumStandardAccountKeysSnapshotSqliteDal {

		public NeuraliumStandardAccountKeysSnapshotSqliteDal(long groupSize, string folderPath, BlockchainServiceSet serviceSet, INeuraliumChainDalCreationFactory chainDalCreationFactory, AppSettingsBase.SerializationTypes serializationType) : base(groupSize, folderPath, serviceSet, chainDalCreationFactory, serializationType) {

		}
	}
}