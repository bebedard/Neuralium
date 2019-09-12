using System;
using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Wallet;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core.Cryptography.Passphrases;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Wallet {

	public class NeuraliumWalletTransactionCacheFileInfo : WalletTransactionCacheFileInfo<NeuraliumWalletTransactionCache> {

		public NeuraliumWalletTransactionCacheFileInfo(IWalletAccount account, string filename, BlockchainServiceSet serviceSet, IWalletSerialisationFal serialisationFal, WalletPassphraseDetails walletSecurityDetails) : base(account, filename, serviceSet, serialisationFal, walletSecurityDetails) {
		}

		/// <summary>
		///     take the sum of all amounts dn tips currently locked in unconfirmed transactions in our cache
		/// </summary>
		/// <returns></returns>
		public (decimal debit, decimal credit, decimal tip) GetTransactionAmounts() {
			lock(this.locker) {
				bool collectionExists = false;

				var debits = this.RunQueryDbOperation(litedbDal => {
					collectionExists = litedbDal.CollectionExists<NeuraliumWalletTransactionCache>();

					if(collectionExists) {
						return litedbDal.Get<NeuraliumWalletTransactionCache, Tuple<decimal, decimal>>(t => t.MoneratyTransactionType == NeuraliumWalletTransactionCache.MoneratyTransactionTypes.Debit, t => new Tuple<decimal, decimal>(t.Amount, t.Tip));
					}

					return default;
				});

				var credits = this.RunQueryDbOperation(litedbDal => {
					if(collectionExists) {
						return litedbDal.Get<NeuraliumWalletTransactionCache, Tuple<decimal, decimal>>(t => t.MoneratyTransactionType == NeuraliumWalletTransactionCache.MoneratyTransactionTypes.Credit, t => new Tuple<decimal, decimal>(t.Amount, t.Tip));
					}

					return default;
				});

				decimal debit = 0;
				decimal credit = 0;
				decimal tip = 0;

				if(debits?.Any() ?? false) {

					debit = debits.Sum(e => e.Item1);
					tip = debits.Sum(e => e.Item2);
				}

				if(credits?.Any() ?? false) {

					credit = credits.Sum(e => e.Item1);
					tip += credits.Sum(e => e.Item2);
				}

				return (debit, credit, tip);
			}
		}
	}
}