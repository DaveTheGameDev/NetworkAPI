using System;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine;

namespace Installation01.Networking
{
    public class GenericPlayerNetworkObject : NetworkObject
    {
        protected override void OnInitialised(BitBuffer spawnData)
        {
            spawnData.Release();
        }
    }
}