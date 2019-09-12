using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization.Blockchain.Utils;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization.Moderator.V1;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.Serialization;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator.V1 {
	public interface INeuraliumChainSettingsTransaction : IChainOperatingRulesTransaction, INeuraliumModerationTransaction {
		Amount SAFUDailyRatio { get; set; }
	}

	public class NeuraliumChainOperatingRulesTransaction : ChainOperatingRulesTransaction, INeuraliumChainSettingsTransaction {

		/// <summary>
		///     this is the amount of neuraliums that must be paid to be part of the SAFU for 1 day and be able to protect 1
		///     neuralium.
		/// </summary>
		public Amount SAFUDailyRatio { get; set; } = new Amount();

		public override HashNodeList GetStructuresArray() {

			HashNodeList nodeList = base.GetStructuresArray();

			nodeList.Add(this.SAFUDailyRatio);

			return nodeList;
		}

		public override void JsonDehydrate(JsonDeserializer jsonDeserializer) {
			base.JsonDehydrate(jsonDeserializer);

			//
			jsonDeserializer.SetProperty("SAFUDailyRatio", this.SAFUDailyRatio);
		}

		protected override ComponentVersion<TransactionType> SetIdentity() {
			return (base.SetIdentity().Type.Value, 1, 0);
		}

		protected override void RehydrateContents(ChannelsEntries<IDataRehydrator> dataChannels, ITransactionRehydrationFactory rehydrationFactory) {
			base.RehydrateContents(dataChannels, rehydrationFactory);

			this.SAFUDailyRatio.Rehydrate(dataChannels.ContentsData);
		}

		protected override void DehydrateContents(ChannelsEntries<IDataDehydrator> dataChannels) {
			base.DehydrateContents(dataChannels);

			this.SAFUDailyRatio.Dehydrate(dataChannels.ContentsData);
		}
	}
}