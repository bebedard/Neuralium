using System;
using System.Threading;

namespace Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData
{
    internal sealed class ArrayData
        : UnifiedDataBase
    {
        /// <summary>
        /// Length of data provided.
        /// </summary>
        /// <remarks>
        /// Implementors are allowed throw an exception if it is not possible to resolve the length of the data.
        /// </remarks>
        public override long Length { get => this._length; }

        public override byte[] Data {
            get { return this._data; }
        }

        private readonly byte[] _data;
        private readonly int _length;
        private readonly int _startOffset;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayData"/> class.
        /// </summary>
        /// <param name="data">The data to represent.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/></exception>
        public ArrayData(byte[] data)
        {
            this._data = data ?? throw new ArgumentNullException(nameof(data));
            this._length = this.Data.Length;
            this._startOffset = 0;
        }

        public ArrayData(byte[] data, int offset, int length)
        {
            this._data = data ?? throw new ArgumentNullException(nameof(data));
            this._length = length;
            this._startOffset = offset;
        }


        /// <summary>
        /// Executes an action each time a chunk is read.
        /// </summary>
        /// <param name="action">Function to execute.</param>
        /// <param name="cancellationToken">A cancellation token to observe while reading the underlying data.</param>
        /// <exception cref="ArgumentNullException">action</exception>
        public override void ForEachRead(Action<byte[], int, int> action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            
            cancellationToken.ThrowIfCancellationRequested();


            action(this.Data, 0, this._length);
        }


        /// <summary>
        /// Executes an action one or more times, providing the data read as an array whose length is a multiple of groupSize.  
        /// Optionally runs an action on the final remainder group.
        /// </summary>
        /// <param name="groupSize">Length of the groups passed to the action.</param>
        /// <param name="action">Action to execute for each full group read.</param>
        /// <param name="remainderAction">Action to execute if the final group is less than groupSize.  Null values are allowed.</param>
        /// <param name="cancellationToken">A cancellation token to observe while reading the underlying data.</param>
        /// <remarks>remainderAction will not be run if the length of the data is a multiple of groupSize.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">groupSize;groupSize must be greater than 0.</exception>
        /// <exception cref="ArgumentNullException">action</exception>
        public override void ForEachGroup(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction, CancellationToken cancellationToken)
        {
            if (groupSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(groupSize), $"{nameof(groupSize)} must be greater than 0.");

            if (action == null)
                throw new ArgumentNullException(nameof(action));


            cancellationToken.ThrowIfCancellationRequested();


            var remainderLength = this._length % groupSize;

            if (this._length - remainderLength > 0)
                action(this.Data, this._startOffset, this._length - remainderLength);

            
            if (remainderAction != null && remainderLength > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                remainderAction(this.Data, this._startOffset+ this._length - remainderLength, remainderLength);
            }
        }

        /// <summary>
        /// Reads all data and converts it to an in-memory array.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while reading the underlying data.</param>
        /// <returns>Array of bytes read from the data provider.</returns>
        public override byte[] ToArray(CancellationToken cancellationToken) =>  this.Data;
    }
}
