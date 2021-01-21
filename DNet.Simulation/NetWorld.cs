using System.Collections.Generic;
using DNet.NetStack;

namespace DNet.Simulation
{
    public class NetWorld
    {
        private readonly Dictionary<uint, INetworkObject> networkObjects = new Dictionary<uint, INetworkObject>();
        private readonly List<uint> clientsInWorld = new List<uint>();

        private uint currentTick;
        private uint currentMs;
        
        public NetWorld(ushort id)
        {
            Id = id;
        }

        public ushort Id { get; }


        internal uint[] GetClients() => clientsInWorld.ToArray();

        
        public void AddClientToWorld(uint clientId)
        {
            if (clientsInWorld.Contains(clientId)) 
                return;
            
            clientsInWorld.Add(clientId);
        }
        
        public void RemoveClientFromWorld(uint clientId)
        {
            if (!clientsInWorld.Contains(clientId)) 
                return;
            
            clientsInWorld.Remove(clientId);
        }
        
        public void AddNetObjectToWorld(INetworkObject networkObject)
        {
            if (networkObjects.ContainsKey(networkObject.Id)) 
                return;
            
            networkObjects.Add(networkObject.Id, networkObject);
        }
        
        public void RemoveNetObjectFromWorld(INetworkObject networkObject)
        {
            if (!networkObjects.ContainsKey(networkObject.Id)) 
                return;
            
            networkObjects.Remove(networkObject.Id);
        }

        public void Update(uint tick, uint ms, BitBuffer buffer)
        {
            // If this is a late packet or 
            if(currentTick >= tick)
                return;
            
            currentTick = tick;
            currentMs   = ms;
            
            var objectCount = buffer.ReadInt();

            for (uint i = 0; i < objectCount; i++)
            {
                var netObjectId = buffer.ReadUInt();
                
                if (networkObjects.TryGetValue(netObjectId, out var networkObject))
                {
                    networkObject.Deserialize(buffer);
                    networkObject.NetworkUpdate(tick, ms);
                    continue;
                }
                
                Network.Logger.LogError("NETWORK: FATAL ERROR - Failed to find object in world (NetWorld.Update)");
            }
        }
        
        public void Update(uint tick, uint ms)
        {
            currentTick = tick;
            currentMs   = ms;
            ServerNetworkMessages.UpdateWorld(tick, ms, Id, networkObjects.Values, clientsInWorld.ToArray());
        }

        public void Destroy()
        {
            foreach (var networkObject in networkObjects.Values)
            {
                networkObject.Destroy();
            }
        }

        public bool TryDestroyObject(uint objectId)
        {
            if (networkObjects.TryGetValue(objectId, out var networkObject))
            {
                networkObject.Destroy();
                networkObjects.Remove(objectId);
                return true;
            }

            return false;
        }
    }
}