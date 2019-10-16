using System;
using System.Collections.Generic;
using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Results.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.Structures;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1.SAFU;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1.Security;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Results.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.General.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.Moderator.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Processors.TransactionInterpretation.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Tools;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.General.Types;
using Serilog;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Processors.TransactionInterpretation.V1 {

	public class NeuraliumTransactionInterpretationProcessor<ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT> : TransactionInterpretationProcessor<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider, ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT>
		where ACCOUNT_SNAPSHOT : INeuraliumAccountSnapshot
		where STANDARD_ACCOUNT_SNAPSHOT : class, INeuraliumStandardAccountSnapshot<STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where STANDARD_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeature, new()
		where STANDARD_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreeze, new()
		where JOINT_ACCOUNT_SNAPSHOT : class, INeuraliumJointAccountSnapshot<JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where JOINT_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeature, new()
		where JOINT_ACCOUNT_MEMBERS_SNAPSHOT : class, INeuraliumJointMemberAccount, new()
		where JOINT_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreeze, new()
		where STANDARD_ACCOUNT_KEY_SNAPSHOT : class, INeuraliumStandardAccountKeysSnapshot, new()
		where ACCREDITATION_CERTIFICATE_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshot<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>, new()
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshotAccount, new()
		where CHAIN_OPTIONS_SNAPSHOT : class, INeuraliumChainOptionsSnapshot, new() {

		public NeuraliumTransactionInterpretationProcessor(INeuraliumCentralCoordinator centralCoordinator) : this(centralCoordinator, TransactionImpactSet.OperationModes.Real) {

		}

		public NeuraliumTransactionInterpretationProcessor(INeuraliumCentralCoordinator centralCoordinator, TransactionImpactSet.OperationModes operationMode) : base(centralCoordinator, operationMode) {
			this.snapshotCacheSet = new NeuraliumSnapshotCacheSet<ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT>(NeuraliumCardsUtils.Instance);
		}

		protected override void RegisterTransactionImpactSets() {
			base.RegisterTransactionImpactSets();

			this.RegisterTransactionImpactSetOverride(new SupersetTransactionImpactSet<INeuraliumStandardPresentationTransaction, IStandardPresentationTransaction>());
			this.RegisterTransactionImpactSetOverride(new SupersetTransactionImpactSet<INeuraliumJointPresentationTransaction, IJointPresentationTransaction>());

			this.RegisterTransactionImpactSetOverride(new SupersetTransactionImpactSet<INeuraliumChainAccreditationCertificateTransaction, IChainAccreditationCertificateTransaction> {
				InterpretTransactionAccreditationCertificatesFunc = (t, blockId, snapshotCache, operationMode) => {

					ACCREDITATION_CERTIFICATE_SNAPSHOT certificate = snapshotCache.GetAccreditationCertificateSnapshotModify((int) t.CertificateId.Value);

					certificate.ProviderBountyshare = t.ProviderBountyshare;
					certificate.InfrastructureServiceFees = t.InfrastructureServiceFees;
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliumTransferTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedKeysSnapshots) => {

					affectedKeysSnapshots.AddAccountId(t.TransactionId.Account);
					affectedKeysSnapshots.AddAccountId(t.Recipient);

				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					INeuraliumAccountSnapshot senderAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(t.TransactionId.Account);
					INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(t.Recipient);

					if(senderAccount != null) {
						senderAccount.Balance -= t.Amount + t.Tip;
					}

					if(recipientAccount != null) {
						recipientAccount.Balance += t.Amount;
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliumMultiTransferTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedSnapshots) => {

					affectedSnapshots.AddAccountId(t.TransactionId.Account);
					affectedSnapshots.AddAccounts(t.Recipients.Select(r => r.Recipient).ToList());

				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					INeuraliumAccountSnapshot senderAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(t.TransactionId.Account);

					if(senderAccount != null) {
						senderAccount.Balance -= t.Total + t.Tip;
					}

					foreach(RecipientSet recipientSet in t.Recipients) {
						INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(recipientSet.Recipient);

						if(recipientAccount != null) {
							recipientAccount.Balance += recipientSet.Amount;
						}
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<IEmittingTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedKeysSnapshots) => {

					affectedKeysSnapshots.AddAccountId(t.Recipient);
				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(t.Recipient);

					if(recipientAccount != null) {
						recipientAccount.Balance += t.Amount;
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<IMultiEmittingTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedKeysSnapshots) => {

					affectedKeysSnapshots.AddAccounts(t.Recipients.Select(r => r.Recipient).ToList());
				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					foreach(RecipientSet recipientSet in t.Recipients) {
						INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(recipientSet.Recipient);

						if(recipientAccount != null) {
							recipientAccount.Balance += recipientSet.Amount;
						}
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<IDestroyNeuraliumsTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedKeysSnapshots) => {

					affectedKeysSnapshots.AddAccountId(new AccountId(NeuraliumConstants.DEFAULT_MODERATOR_DESTRUCTION_ACCOUNT_ID, Enums.AccountTypes.Standard));
				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					INeuraliumAccountSnapshot deflationAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(new AccountId(NeuraliumConstants.DEFAULT_MODERATOR_DESTRUCTION_ACCOUNT_ID, Enums.AccountTypes.Standard));

					if(deflationAccount != null) {
						// lets eliminate it all, we are deflating the neuralium
						deflationAccount.Balance = 0;
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliumSAFUContributionTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedSnapshots) => {

					affectedSnapshots.AddAccountId(new AccountId(NeuraliumConstants.DEFAULT_NEURALIUM_SAFU_ACCOUNT_ID, Enums.AccountTypes.Standard));
					affectedSnapshots.AddAccountId(t.TransactionId.Account);

				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					if(t.AcceptSAFUTermsOfService == false) {
						return;
					}

					// ensure the safu certificate is valid
					ACCREDITATION_CERTIFICATE_SNAPSHOT safuCertificate = snapshotCache.GetAccreditationCertificateSnapshotReadonly(NeuraliumConstants.SAFU_ACCREDITATION_CERTIFICATE_ID);

					if(AccreditationCertificateUtils.IsValid(safuCertificate) == false) {
						return;
					}

					INeuraliumAccountSnapshot senderAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(t.TransactionId.Account);

					if(senderAccount == null) {
						return;
					}

					senderAccount.Balance -= t.Total + t.Tip;

					senderAccount.CreateNewCollectionEntry(out IAccountFeature feature);

					feature.Start = t.Start;

					CHAIN_OPTIONS_SNAPSHOT chainOptionsSnapshot = snapshotCache.GetChainOptionsSnapshotReadonly(1);

					// lets determine the participation range

					decimal divider = chainOptionsSnapshot.SAFUDailyRatio * t.DailyProtection;

					if(divider <= 0) {
						return;
					}

					int days = (int) Math.Floor(t.Total / divider);

					TimeSpan range = TimeSpan.FromDays(days);

					DateTime transactionStart = this.centralCoordinator.BlockchainServiceSet.TimeService.GetTimestampDateTime(t.TransactionId.Timestamp.Value, this.centralCoordinator.ChainComponentProvider.ChainStateProviderBase.ChainInception);
					feature.Start = t.Start;

					if((feature.Start == null) || (feature.Start < transactionStart)) {
						feature.Start = transactionStart;
					}

					feature.End = feature.Start + range;

					senderAccount.AddCollectionEntry(feature);

					INeuraliumAccountSnapshot safuAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(new AccountId(NeuraliumConstants.DEFAULT_NEURALIUM_SAFU_ACCOUNT_ID, Enums.AccountTypes.Standard));

					if(safuAccount != null) {
						safuAccount.Balance += t.Total;
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliumSAFUTransferTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedSnapshots) => {

					affectedSnapshots.AddAccountId(new AccountId(NeuraliumConstants.DEFAULT_NEURALIUM_SAFU_ACCOUNT_ID, Enums.AccountTypes.Standard));
					affectedSnapshots.AddAccounts(t.Recipients.Select(r => r.Recipient).ToList());

				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					AccountId safuAccountId = new AccountId(NeuraliumConstants.DEFAULT_NEURALIUM_SAFU_ACCOUNT_ID, Enums.AccountTypes.Standard);

					if(t.TransactionId.Account != safuAccountId) {
						return;
					}

					decimal total = 0;

					foreach(RecipientSet recipientSet in t.Recipients) {
						INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(recipientSet.Recipient);

						if(recipientAccount != null) {
							recipientAccount.Balance += recipientSet.Amount;
							total += recipientSet.Amount;
						}
					}

					INeuraliumAccountSnapshot senderAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(safuAccountId);
					senderAccount.Balance -= total;
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliuFreezeSuspiciousFundsTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedSnapshots) => {

					affectedSnapshots.AddAccounts(t.Accounts);

				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					foreach((AccountId accountId, decimal amount) recipientSet in t.GetFlatImpactTree()) {
						INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(recipientSet.accountId);

						recipientAccount.CreateNewCollectionEntry(out INeuraliumAccountFreeze freeze);

						freeze.FreezeId = t.FreezeId;
						freeze.Amount = recipientSet.amount;

						recipientAccount.AddCollectionEntry(freeze);
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliuUnfreezeClearedFundsTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedSnapshots) => {

					affectedSnapshots.AddAccounts(t.Accounts.Select(a => a.AccountId).ToList());

				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					foreach(NeuraliuUnfreezeClearedFundsTransaction.AccountUnfreeze recipientSet in t.Accounts) {
						INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(recipientSet.AccountId);

						recipientAccount.RemoveCollectionEntry(entry => entry.FreezeId == t.FreezeId);
					}
				}
			});

			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliumUnwindStolenFundsTreeTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedSnapshots) => {

					affectedSnapshots.AddAccounts(t.AccountRestoreImpacts.Select(a => a.AccountId).ToList());
					affectedSnapshots.AddAccounts(t.AccountUnwindImpacts.Select(a => a.AccountId).ToList());

				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					// ok, lets unwind the ones that have funds they should not have
					foreach(NeuraliumUnwindStolenFundsTreeTransaction.AccountUnwindImpact recipientSet in t.AccountUnwindImpacts) {
						INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(recipientSet.AccountId);

						recipientAccount.RemoveCollectionEntry(entry => entry.FreezeId == t.FreezeId);

						recipientAccount.Balance -= recipientSet.UnwoundAmount;

						if(recipientAccount.Balance < 0) {
							recipientAccount.Balance = 0;
						}
					}

					// ok, lets refund the ones that were wrong
					foreach(NeuraliumUnwindStolenFundsTreeTransaction.AccountRestoreImpact recipientSet in t.AccountRestoreImpacts) {
						INeuraliumAccountSnapshot recipientAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(recipientSet.AccountId);

						recipientAccount.RemoveCollectionEntry(entry => entry.FreezeId == t.FreezeId);

						recipientAccount.Balance += recipientSet.RestoreAmount;
					}
				}
			});

#if TESTNET || DEVNET
			this.RegisterTransactionImpactSet(new TransactionImpactSet<INeuraliumRefillNeuraliumsTransaction> {
				GetImpactedSnapshotsFunc = (t, affectedKeysSnapshots) => {

					affectedKeysSnapshots.AddAccountId(t.TransactionId.Account);
				},
				InterpretTransactionAccountsFunc = (t, blockId, snapshotCache, mode) => {

					INeuraliumAccountSnapshot senderAccount = snapshotCache.GetAccountSnapshotModify<INeuraliumAccountSnapshot>(t.TransactionId.Account);

					if(senderAccount != null) {
						senderAccount.Balance += 1000;
					}

				}
			});
		}
#endif

		public override void ApplyBlockElectionsInfluence(List<IFinalElectionResults> publicationResult, Dictionary<TransactionId, ITransaction> transactions) {
			base.ApplyBlockElectionsInfluence(publicationResult, transactions);

			// now apply the network service fees if applicable

			var neuraliumElectionResult = publicationResult.OfType<INeuraliumFinalElectionResults>().ToList();

			if(neuraliumElectionResult.Any(r => r.InfrastructureServiceFees?.Value != 0)) {

				this.SetNetworkServiceFees(neuraliumElectionResult.Where(r => (r.InfrastructureServiceFees != null) && (r.InfrastructureServiceFees.Value != 0)).Select(r => r.InfrastructureServiceFees.Value).ToList());
			}
		}

		public override void ApplyBlockElectionsInfluence(List<SynthesizedBlock.SynthesizedElectionResult> finalElectionResults, Dictionary<TransactionId, ITransaction> transactions) {
			base.ApplyBlockElectionsInfluence(finalElectionResults, transactions);

			// now apply the network service fees if applicable

			var neuraliumElectionResult = finalElectionResults.OfType<NeuraliumSynthesizedBlock.NeuraliumSynthesizedElectionResult>().ToList();

			if(neuraliumElectionResult.Any(r => r.InfrastructureServiceFees != 0)) {

				this.SetNetworkServiceFees(neuraliumElectionResult.Where(r => r.InfrastructureServiceFees != 0).Select(r => r.InfrastructureServiceFees).ToList());
			}
		}

		private void SetNetworkServiceFees(List<decimal> networkServiceFees) {
			AccountId networkServiceFeesAccount = new AccountId(NeuraliumConstants.DEFAULT_NETWORK_MAINTENANCE_SERVICE_FEES_ACCOUNT_ID, Enums.AccountTypes.Standard);

			if(this.IsAnyAccountTracked(new[] {networkServiceFeesAccount}.ToList())) {

				SnapshotKeySet impactedSnapshotKeys = new SnapshotKeySet();

				impactedSnapshotKeys.AddAccountId(networkServiceFeesAccount);

				// now, we can query the snapshots we will need
				this.snapshotCacheSet.EnsureSnapshots(impactedSnapshotKeys);

				STANDARD_ACCOUNT_SNAPSHOT serviceFeesAccount = this.snapshotCacheSet.GetStandardAccountSnapshotModify(networkServiceFeesAccount);

				if(serviceFeesAccount == null) {
					serviceFeesAccount = this.snapshotCacheSet.CreateNewStandardAccountSnapshot(networkServiceFeesAccount, null);
				}

				if(networkServiceFees.Any()) {
					serviceFeesAccount.Balance += networkServiceFees.Sum();
				}
			}
		}

		protected override void ApplyDelegateResultsToSnapshot(ACCOUNT_SNAPSHOT snapshot, IDelegateResults delegateResults, Dictionary<TransactionId, ITransaction> transactions) {
			base.ApplyDelegateResultsToSnapshot(snapshot, delegateResults, transactions);

			INeuraliumDelegateResults results = (INeuraliumDelegateResults) delegateResults;

			snapshot.Balance += results.BountyShare;
		}

		protected override void ApplyElectedResultsToSnapshot(ACCOUNT_SNAPSHOT snapshot, IElectedResults electedResults, Dictionary<TransactionId, ITransaction> transactions) {
			base.ApplyElectedResultsToSnapshot(snapshot, electedResults, transactions);

			INeuraliumElectedResults results = (INeuraliumElectedResults) electedResults;

			snapshot.Balance += results.BountyShare;

			// now we apply transaction fees
			foreach(TransactionId transaction in results.Transactions) {

				if(!transactions.ContainsKey(transaction)) {
					// this is a serious issue, obviously.  lets prevent it from crashing, but really, its a big deal
					Log.Error($"Transaction was not found for TID {transaction} while applying elected result tips.");

					continue;
				}

				if(transactions[transaction] is ITipTransaction tipTransaction) {
					snapshot.Balance += tipTransaction.Tip;
				}
			}

		}

		protected override void ApplyDelegateResultsToSnapshot(ACCOUNT_SNAPSHOT snapshot, AccountId accountId, SynthesizedBlock.SynthesizedElectionResult synthesizedElectionResult, Dictionary<TransactionId, ITransaction> transactions) {
			base.ApplyDelegateResultsToSnapshot(snapshot, accountId, synthesizedElectionResult, transactions);

			if(synthesizedElectionResult is NeuraliumSynthesizedBlock.NeuraliumSynthesizedElectionResult neuraliumSynthesizedElectionResult) {
				snapshot.Balance += neuraliumSynthesizedElectionResult.DelegateBounties[accountId];
			}
		}

		protected override void ApplyElectedResultsToSnapshot(ACCOUNT_SNAPSHOT snapshot, AccountId accountId, SynthesizedBlock.SynthesizedElectionResult synthesizedElectionResult, Dictionary<TransactionId, ITransaction> transactions) {
			base.ApplyElectedResultsToSnapshot(snapshot, accountId, synthesizedElectionResult, transactions);

			if(synthesizedElectionResult is NeuraliumSynthesizedBlock.NeuraliumSynthesizedElectionResult neuraliumSynthesizedElectionResult) {

				(decimal bountyShare, decimal tips) electedEntry = neuraliumSynthesizedElectionResult.ElectedGains[accountId];
				decimal gain = electedEntry.bountyShare + electedEntry.tips;
				snapshot.Balance += gain;

				Log.Information($"We were elected in the block! We were allocated {electedEntry.bountyShare}N in bounty, {electedEntry.tips}N in tips for a total gain of {gain}N. Our new total is {snapshot.Balance}N");
			}
		}
	}
}