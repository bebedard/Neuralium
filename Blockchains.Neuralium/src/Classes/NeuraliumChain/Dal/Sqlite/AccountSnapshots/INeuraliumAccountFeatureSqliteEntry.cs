using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Sqlite.AccountSnapshots;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Sqlite.AccountSnapshots {

	public interface INeuraliumAccountFeatureSqliteEntry : IAccountFeatureSqliteEntry, INeuraliumAccountFeatureEntry {
	}

	public class NeuraliumAccountFeatureSqliteEntry : AccountFeatureSqliteEntry, INeuraliumAccountFeatureSqliteEntry {
	}
}