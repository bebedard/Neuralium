using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Cards;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards {

	public interface INeuraliumJointAccountSnapshot : IJointAccountSnapshot, INeuraliumAccountSnapshot {
	}

	public interface INeuraliumJointAccountSnapshot<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE, ACCOUNT_FREEZE> : IJointAccountSnapshot<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE>, INeuraliumAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, INeuraliumJointAccountSnapshot
		where ACCOUNT_FEATURE : INeuraliumAccountFeature
		where JOINT_MEMBER_FEATURE : IJointMemberAccount
		where ACCOUNT_FREEZE : INeuraliumAccountFreeze {
	}
}