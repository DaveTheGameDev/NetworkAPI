using DNet.Simulation;
using DNet.NetStack;

namespace DNet.Unity
{
    internal struct ServerMessage
    {
        public uint ClientId;
        public BitBuffer Buffer;
    }
    public class ServerEventListener : ServerEventListenerBase
    {
        private readonly ConcurrentQueue<ServerMessage> _Queue = new ConcurrentQueue<ServerMessage>();
        
        public override void OnConnected(uint clientId)
        {
            var world = Simulation.Simulation.CreateWorld();
            world.AddClientToWorld(clientId);
            Simulation.Simulation.InstantiateNetworkObject(0, world);
        }

        public override void OnDisconnected(uint clientId, DisconnectReason reason)
        {
            
        }

        public override void OnDataReceived(uint clientId, BitBuffer buffer)
        {
            _Queue.Enqueue(new ServerMessage{ClientId = clientId, Buffer = buffer});
        }

        public override void Update()
        {
            while (!_Queue.IsEmpty)
            {
                if (!_Queue.TryDequeue(out var message))
                    continue;
                
                var messageId = message.Buffer.ReadUShort();
                var processed = ServerNetworkMessages.ProcessMessage(messageId, message.ClientId, message.Buffer);
            }
        }
        
        public override void OnNetworkShutdown()
        {
            
        }
    }
}