using Installation01.Networking.NetStack.Serialization;

namespace Installation01.Networking
{
	public struct ServerMessage
	{
		public Peer Sender;
		public BitBuffer BitBuffer;
	}
}