using Installation01.Networking.NetStack.Serialization;
using UnityEditor;
using UnityEngine;

namespace Installation01.Networking
{
    public class NetworkDebugger : EditorWindow
    {
        [MenuItem ("Window/Analysis/Network Debugger")]
        public static void ShowWindow ()
        {
            EditorWindow window = EditorWindow.GetWindow <NetworkDebugger>("Network Debugger");
        }

        private void OnGUI ()
        {
            Repaint();

            GUI.enabled = EditorApplication.isPlaying;
            
            GUILayout.BeginHorizontal("Box");

            ShowControls();
            DisplayStats();
            
            GUILayout.Space(5);

            if (Network.IsRunning)
            {
                GUILayout.BeginScrollView(new Vector2(80, 250));
            
                ClientData[] clients = Network.GetConnections();

                if (clients != null)
                {
                    foreach (ClientData client in clients)
                    {
                        ShowPeer(client);
                    }
                }

                GUILayout.EndScrollView();
            }
           
            GUILayout.EndHorizontal();
        }
        

        private void ShowPeer(ClientData client)
        {
            GUILayout.BeginVertical ("box");
            
            GUILayout.Label($"Client {client.Id} ({client.Peer.IP}:{client.Peer.Port})", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"BytesSent: {client.Peer.BytesSent}");
            EditorGUILayout.LabelField($"PacketsSent: {client.Peer.PacketsSent}");
            EditorGUILayout.LabelField($"RTT: {client.Peer.RoundTripTime}");

            if (GUILayout.Button("Kick"))
            {
                Network.KickConnection(client.Id);
            }
            
            if (GUILayout.Button("SendDebugPacket"))
            {
                INetSerialize message = new DebugTestMessage(55);
                Network.BroadcastReliable(ref message);
            }
            
            GUILayout.EndVertical ();
        }

        private void ShowControls()
        {
            GUILayout.BeginVertical ("box");

            if (!Network.IsRunning)
            {
                if(GUILayout.Button("Start Server"))
                {
                    if (!Network.IsRunning)
                        Network.StartServer(new ServerMessageProcessor(), 34377, 32);
                }
            
                if(GUILayout.Button("Start Client"))
                {
                    if (!Network.IsRunning)
                        Network.Connect(new ClientMessageProcessor(), "127.0.0.1", 34377);
                }
            }
            else
            {
                if(GUILayout.Button("Stop Network"))
                {
                    if (Network.IsRunning)
                        Network.Shutdown(true);
                }
            }
           
            
            GUILayout.EndVertical ();
        }

        private void DisplayStats()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(Network.NetworkType.ToString());

            NetworkStats stats = Network.GetStats();

            EditorGUILayout.LabelField($"BytesReceived: {stats.BytesReceived}");
            EditorGUILayout.LabelField($"BytesSent: {stats.BytesSent}");
            EditorGUILayout.LabelField($"PacketsReceived: {stats.PacketsReceived}");
            EditorGUILayout.LabelField($"PacketsSent: {stats.PacketsSent}");
            
            if (Network.NetworkType == NetworkType.Server)
                EditorGUILayout.LabelField($"PeersCount: {stats.PeersCount}");
            
            if (Network.NetworkType == NetworkType.Client)
                EditorGUILayout.LabelField($"Connection State: {Network.ConnectionState}");
            
            GUILayout.EndVertical();
        }
    }


    public readonly struct DebugTestMessage : INetSerialize
    {
        public readonly int Int;
        
        public DebugTestMessage(int i)
        {
            Int = i;
        }

        public void Serialize(BitBuffer buffer)
        {
            buffer.AddInt(Int);
        }

        public void Deserialize(BitBuffer buffer)
        {
            
        }
    }

  

    internal unsafe class ClientMessageProcessor : IClientMessageHandler
    {
        public unsafe void MessageReceived(BitBuffer data)
        {
            NetworkDispatcher.Run(() =>
            {
                data.Release();
                INetSerialize message = new DebugTestMessage(55);
                Network.BroadcastReliable(ref message);
            });
        }
    }

    internal class ServerMessageProcessor : IServerMessageHandler
    {
        public unsafe void MessageReceived(Peer sender, BitBuffer data)
        {
            NetworkDispatcher.Run(() =>
            {
                data.Release();
                INetSerialize message = new DebugTestMessage(55);
                Network.BroadcastReliable(ref message);
            });
        }
    }
}