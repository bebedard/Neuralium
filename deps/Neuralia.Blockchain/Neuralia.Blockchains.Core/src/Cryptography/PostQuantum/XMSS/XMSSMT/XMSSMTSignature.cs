using System;
using System.Collections.Generic;
using Neuralia.Blockchains.Core.Cryptography.PostQuantum.XMSS.Utils;
using Neuralia.Blockchains.Core.Cryptography.PostQuantum.XMSS.WOTS;
using Neuralia.Blockchains.Core.Cryptography.PostQuantum.XMSS.XMSS;
using Neuralia.Blockchains.Tools;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Core.Cryptography.PostQuantum.XMSS.XMSSMT {
	public class XMSSMTSignature : IDisposable2 {

		// versioning information
		public readonly byte Major = 1;
		public readonly byte Minor = 0;
		public readonly byte Revision = 0;
		protected readonly XMSSExecutionContext XmssExecutionContext;

		public XMSSMTSignature(XMSSExecutionContext xmssExecutionContext) {
			this.XmssExecutionContext = xmssExecutionContext;
		}

		public XMSSMTSignature(IByteArray random, long index, XMSSExecutionContext xmssExecutionContext) : this(xmssExecutionContext) {
			this.Random = random;
			this.Index = index;
		}

		public IByteArray Random { get; private set; }
		public long Index { get; private set; }

		public Dictionary<int, XMSSSignature> Signatures { get; } = new Dictionary<int, XMSSSignature>();

		public void Load(IByteArray signature, WotsPlusEngine wotsPlusProvider, int height, int layers) {
			IDataRehydrator rehydrator = DataSerializationFactory.CreateRehydrator(signature);

			this.Rehydrate(rehydrator, wotsPlusProvider, height, layers);
		}

		protected virtual void Rehydrate(IDataRehydrator rehydrator, WotsPlusEngine wotsPlusProvider, int height, int layers) {

			int type = rehydrator.ReadByte();
			int major = rehydrator.ReadByte();
			int minor = rehydrator.ReadByte();
			int increment = rehydrator.ReadByte();

			int n = this.XmssExecutionContext.DigestSize;

			this.Index = rehydrator.ReadLong();
			this.Random = rehydrator.ReadArray(n);

			int count = rehydrator.ReadInt();

			this.Signatures.Clear();

			for(int i = 0; i < count; i++) {

				int layer = rehydrator.ReadInt();

				IByteArray signatureBytes = rehydrator.ReadArray();

				XMSSSignature xmssSignature = new XMSSSignature(this.XmssExecutionContext);

				xmssSignature.Load(signatureBytes, wotsPlusProvider, height / layers);

				this.Signatures.Add(layer, xmssSignature);
			}
		}

		public IByteArray Save() {

			IDataDehydrator dehydrator = DataSerializationFactory.CreateDehydrator();

			this.Dehydrate(dehydrator);

			return dehydrator.ToArray();
		}

		protected virtual void Dehydrate(IDataDehydrator dehydrator) {

			dehydrator.Write((byte) 1);
			dehydrator.Write(this.Major);
			dehydrator.Write(this.Minor);
			dehydrator.Write(this.Revision);

			dehydrator.Write(this.Index);
			dehydrator.WriteRawArray(this.Random);

			dehydrator.Write(this.Signatures.Count);

			foreach(var signature in this.Signatures) {

				dehydrator.Write(signature.Key);

				IByteArray bytes = signature.Value.Save();
				dehydrator.Write(bytes);
				bytes.Return();
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

		~XMSSMTSignature() {
			this.Dispose(false);
		}

		protected virtual void DisposeAll() {

		}

	#endregion

	}
}