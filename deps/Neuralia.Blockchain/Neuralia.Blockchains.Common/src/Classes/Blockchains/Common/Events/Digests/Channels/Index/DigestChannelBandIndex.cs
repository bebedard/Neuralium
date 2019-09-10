using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.FileInterpretationProviders;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.FileNamingProviders;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Utils;
using Neuralia.Blockchains.Core.Compression;
using Neuralia.Blockchains.Core.Cryptography.Trees;
using Neuralia.Blockchains.Tools.Data;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Index {

	//TODO: this entire digest engine needs a serious refactor. it was built very quickly under pressure...
	public interface IDigestChannelBandIndex {
		void Initialize();

		Dictionary<int, IByteArray> HashFiles(int groupIndex);

		List<int> GetFileTypes();

		IByteArray GetFileBytes(int fileId, uint partIndex, long offset, int length);
	}

	public interface IDigestChannelBandIndex<CARD_TYPE, KEY> : IDigestChannelBandIndex
		where CARD_TYPE : class
		where KEY : struct, IEquatable<KEY> {
	}

	public interface IDigestChannelBandIndex<CHANEL_BANDS, CARD_TYPE, KEY, INPUT_QUERY_KEY> : IDigestChannelBandIndex<CARD_TYPE, KEY>
		where CHANEL_BANDS : struct, Enum, IConvertible
		where CARD_TYPE : class
		where KEY : struct, IEquatable<KEY> {

		DigestChannelBandEntries<CARD_TYPE, CHANEL_BANDS> QueryCard(INPUT_QUERY_KEY key);
	}

	public interface IDigestChannelBandIndex<CHANEL_BANDS, CARD_TYPE, KEY, INPUT_QUERY_KEY, QUERY_KEY> : IDigestChannelBandIndex<CHANEL_BANDS, CARD_TYPE, KEY, INPUT_QUERY_KEY>
		where CHANEL_BANDS : struct, Enum, IConvertible
		where CARD_TYPE : class
		where KEY : struct, IEquatable<KEY> {
	}

	public interface IDigestChannelBandIndex<CHANEL_BANDS, CARD_TYPE, KEY, INPUT_QUERY_KEY, QUERY_KEY, NAMING_PROVIDER> : IDigestChannelBandIndex<CHANEL_BANDS, CARD_TYPE, KEY, INPUT_QUERY_KEY, QUERY_KEY>
		where CHANEL_BANDS : struct, Enum, IConvertible
		where CARD_TYPE : class
		where KEY : struct, IEquatable<KEY>
		where NAMING_PROVIDER : DigestChannelBandFileNamingProvider {

		List<CHANEL_BANDS> ChannelTypes { get; }
	}

	public abstract class DigestChannelBandIndex<CHANEL_BANDS, CARD_TYPE, KEY, INPUT_QUERY_KEY, QUERY_KEY, NAMING_PROVIDER> : IDigestChannelBandIndex<CHANEL_BANDS, CARD_TYPE, KEY, INPUT_QUERY_KEY, QUERY_KEY, NAMING_PROVIDER>
		where CHANEL_BANDS : struct, Enum, IConvertible
		where CARD_TYPE : class
		where KEY : struct, IEquatable<KEY>
		where NAMING_PROVIDER : DigestChannelBandFileNamingProvider {

		protected readonly string baseFolder;

		protected readonly CHANEL_BANDS enabledBands;
		protected readonly IFileSystem fileSystem;
		protected readonly string scopeFolder;

		protected DigestChannelBandIndex(string filename, string baseFolder, string scopeFolder, CHANEL_BANDS enabledBands, IFileSystem fileSystem) {
			this.enabledBands = enabledBands;

			EnumsUtils.RunForFlags(enabledBands, flag => {
				this.EnabledChannelBandTypes.Add(flag);
			});

			this.baseFolder = baseFolder;
			this.scopeFolder = scopeFolder;
			this.fileSystem = fileSystem;
		}

		public Dictionary<CHANEL_BANDS, IDigestChannelBandFileInterpretationProvider<CARD_TYPE, NAMING_PROVIDER>> Providers { get; } = new Dictionary<CHANEL_BANDS, IDigestChannelBandFileInterpretationProvider<CARD_TYPE, NAMING_PROVIDER>>();

		public List<CHANEL_BANDS> EnabledChannelBandTypes { get; } = new List<CHANEL_BANDS>();

		public List<CHANEL_BANDS> ChannelTypes => this.Providers.Keys.ToList();

		public virtual void Initialize() {

		}

		public abstract IByteArray GetFileBytes(int fileId, uint partIndex, long offset, int length);
		public abstract Dictionary<int, IByteArray> HashFiles(int groupIndex);

		public abstract List<int> GetFileTypes();

		public abstract DigestChannelBandEntries<CARD_TYPE, CHANEL_BANDS> QueryCard(INPUT_QUERY_KEY keySet);

		protected (string extractedName, CHANEL_BANDS band) EnsureFilesetExtracted(string bandName, object[] parameters, CHANEL_BANDS band) {
			return this.EnsureFilesetExtracted(new[] {(bandName, parameters, band)}).Single();
		}

		protected string GenerateFullPath(string filePath) {
			return Path.Combine(this.baseFolder, filePath);
		}

		protected void EnsureFileExtracted(string expandedFilename, string archivedFilename) {
			if(!this.fileSystem.File.Exists(expandedFilename)) {

				string directoryPath = Path.GetDirectoryName(expandedFilename);

				if(!this.fileSystem.Directory.Exists(directoryPath)) {
					this.fileSystem.Directory.CreateDirectory(directoryPath);
				}

				// ok, expand the file

				using(Stream input = File.OpenRead(archivedFilename)) {
					using(Stream output = File.OpenWrite(expandedFilename)) {
						Compressors.DigestCompressor.Decompress(input, output);
					}
				}
			}
		}

		public string GetExpandedBandName(CHANEL_BANDS band, object[] parameters) {
			return this.GenerateFullPath(this.Providers[band].NamingProvider.GeneratedExpandedFileName(band.ToString(), this.scopeFolder, parameters));

		}

		public string GetArchivedBandName(CHANEL_BANDS band, object[] parameters) {
			return this.GenerateFullPath(this.Providers[band].NamingProvider.GeneratedArchivedFileName(band.ToString(), this.scopeFolder, parameters));
		}

		public string GetExpandedBandName(CHANEL_BANDS band, uint index) {
			return this.GenerateFullPath(this.Providers[band].NamingProvider.GeneratedExpandedFileName(band.ToString(), this.scopeFolder, new object[] {index}));

		}

		public string GetArchivedBandName(CHANEL_BANDS band, uint index) {
			return this.GenerateFullPath(this.Providers[band].NamingProvider.GeneratedArchivedFileName(band.ToString(), this.scopeFolder, new object[] {index}));
		}

		public string GetExpandedBandName(CHANEL_BANDS band) {
			return this.GenerateFullPath(this.Providers[band].NamingProvider.GeneratedExpandedFileName(band.ToString(), this.scopeFolder, new object[] { }));

		}

		public string GetArchivedBandName(CHANEL_BANDS band) {
			return this.GenerateFullPath(this.Providers[band].NamingProvider.GeneratedArchivedFileName(band.ToString(), this.scopeFolder, new object[] { }));
		}

		protected List<(string extractedName, CHANEL_BANDS band)> EnsureFilesetExtracted((string bandName, object[] parameters, CHANEL_BANDS band)[] bandNames) {

			var results = new List<(string extractedName, CHANEL_BANDS band)>();

			// ensure each band is extracted, otherwise do so
			foreach(var bandName in bandNames) {
				string expandedFilename = this.GetExpandedBandName(bandName.band, bandName.parameters);
				string archivedFilename = this.GetArchivedBandName(bandName.band, bandName.parameters);

				this.EnsureFileExtracted(expandedFilename, archivedFilename);
				results.Add((expandedFilename, bandName.band));
			}

			return results;
		}

		protected IByteArray HashFile(string filename) {
			Sha3SakuraTree hasher = new Sha3SakuraTree();

			using(FileStreamSliceHashNodeList streamSliceHashNodeList = new FileStreamSliceHashNodeList(filename, this.fileSystem)) {
				return hasher.Hash(streamSliceHashNodeList);
			}
		}
	}
}