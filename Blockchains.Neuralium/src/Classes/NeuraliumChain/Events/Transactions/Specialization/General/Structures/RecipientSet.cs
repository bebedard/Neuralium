using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.Structures {
	public struct RecipientSet {
		public AccountId Recipient { get; set; }
		public Amount Amount { get; set; }
	}

}