using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.ChainSync.Messages.V1.Structures {
	public class ChannelFileSetKey : IBinarySerializable {

		public ChannelFileSetKey() {

		}

		public ChannelFileSetKey(DigestChannelType channelId, int indexId, int fileId, uint filePart) {
			this.ChannelId = channelId;
			this.IndexId = indexId;
			this.FileId = fileId;
			this.FilePart = filePart;
		}

		public DigestChannelType ChannelId { get; set; }
		public int IndexId { get; set; }
		public int FileId { get; set; }
		public uint FilePart { get; set; }

		public void Rehydrate(IDataRehydrator rehydrator) {
			this.ChannelId = rehydrator.ReadUShort();
			this.IndexId = rehydrator.ReadInt();
			this.FileId = rehydrator.ReadInt();
			this.FilePart = rehydrator.ReadUInt();
		}

		public void Dehydrate(IDataDehydrator dehydrator) {
			dehydrator.Write(this.ChannelId.Value);
			dehydrator.Write(this.IndexId);
			dehydrator.Write(this.FileId);
			dehydrator.Write(this.FilePart);
		}

		public static implicit operator ChannelFileSetKey((DigestChannelType channelId, int indexId, int fileId, uint filePart) d) {
			return new ChannelFileSetKey(d.channelId, d.indexId, d.fileId, d.filePart);
		}

		public static implicit operator ChannelFileSetKey(string d) {
			var items = d.Split('-');

			return new ChannelFileSetKey(ushort.Parse(items[0]), int.Parse(items[1]), int.Parse(items[2]), uint.Parse(items[3]));
		}

		public override string ToString() {
			return $"{this.ChannelId.Value}-{this.IndexId}-{this.FileId}-{this.FilePart}";
		}

		public override bool Equals(object obj) {
			if(obj is ChannelFileSetKey other) {
				if(ReferenceEquals(this, other)) {
					return true;
				}

				if(ReferenceEquals(this, null)) {
					return false;
				}

				if(ReferenceEquals(other, null)) {
					return false;
				}

				if(this.GetType() != other.GetType()) {
					return false;
				}

				return (this.ChannelId == other.ChannelId) && (this.IndexId == other.IndexId) && (this.FileId == other.FileId) && (this.FilePart == other.FilePart);
			}

			return base.Equals(obj);
		}

		public override int GetHashCode() {
			unchecked {
				int hashCode = this.ChannelId.Value;
				hashCode = (hashCode * 397) ^ this.IndexId;
				hashCode = (hashCode * 397) ^ this.FileId;
				hashCode = (hashCode * 397) ^ (int) this.FilePart;

				return hashCode;
			}
		}
	}
}