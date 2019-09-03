using System;
using System.Collections.Generic;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage;
using Blockchains.Neuralium.Classes.NeuraliumChain.Factories;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots.Storage;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Tools.Data;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots.Storage {

	public interface INeuraliumStandardAccountSnapshotSqliteDal : INeuraliumStandardAccountSnapshotDal<NeuraliumStandardAccountSnapshotSqliteContext, NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry, NeuraliumStandardAccountFreezeSqlite>, IStandardAccountSnapshotSqliteDal<NeuraliumStandardAccountSnapshotSqliteContext, NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry> {
	}

	public class NeuraliumStandardAccountSnapshotSqliteDal : StandardAccountSnapshotSqliteDal<NeuraliumStandardAccountSnapshotSqliteContext, NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry>, INeuraliumStandardAccountSnapshotSqliteDal {

		public NeuraliumStandardAccountSnapshotSqliteDal(long groupSize, string folderPath, BlockchainServiceSet serviceSet, INeuraliumChainDalCreationFactory chainDalCreationFactory, AppSettingsBase.SerializationTypes serializationType) : base(groupSize, folderPath, serviceSet, chainDalCreationFactory, serializationType) {

		}

		public override void InsertNewAccount(AccountId accountId, List<(byte ordinal, IByteArray key, TransactionId declarationTransactionId)> keys, long inceptionBlockId) {
			throw new NotImplementedException();
		}
	}
}