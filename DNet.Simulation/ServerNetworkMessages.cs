using System;
using System.Collections.Generic;
using DNet.NetStack;
using ClientValues = System.Collections.Generic.Dictionary<uint, DNet.Simulation.INetworkObject>.ValueCollection;

namespace DNet.Simulation
{
    public static class ServerNetworkMessages
    {
        
        public static void CreateWorld(uint id)
        {
            var buffer = BitBuffer.Create();
            
            buffer.AddUShort((ushort)SimMessageId.CreateWorld);
            buffer.AddUInt(id);
            
            Network.BroadcastReliable(buffer, 247);
        }

        public static void DestroyWorld(uint id)
        {
            var buffer = BitBuffer.Create();
            
            buffer.AddUShort((ushort)SimMessageId.DestroyWorld);
            buffer.AddUInt(id);
            
            Network.BroadcastReliable(buffer, 247);
        }

        public static void InstantiateNetworkObject(uint world, uint[] clientsInWorld, uint networkObjectId, uint networkObjectPrefabId)
        {
            var buffer  = BitBuffer.Create();
            
            buffer.AddUShort((ushort)SimMessageId.InstantiateObject);
            buffer.AddUInt(world);
            buffer.AddUInt(networkObjectId);
            buffer.AddUInt(networkObjectPrefabId);
            
            Network.BroadcastTargetsReliable(buffer, clientsInWorld, 247);
        }

        public static void DestroyNetworkObject(uint id, uint objectId, uint[] clientsInWorld)
        {
            var buffer = BitBuffer.Create();
            
            buffer.AddUShort((ushort)SimMessageId.DestroyObject);
            buffer.AddUInt(id);
            buffer.AddUInt(objectId);
            Network.BroadcastTargetsReliable(buffer, clientsInWorld, 247);
        }

        public static void UpdateWorld(uint tick, uint ms, ushort id, ClientValues networkObjects, uint[] clientsInWorld)
        {
            var buffer = BitBuffer.Create();
            
            buffer.AddUShort((ushort)SimMessageId.WorldUpdate);
            buffer.AddUInt(tick);
            buffer.AddUInt(ms);
            buffer.AddUShort(id);
            
            buffer.AddInt(networkObjects.Count);
            foreach (var networkObject in networkObjects)
            {
                buffer.AddUInt(networkObject.Id);
                networkObject.Serialize(buffer);
            }
            
            Network.BroadcastTargets(buffer, clientsInWorld, 248);
        }

        public static void AddClientToWorld(ushort worldId, ushort clientId)
        {
            var buffer = BitBuffer.Create();
            
            buffer.AddUShort((ushort)SimMessageId.AddClientToWorld);
            buffer.AddUShort(worldId);
            buffer.AddUShort(clientId);
            
            Network.BroadcastTargetReliable(buffer, clientId, 248);
        }
        
        public static void RemoveClientToWorld(ushort worldId, ushort clientId)
        {
            var buffer = BitBuffer.Create();
            
            buffer.AddUShort((ushort)SimMessageId.RemoveClientFromWorld);
            buffer.AddUShort(worldId);
            buffer.AddUShort(clientId);
            
            Network.BroadcastTargetReliable(buffer, clientId, 248);
        }

        public static bool ProcessMessage(in ushort messageId, in uint clientId, BitBuffer buffer)
        {
            switch ((SimMessageId) messageId)
            {
                case SimMessageId.InstantiateObject: 
                    break;
                case SimMessageId.DestroyObject:    
                    break;
                case SimMessageId.CreateWorld:       
                    break;
                case SimMessageId.DestroyWorld:      
                    break;
                case SimMessageId.WorldUpdate:       
                    break;
                case SimMessageId.AddClientToWorld:
                    break;
                case SimMessageId.RemoveClientFromWorld:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(messageId), messageId, null);
            }

            return false;
        }
    }
}