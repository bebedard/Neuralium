using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Envelopes;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.Base;
using Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Messages;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Gossip.TransactionReceivedGossip;
using Neuralia.Blockchains.Core.P2p.Connections;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Gossip.TransactionReceivedGossip {

	/// <summary>
	///     This workflow is activated when we receive a gossip messgae informing us that a new
	///     transaction has been created.
	///     we validate it and then add it to our quarantine
	/// </summary>
	public class NeuraliumNewTransactionReceivedGossipWorkflow : NewTransactionReceivedGossipWorkflow<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider, INeuraliumGossipMessageSet<INeuraliumGossipWorkflowTriggerMessage<INeuraliumTransactionEnvelope>, INeuraliumTransactionEnvelope>, INeuraliumGossipWorkflowTriggerMessage<INeuraliumTransactionEnvelope>, INeuraliumTransactionEnvelope> {
		public NeuraliumNewTransactionReceivedGossipWorkflow(INeuraliumCentralCoordinator centralCoordinator, INeuraliumGossipMessageSet<INeuraliumGossipWorkflowTriggerMessage<INeuraliumTransactionEnvelope>, INeuraliumTransactionEnvelope> triggerMessage, PeerConnection peerConnectionn) : base(NeuraliumBlockchainTypes.NeuraliumInstance.Neuralium, centralCoordinator, triggerMessage, peerConnectionn) {
		}
	}
}