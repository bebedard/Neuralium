using System.Collections.Generic;
using System.Linq;
using Neuralia.Blockchains.Common.Classes.Tools.Serialization;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.General.Arrays;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.Moderator.V1 {

	public interface IReclaimAccountsTransaction : IModerationTransaction {
		List<ReclaimAccountsTransaction.AccountReset> Accounts { get; }
	}

	public class ReclaimAccountsTransaction : ModerationTransaction, IReclaimAccountsTransaction {

		public List<AccountReset> Accounts { get; } = new List<AccountReset>();

		public override HashNodeList GetStructuresArray() {
			HashNodeList nodeList = base.GetStructuresArray();

			nodeList.Add(this.Accounts.Count);

			foreach(AccountReset entry in this.Accounts.OrderBy(a => a)) {
				nodeList.Add(entry.Account);
				nodeList.Add(entry.Reason);
			}

			return nodeList;
		}

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			base.JsonDehydrate(jsonDeserializer);

			jsonDeserializer.SetArray("Accounts", this.Accounts, (ds, e) => {

				ds.SetProperty("Account", e.Account);
				ds.SetProperty("Reason", e.Reason);
			});
		}

		protected override void RehydrateHeader(IDataRehydrator rehydrator) {
			base.RehydrateHeader(rehydrator);

			var parameters = new AccountIdGroupSerializer.AccountIdGroupSerializerRehydrateParameters<AccountId>();

			var accounts = AccountIdGroupSerializer.Rehydrate(rehydrator, true, parameters);

			int count = accounts.Count;
			var orderedAccounts = accounts.OrderBy(s => s).ToList();

			TwoBitArray twoBitArray = new TwoBitArray(rehydrator.ReadArray(TwoBitArray.GetCorrespondingByteSize(count)), count);

			for(int i = 0; i < count; i++) {
				AccountReset entry = new AccountReset();

				entry.Account = orderedAccounts[i];
				entry.Reason = twoBitArray[i];

				this.Accounts.Add(entry);
			}
		}

		protected override void DehydrateHeader(IDataDehydrator dehydrator) {
			base.DehydrateHeader(dehydrator);

			var parameters = new AccountIdGroupSerializer.AccountIdGroupSerializerDehydrateParameters<AccountReset, AccountId>();

			AccountIdGroupSerializer.Dehydrate(this.Accounts.ToDictionary(a => a.Account), dehydrator, true, parameters);

			var orderedAccounts = this.Accounts.OrderBy(s => s.Account);

			TwoBitArray twoBitArray = new TwoBitArray(this.Accounts.Count);
			int accountIndex = 0;

			foreach(AccountReset entry in orderedAccounts) {
				twoBitArray[accountIndex++] = entry.Reason;
			}

			dehydrator.Write(twoBitArray.GetData());

		}

		protected override ComponentVersion<TransactionType> SetIdentity() {
			return (TransactionTypes.Instance.MODERATION_RECLAIM_ACCOUNTS, 1, 0);
		}

		public struct AccountReset {
			public AccountId Account { get; set; }
			public byte Reason { get; set; }
		}
	}
}