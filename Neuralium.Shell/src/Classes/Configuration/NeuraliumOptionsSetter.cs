using Blockchains.Neuralium.Classes;
using Neuralia.Blockchains.Core.Configuration;

namespace Neuralium.Shell.Classes.Configuration {
	public class NeuraliumOptionsSetter<A, O> : OptionsSetter<A, O>
		where A : AppSettings
		where O : NeuraliumOptions {

		public override void SetOptions(A appSettings, O cmdOptions) {
			base.SetOptions(appSettings, cmdOptions);

			if(cmdOptions == null) {
				return;
			}

			if(cmdOptions.NoP2p) {
				appSettings.DisableP2p = cmdOptions.NoP2p;
			}

			if(cmdOptions.NoTimeServer) {
				appSettings.DisableTimeServer = cmdOptions.NoTimeServer;
			}

			if(cmdOptions.NoRPC) {
				appSettings.RpcMode = AppSettingsBase.RpcModes.None;
			}

			if(cmdOptions.SkipGenesisHashVerification) {
				appSettings.GetChainConfiguration(NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium).SkipGenesisHashVerification = cmdOptions.SkipGenesisHashVerification;
			}

			if(cmdOptions.SkipDigestHashVerification) {
				appSettings.GetChainConfiguration(NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium).SkipDigestHashVerification = cmdOptions.SkipDigestHashVerification;
			}

			if(cmdOptions.NoNeuraliumsChain) {
				appSettings.NeuraliumChainConfiguration.Enabled = cmdOptions.NoNeuraliumsChain;
			}

			if(cmdOptions.Port.HasValue) {
				appSettings.port = cmdOptions.Port.Value;
			}

            if (cmdOptions.IpAdrress != null)
            {
                appSettings.ip = cmdOptions.IpAdrress;
            }

            if (!string.IsNullOrWhiteSpace(cmdOptions.SerializationType)) {
				if(cmdOptions.SerializationType.ToUpper() == "MASTER") {
					appSettings.SerializationType = AppSettingsBase.SerializationTypes.Master;
				} else if(cmdOptions.SerializationType.ToUpper() == "FEEDER") {
					appSettings.SerializationType = AppSettingsBase.SerializationTypes.Feeder;
				}
			}
		}
	}

	public class NeuraliumOptionsSetter : NeuraliumOptionsSetter<AppSettings, NeuraliumOptions> {
	}
}