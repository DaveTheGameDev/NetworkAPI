using DNet.NetStack;
using ENet;


namespace DNet.ENetTransport
{
    public class ENetBackend : INetBackend
    {
        public const byte ConnectionChannel = 254;

        ClientEventListenerBase INetBackend.ClientEventListenerBase { get; set; }
        ServerEventListenerBase INetBackend.ServerEventListenerBase { get; set; }

        private NetworkServer server;
        private NetworkClient client;

        private static readonly byte[] ByteBuffer = new byte[1024];

        public void Initialize()
        {
            Library.Initialize();
        }

        public void Shutdown()
        {
            client?.Shutdown();
            server?.Shutdown();
            Library.Deinitialize();
        }

        public void StartServer(ushort port, int maxConnections, IConnectionToken connectionToken)
        {
            server = new NetworkServer(port, maxConnections, connectionToken, ((INetBackend) this).ServerEventListenerBase);
            server.Start();
        }

        public void ConnectToServer(string ip, ushort port, IConnectionToken connectionToken)
        {
            client = new NetworkClient(((INetBackend) this).ClientEventListenerBase);
            client.Connect(ip, port, connectionToken);
        }

        public void DisconnectClient(uint id, DisconnectReason reason)
        {
            server.GetClient(id).peer.DisconnectNow((uint) reason);
        }

        #region BitBuffer Broadcast

        /// <summary>
        /// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public void Broadcast(BitBuffer networkMessage, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.None, default, false);

        /// <summary>
        /// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public void BroadcastReliable(BitBuffer networkMessage, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.Reliable, default, false);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTarget(BitBuffer networkMessage, uint target, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.None, target, false);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTargets(BitBuffer networkMessage, uint[] targets, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.None, targets);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTargetsReliable(BitBuffer networkMessage, uint[] targets, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.None, targets);

        /// <summary>
        /// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public void BroadcastExceptTarget(BitBuffer networkMessage, uint ignoredTarget, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.None, ignoredTarget, true);

        /// <summary>
        /// Server specific function to send a reliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTargetReliable(BitBuffer networkMessage, uint target, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.Reliable, target, false);

        /// <summary>
        /// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public void BroadcastExceptTargetReliable(BitBuffer networkMessage, uint ignoredTarget, byte channel = 0) =>
            Broadcast(networkMessage, channel, PacketFlags.Reliable, ignoredTarget, true);

        /// <summary>
        /// Generic method to handle sending data to connections.
        /// </summary>
        private void Broadcast(BitBuffer buffer, byte channel, PacketFlags packetFlags, uint target, bool excludeTarget)
        {
            Packet packet = default;
            CreatePacket(buffer, packetFlags, ref packet);
            FinalBroadcast(ref packet, channel, target, excludeTarget);
        }

        /// <summary>
        /// Generic method to handle sending data to connections.
        /// </summary>
        private void Broadcast(BitBuffer buffer, byte channel, PacketFlags packetFlags, uint[] target)
        {
            Packet packet = default;
            CreatePacket(buffer, packetFlags, ref packet);
            FinalBroadcastTargets(ref packet, channel, target);
        }

        #endregion

        #region byte[] Broadcast

        /// <summary>
        /// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public void Broadcast(byte[] networkMessage, int len, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.None, default, false);

        /// <summary>
        /// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public void BroadcastReliable(byte[] networkMessage, int len, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.Reliable, default, false);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTarget(byte[] networkMessage, int len, uint target, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.None, target, false);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTargets(byte[] networkMessage, int len, uint[] targets, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.None, targets);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTargetsReliable(byte[] networkMessage, int len, uint[] targets, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.None, targets);

        /// <summary>
        /// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public void BroadcastExceptTarget(byte[] networkMessage, int len, uint ignoredTarget, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.None, ignoredTarget, true);

        /// <summary>
        /// Server specific function to send a reliable packet to a ignoredTarget connection.
        /// </summary>
        public void BroadcastTargetReliable(byte[] networkMessage, int len, uint target, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.Reliable, target, false);

        /// <summary>
        /// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public void BroadcastExceptTargetReliable(byte[] networkMessage, int len, uint ignoredTarget, byte channel = 0) =>
            Broadcast(networkMessage, len, channel, PacketFlags.Reliable, ignoredTarget, true);

        /// <summary>
        /// Generic method to handle sending data to connections.
        /// </summary>
        private void Broadcast(byte[] buffer, int len, byte channel, PacketFlags packetFlags, uint target, bool excludeTarget)
        {
            Packet packet = default;
            CreatePacket(buffer, len, packetFlags, ref packet);
            BufferPool.Release(buffer);
            FinalBroadcast(ref packet, channel, target, excludeTarget);
        }

        /// <summary>
        /// Generic method to handle sending data to connections.
        /// </summary>
        private void Broadcast(byte[] buffer, int len, byte channel, PacketFlags packetFlags, uint[] target)
        {
            Packet packet = default;
            CreatePacket(buffer, len, packetFlags, ref packet);
            FinalBroadcastTargets(ref packet, channel, target);
        }

        #endregion

        private void FinalBroadcastTargets(ref Packet packet, byte channel, uint[] targets)
        {
            if (server == null || !server.isRunning)
                return;

            var clients = new Peer[targets.Length];

            for (var i = 0; i < clients.Length; i++)
                clients[i] = server.GetClient(targets[i]).peer;

            server.host.Broadcast(channel, ref packet, clients);
        }

        private void FinalBroadcast(ref Packet packet, byte channel, uint target, bool excludeTarget)
        {
            if (server != null && server.isRunning)
            {
                var peer = server.GetClient(target).peer;
                if (peer.IsSet)
                {
                    if (!excludeTarget) // Send message from server to target.
                        peer.Send(channel, ref packet);
                    else // Send message from server to all connected clients except target.
                        server.host.Broadcast(channel, ref packet, peer);
                }
                else // Send message from server to all connected clients.
                {
                    server.host.Broadcast(channel, ref packet);
                }
            }
            else if (client != null && client.isRunning) // Send message to server from client.
            {
                client.peer.Send(channel, ref packet);
            }
        }

        private void CreatePacket(BitBuffer buffer, PacketFlags packetFlags, ref Packet packet)
        {
            buffer.ToArray(ByteBuffer);
            packet.Create(ByteBuffer, buffer.Length, packetFlags);
            buffer.Release();
        }

        private void CreatePacket(byte[] buffer, int len, PacketFlags packetFlags, ref Packet packet)
        {
            packet.Create(buffer, len, packetFlags);
        }
    }
}