using System;
using System.Security.Cryptography;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Tools.Cryptography {
	public class GlobalRandom : Random {
		private const int DATA_POOL_SIZE = 1024 << 1;

		private static readonly Lazy<RandomNumberGenerator> Randomizer = new Lazy<RandomNumberGenerator>(RandomNumberGenerator.Create);

		private static readonly object locker = new object();

		private static readonly Lazy<byte[]> Pool = new Lazy<byte[]>(() => GenerateNewPool(new byte[DATA_POOL_SIZE]));

		private static int position;

		public static int GetNext() {
			return GetRandomInt32();
		}

		public static int GetNext(int maxValue) {
			if(maxValue < 1) {
				throw new ArgumentException("Must be greater than zero.", nameof(maxValue));
			}

			return GetNext(0, maxValue);
		}

		public static int GetNext(int minValue, int maxValue) {
			const long max = 1 + (long) uint.MaxValue;

			if(minValue >= maxValue) {
				throw new ArgumentException($"{nameof(minValue)} is greater than or equal to {nameof(maxValue)}");
			}

			long diff = maxValue - minValue;
			long limit = max - (max % diff);

			while(true) {
				uint rand = GetRandomUInt32();

				if(rand < limit) {
					return (int) (minValue + (rand % diff));
				}
			}
		}

		public static void GetNextBytes(byte[] buffer) {
			GetNextBytes(buffer, buffer.Length);
		}

		public static void GetNextBytes(IByteArray buffer) {
			GetNextBytes(buffer.Bytes, buffer.Offset, buffer.Length);
		}

		public static void GetNextBytes(byte[] buffer, int length) {
			GetNextBytes(buffer, 0, length);
		}

		public static void GetNextBytes(byte[] buffer, int offset, int length) {
			if(buffer == null) {
				throw new ArgumentNullException(nameof(buffer));
			}

			if(length < DATA_POOL_SIZE) {
				lock(locker) {
					if((DATA_POOL_SIZE - position) < length) {
						GenerateNewPool(Pool.Value);
					}

					Buffer.BlockCopy(Pool.Value, position, buffer, offset, length);
					position += buffer.Length;
				}
			} else {
				Randomizer.Value.GetBytes(buffer, offset, length);
			}
		}

		public static short GetNextShort() {
			return GetRandomInt16();
		}

		public static ushort GetNextUShort() {
			return GetRandomUInt16();
		}

		public static uint GetNextUInt() {
			return GetRandomUInt32();
		}

		public static long GetNextLong() {
			return GetRandomInt64();
		}

		public static ulong GetNextULong() {
			return GetRandomUInt64();
		}

		public static double GetNextDouble() {
			return GetRandomUInt32() / (1.0 + uint.MaxValue);
		}

		public override int Next() {
			return GetNext();
		}

		public override int Next(int maxValue) {
			return GetNext(0, maxValue);
		}

		public override int Next(int minValue, int maxValue) {
			return GetNext(minValue, maxValue);
		}

		public override void NextBytes(byte[] buffer) {
			GetNextBytes(buffer);
		}

		public override double NextDouble() {
			return GetNextDouble();
		}

		private static byte[] GenerateNewPool(byte[] buffer) {
			position = 0;
			Randomizer.Value.GetBytes(buffer);

			return buffer;
		}

		private static short GetRandomInt16() {
			short result;

			lock(locker) {
				if((DATA_POOL_SIZE - position) < sizeof(short)) {
					GenerateNewPool(Pool.Value);
				}

				result = BitConverter.ToInt16(Pool.Value, position);
				position += sizeof(short);
			}

			return result;
		}

		private static ushort GetRandomUInt16() {
			ushort result;

			lock(locker) {
				if((DATA_POOL_SIZE - position) < sizeof(ushort)) {
					GenerateNewPool(Pool.Value);
				}

				result = BitConverter.ToUInt16(Pool.Value, position);
				position += sizeof(ushort);
			}

			return result;
		}

		private static int GetRandomInt32() {
			int result;

			lock(locker) {
				if((DATA_POOL_SIZE - position) < sizeof(int)) {
					GenerateNewPool(Pool.Value);
				}

				result = BitConverter.ToInt32(Pool.Value, position);
				position += sizeof(int);
			}

			return result;
		}

		private static uint GetRandomUInt32() {
			uint result;

			lock(locker) {
				if((DATA_POOL_SIZE - position) < sizeof(uint)) {
					GenerateNewPool(Pool.Value);
				}

				result = BitConverter.ToUInt32(Pool.Value, position);
				position += sizeof(uint);
			}

			return result;
		}

		private static long GetRandomInt64() {
			long result;

			lock(locker) {
				if((DATA_POOL_SIZE - position) < sizeof(long)) {
					GenerateNewPool(Pool.Value);
				}

				result = BitConverter.ToInt64(Pool.Value, position);
				position += sizeof(long);
			}

			return result;
		}

		private static ulong GetRandomUInt64() {
			ulong result;

			lock(locker) {
				if((DATA_POOL_SIZE - position) < sizeof(ulong)) {
					GenerateNewPool(Pool.Value);
				}

				result = BitConverter.ToUInt64(Pool.Value, position);
				position += sizeof(ulong);
			}

			return result;
		}
	}
}