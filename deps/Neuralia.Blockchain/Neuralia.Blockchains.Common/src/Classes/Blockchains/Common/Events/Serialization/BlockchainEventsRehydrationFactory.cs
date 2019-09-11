using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization.Blockchain.Utils;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Genesis;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Messages;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Messages.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Contents;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Operations;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Specialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Gossip.Metadata;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Core.P2p.Messages.MessageSets.GossipMessageMetadatas;
using Neuralia.Blockchains.Core.Tools;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization {

	public interface ICommonRehydrationFactory : IRehydrationFactory {

		BlockChannelUtils.BlockChannelTypes ActiveBlockchainChannels { get; }

		BlockChannelUtils.BlockChannelTypes CompressedBlockchainChannels { get; }
	}

	public interface IEnvelopeRehydrationFactory : ICommonRehydrationFactory {

		IEnvelope RehydrateEnvelope(IByteArray data);

		ENVELOPE_TYPE RehydrateEnvelope<ENVELOPE_TYPE>(IByteArray data)
			where ENVELOPE_TYPE : IEnvelope;
	}

	public interface IBlockRehydrationFactory : ICommonRehydrationFactory {
		IBlock CreateBlock(IDataRehydrator bodyRehydrator);
		IBlock CreateBlock(IDehydratedBlock dehydratedBlock);
		IBlockComponentsRehydrationFactory CreateBlockComponentsRehydrationFactory();

		void PrepareBlock(IBlock block);
		void PrepareBlockHeader(IBlockHeader block);
	}

	public interface IDigestRehydrationFactory : ICommonRehydrationFactory {
		IBlockchainDigest CreateDigest(IDehydratedBlockchainDigest dehydratedDigest);
		IBlockchainDigestChannelFactory CreateDigestChannelfactory();
	}

	public interface IMessageRehydrationFactory : ICommonRehydrationFactory {
		IBlockchainMessage CreateMessage(IDehydratedBlockchainMessage dehydratedMessage);
	}

	public interface ITransactionRehydrationFactory : ICommonRehydrationFactory {

		//		TransactionSerializationMap CreateTransactionDehydrationMap(byte type, byte major, byte minor, ByteArray keyLengths);
		TransactionContent CreateTransactionContent(IDataRehydrator rehydrator);
		IOperation CreateTransactionOperation(IDataRehydrator rehydrator);

		ITransaction CreateTransaction(IDehydratedTransaction dehydratedTransaction);
		IKeyedTransaction CreateKeyedTransaction(IDehydratedTransaction dehydratedTransaction);
	}

	public interface IBlockchainEventsRehydrationFactory : IBlockRehydrationFactory, IMessageRehydrationFactory, ITransactionRehydrationFactory, IEnvelopeRehydrationFactory, IDigestRehydrationFactory {
	}

	public abstract class BlockchainEventsRehydrationFactory<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> : IBlockchainEventsRehydrationFactory
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> {

		protected readonly CENTRAL_COORDINATOR centralCoordinator;

		public BlockchainEventsRehydrationFactory(CENTRAL_COORDINATOR centralCoordinator) {
			this.centralCoordinator = centralCoordinator;

		}

		// special method to create the proper transaction type based on the transaction type and the version. override for latter additions
		public abstract ITransaction CreateTransaction(IDehydratedTransaction dehydratedTransaction);
		public abstract BlockChannelUtils.BlockChannelTypes ActiveBlockchainChannels { get; }

		public abstract BlockChannelUtils.BlockChannelTypes CompressedBlockchainChannels { get; }

		public abstract IKeyedTransaction CreateKeyedTransaction(IDehydratedTransaction dehydratedTransaction);

		//		public abstract TransactionSerializationMap CreateTransactionDehydrationMap(byte type, byte major, byte minor, ByteArray keyLengths);

		public abstract TransactionContent CreateTransactionContent(IDataRehydrator rehydrator);

		public abstract IOperation CreateTransactionOperation(IDataRehydrator rehydrator);

		public abstract IBlockchainDigest CreateDigest(IDehydratedBlockchainDigest dehydratedDigest);
		public abstract IBlockchainDigestChannelFactory CreateDigestChannelfactory();

		public abstract IBlock CreateBlock(IDataRehydrator bodyRehydrator);
		public abstract IBlock CreateBlock(IDehydratedBlock dehydratedBlock);

		public abstract IBlockchainMessage CreateMessage(IDehydratedBlockchainMessage dehydratedMessage);

		public virtual IEnvelope RehydrateEnvelope(IByteArray data) {
			var version = new ComponentVersion<EnvelopeType>();
			version.Rehydrate(DataSerializationFactory.CreateRehydrator(data));

			IEnvelope hashedEnvelope = this.CreateNewEnvelope(version);

			hashedEnvelope.RehydrateEnvelope(data);

			return hashedEnvelope;
		}

		public virtual ENVELOPE_TYPE RehydrateEnvelope<ENVELOPE_TYPE>(IByteArray data)
			where ENVELOPE_TYPE : IEnvelope {

			return (ENVELOPE_TYPE) this.RehydrateEnvelope(data);
		}

		public abstract IBlockComponentsRehydrationFactory CreateBlockComponentsRehydrationFactory();

		/// <summary>
		///     Perform preparations on the block after rehydration
		/// </summary>
		/// <param name="block"></param>
		public virtual void PrepareBlock(IBlock block) {
			this.PrepareBlockHeader(block);
		}

		// <summary>
		/// Perform preparations on the block header after rehydration
		/// </summary>
		/// <param name="block"></param>
		public virtual void PrepareBlockHeader(IBlockHeader block) {
			if(block != null) {
				// let's restore the actual time of the block
				if(block is IGenesisBlock genesis) {
					block.FullTimestamp = genesis.Inception;
				} else {
					block.FullTimestamp = this.centralCoordinator.BlockchainServiceSet.BlockchainTimeService.GetTransactionDateTime(block.Timestamp, this.centralCoordinator.ChainComponentProvider.ChainStateProviderBase.ChainInception);

				}
			}
		}

		public IGossipMessageMetadataDetails RehydrateGossipMessageMetadataDetails(byte type, IDataRehydrator rehydrator) {

			IGossipMessageMetadataDetails gossipMessageMetadataDetails = null;

			if(type == 1) {
				// nothing
			} else if(type == 2) {
				gossipMessageMetadataDetails = new BlockGossipMessageMetadataDetails();
			}

			gossipMessageMetadataDetails?.Rehydrate(rehydrator);

			return gossipMessageMetadataDetails;
		}

		public abstract IBlockchainDigest CreateDigest(IDataRehydrator rehydrator);

		public abstract IEnvelope CreateNewEnvelope(ComponentVersion<EnvelopeType> version);
	}
}