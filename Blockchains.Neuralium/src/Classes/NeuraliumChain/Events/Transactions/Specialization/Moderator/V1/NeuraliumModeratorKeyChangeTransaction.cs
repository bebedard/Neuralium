using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.Moderator.V1;
using Neuralia.Blockchains.Core;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1 {
	public class NeuraliumModeratorKeyChangeTransaction : ModeratorKeyChangeTransaction, INeuraliumModerationTransaction {

		public NeuraliumModeratorKeyChangeTransaction() {
			// rehydration only
		}

		public NeuraliumModeratorKeyChangeTransaction(byte ordinalId, Enums.KeyTypes keyType) : base(ordinalId, keyType) {
		}
	}
}