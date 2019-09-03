using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots {

	public interface INeuraliumAccountFreezeSqlite : INeuraliumAccountFreezeEntry {
	}

	public abstract class NeuraliumAccountFreezeSqlite : INeuraliumAccountFreezeSqlite {

		public int FreezeId { get; set; }
		public decimal Amount { get; set; }
		public long Id { get; set; }
		public long AccountId { get; set; }
	}
}