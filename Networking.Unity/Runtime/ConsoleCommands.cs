using Debugging.DeveloperConsole;
using Installation01.Networking;

namespace Debugging
{
    public static class ConsoleCommands
    {
        
        [ConsoleCommand]
        [CommandDescription("Connect to a server")]
        private static void Connect(string ip, ushort port)
        {
            Network.Connect(new ClientMessageProcessor(), ip, port);
        }
        
        [ConsoleCommand]
        [CommandDescription("Connect to a server")]
        private static void Lan()
        {
            Network.Connect(new ClientMessageProcessor(), "127.0.0.1", 34377);
        }
        
        [ConsoleCommand]
        [CommandDescription("Start a server")]
        private static void Server()
        {
            Network.StartServer(new ServerMessageProcessor(), 34377, 15);
        }
        
        [ConsoleCommand]
        [CommandDescription("Start a server")]
        private static void Disconnect()
        {
            Network.Shutdown(false);
        }
    }
}