using System;
using System.Collections.Generic;
using System.Threading;
using Installation01.Networking.NetStack.Serialization;

namespace Installation01.Networking
{
	internal class NetworkInstance
	{
		internal readonly Dictionary<uint, ClientData> Connections = new Dictionary<uint, ClientData>();
		public bool IsRunning;
		public Host Host;
		public Peer Peer;
		
		public NetworkType NetworkType;
		public PeerState ConnectionState => Peer.IsSet ? Peer.State : PeerState.Uninitialized;

		public uint Id => Peer.IsSet ? Peer.ID : uint.MaxValue;

		private IClientMessageHandler clientMessageHandler;
		private IServerMessageHandler serverMessageHandler;
		internal NetworkStats networkStats;

		public void Initialize(IClientMessageHandler messageHandler)
		{
			clientMessageHandler = messageHandler;
			NetworkType = NetworkType.Client;
		}
		
		public void Initialize(IServerMessageHandler messageHandler)
		{
			serverMessageHandler = messageHandler;
			NetworkType = NetworkType.Server;
		}
		
		public void Start(string ip = "0.0.0.0", ushort port = 4000, int maxConnections = 32)
		{
			Host = new Host();

			Address address = new Address {Port = port};

			if (NetworkType == NetworkType.Server)
			{
				Host.Create(address, maxConnections);
				NetworkLogger.Log($"Started server listening on port {port}");
				NetworkStarted?.Invoke();
			}
			else if (NetworkType == NetworkType.Client)
			{
				address.SetHost(ip);
				Host.Create();
				Peer = Host.Connect(address);
				NetworkLogger.Log($"Connecting to {ip}:{port}");
			}

			Host.SetChannelLimit(255);
			
			IsRunning = true;

			Thread thread = new Thread(() => {
				while (IsRunning)
				{
					Poll();
				}
				if(Host.IsSet)
					Host.Flush();
			});
			thread.Start();
		}

		private unsafe void Poll()
		{
			if (!Host.IsSet)
			{
				return;
			}

			networkStats = new NetworkStats
			{
				BytesReceived = Host.BytesReceived,
				BytesSent = Host.BytesSent,
				PacketsReceived = Host.PacketsReceived,
				PacketsSent = Host.BytesSent,
				PeersCount = Host.PeersCount
			};
	
			bool polled = false;

			while (!polled) {
				if (Host.CheckEvents(out NetworkEvent netEvent) <= 0) {
					if (Host.Service(15, out netEvent) <= 0)
						break;

					polled = true;
				}

				switch (netEvent.Type) {
					case EventType.Connect:
					{
						if(NetworkType == NetworkType.Server)
						{
							ClientData clientData = new ClientData
							{
								Peer = netEvent.Peer
							};
								
							Connections.Add(netEvent.Peer.ID, clientData);
							
							NetworkDispatcher.Run(() =>
							{
								NetworkLogger.Log("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
								//Invoke client connection event.
								ClientConnected?.Invoke(clientData);
							});
							
						}
						else if (NetworkType == NetworkType.Client)
						{
							NetworkDispatcher.Run(() =>
							{
								// Invoke connection to server event
								ConnectedToServer?.Invoke();
							});
						}
						break;
					}
					case EventType.Disconnect:
					{
						if (NetworkType == NetworkType.Server)
						{
							NetworkDispatcher.Run(() =>
							{
								NetworkLogger.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
								ClientDisconnected?.Invoke(Connections[netEvent.Peer.ID]);
								Connections.Remove(netEvent.Peer.ID);
							});
						}
						else if (NetworkType == NetworkType.Client)
						{
							NetworkDispatcher.Run(() =>
							{
								NetworkLogger.Log($"Disconnected from server (reason: {(DisconnectReason)netEvent.Data}");
								// Invoke connection to server event
								DisconnectedFromServer?.Invoke((DisconnectReason)netEvent.Data);
							});
						}
						break;
					}

					case EventType.Timeout:
					{
						if (NetworkType == NetworkType.Server)
						{
							
							NetworkDispatcher.Run(() =>
							{
								NetworkLogger.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
								ClientDisconnected?.Invoke(Connections[netEvent.Peer.ID]);
								Connections.Remove(netEvent.Peer.ID);
							});
						}
						else if (NetworkType == NetworkType.Client)
						{
							NetworkDispatcher.Run(() =>
							{
								NetworkLogger.Log("Timed out from server");
								// Invoke connection to server event
								DisconnectedFromServer?.Invoke(DisconnectReason.Timeout);
							});
						}
						break;
					}

					case EventType.Receive:
					{
						BitBuffer bitBuffer = BitBuffer.Create(netEvent);

						if (NetworkType == NetworkType.Client)
						{
							clientMessageHandler.MessageReceived(bitBuffer);
						}
						else
						{
							serverMessageHandler.MessageReceived(netEvent.Peer, bitBuffer);
						}
	
						netEvent.Packet.Dispose();
						break;
					}
				}
			}
		}
		
		public void Shutdown(bool clearEvents)
		{
			if(NetworkType == NetworkType.Client)
				Peer.DisconnectNow((uint) DisconnectReason.Disconnect);

			// Disconnect all connections before full shutdown
			foreach (var connection in Connections)
			{
				connection.Value.Peer.DisconnectNow((uint)DisconnectReason.Disconnect);
			}
			
			Host.Flush();
			Host.Dispose();
			
			IsRunning = false;

			ShutdownEvent?.Invoke();
			
			if (clearEvents)
			{
				NetworkStarted = null;
				ConnectedToServer = null;
				DisconnectedFromServer = null;
				ClientConnected = null;
				ClientDisconnected = null;
				ShutdownEvent = null;
			}
		}

		public event Action NetworkStarted;
		public event Action ConnectedToServer;
		public event Action<DisconnectReason> DisconnectedFromServer;
		public event Action<ClientData> ClientConnected;
		public event Action<ClientData> ClientDisconnected;
		public event Action ShutdownEvent;
	}

	public enum NetworkType
	{
		Server,
		Client,
		None
	}
}