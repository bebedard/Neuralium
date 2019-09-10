using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Envelopes;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.Base;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Gossip.BlockchainMessageReceivedGossip.Messages.V1;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.BlockchainMessageReceivedGossip.Messages.V1 {
	public class NeuraliumBlockchainMessageCreatedGossipMessage : BlockchainMessageCreatedGossipMessage<INeuraliumMessageEnvelope>, INeuraliumGossipWorkflowTriggerMessage<INeuraliumMessageEnvelope> {

		public override void Dehydrate(IDataDehydrator dehydrator) {
			base.Dehydrate(dehydrator);

		}
	}
}