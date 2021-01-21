using DNet.NetStack;

namespace DNet
{
    public abstract class ClientEventListenerBase
    {
         public abstract void OnConnected();
         public abstract void OnDisconnected(DisconnectReason reason);
         public abstract void OnDataReceived(BitBuffer buffer);
         public abstract void Update();
         public abstract void OnNetworkShutdown();
        
    }
}