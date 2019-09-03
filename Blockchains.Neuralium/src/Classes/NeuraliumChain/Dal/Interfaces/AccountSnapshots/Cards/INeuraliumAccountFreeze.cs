namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards {
	public interface INeuraliumAccountFreeze : INeuraliumSnapshot {

		int FreezeId { get; set; }

		decimal Amount { get; set; }
	}

}