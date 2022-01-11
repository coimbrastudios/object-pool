namespace Coimbra
{
    /// <summary>
    /// Use this to define which information is being overriden.
    /// </summary>
    [System.Flags]
    public enum PoolDataOverrides
    {
        Nothing = 0,
        Everything = ~0,
        Prefab = 1 << 0,
        /// <summary>
        /// The preload count, max capacity and allow infinity instances.
        /// </summary>
        Instances = 1 << 1,
        SpawnFallback = 1 << 2,
        AdvancedOptions = 1 << 3,
        MessageType = 1 << 4,
        MessageOption = 1 << 5,
        SpawnMessage = 1 << 6,
        DespawnMessage = 1 << 7
    }
}
