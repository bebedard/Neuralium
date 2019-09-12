using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Base;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Transactions.Identifiers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Bases;
using Neuralia.Blockchains.Core.Workflows.Tasks.Routing;
using Neuralia.Blockchains.Tools.Data;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.debugging {
	public interface IInsertDebugConfirmWorkflow : INeuraliumChainWorkflow {
	}

	public class InsertDebugConfirmWorkflow : NeuraliumChainWorkflow, IInsertDebugConfirmWorkflow {
		private readonly TransactionId guid;
		private readonly IByteArray hash;

		public InsertDebugConfirmWorkflow(TransactionId guid, IByteArray hash, INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
			this.guid = guid;
			this.hash = hash;
		}

		protected override void PerformWork(IChainWorkflow workflow, TaskRoutingContext taskRoutingContext) {
			throw new NotImplementedException();
		}
	}
}