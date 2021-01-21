using DNet.NetStack;

namespace DNet
{
    public static class Network
    {
        private const uint Major = 1;
        private const uint Minor = 1;
        private const uint Patch = 0;

        public const uint Version = (Major << 16) | (Minor << 8) | Patch;

        public static INetworkLogger Logger { get; private set; }
        public static bool ServerRunning    { get; private set; }
        public static bool ClientRunning    { get; private set; }

        private static INetBackend netBackend;

        /// <summary>
        /// Initialize network with backend and logger.
        /// </summary>
        /// <typeparam name="T">Backend Type</typeparam>
        /// <typeparam name="T0">Client Listener</typeparam>
        /// <typeparam name="T1">Server Listener</typeparam>
        /// <typeparam name="T2">Logger</typeparam>
        /// <returns></returns>
        public static void Initialize<T, T0, T1, T2>() 
            where T : INetBackend, new()
            where T0 : ClientEventListenerBase, new()
            where T1 : ServerEventListenerBase, new()
            where T2 : INetworkLogger, new()
        {
            var bEnd = new T
            {
                ClientEventListenerBase = new T0(), 
                ServerEventListenerBase = new T1()
            };
            
            Logger = new  T2();
            netBackend = bEnd;
            netBackend.Initialize();
        }
 
        public static void Update()
        {
            if(ServerRunning)
                netBackend.ServerEventListenerBase.Update();
            
            if(ClientRunning)
                netBackend.ClientEventListenerBase.Update();
        }

        public static void Shutdown()
        {
            netBackend.Shutdown();
            ServerRunning = false;
            ClientRunning = false;
        }

        public static void DisconnectClient(uint id, DisconnectReason reason)
        {
            netBackend.DisconnectClient(id, reason);
        }

        public static void StartServer(ushort port, int maxConnections, IConnectionToken token)
        {
            netBackend.StartServer(port, maxConnections, token);
            ServerRunning = true;
        }

        public static void ConnectToServer(string ip, ushort port, IConnectionToken token)
        {
            netBackend.ConnectToServer(ip, port, token);
            ClientRunning = true;
        }


        /// <summary>
        /// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public static void Broadcast(BitBuffer networkMessage, byte channel = 0) => 
            netBackend.Broadcast(networkMessage, channel);

        /// <summary>
        /// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public static void BroadcastReliable(BitBuffer networkMessage, byte channel = 0) => 
            netBackend.BroadcastReliable(networkMessage, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTarget(BitBuffer networkMessage, uint target, byte channel = 0) => 
            netBackend.BroadcastTarget(networkMessage, target, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTargets(BitBuffer networkMessage, uint[] targets, byte channel = 0) => 
            netBackend.BroadcastTargets(networkMessage, targets, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTargetsReliable(BitBuffer networkMessage, uint[] targets, byte channel = 0) => 
            netBackend.BroadcastTargetsReliable(networkMessage, targets, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public static void BroadcastExceptTarget(BitBuffer networkMessage, uint ignoredTarget, byte channel = 0) => 
            netBackend.BroadcastExceptTarget(networkMessage, ignoredTarget, channel);

        /// <summary>
        /// Server specific function to send a reliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTargetReliable(BitBuffer networkMessage, uint target, byte channel = 0) => 
            netBackend.BroadcastTargetReliable(networkMessage, target, channel);

        /// <summary>
        /// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public static void BroadcastExceptTargetReliable(BitBuffer networkMessage, uint ignoredTarget, byte channel = 0) => 
            netBackend.BroadcastExceptTargetReliable(networkMessage, ignoredTarget, channel);

        /// <summary>
        /// Send an unreliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public static void Broadcast(byte[] networkMessage, int len, byte channel = 0) => 
            netBackend.Broadcast(networkMessage, len, channel);

        /// <summary>
        /// Send a reliable packet to server if network type is running as a client and to all connections if running as a server.
        /// </summary>
        public static void BroadcastReliable(byte[] networkMessage, int len, byte channel = 0) => 
            netBackend.BroadcastReliable(networkMessage, len, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTarget(byte[] networkMessage, int len, uint target, byte channel = 0) => 
            netBackend.BroadcastTarget(networkMessage, len, target, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTargets(byte[] networkMessage, int len, uint[] targets, byte channel = 0) => 
            netBackend.BroadcastTargets(networkMessage, len, targets, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTargetsReliable(byte[] networkMessage, int len, uint[] targets, byte channel = 0) => 
            netBackend.BroadcastTargetsReliable(networkMessage, len, targets, channel);

        /// <summary>
        /// Server specific function to send an unreliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public static void BroadcastExceptTarget(byte[] networkMessage, int len, uint ignoredTarget, byte channel = 0) => 
            netBackend.BroadcastExceptTarget(networkMessage, len, ignoredTarget, channel);

        /// <summary>
        /// Server specific function to send a reliable packet to a ignoredTarget connection.
        /// </summary>
        public static void BroadcastTargetReliable(byte[] networkMessage, int len, uint target, byte channel = 0) => 
            netBackend.BroadcastTargetReliable(networkMessage, len, target, channel);

        /// <summary>
        /// Server specific function to send a reliable packet to all connected clients except a ignoredTarget connection.
        /// </summary>
        public static void BroadcastExceptTargetReliable(byte[] networkMessage, int len, uint ignoredTarget, byte channel = 0) => 
            netBackend.BroadcastExceptTargetReliable(networkMessage, len, ignoredTarget, channel);
    }
}