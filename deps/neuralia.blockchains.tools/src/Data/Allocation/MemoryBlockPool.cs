using System;
using System.Collections.Generic;
using System.Linq;

namespace Neuralia.Blockchains.Tools.Data.Allocation {

	/// <summary>
	///     A fairly simple object pool for items that will be created a lot.
	/// </summary>
	/// <typeparam name="T">The type that is pooled.</typeparam>
	public class MemoryBlockPool<T> : IDisposable2
		where T : class {
		private readonly int expandCount;

		private readonly object locker = new object();

		/// <summary>
		///     The generator for creating new objects.
		/// </summary>
		/// <returns></returns>
		private readonly Func<T> objectFactory;

		/// <summary>
		///     Our pool of objects
		/// </summary>
		private readonly Stack<T> pool = new Stack<T>();

		public MemoryBlockPool(Func<T> objectFactory, int initialCount = 0, int expandCount = 10) {
			this.expandCount = expandCount;
			this.objectFactory = objectFactory;

			this.CreateMore(initialCount);
		}

		public int TotalCreated { get; private set; }

		public void CreateMore(int amount) {
			lock(this.locker) {
				for(int i = amount; i != 0; i--) {
					this.TotalCreated++;
					T newEntry = this.objectFactory.Invoke();
					this.PutObject(newEntry);
				}
			}
		}

		public bool Contains(T entry) {
			lock(this.locker) {

				return false;
			}
		}

		/// <summary>
		///     Returns a pooled object of type T, if none are available another is created.
		/// </summary>
		/// <returns>An instance of T.</returns>
		public virtual T GetObject() {
			lock(this.locker) {
				if(this.pool.Count != 0) {
					T item = this.pool.Pop();

					return item;
				}

				this.CreateMore(this.expandCount);

				return this.pool.Pop();
			}
		}

		public List<TResult> RunAll<TResult>(Func<T, TResult> select) {
			lock(this.locker) {
				return this.pool.Select(select).ToList();
			}
		}

		/// <summary>
		///     Returns an object to the pool.
		/// </summary>
		/// <param name="item">The item to return.</param>
		public virtual void PutObject(T item) {
			if(item is IDisposable2 dis && dis.IsDisposed) {
				return;
			}

			lock(this.locker) {
				this.pool.Push(item);
			}
		}

		///// <summary>
		///// a method to use in rare cases where a certain object must be removed from the pool
		///// </summary>
		///// <param name="item"></param>
		//public void RemoveObject(T item) {
		//	lock(this.locker) {
		//		var node = this.pool.Find(item);

		//		if(node != null) {
		//			this.pool.Remove(node);
		//		}
		//	}
		//}

	#region disposable

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {
				lock(this.locker) {

					// this is a fake dispose, we do a simple memory return
					var copy = this.pool.ToArray();
					this.pool.Clear();

					foreach(T obj in copy) {

						this.DisposeEntry(obj);

						if(obj is IDisposable dispo) {
							dispo.Dispose();
						}
					}
				}
			}

			this.IsDisposed = true;
		}

		protected virtual void DisposeEntry(T entry) {

		}

		~MemoryBlockPool() {
			this.Dispose(false);
		}

	#endregion

	}
}