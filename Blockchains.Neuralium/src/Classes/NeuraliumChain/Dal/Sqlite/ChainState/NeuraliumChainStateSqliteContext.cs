using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.ChainState;
using Microsoft.EntityFrameworkCore;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.ChainState;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.ChainState {
	public interface INeuraliumChainStateSqliteContext : INeuraliumChainStateContext<NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry>, IChainStateSqliteContext<NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry> {
	}

	public class NeuraliumChainStateSqliteContext : ChainStateSqliteContext<NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry>, INeuraliumChainStateSqliteContext {
		protected override void OnModelCreating(ModelBuilder modelBuilder) {

			modelBuilder.Entity<NeuraliumChainStateSqliteEntry>();

			base.OnModelCreating(modelBuilder);
		}
	}
}