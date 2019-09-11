﻿namespace Neuralia.Blockchains.Core.Compression {
	public static class Compressors {

		public static ICompression WalletCompressor => BrotliCompression.Instance;
		public static ICompression DigestCompressor => BrotliCompression.Instance;
		public static ICompression GeneralPurposeCompressor => BrotliCompression.Instance;
	}
}