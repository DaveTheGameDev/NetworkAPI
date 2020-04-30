using System;

namespace Installation01.Networking
{
	public static class NetworkLogger
	{
		public static event Action<string> LogMethod;
		public static event Action<string> LogWarningMethod;
		public static event Action<string> LogErrorMethod;

		public static void Log(string obj)
		{
			LogMethod?.Invoke(obj);
		}

		public static void LogWarning(string obj)
		{
			LogWarningMethod?.Invoke(obj);
		}

		public static void LogError(string obj)
		{
			LogErrorMethod?.Invoke(obj);
		}
	}
}