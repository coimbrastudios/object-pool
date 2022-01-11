namespace Coimbra
{
    /// <summary>
    /// Results available when despawning an object.
    /// </summary>
    public enum PoolDespawnResult
    {
        /// <summary>
        /// Successfully despawned the object.
        /// </summary>
        Despawned,
        /// <summary>
        /// The object got destroyed.
        /// </summary>
        Destroyed,
        /// <summary>
        /// Aborted the operation because the object does not belong to the <see cref="Pool"/>.
        /// </summary>
        Aborted
    }
}
