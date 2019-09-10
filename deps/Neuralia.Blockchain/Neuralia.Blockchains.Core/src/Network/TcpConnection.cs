﻿using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Neuralia.Blockchains.Core.Extensions;
using Neuralia.Blockchains.Core.General.Types.Dynamic;
using Neuralia.Blockchains.Core.Network.Exceptions;
using Neuralia.Blockchains.Core.Network.Protocols;
using Neuralia.Blockchains.Core.P2p.Connections;
using Neuralia.Blockchains.Tools;
using Neuralia.Blockchains.Tools.Data;
using Neuralia.Blockchains.Tools.General.ExclusiveOptions;
using Neuralia.Blockchains.Tools.Serialization;
using Serilog;

namespace Neuralia.Blockchains.Core.Network {

	public interface ITcpConnection : IDisposable2 {
		EndPoint RemoteEndPoint { get; }
		IPMode IPMode { get; }
		NetworkEndPoint EndPoint { get; }
		ConnectionState State { get; }

		Guid ReportedUuid { get; }
		Guid InternalUuid { get; }
		bool IsConnectedUuidProvidedSet { get; }

		event TcpConnection.MessageBytesReceived DataReceived;
		event EventHandler<DisconnectedEventArgs> Disconnected;
		event Action<Guid> ConnectedUuidProvided;

		void Close();
		void Connect(IByteArray bytes, int timeout = 5000);
		bool SendMessage(long hash);
		MessageInstance SendBytes(IByteArray bytes);
		void StartWaitingForHandshake(TcpConnection.MessageBytesReceived handshakeCallback);
		bool CheckConnected();

		bool PerformCounterConnection(int port);
	}

	public interface IProtocolTcpConnection : ITcpConnection {
		void SendSocketBytes(IByteArray bytes, bool sendSize = true);
		void SendSocketBytes(in Span<byte> bytes, bool sendSize = true);
	}

	public static class TcpConnection {

		public delegate void ExceptionOccured(Exception exception, ITcpConnection connection);

		public delegate void MessageBytesReceived(IByteArray buffer);

		/// <summary>
		///     All the protocol messages we support
		/// </summary>
		[Flags]
		public enum ProtocolMessageTypes : short {
			None = 0,
			Tiny = 1 << 0,
			Small = 1 << 1,
			Medium = 1 << 2,
			Large = 1 << 3,
			Split = 1 << 4,
			All = Tiny | Small | Medium | Large | Split
		}
	}

	public abstract class TcpConnection<READING_CONTEXT> : IProtocolTcpConnection
		where READING_CONTEXT : ITcpReadingContext {

		/// <summary>
		///     Reset event that is triggered when the connection is marked Connected.
		/// </summary>
		private readonly ManualResetEvent connectWaitLock = new ManualResetEvent(false);

		protected readonly TcpConnection.ExceptionOccured exceptionCallback;

		protected readonly bool isServer;

		protected readonly object locker = new object();

		protected readonly ProtocolFactory protocolFactory = new ProtocolFactory();

		private readonly ShortExclusiveOption<TcpConnection.ProtocolMessageTypes> protocolMessageFilters;
		private readonly AdaptiveInteger1_4 receiveByteShrinker = new AdaptiveInteger1_4();

		private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);

		private readonly AdaptiveInteger1_4 sendByteShrinker = new AdaptiveInteger1_4();

		/// <summary>
		///     The socket we're managing.
		/// </summary>
		protected readonly Socket socket;

		/// <summary>
		///     if true, any exception will be alerted. whjen we know what we are doing and we want to shutup any further noise, we
		///     set this to false.
		/// </summary>
		private bool alertExceptions = true;

		protected TcpConnectionInformation connectionInfo;

		protected Task dataReceptionTask;

		protected IByteArray handshakeBytes;

		/// <summary>
		///     Did we send the handshake payload yet?
		/// </summary>
		protected HandshakeStatuses handshakeStatus = HandshakeStatuses.NotStarted;

		protected ProtocolCompression peerProtocolCompression;

		protected ProtocolVersion peerProtocolVersion;
		protected int receiveBufferSize;

		private volatile ConnectionState state;

		protected CancellationTokenSource tokenSource;

		public TcpConnection(TcpConnection.ExceptionOccured exceptionCallback, bool isServer = false, ShortExclusiveOption<TcpConnection.ProtocolMessageTypes> protocolMessageFilters = null) {
			this.isServer = isServer;

			this.State = ConnectionState.NotConnected;
			this.exceptionCallback = exceptionCallback;

			// the ability to filter which types of message this socket will allow
			if(protocolMessageFilters == null) {
				this.protocolMessageFilters = TcpConnection.ProtocolMessageTypes.All;
			} else {
				this.protocolMessageFilters = protocolMessageFilters;
			}

			this.tokenSource = new CancellationTokenSource();
		}

		/// <summary>
		///     Creates a TcpConnection from a given TCP Socket. usually called by the TcpServer
		/// </summary>
		/// <param name="socket">The TCP socket to wrap.</param>
		public TcpConnection(Socket socket, TcpConnection.ExceptionOccured exceptionCallback, bool isServer = false, ShortExclusiveOption<TcpConnection.ProtocolMessageTypes> protocolMessageFilters = null) : this(exceptionCallback, isServer, protocolMessageFilters) {
			//Check it's a TCP socket
			if(socket.ProtocolType != ProtocolType.Tcp) {
				throw new ArgumentException("A TcpConnection requires a TCP socket.");
			}

			this.isServer = isServer;

			this.EndPoint = new NetworkEndPoint(socket.RemoteEndPoint);
			this.RemoteEndPoint = socket.RemoteEndPoint;

			this.socket = socket;
			this.socket.NoDelay = true;

			this.connectionInfo = this.socket.GetActiveConnectionInformation();

			this.SocketNewlyConnected();

			this.State = ConnectionState.Connected;
		}

		public TcpConnection(NetworkEndPoint remoteEndPoint, TcpConnection.ExceptionOccured exceptionCallback, bool isServer = false, ShortExclusiveOption<TcpConnection.ProtocolMessageTypes> protocolMessageFilters = null) : this(exceptionCallback, isServer, protocolMessageFilters) {

			if(this.State != ConnectionState.NotConnected) {
				throw new InvalidOperationException("Cannot connect as the Connection is already connected.");
			}

			this.isServer = isServer;

			this.EndPoint = remoteEndPoint;
			this.RemoteEndPoint = remoteEndPoint.EndPoint;
			this.IPMode = remoteEndPoint.IPMode;

			//Create a socket
			if(NodeAddressInfo.IsAddressIpV4(remoteEndPoint)) {
				this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			} else {
				if(!Socket.OSSupportsIPv6) {
					throw new P2pException("IPV6 not supported!", P2pException.Direction.Send, P2pException.Severity.Casual);
				}

				this.socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
				this.socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
			}

			this.socket.InitializeSocketParameters();
		}

		public bool IsDisposing { get; set; }

		// A UUiD we set and use itnernally
		public Guid InternalUuid { get; } = Guid.NewGuid();

		public Guid ReportedUuid { get; private set; }

		public void SendSocketBytes(IByteArray bytes, bool sendSize = true) {
			//Write the bytes to the socket

			this.SendSocketBytes(bytes.Span, sendSize);
		}

		public void SendSocketBytes(in Span<byte> bytes, bool sendSize = true) {
			//Write the bytes to the socket

			try {
				if(this.State != ConnectionState.Connected) {
					throw new SocketException((int) SocketError.Shutdown);
				}

				Task<bool> task = null;

				lock(this.locker) {
					if(sendSize) {
						// write the size of the message first
						this.sendByteShrinker.Value = (uint) bytes.Length;
						this.WritePart((Span<byte>) this.sendByteShrinker.GetShrunkBytes());
					}

					// now the message
					task = this.Write(bytes);

					task?.Wait();
				}
			} catch(Exception e) {
				P2pException he = new P2pException("Could not send data as an occured.", P2pException.Direction.Send, P2pException.Severity.Casual, e);
				this.Close();

				throw he;
			}
		}

		public EndPoint RemoteEndPoint { get; }

		public IPMode IPMode { get; }

		public NetworkEndPoint EndPoint { get; }

		public bool CheckConnected() {
			if(!this.socket.IsReallyConnected(this.connectionInfo)) {
				// yes, we try twice, just in case...
				if(!this.socket.IsReallyConnected(this.connectionInfo)) {
					// ok, we give up, connection is disconnected
					this.state = ConnectionState.NotConnected;

					// seems we are not connected after all
					Log.Verbose("Socket was disconnected ungracefully from the other side. Disconnecting.");
					this.Close();

					return false;
				}
			}

			return true;
		}

		public ConnectionState State {

			get {
				lock(this.locker) {
					if(this.IsDisposed || this.IsDisposing) {
						return ConnectionState.NotConnected;
					}

					if(this.state == ConnectionState.Connected) {
						// here we believe we are still connected. lets confirm that we really still are by checking the socket

						this.CheckConnected();

						return this.state; // we truly are connected
					}

					return this.state;
				}
			}

			protected set {

				lock(this.locker) {
					if(this.IsDisposed || this.IsDisposing) {
						return;
					}

					this.state = value;

					if(this.state == ConnectionState.Connected) {
						this.connectWaitLock.Set();
					} else {
						this.connectWaitLock.Reset();
					}
				}
			}
		}

		public bool IsDisposed { get; set; }

		public event TcpConnection.MessageBytesReceived DataReceived;

		public event EventHandler<DisconnectedEventArgs> Disconnected;
		public event Action<Guid> ConnectedUuidProvided;

		public bool IsConnectedUuidProvidedSet => this.ConnectedUuidProvided != null;

		public void Close() {
			this.Dispose();
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Connect(IByteArray bytes, int timeout = 5000) {
			if(this.IsDisposed || this.IsDisposing) {
				throw new SocketException((int) SocketError.Shutdown);
			}

			if((bytes == null) || bytes.IsEmpty) {
				throw new TcpApplicationException("Handshake bytes can not be null");
			}

			//Connect
			this.State = ConnectionState.Connecting;

			try {
				// we want this synchronously
				EndPoint endpoint = this.RemoteEndPoint;

				if(NodeAddressInfo.IsAddressIpV4(this.EndPoint) && NodeAddressInfo.IsAddressIpv4MappedToIpV6(this.EndPoint)) {
					endpoint = new IPEndPoint(NodeAddressInfo.GetAddressIpV4(this.EndPoint), this.EndPoint.EndPoint.Port);
				}

				IAsyncResult result = this.socket.BeginConnect(endpoint, null, null);
				bool success = result.AsyncWaitHandle.WaitOne(1000 * 10, true);

				if(!success) {
					throw new SocketException((int) SocketError.TimedOut);
				}

				this.connectionInfo = this.socket.GetActiveConnectionInformation();

				if(this.socket.IsReallyConnected(this.connectionInfo)) {

					this.SocketNewlyConnected();
				} else {
					this.Dispose();

					throw new SocketException((int) SocketError.Shutdown);
				}
			} catch(Exception e) {
				this.Dispose();

				throw new P2pException("Could not connect as an exception occured.", P2pException.Direction.Send, P2pException.Severity.Casual, e);
			}

			//Set connected
			this.State = ConnectionState.Connected;

			//Start receiving data
			this.StartReceivingData();

			this.handshakeBytes = bytes;
			this.handshakeStatus = HandshakeStatuses.VersionSentNoBytes;

			//Send handshake
			this.SendHandshakeVersion();

			// now wait for the handshake to complete
			this.resetEvent.WaitOne(TimeSpan.FromSeconds(10));

			if(this.handshakeStatus != HandshakeStatuses.Completed) {

				this.Dispose();

				throw new TcpApplicationException("Failed to complete handshake");
			}
		}

		/// <summary>
		///     Here we try a quick counterconnect to establish if their connection port is open and available
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		/// <exception cref="P2pException"></exception>
		public bool PerformCounterConnection(int port) {

			Socket counterSocket = null;

			try {

				IPEndPoint endpoint = new IPEndPoint(((IPEndPoint) this.RemoteEndPoint).Address, port);

				if(NodeAddressInfo.IsAddressIpV4(endpoint.Address)) {
					counterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				} else {
					if(!Socket.OSSupportsIPv6) {
						throw new P2pException("IPV6 not supported!", P2pException.Direction.Send, P2pException.Severity.Casual);
					}

					counterSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
					counterSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
				}

				counterSocket.InitializeSocketParameters();

				IAsyncResult result = counterSocket.BeginConnect(endpoint, null, null);

				if(result.AsyncWaitHandle.WaitOne(1000 * 10, true)) {

					if(counterSocket.Send(ProtocolFactory.HANDSHAKE_COUNTERCONNECT_BYTES) == ProtocolFactory.HANDSHAKE_COUNTERCONNECT_BYTES.Length) {
						return true;
					}
				}
			} catch {
				// do nothing, we got our answer
			} finally {
				try {
					counterSocket?.Dispose();
				} catch {
					// do nothing, we got our answer
				}
			}

			return false;
		}

		public bool SendMessage(long hash) {
			MessageInstance message = null;

			lock(this.locker) {
				message = this.protocolFactory.WrapMessage(hash);
			}

			if(message == null) {
				return false;
			}

			this.SendMessage(message);

			return true;
		}

		public MessageInstance SendBytes(IByteArray bytes) {

			if((bytes == null) || bytes.IsEmpty) {
				throw new TcpApplicationException("The message bytes can not be null");
			}

			MessageInstance messageInstance = null;

			lock(this.locker) {
				messageInstance = this.protocolFactory.WrapMessage(bytes, this.protocolMessageFilters);
			}

			this.SendMessage(messageInstance);

			return messageInstance;
		}

		public void StartWaitingForHandshake(TcpConnection.MessageBytesReceived handshakeCallback) {

			this.StartReceivingData(handshakeCallback);
		}

		protected virtual void SocketNewlyConnected() {

		}

		protected virtual void SocketClosed() {

		}

		protected virtual void StartReceivingData(TcpConnection.MessageBytesReceived handshakeCallback = null) {

			if(this.IsDisposed) {
				throw new TcpApplicationException("Can not reuse a disposed tcp connection");
			}

			if(this.State != ConnectionState.Connected) {
				throw new TcpApplicationException("Socket is not connected");
			}

			this.dataReceptionTask = this.StartReceptionStream(this.tokenSource.Token, handshakeCallback).WithAllExceptions().ContinueWith(task => {
				if(task.IsFaulted) {
					// an exception occured. but alert only if we should, otherwise let the connection die silently.

					if(this.alertExceptions) {
						P2pException exception = new P2pException("A serious exception occured while receiving data from the socket.", P2pException.Direction.Receive, P2pException.Severity.VerySerious, task.Exception);

						try {
							this.Close();
						} finally {
							// inform the users we had a serious exception. We only invoke this when we receive data, since we want to capture evil peers. we trust ourselves, so we dont act on our own sending errors.
							this.exceptionCallback?.Invoke(exception, this);
						}
					}
				}

				this.resetEvent.Set();
			});
		}

		private async Task<int> StartReceptionStream(CancellationToken ct, TcpConnection.MessageBytesReceived handshakeCallback = null) {

			try {

				await this.ReadHandshake(ct);

				if(this.handshakeStatus == HandshakeStatuses.NotStarted) {
					throw new TcpApplicationException("Handshake protocol has failed");
				}

				if(this.handshakeStatus == HandshakeStatuses.Completed) {
					this.resetEvent.Set();
				}

				try {
					await this.ReadMessage(bytes => {

						if(handshakeCallback != null) {

							this.TriggerHandshakeCallback(ref handshakeCallback, bytes);

							this.handshakeStatus = HandshakeStatuses.Completed;

							this.resetEvent.Set();
						} else {
							// data received
							this.InvokeDataReceived(bytes);
						}
					}, ct);
				} catch(InvalidPeerException ipex) {
					// ok, we got an invalid peer. we dont need to do anything, lost let it go and disconnect
					this.alertExceptions = false;
				} catch(TaskCanceledException tex) {
					// the cancel request might be normal, we dont cause exception right away
					//TODO: how do we handle this?
					this.alertExceptions = false;
				} catch(ObjectDisposedException ode) {
					// do nothing
					this.alertExceptions = false;
				}

			} catch(CounterConnectionException cex) {
				this.alertExceptions = false;
			} finally {
				// disconnected
				this.Close();
			}

			return 1;
		}

		protected virtual void TriggerHandshakeCallback(ref TcpConnection.MessageBytesReceived handshakeCallback, IByteArray bytes) {

			if(this.handshakeStatus != HandshakeStatuses.VersionReceivedNoBytes) {
				throw new TcpApplicationException("Handshake bytes received in the wrong order");
			}

			handshakeCallback(bytes);
			handshakeCallback = null; // we never call it again
		}

		/// <summary>
		///     Write and flush a message
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		protected Task<bool> Write(in ReadOnlySpan<byte> message) {

			lock(this.locker) {
				this.WritePart(message);

				return this.CompleteWrite();
			}
		}

		/// <summary>
		///     Write to the socket, but dont sent it. amke sure you call CompleteWrite()
		/// </summary>
		/// <param name="message"></param>
		protected abstract void WritePart(in ReadOnlySpan<byte> message);

		/// <summary>
		///     Send anything in the buffer
		/// </summary>
		/// <returns></returns>
		protected abstract Task<bool> CompleteWrite();

		private async Task ReadHandshake(CancellationToken cancellationNeuralium = default) {

			READING_CONTEXT read = await this.ReadDataFrame(default, cancellationNeuralium);

			if(read.IsCanceled) {
				this.ReadTaskCancelled();

				throw new TaskCanceledException();
			}

			// can we find a complete frame?
			if(!read.IsEmpty && (read.Length == ProtocolFactory.HANDSHAKE_COUNTERCONNECT_BYTES.Length)) {

				var counterBytes = new byte[read.Length];
				read.CopyTo(counterBytes, 0, 0, counterBytes.Length);

				if(counterBytes.SequenceEqual(ProtocolFactory.HANDSHAKE_COUNTERCONNECT_BYTES)) {
					// ok, we just received a port confirmation counterconnect. we do nothing further with this connection
					this.Dispose();

					throw new CounterConnectionException();
				}
			}

			if(!read.IsEmpty && (read.Length == ProtocolFactory.HANDSHAKE_PROTOCOL_SIZE)) {
				// ok, we should have our handshake request
				(ProtocolVersion version, ProtocolCompression compression, Guid uuid) = this.ParseVersion(read);
				this.peerProtocolVersion = version;
				this.peerProtocolCompression = compression;

				if(uuid == Guid.Empty) {
					throw new TcpApplicationException("Peer uuid cannot be empty");
				}

				this.ReportedUuid = uuid;

				// lets alert that we have a peer uuid, see if we accept it
				try {
					this.ConnectedUuidProvided?.Invoke(uuid);
				} catch {
					// we can't go further, stop.
					this.handshakeStatus = HandshakeStatuses.Unusable;

					throw;
				}

				this.protocolFactory.SetPeerProtocolVersion(this.peerProtocolVersion);
				this.protocolFactory.SetPeerProtocolCompression(this.peerProtocolCompression);

				read.DataRead((int) read.Length);

				if(this.handshakeStatus == HandshakeStatuses.NotStarted) {
					// we are a server and are awaiting for the handshake bytes now
					// now reply and send our own version

					//TODO: as the server, we have the right to insist on another compression mode instead of taking the client's. lets keep this in mind.
					this.SendHandshakeVersion();

					this.handshakeStatus = HandshakeStatuses.VersionReceivedNoBytes;
				} else if(this.handshakeStatus == HandshakeStatuses.VersionSentNoBytes) {
					// ok, we are the client, now is the time to send the handshake bytes
					this.SendHandshakeBytes();

					this.handshakeStatus = HandshakeStatuses.Completed;
				}
			} else {
				throw new TcpApplicationException("invalid protocol handshake format");
			}
		}

		protected virtual (ProtocolVersion version, ProtocolCompression compression, Guid uuid) ParseVersion(READING_CONTEXT read) {

			Span<byte> header = stackalloc byte[ProtocolFactory.HANDSHAKE_PROTOCOL_SIZE];
			read.CopyTo(header, 0, 0, header.Length);

			return this.protocolFactory.ParseVersion(header);
		}

		protected abstract Task<READING_CONTEXT> ReadDataFrame(READING_CONTEXT previous, CancellationToken ct);

		protected virtual void ReadTaskCancelled() {

		}

		protected async Task ReadMessage(TcpConnection.MessageBytesReceived callback, CancellationToken cancellationNeuralium = default) {
			IByteArray mainBuffer = null;

			int bytesCopied = 0;
			int sizeByteSize = 0;

			int tryAttempt = 0;

			READING_CONTEXT read = default;

			while(true) {
				if(cancellationNeuralium.IsCancellationRequested) {
					this.ReadTaskCancelled();

					throw new TaskCanceledException();
				}

				read = await this.ReadDataFrame(read, cancellationNeuralium);

				if(read.IsCanceled) {
					this.ReadTaskCancelled();

					throw new TaskCanceledException();
				}

				// can we find a complete frame?
				if(!read.IsEmpty) {

					// first thing, extract the message size

					int segmentOffset = 0;

					// first we always read the message size
					int messageSize = 0;

					if(sizeByteSize == 0) {
						try {
							bytesCopied = 0;
							tryAttempt = 0;
							sizeByteSize = this.receiveByteShrinker.ReadBytes(read);
							messageSize = (int) this.receiveByteShrinker.Value;

							// yup, we will need this so lets not make it clearable
							mainBuffer = new ByteArray(messageSize);

							read.DataRead(sizeByteSize);

							continue;

						} catch(Exception) {
							sizeByteSize = 0;
							messageSize = 0;
						}
					} else if(mainBuffer != null) {

						int usefulBufferLength = (int) read.Length - segmentOffset;

						// accumulate data		
						if(usefulBufferLength != 0) {

							tryAttempt = 0;

							if(mainBuffer.Length < usefulBufferLength) {
								usefulBufferLength = mainBuffer.Length;
							}

							int remainingLength = usefulBufferLength + bytesCopied;

							if(remainingLength > mainBuffer.Length) {
								usefulBufferLength = mainBuffer.Length - bytesCopied;
							}

							//lets check if we received more data than we expected. if we did, this is critical, means everything is offsetted. this is serious and we break.
							if(bytesCopied > mainBuffer.Length) {
								throw new TcpApplicationException("The amount of data received is greater than expected. fatal error.");
							}

							read.CopyTo(mainBuffer.Span, segmentOffset, bytesCopied, usefulBufferLength);

							bytesCopied += usefulBufferLength;

							// ok we are done with this
							read.DataRead(usefulBufferLength);

							if(bytesCopied == mainBuffer.Length) {

								IMessageEntry messageEntry = null;

								//we expect to read the header to start. if the header is corrupted, this will break and thats it.
								lock(this.locker) {
									messageEntry = this.protocolFactory.CreateMessageParser(mainBuffer).RehydrateHeader(this.protocolMessageFilters);
								}

								// use the entry
								IMessageEntry entry = messageEntry;
								sizeByteSize = 0;
								messageSize = 0;
								IDataRehydrator bufferRehydrator = DataSerializationFactory.CreateRehydrator(mainBuffer);

								// skip the header offset
								bufferRehydrator.Forward(messageEntry.Header.MessageOffset);

								// free the message entry for another message
								mainBuffer = null;
								messageEntry = null;

								if(cancellationNeuralium.IsCancellationRequested) {
									this.ReadTaskCancelled();

									throw new TaskCanceledException();
								}

								//lets handle the completed message. we can launch it in its own thread since message pumping can continue meanwhile independently
								await Task<bool>.Factory.StartNew(() => {

									entry.SetMessageContent(bufferRehydrator);

									this.protocolFactory.HandleCompetedMessage(entry, callback, this);

									return true;
								}, TaskCreationOptions.AttachedToParent).WithAllExceptions().ContinueWith(task => {
									if(task.IsFaulted) {
										//an exception occured
										throw new P2pException("An exception occured while processing a message response.", P2pException.Direction.Receive, P2pException.Severity.VerySerious, task.Exception);
									}
								}, cancellationNeuralium);
							}
						} else {
							tryAttempt++;

							if(tryAttempt == 5) {
								throw new TcpApplicationException("Our sender just hanged. we received no new data that we expected.");
							}
						}
					}
				}

				if(read.IsCompleted) {
					break;
				}
			}
		}

		private void SendHandshakeVersion() {
			if(this.IsDisposed) {
				throw new TcpApplicationException("Can not use a disposed tcp connection");
			}

			IByteArray wrappedBytes = this.CreateHandshakeBytes();

			this.SendSocketBytes(wrappedBytes, false);
		}

		protected virtual IByteArray CreateHandshakeBytes() {
			return this.protocolFactory.CreateHandshake();
		}

		protected void SendMessage(MessageInstance messageInstance) {

			if(this.IsDisposed) {
				throw new TcpApplicationException("Can not use a disposed tcp connection");
			}

			IByteArray sendBytes = null;

			if(messageInstance.IsSpliMessage) {
				if(!MessageCaches.SendCaches.Exists(messageInstance.Hash)) {
					// ensure it is cached
					MessageCaches.SendCaches.AddEntry(messageInstance.SplitMessage);
				}

				if(this.protocolMessageFilters.MissesOption(TcpConnection.ProtocolMessageTypes.Split)) {
					throw new TcpApplicationException("Split messages are not allowed on this socket");
				}

				// here we send the server the header of our big message so they can start the sliced download
				sendBytes = messageInstance.SplitMessage.Dehydrate();
			} else {
				sendBytes = messageInstance.MessageBytes;
			}

			this.SendSocketBytes(sendBytes);
		}

		protected virtual void SendHandshakeBytes() {

			if((this.handshakeBytes == null) || this.handshakeBytes.IsEmpty) {
				throw new TcpApplicationException("The handshake bytes can not be null");
			}

			this.SendBytes(this.handshakeBytes);
			this.handshakeBytes = null;
		}

		protected void InvokeDataReceived(IByteArray bytes) {
			this.DataReceived?.Invoke(bytes);
		}

		protected void InvokeDisconnected() {
			DisconnectedEventArgs args = DisconnectedEventArgs.GetObject();

			//Make a copy to avoid race condition between null check and invocation
			var handler = this.Disconnected;

			handler?.Invoke(this, args);
		}

		protected bool WaitOnConnect(int timeout) {
			return this.connectWaitLock.WaitOne(timeout);
		}

		private void Dispose(bool disposing) {
			bool disposingChanged = false;

			if(this.IsDisposed || this.IsDisposing) {
				return;

			}

			try {
				lock(this.locker) {
					if(!this.IsDisposed && !this.IsDisposing) {
						this.IsDisposing = true;
						disposingChanged = true;
					}
				}

				if(!this.IsDisposed || disposingChanged) {

					this.DisposeAll(disposing);
				}
			} finally {
				this.IsDisposed = true;
				this.IsDisposing = false;
			}
		}

		protected virtual void DisposeAll(bool disposing) {

			// give it a chance to stop cleanly by cancellation
			if(disposing) {

				try {
					this.tokenSource?.Cancel();
				} catch {

				}

				try {
					this.dataReceptionTask?.Wait(5000);
				} catch {

				}

				try {
					this.tokenSource?.Dispose();
					this.tokenSource = null;
				} catch {

				}

				this.state = ConnectionState.NotConnected;

				try {
					this.DisposeSocket();

					this.SocketClosed();
				} finally {

					this.InvokeDisconnected();
				}
			}

		}

		protected virtual void DisposeSocket() {
			try {
				if(this.socket?.Connected ?? false) {
					this.socket?.Shutdown(SocketShutdown.Both);
					this.socket?.Close();
				}
			} finally {
				this.socket?.Dispose();
			}
		}

		protected enum HandshakeStatuses {
			NotStarted,
			VersionSentNoBytes,
			VersionReceivedNoBytes,
			Completed,
			Unusable
		}
	}

	public interface ITcpReadingContext {
		bool IsCanceled { get; }
		bool IsCompleted { get; }
		bool IsEmpty { get; }
		long Length { get; }

		byte this[int i] { get; }
		void DataRead(int amount);

		void CopyTo(in Span<byte> dest, int srcOffset, int destOffset, int length);
	}
}