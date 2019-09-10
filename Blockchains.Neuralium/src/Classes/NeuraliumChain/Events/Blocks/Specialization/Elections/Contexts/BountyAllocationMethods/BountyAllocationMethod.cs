using Neuralia.Blockchains.Core.General.Versions;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.BountyAllocationMethods {
	public interface IBountyAllocationMethod : IVersionable<BountyAllocationMethodType> {
	}

	/// <summary>
	///     By what method do we allocate the bounty
	/// </summary>
	public abstract class BountyAllocationMethod : Versionable<BountyAllocationMethodType>, IBountyAllocationMethod {
	}
}