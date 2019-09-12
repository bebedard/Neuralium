using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags {

	/// <summary>
	///     A special transaction type that is allowed to create new neuraliums
	/// </summary>
	public interface IEmittingTransaction : INeuraliumModerationTransaction {
		Amount Amount { get; set; }
		AccountId Recipient { get; set; }
	}
}