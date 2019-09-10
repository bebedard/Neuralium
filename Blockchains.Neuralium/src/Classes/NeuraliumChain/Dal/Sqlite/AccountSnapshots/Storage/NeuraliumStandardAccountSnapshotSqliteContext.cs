using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage;
using Microsoft.EntityFrameworkCore;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots.Storage;
using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots.Storage {
	public interface INeuraliumStandardAccountSnapshotSqliteContext : INeuraliumStandardAccountSnapshotContext<NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry, NeuraliumStandardAccountFreezeSqlite>, IStandardAccountSnapshotSqliteContext<NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry> {
	}

	public class NeuraliumStandardAccountSnapshotSqliteContext : StandardAccountSnapshotSqliteContext<NeuraliumStandardAccountSnapshotSqliteEntry, NeuraliumStandardAccountFeatureSqliteEntry>, INeuraliumStandardAccountSnapshotSqliteContext {

		public DbSet<NeuraliumStandardAccountFreezeSqlite> StandardAccountFreezes { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {

			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<NeuraliumStandardAccountSnapshotSqliteEntry>(eb => {

				eb.Property(b => b.Balance).HasConversion(v => v.Value, v => (Amount) v);
			});

			modelBuilder.Entity<NeuraliumJointAccountSnapshotSqliteEntry>(eb => {

				eb.Property(b => b.Balance).HasConversion(v => v.Value, v => (Amount) v);
			});

			modelBuilder.Entity<NeuraliumAccreditationCertificateSnapshotSqliteEntry>(eb => {

				eb.Property(b => b.ProviderBountyshare).HasConversion(v => v.Value, v => (Amount) v);

				eb.Property(b => b.InfrastructureServiceFees).HasConversion(v => v.Value, v => (Amount) v);

			});

			modelBuilder.Entity<NeuraliumStandardAccountFreezeSqlite>(eb => {
				eb.HasKey(c => c.Id);
				eb.Property(b => b.Id).ValueGeneratedOnAdd();
				eb.HasIndex(b => b.Id).IsUnique();

				eb.ToTable("AccountFreezes");
			});
		}
	}
}