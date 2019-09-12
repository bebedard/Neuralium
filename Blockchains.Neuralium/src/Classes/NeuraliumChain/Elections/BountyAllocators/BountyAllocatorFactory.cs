using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Elections.BountyAllocators.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.BountyAllocationMethods;
using Neuralia.Blockchains.Core.General.Versions;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Elections.BountyAllocators {
	public static class BountyAllocatorFactory {

		public static BountyAllocator GetRepresentativeSelector(IBountyAllocationMethod bountyAllocationMethod) {
			if(bountyAllocationMethod.Version == BountyAllocationMethodTypes.Instance.EqualSplit) {
				if(bountyAllocationMethod.Version == (1, 0)) {
					return new EqualSplitBountyAllocator(bountyAllocationMethod);
				}
			}

			throw new ApplicationException("Invalid bounty allocation type");
		}

		public static BountyAllocator GetRepresentativeSelector(string bountyAllocationMethodType) {
			var version = new ComponentVersion<BountyAllocationMethodType>(bountyAllocationMethodType);

			if(version == BountyAllocationMethodTypes.Instance.EqualSplit) {
				if(version == (1, 0)) {
					return new EqualSplitBountyAllocator();
				}
			}

			throw new ApplicationException("Invalid bounty allocation type");
		}
	}
}