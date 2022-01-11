namespace Coimbra
{
    /// <summary>
    /// Additional options to use with <see cref="Pool"/>s.
    /// </summary>
    [System.Flags]
    public enum PoolAdvancedOptions
    {
        Nothing = 0,
        Everything = ~0,
        /// <summary>
        /// Should build the component cache when instantiating an instance? It affects the memory used by the <see cref="Pool"/>.
        /// </summary>
        BuildComponentCacheOnInstantiate = 1 << 0,
        /// <summary>
        /// Should instances be renamed on instantiate? It generates garbage if enabled. (Editor Only)
        /// </summary>
        ChangeNamesOnInstantiate = 1 << 1,
        /// <summary>
        /// Should reset the instances scale when spawning? It affects the performance when spawning.
        /// </summary>
        ResetScaleOnSpawn = 1 << 2,
        /// <summary>
        /// Should the instance return to the container when despawning? It affects the performance when despawning.
        /// </summary>
        ReturnToContainerOnDespawn = 1 << 3,
    }
}
