using MessagePack;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.DataStructures {
	
	[MessagePackObject(keyAsPropertyName: true)]
	public class NeuraliumSynthesizedBlockApi : SynthesizedBlockAPI<NeuraliumSynthesizedBlockApi.NeuraliumSynthesizedElectionResultAPI, NeuraliumSynthesizedBlockApi.NeuraliumSynthesizedGenesisBlockAPI> {

		[MessagePackObject(keyAsPropertyName: true)]
		public class NeuraliumSynthesizedElectionResultAPI : SynthesizedElectionResultAPI {

			
			public decimal BountyShare { get; set; }
			public decimal Tips { get; set; }
		}
		
		[MessagePackObject(keyAsPropertyName: true)]
		public class NeuraliumSynthesizedGenesisBlockAPI : SynthesizedGenesisBlockAPI {
		}
	}
}