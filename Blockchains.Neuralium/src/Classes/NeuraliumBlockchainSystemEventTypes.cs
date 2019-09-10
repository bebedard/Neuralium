using Neuralia.Blockchains.Core;

namespace Blockchains.Neuralium.Classes {
	public class NeuraliumBlockchainSystemEventTypes : BlockchainSystemEventTypes {

		public readonly BlockchainSystemEventType AccountTotalUpdated;
		public readonly BlockchainSystemEventType MiningBountyAllocated;
		public readonly BlockchainSystemEventType NeuraliumMiningPrimeElected;

		static NeuraliumBlockchainSystemEventTypes() {
		}

		protected NeuraliumBlockchainSystemEventTypes() {
			this.AccountTotalUpdated = this.CreateChildConstant();
			this.MiningBountyAllocated = this.CreateChildConstant();
			this.NeuraliumMiningPrimeElected = this.CreateChildConstant();

			//for debugging
			//this.PrintValues(",");
		}

		public static NeuraliumBlockchainSystemEventTypes NeuraliumInstance { get; } = new NeuraliumBlockchainSystemEventTypes();
	}
}