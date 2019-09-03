using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Tools;
using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots {
	public interface INeuraliumJointAccountSnapshotSqliteEntry : INeuraliumJointAccountSnapshotEntry<NeuraliumJointAccountFeatureSqliteEntry, NeuraliumJointMemberAccountSqliteEntry, NeuraliumJointAccountFreezeSqlite>, IJointAccountSnapshotSqliteEntry<NeuraliumJointAccountFeatureSqliteEntry, NeuraliumJointMemberAccountSqliteEntry>, INeuraliumAccountSnapshotSqliteEntry<NeuraliumJointAccountFeatureSqliteEntry, NeuraliumJointAccountFreezeSqlite> {
	}

	/// <summary>
	///     Here we store various metadata state about our chain
	/// </summary>
	public class NeuraliumJointAccountSnapshotSqliteEntry : JointAccountSnapshotSqliteEntry<NeuraliumJointAccountFeatureSqliteEntry, NeuraliumJointMemberAccountSqliteEntry>, INeuraliumJointAccountSnapshotSqliteEntry {

		public Amount Balance { get; set; } = new Amount();
		public List<NeuraliumJointAccountFreezeSqlite> AccountFreezes { get; } = new List<NeuraliumJointAccountFreezeSqlite>();

		public void CreateNewCollectionEntry(out INeuraliumAccountFreeze result) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.CreateNewCollectionEntry(this.AccountFreezes, out result);
		}

		public void AddCollectionEntry(INeuraliumAccountFreeze entry) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.AddCollectionEntry(entry, this.AccountFreezes);
		}

		public void RemoveCollectionEntry(Func<INeuraliumAccountFreeze, bool> predicate) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.RemoveCollectionEntry(predicate, this.AccountFreezes);
		}

		public INeuraliumAccountFreeze GetCollectionEntry(Func<INeuraliumAccountFreeze, bool> predicate) {
			return TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollectionEntry(predicate, this.AccountFreezes);
		}

		public List<INeuraliumAccountFreeze> GetCollectionEntries(Func<INeuraliumAccountFreeze, bool> predicate) {
			return TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollectionEntries(predicate, this.AccountFreezes);
		}

		ImmutableList<INeuraliumAccountFreeze> ITypedCollectionExposure<INeuraliumAccountFreeze>.CollectionCopy => TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollection(this.AccountFreezes);
	}
}