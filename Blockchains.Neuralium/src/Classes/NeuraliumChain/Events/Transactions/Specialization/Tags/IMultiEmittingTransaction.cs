using System.Collections.Generic;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.Structures;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags {

	/// <summary>
	///     A special transaction type that is allowed to create new neuraliums
	/// </summary>
	public interface IMultiEmittingTransaction : INeuraliumModerationTransaction {
		List<RecipientSet> Recipients { get; }
	}
}