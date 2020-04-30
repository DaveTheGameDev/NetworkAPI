using System;
using System.Collections.Generic;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Installation01.Networking
{
    public static class NetworkObjectManager
    {
        internal static uint NextId = 0;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialise()
        {
            NetworkObject.NetworkObjects.Clear();
            Network.AddClientConnectedEvent(ClientConnected);
            Network.AddDisconnectedEvent(OnDisconnected);
            Network.AddShutdownEvent(OnShutdown);
        }

        private static void OnShutdown()
        {
            CleanupObjects();
        }
        
        private static void OnDisconnected(DisconnectReason disconnectReason)
        {
            CleanupObjects();
        }

        public static void CleanupObjects()
        {
            foreach (var networkObject in NetworkObject.NetworkObjects)
            {
                if(networkObject.Value)
                    Object.Destroy(networkObject.Value);
            }

            NetworkObject.NetworkObjects.Clear();
            NextId = 0;
        }

        private static void ClientConnected(ClientData clientData)
        {
            foreach (var networkObject in NetworkObject.NetworkObjects.Values)
            {
                var transform = networkObject.transform;
                
                INetSerialize message = new SpawnMessage
                {
                    PrefabName = networkObject.name,
                    Controller = networkObject.ControllerId,
                    NetId = networkObject.Id,
                    AuthorityLevel = networkObject.AuthorityLevel,
                    pos = transform.position,
                    rot = transform.localEulerAngles,
                    SpawnData = networkObject.SpawnData
                };

                Network.BroadcastTargetReliable(ref message, clientData.Peer);
            }
        }

        public static void Instantiate(string prefabName, Vector3 pos, Vector3 rot, uint controller, AuthorityLevel authorityLevel, INetSerialize spawnData)
        {
            if(Network.NetworkType != NetworkType.Server || !Network.IsRunning)
                return;
            
            INetSerialize message = new SpawnMessage
            {
                PrefabName = prefabName,
                Controller = controller,
                NetId = ++NextId,
                AuthorityLevel = authorityLevel,
                pos = pos,
                rot = rot,
                SpawnData = spawnData
            };
            
            Network.BroadcastReliable(ref message);
            
            // This is so the server can use the same code as the client.
            var bitBuffer = BitBuffer.Create();
            spawnData?.Serialize(bitBuffer);
            
            InstantiateObject((SpawnMessage)message, bitBuffer);
        }

        public static void Destroy(GameObject gameObject)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();
            
            if (!networkObject)
            {
                NetworkLogger.LogError($"Trying to destroy a game object that is not a NetworkObject ({gameObject})");
                return;
            }
            
            FinalDestroy(networkObject.Id);
            
            BitBuffer buffer = BitBuffer.Create();
            buffer.AddUShort((ushort) BuiltInMessage.Destroy);
            buffer.AddUInt(networkObject.Id);
        }

        private static void FinalDestroy(uint id)
        {
            if (!NetworkObject.NetworkObjects.ContainsKey(id))
            {
                NetworkLogger.LogError($"Trying to destroy a game object that is not a NetworkObject (ID: {id})");
                return;
            }
            
            var target = NetworkObject.NetworkObjects[id];
            NetworkObject.NetworkObjects.Remove(target.Id);
            
            Object.Destroy(target);
            
            if(target.handle.IsValid())
                Addressables.ReleaseInstance(target.handle);
        }

        [RegisterClientMessage(BuiltInMessage.Instantiate)]
        private static void Instantiate(BitBuffer buffer)
        {
            SpawnMessage message = default;
            message.Deserialize(buffer);
            InstantiateObject(message, buffer);
        }
 
        [RegisterClientMessage(BuiltInMessage.Destroy)]
        private static void Destroy(BitBuffer buffer)
        {
            FinalDestroy(buffer.ReadUInt());
        }
        
        private static void InstantiateObject(SpawnMessage message, BitBuffer spawnData)
        {
            NetworkDispatcher.Run(() =>
            {
                Addressables.InstantiateAsync(message.PrefabName, message.pos, Quaternion.Euler(message.rot)).Completed += handle =>
                {
                    if (!handle.Result)
                        throw new Exception("Invalid prefab name");

                    handle.Result.name = handle.Result.name;
                    var networkObject = handle.Result.GetComponent<NetworkObject>();

                    if (networkObject)
                    {
                        networkObject.SetSpawnData(message.SpawnData);
                        networkObject.Initialise(message.NetId, message.Controller, message.AuthorityLevel, spawnData, handle);
                    }
                    else
                    {
                        throw new Exception("Prefab does not have a network object component");
                    }
                };
            });
        }
    }
}