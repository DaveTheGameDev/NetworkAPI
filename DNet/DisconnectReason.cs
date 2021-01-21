namespace DNet
{
    public enum DisconnectReason : byte
    {
        Disconnected             = 0,
        TimeOut                  = 1,
        Kicked                   = 2,
        Banned                   = 3,
        InvalidVersion           = 4,
        InvalidConnectionToken   = 5
    }
}