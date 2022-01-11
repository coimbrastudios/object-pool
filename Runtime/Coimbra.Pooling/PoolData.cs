using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Internal data structure of a <see cref="Pool"/>.
    /// </summary>
    [System.Serializable]
    public struct PoolData
    {
        public const string DefaultDespawnMessage = "OnDespawn";
        public const string DefaultSpawnMessage = "OnSpawn";

        public static readonly PoolData Default = new PoolData()
        {
            Asset = null,
            Overrides = PoolDataOverrides.Everything,
            Prefab = null,
            PreloadCount = 1,
            MaxCapacity = 1,
            AllowInfinityInstances = false,
            SpawnFallback = PoolSpawnFallback.UseInstantiate,
            AdvancedOptions = PoolAdvancedOptions.Everything,
            MessageType = PoolMessageType.SendMessage,
            MessageOption = SendMessageOptions.DontRequireReceiver,
            SpawnMessage = DefaultSpawnMessage,
            DespawnMessage = DefaultDespawnMessage
        };

#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField] private bool m_IsDuplicated;
#pragma warning restore 0414
#endif
        public PoolDataAsset Asset;
        public PoolDataOverrides Overrides;
        public GameObject Prefab;
        public int PreloadCount;
        public int MaxCapacity;
        public bool AllowInfinityInstances;
        public PoolSpawnFallback SpawnFallback;
        [EnumFlags] public PoolAdvancedOptions AdvancedOptions;
        public PoolMessageType MessageType;
        public SendMessageOptions MessageOption;
        public string SpawnMessage;
        public string DespawnMessage;

        internal void Validate(bool once)
        {
            if (Asset == null)
            {
                return;
            }

            if (Overrides != PoolDataOverrides.Everything)
            {
                Apply(Asset.Data);
            }

            if (once == false)
            {
                return;
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                Asset = null;
            }
        }

        private void Apply(PoolData data)
        {
            if ((Overrides & PoolDataOverrides.Prefab) == 0)
            {
                Prefab = data.Prefab;
            }

            if ((Overrides & PoolDataOverrides.Instances) == 0)
            {
                PreloadCount = data.PreloadCount;
                MaxCapacity = data.MaxCapacity;
                AllowInfinityInstances = data.AllowInfinityInstances;
            }

            if ((Overrides & PoolDataOverrides.SpawnFallback) == 0)
            {
                SpawnFallback = data.SpawnFallback;
            }

            if ((Overrides & PoolDataOverrides.AdvancedOptions) == 0)
            {
                AdvancedOptions = data.AdvancedOptions;
            }

            if ((Overrides & PoolDataOverrides.MessageType) == 0)
            {
                MessageType = data.MessageType;
            }

            if ((Overrides & PoolDataOverrides.MessageOption) == 0)
            {
                MessageOption = data.MessageOption;
            }

            if ((Overrides & PoolDataOverrides.SpawnMessage) == 0)
            {
                SpawnMessage = data.SpawnMessage;
            }

            if ((Overrides & PoolDataOverrides.DespawnMessage) == 0)
            {
                DespawnMessage = data.DespawnMessage;
            }
        }
    }
}
