using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures;
using Blockchains.Neuralium.Classes.NeuraliumChain.Elections;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Results.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags;
using Blockchains.Neuralium.Classes.NeuraliumChain.Tools;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Elections;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Results.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers;
using Neuralia.Blockchains.Core.General.Types;
using Serilog;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Providers {

	public interface INeuraliumChainMiningProvider : IChainMiningProvider<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	/// <summary>
	///     Provide access to the chain state that is saved in the DB
	/// </summary>
	public class NeuraliumChainMiningProvider : ChainMiningProvider<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumChainMiningProvider {

		public NeuraliumChainMiningProvider(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}

		public override bool MiningAllowed => true;

		public override BlockElectionDistillate PrepareBlockElectionContext(IBlock currentBlock, AccountId miningAccountId) {
			NeuraliumBlockElectionDistillate blockElectionDistillate = (NeuraliumBlockElectionDistillate) base.PrepareBlockElectionContext(currentBlock, miningAccountId);

			if(currentBlock is INeuraliumBlock neuraliumBlock) {

				if(neuraliumBlock is INeuraliumElectionBlock neuraliumElectionBlock) {

					if(neuraliumElectionBlock.ElectionContext is INeuraliumElectionContext neuraliumElectionContext) {

					}
				}

			}

			return blockElectionDistillate;
		}

		protected override IElectionProcessorFactory<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> GetElectionProcessorFactory() {
			return new NeuraliumElectionProcessorFactory();
		}

		protected override BlockElectionDistillate CreateBlockElectionContext() {
			return new NeuraliumBlockElectionDistillate();
		}

		protected override void PreparePassiveElectionContext(long currentBlockId, AccountId miningAccountId, PassiveElectionContextDistillate intermediaryResultEntry, IIntermediaryElectionResults intermediaryElectionResult, IBlock currentBlock) {
			base.PreparePassiveElectionContext(currentBlockId, miningAccountId, intermediaryResultEntry, intermediaryElectionResult, currentBlock);

			if(intermediaryElectionResult is INeuraliumIntermediaryElectionResults neuraliumSimpleIntermediaryElectionResults && intermediaryResultEntry is NeuraliumPassiveElectionContextDistillate neuraliumIntermediaryElectionContext) {

			}
		}

		protected override void PrepareFinalElectionContext(long currentBlockId, AccountId miningAccountId, FinalElectionResultDistillate finalResultDistillateEntry, IFinalElectionResults finalElectionResult, IBlock currentBlock) {
			base.PrepareFinalElectionContext(currentBlockId, miningAccountId, finalResultDistillateEntry, finalElectionResult, currentBlock);

			if(finalElectionResult is INeuraliumFinalElectionResults neuraliumSimpleFinalElectionResults && finalResultDistillateEntry is NeuraliumFinalElectionResultDistillate neuraliumFinalElectionContext) {
				if(neuraliumSimpleFinalElectionResults.ElectedCandidates[miningAccountId] is INeuraliumElectedResults neuraliumElectedResults) {

					neuraliumFinalElectionContext.BountyShare = neuraliumElectedResults.BountyShare;
					var tippingTransactions = currentBlock.GetAllConfirmedTransactions().Where(t => neuraliumElectedResults.Transactions.Contains(t.Key)).Select(t => t.Value).OfType<ITipTransaction>().Select(t => t.Tip).ToList();

					if(tippingTransactions.Any()) {
						neuraliumFinalElectionContext.TransactionTips = tippingTransactions.Sum(t => t);
					}
				}
			}
		}

		protected override ElectedCandidateResultDistillate CreateElectedCandidateResult() {
			return new NeuraliumElectedCandidateResultDistillate();
		}

		protected override void ConfirmedPrimeElected(BlockElectionDistillate blockElectionDistillate, FinalElectionResultDistillate finalElectionResultDistillate) {

			base.ConfirmedPrimeElected(blockElectionDistillate, finalElectionResultDistillate);

			NeuraliumBlockElectionDistillate neuraliumBlockElectionDistillate = (NeuraliumBlockElectionDistillate) blockElectionDistillate;

			NeuraliumFinalElectionResultDistillate neuraliumFinalElectionContext = (NeuraliumFinalElectionResultDistillate) finalElectionResultDistillate;

			this.centralCoordinator.PostSystemEvent(NeuraliumSystemEventGenerator.NeuraliumMiningPrimeElected(blockElectionDistillate.currentBlockId, neuraliumFinalElectionContext.BountyShare, neuraliumFinalElectionContext.TransactionTips, AccountId.FromString(neuraliumFinalElectionContext.DelegateAccountId)));

			Log.Information($"We were officially announced as a prime elected in Block {blockElectionDistillate.currentBlockId} for the election that was announced in block {blockElectionDistillate.currentBlockId - neuraliumFinalElectionContext.BlockOffset}");
		}

		protected override MiningHistoryEntry CreateMiningHistoryEntry() {
			return new NeuraliumMiningHistoryEntry();
		}

		protected override MiningHistoryEntry PrepareMiningHistoryEntry(BlockElectionDistillate blockElectionDistillate, FinalElectionResultDistillate finalElectionResultDistillate) {
			MiningHistoryEntry entry = base.PrepareMiningHistoryEntry(blockElectionDistillate, finalElectionResultDistillate);

			if(entry is NeuraliumMiningHistoryEntry neuraliumMiningHistoryEntry && finalElectionResultDistillate is NeuraliumFinalElectionResultDistillate neuraliumFinalElectionContext) {
				neuraliumMiningHistoryEntry.BountyShare = neuraliumFinalElectionContext.BountyShare;
				neuraliumMiningHistoryEntry.TransactionTips = neuraliumFinalElectionContext.TransactionTips;
			}

			return entry;
		}
	}

	public class NeuraliumMiningHistoryEntry : MiningHistoryEntry {
		public decimal BountyShare { get; set; }
		public decimal TransactionTips { get; set; }

		public override MiningHistory ToApiHistory() {
			MiningHistory entry = base.ToApiHistory();

			if(entry is NeuraliumMiningHistory neuraliumMiningHistory) {
				neuraliumMiningHistory.BountyShare = this.BountyShare;
				neuraliumMiningHistory.TransactionTips = this.TransactionTips;
			}

			return entry;
		}

		public override MiningHistory CreateApiMiningHistory() {
			return new NeuraliumMiningHistory();
		}
	}

}