using MessagePack;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI.Wallet;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures {
	
	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumWalletTransactionHistoryHeaderAPI : WalletTransactionHistoryHeaderAPI {
		public decimal Amount { get; set; }
		public decimal Tip { get; set; }

		public override string ToString() {
			return base.ToString() + $", Amount: {this.Amount}, Tip: {this.Tip}";
		}
	}

	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumWalletTransactionHistoryDetailsAPI : WalletTransactionHistoryDetailsAPI {
		public decimal Amount { get; set; }
		public decimal Tip { get; set; }

		public override string ToString() {
			return base.ToString() + $", Amount: {this.Amount}, Tip: {this.Tip}";
		}
	}

}