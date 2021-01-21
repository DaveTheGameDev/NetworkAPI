using System;
using DNet.Simulation;
using DNet.Unity.Quantization;
using DNet.NetStack;
using UnityEngine;

namespace DNet.Unity
{
    public class NetworkObject : MonoBehaviour, INetworkObject
    {
        public uint Id { get; set; }
        public NetWorld World { get; set; }

        private readonly BoundedRange[] ranges = {
            new BoundedRange(-5000, 5000, 0.8f),
            new BoundedRange(-5000, 5000, 0.8f),
            new BoundedRange(-5000, 5000, 0.8f),
        };

        private State lastState;
        
        // 0 = latest
        private State[] states = new State[30];

        public void NetworkUpdate(in uint tick, in uint ms)
        {
            for (var i = 1; i < states.Length; i++)
            {
                states[i - 1] = states[i];
            }

            states[0] = lastState;
        }

        private void Update()
        {
            var t      = transform;
            t.position = lastState.Position;
            t.rotation = lastState.Rotation;
        }

        public void Serialize(BitBuffer buffer)
        {
            var pos = BoundedRange.Quantize(transform.position, ranges);
            var rot = SmallestThree.Quantize(transform.rotation);

            buffer.AddUInt(pos.x);
            buffer.AddUInt(pos.y);
            buffer.AddUInt(pos.z);
            
            buffer.AddUInt(rot.a);
            buffer.AddUInt(rot.b);
            buffer.AddUInt(rot.c);
            buffer.AddUInt(rot.m);
        }

        public void Deserialize(BitBuffer buffer)
        {
            var qPos = new QuantizedVector3(buffer.ReadUInt(), buffer.ReadUInt(), buffer.ReadUInt());
            var qRot = new QuantizedQuaternion(buffer.ReadUInt(), buffer.ReadUInt(), buffer.ReadUInt(), buffer.ReadUInt());

            var pos = BoundedRange.Dequantize(qPos, ranges);
            var rot = SmallestThree.Dequantize(qRot);

            lastState = new State
            {
                Position = pos,
                Rotation = rot
            };
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
        
        public struct State
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }
    }
}