using System;

#if ENABLE_MONO || ENABLE_IL2CPP

#endif

namespace Installation01.Networking.NetStack.Buffers
{
	internal sealed class ArrayPoolEventSource
	{
#if NETSTACK_BUFFERS_LOG
			internal static readonly ArrayPoolEventSource EventLog = new ArrayPoolEventSource();

			internal enum BufferAllocatedReason : int {
				Pooled,
				OverMaximumSize,
				PoolExhausted
			}

			internal void BufferAllocated(int bufferId, int bufferSize, int poolId, int bucketId, BufferAllocatedReason reason) {
				var message =
 "Buffer allocated (Buffer ID: " + bufferId + ", Buffer size: " + bufferSize + ", Pool ID: " + poolId + ", Bucket ID: " + bucketId + ", Reason: " + reason + ")";

				if (reason == BufferAllocatedReason.Pooled)
					Log.Info("Buffers", message);
				else
					Log.Warning("Buffers", message);
			}

			internal void BufferRented(int bufferId, int bufferSize, int poolId, int bucketId) {
				Log.Info("Buffers", "Buffer rented (Buffer ID: " + bufferId + ", Buffer size: " + bufferSize + ", Pool ID: " + poolId + ", Bucket ID: " + bucketId + ")");
			}

			internal void BufferReturned(int bufferId, int bufferSize, int poolId) {
				Log.Info("Buffers", "Buffer returned (Buffer ID: " + bufferId + ", Buffer size: " + bufferSize + ", Pool ID: " + poolId + ")");
			}
#endif
	}

	internal static class Log
	{
		private static string Output(string module, string message)
		{
			return DateTime.Now.ToString("[HH:mm:ss]") + " [NetStack." + module + "] " + message;
		}

		public static void Info(string module, string message)
		{
#if ENABLE_MONO || ENABLE_IL2CPP
			//NetworkDispatcher.Run(() => { DevConsole.LogMessage(Output(module, message)); });
#else
				NetworkDispatcher.Run(() => { DevConsole.LogMessage(Output(module, message)); });
#endif
		}

		public static void Warning(string module, string message)
		{
#if ENABLE_MONO || ENABLE_IL2CPP
			//NetworkDispatcher.Run(() => { DevConsole.LogWarning(Output(module, message)); });
#else
				Console.ForegroundColor = ConsoleColor.Yellow;
				NetworkDispatcher.Run(() => { DevConsole.LogMessage(Output(module, message)); });
				Console.ResetColor();
#endif
		}
	}
}