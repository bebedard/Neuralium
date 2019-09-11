using System.Collections.Generic;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI {
	public class MiningHistory {
		public readonly List<string> selectedTransactions = new List<string>();
		public long blockId { get; set; }

		public override string ToString() {
			return $"BlockId: {this.blockId}, SelectedTransactions: {string.Join(",", this.selectedTransactions)}";
		}
	}
}