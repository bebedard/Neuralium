using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Messages.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Tools;
using Neuralia.Blockchains.Common.Classes.Configuration;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.DataAccess.Interfaces.MessageRegistry;
using Neuralia.Blockchains.Core.Extensions;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Tools.Data;
using Serilog;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers {

	public interface IChainDataWriteProvider : IChainDataLoadProvider {
		void SerializeBlock(IDehydratedBlock dehydratedBlock);
		void SerializeBlockchainMessage(IDehydratedBlockchainMessage dehydratedBlockchainMessage);

		void SaveDigestChannelDescription(int digestId, BlockchainDigestDescriptor blockchainDigestDescriptor);

		void UpdateCurrentDigest(int digestId, long blockHeight);

		void SaveDigestHeader(int digestId, IByteArray digestHeader);
		void SaveAccountKeyIndex(AccountId accountId, IByteArray key, byte treeHeight, byte hashBits, byte ordinal);

		void CacheUnvalidatedBlockGossipMessage(IBlockEnvelope unvalidatedBlockEnvelope, long xxHash);
		void ClearCachedUnvalidatedBlockGossipMessage(long blockId);

		void TruncateBlockFileSizes(long blockId, Dictionary<string, long> fileSizes);
	}

	public interface IChainDataWriteProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> : IChainDataLoadProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>, IChainDataWriteProvider
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> {
	}

	/// <summary>
	///     The main provider for writing chain events to disk. This class is meant to be used by the serialization manager
	///     ONLY. NOT thread safe.
	/// </summary>
	public abstract class ChainDataWriteProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> : ChainDataLoadProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>, IChainDataWriteProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> {

		/// <summary>
		///     We want to ensure we have a single isntance of this provider, so we use this as the marker
		/// </summary>
		private static bool instantiated;

		protected ChainDataWriteProvider(CENTRAL_COORDINATOR centralCoordinator) : base(centralCoordinator) {
			if(instantiated && !GlobalSettings.TestingMode) {
				throw new ApplicationException("Cannot instantiate more than one of this provider");
			}

			// thats it
			instantiated = true;
		}

		private IChainDataLoadProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> ChainDataLoadProvider => this.centralCoordinator.ChainComponentProvider.ChainDataLoadProviderBase;

		protected new IBlockchainEventSerializationFalReadWrite BlockchainEventSerializationFal => (IBlockchainEventSerializationFalReadWrite) base.BlockchainEventSerializationFal;

		public void SerializeBlock(IDehydratedBlock dehydratedBlock) {

			BlockChainConfigurations configuration = this.centralCoordinator.ChainComponentProvider.ChainConfigurationProviderBase.ChainConfiguration;

			if(!BlockchainUtilities.UsesBlocks(configuration.BlockSavingMode)) {
				// we dotn save the blocks
				return;
			}

			//1. ok, there we go. first step, lets make sure this block is not already serialized

			(int index, long startingBlockId) index = this.FindBlockIndex(dehydratedBlock.BlockId.Value);

			//2. now, we will determine where it should physically go, and insert it in the block index

			// we are ready to insert the block index entry

			bool inserted = false;

			try {
				inserted = this.BlockchainEventSerializationFal.InsertBlockEntry(dehydratedBlock.BlockId.Value, index, dehydratedBlock.GetEssentialDataChannels(), dehydratedBlock.RehydratedBlock.KeyedOffsets);
			} catch(Exception ex) {
				Log.Error(ex, $"Failed to insert the block id {dehydratedBlock.BlockId} into the blockchain filesystem");

				throw;
			}

			if(inserted) {
				Log.Information($"Block id {dehydratedBlock.BlockId} has been successfully inserted in the blockchain filesystem.");
			} else {
				Log.Information($"Block id {dehydratedBlock.BlockId} already existed in the blockchain filesystem. Nothing changed.");
			}

			// ok, our block is saved!

			// store in memory for quick access when required
			this.CacheBlock(dehydratedBlock.RehydratedBlock, dehydratedBlock.GetEssentialDataChannels(), dehydratedBlock);
		}

		public void SerializeBlockchainMessage(IDehydratedBlockchainMessage dehydratedBlockchainMessage) {

			//TODO: make this method atomic!!!
			// get our message bytes
			IByteArray messageBytes = dehydratedBlockchainMessage.Dehydrate();

			Guid uuid = dehydratedBlockchainMessage.RehydratedMessage.Uuid;

			// ok, now we create our new entry. lets create the directory structure and blocks file if it does not exist
			string messagesFile = this.GetMessagesFile(uuid);
			string messagesIndexFile = this.GetMessagesIndexFile(uuid);

			this.BlockchainEventSerializationFal.EnsureFileExists(messagesFile);
			this.BlockchainEventSerializationFal.EnsureFileExists(messagesIndexFile);

			var messagesFileSize = this.ChainDataLoadProvider.GetMessagesFileSize(uuid);

			if(!messagesFileSize.HasValue) {
				throw new ApplicationException("Messages file did not exist");
			}

			// we are ready to insert the block index entry
			this.BlockchainEventSerializationFal.InsertMessagesIndexEntry(messagesIndexFile, uuid, messagesFileSize.Value, messageBytes.Length);

			this.BlockchainEventSerializationFal.InsertMessagesEntry(messagesFile, messageBytes);
		}

		public void SaveDigestChannelDescription(int digestId, BlockchainDigestDescriptor blockchainDigestDescriptor) {
			this.BlockchainEventSerializationFal.SaveDigestChannelDescription(this.GetDigestsScopedFolderPath(digestId), blockchainDigestDescriptor);
		}

		public void UpdateCurrentDigest(int digestId, long blockHeight) {
			bool deletePreviousBlocks = this.centralCoordinator.ChainComponentProvider.ChainConfigurationProviderBase.GetChainConfiguration().BlockSavingMode == AppSettingsBase.BlockSavingModes.DigestsThenBlocks;
			(int index, long startingBlockId)? blockGroupIndex = null;

			if(deletePreviousBlocks) {
				blockGroupIndex = this.FindBlockIndex(blockHeight);
			}

			this.BlockchainEventSerializationFal.UpdateCurrentDigest(this.GetDigestsScopedFolderPath(digestId), deletePreviousBlocks, blockGroupIndex);
		}

		public void SaveDigestHeader(int digestId, IByteArray digestHeader) {
			this.BlockchainEventSerializationFal.SaveDigestHeader(this.GetDigestsScopedFolderPath(digestId), digestHeader);
		}

		public void SaveAccountKeyIndex(AccountId accountId, IByteArray key, byte treeHeight, byte hashBits, byte ordinal) {
			this.BlockchainEventSerializationFal.SaveAccountKeyIndex(accountId, key, treeHeight, hashBits, ordinal);
		}

		/// <summary>
		///     trucate the files to the length we had recorded to restore them.
		/// </summary>
		/// <param name="blockId"></param>
		/// <param name="fileSizes"></param>
		public void TruncateBlockFileSizes(long blockId, Dictionary<string, long> fileSizes) {
			(int index, long startingBlockId) index = this.FindBlockIndex(blockId);

			string folderPath = this.BlockchainEventSerializationFal.GetBlockPath(index.index);

			foreach(var entry in fileSizes) {

				string filename = Path.Combine(folderPath, entry.Key);

				if(entry.Value <= 0) {
					if(this.centralCoordinator.FileSystem.File.Exists(filename)) {
						this.centralCoordinator.FileSystem.File.Delete(filename);
					}
				} else {
					if(this.centralCoordinator.FileSystem.File.Exists(filename) && (this.centralCoordinator.FileSystem.FileInfo.FromFileName(filename).Length != entry.Value)) {
						using(Stream fileStream = this.centralCoordinator.FileSystem.FileInfo.FromFileName(filename).OpenWrite()) {
							fileStream.SetLength(entry.Value);
						}
					}
				}
			}

			// now any file not in the list must be deleted
			if(this.centralCoordinator.FileSystem.Directory.Exists(folderPath)) {

				// select files that were actually existing with data
				var snapshotFiles = fileSizes.Where(f => f.Value > 0).Select(f => f.Key).ToList();

				foreach(string entry in this.centralCoordinator.FileSystem.Directory.GetFiles(folderPath).Select(Path.GetFileName).Where(f => !snapshotFiles.Contains(f))) {
					string fullPath = Path.Combine(folderPath, entry);

					if(this.centralCoordinator.FileSystem.File.Exists(fullPath)) {
						this.centralCoordinator.FileSystem.File.Delete(fullPath);
					}
				}
			}
		}

	#region gossip message cache

		/// <summary>
		///     Here we take a block message we can't validate yet and potentially cache it if we can use it later.
		/// </summary>
		/// <param name="blockEnvelope"></param>
		public virtual void CacheUnvalidatedBlockGossipMessage(IBlockEnvelope unvalidatedBlockEnvelope, long xxHash) {

			string walletPath = this.centralCoordinator.ChainComponentProvider.WalletProviderBase.GetChainStorageFilesPath();
			IMessageRegistryDal messageRegistryDal = this.centralCoordinator.BlockchainServiceSet.DataAccessService.CreateMessageRegistryDal(walletPath, this.centralCoordinator.BlockchainServiceSet);

			bool result = false;

			lock(this.locker) {
				result = messageRegistryDal.CacheUnvalidatedBlockGossipMessage(unvalidatedBlockEnvelope.BlockId, xxHash);
			}

			if(result) {
				// ok, let's save the file

				string folderPath = this.GetBlocksGossipCacheFolderPath();

				FileExtensions.EnsureDirectoryStructure(folderPath, this.centralCoordinator.FileSystem);

				string completeFile = this.GetUnvalidatedBlockGossipMessageFullFileName(unvalidatedBlockEnvelope.BlockId, xxHash);

				if(!this.centralCoordinator.FileSystem.File.Exists(completeFile)) {
					FileExtensions.WriteAllBytes(completeFile, unvalidatedBlockEnvelope.DehydrateEnvelope(), this.centralCoordinator.FileSystem);
				}
			}
		}

		public void ClearCachedUnvalidatedBlockGossipMessage(long blockId) {

			string walletPath = this.centralCoordinator.ChainComponentProvider.WalletProviderBase.GetChainStorageFilesPath();
			IMessageRegistryDal messageRegistryDal = this.centralCoordinator.BlockchainServiceSet.DataAccessService.CreateMessageRegistryDal(walletPath, this.centralCoordinator.BlockchainServiceSet);

			string folderPath = this.GetBlocksGossipCacheFolderPath();
			FileExtensions.EnsureDirectoryStructure(folderPath, this.centralCoordinator.FileSystem);

			var deletedEntries = new List<(long blockId, long xxHash)>();

			lock(this.locker) {
				deletedEntries = messageRegistryDal.RemoveCachedUnvalidatedBlockGossipMessages(blockId);
			}

			// delete the files
			foreach((long blockId, long xxHash) entry in deletedEntries) {
				try {
					string completeFile = this.GetUnvalidatedBlockGossipMessageFullFileName(entry.blockId, entry.xxHash);

					if(this.centralCoordinator.FileSystem.File.Exists(completeFile)) {
						this.centralCoordinator.FileSystem.File.Delete(completeFile);
					}
				} catch(Exception ex) {
					Log.Error(ex, $"Failed to delete a cached gossip block message file for block Id {entry.blockId}");
				}
			}
		}

	#endregion

	}
}