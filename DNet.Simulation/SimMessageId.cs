namespace DNet.Simulation
{
    public enum SimMessageId : ushort
    {
        InstantiateObject     = ushort.MaxValue - 6,
        DestroyObject         = ushort.MaxValue - 5,
        CreateWorld           = ushort.MaxValue - 4,
        DestroyWorld          = ushort.MaxValue - 3,
        AddClientToWorld      = ushort.MaxValue - 2,
        RemoveClientFromWorld = ushort.MaxValue - 1,
        WorldUpdate           = ushort.MaxValue
    }
}