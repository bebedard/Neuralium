using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Digests.Channels.Specialization.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Specialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Digests.Channels.Specialization {

	public interface INeuraliumStandardAccountKeysDigestChannel : IStandardAccountKeysDigestChannel, INeuraliumDigestChannel {
	}

	public class NeuraliumStandardAccountKeysDigestChannel : StandardAccountKeysDigestChannel<NeuraliumStandardAccountKeysDigestChannelCard>, INeuraliumStandardAccountKeysDigestChannel {
		[Flags]
		public enum AccountKeysDigestChannelBands {
			Wallets
		}

		public NeuraliumStandardAccountKeysDigestChannel(int groupSize, string folder) : base(groupSize, folder) {
		}

		protected override NeuraliumStandardAccountKeysDigestChannelCard CreateNewCardInstance() {
			return new NeuraliumStandardAccountKeysDigestChannelCard();
		}
	}
}