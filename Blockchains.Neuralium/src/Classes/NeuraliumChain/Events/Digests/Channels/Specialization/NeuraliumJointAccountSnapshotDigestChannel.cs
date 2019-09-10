using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Digests.Channels.Specialization.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Specialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Digests.Channels.Specialization {
	public interface INeuraliumJointAccountSnapshotDigestChannel : IJointAccountSnapshotDigestChannel, INeuraliumDigestChannel {
	}

	public class NeuraliumJointAccountSnapshotDigestChannel : JointAccountSnapshotDigestChannel<INeuraliumJointAccountSnapshotDigestChannelCard>, INeuraliumJointAccountSnapshotDigestChannel {
		[Flags]
		public enum AccountSnapshotDigestChannelBands {
			Wallets
		}

		public NeuraliumJointAccountSnapshotDigestChannel(int groupSize, string folder) : base(groupSize, folder) {
		}

		protected override INeuraliumJointAccountSnapshotDigestChannelCard CreateNewCardInstance() {
			return new NeuraliumJointAccountSnapshotDigestChannelCard();
		}
	}
}