using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Cards;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal {

	public interface INeuraliumCardsUtils : ICardUtils {

		void Copy(INeuraliumChainOptionsSnapshot source, INeuraliumChainOptionsSnapshot destination);
		void Copy(INeuraliumAccreditationCertificateSnapshot source, INeuraliumAccreditationCertificateSnapshot destination);
		void Copy(INeuraliumAccreditationCertificateSnapshotAccount source, INeuraliumAccreditationCertificateSnapshotAccount destination);

		void Copy(INeuraliumAccountKeysSnapshot source, INeuraliumAccountKeysSnapshot destination);
		void Copy(INeuraliumJointAccountSnapshot source, INeuraliumJointAccountSnapshot destination);
		void Copy(INeuraliumStandardAccountSnapshot source, INeuraliumStandardAccountSnapshot destination);
		void Copy(INeuraliumAccountSnapshot source, INeuraliumAccountSnapshot destination);
		void Copy(INeuraliumAccountFeature source, INeuraliumAccountFeature destination);
		void Copy(INeuraliumSnapshot source, INeuraliumSnapshot destination);
		void Copy(INeuraliumAccountFreeze source, INeuraliumAccountFreeze other);

		INeuraliumChainOptionsSnapshot Clone(INeuraliumChainOptionsSnapshot source);
		INeuraliumAccreditationCertificateSnapshot Clone(INeuraliumAccreditationCertificateSnapshot source);
		INeuraliumAccreditationCertificateSnapshotAccount Clone(INeuraliumAccreditationCertificateSnapshotAccount source);
		INeuraliumAccountKeysSnapshot Clone(INeuraliumAccountKeysSnapshot source);
		INeuraliumJointAccountSnapshot Clone(INeuraliumJointAccountSnapshot source);
		INeuraliumStandardAccountSnapshot Clone(INeuraliumStandardAccountSnapshot source);
		INeuraliumAccountSnapshot Clone(INeuraliumAccountSnapshot source);
		INeuraliumAccountFeature Clone(INeuraliumAccountFeature source);
		INeuraliumSnapshot Clone(INeuraliumSnapshot source);
		INeuraliumAccountFreeze Clone(INeuraliumAccountFreeze source);
	}

	//<INeuraliumAccountSnapshot, INeuraliumAccountFeature, INeuraliumStandardAccountSnapshot, INeuraliumJointAccountSnapshot, INeuraliumStandardAccountKeysSnapshot, INeuraliumAccreditationCertificateSnapshot, INeuraliumAccreditationCertificateSnapshotAccount, INeuraliumChainOptionsSnapshot>
	public class NeuraliumCardsUtils : CardsUtils, INeuraliumCardsUtils {

		public override void Copy(IChainOptionsSnapshot source, IChainOptionsSnapshot destination) {

			if(source is INeuraliumChainOptionsSnapshot castedSource && destination is INeuraliumChainOptionsSnapshot castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumChainOptionsSnapshot source, INeuraliumChainOptionsSnapshot destination) {
			base.Copy(source, destination);
		}

		public override void Copy(IAccreditationCertificateSnapshot source, IAccreditationCertificateSnapshot destination) {

			if(source is INeuraliumAccreditationCertificateSnapshot castedSource && destination is INeuraliumAccreditationCertificateSnapshot castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumAccreditationCertificateSnapshot source, INeuraliumAccreditationCertificateSnapshot destination) {
			destination.ProviderBountyshare = source.ProviderBountyshare;
			destination.InfrastructureServiceFees = source.InfrastructureServiceFees;

			base.Copy(source, destination);
		}

		public override void Copy(IAccreditationCertificateSnapshotAccount source, IAccreditationCertificateSnapshotAccount destination) {

			if(source is IAccreditationCertificateSnapshotAccount castedSource && destination is IAccreditationCertificateSnapshotAccount castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumAccreditationCertificateSnapshotAccount source, INeuraliumAccreditationCertificateSnapshotAccount destination) {

			base.Copy(source, destination);
		}

		public override void Copy(IAccountKeysSnapshot source, IAccountKeysSnapshot destination) {

			if(source is INeuraliumAccountKeysSnapshot castedSource && destination is INeuraliumAccountKeysSnapshot castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumAccountKeysSnapshot source, INeuraliumAccountKeysSnapshot destination) {
			base.Copy(source, destination);
		}

		public override void Copy(IAccountSnapshot source, IAccountSnapshot destination) {
			if(source is INeuraliumAccountSnapshot castedSource && destination is INeuraliumAccountSnapshot castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumAccountSnapshot source, INeuraliumAccountSnapshot destination) {
			destination.Balance = source.Balance;

			this.CopyArray<INeuraliumAccountFreeze>(source, destination);

			base.Copy(source, destination);
		}

		public override void Copy(IJointMemberAccount source, IJointMemberAccount destination) {
			if(source is INeuraliumJointMemberAccount castedSource && destination is INeuraliumJointMemberAccount castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public override void Copy(IJointAccountSnapshot source, IJointAccountSnapshot destination) {

			if(source is INeuraliumJointAccountSnapshot castedSource && destination is INeuraliumJointAccountSnapshot castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumJointAccountSnapshot source, INeuraliumJointAccountSnapshot destination) {

			base.Copy(source, destination);

			this.Copy(source, (INeuraliumAccountSnapshot) destination);
		}

		public override void Copy(IStandardAccountSnapshot source, IStandardAccountSnapshot destination) {

			if(source is INeuraliumStandardAccountSnapshot castedSource && destination is INeuraliumStandardAccountSnapshot castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumStandardAccountSnapshot source, INeuraliumStandardAccountSnapshot destination) {
			base.Copy(source, destination);

			this.Copy(source, (INeuraliumAccountSnapshot) destination);
		}

		public override void Copy(IAccountFeature source, IAccountFeature destination) {

			if(source is INeuraliumAccountFeature castedSource && destination is INeuraliumAccountFeature castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumAccountFeature source, INeuraliumAccountFeature destination) {
			base.Copy(source, destination);
		}

		public override void Copy(ISnapshot source, ISnapshot destination) {

			if(source is INeuraliumSnapshot castedSource && destination is INeuraliumSnapshot castedDestination) {
				this.Copy(castedSource, castedDestination);
			} else {
				base.Copy(source, destination);
			}
		}

		public void Copy(INeuraliumSnapshot source, INeuraliumSnapshot destination) {
			if(source is INeuraliumStandardAccountSnapshot castedSource1 && destination is INeuraliumStandardAccountSnapshot castedDestination1) {
				this.Copy(castedSource1, castedDestination1);
			} else if(source is INeuraliumJointAccountSnapshot castedSource2 && destination is INeuraliumJointAccountSnapshot castedDestination2) {
				this.Copy(castedSource2, castedDestination2);
			} else if(source is INeuraliumAccountKeysSnapshot castedSource3 && destination is INeuraliumAccountKeysSnapshot castedDestination3) {
				this.Copy(castedSource3, castedDestination3);
			} else if(source is INeuraliumAccreditationCertificateSnapshot castedSource4 && destination is INeuraliumAccreditationCertificateSnapshot castedDestination4) {
				this.Copy(castedSource4, castedDestination4);
			} else if(source is INeuraliumChainOptionsSnapshot castedSource5 && destination is INeuraliumChainOptionsSnapshot castedDestination5) {
				this.Copy(castedSource5, castedDestination5);
			} else if(source is INeuraliumAccountFeature castedSource6 && destination is INeuraliumAccountFeature castedDestination6) {
				this.Copy(castedSource6, castedDestination6);
			} else if(source is INeuraliumJointMemberAccount castedSource7 && destination is INeuraliumJointMemberAccount castedDestination7) {
				this.Copy(castedSource7, castedDestination7);
			} else if(source is INeuraliumAccountFreeze castedSource8 && destination is INeuraliumAccountFreeze castedDestination8) {
				this.Copy(castedSource8, castedDestination8);
			} else if(source is INeuraliumAccreditationCertificateSnapshotAccount castedSource9 && destination is INeuraliumAccreditationCertificateSnapshotAccount castedDestination9) {
				this.Copy(castedSource9, castedDestination9);
			} else {
				throw new InvalidOperationException();
			}
		}

		public void Copy(INeuraliumAccountFreeze source, INeuraliumAccountFreeze destination) {
			destination.FreezeId = source.FreezeId;
			destination.Amount = source.Amount;

		}

		public INeuraliumChainOptionsSnapshot Clone(INeuraliumChainOptionsSnapshot source) {
			return this.CopyClone(source);
		}

		public INeuraliumAccreditationCertificateSnapshot Clone(INeuraliumAccreditationCertificateSnapshot source) {
			return this.CopyClone(source);
		}

		public INeuraliumAccreditationCertificateSnapshotAccount Clone(INeuraliumAccreditationCertificateSnapshotAccount source) {
			return this.CopyClone(source);
		}

		public INeuraliumAccountKeysSnapshot Clone(INeuraliumAccountKeysSnapshot source) {
			return this.CopyClone(source);
		}

		public INeuraliumJointAccountSnapshot Clone(INeuraliumJointAccountSnapshot source) {
			return this.CopyClone(source);
		}

		public INeuraliumStandardAccountSnapshot Clone(INeuraliumStandardAccountSnapshot source) {
			return this.CopyClone(source);
		}

		public INeuraliumAccountSnapshot Clone(INeuraliumAccountSnapshot source) {
			return this.CopyClone(source);
		}

		public INeuraliumAccountFeature Clone(INeuraliumAccountFeature source) {
			return this.CopyClone(source);
		}

		public INeuraliumSnapshot Clone(INeuraliumSnapshot source) {
			return this.CopyClone(source);
		}

		public INeuraliumAccountFreeze Clone(INeuraliumAccountFreeze source) {
			return this.CopyClone(source);
		}

		public void Copy(INeuraliumJointMemberAccount source, INeuraliumJointMemberAccount destination) {
			base.Copy(source, destination);
		}

	#region Singleton

		static NeuraliumCardsUtils() {
		}

		private NeuraliumCardsUtils() {

		}

		public static NeuraliumCardsUtils Instance { get; } = new NeuraliumCardsUtils();

	#endregion

	}
}