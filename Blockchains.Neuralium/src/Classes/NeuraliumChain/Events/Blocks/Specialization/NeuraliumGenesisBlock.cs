using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Genesis;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization {

	public interface INeuraliumGenesisBlock : IGenesisBlock, INeuraliumBlock {
	}

	public class NeuraliumGenesisBlock : GenesisBlock, INeuraliumGenesisBlock {
	}
}