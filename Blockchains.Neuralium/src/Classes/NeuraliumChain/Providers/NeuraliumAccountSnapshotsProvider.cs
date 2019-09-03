using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage.Base;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots.Storage;
using Microsoft.EntityFrameworkCore;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers;
using Neuralia.Blockchains.Core.General.Types;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Providers {

	public interface INeuraliumAccountSnapshotsProviderCommon : IAccountSnapshotsProvider {
	}

	public interface INeuraliumAccountSnapshotsProvider : INeuraliumAccountSnapshotsProviderGenerix<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider, NeuraliumStandardAccountSnapshotSqliteDal, NeuraliumStandardAccountSnapshotSqliteContext, NeuraliumJointAccountSnapshotSqliteDal, NeuraliumJointAccountSnapshotSqliteContext, NeuraliumAccreditationCertificatesSnapshotSqliteDal, NeuraliumAccreditationCertificatesSnapshotSqliteContext, NeuraliumStandardAccountKeysSnapshotSqliteDal, NeuraliumStandardAccountKeysSnapshotSqliteContext, NeuraliumChainOptionsSnapshotSqliteDal, NeuraliumChainOptionsSnapshotSqliteContext, NeuraliumTrackedAccountsSqliteDal, NeuraliumTrackedAccountsSqliteContext, INeuraliumAccountSnapshot, NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry, NeuraliumStandardAccountFreezeSqlite, NeuraliumJointAccountSnapshotSqliteEntry, NeuraliumJointAccountFeatureSqliteEntry, NeuraliumJointMemberAccountSqliteEntry, NeuraliumJointAccountFreezeSqlite, NeuraliumStandardAccountKeysSnapshotSqliteEntry, NeuraliumAccreditationCertificateSnapshotSqliteEntry, NeuraliumAccreditationCertificateSnapshotAccountSqliteEntry, NeuraliumChainOptionsSnapshotSqliteEntry>, INeuraliumAccountSnapshotsProviderCommon {
	}

	/// <summary>
	///     Provide access to the chain state that is saved in the DB
	/// </summary>
	public class NeuraliumAccountSnapshotsProvider : NeuraliumAccountSnapshotsProviderGenerix<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider, NeuraliumStandardAccountSnapshotSqliteDal, NeuraliumStandardAccountSnapshotSqliteContext, NeuraliumJointAccountSnapshotSqliteDal, NeuraliumJointAccountSnapshotSqliteContext, NeuraliumAccreditationCertificatesSnapshotSqliteDal, NeuraliumAccreditationCertificatesSnapshotSqliteContext, NeuraliumStandardAccountKeysSnapshotSqliteDal, NeuraliumStandardAccountKeysSnapshotSqliteContext, NeuraliumChainOptionsSnapshotSqliteDal, NeuraliumChainOptionsSnapshotSqliteContext, NeuraliumTrackedAccountsSqliteDal, NeuraliumTrackedAccountsSqliteContext, INeuraliumAccountSnapshot, NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry, NeuraliumStandardAccountFreezeSqlite, NeuraliumJointAccountSnapshotSqliteEntry, NeuraliumJointAccountFeatureSqliteEntry, NeuraliumJointMemberAccountSqliteEntry, NeuraliumJointAccountFreezeSqlite, NeuraliumStandardAccountKeysSnapshotSqliteEntry, NeuraliumAccreditationCertificateSnapshotSqliteEntry, NeuraliumAccreditationCertificateSnapshotAccountSqliteEntry, NeuraliumChainOptionsSnapshotSqliteEntry>, INeuraliumAccountSnapshotsProvider {

		public NeuraliumAccountSnapshotsProvider(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}

		protected override ICardUtils CardUtils => NeuraliumCardsUtils.Instance;
	}

	public interface INeuraliumAccountSnapshotsProviderGenerix<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, STANDARD_ACCOUNT_SNAPSHOT_DAL, STANDARD_ACCOUNT_SNAPSHOT_CONTEXT, JOINT_ACCOUNT_SNAPSHOT_DAL, JOINT_ACCOUNT_SNAPSHOT_CONTEXT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL, STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT, CHAIN_OPTIONS_SNAPSHOT_DAL, CHAIN_OPTIONS_SNAPSHOT_CONTEXT, TRACKED_ACCOUNTS_DAL, TRACKED_ACCOUNTS_CONTEXT, ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT> : IAccountSnapshotsProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, STANDARD_ACCOUNT_SNAPSHOT_DAL, STANDARD_ACCOUNT_SNAPSHOT_CONTEXT, JOINT_ACCOUNT_SNAPSHOT_DAL, JOINT_ACCOUNT_SNAPSHOT_CONTEXT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL, STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT, CHAIN_OPTIONS_SNAPSHOT_DAL, CHAIN_OPTIONS_SNAPSHOT_CONTEXT, TRACKED_ACCOUNTS_DAL, TRACKED_ACCOUNTS_CONTEXT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT>
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where STANDARD_ACCOUNT_SNAPSHOT_DAL : class, INeuraliumStandardAccountSnapshotDal<STANDARD_ACCOUNT_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>
		where STANDARD_ACCOUNT_SNAPSHOT_CONTEXT : DbContext, INeuraliumStandardAccountSnapshotContext<STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>
		where JOINT_ACCOUNT_SNAPSHOT_DAL : class, INeuraliumJointAccountSnapshotDal<JOINT_ACCOUNT_SNAPSHOT_CONTEXT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>
		where JOINT_ACCOUNT_SNAPSHOT_CONTEXT : DbContext, INeuraliumJointAccountSnapshotContext<JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL : class, INeuraliumAccreditationCertificatesSnapshotDal<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT : DbContext, INeuraliumAccreditationCertificatesSnapshotContext<ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>
		where STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL : class, INeuraliumAccountKeysSnapshotDal<STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_KEY_SNAPSHOT>
		where STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT : DbContext, INeuraliumAccountKeysSnapshotContext<STANDARD_ACCOUNT_KEY_SNAPSHOT>
		where CHAIN_OPTIONS_SNAPSHOT_DAL : class, INeuraliumChainOptionsSnapshotDal<CHAIN_OPTIONS_SNAPSHOT_CONTEXT, CHAIN_OPTIONS_SNAPSHOT>
		where CHAIN_OPTIONS_SNAPSHOT_CONTEXT : DbContext, INeuraliumChainOptionsSnapshotContext<CHAIN_OPTIONS_SNAPSHOT>
		where TRACKED_ACCOUNTS_DAL : class, INeuraliumTrackedAccountsDal
		where TRACKED_ACCOUNTS_CONTEXT : DbContext, INeuraliumTrackedAccountsContext
		where ACCOUNT_SNAPSHOT : INeuraliumAccountSnapshot
		where STANDARD_ACCOUNT_SNAPSHOT : class, INeuraliumStandardAccountSnapshotEntry<STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where STANDARD_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where STANDARD_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new()
		where JOINT_ACCOUNT_SNAPSHOT : class, INeuraliumJointAccountSnapshotEntry<JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where JOINT_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where JOINT_ACCOUNT_MEMBERS_SNAPSHOT : class, INeuraliumJointMemberAccountEntry, new()
		where JOINT_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new()
		where STANDARD_ACCOUNT_KEY_SNAPSHOT : class, INeuraliumStandardAccountKeysSnapshotEntry, new()
		where ACCREDITATION_CERTIFICATE_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshotEntry<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>, new()
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshotAccountEntry, new()
		where CHAIN_OPTIONS_SNAPSHOT : class, INeuraliumChainOptionsSnapshotEntry, new() {
	}

	public abstract class NeuraliumAccountSnapshotsProviderGenerix<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, STANDARD_ACCOUNT_SNAPSHOT_DAL, STANDARD_ACCOUNT_SNAPSHOT_CONTEXT, JOINT_ACCOUNT_SNAPSHOT_DAL, JOINT_ACCOUNT_SNAPSHOT_CONTEXT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL, STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT, CHAIN_OPTIONS_SNAPSHOT_DAL, CHAIN_OPTIONS_SNAPSHOT_CONTEXT, TRACKED_ACCOUNTS_DAL, TRACKED_ACCOUNTS_CONTEXT, ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT> : AccountSnapshotsProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, STANDARD_ACCOUNT_SNAPSHOT_DAL, STANDARD_ACCOUNT_SNAPSHOT_CONTEXT, JOINT_ACCOUNT_SNAPSHOT_DAL, JOINT_ACCOUNT_SNAPSHOT_CONTEXT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL, STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT, CHAIN_OPTIONS_SNAPSHOT_DAL, CHAIN_OPTIONS_SNAPSHOT_CONTEXT, TRACKED_ACCOUNTS_DAL, TRACKED_ACCOUNTS_CONTEXT, ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT>, INeuraliumAccountSnapshotsProviderGenerix<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, STANDARD_ACCOUNT_SNAPSHOT_DAL, STANDARD_ACCOUNT_SNAPSHOT_CONTEXT, JOINT_ACCOUNT_SNAPSHOT_DAL, JOINT_ACCOUNT_SNAPSHOT_CONTEXT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL, STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT, CHAIN_OPTIONS_SNAPSHOT_DAL, CHAIN_OPTIONS_SNAPSHOT_CONTEXT, TRACKED_ACCOUNTS_DAL, TRACKED_ACCOUNTS_CONTEXT, ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT>
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where STANDARD_ACCOUNT_SNAPSHOT_DAL : class, INeuraliumStandardAccountSnapshotDal<STANDARD_ACCOUNT_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>
		where STANDARD_ACCOUNT_SNAPSHOT_CONTEXT : DbContext, INeuraliumStandardAccountSnapshotContext<STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>
		where JOINT_ACCOUNT_SNAPSHOT_DAL : class, INeuraliumJointAccountSnapshotDal<JOINT_ACCOUNT_SNAPSHOT_CONTEXT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>
		where JOINT_ACCOUNT_SNAPSHOT_CONTEXT : DbContext, INeuraliumJointAccountSnapshotContext<JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL : class, INeuraliumAccreditationCertificatesSnapshotDal<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT : DbContext, INeuraliumAccreditationCertificatesSnapshotContext<ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>
		where STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL : class, INeuraliumAccountKeysSnapshotDal<STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT, STANDARD_ACCOUNT_KEY_SNAPSHOT>
		where STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT : DbContext, INeuraliumAccountKeysSnapshotContext<STANDARD_ACCOUNT_KEY_SNAPSHOT>
		where CHAIN_OPTIONS_SNAPSHOT_DAL : class, INeuraliumChainOptionsSnapshotDal<CHAIN_OPTIONS_SNAPSHOT_CONTEXT, CHAIN_OPTIONS_SNAPSHOT>
		where CHAIN_OPTIONS_SNAPSHOT_CONTEXT : DbContext, INeuraliumChainOptionsSnapshotContext<CHAIN_OPTIONS_SNAPSHOT>
		where TRACKED_ACCOUNTS_DAL : class, INeuraliumTrackedAccountsDal<TRACKED_ACCOUNTS_CONTEXT>
		where TRACKED_ACCOUNTS_CONTEXT : DbContext, INeuraliumTrackedAccountsContext
		where ACCOUNT_SNAPSHOT : INeuraliumAccountSnapshot
		where STANDARD_ACCOUNT_SNAPSHOT : class, INeuraliumStandardAccountSnapshotEntry<STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where STANDARD_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where STANDARD_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new()
		where JOINT_ACCOUNT_SNAPSHOT : class, INeuraliumJointAccountSnapshotEntry<JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where JOINT_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeatureEntry, new()
		where JOINT_ACCOUNT_MEMBERS_SNAPSHOT : class, INeuraliumJointMemberAccountEntry, new()
		where JOINT_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreezeEntry, new()
		where STANDARD_ACCOUNT_KEY_SNAPSHOT : class, INeuraliumStandardAccountKeysSnapshotEntry, new()
		where ACCREDITATION_CERTIFICATE_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshotEntry<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>, new()
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshotAccountEntry, new()
		where CHAIN_OPTIONS_SNAPSHOT : class, INeuraliumChainOptionsSnapshotEntry, new() {

		public NeuraliumAccountSnapshotsProviderGenerix(CENTRAL_COORDINATOR centralCoordinator) : base(centralCoordinator) {
		}

		public override bool IsAccountEntryNull(ACCOUNT_SNAPSHOT accountSnapshotEntry) {
			bool value = base.IsAccountEntryNull(accountSnapshotEntry);

			if(value) {
				return true;
			}

			return accountSnapshotEntry.Balance.Value == 0;
		}

		protected override ICardUtils GetCardUtils() {
			return this.GetNeuraliumCardUtils();
		}

		protected INeuraliumCardsUtils GetNeuraliumCardUtils() {
			return NeuraliumCardsUtils.Instance;
		}

	#region snapshot operations

		protected void UpdateFreezes<ACCOUNT_SNAPSHOT_ENTRY, ACCOUNT_FREEZE_SNAPSHOT_ENTRY>(ACCOUNT_SNAPSHOT_ENTRY snapshot, AccountId accountId, DbSet<ACCOUNT_FREEZE_SNAPSHOT_ENTRY> freezes)
			where ACCOUNT_SNAPSHOT_ENTRY : INeuraliumAccountSnapshot<ACCOUNT_FREEZE_SNAPSHOT_ENTRY>
			where ACCOUNT_FREEZE_SNAPSHOT_ENTRY : class, INeuraliumAccountFreezeEntry {

			var existingFreezes = this.QueryDbSetEntityEntries(freezes, a => a.AccountId == accountId.ToLongRepresentation());

			var snapshotFreezes = snapshot.AccountFreezes.ToList();

			var certificateIds = existingFreezes.Select(a => a.FreezeId).ToList();
			var snapshotCertificates = snapshotFreezes.Select(a => a.FreezeId).ToList();

			// build the delta
			var newFreezes = snapshotFreezes.Where(a => !certificateIds.Contains(a.FreezeId)).ToList();
			var modifyFreezes = snapshotFreezes.Where(a => certificateIds.Contains(a.FreezeId)).ToList();
			var removeFreezes = existingFreezes.Where(a => !snapshotCertificates.Contains(a.FreezeId)).ToList();

			foreach(ACCOUNT_FREEZE_SNAPSHOT_ENTRY attribute in newFreezes) {
				freezes.Add(attribute);
			}

			foreach(ACCOUNT_FREEZE_SNAPSHOT_ENTRY freeze in modifyFreezes) {

				ACCOUNT_FREEZE_SNAPSHOT_ENTRY dbEntry = existingFreezes.Single(a => a.FreezeId == freeze.FreezeId);
				this.GetNeuraliumCardUtils().Copy(freeze, dbEntry);
			}

			foreach(ACCOUNT_FREEZE_SNAPSHOT_ENTRY attribute in removeFreezes) {
				freezes.Remove(attribute);
			}
		}

		protected override STANDARD_ACCOUNT_SNAPSHOT PrepareNewStandardAccountSnapshots(STANDARD_ACCOUNT_SNAPSHOT_CONTEXT db, AccountId accountId, AccountId temporaryHashId, IStandardAccountSnapshot source) {
			STANDARD_ACCOUNT_SNAPSHOT snapshot = base.PrepareNewStandardAccountSnapshots(db, accountId, temporaryHashId, source);

			foreach(STANDARD_ACCOUNT_FREEZE_SNAPSHOT attribute in snapshot.AccountFreezes) {
				db.StandardAccountFreezes.Add(attribute);
			}

			return snapshot;
		}

		protected override STANDARD_ACCOUNT_SNAPSHOT PrepareUpdateStandardAccountSnapshots(STANDARD_ACCOUNT_SNAPSHOT_CONTEXT db, AccountId accountId, IStandardAccountSnapshot source) {
			STANDARD_ACCOUNT_SNAPSHOT snapshot = base.PrepareUpdateStandardAccountSnapshots(db, accountId, source);

			this.UpdateFreezes(snapshot, accountId, db.StandardAccountFreezes);

			return snapshot;
		}

		protected override STANDARD_ACCOUNT_SNAPSHOT PrepareDeleteStandardAccountSnapshots(STANDARD_ACCOUNT_SNAPSHOT_CONTEXT db, AccountId accountId) {
			STANDARD_ACCOUNT_SNAPSHOT snapshot = base.PrepareDeleteStandardAccountSnapshots(db, accountId);

			foreach(STANDARD_ACCOUNT_FREEZE_SNAPSHOT attribute in snapshot.AccountFreezes) {
				db.StandardAccountFreezes.Remove(attribute);
			}

			return snapshot;
		}

		protected override JOINT_ACCOUNT_SNAPSHOT PrepareNewJointAccountSnapshots(JOINT_ACCOUNT_SNAPSHOT_CONTEXT db, AccountId accountId, AccountId temporaryHashId, IJointAccountSnapshot source) {
			JOINT_ACCOUNT_SNAPSHOT snapshot = base.PrepareNewJointAccountSnapshots(db, accountId, temporaryHashId, source);

			foreach(JOINT_ACCOUNT_FREEZE_SNAPSHOT attribute in snapshot.AccountFreezes) {
				db.JointAccountFreezes.Add(attribute);
			}

			return snapshot;
		}

		protected override JOINT_ACCOUNT_SNAPSHOT PrepareUpdateJointAccountSnapshots(JOINT_ACCOUNT_SNAPSHOT_CONTEXT db, AccountId accountId, IJointAccountSnapshot source) {
			JOINT_ACCOUNT_SNAPSHOT snapshot = base.PrepareUpdateJointAccountSnapshots(db, accountId, source);

			this.UpdateFreezes(snapshot, accountId, db.JointAccountFreezes);

			return snapshot;
		}

		protected override JOINT_ACCOUNT_SNAPSHOT PrepareDeleteJointAccountSnapshots(JOINT_ACCOUNT_SNAPSHOT_CONTEXT db, AccountId accountId) {
			JOINT_ACCOUNT_SNAPSHOT snapshot = base.PrepareDeleteJointAccountSnapshots(db, accountId);

			foreach(JOINT_ACCOUNT_FREEZE_SNAPSHOT attribute in snapshot.AccountFreezes) {
				db.JointAccountFreezes.Remove(attribute);
			}

			return snapshot;
		}

	#endregion

	}

}