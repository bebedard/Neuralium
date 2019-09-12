using System.Collections.Generic;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.Moderator;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1.Security {

	public interface INeuraliuUnfreezeClearedFundsTransaction : INeuraliumModerationTransaction {
		ushort FreezeId { get; set; }
		Text Reason { get; set; }
		List<NeuraliuUnfreezeClearedFundsTransaction.AccountUnfreeze> Accounts { get; }
	}

	public class NeuraliuUnfreezeClearedFundsTransaction : ModerationTransaction, INeuraliuUnfreezeClearedFundsTransaction {

		public ushort FreezeId { get; set; }
		public Text Reason { get; set; } = new Text();

		public List<AccountUnfreeze> Accounts { get; } = new List<AccountUnfreeze>();

		public override void Rehydrate(IDataRehydrator rehydrator) {
			base.Rehydrate(rehydrator);
			this.FreezeId = rehydrator.ReadUShort();
			this.Reason.Rehydrate(rehydrator);
			rehydrator.ReadRehydratableArray(this.Accounts);
		}

		public override void Dehydrate(IDataDehydrator dehydrator) {
			base.Dehydrate(dehydrator);

			dehydrator.Write(this.FreezeId);
			this.Reason.Dehydrate(dehydrator);
			dehydrator.Write(this.Accounts);
		}

		public override HashNodeList GetStructuresArray() {
			HashNodeList hasNodes = new HashNodeList();

			hasNodes.Add(this.FreezeId);
			hasNodes.Add(this.Reason);
			hasNodes.Add(this.Accounts);

			return hasNodes;
		}

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {

			jsonDeserializer.SetProperty("FreezeId", this.FreezeId);
			jsonDeserializer.SetProperty("Reason", this.Reason);
			jsonDeserializer.SetArray("Accounts", this.Accounts);
		}

		protected override ComponentVersion<TransactionType> SetIdentity() {
			return (NeuraliumTransactionTypes.Instance.NEURALIUM_UNFREEZE_SUSPICIOUSACCOUNTS, 1, 0);
		}

		public class AccountUnfreeze : ISerializableCombo {
			public AccountId AccountId { get; set; } = new AccountId();
			public Text Notes { get; set; } = new Text();

			public void Rehydrate(IDataRehydrator rehydrator) {

				this.AccountId.Rehydrate(rehydrator);
				this.Notes.Rehydrate(rehydrator);
			}

			public void Dehydrate(IDataDehydrator dehydrator) {
				this.AccountId.Dehydrate(dehydrator);
				this.Notes.Dehydrate(dehydrator);
			}

			public HashNodeList GetStructuresArray() {
				HashNodeList hasNodes = new HashNodeList();

				hasNodes.Add(this.AccountId);
				hasNodes.Add(this.Notes);

				return hasNodes;
			}

			public void JsonDehydrate(JsonDeserializer jsonDeserializer) {
				jsonDeserializer.SetProperty("AccountId", this.AccountId);
				jsonDeserializer.SetProperty("Notes", this.Notes);
			}
		}
	}
}