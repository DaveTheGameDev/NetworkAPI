using System;
using System.Collections.Generic;
using System.Linq;

namespace DNet.Simulation
{
    public static class Simulation
    {
        private static readonly Dictionary<uint, NetWorld> NetWorlds = new Dictionary<uint, NetWorld>();
        
        private static ushort _nextId;
        private static ushort _nextObjectId;

        public delegate INetworkObject InstantiateCallback(uint prefabId);
        public static event InstantiateCallback instantiateCallback;

        /// <summary>
        /// Runs the callback for instantiation of network objects on the engine level
        /// </summary>
        /// <param name="prefabId"></param>
        public static INetworkObject EngineInstantiate(uint prefabId) =>
            instantiateCallback?.Invoke(prefabId);

        public static void Update(uint tick, uint ms)
        {
            if(!Network.ServerRunning)
                return;
            
            foreach (var world in NetWorlds.Values)
            {
                world.Update(tick, ms);
            }
        }

        public static void Initialize(InstantiateCallback instantiateCallback)
        {
            NetWorlds.Clear();
            _nextId              = 0;
            _nextObjectId        = 0;
            Simulation.instantiateCallback = instantiateCallback;
        }

        public static NetWorld CreateWorld()
        {
            var world = new NetWorld(_nextId++);
            NetWorlds.Add(world.Id, world);
            ServerNetworkMessages.CreateWorld(world.Id);
            return world;
        }

        public static NetWorld CreateWorld(ushort id)
        {
            var world = new NetWorld(id);
            NetWorlds.Add(world.Id, world);
            return world;
        }

        public static bool TryGetWorld(ushort id, out NetWorld world)
        {
            return NetWorlds.TryGetValue(id, out world);
        }

        public static IEnumerable<NetWorld> GetAllWorlds()
        {
            return NetWorlds.Values.ToArray();
        }

        public static bool TryDestroyWorld(ushort id)
        {
            if (!TryGetWorld(id, out var world))
                return false;

            world.Destroy();
            NetWorlds.Remove(id);

            if (Network.ServerRunning)
                ServerNetworkMessages.DestroyWorld(id);

            return true;
        }

        public static INetworkObject InstantiateNetworkObject(uint prefabId, NetWorld world)
        {
            if (!Network.ServerRunning)
                throw new Exception("Server not running");
                    
            var networkObject = EngineInstantiate(prefabId);
            
            networkObject.Id    = _nextObjectId++;
            networkObject.World = world;

            world.AddNetObjectToWorld(networkObject);
            
            ServerNetworkMessages.InstantiateNetworkObject(world.Id, world.GetClients(), networkObject.Id, prefabId);
            
            return networkObject;
        }

        public static bool TryDestroyNetworkObject(ushort worldId, uint id)
        {
            var success = TryGetWorld(worldId, out var world) && world.TryDestroyObject(id);

            if (Network.ServerRunning)
                ServerNetworkMessages.DestroyNetworkObject(worldId, id, world.GetClients());

            return success;
        }

        public static void Shutdown()
        {
            foreach (var netWorld in GetAllWorlds()) 
                netWorld.Destroy();
            
            instantiateCallback = null;
        }
    }
}