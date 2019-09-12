using System;
using System.Collections.Generic;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures.Timeline {
	public class TimelineDay {

		public readonly List<TimelineEntry> Entries = new List<TimelineEntry>();

		public DateTime Day { get; set; }
		public int Id { get; set; }
		public decimal EndingTotal { get; set; }

		public class TimelineEntry {

			public DateTime Timestamp { get; set; }

			public string TransactionId { get; set; }
			public string SenderAccountId { get; set; }
			public string RecipientAccountId { get; set; }
			public decimal Amount { get; set; }
			public decimal Tips { get; set; }

			public decimal Total { get; set; }

			public byte Direction { get; set; }
			public byte CreditType { get; set; }
			public bool Confirmed { get; set; }

			public int DayId { get; set; }
		}
	}
}