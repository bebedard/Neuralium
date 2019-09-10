using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Simple;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization {

	public interface INeuraliumSimpleBlock : ISimpleBlock, INeuraliumBlock {
	}

	public class NeuraliumSimpleBlock : SimpleBlock, INeuraliumSimpleBlock {
	}
}