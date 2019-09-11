﻿using System;
using System.IO.Abstractions;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage.Bases;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.ChainPool;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.ChainState;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Wallet;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization.Blockchain.Utils;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Providers;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core.Configuration;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Factories {

	public interface IChainDalCreationFactory {
		// here are replaceable injection functions
		Func<ChainConfigurations, BlockChannelUtils.BlockChannelTypes, string, string, IBlockchainDigestChannelFactory, IFileSystem, IBlockchainEventSerializationFalReadonly> CreateSerializedArchiveFal { get; }

		CHAIN_STATE_DAL CreateChainStateDal<CHAIN_STATE_DAL, CHAIN_STATE_SNAPSHOT>(string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_STATE_DAL : IChainStateDal
			where CHAIN_STATE_SNAPSHOT : IChainStateEntry;

		CHAIN_STATE_CONTEXT CreateChainStateContext<CHAIN_STATE_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_STATE_CONTEXT : IChainStateContext;

		CHAIN_POOL_DAL CreateChainPoolDal<CHAIN_POOL_DAL>(string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_POOL_DAL : IChainPoolDal;

		CHAIN_POOL_CONTEXT CreateChainPoolContext<CHAIN_POOL_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_POOL_CONTEXT : IChainPoolContext;

		STANDARD_ACCOUNT_SNAPSHOT_DAL CreateStandardAccountSnapshotDal<STANDARD_ACCOUNT_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_SNAPSHOT_DAL : IStandardAccountSnapshotDal;

		STANDARD_ACCOUNT_SNAPSHOT_CONTEXT CreateStandardAccountSnapshotContext<STANDARD_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_SNAPSHOT_CONTEXT : IStandardAccountSnapshotContext;

		JOINT_ACCOUNT_SNAPSHOT_DAL CreateJointAccountSnapshotDal<JOINT_ACCOUNT_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where JOINT_ACCOUNT_SNAPSHOT_DAL : IJointAccountSnapshotDal;

		JOINT_ACCOUNT_SNAPSHOT_CONTEXT CreateJointAccountSnapshotContext<JOINT_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where JOINT_ACCOUNT_SNAPSHOT_CONTEXT : IJointAccountSnapshotContext;

		ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL CreateAccreditationCertificateAccountSnapshotDal<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL : IAccreditationCertificatesSnapshotDal;

		ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT CreateAccreditationCertificateSnapshotContext<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT : IAccreditationCertificatesSnapshotContext;

		STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL CreateStandardAccountKeysSnapshotDal<STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL : IAccountKeysSnapshotDal;

		STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT CreateStandardAccountKeysSnapshotContext<STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT : IAccountKeysSnapshotContext;

		CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_DAL CreateChainOptionsSnapshotDal<CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_DAL>(string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_DAL : IChainOptionsSnapshotDal;

		CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_CONTEXT CreateChainOptionsSnapshotContext<CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_CONTEXT : IChainOptionsSnapshotContext;

		TRACKED_ACCOUNTS_DAL CreateTrackedAccountsDal<TRACKED_ACCOUNTS_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where TRACKED_ACCOUNTS_DAL : ITrackedAccountsDal;

		TRACKED_ACCOUNTS_CONTEXT CreateTrackedAccountsContext<TRACKED_ACCOUNTS_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where TRACKED_ACCOUNTS_CONTEXT : ITrackedAccountsContext;
	}

	public interface IChainDalCreationFactory<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> : IChainDalCreationFactory
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> {

		Func<CENTRAL_COORDINATOR, string, IFileSystem, IWalletSerialisationFal> CreateWalletSerialisationFal { get; }
	}

	/// <summary>
	///     A factory class to instantiate all Dals in the system
	/// </summary>
	public abstract class ChainDalCreationFactory<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> : IChainDalCreationFactory<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CENTRAL_COORDINATOR : ICentralCoordinator<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER>
		where CHAIN_COMPONENT_PROVIDER : IChainComponentProvider<CENTRAL_COORDINATOR, CHAIN_COMPONENT_PROVIDER> {

		// now the chain SqliteDals. because of their generics, we can not have a generic func propety. so here we have a method, and the chain child class
		// can override it and provide the func property down there.

		public abstract CHAIN_STATE_DAL CreateChainStateDal<CHAIN_STATE_DAL, CHAIN_STATE_SNAPSHOT>(string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_STATE_DAL : IChainStateDal
			where CHAIN_STATE_SNAPSHOT : IChainStateEntry;

		public abstract CHAIN_POOL_DAL CreateChainPoolDal<CHAIN_POOL_DAL>(string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_POOL_DAL : IChainPoolDal;

		public abstract Func<ChainConfigurations, BlockChannelUtils.BlockChannelTypes, string, string, IBlockchainDigestChannelFactory, IFileSystem, IBlockchainEventSerializationFalReadonly> CreateSerializedArchiveFal { get; }

		public abstract Func<CENTRAL_COORDINATOR, string, IFileSystem, IWalletSerialisationFal> CreateWalletSerialisationFal { get; }

		// contexts

		public abstract CHAIN_STATE_CONTEXT CreateChainStateContext<CHAIN_STATE_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_STATE_CONTEXT : IChainStateContext;

		public abstract CHAIN_POOL_CONTEXT CreateChainPoolContext<CHAIN_POOL_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_POOL_CONTEXT : IChainPoolContext;

		public abstract STANDARD_ACCOUNT_SNAPSHOT_DAL CreateStandardAccountSnapshotDal<STANDARD_ACCOUNT_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_SNAPSHOT_DAL : IStandardAccountSnapshotDal;

		public abstract STANDARD_ACCOUNT_SNAPSHOT_CONTEXT CreateStandardAccountSnapshotContext<STANDARD_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_SNAPSHOT_CONTEXT : IStandardAccountSnapshotContext;

		public abstract JOINT_ACCOUNT_SNAPSHOT_DAL CreateJointAccountSnapshotDal<JOINT_ACCOUNT_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where JOINT_ACCOUNT_SNAPSHOT_DAL : IJointAccountSnapshotDal;

		public abstract JOINT_ACCOUNT_SNAPSHOT_CONTEXT CreateJointAccountSnapshotContext<JOINT_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where JOINT_ACCOUNT_SNAPSHOT_CONTEXT : IJointAccountSnapshotContext;

		public abstract ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL CreateAccreditationCertificateAccountSnapshotDal<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_DAL : IAccreditationCertificatesSnapshotDal;

		public abstract ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT CreateAccreditationCertificateSnapshotContext<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT_CONTEXT : IAccreditationCertificatesSnapshotContext;

		public abstract STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL CreateStandardAccountKeysSnapshotDal<STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_KEYS_SNAPSHOT_DAL : IAccountKeysSnapshotDal;

		public abstract STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT CreateStandardAccountKeysSnapshotContext<STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where STANDARD_ACCOUNT_KEYS_SNAPSHOT_CONTEXT : IAccountKeysSnapshotContext;

		public abstract CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_DAL CreateChainOptionsSnapshotDal<CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_DAL>(string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_DAL : IChainOptionsSnapshotDal;

		public abstract CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_CONTEXT CreateChainOptionsSnapshotContext<CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where CHAIN_OPTIONS_ACCOUNT_SNAPSHOT_CONTEXT : IChainOptionsSnapshotContext;

		public abstract TRACKED_ACCOUNTS_DAL CreateTrackedAccountsDal<TRACKED_ACCOUNTS_DAL>(long groupSize, string folderPath, BlockchainServiceSet serviceSet, AppSettingsBase.SerializationTypes serializationType)
			where TRACKED_ACCOUNTS_DAL : ITrackedAccountsDal;

		public abstract TRACKED_ACCOUNTS_CONTEXT CreateTrackedAccountsContext<TRACKED_ACCOUNTS_CONTEXT>(AppSettingsBase.SerializationTypes serializationType)
			where TRACKED_ACCOUNTS_CONTEXT : ITrackedAccountsContext;
	}
}