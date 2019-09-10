using System;
using System.Collections.Generic;
using System.Linq;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization.Blockchain.Utils;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.Moderator;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1.Security {

	public interface INeuraliuFreezeSuspiciousFundsTransaction : INeuraliumModerationTransaction {
		List<NeuraliuFreezeSuspiciousFundsTransaction.TransactionFlowNode> SuspectTransactionFreezeTree { get; }
		List<AccountId> Accounts { get; }
		ushort FreezeId { get; set; }
		Text EventDescription { get; set; }
		DateTime AllegedTheftTimestamp { get; set; }
		List<(AccountId accountId, decimal amount)> GetFlatImpactTree();
	}

	public class NeuraliuFreezeSuspiciousFundsTransaction : ModerationTransaction, INeuraliuFreezeSuspiciousFundsTransaction {

		public ushort FreezeId { get; set; }
		public Text EventDescription { get; set; } = new Text();
		public DateTime AllegedTheftTimestamp { get; set; }

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			base.JsonDehydrate(jsonDeserializer);

			jsonDeserializer.SetProperty("FreezeId", this.FreezeId);
			jsonDeserializer.SetProperty("EventDescription", this.EventDescription);
			jsonDeserializer.SetProperty("AllegedTheftTimestamp", this.AllegedTheftTimestamp);
			jsonDeserializer.SetArray("SuspectTransactionFreezeTree", this.SuspectTransactionFreezeTree);
		}

		public override HashNodeList GetStructuresArray() {
			HashNodeList hasNodes = base.GetStructuresArray();

			hasNodes.Add(this.SuspectTransactionFreezeTree);

			return hasNodes;
		}

		public List<AccountId> Accounts => this.SuspectTransactionFreezeTree.Select(t => t.TransactionId.Account).Distinct().ToList();
		public List<TransactionFlowNode> SuspectTransactionFreezeTree { get; } = new List<TransactionFlowNode>();

		public List<(AccountId accountId, decimal amount)> GetFlatImpactTree() {
			var impacts = new List<(AccountId accountId, decimal amount)>();

			this.ParseTreeNode(this.SuspectTransactionFreezeTree, impacts);

			return impacts;
		}

		protected override void RehydrateContents(ChannelsEntries<IDataRehydrator> dataChannels, ITransactionRehydrationFactory rehydrationFactory) {
			base.RehydrateContents(dataChannels, rehydrationFactory);

			this.FreezeId = dataChannels.ContentsData.ReadUShort();
			this.EventDescription.Rehydrate(dataChannels.ContentsData);
			this.AllegedTheftTimestamp = dataChannels.ContentsData.ReadDateTime();

			dataChannels.ContentsData.ReadRehydratableArray(this.SuspectTransactionFreezeTree);
		}

		protected override void DehydrateContents(ChannelsEntries<IDataDehydrator> dataChannels) {
			base.DehydrateContents(dataChannels);

			dataChannels.ContentsData.Write(this.FreezeId);
			this.EventDescription.Dehydrate(dataChannels.ContentsData);
			dataChannels.ContentsData.Write(this.AllegedTheftTimestamp);

			dataChannels.ContentsData.Write(this.SuspectTransactionFreezeTree);
		}

		private void ParseTreeNode(List<TransactionFlowNode> nodes, List<(AccountId accountId, decimal amount)> impacts) {

			foreach(TransactionFlowNode node in nodes) {

				(AccountId accountId, decimal amount) entry = impacts.SingleOrDefault(e => e.accountId == node.TransactionId.Account);

				entry.amount += node.Amount;

				this.ParseTreeNode(node.OutgoingSuspectTransactions, impacts);
			}
		}

		protected override ComponentVersion<TransactionType> SetIdentity() {
			return (NeuraliumTransactionTypes.Instance.NEURALIUM_FREEZE_SUSPICIOUSACCOUNTS, 1, 0);
		}

		public class TransactionFlowNode : ISerializableCombo {
			public TransactionId TransactionId { get; set; } = new TransactionId();
			public Amount Amount { get; set; } = new Amount();

			public List<TransactionFlowNode> OutgoingSuspectTransactions { get; } = new List<TransactionFlowNode>();

			/// <summary>
			///     Get the amount that must be frozen on this account
			/// </summary>
			public decimal FrozenTotal {
				get {
					decimal outgoingTotal = 0;

					if(this.OutgoingSuspectTransactions.Any()) {
						outgoingTotal = this.OutgoingSuspectTransactions.Sum(s => s.Amount.Value);
					}

					return outgoingTotal;
				}
			}

			public void Rehydrate(IDataRehydrator rehydrator) {

				this.TransactionId.Rehydrate(rehydrator);
				this.Amount.Rehydrate(rehydrator);
				rehydrator.ReadRehydratableArray(this.OutgoingSuspectTransactions);
			}

			public void Dehydrate(IDataDehydrator dehydrator) {
				this.TransactionId.Dehydrate(dehydrator);
				this.Amount.Dehydrate(dehydrator);
				dehydrator.Write(this.OutgoingSuspectTransactions);
			}

			public HashNodeList GetStructuresArray() {
				HashNodeList hasNodes = new HashNodeList();

				hasNodes.Add(this.TransactionId);
				hasNodes.Add(this.Amount);
				hasNodes.Add(this.OutgoingSuspectTransactions);

				return hasNodes;
			}

			public void JsonDehydrate(JsonDeserializer jsonDeserializer) {
				jsonDeserializer.SetProperty("TransactionId", this.TransactionId);
				jsonDeserializer.SetProperty("Amount", this.Amount);

				jsonDeserializer.SetArray("OutgoingSuspectTransactions", this.OutgoingSuspectTransactions);
			}
		}
	}
}