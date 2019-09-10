using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures.Validation;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.General.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Moderator;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Transactions.Specialization.Tags;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.Validation;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Creation.Transactions {
	public static class NeuraliumTransactionCreationUtils {
		public static ValidationResult ValidateTransaction(ITransaction transaction) {
			if(transaction is INeuraliumTransaction neuraliumTransaction) {

				if(neuraliumTransaction is INeuraliumTransferTransaction neuraliumTransferTransaction) {
					if(neuraliumTransferTransaction.Amount <= 0) {
						//this is an error.
						return new TransactionValidationResult(ValidationResult.ValidationResults.Invalid, NeuraliumTransactionValidationErrorCodes.Instance.INVALID_AMOUNT);
					}
				} else if(neuraliumTransaction is INeuraliumMultiTransferTransaction neuraliumMultiTransferTransaction) {
					if(neuraliumMultiTransferTransaction.Total <= 0) {
						//this is an error.
						return new TransactionValidationResult(ValidationResult.ValidationResults.Invalid, NeuraliumTransactionValidationErrorCodes.Instance.INVALID_AMOUNT);
					}
				}
			}

			if(transaction is INeuraliumModerationTransaction neuraliumModeratorTransaction) {

				if(neuraliumModeratorTransaction is IEmittingTransaction neuraliumTransferTransaction) {
					if(neuraliumTransferTransaction.Amount <= 0) {
						//this is an error.
						return new TransactionValidationResult(ValidationResult.ValidationResults.Invalid, NeuraliumTransactionValidationErrorCodes.Instance.INVALID_AMOUNT);
					}
				} else if(neuraliumModeratorTransaction is IMultiEmittingTransaction neuraliumMultiTransferTransaction) {

					decimal total = 0;

					if(neuraliumMultiTransferTransaction.Recipients.Any()) {
						total = neuraliumMultiTransferTransaction.Recipients.Sum(t2 => t2.Amount.Value);
					}

					if(total <= 0) {
						//this is an error.
						return new TransactionValidationResult(ValidationResult.ValidationResults.Invalid, NeuraliumTransactionValidationErrorCodes.Instance.INVALID_AMOUNT);
					}
				}
			}

			if(transaction is ITipTransaction tipTransaction) {

				if(tipTransaction.Tip < 0) {
					//this is an error.
					return new TransactionValidationResult(ValidationResult.ValidationResults.Invalid, NeuraliumTransactionValidationErrorCodes.Instance.NEGATIVE_TIP);
				}

			}

			return new ValidationResult(ValidationResult.ValidationResults.Valid);
		}
	}
}