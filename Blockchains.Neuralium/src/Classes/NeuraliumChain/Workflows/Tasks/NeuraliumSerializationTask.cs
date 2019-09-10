using Blockchains.Neuralium.Classes.NeuraliumChain.Managers;
using Blockchains.Neuralium.Classes.NeuraliumChain.Providers;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Workflows.Tasks.Base;
using Neuralia.Blockchains.Core.Workflows.Tasks.Routing;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Workflows.Tasks {
	public interface INeuraliumSerializationTask : ISerializationTask {
	}

	public interface INeuraliumSerializationTask<K> : INeuraliumSerializationTask, ISerializationTask<K> {
	}

	public interface INeuraliumSerializationTask<T, K> : INeuraliumSerializationTask<K>, ISerializationTask<T, K>
		where T : IRoutedTaskRoutingHandler {
	}

	public class NeuraliumSerializationTask<K> : NeuraliumSerializationTaskGenerix<INeuraliumSerializationManager, K> {
	}

	public class NeuraliumSerializationTaskGenerix<T, K> : SerializationTask<T, K, INeuraliumCentralCoordinator, INeuraliumChainComponentProvider>, INeuraliumSerializationTask<T, K>
		where T : IRoutedTaskRoutingHandler {
	}
}