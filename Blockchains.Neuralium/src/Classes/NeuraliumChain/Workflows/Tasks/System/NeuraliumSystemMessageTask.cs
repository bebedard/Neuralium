using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Tasks.System;
using Neuralia.Blockchains.Core;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Tasks.System {
	public class NeuraliumSystemMessageTask : SystemMessageTask {
		public NeuraliumSystemMessageTask(BlockchainSystemEventType eventType) : base(eventType) {
		}
	}
}