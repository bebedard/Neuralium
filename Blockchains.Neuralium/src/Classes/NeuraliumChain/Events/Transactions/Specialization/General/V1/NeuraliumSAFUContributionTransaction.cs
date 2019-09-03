using System;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1 {

	public interface INeuraliumSAFUContributionTransaction : INeuraliumTipingTransaction {
		Amount Total { get; set; }
		Amount DailyProtection { get; set; }
		DateTime? Start { get; set; }
		bool AcceptSAFUTermsOfService { get; set; }
	}

	public class NeuraliumSAFUContributionTransaction : NeuraliumTipingTransaction, INeuraliumSAFUContributionTransaction {

		public bool AcceptSAFUTermsOfService { get; set; }
		public Amount Total { get; set; } = new Amount();
		public Amount DailyProtection { get; set; } = new Amount();

		public DateTime? Start { get; set; }

		public override HashNodeList GetStructuresArray() {
			HashNodeList nodeList = base.GetStructuresArray();

			nodeList.Add(this.AcceptSAFUTermsOfService);
			nodeList.Add(this.Total);
			nodeList.Add(this.DailyProtection);
			nodeList.Add(this.Start);

			return nodeList;
		}

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			base.JsonDehydrate(jsonDeserializer);

			//
			jsonDeserializer.SetProperty("AcceptSAFUTermsOfService", this.AcceptSAFUTermsOfService);
			jsonDeserializer.SetProperty("Total", this.Total.Value);
			jsonDeserializer.SetProperty("DailyProtection", this.DailyProtection.Value);
			jsonDeserializer.SetProperty("Start", this.Start);
		}

		protected override ComponentVersion<TransactionType> SetIdentity() {
			return (TOKEN_TRANSFER: NeuraliumTransactionTypes.Instance.NEURALIUM_SAFU_CONTRIBUTIONS, 1, 0);
		}

		protected override void RehydrateHeader(IDataRehydrator rehydrator) {
			base.RehydrateHeader(rehydrator);

			this.AcceptSAFUTermsOfService = rehydrator.ReadBool();
			this.Total.Rehydrate(rehydrator);
			this.DailyProtection.Rehydrate(rehydrator);
			this.Start = rehydrator.ReadNullableDateTime();

		}

		protected override void DehydrateHeader(IDataDehydrator dehydrator) {
			base.DehydrateHeader(dehydrator);

			dehydrator.Write(this.AcceptSAFUTermsOfService);
			this.Total.Dehydrate(dehydrator);
			this.DailyProtection.Dehydrate(dehydrator);
			dehydrator.Write(this.Start);
		}
	}

}