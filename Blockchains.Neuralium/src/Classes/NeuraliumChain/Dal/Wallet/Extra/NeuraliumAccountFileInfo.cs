using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Wallet.Extra;
using Neuralia.Blockchains.Core.Cryptography.Passphrases;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Wallet.Extra {

	public interface INeuraliumAccountFileInfo : IAccountFileInfo {
		INeuraliumWalletTimelineFileInfo WalletTimelineFileInfo { get; set; }
	}

	public class NeuraliumAccountFileInfo : AccountFileInfo, INeuraliumAccountFileInfo {

		public NeuraliumAccountFileInfo(AccountPassphraseDetails accountSecurityDetails) : base(accountSecurityDetails) {
		}

		public INeuraliumWalletTimelineFileInfo WalletTimelineFileInfo { get; set; }

		public override void Load() {
			base.Load();

			this.WalletTimelineFileInfo.Load();
		}

		public override void Save() {
			base.Save();

			this.WalletTimelineFileInfo.Save();
		}

		public override void ChangeEncryption() {
			base.ChangeEncryption();

			this.WalletTimelineFileInfo.ChangeEncryption();
		}

		public override void Reset() {
			base.Reset();

			this.WalletTimelineFileInfo.Reset();
		}

		public override void ReloadFileBytes() {
			base.ReloadFileBytes();

			this.WalletTimelineFileInfo.ReloadFileBytes();
		}
	}
}