using Installation01.Networking.NetStack.Serialization;
using Installation01.Networking.NetStack.Threading;

namespace Installation01.Networking.NetStack
{
	internal static class BufferPool
	{
		private static readonly ConcurrentPool<BitBuffer> pool = new ConcurrentPool<BitBuffer>(64, Create);

		public static BitBuffer GetBuffer()
		{
			return pool.Acquire();
		}

		public static void Release(BitBuffer bitBuffer)
		{
			pool.Release(bitBuffer);
		}

		private static BitBuffer Create()
		{
			return new BitBuffer();
		}
	}
}