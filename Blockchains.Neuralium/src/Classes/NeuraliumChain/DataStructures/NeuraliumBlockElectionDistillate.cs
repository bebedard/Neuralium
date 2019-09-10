using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures {

	public class NeuraliumBlockElectionDistillate : BlockElectionDistillate {

		public override PassiveElectionContextDistillate CreatePassiveElectionContext() {
			return new NeuraliumPassiveElectionContextDistillate();
		}

		public override FinalElectionResultDistillate CreateFinalElectionResult() {
			return new NeuraliumFinalElectionResultDistillate();
		}
	}

	public class NeuraliumPassiveElectionContextDistillate : PassiveElectionContextDistillate {
	}

	public class NeuraliumFinalElectionResultDistillate : FinalElectionResultDistillate {
		public decimal BountyShare;
		public decimal TransactionTips;
	}

	public class NeuraliumElectedCandidateResultDistillate : ElectedCandidateResultDistillate {
	}
}