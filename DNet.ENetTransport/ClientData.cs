using DNet.NetStack;
using ENet;

namespace DNet.ENetTransport
{
    public struct ClientData
    {
        public uint Id => peer.ID;
        public Peer peer;
        public BitBuffer connectionData;
    }
}