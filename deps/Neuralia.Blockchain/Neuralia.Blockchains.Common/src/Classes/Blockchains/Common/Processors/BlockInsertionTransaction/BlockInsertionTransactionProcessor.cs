using System;
using System.Collections.Generic;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.ChainState;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers;
using Neuralia.Blockchains.Core.Extensions;
using Neuralia.Blockchains.Core.Services;
using Neuralia.Blockchains.Tools;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Processors.BlockInsertionTransaction {

	public interface IBlockInsertionTransactionProcessor : IDisposable2 {
		long DiskBlockHeight { get; set; }
		DateTime LastBlockTimestamp { get; set; }
		ushort LastBlockLifespan { get; set; }
		IByteArray LastBlockHash { get; set; }
		int BlockInsertionStatus { get; set; }
		(int index, long startingBlockId) BlockIndex { get; set; }
		Dictionary<string, long> FileSizes { get; set; }
		IByteArray BlockIdFile { get; set; }
		IByteArray ModeratorKey { get; set; }
		void CreateSnapshot();
		void Commit();
		void Rollback();
	}

	public abstract class BlockInsertionTransactionProcessor<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> : IBlockInsertionTransactionProcessor
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> {

		private readonly CENTRAL_COORDINATOR centralCoordinator;

		private readonly byte moderatorKeyOrdinal;

		private bool commited;

		public BlockInsertionTransactionProcessor(CENTRAL_COORDINATOR centralCoordinator, byte moderatorKeyOrdinal) {
			this.centralCoordinator = centralCoordinator;
			this.moderatorKeyOrdinal = moderatorKeyOrdinal;

			this.CreateSnapshot();
		}

		public long DiskBlockHeight { get; set; }

		public DateTime LastBlockTimestamp { get; set; }
		public ushort LastBlockLifespan { get; set; }
		public IByteArray LastBlockHash { get; set; }
		public int BlockInsertionStatus { get; set; }

		public (int index, long startingBlockId) BlockIndex { get; set; }

		public Dictionary<string, long> FileSizes { get; set; }

		public IByteArray BlockIdFile { get; set; }
		public IByteArray ModeratorKey { get; set; }

		public void CreateSnapshot() {

			IChainStateProvider chainStateProvider = this.centralCoordinator.ChainComponentProvider.ChainStateProviderBase;

			this.DiskBlockHeight = chainStateProvider.DiskBlockHeight;
			this.LastBlockTimestamp = chainStateProvider.LastBlockTimestamp;
			this.LastBlockLifespan = chainStateProvider.LastBlockLifespan;
			this.LastBlockHash = (ByteArray) chainStateProvider.LastBlockHash;
			this.BlockInsertionStatus = (int) chainStateProvider.BlockInterpretationStatus;

			if(this.moderatorKeyOrdinal == GlobalsService.MODERATOR_BLOCKS_KEY_SEQUENTIAL_ID) {
				this.ModeratorKey = chainStateProvider.GetModeratorKeyBytes(this.moderatorKeyOrdinal);
			}

			this.BlockIndex = this.centralCoordinator.ChainComponentProvider.ChainDataLoadProviderBase.FindBlockIndex(this.DiskBlockHeight);

			this.FileSizes = this.centralCoordinator.ChainComponentProvider.ChainDataLoadProviderBase.GetBlockFileSizes(this.DiskBlockHeight);

		}

		public void Commit() {
			this.commited = true;
		}

		public void Rollback() {
			if(!this.commited) {

				IChainStateProvider chainStateProvider = this.centralCoordinator.ChainComponentProvider.ChainStateProviderBase;

				chainStateProvider.DiskBlockHeight = this.DiskBlockHeight;
				chainStateProvider.LastBlockTimestamp = this.LastBlockTimestamp;
				chainStateProvider.LastBlockLifespan = this.LastBlockLifespan;
				chainStateProvider.LastBlockHash = this.LastBlockHash.ToExactByteArrayCopy();
				chainStateProvider.BlockInterpretationStatus = (ChainStateEntryFields.BlockInterpretationStatuses) this.BlockInsertionStatus;

				((IChainDataWriteProvider) this.centralCoordinator.ChainComponentProvider.ChainDataLoadProviderBase).TruncateBlockFileSizes(this.DiskBlockHeight, this.FileSizes);

				if(this.BlockIdFile?.HasData ?? false) {
					FileExtensions.WriteAllBytes(this.centralCoordinator.ChainComponentProvider.ChainStateProviderBase.GetBlocksIdFilePath(), this.BlockIdFile, this.centralCoordinator.FileSystem);
				}

				if(this.moderatorKeyOrdinal == GlobalsService.MODERATOR_BLOCKS_KEY_SEQUENTIAL_ID) {
					chainStateProvider.UpdateModeratorKey(new TransactionId(), this.moderatorKeyOrdinal, this.ModeratorKey);
				}
			}
		}

	#region Disposable

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if(!this.IsDisposed) {
				try {

					if(disposing) {
						this.Rollback();
					}
				} finally {
					this.IsDisposed = true;
				}
			}
		}

		~BlockInsertionTransactionProcessor() {
			this.Dispose(false);
		}

		public bool IsDisposed { get; private set; }

	#endregion

	}
}