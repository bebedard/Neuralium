﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData
{
    /// <summary>
    /// Centralized methodology for accessing data used by Data.HashFunction.
    /// </summary>
    internal abstract class UnifiedDataAsyncBase
        : UnifiedDataBase,
            IUnifiedDataAsync
    {
        /// <summary>
        /// Executes an action each time a chunk is read.
        /// </summary>
        /// <param name="action">Function to execute.</param>
        /// <param name="cancellationToken">A cancellation token to observe while reading the underlying data.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">action</exception>
        public abstract Task ForEachReadAsync(Action<byte[], int, int> action, CancellationToken cancellationToken);


        /// <summary>
        /// Executes an action one or more times, providing the data read as an array whose length is a multiple of groupSize.  
        /// Optionally runs an action on the final remainder group.
        /// </summary>
        /// <param name="groupSize">Length of the groups passed to the action.</param>
        /// <param name="action">Action to execute for each full group read.</param>
        /// <param name="remainderAction">Action to execute if the final group is less than groupSize.  Null values are allowed.</param>
        /// <param name="cancellationToken">A cancellation token to observe while reading the underlying data.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        /// <remarks>remainderAction will not be run if the length of the data is a multiple of groupSize.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">groupSize;groupSize must be greater than 0.</exception>
        /// <exception cref="ArgumentNullException">action</exception>        
        public abstract Task ForEachGroupAsync(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction, CancellationToken cancellationToken);

        /// <summary>
        /// Reads all data and converts it to an in-memory array.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while reading the underlying data.</param>
        /// <returns>Array of bytes read from the data provider.</returns>
        public abstract Task<byte[]> ToArrayAsync(CancellationToken cancellationToken);
    }
}
