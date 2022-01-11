using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Available behaviours when trying to spawn with a <see cref="Pool"/> that reached it's limit.
    /// </summary>
    public enum PoolSpawnFallback
    {
        /// <summary>
        /// Use <see cref="Object.Instantiate(Object)"/> when there is no available instance.
        /// </summary>
        UseInstantiate,
        /// <summary>
        /// Don't spawn anything and return null.
        /// </summary>
        ReturnNull,
        /// <summary>
        /// Debug an exception and return null.
        /// </summary>
        DebugException
    }
}
