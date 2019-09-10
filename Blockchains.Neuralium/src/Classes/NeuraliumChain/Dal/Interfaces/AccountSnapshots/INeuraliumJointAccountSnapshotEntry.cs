using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots {
	public interface INeuraliumJointAccountSnapshotEntry<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE, ACCOUNT_FREEZE> : INeuraliumJointAccountSnapshot<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE, ACCOUNT_FREEZE>, INeuraliumAccountSnapshotEntry<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, IJointAccountSnapshotEntry<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE>
		where ACCOUNT_FEATURE : INeuraliumAccountFeatureEntry
		where JOINT_MEMBER_FEATURE : IJointMemberAccountEntry
		where ACCOUNT_FREEZE : INeuraliumAccountFreezeEntry {
	}

}