using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Use this to share data between different <see cref="Pool"/>s.
    /// </summary>
    public sealed class PoolDataAsset : ScriptableObject
    {
        // This variable name should match the one in Pool.cs
        [SerializeField] private PoolData _data = PoolData.Default;

        /// <summary>
        /// The actual data.
        /// </summary>
        public PoolData Data
        {
            get
            {
                _data.Validate(false);

                return _data;
            }
            private set { _data = value; }
        }

        /// <summary>
        /// Create a new asset with the given data.
        /// </summary>
        public static PoolDataAsset CreateInstance(PoolData data)
        {
            var poolBase = CreateInstance<PoolDataAsset>();
            poolBase.Data = data;

            return poolBase;
        }
    }
}
