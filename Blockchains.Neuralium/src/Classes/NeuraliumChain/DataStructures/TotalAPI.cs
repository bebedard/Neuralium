using MessagePack;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures {
	
	[MessagePackObject(keyAsPropertyName: true)]
	public class TotalAPI {
		public decimal Total { get; set; }
		public decimal ReservedCredit { get; set; }
		public decimal ReservedDebit { get; set; }
		public decimal Frozen { get; set; }
	}
}