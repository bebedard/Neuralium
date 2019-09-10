using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards;
using Blockchains.Neuralium.Classes.NeuraliumChain.Dal.Interfaces.AccountSnapshots.Cards.Implementations;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Dal.Interfaces.AccountSnapshots.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Events.Digests.Channels.Specialization.Cards;
using Neuralia.Blockchains.Common.Classes.Blockchains.Common.Tools;
using Neuralia.Blockchains.Core.General.Types.Specialized;
using Neuralia.Blockchains.Tools.Serialization;

namespace Blockchains.Neuralium.Classes.NeuraliumChain.Events.Digests.Channels.Specialization.Cards {

	public interface INeuraliumStandardAccountSnapshotDigestChannelCard : INeuraliumAccountSnapshotDigestChannelCard, IStandardAccountSnapshotDigestChannelCard {
	}

	public class NeuraliumStandardAccountSnapshotDigestChannelCard : StandardAccountSnapshotDigestChannelCard, INeuraliumStandardAccountSnapshotDigestChannelCard {
		public List<INeuraliumAccountFreeze> AccountFreezes { get; } = new List<INeuraliumAccountFreeze>();

		public Amount Balance { get; set; } = new Amount();

		public override void Rehydrate(IDataRehydrator rehydrator) {
			base.Rehydrate(rehydrator);

			this.Balance.Rehydrate(rehydrator);

			this.AccountFreezes.Clear();
			bool any = rehydrator.ReadBool();

			if(any) {
				int count = rehydrator.ReadByte();

				for(int i = 0; i < count; i++) {
					INeuraliumAccountFreeze freeze = new NeuraliumAccountFreeze();

					freeze.FreezeId = rehydrator.ReadInt();
					Amount amount = new Amount();
					amount.Rehydrate(rehydrator);
					freeze.Amount = amount.Value;

					this.AccountFreezes.Add(freeze);
				}
			}
		}

		public override void Dehydrate(IDataDehydrator dehydrator) {
			base.Dehydrate(dehydrator);

			this.Balance.Dehydrate(dehydrator);

			bool any = this.AccountFreezes.Any();
			dehydrator.Write(any);

			if(any) {
				dehydrator.Write((byte) this.AccountFreezes.Count);

				foreach(INeuraliumAccountFreeze entry in this.AccountFreezes) {

					dehydrator.Write(entry.FreezeId);
					Amount amount = new Amount(entry.Amount);
					amount.Dehydrate(dehydrator);
				}
			}
		}

		public void CreateNewCollectionEntry(out INeuraliumAccountFreeze result) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.CreateNewCollectionEntry(this.AccountFreezes, out result);
		}

		public void AddCollectionEntry(INeuraliumAccountFreeze entry) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.AddCollectionEntry(entry, this.AccountFreezes);
		}

		public void RemoveCollectionEntry(Func<INeuraliumAccountFreeze, bool> predicate) {
			TypedCollectionExposureUtil<INeuraliumAccountFreeze>.RemoveCollectionEntry(predicate, this.AccountFreezes);
		}

		public INeuraliumAccountFreeze GetCollectionEntry(Func<INeuraliumAccountFreeze, bool> predicate) {
			return TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollectionEntry(predicate, this.AccountFreezes);
		}

		public List<INeuraliumAccountFreeze> GetCollectionEntries(Func<INeuraliumAccountFreeze, bool> predicate) {
			return TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollectionEntries(predicate, this.AccountFreezes);
		}

		ImmutableList<INeuraliumAccountFreeze> ITypedCollectionExposure<INeuraliumAccountFreeze>.CollectionCopy => TypedCollectionExposureUtil<INeuraliumAccountFreeze>.GetCollection(this.AccountFreezes);

		protected override IAccountFeature CreateAccountFeature() {
			return new NeuraliumAccountFeature();
		}

		protected override IAccountSnapshotDigestChannelCard CreateCard() {
			return new NeuraliumStandardAccountSnapshotDigestChannelCard();
		}
	}
}