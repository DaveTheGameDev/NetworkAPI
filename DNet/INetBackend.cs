using DNet.NetStack;

namespace DNet
{
    /// <summary>
    /// This interface passes incoming network events from a transport to the main networking layer
    /// </summary>
    public interface INetBackend
    {
        ClientEventListenerBase ClientEventListenerBase { get; set; }
        ServerEventListenerBase ServerEventListenerBase { get; set; }

        void Initialize();
        void Shutdown();
        
        void StartServer(ushort port, int maxConnections, IConnectionToken connectionToken);
        void ConnectToServer(string ip, ushort port, IConnectionToken connectionToken);

        void DisconnectClient(uint id, DisconnectReason reason);

        /// <summary>
        /// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        void Broadcast(BitBuffer networkMessage, byte channel = 0);

        /// <summary>
        /// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        void BroadcastReliable(BitBuffer networkMessage, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTarget(BitBuffer networkMessage, uint target, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTargets(BitBuffer networkMessage, uint[] targets, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTargetsReliable(BitBuffer networkMessage, uint[] targets, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        void BroadcastExceptTarget(BitBuffer networkMessage, uint ignoredTarget, byte channel = 0);

        /// <summary>
        /// Server specific function to send a reliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTargetReliable(BitBuffer networkMessage, uint target, byte channel = 0);

        /// <summary>
        /// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        void BroadcastExceptTargetReliable(BitBuffer networkMessage, uint ignoredTarget, byte channel = 0);

        /// <summary>
        /// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        void Broadcast(byte[] networkMessage, int len, byte channel = 0);

        /// <summary>
        /// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        void BroadcastReliable(byte[] networkMessage, int len, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTarget(byte[] networkMessage, int len, uint target, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTargets(byte[] networkMessage, int len, uint[] targets, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTargetsReliable(byte[] networkMessage, int len, uint[] targets, byte channel = 0);

        /// <summary>
        /// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        void BroadcastExceptTarget(byte[] networkMessage, int len, uint ignoredTarget, byte channel = 0);

        /// <summary>
        /// Server specific function to send a reliable packet to a ignoredTarget connection.
        /// </summary>
        void BroadcastTargetReliable(byte[] networkMessage, int len, uint targetId, byte channel = 0);

        /// <summary>
        /// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        void BroadcastExceptTargetReliable(byte[] networkMessage, int len, uint ignoredTargetId, byte channel = 0);

    }
}