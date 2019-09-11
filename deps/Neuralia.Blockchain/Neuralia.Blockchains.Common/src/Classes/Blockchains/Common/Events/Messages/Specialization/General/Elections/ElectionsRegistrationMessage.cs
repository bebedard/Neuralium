﻿using System.Collections.Generic;
using System.Linq;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.AccreditationCertificates;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Serialization;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Messages.Specialization.General.Elections {

	public interface IElectionsRegistrationMessage : IBlockchainMessage {
		IByteArray EncryptedMessage { get; set; }
		AccountId AccountId { get; set; }
		AccountId DelegateAccountId { get; set; }

		List<AccreditationCertificateMetadata> Certificates { get; }
	}

	/// <summary>
	///     This message will request on chain a registration so that we can participate in the comming elections
	/// </summary>
	public abstract class ElectionsRegistrationMessage : BlockchainMessage, IElectionsRegistrationMessage {

		// an encrypted instance of MinerRegistrationInfo
		public IByteArray EncryptedMessage { get; set; }

		/// <summary>
		///     We want this to be public and unencrypted, so everybody can know this account requested to be registered for the
		///     elections
		/// </summary>
		public AccountId AccountId { get; set; } = new AccountId();

		/// <summary>
		///     If we are delegating our winnings to another account (such as a mining pool), we indicate it here
		/// </summary>
		public AccountId DelegateAccountId { get; set; }

		public List<AccreditationCertificateMetadata> Certificates { get; private set; }

		protected override void RehydrateContents(IDataRehydrator rehydrator, IMessageRehydrationFactory rehydrationFactory) {
			base.RehydrateContents(rehydrator, rehydrationFactory);

			this.EncryptedMessage = rehydrator.ReadNonNullableArray();
			this.AccountId.Rehydrate(rehydrator);

			this.DelegateAccountId = rehydrator.ReadRehydratable<AccountId>();

			bool any = rehydrator.ReadBool();

			if(any) {
				int count = rehydrator.ReadByte();

				AccreditationCertificateMetadataFactory factory = this.CreateAccreditationCertificateMetadataFactory();
				this.Certificates = new List<AccreditationCertificateMetadata>();

				for(int i = 0; i < count; i++) {

					IByteArray data = rehydrator.ReadNonNullableArray();

					this.Certificates.Add(factory.RehydrateMetadata(data));
				}
			}
		}

		protected abstract AccreditationCertificateMetadataFactory CreateAccreditationCertificateMetadataFactory();

		protected override void DehydrateContents(IDataDehydrator dehydrator) {
			base.DehydrateContents(dehydrator);

			dehydrator.WriteNonNullable(this.EncryptedMessage);
			this.AccountId.Dehydrate(dehydrator);

			dehydrator.Write(this.DelegateAccountId);

			bool any = this.Certificates?.Any() ?? false;
			dehydrator.Write(any);

			if(any) {
				dehydrator.Write((byte) this.Certificates.Count);

				foreach(AccreditationCertificateMetadata entry in this.Certificates) {

					using(IDataDehydrator subDh = DataSerializationFactory.CreateDehydrator()) {
						entry.Dehydrate(subDh);

						IByteArray data = subDh.ToArray();

						dehydrator.WriteNonNullable(data);
						data.Return();
					}
				}
			}

		}

		protected override ComponentVersion<BlockchainMessageType> SetIdentity() {
			return (BlockchainMessageTypes.Instance.ELECTIONS_REGISTRATION, 1, 0);
		}
	}
}