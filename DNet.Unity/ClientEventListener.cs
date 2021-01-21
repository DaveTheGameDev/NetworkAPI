using DNet.Simulation;
using DNet.NetStack;

namespace DNet.Unity
{
    public class ClientEventListener : ClientEventListenerBase
    {
        private readonly ConcurrentQueue<BitBuffer> Queue = new ConcurrentQueue<BitBuffer>();
        
        public override void OnConnected()
        {
            
        }

        public override void OnDisconnected(DisconnectReason reason)
        {
            
        }

        public override void OnDataReceived(BitBuffer buffer)
        {
            Queue.Enqueue(buffer);
        }

        public override void Update()
        {
            while (!Queue.IsEmpty)
            {
                if (!Queue.TryDequeue(out var message))
                    continue;
                
                var messageId = message.ReadUShort();
                var processed = ClientNetworkMessages.ProcessMessage(messageId, message);
            }
        }

        public override void OnNetworkShutdown()
        {
            
        }
    }
}