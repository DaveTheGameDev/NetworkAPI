namespace Installation01.Networking
{
    internal enum BuiltInMessage : ushort
    {
        SceneLoad      = ushort.MaxValue,
        Instantiate    = ushort.MaxValue - 1,
        Destroy        = ushort.MaxValue - 2,
        State          = ushort.MaxValue - 3,
        ControlChanged = ushort.MaxValue - 4
    }
}