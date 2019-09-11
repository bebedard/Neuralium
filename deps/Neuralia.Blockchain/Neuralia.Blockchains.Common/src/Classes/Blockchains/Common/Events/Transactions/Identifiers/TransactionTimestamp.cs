using System;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal;
using Neuralia.Blockchains.Common.Classes.General.Json.Converters;
using Neuralia.Blockchains.Core.General.Json.Converters;
using Neuralia.Blockchains.Core.General.Types.Dynamic;
using Newtonsoft.Json;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers {
	/// <summary>
	///     Number of seconds since the chain inception
	/// </summary>
	/// <remarks>
	///     to encode 1 year (31,536,000), we need 25 bits (4 bytes), so there is no point in encoding smaller sizes. we
	///     will always need 4 or more bytes to store a time offset.
	/// </remarks>
	[JsonConverter(typeof(TransactionTimestampJsonConverter))]
	public class TransactionTimestamp : AdaptiveLong3_6, IComparable<TransactionTimestamp> {

		/// <summary>
		///     we use a maximum of 7 bytes.
		/// </summary>
		static TransactionTimestamp() {
			LiteDBMappers.RegisterTransactionTimestamp();
		}

		public TransactionTimestamp() {

		}

		public TransactionTimestamp(long timestamp) : base(timestamp) {

		}

		public TransactionTimestamp(string timestamp) : this(long.Parse(timestamp)) {

		}

		public TransactionTimestamp(TransactionTimestamp other) : this(other.Value) {

		}

		[JsonIgnore]
		public TransactionTimestamp Clone => new TransactionTimestamp(this);

		public int CompareTo(TransactionTimestamp other) {
			return this.Value.CompareTo(other.Value);
		}

		public override bool Equals(object obj) {
			if(obj is TransactionTimestamp other) {
				return this.Value == other.Value;
			}

			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return this.Value.GetHashCode();
		}

		public override string ToString() {
			return this.Value.ToString();
		}

		public static implicit operator TransactionTimestamp(int value) {
			return new TransactionTimestamp(value);
		}

		public static implicit operator TransactionTimestamp(long value) {
			return new TransactionTimestamp(value);
		}
	}
}