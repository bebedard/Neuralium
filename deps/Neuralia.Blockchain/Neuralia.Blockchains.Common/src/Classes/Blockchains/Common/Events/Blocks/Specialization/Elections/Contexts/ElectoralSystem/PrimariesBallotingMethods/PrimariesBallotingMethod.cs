using System;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Dynamic;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Contexts.ElectoralSystem.PrimariesBallotingMethods {
	public interface IPrimariesBallotingMethod : IVersionable<PrimariesBallotingMethodType>, IJsonSerializable {

		AdaptiveLong1_9 Difficulty { get; set; }
		IByteArray PerformBallot(IByteArray candidature, BlockElectionDistillate blockElectionDistillate, AccountId miningAccount);
	}

	/// <summary>
	///     Different election methodologies to determine who gets to be elected in the primaries
	/// </summary>
	public abstract class PrimariesBallotingMethod : Versionable<PrimariesBallotingMethodType>, IPrimariesBallotingMethod {

		public AdaptiveLong1_9 Difficulty { get; set; } = new AdaptiveLong1_9();

		public abstract IByteArray PerformBallot(IByteArray candidature, BlockElectionDistillate blockElectionDistillate, AccountId miningAccount);

		public override void Dehydrate(IDataDehydrator dehydrator) {
			throw new NotSupportedException();
		}

		public override void Rehydrate(IDataRehydrator rehydrator) {
			base.Rehydrate(rehydrator);

			this.Difficulty.Rehydrate(rehydrator);
		}

		public override HashNodeList GetStructuresArray() {
			HashNodeList nodeList = base.GetStructuresArray();

			nodeList.Add(this.Difficulty);

			return nodeList;
		}
	}
}