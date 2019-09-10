using System;

namespace Neuralia.Blockchains.Core.Workflows.Tasks.Routing {

	public interface ITaskRouter {
		bool RouteTask(IRoutedTask task);
		bool RouteTask(IRoutedTask task, string destination);
		void ScheduleWalletproviderChildTransactionalThread(Action action);
		bool IsWalletProviderTransaction(IRoutedTask task);
	}
}