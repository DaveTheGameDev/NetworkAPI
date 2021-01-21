using UnityEngine;

namespace DNet.Unity
{
    public class UnityNetworkLogger : INetworkLogger
    {
        private struct UnityLog
        {
            public LogType LogType;
            public string Message;
        }

        private static readonly ConcurrentQueue<UnityLog> _Queue = new ConcurrentQueue<UnityLog>();

        public void LogMessage(string message)
        {
            _Queue.Enqueue(new UnityLog {LogType = LogType.Log, Message = message});
        }

        public void LogWarning(string message)
        {
            _Queue.Enqueue(new UnityLog {LogType = LogType.Warning, Message = message});
        }

        public void LogError(string message)
        {
            _Queue.Enqueue(new UnityLog {LogType = LogType.Error, Message = message});
        }

        public static void RunQueue()
        {
            while (!_Queue.IsEmpty)
            {
                if (_Queue.TryDequeue(out var message))
                {
                    switch (message.LogType)
                    {
                        case LogType.Error:
                            Debug.LogError(message.Message);
                            break;
                        case LogType.Assert:
                            Debug.LogError(message.Message);
                            break;
                        case LogType.Warning:
                            Debug.LogWarning(message.Message);
                            break;
                        case LogType.Log:
                            Debug.Log(message.Message);
                            break;
                        case LogType.Exception:
                            Debug.LogError(message.Message);
                            break;
                    }
                }
            }
        }
    }
}