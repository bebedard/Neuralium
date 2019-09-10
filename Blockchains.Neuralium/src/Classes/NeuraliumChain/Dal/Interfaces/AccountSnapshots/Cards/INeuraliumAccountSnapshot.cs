using System.Collections.Generic;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Tools;
using Neuralia.Blockchains.Core.General.Types.Specialized;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards {

	public interface INeuraliumAccountSnapshot : INeuraliumSnapshot, IAccountSnapshot, ITypedCollectionExposure<INeuraliumAccountFreeze> {
		/// <summary>
		///     our account balance
		/// </summary>
		Amount Balance { get; set; }
	}

	public interface INeuraliumAccountSnapshot<ACCOUNT_FREEZE> : INeuraliumAccountSnapshot
		where ACCOUNT_FREEZE : INeuraliumAccountFreeze {

		List<ACCOUNT_FREEZE> AccountFreezes { get; }
	}

	public interface INeuraliumAccountSnapshot<ACCOUNT_FEATURE, ACCOUNT_FREEZE> : IAccountSnapshot<ACCOUNT_FEATURE>, INeuraliumAccountSnapshot<ACCOUNT_FREEZE>
		where ACCOUNT_FEATURE : INeuraliumAccountFeature
		where ACCOUNT_FREEZE : INeuraliumAccountFreeze {
	}
}