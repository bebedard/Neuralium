using LiteDB;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account {

	public interface INeuraliumWalletElectionsHistory : IWalletElectionsHistory {

		[BsonId]
		new long BlockId { get; set; }

		decimal Bounty { get; set; }

		decimal Tips { get; set; }
	}

	public class NeuraliumWalletElectionsHistory : WalletElectionsHistory, INeuraliumWalletElectionsHistory {

		[BsonId]
		public new long BlockId { get; set; }

		public decimal Bounty { get; set; }
		public decimal Tips { get; set; }
	}
}