using System;
using System.Collections.Generic;
using Installation01.Networking.NetStack.Serialization;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Installation01.Networking
{
    public abstract class NetworkObject : MonoBehaviour
    {
        internal static readonly Dictionary<uint, NetworkObject> NetworkObjects = new Dictionary<uint, NetworkObject>();
        [RuntimeInitializeOnLoadMethod] private static void Init() => NetworkObjects.Clear();
        
        public INetSerialize SpawnData { get; private set; } = null;


        private INetSerialize extraData = null;
        private float smoothTime = 0.1f;


        [ShowNativeProperty] private int NetworkId => (int)Id;
        [ShowNativeProperty] private int NetworkControllerId => (int)ControllerId;
        

        public uint Id { get; private set; }
        public uint ControllerId { get; private set; } = uint.MaxValue;

        [ShowNativeProperty]
        public bool InControl => ControllerId == Network.NetworkId;
        [ShowNativeProperty]
        public AuthorityLevel AuthorityLevel { get; private set; }

        [ShowNativeProperty]
        public bool Initialised { get; private set; }
        public INetSerialize ExtraData => extraData;
        
        internal AsyncOperationHandle<GameObject> handle;
        
        /// <summary>
        /// Set the data required for this object when spawned over the network
        /// </summary>
        /// <param name="data"></param>
        public void SetSpawnData(INetSerialize data)
        {
            SpawnData = data;
        }
        
        /// <summary>
        /// Set the extra data this object needs to send when state sync occurs
        /// </summary>
        /// <param name="data"></param>
        public void SetExtraData(INetSerialize data)
        {
            extraData = data;
        }
        
        /// <summary>
        ///  Used to take control of an object.
        /// Only the server can call this method.
        /// </summary>
        public void TakeControl()
        {
            if(Network.NetworkType != NetworkType.Server)
                return;

            ControllerId = Network.NetworkId;
            
            var buffer = BitBuffer.Create();
            buffer.AddUShort((ushort) BuiltInMessage.ControlChanged);
            buffer.AddUInt(Id);
            buffer.AddUInt(Network.NetworkId);
            Network.Broadcast(buffer, 0, SendMode.Reliable, default, false);
        }
        
        public void AssignControl(uint id)
        {
            if(Network.NetworkType != NetworkType.Server)
                return;

            ControllerId = id;
            
            var buffer = BitBuffer.Create();
            buffer.AddUShort((ushort) BuiltInMessage.ControlChanged);
            buffer.AddUInt(Id);
            buffer.AddUInt(id);
            Network.Broadcast(buffer, 0, SendMode.Reliable, default, false);
        }


        public void Destroy()
        {
            OnDestroy();
            NetworkObjectManager.Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
        }

        internal void Initialise(uint id, uint controllerId, AuthorityLevel authLevel, BitBuffer spawnData, AsyncOperationHandle<GameObject> handle)
        {
            
            NetworkDispatcher.Run(() =>
            {
                if(Initialised)
                    return;

                this.ControllerId = controllerId;
                this.Id = id;
                AuthorityLevel = authLevel;
                
                NetworkObjects.Add(id, this);
                OnNetworkStart();
                Initialised = true;
                OnInitialised(spawnData);
                this.handle = handle;
            });
        }

        private void OnNetworkStart()
        {
            var rb = GetComponent<Rigidbody>();

            //Disable Rigidbody physics here to only apply physics from server version of player.
            if (rb)
                rb.isKinematic = Network.NetworkType == NetworkType.Server;
                        
            // if (AuthorityLevel == AuthorityLevel.Server && Network.NetworkType == NetworkType.Server)
            // {
            //     if(!Initialised)
            //         Initialise(NetworkObjectManager.NextId, Network.NetworkId, AuthorityLevel, null);
            // }
        }

        protected virtual void FixedUpdate()
        {
            if(!Network.IsRunning || !Initialised)
                return;

            if (AuthorityLevel == AuthorityLevel.Server)
            {
                if(Network.NetworkType != NetworkType.Server)
                    return;
            }
            else
            {
                if(!InControl)
                    return;
            }
            
            var t = transform;
            
            INetSerialize state = new NetworkObjectData
            {
                HasTransformState = true,
                position = t.position,
                rotation = t.localEulerAngles,
                NetId = Id,
                extraData = ExtraData
            };
            
            Network.Broadcast(ref state);
        }

        [RegisterClientMessage(BuiltInMessage.ControlChanged)]
        private static void ControlChanged(BitBuffer buffer)
        {
            //uint id = buffer.ReadUInt();
            //NetworkObjects[id].Id = buffer.ReadUInt();
        }
        
        [RegisterServerMessage(BuiltInMessage.State)]
        private static void StateUpdateMessageHandler(Peer peer, BitBuffer buffer)
        {
            NetworkDispatcher.Run(() =>
            {
                NetworkObjectData networkObjectData = default;
                networkObjectData.Deserialize(buffer);
               
                
                if (NetworkObjects.ContainsKey(networkObjectData.NetId))
                {
                    var networkObject = NetworkObjects[networkObjectData.NetId];
                    networkObject.StateUpdated(networkObjectData);
                    networkObject.OnStateUpdated(buffer);
                   
                    
                    networkObject.SerialiseStateData(ref networkObjectData);
                    INetSerialize state = networkObjectData;
  
                    // Send back the state to clients.
                    Network.BroadcastExceptTarget(ref state, peer);
                }
            });
        }

        protected virtual void SerialiseStateData(ref NetworkObjectData networkObjectData)
        {
            
        }
        
        [RegisterClientMessage(BuiltInMessage.State)]
        private static void StateUpdateMessageHandler(BitBuffer buffer)
        {
            NetworkObjectData networkObjectData = default;
            networkObjectData.Deserialize(buffer);

            NetworkDispatcher.Run(() =>
            {
                if (NetworkObjects.ContainsKey(networkObjectData.NetId))
                {
                    var networkObject = NetworkObjects[networkObjectData.NetId];
                    networkObject.StateUpdated(networkObjectData);
                    networkObject.OnStateUpdated(buffer);
                }
            });
        }

        private void StateUpdated(NetworkObjectData stateData)
        {
            if (stateData.HasTransformState && !InControl)
            {
                Transform t;
                (t = transform).position = Vector3.Lerp(transform.position, stateData.position, smoothTime);
                transform.rotation = Quaternion.Euler(Vector3.Lerp(t.localEulerAngles, stateData.rotation, smoothTime));
            }
        }

        protected virtual void OnStateUpdated(BitBuffer stateData)
        {
            stateData?.Release();
        }
        
        protected abstract void OnInitialised(BitBuffer spawnData);
    }
}