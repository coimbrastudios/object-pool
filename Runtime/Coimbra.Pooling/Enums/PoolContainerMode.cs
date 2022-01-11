namespace Coimbra
{
    /// <summary>
    /// Define where to keep the available instances.
    /// </summary>
    public enum PoolContainerMode
    {
        /// <summary>
        /// Let the <see cref="Pool"/> create a container for it's objects.
        /// </summary>
        Automatic,
        /// <summary>
        /// Manually choose a parent to be the <see cref="Pool"/>'s container.
        /// </summary>
        Manual
    }
}
