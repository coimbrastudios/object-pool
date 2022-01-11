using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Coimbra
{
    /// <summary>
    /// Global manager for <see cref="Pool"/>s. Setup it in Edit -> Project Settings -> Pool System.
    /// </summary>
    [PublicAPI, AddComponentMenu("")]
    public sealed class PoolSystem : Singleton<PoolSystem>, ISerializationCallbackReceiver
    {
        private static readonly Pool.PoolAction OnPoolDestroy = HandlePoolDestroy;
        private static readonly Pool.PoolAction OnPoolInstantiate = HandlePoolInstantiate;

        private static bool m_HasInstance;
        private static PoolSystem m_Instance;

#if UNITY_EDITOR
#pragma warning disable 0414
        [Tooltip("The current pools added and available to use.")]
        [SerializeField] private List<Pool> m_Pools;
#pragma warning restore 0414
#endif

        private readonly List<Pool> UnloadedPoolsCache = new List<Pool>(32);
        private readonly List<PoolData> LoadedPoolsCache = new List<PoolData>(32);
        private readonly Dictionary<int, Pool> InstancesPools = new Dictionary<int, Pool>();
        private readonly Dictionary<int, Pool> PrefabsPools = new Dictionary<int, Pool>();
        private readonly Dictionary<Pool, PoolInfo> Pools = new Dictionary<Pool, PoolInfo>();

        /// <summary>
        /// Check if the <see cref="PoolSystem"/> is available to be used. Only needed while inside the Unity Editor.
        /// </summary>
        public static bool IsAvailable => IsEditModeOrExitingPlayMode == false;

        private static bool IsEditModeOrExitingPlayMode
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying == false || UnityEditor.EditorApplication.isPlaying != UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
                return false;
#endif
            }
        }

        private static PoolSystem Instance
        {
            get
            {
#if UNITY_EDITOR
                if (IsEditModeOrExitingPlayMode)
                {
                    throw new System.InvalidOperationException($"{nameof(PoolSystem)} should not be called in edit mode or when exiting play mode!");
                }
#endif
                Initialize();

                return m_Instance;
            }
        }

        /// <summary>
        /// Initialize the <see cref="PoolSystem"/>. You only need to call it once.
        /// </summary>
        public static void Initialize()
        {
            if (m_HasInstance == false)
            {
                m_Instance = GetInstance(true);
                m_HasInstance = true;
            }
        }

        /// <summary>
        /// Clear the current cached components in all <see cref="Pool"/>s.
        /// </summary>
        public static void ClearComponentCache()
        {
            foreach (KeyValuePair<Pool, PoolInfo> item in Instance.Pools)
            {
                item.Key.ClearComponentCache();
            }
        }

        /// <summary>
        /// Add and activate a <see cref="Pool"/>. Return if the <see cref="Pool"/> could be added.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="isPersistent">If persistent, the <see cref="Pool"/> will not be destroyed when loading new scenes, it will need to be removed manually.</param>
        public static bool AddPool(Pool pool, bool isPersistent = false)
        {
            return Instance.Internal_AddPool(pool, isPersistent);
        }

        /// <summary>
        /// Check if <see cref="PoolSystem"/> has a <see cref="Pool"/>.
        /// </summary>
        public static bool ContainsPool(Pool pool)
        {
            return Instance.Pools.ContainsKey(pool);
        }

        /// <summary>
        /// Check if there is any <see cref="Pool"/> using the specified prefab.
        /// </summary>
        public static bool ContainsPool(Component prefab)
        {
            return Instance.Internal_ContainsPool(prefab.gameObject);
        }

        /// <summary>
        /// Check if there is any <see cref="Pool"/> using the specified prefab.
        /// </summary>
        public static bool ContainsPool(GameObject prefab)
        {
            return Instance.Internal_ContainsPool(prefab);
        }

        /// <summary>
        /// Despawn an object. Return false if instance needed to be destroyed.
        /// </summary>
        public static bool Despawn(Component instance)
        {
            return Instance.Internal_Despawn(instance.gameObject);
        }

        /// <summary>
        /// Despawn an object. Return false if instance needed to be destroyed.
        /// </summary>
        public static bool Despawn(GameObject instance)
        {
            return Instance.Internal_Despawn(instance);
        }

        /// <summary>
        /// Remove and disable a <see cref="Pool"/>. Return if the <seealso cref="Pool"/> could be removed.
        /// </summary>
        public static bool RemovePool(Pool pool)
        {
            return Instance.Internal_RemovePool(pool);
        }

        /// <summary>
        /// Remove and disable a <see cref="Pool"/> using the specified prefab. Return if the <seealso cref="Pool"/> could be removed.
        /// </summary>
        public static bool RemovePool(Component prefab)
        {
            return Instance.Internal_RemovePool(prefab.gameObject);
        }

        /// <summary>
        /// Remove and disable a <see cref="Pool"/> using the specified prefab. Return if the <seealso cref="Pool"/> could be removed.
        /// </summary>
        public static bool RemovePool(GameObject prefab)
        {
            return Instance.Internal_RemovePool(prefab);
        }

        /// <summary>
        /// Set if a <see cref="Pool"/> should be persistent. Return true if could set the desired value.
        /// </summary>
        public static bool SetPoolPersistent(Pool pool, bool isPersistent)
        {
            return Instance.Internal_SetPoolPersistent(pool, isPersistent);
        }

        /// <summary>
        /// Set if a <see cref="Pool"/> with the specified prefab should be persistent. Return true if could set the desired value.
        /// </summary>
        public static bool SetPoolPersistent(Component prefab, bool isPersistent)
        {
            return Instance.Internal_SetPoolPersistent(prefab.gameObject, isPersistent);
        }

        /// <summary>
        /// Set if a <see cref="Pool"/> with the specified prefab should be persistent. Return true if could set the desired value.
        /// </summary>
        public static bool SetPoolPersistent(GameObject prefab, bool isPersistent)
        {
            return Instance.Internal_SetPoolPersistent(prefab, isPersistent);
        }

        /// <summary>
        /// Get the prefab of the instance. Returns null if it doesn't have a pool.
        /// </summary>
        public static GameObject GetPrefab(Component instance)
        {
            return Instance.Internal_GetPrefab(instance.gameObject);
        }

        /// <summary>
        /// Get the prefab of the instance. Returns null if it doesn't have a pool.
        /// </summary>
        public static GameObject GetPrefab(GameObject instance)
        {
            return Instance.Internal_GetPrefab(instance);
        }

        /// <summary>
        /// Spawn an object or instantiate it if doesn't have a pool.
        /// </summary>
        public static GameObject Spawn(GameObject prefab, Transform parent = null, bool spawnInWorldSpace = false)
        {
            return Instance.Internal_Spawn(prefab, parent, spawnInWorldSpace);
        }

        /// <summary>
        /// Spawn an object or instantiate it if doesn't have a pool.
        /// </summary>
        public static GameObject Spawn(GameObject prefab, Vector3 position, Vector3 eulerAngles, Transform parent = null)
        {
            return Instance.Internal_Spawn(prefab, position, Quaternion.Euler(eulerAngles), parent);
        }

        /// <summary>
        /// Spawn an object or instantiate it if doesn't have a pool.
        /// </summary>
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Instance.Internal_Spawn(prefab, position, rotation, parent);
        }

        /// <summary>
        /// Spawn an object or instantiate it if doesn't have a pool.
        /// </summary>
        public static T Spawn<T>(T prefab, Transform parent = null, bool spawnInWorldSpace = false) where T : Component
        {
            return Instance.Internal_Spawn(prefab, parent, spawnInWorldSpace);
        }

        /// <summary>
        /// Spawn an object or instantiate it if doesn't have a pool.
        /// </summary>
        public static T Spawn<T>(T prefab, Vector3 position, Vector3 eulerAngles, Transform parent = null) where T : Component
        {
            return Instance.Internal_Spawn(prefab, position, Quaternion.Euler(eulerAngles), parent);
        }

        /// <summary>
        /// Spawn an object or instantiate it if doesn't have a pool.
        /// </summary>
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            return Instance.Internal_Spawn(prefab, position, rotation, parent);
        }

        /// <summary>
        /// Get the prefab's pool. Returns null if it doesn't have a pool.
        /// </summary>
        public static Pool GetPool(Component prefab)
        {
            return Instance.Internal_GetPool(prefab.gameObject);
        }

        /// <summary>
        /// Get the prefab's pool. Returns null if it doesn't have a pool.
        /// </summary>
        public static Pool GetPool(GameObject prefab)
        {
            return Instance.Internal_GetPool(prefab);
        }

        /// <summary>
        /// Get all pools in <see cref="PoolSystem"/>.
        /// </summary>
        public static Pool[] GetAllPools()
        {
            return Instance.Internal_GetAllPools();
        }

        protected override void OnDispose(bool isInstance)
        {
            if (isInstance)
            {
                SceneManager.sceneUnloaded -= HandleSceneUnloaded;
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        protected override void OnInitialize()
        {
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;

            PoolSystemSettings.GetPersistentPools(LoadedPoolsCache);

            for (int i = 0; i < LoadedPoolsCache.Count; i++)
            {
                LoadedPoolsCache[i].Validate(true);

                if (LoadedPoolsCache[i].Prefab == null)
                {
                    continue;
                }

                if (PrefabsPools.ContainsKey(LoadedPoolsCache[i].Prefab.GetInstanceID()) == false)
                {
                    AddPoolImmediate(new Pool(LoadedPoolsCache[i]), new PoolInfo(true));
                }
            }
        }

        private static void HandlePoolDestroy(Pool pool, GameObject instance)
        {
            Instance.InstancesPools.Remove(instance.GetInstanceID());
        }

        private static void HandlePoolInstantiate(Pool pool, GameObject instance)
        {
            Instance.InstancesPools.Add(instance.GetInstanceID(), pool);
        }

        private void AddPoolImmediate(Pool pool, PoolInfo poolInfo)
        {
            PrefabsPools.Add(pool.Prefab.GetInstanceID(), pool);
            Pools.Add(pool, poolInfo);
            pool.OnDestroy -= OnPoolDestroy;
            pool.OnDestroy += OnPoolDestroy;
            pool.OnInstantiate -= OnPoolInstantiate;
            pool.OnInstantiate += OnPoolInstantiate;
            pool.SetContainerMode(PoolContainerMode.Automatic);
            pool.Activate();
            pool.Container.parent = transform;
        }

        private void RemovePoolImmediate(Pool pool)
        {
            PrefabsPools.Remove(pool.Prefab.GetInstanceID());
            Pools.Remove(pool);
            pool.Deactivate();
            pool.OnDestroy -= OnPoolDestroy;
            pool.OnInstantiate -= OnPoolInstantiate;
        }

        private bool Internal_AddPool(Pool pool, bool isPersistent)
        {
            if (pool != null
             && pool.IsActive == false
             && pool.Prefab != null
             && PrefabsPools.ContainsKey(pool.Prefab.GetInstanceID()) == false
             && Pools.ContainsKey(pool) == false)
            {
                AddPoolImmediate(pool, new PoolInfo(isPersistent));

                return true;
            }

            return false;
        }

        private bool Internal_ContainsPool(GameObject prefab)
        {
            return PrefabsPools.ContainsKey(prefab.GetInstanceID());
        }

        private bool Internal_Despawn(GameObject instance)
        {
            if (InstancesPools.TryGetValue(instance.GetInstanceID(), out Pool pool))
            {
                return pool.Despawn(instance) == PoolDespawnResult.Despawned;
            }

            Destroy(instance);

            return false;
        }

        private bool Internal_RemovePool(Pool pool)
        {
            if (pool != null)
            {
                if (Pools.ContainsKey(pool))
                {
                    RemovePoolImmediate(pool);

                    return true;
                }

                if (pool.IsActive)
                {
                    if (PrefabsPools.TryGetValue(pool.Prefab.GetInstanceID(), out pool))
                    {
                        RemovePoolImmediate(pool);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool Internal_RemovePool(GameObject prefab)
        {
            if (PrefabsPools.TryGetValue(prefab.GetInstanceID(), out Pool pool))
            {
                RemovePoolImmediate(pool);

                return true;
            }

            return false;
        }

        private bool Internal_SetPoolPersistent(Pool pool, bool isPersistent)
        {
            if (pool != null)
            {
                if (Pools.ContainsKey(pool))
                {
                    Pools[pool].SetPersistent(isPersistent);

                    return true;
                }

                if (pool.IsActive)
                {
                    if (PrefabsPools.TryGetValue(pool.Prefab.GetInstanceID(), out pool))
                    {
                        Pools[pool].SetPersistent(isPersistent);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool Internal_SetPoolPersistent(GameObject prefab, bool isPersistent)
        {
            if (PrefabsPools.TryGetValue(prefab.GetInstanceID(), out Pool pool))
            {
                Pools[pool].SetPersistent(isPersistent);

                return true;
            }

            return false;
        }

        private GameObject Internal_GetPrefab(GameObject instance)
        {
            return InstancesPools.TryGetValue(instance.GetInstanceID(), out Pool pool)
                       ? pool.Prefab
                       : null;
        }

        private GameObject Internal_Spawn(GameObject prefab, Transform parent, bool spawnInWorldSpace)
        {
            return PrefabsPools.TryGetValue(prefab.GetInstanceID(), out Pool pool)
                       ? pool.Spawn(parent, spawnInWorldSpace)
                       : Instantiate(prefab, parent, spawnInWorldSpace);
        }

        private GameObject Internal_Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            return PrefabsPools.TryGetValue(prefab.GetInstanceID(), out Pool pool)
                       ? pool.Spawn(position, rotation, parent)
                       : Instantiate(prefab, position, rotation, parent);
        }

        private T Internal_Spawn<T>(T prefab, Transform parent, bool spawnInWorldSpace) where T : Component
        {
            return PrefabsPools.TryGetValue(prefab.gameObject.GetInstanceID(), out Pool pool)
                       ? pool.Spawn(prefab, parent, spawnInWorldSpace)
                       : Instantiate(prefab, parent, spawnInWorldSpace);
        }

        private T Internal_Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            return PrefabsPools.TryGetValue(prefab.gameObject.GetInstanceID(), out Pool pool)
                       ? pool.Spawn(prefab, position, rotation, parent)
                       : Instantiate(prefab, position, rotation, parent);
        }

        private Pool Internal_GetPool(GameObject prefab)
        {
            return PrefabsPools.TryGetValue(prefab.GetInstanceID(), out Pool pool)
                       ? pool
                       : null;
        }

        private Pool[] Internal_GetAllPools()
        {
            var value = new Pool[Pools.Count];
            Pools.Keys.CopyTo(value, 0);

            return value;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            PoolSystemSettings.GetTemporaryPools(scene.path, LoadedPoolsCache);

            for (int i = 0; i < LoadedPoolsCache.Count; i++)
            {
                LoadedPoolsCache[i].Validate(true);

                if (LoadedPoolsCache[i].Prefab == null)
                {
                    continue;
                }

                if (PrefabsPools.TryGetValue(LoadedPoolsCache[i].Prefab.GetInstanceID(), out Pool pool))
                {
                    Pools[pool].Scenes.Add(scene);
                }
                else
                {
                    AddPoolImmediate(new Pool(LoadedPoolsCache[i]), new PoolInfo(scene));
                }
            }
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            UnloadedPoolsCache.Clear();

            foreach (KeyValuePair<Pool, PoolInfo> item in Pools)
            {
                if (item.Value.Scenes.Remove(scene))
                {
                    if (item.Value.IsPersistent)
                    {
                        continue;
                    }

                    if (item.Value.Scenes.Count == 0)
                    {
                        UnloadedPoolsCache.Add(item.Key);
                    }
                }
            }

            for (int i = 0; i < UnloadedPoolsCache.Count; i++)
            {
                Pools.Remove(UnloadedPoolsCache[i]);
                UnloadedPoolsCache[i].Deactivate();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (Application.isPlaying && Pools != null)
            {
                if (m_Pools == null)
                {
                    m_Pools = new List<Pool>(Pools.Keys);
                }
                else
                {
                    m_Pools.Clear();
                    m_Pools.AddRange(Pools.Keys);
                }
            }
#endif
        }
    }
}
