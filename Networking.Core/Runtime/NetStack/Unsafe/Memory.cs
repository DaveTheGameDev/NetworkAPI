using System;
using System.Runtime.CompilerServices;

namespace Installation01.Networking.NetStack.Unsafe
{
#if NET_4_6 || NET_STANDARD_2_0
	public static class Memory
	{
		[MethodImpl(256)]
		public static unsafe void Copy(IntPtr source, int sourceOffset, byte[] destination, int destinationOffset,
			int length)
		{
			if (length > 0)
			{
				fixed (byte* destinationPointer = &destination[destinationOffset])
				{
					byte* sourcePointer = (byte*) source + sourceOffset;

					Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
				}
			}
		}

		[MethodImpl(256)]
		public static unsafe void Copy(byte[] source, int sourceOffset, IntPtr destination, int destinationOffset,
			int length)
		{
			if (length > 0)
			{
				fixed (byte* sourcePointer = &source[sourceOffset])
				{
					byte* destinationPointer = (byte*) destination + destinationOffset;

					Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
				}
			}
		}

		[MethodImpl(256)]
		public static unsafe void Copy(byte[] source, int sourceOffset, byte[] destination, int destinationOffset,
			int length)
		{
			if (length > 0)
			{
				fixed (byte* sourcePointer = &source[sourceOffset])
				{
					fixed (byte* destinationPointer = &destination[destinationOffset])
					{
						Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
					}
				}
			}
		}
		
		[MethodImpl(256)]
		public static unsafe void Copy(IntPtr source, IntPtr destination, int length)
		{
			if (length > 0)
			{
				Buffer.MemoryCopy((void*)source, (void*)destination, length, length);
			}
		}
	}
#endif
}