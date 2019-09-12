using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Models;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Creation.Messages.Elections;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.General.Types;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Creation.Messages.Elections {

	public interface INeuraliumSendElectionsRegistrationMessageWorkflow : ISendElectionsRegistrationMessageWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumSendElectionsRegistrationMessageWorkflow : SendElectionsRegistrationMessageWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider, INeuraliumAssemblyProvider>, INeuraliumSendElectionsRegistrationMessageWorkflow {

		public NeuraliumSendElectionsRegistrationMessageWorkflow(AccountId candidateAccountID, ElectionsCandidateRegistrationInfo electionsCandidateRegistrationInfo, ChainConfigurations.RegistrationMethods registrationMethod, INeuraliumCentralCoordinator centralCoordinator, CorrelationContext correlationContext) : base(candidateAccountID, electionsCandidateRegistrationInfo, registrationMethod, centralCoordinator, correlationContext) {
		}
	}
}