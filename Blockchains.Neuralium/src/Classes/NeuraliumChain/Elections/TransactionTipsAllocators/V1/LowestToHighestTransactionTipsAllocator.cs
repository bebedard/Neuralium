using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.TransactionTipsAllocationMethods;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Results.V1;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Core.General.Types;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Tools.Data;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Elections.TransactionTipsAllocators.V1 {
	public class LowestToHighestTransactionTipsAllocator : TransactionTipsAllocator {

		public LowestToHighestTransactionTipsAllocator(ITransactionTipsAllocationMethod TransactionTipsAllocationMethod) : base(TransactionTipsAllocationMethod) {
		}

		public override void AllocateTransactionTips(IFinalElectionResults result, Dictionary<AccountId, (IByteArray electionHash, List<TransactionId> transactionIds)> electionResults, Dictionary<TransactionId, Amount> transactionTips) {

			var primeElectionResults = electionResults.Where(e => result.ElectedCandidates.ContainsKey(e.Key)).ToDictionary(e => e.Key, e => e.Value);

			// first we get the election hash into a number for the prime elected only
			var preparedElectionResults = primeElectionResults.ToDictionary(t => t.Key, t => new BigInteger(t.Value.electionHash.ToExactByteArray()));

			// sort the elected by the size of their election hash; lowest to highest
			var sortedElectedAccounts = preparedElectionResults.OrderBy(t => t.Value).Select(a => a.Key).ToList();

			// copy their selections, so we get the order priority and order them by tip worth
			var electedSelections = primeElectionResults.ToDictionary(t => t.Key, t => t.Value.transactionIds.Where(transactionTips.ContainsKey).OrderByDescending(t2 => transactionTips[t2]).ToList());

			var transactionAccountMappings = primeElectionResults.SelectMany(e => e.Value.transactionIds.Where(transactionTips.ContainsKey).Select((t, index) => (TransactionId: t, index, AccountId: e.Key)));
			var groupedTransactions = transactionAccountMappings.GroupBy(t => t.TransactionId).ToDictionary(g => g.Key, g => g);

			// now the unique list of transactions
			var allTransactions = transactionTips.Keys.ToList();

			// now loop all transactions and allocate them correctly

			int currentElectedIndex = 0;

			// loop each sorted elected in order and assign transactions until nothing is left
			while(allTransactions.Any()) {

				AccountId currentId = sortedElectedAccounts[currentElectedIndex];

				if(electedSelections.ContainsKey(currentId)) {
					TransactionId selectedTransaction = electedSelections[currentId].FirstOrDefault();

					if(selectedTransaction != null) {

						// ok, its a selection, lets assign it to the elected
						result.ElectedCandidates[currentId].Transactions.Add(selectedTransaction);

						// lets remove this transaction from all the lists to clean our workspace and make sure its not assigned again, it now out of the circuit
						foreach((TransactionId TransactionId, int index, AccountId AccountId) accountSets in groupedTransactions[selectedTransaction]) {
							electedSelections[accountSets.AccountId].Remove(selectedTransaction);
						}

						allTransactions.Remove(selectedTransaction);
					}

					if(!electedSelections[currentId].Any()) {
						// this elected has no more transactions, we take it out
						electedSelections.Remove(currentId);
					}
				}

				currentElectedIndex++;

				if(currentElectedIndex == sortedElectedAccounts.Count) {
					currentElectedIndex = 0;
				}
			}
		}
	}
}