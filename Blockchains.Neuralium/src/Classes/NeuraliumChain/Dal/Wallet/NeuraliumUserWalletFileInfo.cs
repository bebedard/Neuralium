using Blockchains.Neuralium.Classes.NeuraliumChain.Wallet;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Wallet;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core.Cryptography.Passphrases;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Wallet {

	public interface INeuraliumUserWalletFileInfo : IUserWalletFileInfo {
	}

	public class NeuraliumUserWalletFileInfo : UserWalletFileInfo<NeuraliumUserWallet>, INeuraliumUserWalletFileInfo {

		public NeuraliumUserWalletFileInfo(string filename, BlockchainServiceSet serviceSet, IWalletSerialisationFal serialisationFal, WalletPassphraseDetails walletSecurityDetails) : base(filename, serviceSet, serialisationFal, walletSecurityDetails) {
		}
	}
}