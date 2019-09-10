using System.Collections.Generic;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.TransactionTipsAllocationMethods;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Results.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Tools.Data;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Elections.TransactionTipsAllocators {
	public abstract class TransactionTipsAllocator {

		protected readonly ITransactionTipsAllocationMethod TransactionTipsAllocationMethod;

		public TransactionTipsAllocator(ITransactionTipsAllocationMethod TransactionTipsAllocationMethod) {
			this.TransactionTipsAllocationMethod = TransactionTipsAllocationMethod;
		}

		public abstract void AllocateTransactionTips(IFinalElectionResults result, Dictionary<AccountId, (IByteArray electionHash, List<TransactionId> transactionIds)> electionResults, Dictionary<TransactionId, Amount> TransactionTip);
	}
}