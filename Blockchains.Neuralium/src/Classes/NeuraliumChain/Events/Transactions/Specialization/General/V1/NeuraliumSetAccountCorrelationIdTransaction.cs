using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.General.V1;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1 {

	public interface INeuraliumSetAccountCorrelationIdTransaction : ISetAccountCorrelationIdTransaction, INeuraliumTransaction {
	}

	public class NeuraliumSetAccountCorrelationIdTransaction : SetAccountCorrelationIdTransaction, INeuraliumSetAccountCorrelationIdTransaction {
	}
}