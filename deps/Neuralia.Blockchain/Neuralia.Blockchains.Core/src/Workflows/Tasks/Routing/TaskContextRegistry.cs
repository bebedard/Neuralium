using System.Collections.Generic;
using System.Threading;

namespace Neuralia.Blockchains.Core.Workflows.Tasks.Routing {
	/// <summary>
	///     a class store contexts for the active thread
	/// </summary>
	public sealed class TaskContextRegistry {
		private readonly object locker = new object();

		private readonly Dictionary<int, TaskContext> registry = new Dictionary<int, TaskContext>();

		static TaskContextRegistry() {
		}

		private TaskContextRegistry() {
		}

		public static TaskContextRegistry Instance { get; } = new TaskContextRegistry();

		public void RegisterActiveTaskContext(TaskRoutingContext taskRoutingContext) {
			this.RegisterActiveTaskContext(taskRoutingContext, new CorrelationContext());
		}

		public void ClearActiveTaskRoutingContext() {
			lock(this.locker) {
				int threadId = Thread.CurrentThread.ManagedThreadId;

				if(this.registry.ContainsKey(threadId)) {
					this.registry.Remove(threadId);
				}
			}
		}

		public void RegisterActiveTaskContext(TaskRoutingContext taskRoutingContext, CorrelationContext correlationContext) {
			lock(this.locker) {
				int threadId = Thread.CurrentThread.ManagedThreadId;

				if(!this.registry.ContainsKey(threadId)) {
					this.registry.Add(threadId, null);
				}

				this.registry[threadId] = new TaskContext {TaskRoutingContext = taskRoutingContext, CorrelationContext = correlationContext};
			}
		}

		public TaskContext GetTaskRoutingContext() {
			lock(this.locker) {
				int threadId = Thread.CurrentThread.ManagedThreadId;

				if(!this.registry.ContainsKey(threadId)) {
					return null;
				}

				return this.registry[threadId];
			}
		}

		public TaskRoutingContext GetTaskRoutingTaskRoutingContext() {
			TaskContext context = this.GetTaskRoutingContext();

			return context?.TaskRoutingContext;
		}

		public CorrelationContext GetTaskRoutingCorrelationContext() {
			TaskContext context = this.GetTaskRoutingContext();

			return context?.CorrelationContext ?? new CorrelationContext();
		}
	}

	public class TaskContext {
		public CorrelationContext CorrelationContext;
		public TaskRoutingContext TaskRoutingContext;
	}
}