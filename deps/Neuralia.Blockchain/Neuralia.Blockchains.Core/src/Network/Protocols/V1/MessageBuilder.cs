﻿using Neuralia.Blockchains.Core.Compression;
using Neuralia.Blockchains.Core.Network.Protocols.SplitMessages;
using Neuralia.Blockchains.Core.Network.Protocols.V1.Messages.Large;
using Neuralia.Blockchains.Core.Network.Protocols.V1.Messages.Medium;
using Neuralia.Blockchains.Core.Network.Protocols.V1.Messages.Small;
using Neuralia.Blockchains.Core.Network.Protocols.V1.Messages.Split;
using Neuralia.Blockchains.Core.Network.Protocols.V1.Messages.Tiny;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Core.Network.Protocols.V1 {

	/// <summary>
	///     Class to build and assemble the messages
	/// </summary>
	public class MessageBuilder : IMessageBuilder {

		public static readonly ProtocolVersion ProtocolVersion = new ProtocolVersion(1, 1);
		public static readonly ProtocolCompression ProtocolCompression = new ProtocolCompression(ProtocolCompression.CompressionAlgorithm.Brotli, CompressionLevelByte.Nine);

		public IByteArray BuildTinyMessage(IByteArray bytes) {

			TinyMessageEntry messageEntry = new TinyMessageEntry(bytes);

			return messageEntry.Dehydrate();
		}

		public IByteArray BuildSmallMessage(IByteArray bytes) {

			SmallMessageEntry messageEntry = new SmallMessageEntry(bytes);

			return messageEntry.Dehydrate();
		}

		public IByteArray BuildMediumMessage(IByteArray bytes) {

			MediumMessageEntry messageEntry = new MediumMessageEntry(bytes);

			return messageEntry.Dehydrate();
		}

		public IByteArray BuildLargeMessage(IByteArray bytes) {
			LargeMessageEntry messageEntry = new LargeMessageEntry(bytes);

			return messageEntry.Dehydrate();
		}

		public ISplitMessageEntry BuildSplitMessage(IByteArray bytes) {
			return new SplitMessageEntry(bytes);

		}
	}
}