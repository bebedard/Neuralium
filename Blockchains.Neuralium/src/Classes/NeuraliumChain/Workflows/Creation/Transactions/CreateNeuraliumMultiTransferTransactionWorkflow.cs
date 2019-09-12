using System;
using System.Collections.Generic;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.Structures;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.Validation;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Creation.Transactions;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Tools.Data;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Creation.Transactions {
	public interface ICreateNeuraliumMultiTransferTransactionWorkflow : IGenerateNewTransactionWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class CreateNeuraliumMultiTransferTransactionWorkflow : GenerateNewTransactionWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider, INeuraliumAssemblyProvider>, ICreateNeuraliumMultiTransferTransactionWorkflow {
		private readonly Guid accountUuid;
		private readonly Amount amount;
		private readonly TransactionId guid;
		private readonly IByteArray hash;

		private readonly List<RecipientSet> recipients;
		private readonly long targetAccountId;
		private readonly Amount tip;

		public CreateNeuraliumMultiTransferTransactionWorkflow(Guid accountUuid, List<RecipientSet> recipients, Amount tip, string note, INeuraliumCentralCoordinator centralCoordinator, CorrelationContext correlationContext) : base(centralCoordinator, note, correlationContext) {
			this.accountUuid = accountUuid;
			this.recipients = recipients;
			this.tip = tip;
		}

		protected override ValidationResult ValidateContents(ITransactionEnvelope envelope) {
			ValidationResult result = base.ValidateContents(envelope);

			if(result.Invalid) {
				return result;
			}

			return NeuraliumTransactionCreationUtils.ValidateTransaction(envelope.Contents.RehydratedTransaction);
		}

		protected override ITransactionEnvelope AssembleEvent() {
			return this.centralCoordinator.ChainComponentProvider.AssemblyProvider.GenerateNeuraliumMultiTransferTransaction(this.accountUuid, this.recipients, this.tip, this.correlationContext);
		}
	}
}