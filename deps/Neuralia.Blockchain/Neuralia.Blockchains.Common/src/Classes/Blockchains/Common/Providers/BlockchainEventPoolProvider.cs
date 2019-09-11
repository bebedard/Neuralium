using System.Collections.Generic;
using System.IO;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.ChainPool;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.Extensions;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers {

	public interface IEventPoolProvider {
		AppSettingsBase.TransactionPoolHandling TransactionPoolHandlingMode { get; }

		bool EventPoolEnabled { get; }
		bool SaveTransactionEnvelopes { get; }

		void InsertTransaction(ITransactionEnvelope transactionEnvelope);
		List<(ITransactionEnvelope envelope, TransactionId transactionId)> GetTransactions();
		List<TransactionId> GetTransactionIds();
		void DeleteTransactions(List<TransactionId> transactionIds);
		void DeleteExpiredTransactions();
	}

	public interface IEventPoolProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, CHAIN_POOL_DAL, CHAIN_POOL_CONTEXT, CHAIN_POOL_PUBLIC_TRANSACTIONS> : IEventPoolProvider
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_POOL_DAL : class, IChainPoolDal<CHAIN_POOL_PUBLIC_TRANSACTIONS>
		where CHAIN_POOL_CONTEXT : class, IChainPoolContext<CHAIN_POOL_PUBLIC_TRANSACTIONS>
		where CHAIN_POOL_PUBLIC_TRANSACTIONS : class, IChainPoolPublicTransactions<CHAIN_POOL_PUBLIC_TRANSACTIONS> {

		CHAIN_POOL_DAL ChainPoolDal { get; }
	}

	/// <summary>
	///     A provider that offers the chain state parameters from the DB
	/// </summary>
	/// <typeparam name="CHAIN_POOL_DAL"></typeparam>
	/// <typeparam name="CHAIN_POOL_CONTEXT"></typeparam>
	/// <typeparam name="CHAIN_POOL_ENTRY"></typeparam>
	public abstract class BlockchainEventPoolProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, CHAIN_POOL_DAL, CHAIN_POOL_CONTEXT, CHAIN_POOL_PUBLIC_TRANSACTIONS> : IEventPoolProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER, CHAIN_POOL_DAL, CHAIN_POOL_CONTEXT, CHAIN_POOL_PUBLIC_TRANSACTIONS>
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_POOL_DAL : class, IChainPoolDal<CHAIN_POOL_PUBLIC_TRANSACTIONS>
		where CHAIN_POOL_CONTEXT : class, IChainPoolContext<CHAIN_POOL_PUBLIC_TRANSACTIONS>
		where CHAIN_POOL_PUBLIC_TRANSACTIONS : class, IChainPoolPublicTransactions<CHAIN_POOL_PUBLIC_TRANSACTIONS> {

		public const string EVENT_POOL_PATH = "pool";
		public const string PUBLIC_POOL_PATH = "public";

		private readonly CENTRAL_COORDINATOR centralCoordinator;

		private readonly object locker = new object();

		protected readonly IChainMiningStatusProvider miningStatusProvider;

		private CHAIN_POOL_DAL chainPoolDal;

		public BlockchainEventPoolProvider(CENTRAL_COORDINATOR centralCoordinator, IChainMiningStatusProvider miningStatusProvider) {
			this.centralCoordinator = centralCoordinator;
			this.miningStatusProvider = miningStatusProvider;
		}

		protected string WalletDirectoryPath => this.centralCoordinator.ChainComponentProvider.WalletProviderBase.GetChainDirectoryPath();

		public CHAIN_POOL_DAL ChainPoolDal {
			get {
				lock(this.locker) {
					if(this.chainPoolDal == null) {
						this.chainPoolDal = this.centralCoordinator.ChainDalCreationFactory.CreateChainPoolDal<CHAIN_POOL_DAL>(this.centralCoordinator.ChainComponentProvider.WalletProviderBase.GetChainStorageFilesPath(), this.centralCoordinator.BlockchainServiceSet, this.centralCoordinator.ChainComponentProvider.ChainConfigurationProviderBase.ChainConfiguration.SerializationType);
					}
				}

				return this.chainPoolDal;
			}
		}

		public virtual void InsertTransaction(ITransactionEnvelope transactionEnvelope) {
			if(this.EventPoolEnabled) {
				this.ChainPoolDal.InsertTransactionEntry(transactionEnvelope, this.centralCoordinator.ChainComponentProvider.ChainStateProviderBase.ChainInception);

				if(this.SaveTransactionEnvelopes) {
					IByteArray envelope = transactionEnvelope.DehydrateEnvelope();

					string publicPath = this.GetPublicPath();

					if(!Directory.Exists(publicPath)) {
						//Log.Information($"Creating new wallet baseFolder in path: {this.chainWalletDirectoryPath}");
						Directory.CreateDirectory(publicPath);
					}

					// save it for future use
					FileExtensions.OpenWrite(Path.Combine(publicPath, transactionEnvelope.Contents.Uuid.ToString()), envelope);
				}
			}
		}

		public virtual List<TransactionId> GetTransactionIds() {
			if(!this.EventPoolEnabled) {
				return new List<TransactionId>(); // if disabled, we return nothing
			}

			return this.ChainPoolDal.GetTransactions();
		}

		public virtual List<(ITransactionEnvelope envelope, TransactionId transactionId)> GetTransactions() {

			if(!this.EventPoolEnabled) {
				return new List<(ITransactionEnvelope envelope, TransactionId transactionId)>(); // if disabled, we return nothing
			}

			var poolTransactions = this.GetTransactionIds();

			var results = new List<(ITransactionEnvelope envelope, TransactionId transactionId)>();
			string publicPath = this.GetPublicPath();

			foreach(TransactionId trx in poolTransactions) {
				string trxfile = Path.Combine(publicPath, trx.ToString());

				ITransactionEnvelope envelope = null;

				if(this.SaveTransactionEnvelopes && File.Exists(trxfile)) {
					IByteArray trxBytes = (ByteArray) File.ReadAllBytes(trxfile);

					envelope = this.centralCoordinator.ChainComponentProvider.ChainFactoryProviderBase.BlockchainEventsRehydrationFactoryBase.RehydrateEnvelope<ITransactionEnvelope>(trxBytes);
				}

				results.Add((envelope, trx));
			}

			return results;
		}

		public virtual void DeleteExpiredTransactions() {
			if(this.EventPoolEnabled) {
				this.ChainPoolDal.ClearExpiredTransactions();
			}
		}

		public virtual void DeleteTransactions(List<TransactionId> transactionIds) {
			if(this.EventPoolEnabled) {
				this.ChainPoolDal.RemoveTransactionEntries(transactionIds);

				this.DeleteTransactionEnvelopes(transactionIds);
			}
		}

		public AppSettingsBase.TransactionPoolHandling TransactionPoolHandlingMode => GlobalSettings.ApplicationSettings.TransactionPoolHandlingMode;

		/// <summary>
		///     do we save transactions into the event pool?
		/// </summary>
		/// <param name="isMining"></param>
		/// <returns></returns>
		public bool EventPoolEnabled => (this.TransactionPoolHandlingMode == AppSettingsBase.TransactionPoolHandling.AlwaysFull) || (this.TransactionPoolHandlingMode == AppSettingsBase.TransactionPoolHandling.AlwaysMetadata) || (this.miningStatusProvider.MiningEnabled && ((this.TransactionPoolHandlingMode == AppSettingsBase.TransactionPoolHandling.MiningMetadata) || (this.TransactionPoolHandlingMode == AppSettingsBase.TransactionPoolHandling.MiningFull)));

		/// <summary>
		///     Do we save entire envelope bodies on disk?
		/// </summary>
		/// <param name="isMining"></param>
		/// <returns></returns>
		public bool SaveTransactionEnvelopes => (this.TransactionPoolHandlingMode == AppSettingsBase.TransactionPoolHandling.AlwaysFull) || (this.miningStatusProvider.MiningEnabled && (this.TransactionPoolHandlingMode == AppSettingsBase.TransactionPoolHandling.MiningFull));

		protected string GetEventPoolPath() {
			return Path.Combine(this.WalletDirectoryPath, EVENT_POOL_PATH);
		}

		protected string GetPublicPath() {
			return Path.Combine(this.GetEventPoolPath(), PUBLIC_POOL_PATH);
		}

		protected void DeleteTransactionEnvelopes(List<TransactionId> transactionIds) {
			if(this.SaveTransactionEnvelopes) {

				string publicPath = this.GetPublicPath();

				foreach(TransactionId transaction in transactionIds) {
					string trxfile = Path.Combine(publicPath, transaction.ToString());

					if(File.Exists(trxfile)) {
						File.Delete(trxfile);
					}
				}
			}
		}
	}
}