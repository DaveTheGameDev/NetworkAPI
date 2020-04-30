using System;
using System.Threading;

namespace Installation01.Networking.NetStack.Buffers
{
	internal sealed partial class DefaultArrayPool<T> : ArrayPool<T>
	{
		private sealed class Bucket
		{
			internal readonly int _bufferLength;
			private readonly T[][] _buffers;
#if NETSTACK_BUFFERS_LOG
				private readonly int _poolId;
#endif
#if NET_4_6 || NET_STANDARD_2_0
			private SpinLock _lock;
#else
				private object _lock;
#endif
			private int _index;

			internal Bucket(int bufferLength, int numberOfBuffers, int poolId)
			{
#if NET_4_6 || NET_STANDARD_2_0
				_lock = new SpinLock();
#else
					_lock = new Object();
#endif
				_buffers = new T[numberOfBuffers][];
				_bufferLength = bufferLength;

#if NETSTACK_BUFFERS_LOG
					_poolId = poolId;
#endif
			}

#if NET_4_6 || NET_STANDARD_2_0
			internal int Id => GetHashCode();
#else
				internal int Id {
					get {
						return GetHashCode();
					}
				}
#endif

			internal T[] Rent()
			{
				T[][] buffers = _buffers;
				T[] buffer = null;
				bool allocateBuffer = false;

#if NET_4_6 || NET_STANDARD_2_0
				bool lockTaken = false;

				try
				{
					_lock.Enter(ref lockTaken);

					if (_index < buffers.Length)
					{
						buffer = buffers[_index];
						buffers[_index++] = null;
						allocateBuffer = buffer == null;
					}
				}

				finally
				{
					if (lockTaken)
						_lock.Exit(false);
				}
#else
					try {
						Monitor.Enter(_lock);

						if (_index < buffers.Length) {
							buffer = buffers[_index];
							buffers[_index++] = null;
							allocateBuffer = buffer == null;
						}
					}

					finally {
						Monitor.Exit(_lock);
					}
#endif

				if (allocateBuffer)
				{
					buffer = new T[_bufferLength];

#if NETSTACK_BUFFERS_LOG
						var log = ArrayPoolEventSource.EventLog;

						log.BufferAllocated(buffer.GetHashCode(), _bufferLength, _poolId, Id, ArrayPoolEventSource.BufferAllocatedReason.Pooled);
#endif
				}

				return buffer;
			}

			internal void Return(T[] array)
			{
				if (array.Length != _bufferLength)
					throw new ArgumentException("BufferNotFromPool", "array");

#if NET_4_6 || NET_STANDARD_2_0
				bool lockTaken = false;

				try
				{
					_lock.Enter(ref lockTaken);

					if (_index != 0)
						_buffers[--_index] = array;
				}

				finally
				{
					if (lockTaken)
						_lock.Exit(false);
				}
#else
					try {
						Monitor.Enter(_lock);

						if (_index != 0)
							_buffers[--_index] = array;
					}

					finally {
						Monitor.Exit(_lock);
					}
#endif
			}
		}
	}
}