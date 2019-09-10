﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using Neuralia.Data.HashFunction.xxHash;
using Neuralia.Data.HashFunction.xxHash.Utilities;
using Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData;
using Neuralia.Data.HashFunction.xxHash.Utilities;
using Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData;

namespace Neuralia.Data.HashFunction.xxHash
{
    /// <summary>
    /// Abstract implementation of an <see cref="IHashFunctionAsync"/>.
    /// Provides convenience checks and ensures a default HashSize has been set at construction.
    /// </summary>
    public abstract class HashFunctionAsyncBase 
        : HashFunctionBase, 
            IHashFunctionAsync
    {

        /// <summary>
        /// Computes hash value for given stream asynchronously.
        /// </summary>
        /// <param name="data">Stream of data to hash.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <remarks>
        /// All stream IO is done via ReadAsync.
        /// </remarks>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be readable.;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be seekable for this type of hash function.;<paramref name="data"/></exception>
        public Task<IHashValue> ComputeHashAsync(RecyclableMemoryStream data) => this.ComputeHashAsync(data, CancellationToken.None);

        /// <summary>
        /// Computes hash value for given stream asynchronously.
        /// </summary>
        /// <param name="data">Stream of data to hash.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <remarks>
        /// All stream IO is done via ReadAsync.
        /// </remarks>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be readable.;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be seekable for this type of hash function.;<paramref name="data"/></exception>
        /// <exception cref="TaskCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
        public async Task<IHashValue> ComputeHashAsync(RecyclableMemoryStream data, CancellationToken cancellationToken)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (!data.CanRead)
                throw new ArgumentException("Stream \"data\" must be readable.", "data");

            cancellationToken.ThrowIfCancellationRequested();


            return new HashValue(
                await this.ComputeHashAsyncInternal(new StreamData(data), cancellationToken)
                    .ConfigureAwait(false),
                this.HashSizeInBits);
        }

        /// <summary>
        /// Computes hash value for given stream asynchronously.
        /// </summary>
        /// <param name="data">Data to hash.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>
        /// Hash value of data as byte array.
        /// </returns>
        /// <exception cref="TaskCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
        protected abstract Task<byte[]> ComputeHashAsyncInternal(IUnifiedDataAsync data, CancellationToken cancellationToken);

    }
}
