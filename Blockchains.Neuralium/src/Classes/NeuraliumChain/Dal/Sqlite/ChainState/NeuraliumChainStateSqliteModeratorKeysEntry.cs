using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.ChainState;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.ChainState;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.ChainState {

	public interface INeuraliumChainStateSqliteModeratorKeysEntry : INeuraliumChainStateModeratorKeysEntry<NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry>, IChainStateSqliteModeratorKeysEntry<NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry> {
	}

	public class NeuraliumChainStateSqliteModeratorKeysEntry : ChainStateSqliteModeratorKeysEntry<NeuraliumChainStateSqliteEntry, NeuraliumChainStateSqliteModeratorKeysEntry>, INeuraliumChainStateSqliteModeratorKeysEntry {
	}
}