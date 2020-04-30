using Installation01.Networking.NetStack.Serialization;

namespace Installation01.Networking
{
    internal struct SceneLoadMessage : INetSerialize
    {
        private const ushort Id = (ushort) BuiltInMessage.SceneLoad;
        public string SceneName;
        
        public void Serialize(BitBuffer buffer)
        {
            buffer.AddUShort(Id);
            buffer.AddString(SceneName);
        }

        public void Deserialize(BitBuffer buffer)
        {
            SceneName = buffer.ReadString();
        }
    }
}