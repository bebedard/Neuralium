using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Widgets;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Widgets {

	// 	=====================================
	// 	printing values for NeuraliumRejectionCodes
	// 	_____________________________________
	// 	GENESIS = 1;
	// SIMPLE_PRESENTATION = 2;
	// JOINT_PRESENTATION = 3;
	// KEY_CHANGE = 4;
	// ACCREDITATION_CERTIFICATE = 5;
	// SET_ACCOUNT_RECOVERY = 6;
	// MODERATION_KEY_CHANGE = 1007;
	// MODERATION_SECRET_KEY_CHANGE = 1008;
	// MODERATION_ACCOUNT_RESET = 1009;
	// MODERATION_ACCOUNT_RESET_WARNING = 1010;
	// MODERATION_RECLAIM_ACCOUNTS = 1011;
	// MODERATION_PRESENTATION = 1012;
	// MODERATION_OPERATING_RULES = 1013;
	// DEBUG = 1014;
	// NONE = 1015;
	// --------Children-----------
	// NEURALIUM_TRANSFER = 10001;
	// NEURALIUM_MULTI_TRANSFER = 10002;
	// NEURALIUM_MODERATOR_DESTROY_TOKENS = 10003;
	// NEURALIUM_REFILL_NEURLIUMS = 10004;
	// =====================================

	public class NeuraliumRejectionCodes : RejectionCodes {

		public readonly RejectionCode INSUFFICIENT_BALANCE;
		public readonly RejectionCode NEGATIVE_AMOUNT;
		public readonly RejectionCode NEGATIVE_TIP;

		public readonly RejectionCode ZERO_AMOUNT;

		static NeuraliumRejectionCodes() {
		}

		protected NeuraliumRejectionCodes() {
			this.INSUFFICIENT_BALANCE = this.CreateChildConstant();
			this.NEGATIVE_AMOUNT = this.CreateChildConstant();
			this.ZERO_AMOUNT = this.CreateChildConstant();
			this.NEGATIVE_TIP = this.CreateChildConstant();

			//this.PrintValues(";");
		}

		public static new NeuraliumRejectionCodes Instance { get; } = new NeuraliumRejectionCodes();
	}

}