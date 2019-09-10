using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Envelopes;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.Base;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Gossip.BlockReceivedGossip.Messages.V1;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.BlockReceivedGossip.Messages.V1 {
	public class NeuraliumBlockCreatedGossipMessage : BlockCreatedGossipMessage<INeuraliumBlockEnvelope>, INeuraliumGossipWorkflowTriggerMessage<INeuraliumBlockEnvelope> {

		public override void Dehydrate(IDataDehydrator dehydrator) {
			base.Dehydrate(dehydrator);

		}
	}
}