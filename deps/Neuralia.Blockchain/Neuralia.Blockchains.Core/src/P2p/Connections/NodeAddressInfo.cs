using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Neuralia.Blockchains.Core.Network;
using Neuralia.Blockchains.Core.Services;
using Neuralia.Blockchains.Tools.Serialization;

namespace Neuralia.Blockchains.Core.P2p.Connections {
	/// <summary>
	///     A simple structure to hold ip and port of a network nodeAddressInfo
	/// </summary>
	public class NodeAddressInfo : IBinarySerializable {

		private IPAddress address;

		/// <summary>
		///     the IP, always in IPv6 format
		/// </summary>
		private IPAddress cacheAdjustedV6Address;

		public NodeAddressInfo(string ip, int port, Enums.PeerTypes peerType, Dictionary<BlockchainType, ChainSettings> chainSettings) : this(ip, (int?) port, peerType, chainSettings) {

		}

		public NodeAddressInfo(string ip, int? port, Enums.PeerTypes peerType, Dictionary<BlockchainType, ChainSettings> chainSettings) : this(ip, port, peerType) {
			this.ChainSettings = chainSettings;
		}

		public NodeAddressInfo(string ip, int port, Enums.PeerTypes peerType) {
			this.Ip = ip;
			this.Port = port == 0 ? null : (int?) port;
			this.PeerType = this.PeerType;

			this.UpdateNetworkEndPoint();
		}

		public NodeAddressInfo(string ip, int? port, Enums.PeerTypes peerType) {
			this.Ip = ip;
			this.Port = port;
			this.PeerType = this.PeerType;

			this.UpdateNetworkEndPoint();
		}

		public NodeAddressInfo(string ip, Enums.PeerTypes peerType) {
			this.Ip = ip;
			this.Port = null;
			this.PeerType = this.PeerType;

			this.UpdateNetworkEndPoint();
		}

		public NodeAddressInfo(IPAddress ip, int port, Enums.PeerTypes peerType) {
			this.Address = ip;
			this.Port = port == 0 ? null : (int?) port;
			this.PeerType = this.PeerType;

			this.UpdateNetworkEndPoint();
		}

		public NodeAddressInfo(IPAddress ip, int? port, Enums.PeerTypes peerType) {
			this.Address = ip;
			this.Port = port;
			this.PeerType = this.PeerType;

			this.UpdateNetworkEndPoint();
		}

		public NodeAddressInfo(IPAddress ip, Enums.PeerTypes peerType) {
			this.Address = ip;
			this.Port = null;
			this.PeerType = this.PeerType;

			this.UpdateNetworkEndPoint();
		}

		private NodeAddressInfo() {

		}

		/// <summary>
		///     is the node directly connectable? will be false if port is closed behind a firewall
		/// </summary>
		public bool IsConnectable { get; set; }

		public NetworkEndPoint NetworkEndPoint { get; private set; }

		/// <summary>
		///     Here we store the calculated concensus between all peers and the data they sent us. hopefully they all agree!
		/// </summary>
		public Dictionary<BlockchainType, ChainSettings> ChainSettings { get; private set; } = new Dictionary<BlockchainType, ChainSettings>();

		public string AdjustedIp => this.AdjustedAddress.ToString();

		/// <summary>
		///     The IP adjusted to its true format, V4 or V6
		/// </summary>
		public IPAddress AdjustedAddress { get; private set; }

		public string Ip {
			get => this.cacheAdjustedV6Address.ToString();
			set {
				if(IPAddress.TryParse(value, out IPAddress valueAddress)) {
					this.Address = valueAddress;

					this.UpdateNetworkEndPoint();
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		public IPAddress Address {
			get => this.address;
			set {
				this.address = value;

				if(this.IsIpV4 && !this.IsIpv4MappedToIpV6) {
					this.address = value.MapToIPv6();

					// IPv4
					this.AdjustedAddress = value;
				} else {

					// IPv6
					this.AdjustedAddress = this.IsIpv4MappedToIpV6 ? this.address.MapToIPv4() : this.address;
				}

				this.cacheAdjustedV6Address = this.address;

				this.UpdateNetworkEndPoint();
			}
		}

		public bool IsIpV6 => IsAddressIpV6(this.address);
		public bool IsIpV4 => IsAddressIpV4(this.address);
		public bool IsIpv4MappedToIpV6 => IsAddressIpv4MappedToIpV6(this.address);

		public Enums.PeerTypes PeerType { get; set; }

		public bool IsPeerTypeKnown => this.PeerType != Enums.PeerTypes.Unknown;

		/// <summary>
		///     here we ensure to always return the actual port, never null
		/// </summary>
		/// <returns></returns>
		public int RealPort {
			get => this.Port ?? GlobalsService.DEFAULT_PORT;
			set {
				this.Port = value;
				this.UpdateNetworkEndPoint();
			}
		}

		public int? Port { get; private set; }

		/// <summary>
		///     always V6 format
		/// </summary>
		public string ScopedIp => $"[{this.Ip}]:{this.RealPort}";

		/// <summary>
		///     Ipv4 or V6 format
		/// </summary>
		public string ScopedAdjustedIp => $"[{this.AdjustedIp}]:{this.RealPort}";

		public void Dehydrate(IDataDehydrator dehydrator) {
			dehydrator.Write(this.Ip);
			dehydrator.Write(this.Port);
			dehydrator.Write((byte) this.PeerType);
			dehydrator.Write(this.IsConnectable);

			// now the chain optionsBase
			dehydrator.Write(this.ChainSettings.Count);

			foreach(var chainSetting in this.ChainSettings) {
				dehydrator.Write(chainSetting.Key.Value);

				chainSetting.Value.Dehydrate(dehydrator);
			}
		}

		public void Rehydrate(IDataRehydrator rehydrator) {
			this.Ip = rehydrator.ReadString();
			this.Port = rehydrator.ReadNullableInt();
			this.PeerType = (Enums.PeerTypes) rehydrator.ReadByte();
			this.IsConnectable = rehydrator.ReadBool();

			this.ChainSettings.Clear();
			int chainSettingCount = rehydrator.ReadInt();

			for(int j = 0; j < chainSettingCount; j++) {
				BlockchainType chainId = rehydrator.ReadUShort();

				ChainSettings chainSetting = new ChainSettings();
				chainSetting.Rehydrate(rehydrator);

				this.ChainSettings.Add(chainId, chainSetting);
			}

			this.UpdateNetworkEndPoint();
		}

		private void UpdateNetworkEndPoint() {
			this.NetworkEndPoint = new NetworkEndPoint(this.address, this.RealPort, this.IsIpV6 ? IPMode.IPv6 : IPMode.IPv4);
		}

		public static bool IsAddressIpV6(IPAddress address) {
			return (address.AddressFamily == AddressFamily.InterNetworkV6) && !address.IsIPv4MappedToIPv6;
		}

		public static bool IsAddressIpV4(IPAddress address) {
			return (address.AddressFamily == AddressFamily.InterNetwork) || IsAddressIpv4MappedToIpV6(address);
		}

		public static bool IsAddressIpv4MappedToIpV6(IPAddress address) {
			return (address.AddressFamily == AddressFamily.InterNetworkV6) && address.IsIPv4MappedToIPv6;
		}

		public static bool IsAddressIpV6(NetworkEndPoint remoteEndPoint) {
			if(remoteEndPoint.EndPoint is IPEndPoint ip) {
				return IsAddressIpV6(ip.Address);
			}

			return remoteEndPoint.IPMode == IPMode.IPv6;
		}

		public static bool IsAddressIpV4(NetworkEndPoint remoteEndPoint) {
			if(remoteEndPoint.EndPoint is IPEndPoint ip) {
				return IsAddressIpV4(ip.Address);
			}

			return remoteEndPoint.IPMode == IPMode.IPv4;
		}

		public static bool IsAddressIpv4MappedToIpV6(NetworkEndPoint remoteEndPoint) {
			if(remoteEndPoint.EndPoint is IPEndPoint ip) {
				return IsAddressIpv4MappedToIpV6(ip.Address);
			}

			return false;
		}

		public static IPAddress GetAddressIpV4(NetworkEndPoint remoteEndPoint) {
			if(IsAddressIpv4MappedToIpV6(remoteEndPoint) && remoteEndPoint.EndPoint is IPEndPoint ip) {
				return ip.Address.MapToIPv4();
			}

			return null;
		}

		/// <summary>
		///     ensure the address is in ipv6 format, which we support
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public static IPAddress PrepareAddress(IPAddress address) {
			return IsAddressIpV4(address) ? address.MapToIPv6() : address;
		}

		/// <summary>
		///     ensure the address is in ipv6 format, which we support
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public static IPEndPoint PrepareAddress(IPEndPoint endpoint) {
			return new IPEndPoint(PrepareAddress(endpoint.Address), endpoint.Port);
		}

		public override string ToString() {
			return $"{this.ScopedAdjustedIp}";
		}

		public static NodeAddressInfo CreateEmpty() {
			return new NodeAddressInfo();
		}

		public bool Equals(NodeAddressInfo other) {
			if(ReferenceEquals(null, other)) {
				return false;
			}

			return (this.Port == other.Port) && this.EqualsIp(other);
		}

		public bool EqualsIp(NodeAddressInfo other) {
			return Equals(this.address, other.address);
		}

		public static bool operator ==(NodeAddressInfo a, NodeAddressInfo b) {
			if(ReferenceEquals(null, a)) {
				return ReferenceEquals(null, b);
			}

			return a.Equals(b);
		}

		public static bool operator !=(NodeAddressInfo a, NodeAddressInfo b) {
			return !(a == b);
		}

		/// <summary>
		///     a nodeAddressInfo is equal if they have the same IP. port doesnt matter really
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj) {
			if(ReferenceEquals(null, obj)) {
				return false;
			}

			if(ReferenceEquals(this, obj)) {
				return true;
			}

			if(obj.GetType() != this.GetType()) {
				return false;
			}

			return this.Equals((NodeAddressInfo) obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (this.Port.GetHashCode() * 397) ^ (this.address != null ? this.address.GetHashCode() : 0);
			}
		}

		public void SetChainSettings(Dictionary<BlockchainType, ChainSettings> chainSettings) {

			this.ChainSettings = chainSettings;
		}
	}
}