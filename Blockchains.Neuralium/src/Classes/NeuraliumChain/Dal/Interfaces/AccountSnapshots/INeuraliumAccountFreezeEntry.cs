using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots {

	public interface INeuraliumAccountFreezeEntry : INeuraliumAccountFreeze {

		long Id { get; set; }

		long AccountId { get; set; }
	}

}