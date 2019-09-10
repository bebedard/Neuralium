using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.ChainState;
using Blockchains.Neuralium.Classes.NeuraliumChain.Factories;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.ChainState;
using Neuralia.Blockchains.Common.Classes.Tools;
using Neuralia.Blockchains.Core.Configuration;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.ChainState {

	public interface INeuraliumChainStateSqliteDal : INeuraliumChainStateDal<NeuraliumChainStateSqliteContext, NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry> {
	}

	public class NeuraliumChainStateSqliteDal : ChainStateSqliteDal<NeuraliumChainStateSqliteContext, NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry>, INeuraliumChainStateSqliteDal {

		public NeuraliumChainStateSqliteDal(string folderPath, BlockchainServiceSet serviceSet, INeuraliumChainDalCreationFactory chainDalCreationFactory, AppSettingsBase.SerializationTypes serializationType) : base(folderPath, serviceSet, chainDalCreationFactory, serializationType) {

		}
	}
}