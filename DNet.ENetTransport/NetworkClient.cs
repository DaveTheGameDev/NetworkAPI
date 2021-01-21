using System;
using System.Threading;
using DNet.NetStack;
using ENet;

namespace DNet.ENetTransport
{
    public class NetworkClient
    {
        public bool isRunning;
        internal Host host;
        internal Peer peer;
        private IConnectionToken connectionToken;
        private ClientEventListenerBase networkCallbacks;

        public NetworkClient(ClientEventListenerBase  callbacks)
        {
            networkCallbacks = callbacks;
        }
        
        public void Connect(string ip, ushort port, IConnectionToken connectionToken)
        {
            this.connectionToken = connectionToken;
            
            host = new Host();
            var address = new Address();
            address.SetHost(ip);
            address.Port = port;

            host.Create();
            peer = host.Connect(address, byte.MaxValue, Network.Version);
            Network.Logger.LogMessage($"Connecting to {ip}:{port}\n");

            host.SetChannelLimit(byte.MaxValue);
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
            if (!host.IsSet)
                return;

            var polled = false;

            while (!polled)
            {
                if (host.CheckEvents(out var netEvent) <= 0)
                {
                    if (host.Service(15, out netEvent) <= 0)
                        break;
                    polled = true;
                }

                switch (netEvent.Type)
                {
                    case EventType.Connect:
                        OnConnectEvent();
                        break;
                    case EventType.Disconnect:
                        OnDisconnectEvent(netEvent);
                        break;
                    case EventType.Timeout:
                        OnTimeoutEvent();
                        break;
                    case EventType.Receive:
                        OnReceiveEvent(netEvent);
                        break;
                }
            }
        }

        private unsafe void OnReceiveEvent(Event netEvent)
        {
            var bitBuffer = BitBuffer.Create();
            var buffer = new ReadOnlySpan<byte>((byte*) netEvent.Packet.Data, netEvent.Packet.Length);
            bitBuffer.FromSpan(ref buffer, netEvent.Packet.Length);
            netEvent.Packet.Dispose();
            networkCallbacks.OnDataReceived(bitBuffer);
        }

        private void OnTimeoutEvent()
        {
            Network.Logger.LogMessage("Timed out from server");
            networkCallbacks.OnDisconnected(DisconnectReason.TimeOut);
        }

        private void OnDisconnectEvent(Event netEvent)
        {
            Network.Logger.LogMessage($"Disconnected from server (reason: {(DisconnectReason) netEvent.Data}");
            networkCallbacks.OnDisconnected((DisconnectReason) netEvent.Data);
        }

        private void OnConnectEvent()
        {
            Network.Logger.LogMessage($"Connected to server. Sending connection data.\n");
            
            // Send connection data.
            var bitBuffer = BitBuffer.Create();
            connectionToken.Write(bitBuffer);
            Network.BroadcastReliable(bitBuffer, ENetBackend.ConnectionChannel);
            networkCallbacks.OnConnected();
        }

        public void Shutdown()
        {
            host.Flush();
            peer.DisconnectNow((uint) DisconnectReason.Disconnected);
            host.Dispose();

            isRunning = false;
            networkCallbacks.OnNetworkShutdown();
        }
    }
}