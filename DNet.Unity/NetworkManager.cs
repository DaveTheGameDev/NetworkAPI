using System;
using DNet.ENetTransport;
using DNet.Simulation;
using UnityEngine;

namespace DNet.Unity
{
    public class NetworkManager : MonoBehaviour
    {
        [SerializeField] private NetworkBackend networkBackend;
        [SerializeField] private int tickRate = 20;

        private uint tick = 0;
        private uint ms   = 0;
        private int frame = 0;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<DispatcherRunner>();
        }


        private void Start()
        {
            switch (networkBackend)
            {
                case NetworkBackend.ENet: 
                    Debug.Log("ENet backend enabled...");
                    Network.Initialize<ENetBackend, ClientEventListener, ServerEventListener, UnityNetworkLogger>();
                    break;
                case NetworkBackend.Steam:
                    Debug.LogError("Steam integration is disabled");
                    return;
                default:                  
                    throw new ArgumentOutOfRangeException();
            }
            
            Simulation.Simulation.Initialize(NetworkObjectExt.InstantiateCallback);
        }

        private void Update()
        {
           Network.Update();

           frame++;

            if (frame % tickRate == 0)
            {
                frame = 0;
                Simulation.Simulation.Update(tick++, ms);
            }
        }

        private void OnDestroy()
        {
            Simulation.Simulation.Shutdown();
            Network.Shutdown();
        }
    }
}