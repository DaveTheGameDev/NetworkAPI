using System.Runtime.CompilerServices;
using DNet.NetStack;
using ENet;

namespace DNet.ENetTransport
{
    public static class BitBufferExt
    {
        [MethodImpl(256)]
        public static BitBuffer CreateFromEvent(Event netEvent)
        {
            var buffer = BufferPool.GetBuffer();
            buffer.Clear();

            int len = netEvent.Packet.Length;
			
            var dataBuffer = BufferPool.GetBuffer(netEvent.Packet.Length);
            netEvent.Packet.CopyTo(dataBuffer);

            buffer.FromArray(dataBuffer, len);
            return buffer;
        }
    }
}