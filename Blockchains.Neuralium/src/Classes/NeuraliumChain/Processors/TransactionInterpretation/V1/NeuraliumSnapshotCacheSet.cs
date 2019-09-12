using Blockchains.Neuralium.Classes.NeuraliumChain.Dal;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Processors.TransactionInterpretation.V1;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Processors.TransactionInterpretation.V1 {
	public class NeuraliumSnapshotCacheSet<ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT> : SnapshotCacheSet<ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_SNAPSHOT, STANDARD_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_SNAPSHOT, JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, STANDARD_ACCOUNT_KEY_SNAPSHOT, ACCREDITATION_CERTIFICATE_SNAPSHOT, ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT, CHAIN_OPTIONS_SNAPSHOT>
		where ACCOUNT_SNAPSHOT : INeuraliumAccountSnapshot
		where STANDARD_ACCOUNT_SNAPSHOT : class, INeuraliumStandardAccountSnapshot<STANDARD_ACCOUNT_FEATURE_SNAPSHOT, STANDARD_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where STANDARD_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeature, new()
		where STANDARD_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreeze, new()
		where JOINT_ACCOUNT_SNAPSHOT : class, INeuraliumJointAccountSnapshot<JOINT_ACCOUNT_FEATURE_SNAPSHOT, JOINT_ACCOUNT_MEMBERS_SNAPSHOT, JOINT_ACCOUNT_FREEZE_SNAPSHOT>, ACCOUNT_SNAPSHOT, new()
		where JOINT_ACCOUNT_FEATURE_SNAPSHOT : class, INeuraliumAccountFeature, new()
		where JOINT_ACCOUNT_MEMBERS_SNAPSHOT : class, INeuraliumJointMemberAccount, new()
		where JOINT_ACCOUNT_FREEZE_SNAPSHOT : class, INeuraliumAccountFreeze, new()
		where STANDARD_ACCOUNT_KEY_SNAPSHOT : class, INeuraliumStandardAccountKeysSnapshot, new()
		where ACCREDITATION_CERTIFICATE_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshot<ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT>, new()
		where ACCREDITATION_CERTIFICATE_ACCOUNT_SNAPSHOT : class, INeuraliumAccreditationCertificateSnapshotAccount, new()
		where CHAIN_OPTIONS_SNAPSHOT : class, INeuraliumChainOptionsSnapshot, new() {

		public NeuraliumSnapshotCacheSet(INeuraliumCardsUtils cardUtils) : base(cardUtils) {
		}
	}
}