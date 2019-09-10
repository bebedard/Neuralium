using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neuralia.Blockchains.Core.Configuration;
using Neuralium.Shell.Classes.Configuration;
using Neuralium.Shell.Controllers;
using Newtonsoft.Json.Serialization;

namespace Neuralium.Shell.Classes.Runtime {
	public class Startup<RPC_HUB, RCP_CLIENT>
		where RPC_HUB : RpcHub<RCP_CLIENT>
		where RCP_CLIENT : class, IRpcClient {

		private readonly AppSettings appSettings;

		public Startup(IConfiguration configuration, IOptions<AppSettings> appSettings) {
			this.Configuration = configuration;
			this.appSettings = appSettings.Value;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			//https://www.meziantou.net/2017/06/19/minimal-asp-net-core-web-api-project

			//webapi RPC:
			if(this.appSettings.RpcMode.HasFlag(AppSettingsBase.RpcModes.Rest)) {
				services.AddMvcCore().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)

					//.AddApiExplorer()     // Optional (Microsoft.AspNetCore.Mvc.ApiExplorer)
					//.AddAuthorization()   // Optional if no authentication
					.AddFormatterMappings()

					//.AddDataAnnotations() // Optional if no validation using attributes (Microsoft.AspNetCore.Mvc.DataAnnotations)
					.AddJsonFormatters();

				//.AddCors()            // Optional (Microsoft.AspNetCore.Mvc.Cors)
			}

			if(this.appSettings.RpcMode.HasFlag(AppSettingsBase.RpcModes.Signal)) {
				services.AddSignalR(hubOptions => {
					//hubOptions.SupportedProtocols.Clear();

#if TESTNET || DEVNET
					hubOptions.EnableDetailedErrors = true;
#endif
					hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
					hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
				}).AddJsonProtocol(options => {
					options.PayloadSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
				});
			}

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {

			if(this.appSettings.RpcMode.HasFlag(AppSettingsBase.RpcModes.Signal)) {
				app.UseSignalR(routes => {
					routes.MapHub<RPC_HUB>("/signal", option => {
						option.ApplicationMaxBufferSize = 0;
						option.TransportMaxBufferSize = 0;
					});
				});
			}

			if(this.appSettings.RpcTransport == AppSettingsBase.RpcTransports.Secured) {
				app.UseHttpsRedirection();
			}

			//webapi RPC:
			if((this.appSettings.RpcMode == AppSettingsBase.RpcModes.Rest) || (this.appSettings.RpcMode == AppSettingsBase.RpcModes.Both)) {

				if(env.IsDevelopment()) {
					app.UseDeveloperExceptionPage();
				}

				app.UseMvc();
			}
		}
	}
}