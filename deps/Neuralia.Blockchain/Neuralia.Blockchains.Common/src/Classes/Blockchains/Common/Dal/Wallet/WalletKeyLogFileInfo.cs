using System;
using System.Linq;
using LiteDB;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.Cryptography.Passphrases;
using Neuralia.Blockchains.Core.DataAccess.Dal;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Wallet {
	public class WalletKeyLogFileInfo : SingleEntryWalletFileInfo<WalletAccountKeyLog> {

		private readonly IWalletAccount account;

		public WalletKeyLogFileInfo(IWalletAccount account, string filename, BlockchainServiceSet serviceSet, IWalletSerialisationFal serialisationFal, WalletPassphraseDetails walletSecurityDetails) : base(filename, serviceSet, serialisationFal, walletSecurityDetails) {
			this.account = account;

		}

		/// <summary>
		///     Insert the new empty wallet
		/// </summary>
		/// <param name="wallet"></param>
		protected override void InsertNewDbData(WalletAccountKeyLog keyLog) {

			this.RunDbOperation(litedbDal => {
				litedbDal.Insert(keyLog, c => c.Id);
			});

		}

		protected override void CreateDbFile(LiteDBDAL litedbDal) {
			litedbDal.CreateDbFile<WalletAccountKeyLog, ObjectId>(i => i.Id);
		}

		protected override void PrepareEncryptionInfo() {
			this.CreateSecurityDetails();
		}

		protected override void CreateSecurityDetails() {
			lock(this.locker) {
				if(this.EncryptionInfo == null) {
					this.EncryptionInfo = new EncryptionInfo();

					this.EncryptionInfo.encrypt = this.WalletSecurityDetails.EncryptWallet;

					if(this.EncryptionInfo.encrypt) {

						this.EncryptionInfo.encryptionParameters = this.account.KeyLogFileEncryptionParameters;
						this.EncryptionInfo.Secret = () => this.account.KeyLogFileSecret;
					}
				}
			}
		}

		protected override void UpdateDbEntry() {
			// do nothing, we dont udpate

		}

		public void InsertKeyLogEntry(WalletAccountKeyLog walletAccountKeyLog) {
			lock(this.locker) {
				this.RunDbOperation(litedbDal => {
					// lets check the last one inserted, make sure it was a lower key height than ours now

					// -1 is for keys without an index. otherwise we check it
					if(walletAccountKeyLog.KeyUseIndex?.KeyUseIndex.Value != -1) {
						WalletAccountKeyLog last = null;

						if(litedbDal.CollectionExists<WalletAccountKeyLog>()) {
							last = litedbDal.Get<WalletAccountKeyLog>(k => k.KeyOrdinalId.Equals(walletAccountKeyLog.KeyOrdinalId)).OrderByDescending(k => k.Timestamp).FirstOrDefault();
						}

						if(last?.KeyUseIndex != null) {

							if(last.KeyUseIndex > walletAccountKeyLog.KeyUseIndex) {
								throw new ApplicationException("You are using a key height that is inferior to one previously used. Your wallet could be an older version.");
							}
						}
					}

					litedbDal.Insert(walletAccountKeyLog, k => k.Id);
				});

				this.Save();
			}
		}

		public bool ConfirmKeyLogBlockEntry(long confirmationBlockId) {
			return this.ConfirmKeyLogEntry(confirmationBlockId.ToString(), Enums.BlockchainEventTypes.Block, confirmationBlockId, null);
		}

		public bool ConfirmKeyLogTransactionEntry(TransactionIdExtended transactionId, long confirmationBlockId) {
			return this.ConfirmKeyLogEntry(transactionId.ToEssentialString(), Enums.BlockchainEventTypes.Transaction, confirmationBlockId, transactionId.KeyUseIndex);
		}

		public bool ConfirmKeyLogEntry(string eventId, Enums.BlockchainEventTypes eventType, long confirmationBlockId, KeyUseIndexSet keyUseIndexSet) {
			lock(this.locker) {
				bool result = this.RunDbOperation(litedbDal => {
					// lets check the last one inserted, make sure it was a lower key height than ours now
					WalletAccountKeyLog entry = null;

					if(litedbDal.CollectionExists<WalletAccountKeyLog>()) {
						entry = litedbDal.Get<WalletAccountKeyLog>(k => (k.EventId == eventId) && (k.EventType == (byte) eventType)).FirstOrDefault();
					}

					if(entry == null) {
						return false;
					}

					if((keyUseIndexSet != null) && (entry.KeyUseIndex != keyUseIndexSet)) {
						throw new ApplicationException($"Failed to confirm keylog entry for event '{eventId}'. Expected key use index to be '{keyUseIndexSet}' but found '{entry.KeyUseIndex}' instead.");
					}

					entry.ConfirmationBlockId = confirmationBlockId;

					return litedbDal.Update(entry);
				});

				if(result) {
					this.Save();
				}

				return result;
			}
		}

		public bool KeyLogBlockExists(BlockId blockId) {

			return this.KeyLogEntryExists(blockId.ToString(), Enums.BlockchainEventTypes.Block);
		}

		public bool KeyLogTransactionExists(TransactionId transactionId) {

			return this.KeyLogEntryExists(transactionId.ToString(), Enums.BlockchainEventTypes.Transaction);
		}

		public bool KeyLogEntryExists(string eventId, Enums.BlockchainEventTypes eventType) {

			return this.RunQueryDbOperation(litedbDal => {
				if(!litedbDal.CollectionExists<WalletAccountKeyLog>()) {
					return false;
				}

				return litedbDal.Exists<WalletAccountKeyLog>(k => (k.EventId == eventId) && (k.EventType == (byte) eventType));
			});
		}

		public bool KeyLogBlockIsConfirmed(BlockId blockId) {

			return this.KeyLogEntryIsConfirmed(blockId.ToString(), Enums.BlockchainEventTypes.Block);
		}

		public bool KeyLogTransactionIsConfirmed(TransactionIdExtended transactionId) {

			return this.KeyLogEntryIsConfirmed(transactionId.ToEssentialString(), Enums.BlockchainEventTypes.Transaction);
		}

		public bool KeyLogEntryIsConfirmed(string eventId, Enums.BlockchainEventTypes eventType) {

			return this.RunQueryDbOperation(litedbDal => {
				WalletAccountKeyLog entry = null;

				if(litedbDal.CollectionExists<WalletAccountKeyLog>()) {
					entry = litedbDal.Get<WalletAccountKeyLog>(k => (k.EventId == eventId) && (k.EventType == (byte) eventType)).FirstOrDefault();
				}

				return entry?.ConfirmationBlockId != null;
			});
		}
	}
}