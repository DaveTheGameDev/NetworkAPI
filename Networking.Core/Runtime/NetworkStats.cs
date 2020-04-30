using System.Text;

namespace Installation01.Networking
{
	public struct NetworkStats
	{
		public uint BytesReceived;
		public uint BytesSent;
		public uint PacketsReceived;
		public uint PacketsSent;
		public uint PeersCount;

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.Append("Network Stats \n");
			stringBuilder.Append($"BytesReceived {BytesReceived} \n");
			stringBuilder.Append($"BytesSent {BytesSent} \n");
			stringBuilder.Append($"PacketsReceived {PacketsReceived} \n");
			stringBuilder.Append($"PacketsSent {PacketsSent} \n");
			stringBuilder.Append($"PeersCount {PeersCount} \n");

			return stringBuilder.ToString();
		}
	}
}