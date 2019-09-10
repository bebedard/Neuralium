﻿using System;
using System.Net.Sockets;
using System.Threading;
using Neuralia.Blockchains.Core.Network;
using Neuralia.Blockchains.Core.Network.Exceptions;
using Neuralia.Blockchains.Core.P2p.Connections;
using Neuralia.Blockchains.Core.P2p.Messages.MessageSets;
using Neuralia.Blockchains.Core.Services;
using Neuralia.Blockchains.Tools.Data;
using Serilog;

namespace Neuralia.Blockchains.Core.Tools {
	/// <summary>
	///     class used to simplify sending messages and bytes over a socket
	/// </summary>
	public class DataDispatcher {

		private readonly object locker = new object();
		private readonly ITimeService timeService;

		public DataDispatcher(ITimeService timeService) {
			this.timeService = timeService;
		}

		private IByteArray DehydrateMessage(INetworkMessageSet message) {
			IByteArray bytes = message.Dehydrate();

			return bytes;
		}

		public bool SendMessage(PeerConnection peerConnection, INetworkMessageSet message) {
			message.BaseHeader.SentTime = this.timeService.CurrentRealTime;

			return this.SendBytes(peerConnection, this.DehydrateMessage(message));
		}

		public bool SendFinalMessage(PeerConnection peerConnection, INetworkMessageSet message) {
			message.BaseHeader.SentTime = this.timeService.CurrentRealTime;

			return this.SendFinalBytes(peerConnection, this.DehydrateMessage(message));
		}

		public bool SendBytes(PeerConnection peerConnection, IByteArray data) {
			return this.Send(() => this.SendBytesToPeer(peerConnection, data));
		}

		public bool SendFinalBytes(PeerConnection peerConnection, IByteArray data) {
			bool SendAction() {
				try {
					return this.SendBytesToPeer(peerConnection, data);
				} catch(Exception e) {
					Log.Verbose("error occured", e);

					return false;
				} finally {
					peerConnection.Dispose();
				}
			}

			return this.Send(SendAction);
		}

		private bool SendBytesToPeer(PeerConnection peerConnection, IByteArray data) {

			lock(this.locker) {

				ConnectionState state = peerConnection.connection.State;

				try {
					if(peerConnection.IsDisposed) {
						return false;
					}

					if(state == ConnectionState.NotConnected) {
						return this.ConnectAndSendBytesToPeer(peerConnection, data);
					}

					if(state != ConnectionState.Connected) {
						if(state == ConnectionState.Connecting) {
							//wait a little bit then retry
							int attempt = 1;
							int sleepTime = 100;

							do {
								Thread.Sleep(sleepTime);

								state = peerConnection.connection.State;

								if(state != ConnectionState.Connected) {
									sleepTime = 500;
									attempt++;
								}
							} while(attempt <= 3);
						}

						if(state == ConnectionState.Disconnecting) {
							throw new TcpApplicationException("Connection is in the process of disconnecting.");
						}

						if(state != ConnectionState.Connected) {
							throw new TcpApplicationException("Failed to connect to the peer.");
						}
					}

					peerConnection.connection.SendBytes(data);
				} catch(Exception ex) {
					Log.Verbose(ex, "Failed to send bytes to peer.");

					return false;
				}
			}

			return true;
		}

		private bool ConnectAndSendBytesToPeer(PeerConnection peerConnection, IByteArray data) {

			if(peerConnection.IsDisposed) {
				return false;
			}

			ConnectionState state = peerConnection.connection.State;

			lock(this.locker) {
				try {
					if(state != ConnectionState.NotConnected) {

						if(state == ConnectionState.Connecting) {
							//wait a little bit then retry
							int attempt = 1;
							int sleepTime = 100;

							do {
								Thread.Sleep(sleepTime);

								state = peerConnection.connection.State;

								if(state != ConnectionState.Connected) {
									sleepTime = 500;
									attempt++;
								}
							} while(attempt <= 3);
						}

						if(state == ConnectionState.Connected) {
							return this.SendBytesToPeer(peerConnection, data);
						}

						if(state == ConnectionState.Disconnecting) {
							//wait a little bit then retry
							int attempt = 1;
							int sleepTime = 100;

							do {
								Thread.Sleep(sleepTime);

								state = peerConnection.connection.State;

								if(state != ConnectionState.NotConnected) {
									sleepTime = 500;
									attempt++;
								}
							} while(attempt <= 3);
						}

						if(state != ConnectionState.NotConnected) {
							throw new TcpApplicationException("Connection is already connected. could not connect.");
						}
					}

					try {
						peerConnection.connection.Connect(data);
					} catch(Exception ex) {
						throw new TcpApplicationException("Failed to connect to client. They are not available", ex);
					}
				} catch(TcpApplicationException tcpEx) {

					if(tcpEx.InnerException is P2pException p2pEx) {
						if(p2pEx.InnerException is SocketException socketException) {

						} else {
							Log.Verbose(tcpEx, "Failed to send bytes to peer.");
						}
					} else {
						Log.Verbose(tcpEx, "Failed to send bytes to peer.");
					}

					return false;
				} catch(P2pException p2pEx) {
					if(p2pEx.InnerException is SocketException socketException) {
						if((socketException.SocketErrorCode == SocketError.TimedOut) || (socketException.SocketErrorCode == SocketError.Shutdown)) {
							// thats it, we time out, let's report it simply
							Log.Verbose("Failed to send bytes to peer. socket connection was unavailable");
						} else {
							Log.Verbose(p2pEx, "Failed to send bytes to peer.");
						}
					} else {
						Log.Verbose(p2pEx, "Failed to send bytes to peer.");
					}

					return false;
				} catch(Exception ex) {
					Log.Verbose(ex, "Failed to send bytes to peer.");

					return false;
				}
			}

			return true;
		}

		/// <summary>
		///     Attempt some connection event x amount of times, checking for cancel every time
		/// </summary>
		/// <param name="action"></param>
		/// <param name="trycount"></param>
		/// <returns></returns>
		private bool Send(Func<bool> action, int tryCount = 3) {
			int counter = 0;
			Exception lastException = null;

			try {
				do {
					try {
						return action();
					} catch(Exception ex) {
						// lets make sure we check before every loop so we dont waste time. every connection can idle for 5 seconds. thats long at shutdown

						counter++;
						lastException = ex;
						Thread.Sleep(100);
					}

					if(counter == tryCount) {
						throw new TcpApplicationException("Failed to connect to client. They are not available", lastException);
					}
				} while(true);
			} catch(Exception ex) {
				Log.Verbose(ex, "Failed to send bytes to peer.");
			}

			return false;
		}
	}
}