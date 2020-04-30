using Installation01.Networking.NetStack.Serialization;

namespace Installation01.Networking
{
	public interface IClientMessageHandler
	{
		void MessageReceived(BitBuffer data);
	}
	
	public interface IServerMessageHandler
	{
		void MessageReceived(Peer sender, BitBuffer data);
	}
}