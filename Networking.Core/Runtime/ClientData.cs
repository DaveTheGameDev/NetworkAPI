using Installation01.Networking;
using Installation01.Networking.NetStack.Serialization;

namespace Installation01.Networking
{
	public struct ClientData
	{
		public uint Id => Peer.ID;
		public Peer Peer;
		public bool levelLoaded;
		public BitBuffer ConnectionData;
	}
}