using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.ChainSync.Messages.V1.Structures;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.ChainSync.Messages.V1.Tags;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Core.General.Types.Simple;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.P2p.Messages.Base;
using Neuralia.Blockchains.Core.Workflows;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.ChainSync.Messages.V1.Digest {
	public class ServerSendDigestFile : NetworkMessage<IBlockchainEventsRehydrationFactory>, ISyncDataResponse<DigestFilesInfoSet<DataSlice>, DataSlice, int, ChannelFileSetKey> {

		/// <summary>
		///     How many tries have we attempted. we use this field to inform our peer, and play nice so they dont ban us for being
		///     abusive.
		/// </summary>
		public byte RequestAttempt { get; set; }

		public int Id { get; set; }

		public DigestFilesInfoSet<DataSlice> Slices { get; } = new DigestFilesInfoSet<DataSlice>();

		public override void Dehydrate(IDataDehydrator dehydrator) {
			base.Dehydrate(dehydrator);

			dehydrator.Write(this.Id);

			dehydrator.Write(this.RequestAttempt);

			this.Slices.Dehydrate(dehydrator);

		}

		public override HashNodeList GetStructuresArray() {

			HashNodeList nodesList = base.GetStructuresArray();

			nodesList.Add(this.Id);

			nodesList.Add(this.RequestAttempt);

			nodesList.Add(this.Slices);

			return nodesList;
		}

		public override void Rehydrate(IDataRehydrator rehydrator, IBlockchainEventsRehydrationFactory rehydrationFactory) {
			base.Rehydrate(rehydrator, rehydrationFactory);

			this.Id = rehydrator.ReadInt();

			this.RequestAttempt = rehydrator.ReadByte();

			this.Slices.Rehydrate(rehydrator);
		}

		protected override ComponentVersion<SimpleUShort> SetIdentity() {
			return (ChainSyncMessageFactoryIds.SEND_DIGEST_FILE, 1, 0);
		}

		protected override short SetWorkflowType() {
			return WorkflowIDs.CHAIN_SYNC;
		}
	}
}