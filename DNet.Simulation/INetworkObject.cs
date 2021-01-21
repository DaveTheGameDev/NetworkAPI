using DNet.NetStack;

namespace DNet.Simulation
{
    public interface INetworkObject
    {
        uint Id      { get; set; }
        NetWorld World { get; set; }
        
        void NetworkUpdate(in uint tick, in uint ms);
        
        void Serialize(BitBuffer buffer);
        void Deserialize(BitBuffer buffer);
        void Destroy();
    }
}