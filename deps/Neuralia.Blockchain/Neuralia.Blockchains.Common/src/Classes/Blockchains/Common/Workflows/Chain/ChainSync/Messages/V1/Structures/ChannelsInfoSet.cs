using System;
using System.Collections.Generic;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization.Blockchain.Utils;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Chain.ChainSync.Messages.V1.Structures {
	public class BlockChannelsInfoSet<T> : ChannelsInfoSet<BlockChannelUtils.BlockChannelTypes, T>
		where T : DataSliceSize, new() {

		public BlockChannelsInfoSet() : base((key, dehydrator) => dehydrator.Write((int) key), rehydrator => (BlockChannelUtils.BlockChannelTypes) rehydrator.ReadInt()) {
		}

		public T HighHeaderInfo => this.SlicesInfo.ContainsKey(BlockChannelUtils.BlockChannelTypes.HighHeader) ? this.SlicesInfo[BlockChannelUtils.BlockChannelTypes.HighHeader] : default;
		public T LowHeaderInfo => this.SlicesInfo.ContainsKey(BlockChannelUtils.BlockChannelTypes.LowHeader) ? this.SlicesInfo[BlockChannelUtils.BlockChannelTypes.LowHeader] : default;
		public T ContentsInfo => this.SlicesInfo.ContainsKey(BlockChannelUtils.BlockChannelTypes.Contents) ? this.SlicesInfo[BlockChannelUtils.BlockChannelTypes.Contents] : default;
	}

	public class DigestChannelsInfoSet<T> : ChannelsInfoSet<int, T>
		where T : DataSliceSize, new() {

		public DigestChannelsInfoSet() : base((key, dehydrator) => dehydrator.Write(key), rehydrator => rehydrator.ReadInt()) {
			this.SlicesInfo.Add(1, new T());
		}

		public T FileInfo => this.SlicesInfo[1];
	}

	public class DigestFilesInfoSet<T> : ChannelsInfoSet<ChannelFileSetKey, T>
		where T : DataSliceSize, new() {

		public DigestFilesInfoSet() : base((key, dehydrator) => {
			key.Dehydrate(dehydrator);
		}, rehydrator => {
			ChannelFileSetKey key = new ChannelFileSetKey();
			key.Rehydrate(rehydrator);

			return key;
		}) {

		}
	}

	public class ChannelsInfoSet<KEY, T> : IBinarySerializable, ITreeHashable
		where T : DataSliceSize, new() {

		private readonly Action<KEY, IDataDehydrator> dehydrateKey;
		private readonly Func<IDataRehydrator, KEY> rehydrateKey;

		public ChannelsInfoSet(Action<KEY, IDataDehydrator> dehydrateKey, Func<IDataRehydrator, KEY> rehydrateKey) {
			this.dehydrateKey = dehydrateKey;
			this.rehydrateKey = rehydrateKey;
		}

		public ushort FileId { get; set; }
		public Dictionary<KEY, T> SlicesInfo { get; } = new Dictionary<KEY, T>();

		public void Dehydrate(IDataDehydrator dehydrator) {

			dehydrator.Write(this.FileId);
			dehydrator.Write((byte) this.SlicesInfo.Count);

			foreach(var entry in this.SlicesInfo) {

				this.dehydrateKey(entry.Key, dehydrator);

				entry.Value.Dehydrate(dehydrator);
			}
		}

		public void Rehydrate(IDataRehydrator rehydrator) {

			this.FileId = rehydrator.ReadUShort();
			byte count = rehydrator.ReadByte();

			this.SlicesInfo.Clear();

			for(int i = 0; i < count; i++) {
				KEY key = this.rehydrateKey(rehydrator);
				T sliceInfo = new T();

				sliceInfo.Rehydrate(rehydrator);

				this.SlicesInfo.Add(key, sliceInfo);
			}
		}

		public HashNodeList GetStructuresArray() {

			HashNodeList nodesList = new HashNodeList();

			nodesList.Add(this.FileId);

			foreach(var entry in this.SlicesInfo) {
				nodesList.Add(entry.Key);
				nodesList.Add(entry.Value);
			}

			return nodesList;
		}
	}

	public static class ChannelsInfoSet {
		/// <summary>
		///     reorganize the channel bands to facilitate consensus determination
		/// </summary>
		/// <param name="peerEntries"></param>
		/// <returns></returns>
		public static Dictionary<KEY, List<(Guid peerId, T entry)>> RestructureConsensusBands<KEY, CHANNEL_INFO_SET, T>(Dictionary<Guid, CHANNEL_INFO_SET> peerEntries)
			where CHANNEL_INFO_SET : ChannelsInfoSet<KEY, T>
			where T : DataSliceSize, new() {

			var result = new Dictionary<KEY, List<(Guid peerId, T entry)>>();

			foreach(var peerSet in peerEntries) {

				foreach(var sliceInfo in peerSet.Value.SlicesInfo) {

					if(!result.ContainsKey(sliceInfo.Key)) {
						result.Add(sliceInfo.Key, new List<(Guid peerId, T entry)>());
					}

					result[sliceInfo.Key].Add((peerSet.Key, sliceInfo.Value));
				}
			}

			return result;
		}
	}
}