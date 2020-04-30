using System;
using System.Collections.Concurrent;

namespace Installation01.Networking
{
    /// <summary>
    ///     Used to dispatch action events to the main thread.
    /// </summary>
    public static class NetworkDispatcher
	{
		private static ConcurrentQueue<Action> executionQueue = new ConcurrentQueue<Action>();
		public static bool IsEmpty => executionQueue.IsEmpty;

		/// <summary>
        ///     Call this on the main thread to get desired results.
        /// </summary>
        public static void Update()
		{
			if (executionQueue.IsEmpty)
			{
				return;
			}

			for (int i = 0; i < executionQueue.Count; i++)
			{
				if (executionQueue.TryDequeue(out var action))
					action?.Invoke();
			}
		}

		/// <summary>
        ///     Enqueue an action to the dispatcher to be run on the main thread.
        /// </summary>
        /// <param name="action"></param>
        public static void Run(Action action)
		{
			if (action != null)
			{
				executionQueue.Enqueue(action);
			}
		}

       
        

        public static void Clear()
        {
	        executionQueue = new ConcurrentQueue<Action>();
        }
	}
}