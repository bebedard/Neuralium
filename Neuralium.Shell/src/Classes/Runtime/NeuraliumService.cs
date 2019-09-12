using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Neuralia.Blockchains.Core.General;
using Neuralia.Blockchains.Tools;
using Neuralium.Shell.Classes.Configuration;
using Neuralium.Shell.Resources;
using Serilog;

namespace Neuralium.Shell.Classes.Runtime {
	public interface INeuraliumService : IHostedService, IDisposable2 {
	}

	public class NeuraliumService : INeuraliumService {
		public const string SLA_ACCEPTANCE = "YES";

		protected readonly IApplicationLifetime applicationLifetime;

		protected readonly INeuraliumApp neuraliumApp;
		protected readonly NeuraliumOptions options;

		public NeuraliumService(IApplicationLifetime ApplicationLifetime, INeuraliumApp neuraliumApp, NeuraliumOptions options) {

			this.applicationLifetime = ApplicationLifetime;
			this.neuraliumApp = neuraliumApp;
			this.options = options;
		}

		public Task StartAsync(CancellationToken cancellationNeuralium) {

#if TESTNET

			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Currently in TESTNET mode");
			Console.ResetColor();

			this.CheckTestnetDelay();

			TimeSpan waitTime = TimeSpan.FromHours(1);

			this.pollingTimer = new Timer(state => {

				this.CheckTestnetDelay();

			}, this, waitTime, waitTime);

#elif NET
#elif DEVNET
			Console.BackgroundColor = ConsoleColor.Yellow;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Currently in DEVNET mode");
			Console.ResetColor();

			this.CheckDevnetDelay();

			TimeSpan waitTime = TimeSpan.FromHours(1);

			this.pollingTimer = new Timer(state => {

				this.CheckDevnetDelay();

			}, this, waitTime, waitTime);

#endif

			// lets do the legal stuff. sucks but thats how it is now...
			this.VerifySoftwareLicenseAgreement();

			Log.Information("Daemon is starting....");

			this.applicationLifetime.ApplicationStarted.Register(this.OnStarted);
			this.applicationLifetime.ApplicationStopping.Register(this.OnStopping);
			this.applicationLifetime.ApplicationStopped.Register(this.OnStopped);

			this.neuraliumApp.Error += (app, exception) => {
				Log.Error(exception, "Failed to run neuralium app. exception occured");

				Log.Information("Hit any key to exit....");

				Task task = new TaskFactory().StartNew(() => {
					Console.ReadKey();
				});

				// auto shutdown after a few seconds.
				task.Wait(1000 * 10);

				this.applicationLifetime.StopApplication();
			};

			this.neuraliumApp.Start();

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationNeuralium) {

			Log.Information("Daemon shutdown in progress...");
#if TESTNET || DEVNET
			this.pollingTimer?.Dispose();
			this.autoResetEvent.Set();
#endif

			this.neuraliumApp.Stop();
			this.neuraliumApp.WaitStop(TimeSpan.FromSeconds(20));

			this.neuraliumApp.Dispose();

			return Task.CompletedTask;
		}

		/// <summary>
		///     Verify and confirm the users has accepted the software license agreement
		/// </summary>
		protected virtual void VerifySoftwareLicenseAgreement() {

			// Display the TOS
			Log.Information(NeuraliumAppTranslationsManager.Instance.TOSPresentation);

			// now we check if the license has been accepted
			string slaFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".neuralium-sla");

			if(this.options.AcceptSoftwareLicenseAgreement == SLA_ACCEPTANCE) {
				try {
					File.WriteAllText(slaFilePath, this.options.AcceptSoftwareLicenseAgreement);
				} catch(Exception ex) {
					Log.Error(ex, $"Failed to write software license agreement file to {slaFilePath}");

					// we can keep going, this is not critical
				}
			} else {
				bool accepted = false;

				try {
					if(File.Exists(slaFilePath)) {
						if(File.ReadAllText(slaFilePath) == SLA_ACCEPTANCE) {
							accepted = true;
						}
					}
				} catch {
				}

				if(!accepted) {
					Log.Warning("Confirm your acceptance of the terms of the software license agreement. Type \"YES\" to accept.");

					string value = Console.ReadLine();

					if(value == SLA_ACCEPTANCE) {
						File.WriteAllText(slaFilePath, value);
						accepted = true;
					}
				}

				if(!accepted) {
					Log.Fatal("The Software License Agreement has not been accepted. We can not continue.");
					Environment.Exit(0);
				}
			}

			Log.Information("The software license agreement has been accepted.");
		}

		protected virtual void OnStarted() {
			Log.Information("Daemon is successfully started.");

			// Post-startup code goes here
		}

		protected virtual void OnStopping() {
			Log.Information("Daemon shutdown requested.");
		}

		protected virtual void OnStopped() {
			Log.Information("Daemon successfully stopped");
		}
#if TESTNET
		private Timer pollingTimer;

		private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
		protected virtual void CheckTestnetDelay() {
			
			//TODO: this needs review
			//TimeSpan allowDelay = TimeSpan.FromDays(5);
			//DateTime fileBuildTime = AssemblyUtils.GetBuildTimestamp(typeof(NeuraliumService));
			
			//TimeSpan allowDelay =  - fileBuildTime;
			
			//TimeSpan elapsed = DateTime.Now - ;

			var limit = new DateTime(2019, 9, 13, 23, 0, 0).ToLocalTime();
			if(DateTime.Now > limit) {
			
				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Red;
				Log.Fatal("This TESTNET release has expired! It can not be used anymore. Please download a more recent version from https://www.neuralium.com.");
			
				Environment.Exit(0);
			} else {
				TimeSpan remaining = limit - DateTime.Now;
			
				Log.Warning($"This TESTNET release is still valid for {remaining.Days} days and {remaining.Hours} hours.");
			}
		}
#elif DEVNET
		private Timer pollingTimer;

		private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
		protected virtual void CheckDevnetDelay() {
			//TimeSpan allowDelay = TimeSpan.FromDays(5);
			TimeSpan allowDelay = new DateTime(2019, 9, 4, 23, 0, 0).ToLocalTime() - DateTime.Now;

			TimeSpan elapsed = DateTime.Now - AssemblyUtils.GetBuildTimestamp(typeof(NeuraliumService));

			if(elapsed > allowDelay) {

				Console.BackgroundColor = ConsoleColor.Black;
				Console.ForegroundColor = ConsoleColor.Red;
				Log.Fatal("This DEVNET release has expired! if can not be used anymore. please download a newer version at: https://www.neuralium.com.");

				Environment.Exit(0);
			} else {
				TimeSpan remaining = allowDelay - elapsed;

				Log.Warning($"This DEVNET release is still valid for {remaining.Days} days and {remaining.Hours} hours.");
			}
		}
#endif

	#region Dispose

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {

				try {
					try {

					} catch(Exception ex) {
						Log.Verbose("error occured", ex);
					}

				} catch(Exception ex) {
					Log.Error(ex, "failed to dispose of Neuralium service");
				} finally {
					this.IsDisposed = true;
				}
			}
		}

		~NeuraliumService() {
			this.Dispose(false);
		}

	#endregion

	}
}