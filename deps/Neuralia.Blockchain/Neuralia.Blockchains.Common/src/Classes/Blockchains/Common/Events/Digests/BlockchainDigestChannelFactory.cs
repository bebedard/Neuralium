using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Specialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests {
	public interface IBlockchainDigestChannelFactory {

		IDigestChannel CreateCreateDigestChannels(BlockchainDigestSimpleChannelDescriptor channelDescriptor, string folder);

		T CreateCreateDigestChannels<T>(BlockchainDigestSimpleChannelDescriptor channelDescriptor, string folder)
			where T : class, IDigestChannel;

		IStandardAccountKeysDigestChannel CreateStandardAccountKeysDigestChannel(int groupSize, string folder);

		IStandardAccountSnapshotDigestChannel CreateStandardAccountSnapshotDigestChannel(int groupSize, string folder);
		IJointAccountSnapshotDigestChannel CreateJointAccountSnapshotDigestChannel(int groupSize, string folder);

		IAccreditationCertificateDigestChannel CreateAccreditationCertificateDigestChannel(string folder);

		IChainOptionsDigestChannel CreateChainOptionsDigestChannel(string folder);
	}

	public abstract class BlockchainDigestChannelFactory : IBlockchainDigestChannelFactory {

		public virtual IDigestChannel CreateCreateDigestChannels(BlockchainDigestSimpleChannelDescriptor channelDescriptor, string folder) {
			return this.CreateCreateDigestChannels<IDigestChannel>(channelDescriptor, folder);
		}

		public virtual T CreateCreateDigestChannels<T>(BlockchainDigestSimpleChannelDescriptor channelDescriptor, string folder)
			where T : class, IDigestChannel {
			if(channelDescriptor.Version == DigestChannelTypes.Instance.StandardAccountSnapshot) {
				if(channelDescriptor.Version == (1, 0)) {
					return (T) this.CreateStandardAccountSnapshotDigestChannel(channelDescriptor.GroupSize, folder);
				}
			}

			if(channelDescriptor.Version == DigestChannelTypes.Instance.JointAccountSnapshot) {
				if(channelDescriptor.Version == (1, 0)) {
					return (T) this.CreateJointAccountSnapshotDigestChannel(channelDescriptor.GroupSize, folder);
				}
			}

			if(channelDescriptor.Version == DigestChannelTypes.Instance.StandardAccountKeys) {
				if(channelDescriptor.Version == (1, 0)) {
					return (T) this.CreateStandardAccountKeysDigestChannel(channelDescriptor.GroupSize, folder);
				}
			}

			if(channelDescriptor.Version == DigestChannelTypes.Instance.AccreditationCertificates) {
				if(channelDescriptor.Version == (1, 0)) {
					return (T) this.CreateAccreditationCertificateDigestChannel(folder);
				}
			}

			if(channelDescriptor.Version == DigestChannelTypes.Instance.ChainOptions) {
				if(channelDescriptor.Version == (1, 0)) {
					return (T) this.CreateChainOptionsDigestChannel(folder);
				}
			}

			return null;
		}

		public abstract IStandardAccountKeysDigestChannel CreateStandardAccountKeysDigestChannel(int groupSize, string folder);

		public abstract IStandardAccountSnapshotDigestChannel CreateStandardAccountSnapshotDigestChannel(int groupSize, string folder);
		public abstract IJointAccountSnapshotDigestChannel CreateJointAccountSnapshotDigestChannel(int groupSize, string folder);

		public abstract IAccreditationCertificateDigestChannel CreateAccreditationCertificateDigestChannel(string folder);

		public abstract IChainOptionsDigestChannel CreateChainOptionsDigestChannel(string folder);
	}
}