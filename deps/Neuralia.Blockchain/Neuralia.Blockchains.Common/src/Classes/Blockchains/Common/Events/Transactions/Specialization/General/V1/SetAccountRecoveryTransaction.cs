using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization.Blockchain.Utils;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.General.V1 {

	public interface ISetAccountRecoveryTransaction : ITransaction {
		SetAccountRecoveryTransaction.OperationTypes Operation { get; set; }
		IByteArray AccountRecoveryHash { get; set; }
	}

	public abstract class SetAccountRecoveryTransaction : Transaction, ISetAccountRecoveryTransaction {

		public enum OperationTypes : byte {
			Create = 1,
			Revoke = 2
		}

		public override HashNodeList GetStructuresArray() {
			HashNodeList hashNodeList = base.GetStructuresArray();

			hashNodeList.Add((byte) this.Operation);
			hashNodeList.Add(this.AccountRecoveryHash);

			return hashNodeList;
		}

		public OperationTypes Operation { get; set; }
		public IByteArray AccountRecoveryHash { get; set; }

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			base.JsonDehydrate(jsonDeserializer);

			//
			jsonDeserializer.SetProperty("Operation", this.Operation);
			jsonDeserializer.SetProperty("AccountRecoveryHash", this.AccountRecoveryHash);
		}

		protected override void RehydrateContents(ChannelsEntries<IDataRehydrator> dataChannels, ITransactionRehydrationFactory rehydrationFactory) {
			base.RehydrateContents(dataChannels, rehydrationFactory);

			this.Operation = (OperationTypes) dataChannels.ContentsData.ReadByte();
			this.AccountRecoveryHash = dataChannels.ContentsData.ReadNonNullableArray();

		}

		protected override void DehydrateContents(ChannelsEntries<IDataDehydrator> dataChannels) {
			base.DehydrateContents(dataChannels);

			dataChannels.ContentsData.Write((byte) this.Operation);
			dataChannels.ContentsData.WriteNonNullable(this.AccountRecoveryHash);
		}

		protected override ComponentVersion<TransactionType> SetIdentity() {
			return (TransactionTypes.Instance.SET_ACCOUNT_RECOVERY, 1, 0);
		}
	}
}