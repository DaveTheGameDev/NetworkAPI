using System;
using UnityEngine;

namespace DNet.Unity
{
    public class DispatcherRunner : MonoBehaviour
    {
        private void Update()
        {
            UnityNetworkLogger.RunQueue();
        }
    }
}