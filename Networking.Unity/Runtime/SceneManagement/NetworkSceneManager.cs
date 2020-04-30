using System;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Installation01.Networking
{
    public static class NetworkSceneManager
    {
        /// <summary>
        /// THe name of the current home scene. (Usually the main menu)
        /// </summary>
        public static string HomeScene { get; private set; }
        
        /// <summary>
        /// Name of the current scene.
        /// </summary>
        public static string CurrentScene { get; private set; }

        /// <summary>
        /// Invoked when all clients and server has loaded their levels or the local client has loaded their level/
        /// </summary>
        public static event Action<string> SceneLoaded;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            HomeScene = SceneManager.GetActiveScene().name;
            CurrentScene = HomeScene;
            Network.AddClientConnectedEvent(ClientConnected);    
        }

        private static void ClientConnected(ClientData data)
        {
            if(CurrentScene.Equals(HomeScene))
                return;
            
            INetSerialize message = new SceneLoadMessage {SceneName = CurrentScene};
            Network.BroadcastTargetReliable(ref message, data.Peer);
        }

        /// <summary>
        /// Set the current home scene by name.
        /// </summary>
        /// <param name="newHomeSceneName"></param>
        public static void OverrideHomeScene(string newHomeSceneName)
        {
            HomeScene = newHomeSceneName;
        } 

        /// <summary>
        /// Load a scene over the network.
        /// </summary>
        /// <param name="newScene"></param>
        /// <exception cref="Exception">If not a server exception will be thrown telling the client they cannot load levels this way.</exception>
        public static void LoadScene(string newScene)
        {
            if(Network.NetworkType != NetworkType.Server)
                throw new Exception("Only server can load scenes.");
            
            CurrentScene = newScene;
            Network.SetAllClientLevelLoaded(false);
            SceneManager.LoadSceneAsync(newScene).completed += ServerLocalSceneLoaded;
            
            INetSerialize message = new SceneLoadMessage { SceneName = newScene };
            Network.BroadcastReliable(ref message);
        }

        /// <summary>
        /// Return to the home scene.
        /// </summary>
        public static void ReturnHome()
        {
            LoadScene(HomeScene);
        }
        
        private static void ServerLocalSceneLoaded(AsyncOperation obj)
        {
            if (Network.CheckLoadStatus())
                SceneLoaded?.Invoke(CurrentScene);
        }
        
        
        [RegisterServerMessage(BuiltInMessage.SceneLoad)]
        private static void RemoteSceneLoaded(Peer peer, BitBuffer buffer)
        {
            Network.SetClientLevelLoaded(peer, true);
            
            if (Network.CheckLoadStatus())
                SceneLoaded?.Invoke(CurrentScene);
        }
        
        
        [RegisterClientMessage(BuiltInMessage.SceneLoad)]
        private static void SceneLoad(BitBuffer buffer)
        {
            var newScene = buffer.ReadString();
            
            // Something bad must have happened for this message to be received and current map is the same as the new map
            if(CurrentScene.Equals(newScene))
                return;
            
            CurrentScene = newScene;
            SceneManager.LoadSceneAsync(newScene).completed += ClientLocalSceneLoaded;
        }

        /// <summary>
        /// Invoked when the local client (not server) has loaded their level.
        /// </summary>
        private static void ClientLocalSceneLoaded(AsyncOperation obj)
        {
            // Let server know we loaded the scene successfully.
            var buffer = BitBuffer.Create();
            buffer.AddUShort((ushort) BuiltInMessage.SceneLoad);
            Network.Broadcast(buffer, 0, SendMode.Reliable, default, false);
            
            SceneLoaded?.Invoke(CurrentScene);
        }
    }
}