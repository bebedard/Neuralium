using System;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.TransactionSelectionMethods;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Contexts.V1;
using Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Specialization.Elections.Results;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Serialization;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Contexts;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Contexts.TransactionSelectionMethods;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Blocks.Specialization.Elections.Results;
using Neuralia.Blockchains.Core.Compression;
using Neuralia.Blockchains.Core.General.Versions;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Blocks.Serialization {
	public interface INeuraliumBlockComponentsRehydrationFactory : IBlockComponentsRehydrationFactory {
	}

	public class NeuraliumBlockComponentsRehydrationFactory : BlockComponentsRehydrationFactory, INeuraliumBlockComponentsRehydrationFactory {
		public override IElectionContext CreateElectionContext(IByteArray compressedContext) {

			GzipCompression compressor = new GzipCompression();

			IByteArray decompressedContext = compressor.Decompress(compressedContext);

			IDataRehydrator electionContextRehydrator = DataSerializationFactory.CreateRehydrator(decompressedContext);

			var version = electionContextRehydrator.RehydrateRewind<ComponentVersion<ElectionContextType>>();

			IElectionContext context = null;

			if(version.Type == ElectionContextTypes.Instance.Active) {
				if(version == (1, 0)) {
					context = new NeuraliumActiveElectionContext();
				}
			}

			if(version.Type == ElectionContextTypes.Instance.Passive) {
				if(version == (1, 0)) {
					context = new NeuraliumPassiveElectionContext();
				}
			}

			if(context == null) {
				throw new ApplicationException("Unrecognized election context version.");
			}

			context.Rehydrate(electionContextRehydrator, this);

			decompressedContext.Return();

			return context;
		}

		public override ITransactionSelectionMethodFactory CreateTransactionSelectionMethodFactory() {
			return new NeuraliumTransactionSelectionMethodFactory();
		}

		public override IElectionResultsRehydrator CreateElectionResultsRehydrator() {
			return new NeuraliumElectionResultsRehydrator();
		}
	}
}