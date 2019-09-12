using Neuralia.Blockchains.Core.General.Versions;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.TransactionTipsAllocationMethods {
	public interface ITransactionTipsAllocationMethod : IVersionable<TransactionTipsAllocationMethodType> {
	}

	/// <summary>
	///     By what method do we allocate the bounty
	/// </summary>
	public abstract class TransactionTipsAllocationMethod : Versionable<TransactionTipsAllocationMethodType>, ITransactionTipsAllocationMethod {
	}
}