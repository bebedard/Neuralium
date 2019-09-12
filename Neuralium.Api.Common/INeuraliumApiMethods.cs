using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neuralium.Api.Common {

	public interface INeuraliumApiMethods {

		Task ToggleServerMessages(bool enable);
		Task EnterWalletPassphrase(int correlationId, ushort chainType, int keyCorrelationCode, string passphrase);
		Task EnterKeyPassphrase(int correlationId, ushort chainType, int keyCorrelationCode, string passphrase);
		Task WalletKeyFileCopied(int correlationId, ushort chainType, int keyCorrelationCode);

		Task<object> QuerySystemInfo();
		Task<List<object>> QuerySupportedChains();
		Task<string> Ping();

		Task<bool> CompleteLongRunningEvent(int correlationId, object data);
		Task<bool> RenewLongRunningEvent(int correlationId);

		Task<bool> IsBlockchainSynced(ushort chainType);
		Task<bool> IsWalletSynced(ushort chainType);

		Task<bool> SyncBlockchain(ushort chainType, bool force);
		Task<bool> Shutdown();
		Task<object> BackupWallet(ushort chainType);

		Task<int> QueryTotalConnectedPeersCount();
		Task<object> QueryChainStatus(ushort chainType);
		Task<object> QueryBlockChainInfo(ushort chainType);
		Task<bool> IsWalletLoaded(ushort chainType);
		Task<bool> WalletExists(ushort chainType);
		Task<int> LoadWallet(ushort chainType);
		Task<long> QueryBlockHeight(ushort chainType);

		Task<string> QueryBlock(ushort chainType, long blockId);
		Task<byte[]> QueryCompressedBlock(ushort chainType, long blockId);
		Task<List<object>> QueryBlockBinaryTransactions(ushort chainType, long blockId);
		Task<int> CreateAccount(ushort chainType, string accountName, bool publishAccount, bool encryptKeys, bool encryptKeysIndividually, Dictionary<int, string> passphrases);
		Task<bool> SetActiveAccount(ushort chainType, Guid accountUuid);

		Task<int> CreateNewWallet(ushort chainType, string accountName, bool encryptWallet, bool encryptKey, bool encryptKeysIndividually, Dictionary<int, string> passphrases, bool publishAccount);

		Task<bool> SetWalletPassphrase(int correlationId, string passphrase);
		Task<bool> SetKeysPassphrase(int correlationId, string passphrase);

		Task<List<object>> QueryWalletTransactionHistory(ushort chainType, Guid accountUuid);
		Task<object> QueryWalletTransationHistoryDetails(ushort chainType, Guid accountUuid, string transactionId);
		Task<List<object>> QueryWalletAccounts(ushort chainType);
		Task<object> QueryWalletAccountDetails(ushort chainType, Guid accountUuid);
		Task<string> QueryWalletAccountPresentationTransactionId(ushort chainType, Guid accountUuid);

		Task<int> PublishAccount(ushort chainType, string accountUuId);
		Task StartMining(ushort chainType, string delegateAccountId);
		Task StopMining(ushort chainType);
		Task<bool> IsMiningEnabled(ushort chainType);
		Task<bool> IsMiningAllowed(ushort chainType);
		Task<bool> QueryBlockchainSynced(ushort chainType);
		Task<bool> QueryWalletSynced(ushort chainType);
		Task<object> QueryAccountTotalNeuraliums(Guid accountId);

		Task<bool> CreateNextXmssKey(ushort chainType, Guid accountUuid, byte ordinal);

		Task<int> SendNeuraliums(string targetAccountId, decimal amount, decimal fees, string note);
		Task<object> QueryNeuraliumTimelineHeader(Guid accountUuid);
		Task<List<object>> QueryNeuraliumTimelineSection(Guid accountUuid, DateTime firstday, int skip, int take);
		
		Task<byte[]> SignXmssMessage(ushort chainType, Guid accountUuid, byte[] message);

#if TESTNET || DEVNET
		Task<int> RefillNeuraliums(Guid accountUuid);
#endif

		Task<object> QueryElectionContext(ushort chainType, long blockId);
		Task<List<object>> QueryNeuraliumTransactionPool();

		Task<List<object>> QueryMiningHistory(ushort chainType);
	}
}