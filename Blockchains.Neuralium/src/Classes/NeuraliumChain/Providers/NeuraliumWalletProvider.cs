using System;
using System.Collections.Generic;
using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Wallet;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Wallet.Extra;
using Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures;
using Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures.Timeline;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags;
using Blockchains.Neuralium.Classes.NeuraliumChain.Tools;
using Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account;
using Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account.Snapshots;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Wallet;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Wallet.Extra;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI.Wallet;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account.Snapshots;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.Cryptography.Passphrases;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Services;
using Newtonsoft.Json;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Providers {

	public interface INeuraliumWalletProvider : IWalletProvider {

		TotalAPI GetAccountBalance(bool includeReserved);
		TotalAPI GetAccountBalance(AccountId accountId, bool includeReserved);
		TotalAPI GetAccountBalance(Guid accountUuid, bool includeReserved);
		TimelineHeader GetTimelineHeader(Guid accountUuid);
		List<TimelineDay> GetTimelineSection(Guid accountUuid, DateTime firstday, int skip = 0, int take = 1);
	}

	public interface INeuraliumWalletProviderInternal : INeuraliumWalletProvider, IWalletProviderInternal {
	}

	public class NeuraliumWalletProvider : WalletProvider<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumWalletProviderInternal {

		public NeuraliumWalletProvider(INeuraliumCentralCoordinator centralCoordinator) : base(GlobalsService.TOKEN_CHAIN_NAME, centralCoordinator) {

		}

		public new INeuraliumWalletSerialisationFal SerialisationFal => (INeuraliumWalletSerialisationFal) base.SerialisationFal;

		protected override ICardUtils CardUtils => NeuraliumCardsUtils.Instance;

		public override IWalletElectionsHistory InsertElectionsHistoryEntry(SynthesizedBlock.SynthesizedElectionResult electionResult, AccountId electedAccountId) {
			this.EnsureWalletLoaded();
			IWalletElectionsHistory historyEntry = base.InsertElectionsHistoryEntry(electionResult, electedAccountId);

			// now let's add a neuralium timeline entry
			if(historyEntry is INeuraliumWalletElectionsHistory neuraliumWalletElectionsHistory && electionResult is NeuraliumSynthesizedBlock.NeuraliumSynthesizedElectionResult neuraliumSynthesizedElectionResult) {

				IWalletAccount account = this.WalletFileInfo.WalletBase.Accounts.Values.SingleOrDefault(a => a.GetAccountId() == electedAccountId);

				if(account == null) {
					throw new ApplicationException("Invalid account");
				}

				if(this.WalletFileInfo.Accounts[account.AccountUuid] is INeuraliumAccountFileInfo neuraliumAccountFileInfo) {
					NeuraliumWalletTimeline neuraliumWalletTimeline = new NeuraliumWalletTimeline();
					INeuraliumWalletTimelineFileInfo neuraliumWalletTimelineFileInfo = neuraliumAccountFileInfo.WalletTimelineFileInfo;

					neuraliumWalletTimeline.Timestamp = neuraliumSynthesizedElectionResult.Timestamp;
					neuraliumWalletTimeline.Amount = neuraliumSynthesizedElectionResult.ElectedGains[electedAccountId].bountyShare;
					neuraliumWalletTimeline.Tips = neuraliumSynthesizedElectionResult.ElectedGains[electedAccountId].tips;

					neuraliumWalletTimeline.RecipientAccountId = electedAccountId;
					neuraliumWalletTimeline.Direction = NeuraliumWalletTimeline.MoneratyTransactionTypes.Credit;
					neuraliumWalletTimeline.CreditType = NeuraliumWalletTimeline.CreditTypes.Election;

					neuraliumWalletTimeline.Total = this.GetAccountBalance(electedAccountId, false).Total + neuraliumWalletTimeline.Amount + neuraliumWalletTimeline.Tips;

					neuraliumWalletTimelineFileInfo.InsertTimelineEntry(neuraliumWalletTimeline);
				}
			}

			return historyEntry;
		}

		public override IWalletTransactionHistoryFileInfo UpdateLocalTransactionHistoryEntry(TransactionId transactionId, WalletTransactionHistory.TransactionStatuses status) {
			this.EnsureWalletLoaded();
			IWalletTransactionHistoryFileInfo historyEntry = base.UpdateLocalTransactionHistoryEntry(transactionId, status);

			IWalletAccount account = this.WalletFileInfo.WalletBase.Accounts.Values.SingleOrDefault(a => (a.GetAccountId() == transactionId.Account) || (a.PresentationTransactionId == transactionId));

			if(account == null) {
				throw new ApplicationException("Invalid account");
			}

			if(historyEntry is INeuraliumWalletTransactionHistoryFileInfo neuraliumWalletTransactionHistoryFileInfo && this.WalletFileInfo.Accounts[account.AccountUuid] is INeuraliumAccountFileInfo neuraliumAccountFileInfo) {
				INeuraliumWalletTimelineFileInfo neuraliumWalletTimelineFileInfo = neuraliumAccountFileInfo.WalletTimelineFileInfo;

				if(status == WalletTransactionHistory.TransactionStatuses.Confirmed) {
					// now let's add a neuralium timeline entry
					neuraliumWalletTimelineFileInfo.ConfirmLocalTimelineEntry(transactionId);

				} else if(status == WalletTransactionHistory.TransactionStatuses.Rejected) {
					// now let's add a neuralium timeline entry
					neuraliumWalletTimelineFileInfo.RemoveLocalTimelineEntry(transactionId);
				}
			}

			return historyEntry;
		}

		public override IWalletTransactionHistory InsertTransactionHistoryEntry(ITransaction transaction, AccountId targetAccountId, string note) {
			this.EnsureWalletLoaded();
			IWalletTransactionHistory historyEntry = base.InsertTransactionHistoryEntry(transaction, targetAccountId, note);

			if(historyEntry is INeuraliumWalletTransactionHistory neuraliumWalletTransactionHistory) {

				this.InsertNeuraliumTransactionTimelineEntry(transaction, targetAccountId, neuraliumWalletTransactionHistory);
			}

			return historyEntry;
		}

		public override List<WalletTransactionHistoryHeaderAPI> APIQueryWalletTransactionHistory(Guid accountUuid) {
			this.EnsureWalletLoaded();

			if(!this.IsWalletLoaded) {
				throw new ApplicationException("Wallet is not loaded");
			}

			if(accountUuid == Guid.Empty) {
				accountUuid = this.GetAccountUuid();
			}

			//TODO: merge correctly with base version of this method
			var results = this.WalletFileInfo.Accounts[accountUuid].WalletTransactionHistoryInfo.RunQuery<NeuraliumWalletTransactionHistoryHeaderAPI, NeuraliumWalletTransactionHistory>(caches => caches.Select(t => {

				TransactionId transactionId = new TransactionId(t.TransactionId);
				var version = new ComponentVersion<TransactionType>(t.Version);

				return new NeuraliumWalletTransactionHistoryHeaderAPI {
					TransactionId = t.TransactionId, Sender = transactionId.Account.ToString(), Timestamp = t.Timestamp, Status = t.Status,
					Version = new {transactionType = version.Type.Value, major = version.Major.Value, minor = version.Minor.Value}, Recipient = t.Recipient, Local = t.Local, Amount = t.Amount,
					Tip = t.Tip, Note = t.Note
				};
			}).OrderByDescending(t => t.Timestamp).ToList());

			return results.Cast<WalletTransactionHistoryHeaderAPI>().ToList();

		}

		public override WalletTransactionHistoryDetailsAPI APIQueryWalletTransationHistoryDetails(Guid accountUuid, string transactionId) {
			this.EnsureWalletLoaded();

			if(!this.IsWalletLoaded) {
				throw new ApplicationException("Wallet is not loaded");
			}

			if(accountUuid == Guid.Empty) {
				accountUuid = this.GetAccountUuid();
			}

			var results = this.WalletFileInfo.Accounts[accountUuid].WalletTransactionHistoryInfo.RunQuery<NeuraliumWalletTransactionHistoryDetailsAPI, NeuraliumWalletTransactionHistory>(caches => caches.Where(t => t.TransactionId == transactionId).Select(t => {

				var version = new ComponentVersion<TransactionType>(t.Version);

				return new NeuraliumWalletTransactionHistoryDetailsAPI {
					TransactionId = t.TransactionId, Sender = new TransactionId(t.TransactionId).Account.ToString(), Timestamp = t.Timestamp, Status = t.Status,
					Version = new {transactionType = version.Type.Value, major = version.Major.Value, minor = version.Minor.Value}, Recipient = t.Recipient, Contents = t.Contents, Local = t.Local,
					Amount = t.Amount, Tip = t.Tip, Note = t.Note
				};
			}).ToList());

			return results.SingleOrDefault();

		}

		public TotalAPI GetAccountBalance(bool includeReserved) {
			return this.GetAccountBalance(this.GetActiveAccount().AccountUuid, includeReserved);
		}

		public TotalAPI GetAccountBalance(AccountId accountId, bool includeReserved) {

			return this.GetAccountBalance(this.GetWalletAccount(accountId).AccountUuid, includeReserved);
		}

		public TotalAPI GetAccountBalance(Guid accountUuid, bool includeReserved) {
			this.EnsureWalletLoaded();

			TotalAPI result = new TotalAPI();

			if(!this.WalletFileInfo.Accounts.ContainsKey(accountUuid)) {
				return result;
			}

			IWalletAccountSnapshot accountBase = this.WalletFileInfo.Accounts[accountUuid].WalletSnapshotInfo.WalletAccountSnapshot;

			if(accountBase is INeuraliumWalletAccountSnapshot walletAccountSnapshot) {
				result.Total = walletAccountSnapshot.Balance;
			}

			if(includeReserved) {
				IWalletTransactionCacheFileInfo accountCacheBase = this.WalletFileInfo.Accounts[accountUuid].WalletTransactionCacheInfo;

				if(accountCacheBase is NeuraliumWalletTransactionCacheFileInfo neuraliumWalletTransactionCacheFileInfo) {
					(decimal debit, decimal credit, decimal tip) results = neuraliumWalletTransactionCacheFileInfo.GetTransactionAmounts();

					result.ReservedDebit = results.debit + results.tip;
					result.ReservedCredit = results.credit;
				}
			}

			return result;
		}

		public override void InsertLocalTransactionCacheEntry(ITransactionEnvelope transactionEnvelope) {
			base.InsertLocalTransactionCacheEntry(transactionEnvelope);

			AccountId targetAccountId = transactionEnvelope.Contents.Uuid.Account;

			TotalAPI total = this.GetAccountBalance(targetAccountId, true);
			this.centralCoordinator.PostSystemEvent(NeuraliumSystemEventGenerator.NeuraliumAccountTotalUpdated(targetAccountId.SequenceId, targetAccountId.AccountType, total));
		}

		public override void UpdateLocalTransactionCacheEntry(TransactionId transactionId, WalletTransactionCache.TransactionStatuses status, long gossipMessageHash) {
			base.UpdateLocalTransactionCacheEntry(transactionId, status, gossipMessageHash);

			AccountId targetAccountId = transactionId.Account;

			TotalAPI total = this.GetAccountBalance(targetAccountId, true);
			this.centralCoordinator.PostSystemEvent(NeuraliumSystemEventGenerator.NeuraliumAccountTotalUpdated(targetAccountId.SequenceId, targetAccountId.AccountType, total));
		}

		private void InsertNeuraliumTransactionTimelineEntry(ITransaction transaction, AccountId targetAccountId, INeuraliumWalletTransactionHistory neuraliumWalletTransactionHistory) {
			if((neuraliumWalletTransactionHistory.Amount == 0) && (neuraliumWalletTransactionHistory.Tip == 0)) {
				// this transaction is most probably not a token influencing transaction. let's ignore 0 values
				return;
			}

			// this is an incomming transaction, now let's add a neuralium timeline entry

			IWalletAccount account = this.WalletFileInfo.WalletBase.Accounts.Values.SingleOrDefault(a => a.GetAccountId() == targetAccountId);

			if(account == null) {
				throw new ApplicationException("Invalid account");
			}

			if(this.WalletFileInfo.Accounts[account.AccountUuid] is INeuraliumAccountFileInfo neuraliumAccountFileInfo) {
				NeuraliumWalletTimeline neuraliumWalletTimeline = new NeuraliumWalletTimeline();
				INeuraliumWalletTimelineFileInfo neuraliumWalletTimelineFileInfo = neuraliumAccountFileInfo.WalletTimelineFileInfo;

				neuraliumWalletTimeline.Timestamp = this.serviceSet.TimeService.GetTimestampDateTime(transaction.TransactionId.Timestamp.Value, this.centralCoordinator.ChainComponentProvider.ChainStateProvider.ChainInception);
				neuraliumWalletTimeline.Amount = neuraliumWalletTransactionHistory.Amount;
				neuraliumWalletTimeline.Tips = 0;

				neuraliumWalletTimeline.TransactionId = transaction.TransactionId.ToString();

				bool local = targetAccountId == transaction.TransactionId.Account;
				neuraliumWalletTimeline.SenderAccountId = transaction.TransactionId.Account;
				neuraliumWalletTimeline.RecipientAccountId = targetAccountId;

				decimal total = this.GetAccountBalance(targetAccountId, false).Total;

				if(local) {

					// in most cases, transactions we make wil be debits
					neuraliumWalletTimeline.Direction = NeuraliumWalletTimeline.MoneratyTransactionTypes.Debit;
					neuraliumWalletTimeline.Total = total - neuraliumWalletTimeline.Amount;
					neuraliumWalletTimeline.Tips = neuraliumWalletTransactionHistory.Tip;

#if TESTNET || DEVNET
					if(transaction is INeuraliumRefillNeuraliumsTransaction) {
						neuraliumWalletTimeline.Total = total + neuraliumWalletTimeline.Amount;
						neuraliumWalletTimeline.Direction = NeuraliumWalletTimeline.MoneratyTransactionTypes.Credit;
						neuraliumWalletTimeline.CreditType = NeuraliumWalletTimeline.CreditTypes.Tranasaction;
					}
#endif

					neuraliumWalletTimeline.Total -= neuraliumWalletTimeline.Tips;
					neuraliumWalletTimeline.Confirmed = false;

				} else {
					neuraliumWalletTimeline.Total = total + neuraliumWalletTimeline.Amount;

					neuraliumWalletTimeline.Confirmed = true;
					neuraliumWalletTimeline.Direction = NeuraliumWalletTimeline.MoneratyTransactionTypes.Credit;
					neuraliumWalletTimeline.CreditType = NeuraliumWalletTimeline.CreditTypes.Tranasaction;
				}

				neuraliumWalletTimelineFileInfo.InsertTimelineEntry(neuraliumWalletTimeline);
			}
		}

		protected override IAccountFileInfo CreateNewAccountFileInfo(AccountPassphraseDetails accountSecurityDetails) {
			return new NeuraliumAccountFileInfo(accountSecurityDetails);
		}

		protected override void FillWalletElectionsHistoryEntry(IWalletElectionsHistory walletElectionsHistory, SynthesizedBlock.SynthesizedElectionResult electionResult, AccountId electedAccountId) {

			if(walletElectionsHistory is INeuraliumWalletElectionsHistory neuraliumWalletElectionsHistory && electionResult is NeuraliumSynthesizedBlock.NeuraliumSynthesizedElectionResult neuraliumSynthesizedElectionResult) {

				neuraliumWalletElectionsHistory.Bounty = neuraliumSynthesizedElectionResult.ElectedGains[electedAccountId].bountyShare;
				neuraliumWalletElectionsHistory.Tips = neuraliumSynthesizedElectionResult.ElectedGains[electedAccountId].tips;
			}
		}

		protected override void FillWalletTransactionHistoryEntry(IWalletTransactionHistory walletAccountTransactionHistory, ITransaction transaction, AccountId targetAccountId, string note) {
			this.EnsureWalletLoaded();
			base.FillWalletTransactionHistoryEntry(walletAccountTransactionHistory, transaction, targetAccountId, note);

			if(walletAccountTransactionHistory is INeuraliumWalletTransactionHistory neuraliumWalletTransactionHistory) {

				bool ours = transaction.TransactionId.Account == targetAccountId;

				//here we record the impact amount. + value increases our amount. - reduces
				if(transaction is INeuraliumTransferTransaction neuraliumTransferTransaction) {

					neuraliumWalletTransactionHistory.Amount = neuraliumTransferTransaction.Amount;
					neuraliumWalletTransactionHistory.MoneratyTransactionType = NeuraliumWalletTransactionHistory.MoneratyTransactionTypes.Debit;
					neuraliumWalletTransactionHistory.Recipient = neuraliumTransferTransaction.Recipient.ToString();
				} else if(transaction is INeuraliumMultiTransferTransaction neuraliumMultiTransferTransaction) {

					if(ours) {
						neuraliumWalletTransactionHistory.Amount = neuraliumMultiTransferTransaction.Total;

					} else {
						neuraliumWalletTransactionHistory.Amount = neuraliumMultiTransferTransaction.Recipients.SingleOrDefault(r => r.Recipient == targetAccountId).Amount;
					}

					neuraliumWalletTransactionHistory.MoneratyTransactionType = NeuraliumWalletTransactionHistory.MoneratyTransactionTypes.Debit;
					neuraliumWalletTransactionHistory.Recipient = string.Join(",", neuraliumMultiTransferTransaction.Recipients.Select(a => a.Recipient).OrderBy(a => a.ToLongRepresentation()));
				} else if(transaction is INeuraliumRefillNeuraliumsTransaction neuraliumsTransaction) {
					if(ours) {
						neuraliumWalletTransactionHistory.Amount = 1000;
						neuraliumWalletTransactionHistory.MoneratyTransactionType = NeuraliumWalletTransactionHistory.MoneratyTransactionTypes.Credit;
						neuraliumWalletTransactionHistory.Recipient = neuraliumsTransaction.TransactionId.Account.ToString();
					}
				}

				if(transaction is ITipTransaction tipTransaction) {
					neuraliumWalletTransactionHistory.Tip = tipTransaction.Tip;
				}
			}
		}

		protected override void FillWalletTransactionCacheEntry(IWalletTransactionCache walletAccountTransactionCache, ITransactionEnvelope transactionEnvelope, AccountId targetAccountId) {
			this.EnsureWalletLoaded();
			base.FillWalletTransactionCacheEntry(walletAccountTransactionCache, transactionEnvelope, targetAccountId);

			ITransaction transaction = transactionEnvelope.Contents.RehydratedTransaction;

			if(walletAccountTransactionCache is INeuraliumWalletTransactionCache neuraliumWalletTransactionCache) {

				bool ours = transaction.TransactionId.Account == targetAccountId;

				//here we record the impact amount. + value increases our amount. - reduces
				if(transaction is INeuraliumTransferTransaction neuraliumTransferTransaction) {
					neuraliumWalletTransactionCache.Amount = neuraliumTransferTransaction.Amount;
					neuraliumWalletTransactionCache.MoneratyTransactionType = NeuraliumWalletTransactionCache.MoneratyTransactionTypes.Debit;
				} else if(transaction is INeuraliumMultiTransferTransaction neuraliumMultiTransferTransaction) {

					neuraliumWalletTransactionCache.Amount = neuraliumMultiTransferTransaction.Total;
					neuraliumWalletTransactionCache.MoneratyTransactionType = NeuraliumWalletTransactionCache.MoneratyTransactionTypes.Debit;
				} else if(transaction is INeuraliumRefillNeuraliumsTransaction neuraliumsTransaction) {

					neuraliumWalletTransactionCache.Amount = 1000;
					neuraliumWalletTransactionCache.MoneratyTransactionType = NeuraliumWalletTransactionCache.MoneratyTransactionTypes.Credit;
				}

				if(transaction is ITipTransaction tipTransaction) {
					neuraliumWalletTransactionCache.Tip = tipTransaction.Tip;
				}
			}
		}

		protected override void FillStandardAccountSnapshot(IWalletAccount account, IWalletStandardAccountSnapshot accountSnapshot) {
			base.FillStandardAccountSnapshot(account, accountSnapshot);

			// anything else?
		}

		protected override void FillJointAccountSnapshot(IWalletAccount account, IWalletJointAccountSnapshot accountSnapshot) {
			base.FillJointAccountSnapshot(account, accountSnapshot);

			// anything else?
		}

		public new INeuraliumWalletAccount GetActiveAccount() {
			return (INeuraliumWalletAccount) base.GetActiveAccount();
		}

		protected override void PrepareAccountInfos(IAccountFileInfo accountFileInfo) {
			this.EnsureWalletLoaded();

			base.PrepareAccountInfos(accountFileInfo);

			if(accountFileInfo is INeuraliumAccountFileInfo neuraliumAccountFileInfo) {
				neuraliumAccountFileInfo.WalletTimelineFileInfo.CreateEmptyFile();
			}

		}

		protected override void CreateNewAccountInfoContents(IAccountFileInfo accountFileInfo, IWalletAccount account) {
			this.EnsureWalletLoaded();

			base.CreateNewAccountInfoContents(accountFileInfo, account);

			if(accountFileInfo is INeuraliumAccountFileInfo neuraliumAccountFileInfo) {
				neuraliumAccountFileInfo.WalletTimelineFileInfo = this.SerialisationFal.CreateNeuraliumWalletTimelineFileInfo(account, this.WalletFileInfo.WalletSecurityDetails);
			}
		}

	#region external API methods

		public TimelineHeader GetTimelineHeader(Guid accountUuid) {
			this.EnsureWalletLoaded();

			if(accountUuid == Guid.Empty) {
				accountUuid = this.GetAccountUuid();
			}

			TimelineHeader timelineHeader = new TimelineHeader();

			//TODO: merge correctly with base version of this method
			if(this.WalletFileInfo.Accounts[accountUuid] is INeuraliumAccountFileInfo neuraliumAccountFileInfo) {

				timelineHeader.FirstDay = neuraliumAccountFileInfo.WalletTimelineFileInfo.GetFirstDay();
				timelineHeader.NumberOfDays = neuraliumAccountFileInfo.WalletTimelineFileInfo.GetDaysCount();
			}

			return timelineHeader;

		}

		public List<TimelineDay> GetTimelineSection(Guid accountUuid, DateTime firstday, int skip = 0, int take = 1) {
			this.EnsureWalletLoaded();

			if(accountUuid == Guid.Empty) {
				accountUuid = this.GetAccountUuid();
			}

			var results = new List<TimelineDay>();

			if(this.WalletFileInfo.Accounts[accountUuid] is INeuraliumAccountFileInfo neuraliumAccountFileInfo) {
				results.AddRange(neuraliumAccountFileInfo.WalletTimelineFileInfo.RunQuery<TimelineDay, NeuraliumWalletTimelineDay>(d => d.Where(t => t.Timestamp <= firstday).OrderByDescending(t => t.Timestamp).Skip(skip).Take(take).Select(e => new TimelineDay {Day = e.Timestamp, EndingTotal = e.Total, Id = e.Id}).ToList()));

				var dayIds = results.Select(d => d.Id).ToList();

				var dayEntries = neuraliumAccountFileInfo.WalletTimelineFileInfo.RunQuery<TimelineDay.TimelineEntry, NeuraliumWalletTimeline>(d => d.Where(e => dayIds.Contains(e.DayId)).Select(e => new TimelineDay.TimelineEntry {
					Timestamp = e.Timestamp, SenderAccountId = e.SenderAccountId?.ToString() ?? "", RecipientAccountId = e.RecipientAccountId?.ToString() ?? "", Amount = e.Amount,
					Tips = e.Tips, Total = e.Total, Direction = (byte) e.Direction, CreditType = (byte) e.CreditType,
					Confirmed = e.Confirmed, DayId = e.DayId, TransactionId = e.TransactionId ?? ""
				}).OrderByDescending(e => e.Timestamp).ToList());

				foreach(TimelineDay day in results) {
					day.Entries.AddRange(dayEntries.Where(e => e.DayId == day.Id));
				}
			}

			return results;

		}

	#endregion

	#region wallet manager

		public override SynthesizedBlock ConvertApiSynthesizedBlock(SynthesizedBlockAPI synthesizedBlockApi) {
			SynthesizedBlock synthesizedBlock = base.ConvertApiSynthesizedBlock(synthesizedBlockApi);

			AccountId accountId = synthesizedBlockApi.AccountId != null ? new AccountId(synthesizedBlockApi.AccountId) : new AccountId(synthesizedBlockApi.AccountHash);

			if(synthesizedBlockApi is NeuraliumSynthesizedBlockApi neuraliumSynthesizedBlockApi && synthesizedBlock is NeuraliumSynthesizedBlock neuraliumSynthesizedBlock) {

				foreach(NeuraliumSynthesizedBlockApi.NeuraliumSynthesizedElectionResultAPI electionResult in neuraliumSynthesizedBlockApi.FinalElectionResults) {

					NeuraliumSynthesizedBlock.NeuraliumSynthesizedElectionResult neuraliumSynthesizedElectionResult = new NeuraliumSynthesizedBlock.NeuraliumSynthesizedElectionResult();

					neuraliumSynthesizedElectionResult.BlockId = synthesizedBlockApi.BlockId;
					neuraliumSynthesizedElectionResult.Timestamp = synthesizedBlockApi.Timestamp;

					AccountId delegateAccountId = null;

					if(!string.IsNullOrWhiteSpace(electionResult.DelegateAccountId)) {
						delegateAccountId = new AccountId(electionResult.DelegateAccountId);
					}

					neuraliumSynthesizedElectionResult.ElectedAccounts.Add(accountId, (accountId, delegateAccountId, (Enums.ElectedPeerShareTypes) electionResult.PeerType, electionResult.SelectedTransactions));
					neuraliumSynthesizedElectionResult.BlockId = synthesizedBlockApi.BlockId - electionResult.Offset;
					neuraliumSynthesizedElectionResult.ElectedGains.Add(accountId, (electionResult.BountyShare, electionResult.Tips));

					neuraliumSynthesizedBlock.FinalElectionResults.Add(neuraliumSynthesizedElectionResult);
				}
			}

			return synthesizedBlock;
		}

		protected override SynthesizedBlock CreateSynthesizedBlockFromApi(SynthesizedBlockAPI synthesizedBlockApi) {
			return new NeuraliumSynthesizedBlock();
		}

		public override SynthesizedBlockAPI DeserializeSynthesizedBlockAPI(string synthesizedBlock) {
			return JsonConvert.DeserializeObject<NeuraliumSynthesizedBlockApi>(synthesizedBlock);
		}

	#endregion

	}
}