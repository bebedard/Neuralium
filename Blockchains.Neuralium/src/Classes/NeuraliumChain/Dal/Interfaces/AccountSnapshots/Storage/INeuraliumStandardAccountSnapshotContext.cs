using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage.Base;
using Microsoft.EntityFrameworkCore;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage {

	public interface INeuraliumStandardAccountSnapshotContext : IStandardAccountSnapshotContext, INeuraliumAccountSnapshotContext {
	}

	public interface INeuraliumStandardAccountSnapshotContext<ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT> : INeuraliumAccountSnapshotContext<ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, IStandardAccountSnapshotContext<ACCOUNT_SNAPSHOT, ACCOUNT_FEATURE_SNAPSHOT>, INeuraliumStandardAccountSnapshotContext
		where ACCOUNT_SNAPSHOT : class, INeuraliumStandardAccountSnapshotEntry<ACCOUNT_FEATURE_SNAPSHOT, ACCOUNT_FREEZE_SNAPSHOT>, new()
		where ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new() {

		DbSet<ACCOUNT_FREEZE_SNAPSHOT> StandardAccountFreezes { get; set; }
	}
}