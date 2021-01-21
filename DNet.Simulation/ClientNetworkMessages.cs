using System;
using DNet.NetStack;

namespace DNet.Simulation
{
    public static class ClientNetworkMessages
    {

        public static bool ProcessMessage(ushort messageId, BitBuffer buffer)
        {
            switch ((SimMessageId)messageId)
            {
                case SimMessageId.InstantiateObject:     
                    InstantiateNetworkObject(buffer);
                    return true;
                case SimMessageId.DestroyObject:    
                    DestroyNetworkObject(buffer);
                    return true;
                case SimMessageId.CreateWorld:      
                    CreateWorld(buffer);
                    return true;
                case SimMessageId.DestroyWorld:        
                    DestroyWorld(buffer);
                    return true;
                case SimMessageId.WorldUpdate:        
                    UpdateWorld(buffer);
                    return true;
                case SimMessageId.AddClientToWorld:
                    AddClientToWorld(buffer);
                    return true;
                case SimMessageId.RemoveClientFromWorld:
                    RemoveClientFromWorld(buffer);
                    return true;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static void CreateWorld(BitBuffer buffer)
        {
            var id = buffer.ReadUShort();
            Simulation.CreateWorld(id);
        }

        private static void DestroyWorld(BitBuffer buffer)
        {
            var id = buffer.ReadUShort();
            
            if (Simulation.TryDestroyWorld(id)) 
                return;
            
            Network.Logger.LogError("NETWORK: FATAL ERROR - Failed to find world (DestroyWorld)");
        }

        private static void InstantiateNetworkObject(BitBuffer buffer)
        {
            var worldId  = buffer.ReadUShort();
            var id       = buffer.ReadUInt();
            var prefabId = buffer.ReadUInt();
            var netObj   = Simulation.EngineInstantiate(prefabId);

            netObj.Id = id;

            if (Simulation.TryGetWorld(worldId, out var world))
            {
                netObj.World = world;
                world.AddNetObjectToWorld(netObj);
                return;
            }

            Network.Logger.LogError("NETWORK: FATAL ERROR - Failed to find world for network object (Instantiate)");
        }

        private static void DestroyNetworkObject(BitBuffer buffer)
        {
            var worldId = buffer.ReadUShort();
            var id      = buffer.ReadUShort();

            if (Simulation.TryDestroyNetworkObject(worldId, id))
                return;
            
            Network.Logger.LogError("NETWORK: FATAL ERROR - Failed to find object in world (Destroy)");
        }

        private static void UpdateWorld(BitBuffer buffer)
        {
            var tick    = buffer.ReadUInt();
            var ms      = buffer.ReadUInt();
            var worldId = buffer.ReadUShort();

            if (Simulation.TryGetWorld(worldId, out var world))
            {
                world.Update(tick, ms, buffer);
                return;
            }

            Network.Logger.LogError($"NETWORK: FATAL ERROR - Failed to find world ({worldId}). (UpdateWorld)");
        }

        private static void AddClientToWorld(BitBuffer buffer)
        {
            var worldId  = buffer.ReadUShort();
            var clientId = buffer.ReadUShort();

            if (Simulation.TryGetWorld(worldId, out var world))
            {
                world.AddClientToWorld(clientId);
            }
        }
        
        private static void RemoveClientFromWorld(BitBuffer buffer)
        {
            var worldId  = buffer.ReadUShort();
            var clientId = buffer.ReadUShort();

            if (Simulation.TryGetWorld(worldId, out var world))
            {
                world.RemoveClientFromWorld(clientId);
                return;
            }

            Network.Logger.LogError($"NETWORK: FATAL ERROR - Failed to find world ({worldId}). (UpdateWorld)");
        }
    }
}