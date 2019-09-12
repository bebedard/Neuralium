using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1 {

	public interface INeuraliumTransferTransaction : INeuraliumTipingTransaction {
		AccountId Recipient { get; set; }
		Amount Amount { get; set; }
	}

	public class NeuraliumTransferTransaction : NeuraliumTipingTransaction, INeuraliumTransferTransaction {

		public AccountId Recipient { get; set; } = new AccountId();
		public Amount Amount { get; set; } = new Amount();

		public override HashNodeList GetStructuresArray() {
			HashNodeList nodeList = base.GetStructuresArray();

			nodeList.Add(this.Recipient);
			nodeList.Add(this.Amount);

			return nodeList;
		}

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			base.JsonDehydrate(jsonDeserializer);

			//
			jsonDeserializer.SetProperty("Recipient", this.Recipient);
			jsonDeserializer.SetProperty("Amount", this.Amount);
		}

		protected override ComponentVersion<TransactionType> SetIdentity() {
			return (TOKEN_TRANSFER: NeuraliumTransactionTypes.Instance.NEURALIUM_TRANSFER, 1, 0);
		}

		protected override void RehydrateHeader(IDataRehydrator rehydrator) {
			base.RehydrateHeader(rehydrator);

			this.Recipient.Rehydrate(rehydrator);
			this.Amount.Rehydrate(rehydrator);
		}

		protected override void DehydrateHeader(IDataDehydrator dehydrator) {
			base.DehydrateHeader(dehydrator);

			this.Recipient.Dehydrate(dehydrator);

			this.Amount.Dehydrate(dehydrator);
		}
	}

}