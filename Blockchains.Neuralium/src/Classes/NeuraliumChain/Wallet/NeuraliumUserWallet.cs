using System;
using LiteDB;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet {

	public interface INeuraliumUserWallet : IUserWallet {

		[BsonId]
		new Guid Id { get; set; }
	}

	public class NeuraliumUserWallet : UserWallet, INeuraliumUserWallet {

		[BsonId]
		public new Guid Id { get; set; } = Guid.NewGuid();
	}

}