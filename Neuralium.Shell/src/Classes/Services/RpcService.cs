using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Blockchains.Neuralium.Classes;
using Blockchains.Neuralium.Classes.NeuraliumChain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.Cryptography.TLS;
using Neuralia.Blockchains.Tools;
using Neuralium.Shell.Classes.General;
using Neuralium.Shell.Classes.Runtime;
using Neuralium.Shell.Controllers;
using Serilog;

namespace Neuralium.Shell.Classes.Services {

	public interface IRpcService : IDisposable2 {

		INeuraliumBlockChainInterface NeuraliumBlockChainInterface { get; }

		IBlockChainInterface this[ushort i] { get; set; }

		IBlockChainInterface this[BlockchainType i] { get; set; }

		IRpcProvider RpcProvider { get; }

		Func<bool> ShutdownRequested { get; set; }

		bool IsStarted { get; }
		void Start();
		void Stop();
	}

	public interface IRpcService<RPC_HUB, RCP_CLIENT> : IRpcService
		where RPC_HUB : RpcHub<RCP_CLIENT>
		where RCP_CLIENT : class, IRpcClient {

		IHubContext<RPC_HUB, RCP_CLIENT> hubContext { get; }
	}

	public class RpcService<RPC_HUB, RCP_CLIENT> : IRpcService<RPC_HUB, RCP_CLIENT>
		where RPC_HUB : RpcHub<RCP_CLIENT>
		where RCP_CLIENT : class, IRpcClient {

		private readonly Dictionary<BlockchainType, IBlockChainInterface> chains = new Dictionary<BlockchainType, IBlockChainInterface>();
		protected readonly IRpcProvider<RPC_HUB, RCP_CLIENT> rpcProvider;
		private Task rpcTask;

		private IWebHost rpcWebHost;
		private IServiceProvider serviceProvider;

		public RpcService(IRpcProvider rpcProvider, IServiceProvider serviceProvider) {
			this.rpcProvider = (IRpcProvider<RPC_HUB, RCP_CLIENT>) rpcProvider;
			this.rpcProvider.RpcService = this;
			this.serviceProvider = serviceProvider;
		}

		public Func<bool> ShutdownRequested { get; set; }
		public bool IsStarted { get; private set; }

		public IRpcProvider RpcProvider => this.rpcProvider;

		public IHubContext<RPC_HUB, RCP_CLIENT> hubContext { get; private set; }

		public void Start() {
			if(GlobalSettings.ApplicationSettings.RpcMode != AppSettingsBase.RpcModes.None) {
				try {
					if(this.rpcWebHost != null) {
						throw new ApplicationException("Rpc webhost is already running");
					}

					if(GlobalSettings.ApplicationSettings.RpcMode != AppSettingsBase.RpcModes.None) {
						this.rpcWebHost = this.BuildRpcWebHost(new string[0]);

						// get the signalr hub
						this.hubContext = this.rpcWebHost.Services.GetService<IHubContext<RPC_HUB, RCP_CLIENT>>();
						this.rpcProvider.HubContext = this.hubContext;
						this.rpcTask = this.rpcWebHost.RunAsync();
					}

					this.IsStarted = true;

				} catch(Exception ex) {
					throw new ApplicationException("Failed to start RPC Server", ex);
				}
			}
		}

		public void Stop() {
			try {
				this.rpcWebHost?.StopAsync(TimeSpan.FromSeconds(5));
			} catch(Exception ex) {
				//TODO: im disabling this for the demo. restore error handling!!
			} finally {
				this.rpcWebHost?.Dispose();
				this.rpcWebHost = null;
				this.IsStarted = false;
			}
		}

		public INeuraliumBlockChainInterface NeuraliumBlockChainInterface => (INeuraliumBlockChainInterface) this[NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium];

		public IBlockChainInterface this[ushort i] {
			get => this[(BlockchainType) i];
			set => this[(BlockchainType) i] = value;
		}

		public IBlockChainInterface this[BlockchainType i] {
			get => this.chains[i];
			set {
				value.ChainEventRaised += this.rpcProvider.ValueOnChainEventRaised;
				this.chains.Add(i, value);
			}
		}

		protected IConfigurationRoot GetAspnetCoreConfiguration(string[] args) {
			IConfigurationRoot config = new ConfigurationBuilder().AddCommandLine(args).Build();

			//TODO: set this correctly
			int serverport = config.GetValue<int?>("port") ?? GlobalSettings.ApplicationSettings.rpcPort;
			string serverurls = config.GetValue<string>("server.urls") ?? $"http://*:{serverport}";

			var configDictionary = new Dictionary<string, string> {{"server.urls", serverurls}, {"port", serverport.ToString()}};

			return new ConfigurationBuilder().AddCommandLine(args).AddInMemoryCollection(configDictionary).Build();
		}

		protected IWebHost BuildRpcWebHost(string[] args) {
			IConfigurationRoot config = this.GetAspnetCoreConfiguration(args);
			int port = config.GetValue<int?>("port") ?? 5050;

			IWebHostBuilder builder = new WebHostBuilder().UseEnvironment("Development").UseConfiguration(config).UseContentRoot(Directory.GetCurrentDirectory()).UseKestrel(options => {

				IPAddress listenAddress = IPAddress.Loopback;

				if(GlobalSettings.ApplicationSettings.RpcBindMode == AppSettingsBase.RpcBindModes.Any) {
					listenAddress = IPAddress.Any;
				}

				options.Listen(listenAddress, port, listenOptions => {
					if(GlobalSettings.ApplicationSettings.RpcTransport == AppSettingsBase.RpcTransports.Secured) {

						X509Certificate2 rpcCertificate = null;

						if(string.IsNullOrWhiteSpace(GlobalSettings.ApplicationSettings.TlsCertificate)) {
							// generate a certificate file
							rpcCertificate = new TlsProvider(2048, TlsProvider.HashStrength.Sha256).Build().localCertificate;
						} else {
							// Ok, load our certificate file
							string directory = Path.GetDirectoryName(GlobalSettings.ApplicationSettings.TlsCertificate);
							string file = Path.GetFileName(GlobalSettings.ApplicationSettings.TlsCertificate);

							if(string.IsNullOrWhiteSpace(directory)) {
								directory = Bootstrap.GetExecutingDirectoryName();
							}

							string certfile = Path.Combine(directory, file);

							if(!File.Exists(certfile)) {
								throw new ApplicationException($"The TLS certificate file path did not exist: '{certfile}'");
							}

							rpcCertificate = new X509Certificate2(File.ReadAllBytes(certfile), "");
						}

						listenOptions.UseHttps(rpcCertificate);
					}
				});

				options.Limits.MaxConcurrentConnections = 2;

			});

			this.AddCustomSettings(builder);

			return builder.Build();
		}

		protected virtual void AddCustomSettings(IWebHostBuilder builder) {
			builder.UseStartup<Startup<RPC_HUB, RCP_CLIENT>>();

		}

	#region dispose

		protected virtual void Dispose(bool disposing) {
			if(disposing && !this.IsDisposed) {
				try {
					try {
						this.Stop();
					} catch(Exception ex) {
						Log.Error(ex, "Failed to stop");
					}
				} finally {
					this.IsDisposed = true;
				}
			}
		}

		~RpcService() {
			this.Dispose(false);
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}