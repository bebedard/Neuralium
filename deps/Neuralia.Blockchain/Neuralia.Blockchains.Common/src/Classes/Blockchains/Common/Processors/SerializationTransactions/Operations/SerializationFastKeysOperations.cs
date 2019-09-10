using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Managers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Processors.SerializationTransactions.Operations {
	public class SerializationFastKeysOperations : SerializationTransactionOperation {

		public SerializationFastKeysOperations(ISerializationManager serializationManager, IChainDataWriteProvider chainDataWriteProvider) : base(serializationManager, chainDataWriteProvider) {
		}

		public AccountId AccountId { get; set; } = new AccountId();
		public byte Ordinal { get; set; }
		public byte TreeHeight { get; set; }
		public byte HashBits { get; set; }
		public IByteArray Key { get; set; }

		protected override void SetType() {
			this.SerializationTransactionOperationType = SerializationTransactionOperationTypes.FastKeys;
		}

		public override void Undo() {

			this.chainDataWriteProvider.SaveAccountKeyIndex(this.AccountId, this.Key, this.TreeHeight, this.HashBits, this.Ordinal);
		}

		public override void Rehydrate(IDataRehydrator rehydrator) {
			base.Rehydrate(rehydrator);

			this.AccountId.Rehydrate(rehydrator);
			this.Ordinal = rehydrator.ReadByte();
			this.TreeHeight = rehydrator.ReadByte();
			this.HashBits = rehydrator.ReadByte();
			this.Key = rehydrator.ReadNonNullableArray();
		}

		public override void Dehydrate(IDataDehydrator dehydrator) {
			base.Dehydrate(dehydrator);

			this.AccountId.Dehydrate(dehydrator);
			dehydrator.Write(this.Ordinal).Write(this.TreeHeight).Write(this.HashBits);
			dehydrator.WriteNonNullable(this.Key);
		}
	}
}