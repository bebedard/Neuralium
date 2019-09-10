using System;
using Neuralia.Blockchains.Core;

namespace Neuralia.Blockchains.Common.Classes.Blockchains.Common.DataStructures.ExternalAPI.Wallet {
	public struct WalletAccountAPI {
		public Guid AccountUuid { get; set; }
		public string AccountId { get; set; }
		public string FriendlyName { get; set; }
		public bool IsActive { get; set; }
		public Enums.PublicationStatus Status { get; set; }
	}

	public struct WalletAccountDetailsAPI {
		public Guid AccountUuid { get; set; }
		public string AccountId { get; set; }
		public string AccountHash { get; set; }
		public string FriendlyName { get; set; }
		public bool IsActive { get; set; }
		public Enums.PublicationStatus Status { get; set; }
		public long DeclarationBlockid { get; set; }
		public bool KeysEncrypted { get; set; }
		public int AccountType { get; set; }
		public int TrustLevel { get; set; }
	}
}