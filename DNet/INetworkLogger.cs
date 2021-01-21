namespace DNet
{
    public interface INetworkLogger
    {
        void LogMessage(string message);
        void LogWarning(string message);
        void LogError(string message);
    }

    internal class DefaultLogger : INetworkLogger
    {
        public void LogMessage(string message)=> System.Console.WriteLine(message);

        public void LogWarning(string message)=> System.Console.WriteLine(message);
        public void LogError(string message)=> System.Console.WriteLine(message);
    }
}