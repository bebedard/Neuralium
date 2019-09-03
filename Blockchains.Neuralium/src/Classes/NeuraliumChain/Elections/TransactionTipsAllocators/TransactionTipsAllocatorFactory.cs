using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Elections.TransactionTipsAllocators.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.TransactionTipsAllocationMethods;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Elections.TransactionTipsAllocators {
	public static class TransactionTipsAllocatorFactory {

		public static TransactionTipsAllocator GetTransactionTipsSelector(ITransactionTipsAllocationMethod bountyAllocationMethod) {
			if(bountyAllocationMethod.Version == TransactionTipsAllocationMethodTypes.Instance.LowestToHighest) {
				if(bountyAllocationMethod.Version == (1, 0)) {
					return new LowestToHighestTransactionTipsAllocator(bountyAllocationMethod);
				}
			}

			throw new ApplicationException("Invalid bounty allocation type");
		}
	}
}