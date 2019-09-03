using System.Collections.Generic;
using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.BountyAllocationMethods;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Results.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Tools;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Results.V1;
using Neuralia.Blockchains.Core;
using Neuralia.Blockchains.Core.General.Types;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Elections.BountyAllocators.V1 {
	public class EqualSplitBountyAllocator : BountyAllocator {

		public const decimal FULL_PEER_ALLOCATION = 1; // 100%
		public const decimal WEAK_PEER_ALLOCATION = 0.33M; // 33%

		public EqualSplitBountyAllocator(IBountyAllocationMethod BountyAllocationMethod) : base(BountyAllocationMethod) {
		}

		public EqualSplitBountyAllocator() {
		}

		public override void AllocateBounty(INeuraliumFinalElectionResults result, INeuraliumElectionContext electionContext, Dictionary<AccountId, (Enums.PeerTypes peerType, AccountId delegateAccountId)> electedPeers, Dictionary<AccountId, (decimal delegateBountyShare, decimal InfrastructureServiceFees)> delegateAllocations) {

			int electedCount = result.ElectedCandidates.Count;

			if(electedCount == 0) {
				// if there are no elected, then nothing is allocated; nothing at all and the bounty is lost to everyone
				return;
			}

			decimal effectiveServiceFees = 0;

			if(electionContext.MaintenanceServiceFeesEnabled) {
				effectiveServiceFees = electionContext.Bounty * electionContext.MaintenanceServiceFees.Percentage;
			}

			decimal effectiveBounty = electionContext.Bounty - effectiveServiceFees;

			// assign the fees to the special network service fees account, if any and ONLY if enabled of course.
			result.InfrastructureServiceFees = 0;

			if(electionContext.MaintenanceServiceFeesEnabled && (effectiveServiceFees > 0)) {
				result.InfrastructureServiceFees = effectiveServiceFees;
			}

			// divide it equally among all
			decimal individualBounty = effectiveBounty / electedCount;

			decimal reallocatePool = 0;

			var weakerPeers = result.ElectedCandidates.Where(e => !Enums.CompletePeerTypes.Contains(e.Value.PeerType)).ToList();
			var strongerPeers = result.ElectedCandidates.Where(e => Enums.CompletePeerTypes.Contains(e.Value.PeerType)).ToList();

			// now allocate weaker peers with their lesser part of the bounty

			void AllocateBounty(KeyValuePair<AccountId, IElectedResults> elected, decimal adjustedBounty) {
				INeuraliumElectedResults electedResultsEntry = (INeuraliumElectedResults) result.ElectedCandidates[elected.Key];

				if((electedResultsEntry.DelegateAccountId == null) || !delegateAllocations.ContainsKey(electedResultsEntry.DelegateAccountId)) {
					electedResultsEntry.BountyShare = NeuraliumUtilities.RoundNeuraliumsPrecision(adjustedBounty);

				} else {
					// assign it to the delegate account
					if(!result.DelegateAccounts.ContainsKey(electedResultsEntry.DelegateAccountId)) {

						result.DelegateAccounts.Add(electedResultsEntry.DelegateAccountId, result.CreateDelegateResult());
					}

					decimal infrastructureFees = delegateAllocations[electedResultsEntry.DelegateAccountId].InfrastructureServiceFees;
					decimal allocation = delegateAllocations[electedResultsEntry.DelegateAccountId].delegateBountyShare;

					decimal infrastructureFeesBountyShare = adjustedBounty * infrastructureFees;
					result.InfrastructureServiceFees += infrastructureFeesBountyShare;

					decimal delegateBountyShare = NeuraliumUtilities.RoundNeuraliumsPrecision(adjustedBounty * allocation);
					((INeuraliumDelegateResults) result.DelegateAccounts[electedResultsEntry.DelegateAccountId]).BountyShare += delegateBountyShare;

					// the account gets the rest
					electedResultsEntry.BountyShare = NeuraliumUtilities.RoundNeuraliumsPrecision(adjustedBounty - (delegateBountyShare + infrastructureFeesBountyShare));
				}
			}

			foreach(var elected in weakerPeers) {
				decimal adjustedBounty = individualBounty * WEAK_PEER_ALLOCATION;
				reallocatePool += individualBounty - adjustedBounty;

				AllocateBounty(elected, adjustedBounty);
			}

			decimal strongerBounty = 0;
			int strongPeerCount = strongerPeers.Count();

			if(strongPeerCount > 0) {
				strongerBounty = individualBounty + (reallocatePool / strongPeerCount);
			}

			foreach(var elected in strongerPeers) {

				AllocateBounty(elected, strongerBounty);
			}

		}
	}
}