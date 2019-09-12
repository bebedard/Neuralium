using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1.Structures {

	public interface INeuraliumTransactionAccountFreeze : ISerializableCombo, INeuraliumAccountFreeze {
	}

	public abstract class NeuraliumTransactionAccountFreeze : INeuraliumTransactionAccountFreeze {

		public void Rehydrate(IDataRehydrator rehydrator) {
			throw new NotImplementedException();
		}

		public void Dehydrate(IDataDehydrator dehydrator) {
			throw new NotImplementedException();
		}

		public HashNodeList GetStructuresArray() {
			throw new NotImplementedException();
		}

		public void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			throw new NotImplementedException();
		}

		public int FreezeId { get; set; }
		public decimal Amount { get; set; }
	}
}