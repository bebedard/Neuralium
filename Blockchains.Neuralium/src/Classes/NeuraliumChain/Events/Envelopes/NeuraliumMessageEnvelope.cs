using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Envelopes {
	public interface INeuraliumMessageEnvelope : IMessageEnvelope {
	}

	public class NeuraliumMessageEnvelope : MessageEnvelope, INeuraliumMessageEnvelope {
	}
}