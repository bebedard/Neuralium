using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage {

	public interface INeuraliumAccreditationCertificatesSnapshotContext : IAccreditationCertificatesSnapshotContext {
	}

	public interface INeuraliumAccreditationCertificatesSnapshotContext<ACCREDITATION_CERTIFICATE, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT> : IAccreditationCertificatesSnapshotContext<ACCREDITATION_CERTIFICATE, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>, INeuraliumAccreditationCertificatesSnapshotContext
		where ACCREDITATION_CERTIFICATE : class, INeuraliumAccreditationCertificateSnapshotEntry<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshotAccountEntry {
	}
}