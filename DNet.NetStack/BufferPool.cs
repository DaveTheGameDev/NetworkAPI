using DNet.NetStack.Buffers;

namespace DNet.NetStack
{
    public static class BufferPool
    {
        private static readonly ConcurrentPool<BitBuffer> Pool = new ConcurrentPool<BitBuffer>(64, Create);
        private static readonly ArrayPool<byte> BytePool = ArrayPool<byte>.Create(1024, 32);
        
        public static BitBuffer GetBuffer()
        {
            return Pool.Acquire();
        }
        
        public static byte[] GetBuffer(int minLen)
        {
            return BytePool.Rent(minLen);
        }

        public static void Release(BitBuffer bitBuffer)
        {
            Pool.Release(bitBuffer);
        }
        
        public static void Release(byte[] buffer)
        {
            BytePool.Return(buffer);
        }

        private static BitBuffer Create()
        {
            return new BitBuffer();
        }
    }
}