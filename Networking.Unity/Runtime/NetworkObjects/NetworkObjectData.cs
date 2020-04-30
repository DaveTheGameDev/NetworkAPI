using Installation01.Networking.NetStack.Compression.Extension;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine;

namespace Installation01.Networking
{
    public struct NetworkObjectData : INetSerialize
    {
        private const ushort Id = (ushort) BuiltInMessage.State;

        public bool HasTransformState;
        public uint NetId;
        public Vector3 position;
        public Vector3 rotation;
        public INetSerialize extraData;
        
        public void Serialize(BitBuffer buffer)
        {
            buffer.AddUShort(Id);
            buffer.AddUInt(NetId);

            buffer.AddBool(HasTransformState);
            
            if(HasTransformState)
            {
                buffer.AddVector3(position);
                buffer.AddVector3(rotation);
            }
            extraData?.Serialize(buffer);
        }

        public void Deserialize(BitBuffer buffer)
        {
            NetId = buffer.ReadUInt();
            HasTransformState = buffer.ReadBool();
            
            if(HasTransformState)
            {
                position = buffer.ReadVector3();
                rotation = buffer.ReadVector3();
            }
        }
    }
}