using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1.Implementations;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.General.V1;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1 {

	public interface INeuraliumStandardAccountKeyChangeTransaction : IStandardAccountKeyChangeTransaction, INeuraliumTransaction, ITipTransaction {
	}

	public class NeuraliumStandardAccountKeyChangeTransaction : StandardAccountKeyChangeTransaction, INeuraliumStandardAccountKeyChangeTransaction {

		private readonly NeuraliumTipingTransactionImplementation tipImplement = new NeuraliumTipingTransactionImplementation();

		public NeuraliumStandardAccountKeyChangeTransaction() {
			// rehydration only
		}

		public NeuraliumStandardAccountKeyChangeTransaction(byte ordinalId) : base(ordinalId) {
		}

		public Amount Tip {
			get => this.tipImplement.Tip;
			set => this.tipImplement.Tip = value;
		}

		public override HashNodeList GetStructuresArray() {
			HashNodeList nodeList = base.GetStructuresArray();

			nodeList.Add(this.tipImplement);

			return nodeList;
		}

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			base.JsonDehydrate(jsonDeserializer);

			//
			this.tipImplement.JsonDehydrate(jsonDeserializer);
		}

		protected override void RehydrateHeader(IDataRehydrator rehydrator) {
			base.RehydrateHeader(rehydrator);

			this.tipImplement.Rehydrate(rehydrator);
		}

		protected override void DehydrateHeader(IDataDehydrator dehydrator) {
			base.DehydrateHeader(dehydrator);

			this.tipImplement.Dehydrate(dehydrator);
		}
	}
}