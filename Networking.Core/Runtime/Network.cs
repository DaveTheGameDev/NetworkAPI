using System;
using System.Linq;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine.Assertions;

namespace Installation01.Networking
{
	/// <summary>
	/// Used to start and connect to servers + sending data.
	/// </summary>
	public static class Network
	{
	
		/// <summary>
		/// Will be true when the network has been initialised.
		/// This means it will be true even if connection state is connecting or disconnected.
		/// </summary>
		public static bool IsRunning => NetworkInstance?.IsRunning ?? false;
		
		/// <summary>
		/// Id of our network client (uint max value assigned for server).
		/// </summary>
		public static uint NetworkId => NetworkInstance?.Id ?? uint.MaxValue;

		/// <summary>
		/// Is this a server or a client.
		/// </summary>
		public static NetworkType NetworkType => NetworkInstance?.NetworkType ?? NetworkType.None;
		
		/// <summary>
		/// Returns current state of our connection.
		/// </summary>
		public static PeerState ConnectionState => NetworkInstance?.ConnectionState ?? PeerState.Uninitialized;
		
		/// <summary>
		/// Network instance handles all the communication layer functionality.
		/// </summary>
		private static readonly NetworkInstance NetworkInstance = new NetworkInstance();

		private static readonly byte[] DataBuffer = new byte[1024];
		
		/// <summary>
		/// Starts a server on a given port.
		/// </summary>
		public static void StartServer(IServerMessageHandler messageHandler, ushort port, int maxConnections)
		{
			Assert.IsFalse(IsRunning, "Network is already running, aborting.");
			if(IsRunning)
				return;
			
			NetworkInstance.Initialize(messageHandler);
			NetworkInstance.Start("0.0.0.0", port, maxConnections);
		}

		/// <summary>
		/// Connect to a server on a given ip and port.
		/// </summary>
		public static void Connect(IClientMessageHandler messageHandler, string ip, ushort port)
		{
			Assert.IsFalse(IsRunning, "Network is already running, aborting.");
			if(IsRunning)
				return;
			
			NetworkInstance.Initialize(messageHandler);
			NetworkInstance.Start(ip, port);
		}

		/// <summary>
		/// Returns an array of client data for all the clients currently connected to the server.
		/// </summary>
		/// <returns></returns>
		public static ClientData[] GetConnections()
		{
			Assert.IsTrue(IsRunning, "Network is not running, aborting.");
			return NetworkInstance.Connections.Values.ToArray();
		}

		/// <summary>
		/// Disconnects all connections and cleans up the network.
		/// </summary>
		public static void Shutdown(bool clearEvents)
		{
			if(!IsRunning)
				return;
			
			NetworkInstance.Shutdown(clearEvents);
			NetworkLogger.Log("Network shutting down.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="peer"></param>
		/// <param name="value"></param>
		public static void SetClientLevelLoaded(Peer peer, bool value)
		{
			var clientData = NetworkInstance.Connections[peer.ID];
			clientData.levelLoaded = value;
			
			NetworkInstance.Connections[peer.ID] = clientData;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public static void SetAllClientLevelLoaded(bool value)
		{
			foreach (var connection in GetConnections())
			{
				var clientData = connection;
				clientData.levelLoaded = value;
				NetworkInstance.Connections[connection.Id] = clientData;
			}
		}
		
		/// <summary>
		/// Will be true when all connected clients have loaded their local level successfully.
		/// </summary>
		/// <returns></returns>
		public static bool CheckLoadStatus()
		{
			return GetConnections().All(connection => connection.levelLoaded);
		}
		
		/// <summary>
		/// Kicks a connection from network.
		/// </summary>
		/// <param name="id">Network id of the connection</param>
		public static void KickConnection(uint id)
		{
			Assert.IsTrue(IsRunning, "Network is not running, aborting.");
			Assert.IsTrue(NetworkInstance.Connections.ContainsKey(id), $"Cannot kick with ID {id} as they do not exist");
			
			if (NetworkInstance.Connections.ContainsKey(id))
			{
				NetworkInstance.Connections[id].Peer.DisconnectNow((uint) DisconnectReason.Kick);
			}
		}

		#region Broadcast INetSerialize
		
		/// <summary>
		/// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
		/// </summary>
		public static void Broadcast(ref INetSerialize networkMessage, byte channel = 0)
		{
			Broadcast(ref networkMessage, channel, SendMode.None, default, false);
		}
			
		/// <summary>
		/// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
		/// </summary>
		public static void BroadcastReliable(ref INetSerialize networkMessage, byte channel = 0)
		{
			Broadcast(ref networkMessage, channel, SendMode.Reliable, default, false);
		}
		
		/// <summary>
		/// Server specific function to send an unreliable packet to a ignoredTarget connection.
		/// </summary>
		public static void BroadcastTarget(ref INetSerialize networkMessage, Peer target, byte channel = 0)
		{
			Broadcast(ref networkMessage, channel, SendMode.None, target, false);
		}
		
		/// <summary>
		/// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
		/// </summary>
		public static void BroadcastExceptTarget(ref INetSerialize networkMessage, Peer ignoredTarget, byte channel = 0)
		{
			Broadcast(ref networkMessage, channel, SendMode.None, ignoredTarget, true);
		}

		/// <summary>
		/// Server specific function to send a reliable packet to a ignoredTarget connection.
		/// </summary>
		public static void BroadcastTargetReliable(ref INetSerialize networkMessage, Peer target, byte channel = 0)
		{
			Broadcast(ref networkMessage, channel, SendMode.Reliable, target, false);
		}
		
		/// <summary>
		/// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
		/// </summary>
		public static void BroadcastExceptTargetReliable(ref INetSerialize networkMessage, Peer ignoredTarget, byte channel = 0)
		{
			Broadcast(ref networkMessage, channel, SendMode.Reliable, ignoredTarget, true);
		}

		/// <summary>
		/// Generic method to handle sending data to connections.
		/// </summary>
		public static void Broadcast(ref INetSerialize networkMessage, byte channel, SendMode sendMode, Peer target, bool excludeTarget)
		{
			Assert.IsTrue(IsRunning, "Network is not running, aborting.");
			
			Packet packet = default;
			CreatePacket(networkMessage, sendMode, ref packet);
			FinalBroadcast(ref packet, channel, sendMode, target, excludeTarget);
		}
		#endregion

		#region Broadcast BitBuffer
		
				/// <summary>
		/// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
		/// </summary>
		public static void Broadcast(BitBuffer networkMessage, byte channel = 0)
		{
			Broadcast(networkMessage, channel, SendMode.None, default, false);
		}
			
		/// <summary>
		/// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
		/// </summary>
		public static void BroadcastReliable(BitBuffer networkMessage, byte channel = 0)
		{
			Broadcast(networkMessage, channel, SendMode.Reliable, default, false);
		}
		
		/// <summary>
		/// Server specific function to send an unreliable packet to a ignoredTarget connection.
		/// </summary>
		public static void BroadcastTarget(BitBuffer networkMessage, Peer target, byte channel = 0)
		{
			Broadcast(networkMessage, channel, SendMode.None, target, false);
		}
		
		/// <summary>
		/// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
		/// </summary>
		public static void BroadcastExceptTarget(BitBuffer networkMessage, Peer ignoredTarget, byte channel = 0)
		{
			Broadcast(networkMessage, channel, SendMode.None, ignoredTarget, true);
		}

		/// <summary>
		/// Server specific function to send a reliable packet to a ignoredTarget connection.
		/// </summary>
		public static void BroadcastTargetReliable(BitBuffer networkMessage, Peer target, byte channel = 0)
		{
			Broadcast(networkMessage, channel, SendMode.Reliable, target, false);
		}
		
		/// <summary>
		/// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
		/// </summary>
		public static void BroadcastExceptTargetReliable(BitBuffer networkMessage, Peer ignoredTarget, byte channel = 0)
		{
			Broadcast(networkMessage, channel, SendMode.Reliable, ignoredTarget, true);
		}

		/// <summary>
		/// Generic method to handle sending data to connections.
		/// </summary>
		public static void Broadcast(BitBuffer buffer, byte channel, SendMode sendMode, Peer target, bool excludeTarget)
		{
			Assert.IsTrue(IsRunning, "Network is not running, aborting.");
			
			Packet packet = default;
			CreatePacket(buffer, sendMode, ref packet);
			FinalBroadcast(ref packet, channel, sendMode, target, excludeTarget);
		}
		
		#endregion
		
		
		private static void FinalBroadcast(ref Packet packet, byte channel, SendMode sendMode, Peer target, bool excludeTarget)
		{
			if (NetworkType == NetworkType.Server)
			{
				if(target.IsSet)
				{
					if (!excludeTarget)
					{
						// Send message from server to ignoredTarget.
						target.Send(channel, ref packet);
						return;
					}
					// Send message from server to all connected clients except ignoredTarget.
					NetworkInstance.Host.Broadcast(channel, ref packet, target);
				}
				else
				{
					// Send message from server to all connected clients.
					NetworkInstance.Host.Broadcast(channel, ref packet);
				}
			}
			else if (NetworkType == NetworkType.Client)
			{
				// Send message to server from client.
				NetworkInstance.Peer.Send(channel, ref packet);
			}
		}

		//TODO: create this packet without any allocation
		private static void CreatePacket(INetSerialize networkMessage, SendMode sendMode, ref Packet packet)
		{
			BitBuffer buffer = BitBuffer.Create();
			networkMessage.Serialize(buffer);
			buffer.ToArray(DataBuffer);
			buffer.Release();
			packet.Create(DataBuffer, buffer.Length, sendMode);
		}
		
		
		//TODO: create this packet without any allocation
		private static void CreatePacket(BitBuffer buffer, SendMode sendMode, ref Packet packet)
		{
			buffer.ToArray(DataBuffer);
			buffer.Release();
			packet.Create(DataBuffer, buffer.Length, sendMode);
		}

		/// <summary>
		/// Get current network stats. Useful to debug the network.
		/// </summary>
		/// <returns></returns>
		public static NetworkStats GetStats()
		{
			return NetworkInstance?.networkStats ?? default;
		}
		
		#region Events

		//CLIENT
		
		/// <summary>
		/// Add event listener for when connection to a server occurs.
		/// </summary>
		public static void AddConnectedEvent(Action action)
		{
			NetworkInstance.ConnectedToServer += action;
		}

		/// <summary>
		/// Remove event listener for when connection to a server occurs.
		/// </summary>
		public static void RemoveConnectedEvent(Action action)
		{
			NetworkInstance.ConnectedToServer -= action;
		}

		/// <summary>
		/// Add event listener for when disconnection from a server occurs.
		/// </summary>
		public static void AddDisconnectedEvent(Action<DisconnectReason> action)
		{
			NetworkInstance.DisconnectedFromServer += action;
		}

		/// <summary>
		/// Remove event listener for when disconnection from a server occurs.
		/// </summary>
		public static void RemoveDisconnectedEvent(Action<DisconnectReason> action)
		{
			NetworkInstance.DisconnectedFromServer -= action;
		}
		
		// SERVER
		
		/// <summary>
		/// Add event listener for when a client connects to our server.
		/// </summary>
		public static void AddClientConnectedEvent(Action<ClientData> action)
		{
			NetworkInstance.ClientConnected += action;
		}

		/// <summary>
		/// Remove event listener for when a client connects to our server.
		/// </summary>
		public static void RemoveClientConnectedEvent(Action<ClientData> action)
		{
			NetworkInstance.ClientConnected -= action;
		}

		/// <summary>
		/// Add event listener for when a client disconnects from our server.
		/// </summary>
		public static void AddClientDisconnectedEvent(Action<ClientData> action)
		{
			NetworkInstance.ClientDisconnected += action;
		}

		/// <summary>
		/// Remove event listener for when a client disconnects from our server.
		/// </summary>
		public static void RemoveClientDisconnectedEvent(Action<ClientData> action)
		{
			NetworkInstance.ClientDisconnected -= action;
		}

		
		/// <summary>
		/// Add event listener for when a client disconnects from our server.
		/// </summary>
		public static void AddNetworkStartedEvent(Action action)
		{
			NetworkInstance.NetworkStarted += action;
		}

		/// <summary>
		/// Remove event listener for when a client disconnects from our server.
		/// </summary>
		public static void RemoveNetworkStartedEvent(Action action)
		{
			NetworkInstance.NetworkStarted -= action;
		}
		#endregion

		public static void AddShutdownEvent(Action action)
		{
			NetworkInstance.ShutdownEvent += action;
		}
		
		public static void RemoveShutdownEvent(Action action)
		{
			NetworkInstance.ShutdownEvent -= action;
		}
	}
}