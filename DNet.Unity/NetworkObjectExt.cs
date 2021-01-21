 using System.Runtime.Versioning;
 using DNet.Simulation;
 using UnityEngine;

 namespace DNet.Unity
{
    public static class NetworkObjectExt
    {
        public static INetworkObject InstantiateCallback(uint prefabId)
        {
            return Object.Instantiate(Resources.Load<GameObject>("TestObject")).GetComponent<NetworkObject>();
        }
    }
}