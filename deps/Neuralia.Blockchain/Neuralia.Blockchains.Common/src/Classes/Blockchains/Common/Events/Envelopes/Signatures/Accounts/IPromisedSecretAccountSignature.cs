using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Envelopes.Signatures.Accounts {
	public interface IPromisedSecretAccountSignature : IAccountSignature {

		IByteArray PromisedPublicKey { get; set; }
	}
}