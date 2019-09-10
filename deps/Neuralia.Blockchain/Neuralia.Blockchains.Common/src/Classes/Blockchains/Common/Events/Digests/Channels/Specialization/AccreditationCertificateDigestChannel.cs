using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Index.Sqlite;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Specialization.Cards;
using Neuralia.Blockchains.Core.General.Versions;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Specialization {
	public interface IAccreditationCertificateDigestChannel : IDigestChannel {
	}

	public interface IAccreditationCertificateDigestChannel<ACCREDITATION_CARD> : IAccreditationCertificateDigestChannel
		where ACCREDITATION_CARD : class, IAccreditationCertificateDigestChannelCard {
		ACCREDITATION_CARD GetAccreditationCertificate(int id);

		List<ACCREDITATION_CARD> GetAccreditationCertificates();
	}

	public abstract class AccreditationCertificateDigestChannel<ACCREDITATION_CARD> : DigestChannel<AccreditationCertificateDigestChannel.AccreditationCertificateDigestChannelBands, ACCREDITATION_CARD, int, int, int>, IAccreditationCertificateDigestChannel<ACCREDITATION_CARD>
		where ACCREDITATION_CARD : class, IAccreditationCertificateDigestChannelCard, new() {

		public enum FileTypes {
			Certificates = 1
		}

		protected const string CERTIFICATES_CHANNEL = "certificates";
		protected const string CERTIFICATES_BAND_NAME = "certificates";

		protected AccreditationCertificateDigestChannel(string folder) : base(folder, CERTIFICATES_CHANNEL) {
		}

		public override DigestChannelType ChannelType => DigestChannelTypes.Instance.AccreditationCertificates;

		public ACCREDITATION_CARD GetAccreditationCertificate(int id) {

			var results = this.channelBandIndexSet.QueryCard(id);

			if(results.IsEmpty) {
				return null;
			}

			return results[AccreditationCertificateDigestChannel.AccreditationCertificateDigestChannelBands.Certificates];
		}

		public List<ACCREDITATION_CARD> GetAccreditationCertificates() {
			// this works because we have only one channel for now
			var castedIndex = (SingleSqliteChannelBandIndex<AccreditationCertificateDigestChannel.AccreditationCertificateDigestChannelBands, ACCREDITATION_CARD, int, int, int>) this.channelBandIndexSet.BandIndices.Values.Single();

			return castedIndex.QueryCards();
		}

		protected override void BuildBandsIndices() {

			this.channelBandIndexSet.AddIndex(1, new SingleSqliteChannelBandIndex<AccreditationCertificateDigestChannel.AccreditationCertificateDigestChannelBands, ACCREDITATION_CARD, int, int, int>(CERTIFICATES_BAND_NAME, this.baseFolder, this.scopeFolder, AccreditationCertificateDigestChannel.AccreditationCertificateDigestChannelBands.Certificates, new FileSystem(), key => key));
		}

		protected override ComponentVersion<DigestChannelType> SetIdentity() {
			return (DigestChannelTypes.Instance.AccreditationCertificates, 1, 0);
		}
	}

	public static class AccreditationCertificateDigestChannel {
		[Flags]
		public enum AccreditationCertificateDigestChannelBands {
			Certificates = 1
		}
	}
}