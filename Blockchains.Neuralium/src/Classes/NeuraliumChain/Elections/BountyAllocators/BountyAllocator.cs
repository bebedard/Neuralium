using System.Collections.Generic;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.BountyAllocationMethods;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Results.V1;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.General.Types;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Elections.BountyAllocators {
	public abstract class BountyAllocator {

		protected readonly IBountyAllocationMethod BountyAllocationMethod;

		public BountyAllocator(IBountyAllocationMethod BountyAllocationMethod) {
			this.BountyAllocationMethod = BountyAllocationMethod;
		}

		public BountyAllocator() {

		}

		public abstract void AllocateBounty(INeuraliumFinalElectionResults result, INeuraliumElectionContext electionContext, Dictionary<AccountId, (Enums.ElectedPeerShareTypes peerType, AccountId delegateAccountId)> electedPeers, Dictionary<AccountId, (decimal delegateBountyShare, decimal InfrastructureServiceFees)> delegateAllocations);
	}
}