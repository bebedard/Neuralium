using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Versions;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.BountyAllocationMethods.V1 {
	/// <summary>
	///     A simple but fair allocation method where the bounty is split equally among all prime elected. Various peer types
	///     get various allocation percentages.
	/// </summary>
	public class EqualSplitBountyAllocationMethod : BountyAllocationMethod {

		protected override ComponentVersion<BountyAllocationMethodType> SetIdentity() {
			return (BountyAllocationMethodTypes.Instance.EqualSplit, 1, 0);
		}

		public override HashNodeList GetStructuresArray() {
			HashNodeList nodeList = base.GetStructuresArray();

			return nodeList;
		}
	}
}