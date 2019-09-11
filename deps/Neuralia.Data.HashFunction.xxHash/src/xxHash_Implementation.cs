﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Neuralia.Data.HashFunction.xxHash;
using Neuralia.Data.HashFunction.xxHash.Utilities.UnifiedData;
using Neuralia.Data.HashFunction.xxHash;

namespace Neuralia.Data.HashFunction.xxHash
{
    /// <summary>
    /// Implements xxHash as specified at https://github.com/Cyan4973/xxHash/blob/dev/xxhash.c and 
    ///   https://github.com/Cyan4973/xxHash.
    /// </summary>
    internal class xxHash_Implementation
        : HashFunctionAsyncBase,
            IxxHash
    {

        /// <summary>
        /// Configuration used when creating this instance.
        /// </summary>
        /// <value>
        /// A clone of configuration that was used when creating this instance.
        /// </value>
        public IxxHashConfig Config => this._config.Clone();

        public override int HashSizeInBits => this._config.HashSizeInBits;
        


        private readonly IxxHashConfig _config;


        private static readonly IReadOnlyList<UInt32> _primes32 = 
            new[] {
                2654435761U,
                2246822519U,
                3266489917U,
                 668265263U,
                 374761393U
            };

        private static readonly IReadOnlyList<UInt64> _primes64 = 
            new[] {
                11400714785074694791UL,
                14029467366897019727UL,
                 1609587929392839161UL,
                 9650029242287828579UL,
                 2870177450012600261UL
            };

        private static readonly IEnumerable<int> _validHashSizes = new HashSet<int>() { 32, 64 };


        /// <summary>
        /// Initializes a new instance of the <see cref="xxHash_Implementation" /> class.
        /// </summary>
        /// <param name="config">The configuration to use for this instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="config"/>.<see cref="IxxHashConfig.HashSizeInBits"/>;<paramref name="config"/>.<see cref="IxxHashConfig.HashSizeInBits"/> must be contained within xxHash.ValidHashSizes</exception>
        public xxHash_Implementation(IxxHashConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            this._config = config.Clone();


            if (!_validHashSizes.Contains(this._config.HashSizeInBits))
                throw new ArgumentOutOfRangeException($"{nameof(config)}.{nameof(config.HashSizeInBits)}", this._config.HashSizeInBits, $"{nameof(config)}.{nameof(config.HashSizeInBits)} must be contained within xxHash.ValidHashSizes");
        }



        /// <exception cref="InvalidOperationException">HashSize set to an invalid value.</exception>
        /// <inheritdoc />
        protected override byte[] ComputeHashInternal(IUnifiedData data, CancellationToken cancellationToken)
        {
            byte[] hash;
            Memory<byte> temp = data.Data;
            
            switch (this._config.HashSizeInBits)
            {
                case 32:
                {
                    var h = ((UInt32) this._config.Seed) + _primes32[4];

                    ulong dataCount = 0;
                    byte[] remainder = null;


                    var initValues = new[] {
                        ((UInt32) this._config.Seed) + _primes32[0] + _primes32[1],
                        ((UInt32) this._config.Seed) + _primes32[1],
                        ((UInt32) this._config.Seed),
                        ((UInt32) this._config.Seed) - _primes32[0]
                    };

                    data.ForEachGroup(16, 
                        (dataGroup, position, length) => {
                            for (int x = position; x < position + length; x += 16)
                            {
                                for (var y = 0; y < 4; ++y)
                                {
                                    initValues[y] += this.GetSpanUint(temp.Span, x + (y * 4)) * _primes32[1];
                                    initValues[y] = RotateLeft(initValues[y], 13);
                                    initValues[y] *= _primes32[0];
                                }
                            }

                            dataCount += (ulong)length;
                        },
                        (remainderData, position, length) => {
                            remainder = new byte[length];
                            Buffer.BlockCopy(remainderData, position, remainder, 0, length);

                            dataCount += (ulong)length;
                        },
                        cancellationToken);

                    this.PostProcess(ref h, initValues, dataCount, remainder);

                    hash = BitConverter.GetBytes(h);
                    break;
                }

                case 64:
                {
                     var h = this._config.Seed + _primes64[4];

                    ulong dataCount = 0;
                    byte[] remainder = null;

                    var initValues = new[] {
                        this._config.Seed + _primes64[0] + _primes64[1], this._config.Seed + _primes64[1], this._config.Seed, this._config.Seed - _primes64[0]
                    };

                    
                    data.ForEachGroup(32, 
                        (dataGroup, position, length) => {

                            for (var x = position; x < position + length; x += 32)
                            {
                                for (var y = 0; y < 4; ++y) {


                                    initValues[y] += this.GetSpanUlong(temp.Span, x + (y * 8)) * _primes64[1];
                                    initValues[y] = RotateLeft(initValues[y], 31);
                                    initValues[y] *= _primes64[0];
                                }
                            }

                            dataCount += (ulong) length;
                        },
                        (remainderData, position, length) => {
                            remainder = new byte[length];
                            Buffer.BlockCopy(remainderData, position, remainder, 0, length);

                            dataCount += (ulong) length;
                        },
                        cancellationToken);

                    this.PostProcess(ref h, initValues, dataCount, remainder);

                    hash = BitConverter.GetBytes(h);
                    break;
                }

                default:
                    throw new NotImplementedException();
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong GetSpanUlong(in Span<byte> data, int offset) {
            return MemoryMarshal.Read<ulong>(data.Slice(offset, 8));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetSpanUint(in Span<byte> data, int offset) {
            return MemoryMarshal.Read<uint>(data.Slice(offset, 4));
        }

        /// <exception cref="InvalidOperationException">HashSize set to an invalid value.</exception>
        /// <inheritdoc />
        protected override async Task<byte[]> ComputeHashAsyncInternal(IUnifiedDataAsync data, CancellationToken cancellationToken)
        {
            byte[] hash;
            Memory<byte> temp = data.Data;
            
            switch (this._config.HashSizeInBits)
            {
                case 32:
                {
                    var h = ((UInt32) this._config.Seed) + _primes32[4];

                    ulong dataCount = 0;
                    byte[] remainder = null;


                    var initValues = new[] {
                        ((UInt32) this._config.Seed) + _primes32[0] + _primes32[1],
                        ((UInt32) this._config.Seed) + _primes32[1],
                        ((UInt32) this._config.Seed),
                        ((UInt32) this._config.Seed) - _primes32[0]
                    };

                    await data.ForEachGroupAsync(16, 
                            (dataGroup, position, length) => {
                                for (var x = position; x < position + length; x += 16)
                                {
                                    for (var y = 0; y < 4; ++y)
                                    {
                                        initValues[y] += this.GetSpanUint(temp.Span, x + (y * 4)) * _primes32[1];
                                        initValues[y] = RotateLeft(initValues[y], 13);
                                        initValues[y] *= _primes32[0];
                                    }
                                }

                                dataCount += (ulong) length;
                            },
                            (remainderData, position, length) => {
                                remainder = new byte[length];
                                Buffer.BlockCopy(remainderData, position, remainder, 0, length);

                                dataCount += (ulong) length;
                            },
                            cancellationToken)
                        .ConfigureAwait(false);

                    this.PostProcess(ref h, initValues, dataCount, remainder);

                    hash = BitConverter.GetBytes(h);
                    break;
                }

                case 64:
                {
                     var h = this._config.Seed + _primes64[4];

                    ulong dataCount = 0;
                    byte[] remainder = null;

                    var initValues = new[] {
                        this._config.Seed + _primes64[0] + _primes64[1], this._config.Seed + _primes64[1], this._config.Seed, this._config.Seed - _primes64[0]
                    };


                    await data.ForEachGroupAsync(32, 
                            (dataGroup, position, length) => {
                                for (var x = position; x < position + length; x += 32)
                                {
                                    for (var y = 0; y < 4; ++y)
                                    {
                                        initValues[y] += this.GetSpanUlong(temp.Span, x + (y * 8)) * _primes64[1];
                                        initValues[y] = RotateLeft(initValues[y], 31);
                                        initValues[y] *= _primes64[0];
                                    }
                                }

                                dataCount += (ulong) length;
                            },
                            (remainderData, position, length) => {
                                remainder = new byte[length];
                                Buffer.BlockCopy(remainderData, position, remainder, 0, length);

                                dataCount += (ulong) remainder.Length;
                            },
                            cancellationToken)
                        .ConfigureAwait(false);


                    this.PostProcess(ref h, initValues, dataCount, remainder);

                    hash = BitConverter.GetBytes(h);
                    break;
                }

                default:
                    throw new NotImplementedException();
            }

            return hash;
        }


        private void PostProcess(ref UInt32 h, UInt32[] initValues, ulong dataCount, byte[] remainder) {
            Span<byte> remainderSpan = remainder;
            if (dataCount >= 16)
            {
                h = RotateLeft(initValues[0], 1) + 
                    RotateLeft(initValues[1], 7) + 
                    RotateLeft(initValues[2], 12) + 
                    RotateLeft(initValues[3], 18);
            }


            h += (UInt32) dataCount;

            if (remainder != null)
            {
                // In 4-byte chunks, process all process all full chunks
                for (int x = 0; x < remainder.Length / 4; ++x)
                {
                    
                    h += this.GetSpanUint(remainderSpan, x * 4) * _primes32[2];
                    h  = RotateLeft(h, 17) * _primes32[3];
                }


                // Process last 4 bytes in 1-byte chunks (only runs if data.Length % 4 != 0)
                for (int x = remainder.Length - (remainder.Length % 4); x < remainder.Length; ++x)
                {
                    h += (UInt32) remainder[x] * _primes32[4];
                    h  = RotateLeft(h, 11) * _primes32[0];
                }
            }

            h ^= h >> 15;
            h *= _primes32[1];
            h ^= h >> 13;
            h *= _primes32[2];
            h ^= h >> 16;
        }

        private void PostProcess(ref UInt64 h, UInt64[] initValues, ulong dataCount, byte[] remainder) {
            Span<byte> remainderSpan = remainder;
            
            if (dataCount >= 32)
            {
                h = RotateLeft(initValues[0], 1) +
                    RotateLeft(initValues[1], 7) +
                    RotateLeft(initValues[2], 12) +
                    RotateLeft(initValues[3], 18);


                for (var x = 0; x < initValues.Length; ++x)
                {
                    initValues[x] *= _primes64[1];
                    initValues[x] = RotateLeft(initValues[x], 31);
                    initValues[x] *= _primes64[0];

                    h ^= initValues[x];
                    h = (h * _primes64[0]) + _primes64[3];
                }
            }

            h += (UInt64) dataCount;

            if (remainder != null)
            { 
                // In 8-byte chunks, process all full chunks
                for (int x = 0; x < remainder.Length / 8; ++x)
                {
                    
                    h ^= RotateLeft(this.GetSpanUlong(remainderSpan, x * 8) * _primes64[1], 31) * _primes64[0];
                    h  = (RotateLeft(h, 27) * _primes64[0]) + _primes64[3];
                }


                // Process a 4-byte chunk if it exists
                if ((remainder.Length % 8) >= 4)
                {

                    h ^= ((UInt64) this.GetSpanUint(remainderSpan, remainder.Length - (remainder.Length % 8))) * _primes64[0];
                    h  = (RotateLeft(h, 23) * _primes64[1]) + _primes64[2];
                }

                // Process last 4 bytes in 1-byte chunks (only runs if data.Length % 4 != 0)
                for (int x = remainder.Length - (remainder.Length % 4); x < remainder.Length; ++x)
                {
                    h ^= (UInt64) remainder[x] * _primes64[4];
                    h  = RotateLeft(h, 11) * _primes64[0];
                }
            }


            h ^= h >> 33;
            h *= _primes64[1];
            h ^= h >> 29;
            h *= _primes64[2];
            h ^= h >> 32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt32 RotateLeft(UInt32 operand, int shiftCount)
        {
            shiftCount &= 0x1f;

            return
                (operand << shiftCount) |
                (operand >> (32 - shiftCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 RotateLeft(UInt64 operand, int shiftCount)
        {
            shiftCount &= 0x3f;

            return
                (operand << shiftCount) |
                (operand >> (64 - shiftCount));
        }
    }
}
