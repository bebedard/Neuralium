using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Storage;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Storage {

	public interface INeuraliumTrackedAccountsDal : ITrackedAccountsDal {
	}

	public interface INeuraliumTrackedAccountsDal<ACCOUNT_SNAPSHOT_CONTEXT> : ITrackedAccountsDal<ACCOUNT_SNAPSHOT_CONTEXT>, INeuraliumTrackedAccountsDal
		where ACCOUNT_SNAPSHOT_CONTEXT : INeuraliumTrackedAccountsContext {
	}

}