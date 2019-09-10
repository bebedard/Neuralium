using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Wallet.Account;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core.Configuration;
using Neuralia.Blockchains.Core.Cryptography.Encryption.Symetrical;
using Neuralia.Blockchains.Core.Tools;
using Neuralia.Blockchains.Tools.Cryptography;
using Neuralia.Blockchains.Tools.Data;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Wallet.Account {
	public interface INeuraliumWalletAccount : IWalletAccount {
	}

	public class NeuraliumWalletAccount : WalletAccount, INeuraliumWalletAccount {

		public EncryptorParameters NeuraliumTimelineFileEncryptionParameters { get; set; }
		public ByteArray NeuraliumTimelineFileSecret { get; set; }

		public override void InitializeNewEncryptionParameters(BlockchainServiceSet serviceSet) {
			base.InitializeNewEncryptionParameters(serviceSet);

			this.InitializeNewNeuraliumTimelineEncryptionParameters(serviceSet);
		}

		public override void ClearEncryptionParameters() {
			base.ClearEncryptionParameters();

			this.ClearNeuraliumTimelineEncryptionParameters();
		}

		public void ClearNeuraliumTimelineEncryptionParameters() {
			this.NeuraliumTimelineFileEncryptionParameters = null;
			this.NeuraliumTimelineFileSecret = null;
		}

		public virtual void InitializeNewNeuraliumTimelineEncryptionParameters(BlockchainServiceSet serviceSet) {
			// create those no matter what
			if(this.NeuraliumTimelineFileEncryptionParameters == null) {
				this.NeuraliumTimelineFileEncryptionParameters = FileEncryptorUtils.GenerateEncryptionParameters(GlobalSettings.ApplicationSettings);
				var secretKey = new byte[333];
				GlobalRandom.GetNextBytes(secretKey);
				this.NeuraliumTimelineFileSecret = secretKey;
			}
		}
	}
}