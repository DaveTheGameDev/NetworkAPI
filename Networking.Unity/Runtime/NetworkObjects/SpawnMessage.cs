using Installation01.Networking.NetStack.Compression.Extension;
using Installation01.Networking.NetStack.Serialization;
using UnityEngine;

namespace Installation01.Networking
{
    public struct SpawnMessage : INetSerialize
    {
        private const ushort Id = (ushort) BuiltInMessage.Instantiate;

        
        public uint NetId;
        public uint Controller;
        public string PrefabName;
        public AuthorityLevel AuthorityLevel;

        public Vector3 pos;
        public Vector3 rot;

        public INetSerialize SpawnData;
        public void Serialize(BitBuffer buffer)
        {
            buffer.AddUShort(Id);
            buffer.AddUInt(NetId);
            buffer.AddUInt(Controller);
            buffer.AddString(PrefabName);
            buffer.AddByte((byte) AuthorityLevel);
            buffer.AddVector3(pos);
            buffer.AddVector3(rot);
            SpawnData?.Serialize(buffer);
        }

        public void Deserialize(BitBuffer buffer)
        {
            NetId = buffer.ReadUInt();
            Controller = buffer.ReadUInt();
            PrefabName = buffer.ReadString();
            AuthorityLevel = (AuthorityLevel) buffer.ReadByte();
            pos = buffer.ReadVector3();
            rot = buffer.ReadVector3();
        }
    }
}