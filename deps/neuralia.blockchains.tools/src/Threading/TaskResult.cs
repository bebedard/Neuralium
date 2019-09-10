﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Blockchains.Tools.Threading {
	public class TaskResult<T> {
		public Task<T> awaitableTask;
		public CancellationTokenSource ctSource;
		public T result;
		public Task task;
		public event DelegatesBase.SimpleTypedDelegate<T> Completed;
		public event DelegatesBase.SimpleExceptionDelegate Error;

		public void TriggerCompleted(T result) {
			this.result = result;
			this.Completed?.Invoke(result);
		}

		public void TriggerError(Exception ex) {
			this.Error?.Invoke(ex);
		}

		/// <summary>
		///     useful if we need to await
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		public static explicit operator Task(TaskResult<T> entry) {
			return entry.task;
		}
	}
}