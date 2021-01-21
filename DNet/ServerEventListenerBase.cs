using DNet.NetStack;

namespace DNet
{
    public abstract class ServerEventListenerBase
    {
        public abstract void OnConnected(uint clientId);
        public abstract void OnDisconnected(uint clientId, DisconnectReason reason);
        public abstract void OnDataReceived(uint clientId, BitBuffer buffer);

        public abstract void Update();
        public abstract void OnNetworkShutdown();
    }
}