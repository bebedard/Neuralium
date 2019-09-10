using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards.Implementations;
using LiteDB;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Tools;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account.Snapshots;
using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account.Snapshots {

	public interface INeuraliumWalletJointAccountSnapshot<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE, ACCOUNT_FREEZE> : IWalletJointAccountSnapshot<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE>, INeuraliumWalletAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE>, INeuraliumJointAccountSnapshot<ACCOUNT_FEATURE, JOINT_MEMBER_FEATURE, ACCOUNT_FREEZE>
		where ACCOUNT_FEATURE : INeuraliumAccountFeature, new()
		where JOINT_MEMBER_FEATURE : INeuraliumJointMemberAccount
		where ACCOUNT_FREEZE : INeuraliumAccountFreeze {
	}

	public interface INeuraliumWalletJointAccountSnapshot : INeuraliumWalletJointAccountSnapshot<NeuraliumAccountFeature, NeuraliumJointMemberAccount, NeuraliumAccountFreeze>, INeuraliumWalletAccountSnapshot {
	}

	public class NeuraliumWalletJointAccountSnapshot : WalletJointAccountSnapshot<NeuraliumAccountFeature, NeuraliumJointMemberAccount>, INeuraliumWalletJointAccountSnapshot {
		public byte? FreezeDataVersion { get; set; }
		public byte[] FreezeData { get; set; }

		public Amount Balance { get; set; } = new Amount();
		public List<NeuraliumAccountFreeze> AccountFreezes { get; } = new List<NeuraliumAccountFreeze>();

		public void CreateNewCollectionEntry(out INeuraliumAccountFreeze result) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.CreateNewCollectionEntry(this.AccountFreezes, out result);
		}

		public void AddCollectionEntry(INeuraliumAccountFreeze entry) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.AddCollectionEntry(entry, this.AccountFreezes);
		}

		public void RemoveCollectionEntry(Func<INeuraliumAccountFreeze, bool> predicate) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.RemoveCollectionEntry(predicate, this.AccountFreezes);
		}

		public INeuraliumAccountFreeze GetCollectionEntry(Func<INeuraliumAccountFreeze, bool> predicate) {
			return TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollectionEntry(predicate, this.AccountFreezes);
		}

		public List<INeuraliumAccountFreeze> GetCollectionEntries(Func<INeuraliumAccountFreeze, bool> predicate) {
			return TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollectionEntries(predicate, this.AccountFreezes);
		}

		[BsonIgnore]
		ImmutableList<INeuraliumAccountFreeze> ITypedCollectionExposure<INeuraliumAccountFreeze>.CollectionCopy => TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollection(this.AccountFreezes);
	}
}