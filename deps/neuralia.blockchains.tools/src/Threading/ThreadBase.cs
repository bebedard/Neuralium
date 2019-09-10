using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Threading {
	public interface IThreadBase : IDisposable2 {
		CancellationTokenSource CancelTokenSource { get; }
		CancellationToken CancelNeuralium { get; }

		Task Task { get; }

		bool IsCompleted { get; }
		bool IsStarted { get; }

		Task<bool> CompletionTask { get; }

		void WaitStop(TimeSpan timeout);

		void RenewCancelNeuralium();

		void Stop();

		void Start();

		void StartSync();
		void StopSync();

		void Awaken();

		event Action<bool, object> Completed;

		/// <summary>
		///     trigger when workflow ended in success
		/// </summary>
		event Action<object> Success;

		/// <summary>
		///     trigger when workflow ended in error
		/// </summary>
		event Action<object, Exception> Error;
	}

	public interface IThreadBase<out T> : IThreadBase
		where T : IThreadBase<T> {

		event Action<bool, T> Completed2;

		/// <summary>
		///     trigger when workflow ended in success
		/// </summary>
		event Action<T> Success2;

		/// <summary>
		///     trigger when workflow ended in error
		/// </summary>
		event Action<T, Exception> Error2;
	}

	public abstract class ThreadBase<T> : IThreadBase<T>
		where T : class, IThreadBase<T> {
		/// <summary>
		///     how long the workflow will wait for something before timing out and giving up
		/// </summary>
		protected TimeSpan hibernateTimeoutSpan;

		protected object locker = new object();

		/// <summary>
		///     We hold our own task
		/// </summary>
		private Task task;

		public ThreadBase() {
			// how long do we wait while hibernating until we declare this thread as dead?
			// by default we wait forever, children must override this and set their own value
			this.hibernateTimeoutSpan = TimeSpan.MaxValue;

			// wire up the events to make sure one calls the other
			this.Completed2 += (a, b) => this.Completed?.Invoke(a, b);
			this.Success2 += a => this.Success?.Invoke(a);
			this.Error2 += (a, b) => this.Error?.Invoke(a, b);
		}

		protected List<AutoResetEvent> AutoEvents { get; } = new List<AutoResetEvent>();
		protected AutoResetEvent AutoEvent { get; private set; }

		protected TaskCompletionSource<bool> TaskCompletionSource { get; private set; }

		protected bool Stopping { get; private set; }

		protected bool Initialized { get; private set; }

		public bool IsDisposed { get; private set; }

		/// <summary>
		///     called every time when completed, whether an error happened or not
		/// </summary>
		public event Action<bool, object> Completed;

		public event Action<bool, T> Completed2;

		/// <summary>
		///     trigger when workflow ended in success
		/// </summary>
		public event Action<object> Success;

		public event Action<T> Success2;

		/// <summary>
		///     trigger when workflow ended in error
		/// </summary>
		public event Action<object, Exception> Error;

		public event Action<T, Exception> Error2;

		/// <summary>
		///     The actual task. do
		/// </summary>
		public Task Task { get; private set; }

		/// <summary>
		///     the task to know the completion state of the task
		/// </summary>
		public Task<bool> CompletionTask => this.TaskCompletionSource.Task;

		public bool IsCompleted => this.Task?.IsCompleted ?? false;
		public bool IsStarted { get; private set; }

		public CancellationTokenSource CancelTokenSource { get; private set; }
		public CancellationToken CancelNeuralium { get; private set; }

		public virtual void Stop() {
			this.Stopping = true;

			try {
				lock(this.locker) {
					if(!this.CancelTokenSource?.IsCancellationRequested ?? false) {
						this.CancelTokenSource?.Cancel();
					}
				}
			} catch {
				// we can let this die
			}

			if((this.task != null) && !this.task.IsCompleted) {
				this.Task.Wait(TimeSpan.FromMilliseconds(6000));
			}

			this.Stopping = false;
		}

		public virtual void StopSync() {
			this.Terminate(true);
		}

		/// <summary>
		///     wait for the thread to stop
		/// </summary>
		public void WaitStop(TimeSpan timeout) {
			if(this.Stopping && !this.IsCompleted) {
				this.Task?.Wait(timeout);
			}
		}

		public void RenewCancelNeuralium() {
			this.CancelTokenSource?.Dispose();

			this.CancelTokenSource = new CancellationTokenSource();
			this.CancelNeuralium = this.CancelTokenSource.Token;
		}

		/// <summary>
		///     Main method to start the workflow thread
		/// </summary>
		public virtual void Start() {
			this.IsStarted = true;
			this.InitializeTask();

			// we start the thread of the task in this case
			this.task.Start();
		}

		/// <summary>
		///     Prepare to run asynchronously
		/// </summary>
		public virtual void StartSync() {
			this.Initialize();
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Awaken() {

			AutoResetEvent[] resetEvents = null;

			lock(this.locker) {
				resetEvents = this.AutoEvents.ToArray();
			}

			foreach(AutoResetEvent autoEvent in resetEvents) {
				autoEvent.Set();
			}
		}

		private void InitializeTask() {
			this.RenewCancelNeuralium();

			this.TaskCompletionSource = new TaskCompletionSource<bool>();
			this.task = new Task(this.ThreadRun);

			this.Task = this.task.ContinueWith(previousTask => {
				// workflow is done, lets trigger a removal
				this.TriggerCompleted(!this.TaskCompletionSource.Task.IsFaulted);

				if(!this.TaskCompletionSource.Task.IsFaulted) {
					this.TriggerSuccess();
				} else {
					if(this.TaskCompletionSource.Task.IsFaulted) {
						this.TriggerError(this.TaskCompletionSource.Task.Exception?.Flatten());
					}
				}
			}, TaskContinuationOptions.LongRunning);
		}

		protected AutoResetEvent RegisterNewAutoEvent() {
			lock(this.locker) {
				AutoResetEvent autoEvent = new AutoResetEvent(false);

				this.AutoEvents.Add(autoEvent);

				return autoEvent;
			}
		}

		protected void ClearAutoEvent(AutoResetEvent autoEvent) {
			lock(this.locker) {
				autoEvent.Set();

				this.AutoEvents.Remove(autoEvent);
			}
		}

		protected bool Hibernate() {

			return this.Hibernate(this.hibernateTimeoutSpan);
		}

		protected bool Hibernate(TimeSpan? timeout) {
			return this.Hibernate(timeout, this.AutoEvent);
		}

		/// <summary>
		///     calling this method we go to sleep until we are awoken explicitely
		/// </summary>
		protected void Hibernate(AutoResetEvent autoEvent) {

			this.Hibernate(this.hibernateTimeoutSpan, this.AutoEvent);
		}

		protected bool Hibernate(TimeSpan? timeout, AutoResetEvent autoEvent) {

			if(!timeout.HasValue) {
				timeout = this.hibernateTimeoutSpan;
			}

			if(autoEvent == null) {
				throw new ApplicationException("Network AutoEvent awaiter can not be null");
			}

			DateTime timeoutLimit = DateTime.Now + timeout.Value;

			autoEvent.WaitOne(timeout.Value);

			//TODO: is the precision of datetime high enough here?
			if(DateTime.Now > timeoutLimit) {
				// we timed out, event was not set
				return false;
			}

			this.CheckShouldCancel();

			// event was set
			return true;
		}

		private void ThreadRun() {
			Thread.CurrentThread.Name = this.GetType().Name;

			this.AutoEvent = this.RegisterNewAutoEvent();

			try {
				this.Initialize();

				this.PerformWork();

				this.TaskCompletionSource.SetResult(true);
			} catch(TaskCanceledException) {
				this.TaskCompletionSource.SetCanceled();
			} catch(OperationCanceledException) {
				this.TaskCompletionSource.SetCanceled();
			} catch(Exception ex) {
				this.TaskCompletionSource.SetException(ex);
			} finally {
				this.Terminate(true);
			}
		}

		/// <summary>
		///     the method to override to perform the actual thread work
		/// </summary>
		/// <returns></returns>
		protected abstract void PerformWork();

		/// <summary>
		///     Check if a cancel was requested.abstract if it did, we will stop the thread with an exception
		/// </summary>
		protected void CheckShouldCancel() {
			// Poll on this property if you have to do
			// other cleanup before throwing.
			if(this.CheckCancelRequested() || this.Stopping) {
				this.Terminate(false);

				// Clean up here, then...
				//if(throwException) {
				this.CancelNeuralium.ThrowIfCancellationRequested();
				/*} else {
					return new OperationCanceledException();
				}*/
			}
		}

		protected bool CheckCancelRequested() {
			return this.CancelNeuralium.IsCancellationRequested;
		}

		protected virtual void Initialize() {
			this.Initialized = true;
		}

		/// <summary>
		///     terminate.
		/// </summary>
		/// <param name="clean">if true, the tread ends normally. if false, then it was a hard cancel</param>
		protected void Terminate() {
			this.Terminate(true);
		}

		protected virtual void Terminate(bool clean) {
		}

		protected virtual void TriggerCompleted(bool success) {
			this.Completed2?.Invoke(success, this as T);
		}

		protected virtual void TriggerSuccess() {
			this.Success2?.Invoke(this as T);
		}

		protected virtual void TriggerError(Exception ex) {
			this.Error2?.Invoke(this as T, ex);
		}

		protected void Dispose(bool disposing) {

			if(!this.IsDisposed) {
				try {
					if(disposing) {
						this.Stop();

						this.CancelTokenSource?.Dispose();

						if((this.Task == null) || (this.IsStarted == false)) {
							// we never ran this workflow. lets at least alert that it failed
							this.TriggerCompleted(false);
						}
					}

					this.DisposeAll(disposing);
				} finally {
					this.IsDisposed = true;
				}
			}
		}

		protected virtual void DisposeAll(bool disposing) {
			if(disposing) {
			}
		}

		~ThreadBase() {
			this.Dispose(false);
		}
	}
}