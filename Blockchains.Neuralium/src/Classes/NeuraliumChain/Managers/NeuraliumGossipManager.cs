using System.Collections.Generic;
using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.DataStructures;
using Blockchains.Neuralium.Classes.NeuraliumChain.Processors.BlockInsertionTransaction;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Managers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Processors.BlockInsertionTransaction;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Managers {
	public interface INeuraliumGossipManager : IGossipManager<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider> {
	}

	public class NeuraliumGossipManager : GossipManager<INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumGossipManager {

		public NeuraliumGossipManager(INeuraliumCentralCoordinator centralCoordinator) : base(centralCoordinator) {
		}

	}
}