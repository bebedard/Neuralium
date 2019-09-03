using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Envelopes;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.Base;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Messages;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Gossip.BlockReceivedGossip;
using Neuralia.Blockchains.Core.P2p.Connections;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.BlockReceivedGossip {

	/// <summary>
	///     This workflow is activated when we receive a gossip messgae informing us that a new
	///     transaction has been created.
	///     we validate it and then add it to our quarantine
	/// </summary>
	public class NeuraliumNewBlockReceivedGossipWorkflow : NewBlockReceivedGossipWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider, INeuraliumGossipMessageSet<INeuraliumGossipWorkflowTriggerMessage<INeuraliumBlockEnvelope>, INeuraliumBlockEnvelope>, INeuraliumGossipWorkflowTriggerMessage<INeuraliumBlockEnvelope>, INeuraliumBlockEnvelope> {
		public NeuraliumNewBlockReceivedGossipWorkflow(INeuraliumCentralCoordinator centralCoordinator, INeuraliumGossipMessageSet<INeuraliumGossipWorkflowTriggerMessage<INeuraliumBlockEnvelope>, INeuraliumBlockEnvelope> triggerMessage, PeerConnection peerConnectionn) : base(NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium, centralCoordinator, triggerMessage, peerConnectionn) {
		}
	}
}