using System.Runtime.CompilerServices;
using UnityEngine.Assertions;

#if ENABLE_MONO || ENABLE_IL2CPP

#endif

namespace Installation01.Networking.NetStack.Buffers
{
	internal static class Utilities
	{
		[MethodImpl(256)]
		internal static int SelectBucketIndex(int bufferSize)
		{
#if ENABLE_MONO || ENABLE_IL2CPP
			Assert.IsTrue(bufferSize > 0);
#else
				Debug.Assert(bufferSize > 0);
#endif

			uint bitsRemaining = ((uint) bufferSize - 1) >> 4;
			int poolIndex = 0;

			if (bitsRemaining > 0xFFFF)
			{
				bitsRemaining >>= 16;
				poolIndex = 16;
			}

			if (bitsRemaining > 0xFF)
			{
				bitsRemaining >>= 8;
				poolIndex += 8;
			}

			if (bitsRemaining > 0xF)
			{
				bitsRemaining >>= 4;
				poolIndex += 4;
			}

			if (bitsRemaining > 0x3)
			{
				bitsRemaining >>= 2;
				poolIndex += 2;
			}

			if (bitsRemaining > 0x1)
			{
				bitsRemaining >>= 1;
				poolIndex += 1;
			}

			return poolIndex + (int) bitsRemaining;
		}

		[MethodImpl(256)]
		internal static int GetMaxSizeForBucket(int binIndex)
		{
			int maxSize = 16 << binIndex;

#if ENABLE_MONO || ENABLE_IL2CPP
			Assert.IsTrue(maxSize >= 0);
#else
				Debug.Assert(maxSize >= 0);
#endif

			return maxSize;
		}
	}
}