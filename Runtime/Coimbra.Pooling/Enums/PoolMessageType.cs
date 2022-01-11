using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Message types available to use with <see cref="Pool"/>s.
    /// </summary>
    public enum PoolMessageType
    {
        /// <summary>
        /// Don't send any message.
        /// </summary>
        None,
        /// <summary>
        /// Use <see cref="GameObject.SendMessage(string)"/>
        /// </summary>
        SendMessage,
        /// <summary>
        /// Use <see cref="GameObject.BroadcastMessage(string)"/>
        /// </summary>
        BroadcastMessage
    }
}
