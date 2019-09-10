﻿using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Core.Network.Protocols {
	public interface IMessageEntry {

		IMessageHeader Header { get; }
		bool IsComplete { get; }
		byte Version { get; }

		IByteArray Message { get; }
		IByteArray Dehydrate();
		void RebuildHeader(IByteArray buffer);
		void SetMessageContent(IDataRehydrator bufferRehydrator);
	}

	public interface IMessageEntry<out HEADER_TYPE> : IMessageEntry
		where HEADER_TYPE : IMessageHeader {

		HEADER_TYPE HeaderT { get; }
	}
}