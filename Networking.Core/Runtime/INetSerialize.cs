using Installation01.Networking.NetStack.Serialization;

namespace Installation01.Networking
{
	
	public interface INetSerialize
	{
		void Serialize(BitBuffer buffer);
		void Deserialize(BitBuffer buffer);
	}
}