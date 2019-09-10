﻿using System;
using System.Threading;

namespace Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData
{
    /// <summary>
    /// Centralized methodology for accessing data used by Data.HashFunction.
    /// </summary>
    internal abstract class UnifiedDataBase
        : IUnifiedData
    {

        /// <summary>
        /// Length of data provided.
        /// </summary>
        /// <remarks>
        /// Implementors are allowed throw an exception if it is not possible to resolve the length of the data.
        /// </remarks>
        /// <exception cref="NotSupportedException" />
        public abstract long Length { get; }


        /// <summary>
        /// Length of temporary buffers used, if they are needed.
        /// </summary>
        /// <remarks>
        /// Implementors are not required to use this value.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">value; value must be greater than 0</exception>
        public virtual int BufferSize 
        {
            get { return this._BufferSize; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} must be greater than 0");

                this._BufferSize = value;
            }
        }


        private int _BufferSize = 4096;



        /// <summary>
        /// Executes an action each time a chunk is read.
        /// </summary>
        /// <param name="action">Function to execute.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <exception cref="ArgumentNullException">action</exception>
        public abstract void ForEachRead(Action<byte[], int, int> action, CancellationToken cancellationToken);

        /// <summary>
        /// Executes an action one or more times, providing the data read as an array whose length is a multiple of groupSize.  
        /// Optionally runs an action on the final remainder group.
        /// </summary>
        /// <param name="groupSize">Length of the groups passed to the action.</param>
        /// <param name="action">Action to execute for each full group read.</param>
        /// <param name="remainderAction">Action to execute if the final group is less than groupSize.  Null values are allowed.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <remarks>remainderAction will not be run if the length of the data is a multiple of groupSize.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">groupSize;groupSize must be greater than 0.</exception>
        /// <exception cref="ArgumentNullException">action</exception>
        public abstract void ForEachGroup(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction, CancellationToken cancellationToken);


        /// <summary>
        /// Reads all data and converts it to an in-memory array.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>Array of bytes read from the data provider.</returns>
        public abstract byte[] ToArray(CancellationToken cancellationToken);

        public abstract byte[] Data { get; }
    }
}
