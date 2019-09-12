using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Envelopes {

	public interface INeuraliumTransactionEnvelope : ITransactionEnvelope {
	}

	public class NeuraliumTransactionEnvelope : TransactionEnvelope, INeuraliumTransactionEnvelope {
	}
}