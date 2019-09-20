using MessagePack;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures {

	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumBlockElectionDistillate : BlockElectionDistillate {

		public override IntermediaryElectionContextDistillate CreateIntermediateElectionContext() {
			return new NeuraliumIntermediaryElectionContextDistillate();
		}
		
		public override PassiveElectionContextDistillate CreatePassiveElectionContext() {
			return new NeuraliumPassiveElectionContextDistillate();
		}

		public override FinalElectionResultDistillate CreateFinalElectionResult() {
			return new NeuraliumFinalElectionResultDistillate();
		}
	}

	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumPassiveElectionContextDistillate : PassiveElectionContextDistillate {
	}

	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumFinalElectionResultDistillate : FinalElectionResultDistillate {
		public decimal BountyShare;
		public decimal TransactionTips;
	}

	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumElectedCandidateResultDistillate : ElectedCandidateResultDistillate {
	}

	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumIntermediaryElectionContextDistillate : IntermediaryElectionContextDistillate {
		
	}
}