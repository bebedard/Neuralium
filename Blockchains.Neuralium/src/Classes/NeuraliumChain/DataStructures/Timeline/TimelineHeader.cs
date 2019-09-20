using System;
using MessagePack;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures.Timeline {
	
	[MessagePackObject(keyAsPropertyName: true)]
	public class TimelineHeader {
		public int NumberOfDays { get; set; }
		public string FirstDay { get; set; }
	}
}