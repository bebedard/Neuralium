using System;
using Neuralia.Blockchains.Core.Cryptography.PostQuantum.XMSS.Utils;
using Neuralia.Blockchains.Core.Cryptography.PostQuantum.XMSS.WOTS;
using Neuralia.Blockchains.Core.General.Types.Dynamic;
using Neuralia.Blockchains.Tools;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Data.Allocation;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Core.Cryptography.PostQuantum.XMSS.XMSS {
	public class XMSSSignature : IDisposable2 {

		// versioning information
		public readonly byte Major = 1;
		public readonly byte Minor = 0;
		public readonly byte Revision = 0;
		protected readonly XMSSExecutionContext XmssExecutionContext;

		public XMSSSignature(XMSSExecutionContext xmssExecutionContext) {
			this.XmssExecutionContext = xmssExecutionContext;
			this.XmssTreeSignature = new XMSSTreeSignature(xmssExecutionContext);
		}

		public XMSSSignature(IByteArray random, int index, XMSSTreeSignature xmssTreeSignature, XMSSExecutionContext xmssExecutionContext) : this(xmssExecutionContext) {
			this.Random = random;
			this.Index = index;
			this.XmssTreeSignature = xmssTreeSignature;
		}

		public IByteArray Random { get; private set; }
		public int Index { get; private set; }

		public XMSSTreeSignature XmssTreeSignature { get; }

		public void Load(IByteArray signature, WotsPlusEngine wotsPlusProvider, int height) {
			IDataRehydrator rehydrator = DataSerializationFactory.CreateRehydrator(signature);

			this.Rehydrate(rehydrator, wotsPlusProvider, height);

			this.XmssTreeSignature.Rehydrate(rehydrator, wotsPlusProvider, height);
		}

		protected virtual void Rehydrate(IDataRehydrator rehydrator, WotsPlusEngine wotsPlusProvider, int height) {

			int major = rehydrator.ReadByte();
			int minor = rehydrator.ReadByte();
			int increment = rehydrator.ReadByte();

			int n = this.XmssExecutionContext.DigestSize;

			AdaptiveLong1_9 adaptiveLong = new AdaptiveLong1_9();
			adaptiveLong.Rehydrate(rehydrator);
			this.Index = (int) adaptiveLong.Value;

			this.Random = rehydrator.ReadArray(n);
		}

		public IByteArray Save() {

			int n = this.XmssExecutionContext.DigestSize;

			IDataDehydrator dehydrator = DataSerializationFactory.CreateDehydrator();

			this.Dehydrate(dehydrator);

			this.XmssTreeSignature.Dehydrate(dehydrator);

			return dehydrator.ToArray();
		}

		protected virtual void Dehydrate(IDataDehydrator dehydrator) {

			dehydrator.Write(this.Major);
			dehydrator.Write(this.Minor);
			dehydrator.Write(this.Revision);

			AdaptiveLong1_9 adaptiveLong = new AdaptiveLong1_9();
			adaptiveLong.Value = this.Index;
			adaptiveLong.Dehydrate(dehydrator);

			dehydrator.WriteRawArray(this.Random);
		}

		public class XMSSTreeSignature : IDisposable2 {

			private readonly XMSSExecutionContext xmssExecutionContext;

			public XMSSTreeSignature(XMSSExecutionContext xmssExecutionContext) {
				this.xmssExecutionContext = xmssExecutionContext;
			}

			public XMSSTreeSignature(MemoryBlockDoubleArray otsSignature, MemoryBlockDoubleArray auth, XMSSExecutionContext xmssExecutionContext) : this(xmssExecutionContext) {
				this.otsSignature = otsSignature;
				this.Auth = auth;
			}

			public MemoryBlockDoubleArray otsSignature { get; private set; }
			public MemoryBlockDoubleArray Auth { get; private set; }

			public void Dehydrate(IDataDehydrator dehydrator) {

				foreach(IByteArray sig in this.otsSignature) {
					dehydrator.WriteRawArray(sig);
				}

				foreach(IByteArray auth in this.Auth) {
					dehydrator.WriteRawArray(auth);
				}
			}

			public void Rehydrate(IDataRehydrator rehydrator, WotsPlusEngine wotsPlusProvider, int height) {

				int n = this.xmssExecutionContext.DigestSize;
				int totalSigs = wotsPlusProvider.Len;

				this.otsSignature = MemoryAllocators.Instance.doubleArrayCryptoAllocator.Take(totalSigs);

				for(int i = 0; i < totalSigs; i++) {
					this.otsSignature[i] = rehydrator.ReadArray(n);
				}

				int totalAuths = height;

				this.Auth = MemoryAllocators.Instance.doubleArrayCryptoAllocator.Take(totalAuths);

				for(int i = 0; i < totalAuths; i++) {
					this.Auth[i] = rehydrator.ReadArray(n);
				}
			}

		#region disposable

			public bool IsDisposed { get; private set; }

			public void Dispose() {
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing) {

				if(disposing && !this.IsDisposed) {
					this.DisposeAll();
				}

				this.IsDisposed = true;
			}

			~XMSSTreeSignature() {
				this.Dispose(false);
			}

			protected virtual void DisposeAll() {
				this.Auth?.Dispose();
				this.Auth = null;
				this.otsSignature?.Dispose();
				this.otsSignature = null;
			}

		#endregion

		}

	#region disposable

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {

			if(disposing && !this.IsDisposed) {
				this.DisposeAll();
			}

			this.IsDisposed = true;
		}

		~XMSSSignature() {
			this.Dispose(false);
		}

		protected virtual void DisposeAll() {
			this.Random?.Dispose();
			this.XmssTreeSignature?.Dispose();
		}

	#endregion

	}
}