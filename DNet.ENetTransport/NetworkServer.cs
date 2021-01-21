using System;
using System.Collections.Generic;
using System.Threading;
using DNet.NetStack;
using ENet;

namespace DNet.ENetTransport
{
    public class NetworkServer
    {
        private readonly Dictionary<uint, ClientData> connections = new Dictionary<uint, ClientData>();

        private readonly ushort port;
        private readonly int maxConnections;
        
        private readonly ServerEventListenerBase networkCallbacks;
        private readonly IConnectionToken connectionToken;

        public bool isRunning;
        internal Host host;

        public NetworkServer(ushort port, int maxConnections, IConnectionToken connectionToken, ServerEventListenerBase networkCallbacks)
        {
            this.port = port;
            this.maxConnections = maxConnections;
            this.connectionToken = connectionToken;
            this.networkCallbacks = networkCallbacks;
        }

        public ClientData GetClient(uint id)
        {
            return connections[id];
        }

        public void Start()
        {
            host = new Host();
            var address = new Address {Port = port};

            host.Create(address, maxConnections, byte.MaxValue);
            Network.Logger.LogMessage($"Started server listening on port {port}\n");

            isRunning = true;

            new Thread(() =>
            {
                while (isRunning)
                    Poll();

                if (host.IsSet)
                    host.Flush();
            }).Start();
        }

        private void Poll()
        {
            var polled = false;

            while (!polled)
            {
                if (!host.IsSet)
                    return;
                
                if (host.CheckEvents(out var netEvent) <= 0)
                {
                    if (host.Service(15, out netEvent) <= 0)
                        break;
                    polled = true;
                }

                switch (netEvent.Type)
                {
                    case EventType.Connect:
                        OnConnectEvent(netEvent);
                        break;
                    case EventType.Disconnect:
                        OnDisconnectEvent(netEvent);
                        break;
                    case EventType.Timeout:
                        OnTimeoutEvent(netEvent);
                        break;
                    case EventType.Receive:
                        OnReceiveEvent(netEvent);
                        break;
                }
            }
        }

        private unsafe void OnReceiveEvent(Event netEvent)
        {
            var bitBuffer = BitBufferExt.CreateFromEvent(netEvent);
            var buffer = new ReadOnlySpan<byte>((byte*) netEvent.Packet.Data, netEvent.Packet.Length);
            bitBuffer.FromSpan(ref buffer, netEvent.Packet.Length);
            netEvent.Packet.Dispose();

            // Handle initial connection attempt 
            if (netEvent.ChannelID == ENetBackend.ConnectionChannel)
            {
                ValidateConnection(bitBuffer, netEvent);
            }
            else
            {
                networkCallbacks.OnDataReceived(netEvent.Peer.ID, bitBuffer);
            }
        }

        private void OnTimeoutEvent(Event netEvent)
        {
            Network.Logger.LogMessage($"Client timeout - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}\n");
            connections.Remove(netEvent.Peer.ID);
            networkCallbacks.OnDisconnected(netEvent.Peer.ID, DisconnectReason.TimeOut);
        }

        private void OnDisconnectEvent(Event netEvent)
        {
            Network.Logger.LogMessage($"Client disconnected - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}\n");
            connections.Remove(netEvent.Peer.ID);
            networkCallbacks.OnDisconnected(netEvent.Peer.ID, (DisconnectReason) netEvent.Data);
        }

        private static void OnConnectEvent(Event netEvent)
        {
            Network.Logger.LogMessage($"Client connected - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}.\n ");
            Network.Logger.LogMessage($"Waiting for connection approval \n");

            //Validate version
            if (netEvent.Data != Network.Version)
                netEvent.Peer.DisconnectNow((uint) DisconnectReason.InvalidVersion);
        }

        private void ValidateConnection(BitBuffer bitBuffer, Event netEvent)
        {
            if (!connectionToken.Validate(bitBuffer))
            {
                Network.Logger.LogMessage(
                    $"Client connection rejected (kicking) - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}\n");
                netEvent.Peer.DisconnectNow((uint) DisconnectReason.InvalidConnectionToken);
                bitBuffer.Release();
            }
            else
            {
                var clientData = new ClientData {peer = netEvent.Peer, connectionData = bitBuffer};
                connections.Add(netEvent.Peer.ID, clientData);

                Network.Logger.LogMessage($"Client connection approved - ID: {netEvent.Peer.ID}, IP: {netEvent.Peer.IP}\n");
                networkCallbacks.OnConnected(netEvent.Peer.ID);
            }
        }

        public void Shutdown()
        {
            // Disconnect all connections before full shutdown
            foreach (var connection in connections)
                connection.Value.peer.DisconnectNow((uint) DisconnectReason.Disconnected);
            
            host.Flush();
            host.Dispose();

            isRunning = false;
            networkCallbacks.OnNetworkShutdown();
        }
    }
}