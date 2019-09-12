using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags {

	/// <summary>
	///     if transactions support transaction fees, this is the interface to go to
	/// </summary>
	public interface ITipTransaction {
		Amount Tip { get; set; }
	}
}