using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization {

	public interface INeuraliumElectionBlock : IElectionBlock, INeuraliumBlock {
	}

	public class NeuraliumElectionBlock : ElectionBlock, INeuraliumElectionBlock {
	}
}