using UnityEngine;

namespace Installation01.Networking
{
	public class NetworkDispatcherRunner : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Init()
		{
			new GameObject("Network Dispatcher", typeof(NetworkDispatcherRunner));
		}

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			NetworkDispatcher.Update();
		}

		private void OnDisable()
		{
			while (!NetworkDispatcher.IsEmpty)
			{
				Update();
			}
		}
	}
}