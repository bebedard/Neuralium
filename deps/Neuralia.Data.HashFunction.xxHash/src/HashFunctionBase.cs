using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using Neuralia.Data.HashFunction.xxHash.Utilities;
using Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData;
using Neuralia.Data.HashFunction.xxHash;
using Neuralia.Data.HashFunction.xxHash.Utilities;
using Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData;

namespace Neuralia.Data.HashFunction.xxHash
{
    /// <summary>
    /// Abstract implementation of an <see cref="IHashFunction"/>.
    /// Provides convenience checks and ensures a default HashSize has been set at construction.
    /// </summary>
    public abstract class HashFunctionBase 
        : IHashFunction
    {

        /// <summary>
        /// Size of produced hash, in bits.
        /// </summary>
        /// <value>
        /// The size of the hash, in bits.
        /// </value>
        public abstract int HashSizeInBits { get; }


        /// <summary>
        /// Computes hash value for given byte array.
        /// </summary>
        /// <param name="data">Array of data to hash.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        public IHashValue ComputeHash(byte[] data) => this.ComputeHash(data, CancellationToken.None);

        public IHashValue ComputeHash(byte[] data, int offet, int length) {
            return this.ComputeHash(data, offet, length, CancellationToken.None);
        }

        /// <summary>
        /// Computes hash value for given byte array.
        /// </summary>
        /// <param name="data">Array of data to hash.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="TaskCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
        public IHashValue ComputeHash(byte[] data, CancellationToken cancellationToken)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();


            return new HashValue(
                this.ComputeHashInternal(new ArrayData(data), cancellationToken),
                this.HashSizeInBits);
        }
        
        public IHashValue ComputeHash(byte[] data, int offset, int length, CancellationToken cancellationToken)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();


            return new HashValue(
                this.ComputeHashInternal(new ArrayData(data, offset, length), cancellationToken),
                this.HashSizeInBits);
        }


        /// <summary>
        /// Computes hash value for given stream.
        /// </summary>
        /// <param name="data">Stream of data to hash.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be readable.;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be seekable for this type of hash function.;<paramref name="data"/></exception>
        /// <inheritdoc />
        public IHashValue ComputeHash(RecyclableMemoryStream data) => this.ComputeHash(data, CancellationToken.None);


        /// <summary>
        /// Computes hash value for given stream.
        /// </summary>
        /// <param name="data">Stream of data to hash.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be readable.;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be seekable for this type of hash function.;<paramref name="data"/></exception>
        /// <exception cref="TaskCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
        public IHashValue ComputeHash(RecyclableMemoryStream data, CancellationToken cancellationToken)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (!data.CanRead)
                throw new ArgumentException("Stream must be readable.", nameof(data));


            cancellationToken.ThrowIfCancellationRequested();


            return new HashValue(
                this.ComputeHashInternal(
                    new StreamData(data),
                    cancellationToken),
                this.HashSizeInBits);
        }


        /// <summary>
        /// Computes hash value for given stream.
        /// </summary>
        /// <param name="data">Data to hash.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>
        /// Hash value of data.
        /// </returns>
        /// <exception cref="TaskCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
        protected abstract byte[] ComputeHashInternal(IUnifiedData data, CancellationToken cancellationToken);

    }
}
