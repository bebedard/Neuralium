using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Blockchains.Neuralium.Classes;
using Blockchains.Neuralium.Classes.NeuraliumChain;
using Microsoft.AspNetCore.SignalR;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI.Wallet;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.Services;
using Neuralia.Blockchains.Tools.Cryptography;
using Neuralia.Blockchains.Tools.Data;
using Neuralium.Api.Common;
using Neuralium.Core.Classes.Services;
using Neuralium.Core.Controllers;
using Serilog;

namespace Neuralium.Core.Classes.General {

	public interface IRpcClientMethods {
	}

	public interface IRpcServerMethods : INeuraliumApiMethods {
	}

	public interface IRpcClientEvents {
		Task ReturnClientLongRunningEvent(int correlationId, int result, string error);
		Task LongRunningStatusUpdate(int correlationId, ushort eventId, byte eventType, object message);

		Task EnterWalletPassphrase(int correlationId, ushort chainType, int keyCorrelationCode, int attempt);
		Task EnterKeysPassphrase(int correlationId, ushort chainType, int keyCorrelationCode, Guid accountID, string keyname, int attempt);
		Task CopyWalletKeyFile(int correlationId, ushort chainType, int keyCorrelationCode, Guid accountID, string keyname, int attempt);

		Task AccountTotalUpdated(int correlationId, string accountId, object total);
		Task RequestCopyWallet(byte chainType, string message);
		Task PeerTotalUpdated(int total);

		Task BlockchainSyncStatusChanged(ushort chainType, Enums.ChainSyncState syncStatus);
		Task WalletSyncStatusChanged(ushort chainType, Enums.ChainSyncState syncStatus);

		Task MiningStatusChanged(ushort chainType, bool isMining);

		Task WalletCreationStarted(int correlationId);
		Task WalletCreationEnded(int correlationId);

		Task AccountCreationStarted(int correlationId);
		Task AccountCreationEnded(int correlationId);
		Task AccountCreationMessage(int correlationId, string message);
		Task AccountCreationStep(int correlationId, string stepName, int stepIndex, int stepTotal);
		Task AccountCreationError(int correlationId, string error);

		Task KeyGenerationStarted(int correlationId, string keyName, int keyIndex, int keyTotal);
		Task KeyGenerationEnded(int correlationId, string keyName, int keyIndex, int keyTotal);
		Task KeyGenerationMessage(int correlationId, string keyName, string message, int keyIndex, int keyTotal);
		Task KeyGenerationError(int correlationId, string keyName, string error);
		Task KeyGenerationPercentageUpdate(int correlationId, string keyName, int percentage);
		
		Task AccountPublicationStarted(int correlationId);
		Task AccountPublicationEnded(int correlationId);

		Task AccountPublicationPOWNonceIteration(int correlationId, int nonce, int difficulty);
		Task AccountPublicationPOWNonceFound(int correlationId, int nonce, int difficulty, List<int> powSolutions);

		Task AccountPublicationMessage(int correlationId, string message);
		Task AccountPublicationStep(int correlationId, string stepName, int stepIndex, int stepTotal);
		Task AccountPublicationError(int correlationId, string error);

		Task WalletSyncStarted(ushort chainType);
		Task WalletSyncEnded(ushort chainType);
		Task WalletSyncUpdate(ushort chainType, long currentBlockId, long blockHeight, decimal percentage);
		Task WalletSyncError(ushort chainType, string error);

		Task BlockchainSyncStarted(ushort chainType);
		Task BlockchainSyncEnded(ushort chainType);
		Task BlockchainSyncUpdate(ushort chainType, long currentBlockId, long publicBlockHeight, decimal percentage, string estimatedTimeRemaining);
		Task BlockchainSyncError(ushort chainType, string error);

		Task TransactionSent(int correlationId, string transactionId);
		Task TransactionConfirmed(string transactionId, object transaction);
		Task TransactionReceived(string transactionId);
		Task TransactionMessage(string transactionId, string message);
		Task TransactionRefused(string transactionId, string reason);
		Task TransactionError(int correlationId, string transactionId, List<ushort> errorCodes);

		Task MiningStarted(ushort chainType);
		Task MiningEnded(ushort chainType);
		Task MiningElected(ushort chainType);
		Task MiningBountyAllocated(ushort chainType);

		Task BlockInserted(ushort chainType, long blockId, DateTime timestamp, string hash, long publicBlockId, int lifespan);
		Task DigestInserted(ushort chainType, int digestId, DateTime timestamp, string hash);

		Task Message(string message, DateTime timestamp, string level, object[] properties);
		Task Error(int correlationId, string error);

		Task Alert(int messageCode);
		
		Task ConnectableStatusChanged(bool connectable);

		Task ShutdownCompleted();
		Task ShutdownStarted();
	}

	public interface IRpcClientEventsExtended : IRpcClientEvents {
		Task RequestCopyWallet(ushort chainType, string message);
	}

	public interface IRpcProvider : IRpcServerMethods, IRpcClientMethods {

		Task<object> ValueOnChainEventRaised(CorrelationContext correlationContext, BlockchainSystemEventType eventType, BlockchainType chainType, object[] extraParameters);
		void TotalPeersUpdated(int count);
		void ShutdownCompleted();
		void ShutdownStarted();
		void LogMessage(string message, DateTime timestamp, string level, object[] properties);
	}

	public interface IRpcProvider<RPC_HUB, RCP_CLIENT> : IRpcProvider
		where RPC_HUB : RpcHub<RCP_CLIENT>
		where RCP_CLIENT : class, IRpcClient {

		IRpcService<RPC_HUB, RCP_CLIENT> RpcService { get; set; }

		IHubContext<RPC_HUB, RCP_CLIENT> HubContext { get; set; }
	}

	public class RpcProvider<RPC_HUB, RCP_CLIENT> : IRpcProvider<RPC_HUB, RCP_CLIENT>
		where RPC_HUB : RpcHub<RCP_CLIENT>
		where RCP_CLIENT : class, IRpcClient {

		private readonly object locker = new object();
		private readonly Dictionary<int, LongRunningEvents> longRunningEvents = new Dictionary<int, LongRunningEvents>();
		
		private bool logMessagesEnabled = true;

		//private Timer maintenanceTimer;

		public RpcProvider() {
			//this.maintenanceTimer = new Timer(this.TimerCallback, this, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
		}

		public IHubContext<RPC_HUB, RCP_CLIENT> HubContext { get; set; }

		public IRpcService<RPC_HUB, RCP_CLIENT> RpcService { get; set; }

		public Task ToggleServerMessages(bool enable) {

			return new TaskFactory().StartNew(() => {
				this.logMessagesEnabled = enable;
			});
		}

		public Task<object> QuerySystemInfo() {
			try {

				int systemMode = 0;

#if TESTNET
				systemMode = 1;
#elif DEVNET
			systemMode = 2;
#endif

				return Task.FromResult((object) new SystemInfoAPI(){Version = GlobalSettings.SoftwareVersion.ToString(), Mode = systemMode});
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query system version");

				throw new HubException("Failed to query system version");
			}
		}

		public Task<List<object>> QuerySupportedChains() {
			try {
				IGlobalsService globalsService = DIService.Instance.GetService<IGlobalsService>();

				return Task.FromResult(globalsService.SupportedChains.Select(c => new SupportedChainsAPI(){Id = c.Key.Value, Name= c.Value.Name, Enabled = c.Value.Enabled, Started = c.Value.Started}).Cast<object>().ToList());
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query supported chains API");

				throw new HubException("Failed to query block heights");
			}
		}

		public Task<string> Ping() {
			return Task.FromResult("pong");
		}

		public Task<int> QueryTotalConnectedPeersCount() {
			try {
				INetworkingService networkingService = DIService.Instance.GetService<INetworkingService>();

				int count = 0;

				if(networkingService?.IsStarted ?? false) {
					count = networkingService.CurrentPeerCount;
				}

				return Task.FromResult(count);
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query total peers count API");

				throw new HubException("Failed to Query total peers count");
			}
		}

		public Task<bool> QueryMiningPortConnectable() {
			try {
				INetworkingService networkingService = DIService.Instance.GetService<INetworkingService>();

				bool isConnectable = false;

				if(networkingService?.IsStarted ?? false) {
					isConnectable = networkingService.ConnectionStore.IsConnectable;
				}

				return Task.FromResult(isConnectable);
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query node connectible");

				throw new HubException("Failed to Query node connectible");
			}
		}

		public Task<bool> Shutdown() {

			try {

				return new TaskFactory<bool>().StartNew(() => this.RpcService.ShutdownRequested?.Invoke() ?? false);

			} catch(Exception ex) {
				Log.Error(ex, "Failed to request system shutdown");

				throw new HubException("Failed to request system shutdown");
			}

		}

	#region events

		/// <summary>
		///     Receive chain events and propagate them to the clients
		/// </summary>
		/// <param name="eventType"></param>
		/// <param name="chainType"></param>
		/// <param name="extraParameters"></param>
		/// <returns></returns>
		public async Task<object> ValueOnChainEventRaised(CorrelationContext correlationContext, BlockchainSystemEventType eventType, BlockchainType chainType, object[] parameters) {

			(Task<Task<object>> task, CorrelationContext correlationContext) result;
			var extraParameters = parameters?.ToArray() ?? new object[0];

			Func<CorrelationContext, LongRunningEvents, Task<object>> action = null;

			if(BlockchainSystemEventTypes.Instance.IsValueBaseset(eventType)) {
				if(eventType == BlockchainSystemEventTypes.Instance.RequestWalletPassphrase) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.EnterWalletPassphrase(sessionCorrelationContext.CorrelationId, chainType.Value, (int) extraParameters[0], (int) extraParameters[1]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.RequestKeyPassphrase) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.EnterKeysPassphrase(sessionCorrelationContext.CorrelationId, chainType.Value, (int) extraParameters[0], (Guid) extraParameters[1], extraParameters[2].ToString(), (int) extraParameters[3]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.RequestCopyKeyFile) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.CopyWalletKeyFile(sessionCorrelationContext.CorrelationId, chainType.Value, (int) extraParameters[0], (Guid) extraParameters[1], extraParameters[2].ToString(), (int) extraParameters[3]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.WalletSyncStarted) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.WalletSyncStarted(chainType.Value);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.WalletSyncEnded) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.WalletSyncEnded(chainType.Value);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.WalletSyncUpdate) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.WalletSyncUpdate(chainType.Value, (long) extraParameters[0], (long) extraParameters[1], (decimal) extraParameters[2]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.BlockchainSyncStarted) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.BlockchainSyncStarted(chainType.Value);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.BlockchainSyncEnded) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.BlockchainSyncEnded(chainType.Value);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.BlockchainSyncUpdate) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.BlockchainSyncUpdate(chainType.Value, (long) extraParameters[0], (long) extraParameters[1], (decimal) extraParameters[2], (string) extraParameters[3]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.BlockInserted) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.BlockInserted(chainType.Value, (long) extraParameters[0], (DateTime) extraParameters[1], (string) extraParameters[2], (long) extraParameters[3], (int) extraParameters[4]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.DigestInserted) {
					action = async (sessionCorrelationContext, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.DigestInserted(chainType.Value, (int) extraParameters[0], (DateTime) extraParameters[1], (string) extraParameters[2]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.AccountPublicationEnded) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						//await this.HubContext?.Clients?.All?.AccountPublicationCompleted(sessionCorrelationId, (Guid)parameters[0], (bool)parameters[1], (long)parameters[2]);
						return this.HubContext?.Clients?.All?.AccountPublicationEnded(sessionCorrelationId.CorrelationId);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.AccountPublicationPOWNonceIteration) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.AccountPublicationPOWNonceIteration(sessionCorrelationId.CorrelationId, (int) extraParameters[0], (int) extraParameters[1]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.AccountPublicationPOWNonceFound) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.AccountPublicationPOWNonceFound(sessionCorrelationId.CorrelationId, (int) extraParameters[0], (int) extraParameters[1], (List<int>) extraParameters[2]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.TransactionError) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.TransactionError(sessionCorrelationId.CorrelationId, (string) extraParameters[0], (List<ushort>) extraParameters[1]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.TransactionSent) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.TransactionSent(sessionCorrelationId.CorrelationId, (string) extraParameters[0]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.TransactionReceived) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.TransactionReceived((string) extraParameters[0]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.KeyGenerationStarted) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.KeyGenerationStarted(sessionCorrelationId.CorrelationId, (string) extraParameters[0], (int) extraParameters[1], (int) extraParameters[2]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.KeyGenerationEnded) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.KeyGenerationEnded(sessionCorrelationId.CorrelationId, (string) extraParameters[0], (int) extraParameters[1], (int) extraParameters[2]);
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.KeyGenerationPercentageUpdate) {
					action = async (sessionCorrelationId, resetEvent) => {
						// alert the client of the event
						return this.HubContext?.Clients?.All?.KeyGenerationPercentageUpdate(sessionCorrelationId.CorrelationId, (string) extraParameters[0], (int) extraParameters[1]);
						
					};
				} else if(eventType == BlockchainSystemEventTypes.Instance.Alert) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.Alert((int) extraParameters[0]);
					};
				} 
				else if(eventType == BlockchainSystemEventTypes.Instance.ConnectableStatusChanged) {
					action = async (sessionCorrelationId, resetEvent) => {

						// alert the client of the event
						return this.HubContext?.Clients?.All?.ConnectableStatusChanged((bool) extraParameters[0]);
					};
				}
				else {

					action = async (sessionCorrelationId, resetEvent) => {

						object parameter = null;

						if((extraParameters != null) && extraParameters.Any()) {
							parameter = extraParameters[0];
						}

						// alert the client of the event
						await this.HubContext?.Clients?.All?.LongRunningStatusUpdate(sessionCorrelationId.CorrelationId, eventType.Value, 1, parameter);

						return default;
					};
				}
			} else if(BlockchainSystemEventTypes.Instance.IsValueChildset(eventType)) {
				if(eventType == NeuraliumBlockchainSystemEventTypes.NeuraliumInstance.AccountTotalUpdated) {
					action = (sessionCorrelationId, resetEvent) => {

						string accountId = this.GetParameterField<string>("AccountId", extraParameters[0]);
						object total = this.GetParameterField<object>("Total", extraParameters[0]);

						this.HubContext?.Clients?.All?.AccountTotalUpdated(sessionCorrelationId.CorrelationId, accountId, total);

						return default;
					};
				} else {
					action = (sessionCorrelationId, resetEvent) => {

						object parameter = null;

						if((extraParameters != null) && extraParameters.Any()) {
							parameter = extraParameters[0];
						}

						// alert the client of the event
						this.HubContext?.Clients?.All?.LongRunningStatusUpdate(sessionCorrelationId.CorrelationId, eventType.Value, 2, parameter);

						return default;
					};
				}
			}

			object actionreasult = null;
			if(correlationContext.IsNew) {
				// if we had no previous correlation id, then its an uncorrelated event, so we give it one
				result = this.CreateServerLongRunningEvent(action);

				actionreasult = await result.task.Result;
			} else {
				LongRunningEvents autoEvent = this.longRunningEvents.ContainsKey(correlationContext.CorrelationId) ? this.longRunningEvents[correlationContext.CorrelationId] : null;

				actionreasult = await action(correlationContext, autoEvent);
			}

			return actionreasult;
		}

		public void TotalPeersUpdated(int count) {
			this.HubContext?.Clients?.All?.PeerTotalUpdated(count);
		}

		public void ShutdownStarted() {
			this.HubContext?.Clients?.All?.ShutdownStarted();
		}

		public void ShutdownCompleted() {
			this.HubContext?.Clients?.All?.ShutdownCompleted();
		}

		public void LogMessage(string message, DateTime timestamp, string level, object[] properties) {
			if(this.logMessagesEnabled) {
				this.HubContext?.Clients?.All?.Message(message, timestamp, level, properties);
			}
		}

	#endregion

	#region common chain queries

		public async Task EnterWalletPassphrase(int correlationId, ushort chainType, int keyCorrelationCode, string passphrase) {
			try {
				await this.GetChainInterface(chainType).SetWalletPassphrase(correlationId, keyCorrelationCode, passphrase).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to provide wallet passphrase");

				throw new HubException("Failed to provide wallet passphrase");
			}
		}

		public async Task EnterKeyPassphrase(int correlationId, ushort chainType, int keyCorrelationCode, string passphrase) {
			try {
				await this.GetChainInterface(chainType).SetWalletKeyPassphrase(correlationId, keyCorrelationCode, passphrase).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to provide wallet key passphrase");

				throw new HubException("Failed to provide wallet key passphrase");
			}
		}

		public async Task WalletKeyFileCopied(int correlationId, ushort chainType, int keyCorrelationCode) {
			try {
				await this.GetChainInterface(chainType).WalletKeyFileCopied(correlationId, keyCorrelationCode).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to alert that key file was copied");

				throw new HubException("Failed to alert that key file was copied");
			}
		}

		public async Task<bool> IsBlockchainSynced(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).IsBlockchainSynced().awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to check if blockchains is synced");

				throw new HubException("Failed to check if blockchains is synced");
			}
		}

		public async Task<bool> IsWalletSynced(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).IsWalletSynced().awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to check if wallet is synced");

				throw new HubException("Failed to check if wallet is synced");
			}
		}

		public async Task<bool> SyncBlockchain(ushort chainType, bool force) {
			try {
				return await this.GetChainInterface(chainType).SyncBlockchain(force).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to sync blockchain");

				throw new HubException("Failed to sync blockchain");
			}
		}

		public async Task<object> BackupWallet(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).BackupWallet().awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to backup wallet");

				throw new HubException("Failed to backup wallet");
			}
		}

		public async Task<long> QueryBlockHeight(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).QueryBlockHeight().awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query block height API");

				throw new HubException("Failed to query block heights");
			}
		}

		public async Task<object> QueryChainStatus(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).QueryChainStatus().awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query chain status API");

				throw new HubException("Failed to query chain status");
			}
		}

		public async Task<object> QueryBlockChainInfo(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).QueryBlockChainInfo().awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query blockchain info");

				throw new HubException("Failed to query blockchain info");
			}
		}

		public async Task<bool> IsWalletLoaded(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).IsWalletLoaded().awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query if wallet is loaded API");

				throw new HubException("Failed to query if wallet is loaded");
			}
		}

		public async Task<bool> WalletExists(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).WalletExists().awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query if wallet exists API");

				throw new HubException("Failed to query if wallet exists");
			}
		}

		public async Task<int> LoadWallet(ushort chainType) {
			return await this.CreateClientLongRunningEvent(async (correlationContext, resetEvent) => await this.GetChainInterface(chainType).LoadWallet(correlationContext).awaitableTask);
		}

		public async Task<int> CreateNewWallet(ushort chainType, string accountName, bool encryptWallet, bool encryptKey, bool encryptKeysIndividually, ImmutableDictionary<string, string> passphrases, bool publishAccount) {
			try {
				return await this.CreateClientLongRunningEvent(async (correlationContext, resetEvent) => await this.GetChainInterface(chainType).CreateNewWallet(correlationContext, accountName, encryptWallet, encryptKey, encryptKeysIndividually, passphrases?.ToImmutableDictionary(e => int.Parse(e.Key), e => e.Value), publishAccount).awaitableTask);
			} catch(Exception ex) {
				Log.Error(ex, "Failed to load wallet");

				throw new HubException("Failed to load wallet");
			}
		}

		public async Task<List<object>> QueryWalletTransactionHistory(ushort chainType, Guid accountUuid) {
			try {
				var result = await this.GetChainInterface(chainType).QueryWalletTransactionHistory(accountUuid).awaitableTask;

				return result.Cast<object>().ToList();
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet transaction history");

				throw new HubException("Failed to query wallet transaction history");
			}
		}

		public async Task<object> QueryWalletTransationHistoryDetails(ushort chainType, Guid accountUuid, string transactionId) {
			try {
				return await this.GetChainInterface(chainType).QueryWalletTransationHistoryDetails(accountUuid, transactionId).awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet transaction history details");

				throw new HubException("Failed to query wallet transaction history details");
			}
		}

		public async Task<List<object>> QueryWalletAccounts(ushort chainType) {
			try {
				var result = await this.GetChainInterface(chainType).QueryWalletAccounts().awaitableTask;

				return result.Cast<object>().ToList();
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet accounts");

				throw new HubException("Failed to load wallet accounts");
			}
		}

		public async Task<object> QueryWalletAccountDetails(ushort chainType, Guid accountUuid) {
			try {
				return await this.GetChainInterface(chainType).QueryWalletAccountDetails(accountUuid).awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet account details");

				throw new HubException("Failed to load wallet account details");
			}
		}

		public async Task<string> QueryWalletAccountPresentationTransactionId(ushort chainType, Guid accountUuid) {
			try {
				TransactionId transactionId = await this.GetChainInterface(chainType).QueryWalletAccountPresentationTransactionId(accountUuid).awaitableTask;

				return transactionId.ToString();
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query account presentation transaction Id");

				throw new HubException("Failed to query account presentation transaction Id");
			}
		}

		public async Task<int> CreateAccount(ushort chainType, string accountName, bool publishAccount, bool encryptKeys, bool encryptKeysIndividually, ImmutableDictionary<string, string> passphrases) {
			try {
				return await this.CreateClientLongRunningEvent(async (correlationContext, resetEvent) => await this.GetChainInterface(chainType).CreateAccount(correlationContext, accountName, publishAccount, encryptKeys, encryptKeysIndividually, passphrases?.ToImmutableDictionary(e => int.Parse(e.Key), e => e.Value)).awaitableTask);
			} catch(Exception ex) {
				Log.Error(ex, "Failed to create account");

				throw new HubException("Failed to create account");
			}
		}

		public async Task<bool> SetActiveAccount(ushort chainType, Guid accountUuid) {
			try {
				return await this.GetChainInterface(chainType).SetActiveAccount(accountUuid).awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet accounts");

				throw new HubException("Failed to load wallet accounts");
			}
		}

		public async Task<bool> SetWalletPassphrase(int correlationId, string passphrase) {
			try {
				await this.FullfillLongRunningEvent(correlationId, passphrase);

				return true;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet accounts");

				throw new HubException("Failed to load wallet accounts");
			}
		}

		public async Task<bool> SetKeysPassphrase(int correlationId, string passphrase) {
			try {
				await this.FullfillLongRunningEvent(correlationId, passphrase);

				return true;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet accounts");

				throw new HubException("Failed to load wallet accounts");
			}
		}

		public async Task<int> PublishAccount(ushort chainType, Guid? accountUuId) {
			return await this.CreateClientLongRunningEvent(async (correlationId, resetEvent) => await this.GetChainInterface(chainType).PresentAccountPublicly(correlationId,accountUuId).awaitableTask);
		}

		public Task StartMining(ushort chainType, string delegateAccountId) {
			try {
				AccountId delegateId = null;

				if(!string.IsNullOrWhiteSpace(delegateAccountId)) {
					delegateId = AccountId.FromString(delegateAccountId);
				}

				this.GetChainInterface(chainType).EnableMining(delegateId);

				return Task.FromResult(true);

			} catch(Exception ex) {
				Log.Error(ex, "Failed to enable mining");

				throw new HubException("Failed to enable mining");
			}
		}

		public Task StopMining(ushort chainType) {
			try {
				this.GetChainInterface(chainType).DisableMining();

				return Task.FromResult(true);

			} catch(Exception ex) {
				Log.Error(ex, "Failed to disable mining");

				throw new HubException("Failed to disable mining");
			}
		}

		public Task<bool> IsMiningAllowed(ushort chainType) {
			try {
				return Task.FromResult(this.GetChainInterface(chainType).IsMiningAllowed);

			} catch(Exception ex) {
				Log.Error(ex, "Failed to verify if mining is allowed");

				throw new HubException("Failed to verify if mining is allowed");
			}
		}

		public Task<bool> IsMiningEnabled(ushort chainType) {
			try {
				return Task.FromResult(this.GetChainInterface(chainType).IsMiningEnabled);

			} catch(Exception ex) {
				Log.Error(ex, "Failed to verify if mining is enabled");

				throw new HubException("Failed to verify if mining is enabled");
			}
		}

		public async Task<bool> QueryBlockchainSynced(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).QueryBlockchainSynced().awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to query blockchain sync status");

				throw new HubException("Failed to query blockchain sync status");
			}
		}

		public async Task<bool> QueryWalletSynced(ushort chainType) {
			try {
				return await this.GetChainInterface(chainType).QueryWalletSynced().awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to query wallet sync status");

				throw new HubException("Failed to query wallet sync status");
			}
		}

		public async Task<string> QueryBlock(ushort chainType, long blockId) {
			try {
				return await this.GetChainInterface(chainType).QueryBlock(blockId).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to query block");

				throw new HubException("Failed to query block");
			}
		}

		public async Task<byte[]> QueryCompressedBlock(ushort chainType, long blockId) {
			try {
				return await this.GetChainInterface(chainType).QueryCompressedBlock(blockId).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to query block");

				throw new HubException("Failed to query block");
			}
		}

		public async Task<List<object>> QueryBlockBinaryTransactions(ushort chainType, long blockId) {
			try {
				var result = await this.GetChainInterface(chainType).QueryBlockBinaryTransactions(blockId).awaitableTask;

				return result.Select(e => new {TransactionId = e.Key.ToString(), Data = e.Value}).Cast<object>().ToList();

			} catch(Exception ex) {
				Log.Error(ex, "Failed to query block binary transactions");

				throw new HubException("Failed to query block binary transactions");
			}
		}

		public async Task<object> QueryElectionContext(ushort chainType, long blockId) {
			try {
				return await this.GetChainInterface(chainType).QueryElectionContext(blockId).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to query block election details");

				throw new HubException("Failed to query block election details");
			}
		}

		public async Task<bool> CreateNextXmssKey(ushort chainType, Guid accountUuid, byte ordinal) {
			try {
				return await this.GetChainInterface(chainType).CreateNextXmssKey(accountUuid, ordinal).awaitableTask;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to create next xmss key");

				throw new HubException("Failed to create next xmss key");
			}
		}

		public async Task<byte[]> SignXmssMessage(ushort chainType, Guid accountUuid, byte[] message) {
			try {
				SafeArrayHandle signature = await this.GetChainInterface(chainType).SignXmssMessage(accountUuid, (ByteArray) message).awaitableTask;

				var result = signature.ToExactByteArrayCopy();

				signature.Return();

				return result;

			} catch(Exception ex) {
				Log.Error(ex, "Failed to sign XMSS message");

				throw new HubException("Failed to sign XMSS message");
			}
		}

	#endregion

	#region neuralium chain queries

		public async Task<object> QueryAccountTotalNeuraliums(Guid accountId) {
			try {
				return await this.GetNeuraliumChainInterface().QueryWalletTotal(accountId).awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to Query wallet total");

				throw new HubException("Failed to query wallet total");
			}
		}

		public async Task<int> SendNeuraliums(string targetAccountId, decimal amount, decimal tip, string note) {

			try {
				return await this.CreateClientLongRunningEvent(async (correlationContext, resetEvent) => await this.GetNeuraliumChainInterface().SendNeuraliums(AccountId.FromString(targetAccountId), amount, tip, note, correlationContext).awaitableTask);
			} catch(Exception ex) {
				Log.Error(ex, "Failed to send neuraliums");

				throw new HubException("Failed to send neuraliums");
			}
		}

		public async Task<object> QueryNeuraliumTimelineHeader(Guid accountUuid) {

			try {
				return await this.GetNeuraliumChainInterface().QueryNeuraliumTimelineHeader(accountUuid);
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query neuralium timeline header");

				throw new HubException("Failed to query neuralium timeline header");
			}
		}

		public async Task<List<object>> QueryNeuraliumTimelineSection(Guid accountUuid, DateTime firstday, int skip, int take) {

			try {
				var results = await this.GetNeuraliumChainInterface().QueryNeuraliumTimelineSection(accountUuid, firstday, skip, take);

				return results.Cast<object>().ToList();
			} catch(Exception ex) {
				Log.Error(ex, "Failed to query neuralium timeline header");

				throw new HubException("Failed to query neuralium timeline header");
			}
		}

#if TESTNET || DEVNET
		public async Task<int> RefillNeuraliums(Guid accountUuid) {

			try {
				return await this.CreateClientLongRunningEvent(async (correlationContext, resetEvent) => await this.GetNeuraliumChainInterface().RefillNeuraliums(accountUuid, correlationContext).awaitableTask);
			} catch(Exception ex) {
				Log.Error(ex, "Failed to refill neuraliums");

				throw new HubException("Failed to refill neuraliums");
			}
		}
#endif

		public async Task<List<object>> QueryNeuraliumTransactionPool() {
			try {
				return await this.GetNeuraliumChainInterface().QueryNeuraliumTransactionPool().awaitableTask;
			} catch(Exception ex) {
				Log.Error(ex, "Failed to neuralium transaction pool");

				throw new HubException("Failed to query neuralium transaction pool");
			}
		}

		public Task<List<object>> QueryMiningHistory(ushort chainType) {
			try {
				var results = this.GetChainInterface(chainType).QueryMiningHistory();

				return Task.FromResult(results.Cast<object>().ToList());

			} catch(Exception ex) {
				Log.Error(ex, "Failed to query mining history");

				throw new ApplicationException("Failed to query mining history");
			}
		}

	#endregion

	#region Utils

		public T ConvertParameterType<T>(object parameter)
			where T : class, new() {

			T result = new T();

			Type resultType = typeof(T);

			foreach(PropertyInfo property in parameter.GetType().GetProperties()) {

				PropertyInfo mainProp = resultType.GetProperty(property.Name);

				if(mainProp != null) {

					mainProp.SetValue(result, property.GetValue(parameter));
				}
			}

			return result;
		}

		public T GetParameterField<T>(string fieldName, object parameter) {
			PropertyInfo mainProp = parameter.GetType().GetProperty(fieldName);

			if(mainProp != null) {

				return (T) mainProp.GetValue(parameter);
			}

			return default;
		}

		protected IBlockChainInterface GetChainInterface(ushort chainType) {
			return this.GetChainInterface<IBlockChainInterface>(chainType);
		}

		protected INeuraliumBlockChainInterface GetNeuraliumChainInterface() {
			return this.GetChainInterface<INeuraliumBlockChainInterface>(NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium.Value);
		}

		protected T GetChainInterface<T>(ushort chainType)
			where T : class, IBlockChainInterface {
			BlockchainType castedChainType = chainType;

			T chainInterface = null;

			if(castedChainType.Value == NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium.Value) {
				chainInterface = (T) this.RpcService[NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium];

			} else {
				throw new HubException("Unsupported chain type");
			}

			if(chainInterface == null) {
				throw new HubException("Chain is invalid");
			}

			return chainInterface;
		}

		/// <summary>
		///     Perform some maintenance
		/// </summary>
		/// <param name="state"></param>
		private void TimerCallback(object state) {

			int[] timedOuts;

			lock(this.locker) {
				timedOuts = this.longRunningEvents.Where(e => e.Value.Timeout >= DateTime.UtcNow).Select(e => e.Key).ToArray();
			}

			foreach(int timeout in timedOuts) {
				LongRunningEvents longRunningEvent = this.longRunningEvents[timeout];

				lock(this.locker) {
					this.longRunningEvents.Remove(timeout);
				}

				// lets free the waiting handle.
				longRunningEvent.AutoResetEvent.Set();
			}
		}

		/// <summary>
		///     Create a long running event that may take a while to return
		/// </summary>
		/// <param name="action"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public (Task<Task<T>> task, CorrelationContext correlationContext) CreateServerLongRunningEvent<T>(Func<CorrelationContext, LongRunningEvents, Task<T>> action, int timeout = 60 * 3) {
			CorrelationContext correlationContext = new CorrelationContext(GlobalRandom.GetNext());

			LongRunningEvents longRunningEvent = new LongRunningEvents(TimeSpan.FromMinutes(timeout));

			lock(this.locker) {
				this.longRunningEvents.Add(correlationContext.CorrelationId, longRunningEvent);
			}

			var task = new Task<Task<T>>(async () => {

				try {
					var resultTask = action(correlationContext, longRunningEvent);

					T result = default;

					if(resultTask != null) {
						result = await resultTask;
					}

					return result;
				} catch(Exception ex) {
					//TODO: what to do here?
				} finally {
					// clean up the event
					await this.CompleteLongRunningEvent(correlationContext.CorrelationId, null);
				}

				return default;
			});

			task.Start();

			return (task, correlationContext);
		}

		/// <summary>
		///     Ensure a long running thread is provided it's value
		/// </summary>
		/// <param name="correlationId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public Task<bool> FullfillLongRunningEvent(int correlationId, object data) {

			lock(this.locker) {
				if(!this.longRunningEvents.ContainsKey(correlationId)) {
					return Task.FromResult(true);
				}

				LongRunningEvents longRunningEvent = this.longRunningEvents[correlationId];

				longRunningEvent.State = data;

				longRunningEvent.AutoResetEvent.Set();
			}

			return Task.FromResult(true);
		}

		/// <summary>
		///     Clean the long running event from cache
		/// </summary>
		/// <param name="correlationId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public Task<bool> CompleteLongRunningEvent(int correlationId, object data) {

			lock(this.locker) {

				var result = this.FullfillLongRunningEvent(correlationId, data);

				if(this.longRunningEvents.ContainsKey(correlationId)) {
					this.longRunningEvents.Remove(correlationId);
				}

				return result;
			}
		}

		/// <summary>
		///     If the long running event was triggered by a client, then we need to ensure that we return.
		/// </summary>
		/// <param name="action"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private Task<int> CreateClientLongRunningEvent<T>(Func<CorrelationContext, LongRunningEvents, Task<T>> action) {
			var result = this.CreateServerLongRunningEvent(async (correlationId, longRunningEvent) => {

				try {
					T res = await action(correlationId, longRunningEvent);

					// alert clients that this correlated method has returned
					await this.HubContext?.Clients?.All?.ReturnClientLongRunningEvent(correlationId.CorrelationId, 0, "");

					return res;
				} catch(Exception ex) {
					await this.HubContext?.Clients?.All?.ReturnClientLongRunningEvent(correlationId.CorrelationId, 1, ex.Message);

					throw ex;
				}
			});

			return Task.FromResult(result.correlationContext.CorrelationId);
		}

		/// <summary>
		///     Allow the client to inform us that we can slide the timeout. the client is still there and the wait is justified.
		/// </summary>
		/// <param name="correlationId"></param>
		/// <returns></returns>
		public Task<bool> RenewLongRunningEvent(int correlationId) {

			LongRunningEvents longRunningEvent = null;

			lock(this.locker) {
				if(this.longRunningEvents.ContainsKey(correlationId)) {
					longRunningEvent = this.longRunningEvents[correlationId];
				}
			}

			longRunningEvent?.SlideTimeout();

			return Task.FromResult(true);
		}

		public class LongRunningEvents {
			public readonly AutoResetEvent AutoResetEvent;

			private readonly TimeSpan spanTimeout;
			public DateTime Timeout;

			public LongRunningEvents(TimeSpan timeout) {
				this.spanTimeout = this.spanTimeout;
				this.AutoResetEvent = new AutoResetEvent(false);
				this.SlideTimeout();
			}

			public object State { get; set; }

			public void SlideTimeout() {
				this.Timeout = DateTime.UtcNow + this.spanTimeout;
			}
		}

	#endregion

	}
}