using System.Collections.Generic;
using UnityEngine;

namespace Coimbra
{
    /// <summary>
    /// Use this to create pools and manage them by instance or with <see cref="PoolSystem"/>.
    /// </summary>
    [System.Serializable]
    public sealed class Pool : ISerializationCallbackReceiver
    {
        public delegate void PoolAction(Pool pool, GameObject instance);

        public event PoolAction OnInstantiate;
        public event PoolAction OnDestroy;
        public event PoolAction OnSpawn;
        public event PoolAction OnDespawn;

        private const int DefaultComponentCache = 5;

        [SerializeField] private bool _isActive;
        [SerializeField] private PoolData _data = PoolData.Default;
        [SerializeField] private PoolContainerMode _containerMode = PoolContainerMode.Automatic;
        [SerializeField] private Transform _container;

        private Vector3 _defaultScale;
        private HashSet<int> _availableInstancesIds;
        private List<Component> _instancesComponentsCache;
        private List<Component> _prefabsComponentsCache;
        private Stack<GameObject> _availableInstances;
        private Dictionary<GameObject, Dictionary<int, Component>> _instancesComponents;

#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField] private int m_AvailableCount;
        [SerializeField] private int m_TotalCount;

        private int m_UniqueID;
        private string m_PrefabName;
#pragma warning restore 0414
#endif
        /// <summary>
        /// Is the <see cref="Pool"/> currently active?
        /// </summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// Amount of instances to preload when the <see cref="Pool"/> is activated.
        /// </summary>
        public int PreloadCount
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.PreloadCount;
            }
        }
        /// <summary>
        /// The current container mode selected for this <see cref="Pool"/>.
        /// </summary>
        public PoolContainerMode ContainerMode
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _containerMode;
            }
        }
        /// <summary>
        /// Read only data structure of the <see cref="Pool"/>.
        /// </summary>
        public PoolData Data
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data;
            }
        }
        /// <summary>
        /// The prefab being used. Do not destroy it!
        /// </summary>
        public GameObject Prefab
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.Prefab;
            }
        }
        /// <summary>
        /// The current container used for available instances. Do not destroy it!
        /// <para></para>
        /// It gets automatically destroyed when <see cref="Pool"/> is deactivated if <see cref="ContainerMode"/> is set to <see cref="PoolContainerMode.Automatic"/>.
        /// </summary>
        public Transform Container
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _container;
            }
        }

        /// <summary>
        /// Allow the <see cref="Pool"/> to hold any amount of instances?
        /// </summary>
        public bool AllowInfinityInstances
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.AllowInfinityInstances;
            }
            set
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                _data.Overrides |= PoolDataOverrides.Instances;
                _data.AllowInfinityInstances = value;

                TrimExcess();
            }
        }
        /// <summary>
        /// The <see cref="Pool"/>'s max capacity.
        /// </summary>
        public int MaxCapacity
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.MaxCapacity;
            }
            set
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                _data.Overrides |= PoolDataOverrides.Instances;
                _data.MaxCapacity = Mathf.Max(0, value);

                TrimExcess();
            }
        }
        /// <summary>
        /// Despawn message to be send.
        /// </summary>
        public string DespawnMessage
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.DespawnMessage;
            }
            set
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                _data.Overrides |= PoolDataOverrides.DespawnMessage;
                _data.DespawnMessage = string.IsNullOrEmpty(value) ? PoolData.DefaultDespawnMessage : value;
            }
        }
        /// <summary>
        /// Spawn message to be send.
        /// </summary>
        public string SpawnMessage
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.SpawnMessage;
            }
            set
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                _data.Overrides |= PoolDataOverrides.SpawnMessage;
                _data.SpawnMessage = string.IsNullOrEmpty(value) ? PoolData.DefaultSpawnMessage : value;
            }
        }
        /// <summary>
        /// Message type to be used.
        /// </summary>
        public PoolMessageType MessageType
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.MessageType;
            }
            set
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                _data.Overrides |= PoolDataOverrides.MessageType;
                _data.MessageType = value;
            }
        }
        /// <summary>
        /// Behaviour when trying to spawn and the <see cref="Pool"/> reached it's limit.
        /// </summary>
        public PoolSpawnFallback SpawnFallback
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.SpawnFallback;
            }
            set
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                _data.Overrides |= PoolDataOverrides.SpawnFallback;
                _data.SpawnFallback = value;
            }
        }
        /// <summary>
        /// Message option to be used.
        /// </summary>
        public SendMessageOptions MessageOption
        {
            get
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                return _data.MessageOption;
            }
            set
            {
                if (_isActive == false)
                {
                    _data.Validate(true);
                }

                _data.Overrides |= PoolDataOverrides.MessageOption;
                _data.MessageOption = value;
            }
        }

        /// <summary>
        /// Should build the component cache when instantiating an instance? It affects the memory used by the <see cref="Pool"/>.
        /// </summary>
        public bool BuildCacheOnInstantiate
        {
            get => IsAdvancedOptionSet(PoolAdvancedOptions.BuildComponentCacheOnInstantiate);
            set => SetAdvancedOption(PoolAdvancedOptions.BuildComponentCacheOnInstantiate, value);
        }
        /// <summary>
        /// Should instances be renamed on instantiate? It generates more garbage if enabled. (Editor Only)
        /// </summary>
        public bool ChangeNamesOnInstantiate
        {
            get => IsAdvancedOptionSet(PoolAdvancedOptions.ChangeNamesOnInstantiate);
            set => SetAdvancedOption(PoolAdvancedOptions.ChangeNamesOnInstantiate, value);
        }
        /// <summary>
        /// Should reset the instance scale when spawning? It affects the performance when spawning.
        /// </summary>
        public bool ResetScaleOnSpawn
        {
            get => IsAdvancedOptionSet(PoolAdvancedOptions.ResetScaleOnSpawn);
            set => SetAdvancedOption(PoolAdvancedOptions.ResetScaleOnSpawn, value);
        }
        /// <summary>
        /// Should the instance return to the container when despawning? It affects the performance when despawning.
        /// </summary>
        public bool ReturnToContainerOnDespawn
        {
            get => IsAdvancedOptionSet(PoolAdvancedOptions.ReturnToContainerOnDespawn);
            set => SetAdvancedOption(PoolAdvancedOptions.ReturnToContainerOnDespawn, value);
        }

        /// <param name="container">The container to be used. Ignored when <see cref="PoolContainerMode.Automatic"/> is set.</param>
        public Pool(PoolData data, PoolContainerMode containerMode = PoolContainerMode.Automatic, Transform container = null) : this(containerMode, container)
        {
            if (data.Prefab == null)
            {
                Debug.LogException(new InvalidDataPoolException(nameof(data)));
            }

            _data = data;
        }

        /// <param name="container">The container to be used. Ignored when <see cref="PoolContainerMode.Automatic"/> is set.</param>
        public Pool(GameObject prefab, int preloadCount = 0, PoolContainerMode containerMode = PoolContainerMode.Automatic, Transform container = null) : this(containerMode, container)
        {
            if (prefab == null)
            {
                Debug.LogException(new NullReferencePoolException());
            }

            _data = PoolData.Default;
            _data.Prefab = prefab;
            _data.PreloadCount = preloadCount;
        }

        private Pool() : this(PoolContainerMode.Automatic, null)
        {
            _data = PoolData.Default;
        }

        private Pool(PoolContainerMode containerMode, Transform container)
        {
            _data.Overrides = PoolDataOverrides.Everything;
            _containerMode = containerMode;
            _container = containerMode != PoolContainerMode.Automatic ? container : null;
        }

        /// <summary>
        /// This <see cref="Pool"/>'s <see cref="string"/> representation.
        /// </summary>
        public override string ToString()
        {
            _data.Validate(true);

            return _data.Prefab != null ? $"{_data.Prefab} (Pool)" : "Invalid Pool!";
        }

        /// <summary>
        /// Activate the <see cref="Pool"/>. Return false if already active or <see cref="Prefab"/> is null.
        /// </summary>
        public bool Activate()
        {
            if (_isActive)
            {
                return false;
            }

            _data.Validate(true);

            if (_data.Prefab == null)
            {
                Debug.LogException(new NullReferencePoolException());

                return false;
            }

#if UNITY_EDITOR
            m_UniqueID = 0;
            m_PrefabName = _data.Prefab.name;
#endif
            int instantiateCount = _data.AllowInfinityInstances ? _data.PreloadCount : Mathf.Min(_data.PreloadCount, _data.MaxCapacity);

            _defaultScale = _data.Prefab.transform.localScale;
            _availableInstancesIds = new HashSet<int>();
            _instancesComponentsCache = new List<Component>(DefaultComponentCache);
            _prefabsComponentsCache = new List<Component>(DefaultComponentCache);
            _availableInstances = new Stack<GameObject>(instantiateCount);
            _instancesComponents = new Dictionary<GameObject, Dictionary<int, Component>>(instantiateCount);

            if (_containerMode == PoolContainerMode.Automatic)
            {
                _container = new GameObject($"{_data.Prefab.name} (Pool Container)").transform;
            }

            for (int i = 0; i < instantiateCount; i++)
            {
                GameObject instance = Instantiate(_container, false);
                instance.SetActive(false);
                _availableInstances.Push(instance);
                _availableInstancesIds.Add(instance.GetInstanceID());
            }

            _isActive = true;

            return true;
        }

        /// <summary>
        /// Deactivate the <see cref="Pool"/>. Returns false if already inactive or should be deactivated with <see cref="PoolSystem.RemovePool(Pool)"/>.
        /// </summary>
        public bool Deactivate()
        {
            if (_isActive && PoolSystem.ContainsPool(this) == false)
            {
                _isActive = false;

                for (int i = 0, length = _availableInstances.Count; i < length; i++)
                {
                    GameObject gameObject = _availableInstances.Pop();

                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }
                }

                if (_containerMode == PoolContainerMode.Automatic && _container != null)
                {
                    Object.Destroy(_container.gameObject);
                }

                _availableInstancesIds = null;
                _instancesComponentsCache = null;
                _prefabsComponentsCache = null;
                _availableInstances = null;
                _instancesComponents = null;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear the current cached components for all instances.
        /// </summary>
        public void ClearComponentCache()
        {
            if (_isActive)
            {
                _instancesComponents.Clear();
            }
        }

        /// <summary>
        /// Clear the current cached components for the instance.
        /// </summary>
        public void ClearComponentCache(Component instance)
        {
            ClearComponentCache(instance.gameObject);
        }

        /// <summary>
        /// Clear the current cached components for the instance.
        /// </summary>
        public void ClearComponentCache(GameObject instance)
        {
            if (_isActive)
            {
                _instancesComponents.Remove(instance);
            }
        }

        /// <summary>
        /// Force to clear all null references in <see cref="Pool"/>.
        /// </summary>
        public void ClearNullReferences()
        {
            if (_isActive)
            {
                _availableInstancesIds.Clear();
                
                var stack = new Stack<GameObject>(_availableInstances.Count);

                while (_availableInstances.Count > 0)
                {
                    GameObject gameObject = _availableInstances.Pop();

                    if (gameObject != null)
                    {
                        stack.Push(gameObject);

                        _availableInstancesIds.Add(gameObject.GetInstanceID());
                    }
                }

                _availableInstances = stack;

                var dictionary = new Dictionary<GameObject, Dictionary<int, Component>>(_instancesComponents.Count);

                foreach (KeyValuePair<GameObject, Dictionary<int, Component>> item in _instancesComponents)
                {
                    if (item.Key != null)
                    {
                        dictionary.Add(item.Key, item.Value);
                    }
                }

                _instancesComponents = dictionary;
            }
        }

        /// <summary>
        /// Change the container mode for this <see cref="Pool"/>.
        /// </summary>
        /// <param name="container">The container to be used. Ignored when <see cref="PoolContainerMode.Automatic"/> is set.</param>
        public void SetContainerMode(PoolContainerMode containerMode, Transform container = null)
        {
            if (_isActive)
            {
                if (PoolSystem.ContainsPool(this))
                {
                    return;
                }

                if (_containerMode != containerMode)
                {
                    switch (containerMode)
                    {
                        case PoolContainerMode.Automatic:
                        {
                            _container = new GameObject($"{_data.Prefab.name} (Pool Container)").transform;

                            foreach (GameObject instance in _availableInstances)
                            {
                                instance.transform.SetParent(_container, false);
                            }

                            break;
                            
                        }
                        case PoolContainerMode.Manual:
                        {
                            if (_container != container)
                            {
                                if ((_data.AdvancedOptions & PoolAdvancedOptions.ReturnToContainerOnDespawn) == PoolAdvancedOptions.ReturnToContainerOnDespawn)
                                {
                                    foreach (GameObject instance in _availableInstances)
                                    {
                                        instance.transform.SetParent(container, false);
                                    }
                                }

                                if (_container != null)
                                {
                                    Object.Destroy(_container.gameObject);
                                }

                                _container = container;
                            }

                            break;
                        }
                    }

                    _containerMode = containerMode;
                }
                else if (_containerMode == PoolContainerMode.Manual && _container != container)
                {
                    _container = container;

                    if ((_data.AdvancedOptions & PoolAdvancedOptions.ReturnToContainerOnDespawn) == PoolAdvancedOptions.ReturnToContainerOnDespawn)
                    {
                        foreach (GameObject instance in _availableInstances)
                        {
                            instance.transform.SetParent(_container, false);
                        }
                    }
                }
            }
            else
            {
                _data.Validate(true);
                _containerMode = containerMode;
                _container = containerMode != PoolContainerMode.Automatic ? container : null;
            }
        }

        /// <summary>
        /// Change the <see cref="Pool"/> preload count.
        /// </summary>
        /// <param name="value">If active, will fill the <see cref="Pool"/>'s available instances to match the value.</param>
        /// <param name="trim">Should the <see cref="Pool"/> destroy instances until it has only the preload count?</param>
        public void SetPreloadCount(int value, bool trim = false)
        {
            if (_isActive)
            {
                _data.Overrides |= PoolDataOverrides.Instances;
                _data.PreloadCount = Mathf.Max(0, value);

                while (_availableInstances.Count < _data.PreloadCount)
                {
                    GameObject gameObject = Instantiate(_container, false);
                    gameObject.SetActive(false);
                    _availableInstances.Push(gameObject);
                    _availableInstancesIds.Add(gameObject.GetInstanceID());
                }

                if (trim)
                {
                    while (_availableInstances.Count > _data.PreloadCount)
                    {
                        GameObject gameObject = _availableInstances.Pop();

                        if (gameObject != null)
                        {
                            _availableInstancesIds.Remove(gameObject.GetInstanceID());

                            Destroy(gameObject);
                        }
                    }
                }
            }
            else
            {
                _data.Validate(true);
                _data.Overrides |= PoolDataOverrides.Instances;
                _data.PreloadCount = Mathf.Max(0, value);
            }
        }

        /// <summary>
        /// Despawn an object.
        /// </summary>
        public PoolDespawnResult Despawn(Component instance)
        {
            return Despawn(instance.gameObject);
        }

        /// <summary>
        /// Despawn an object.
        /// </summary>
        public PoolDespawnResult Despawn(GameObject instance)
        {
            int instanceId = instance.GetInstanceID();

            if (_availableInstancesIds.Contains(instanceId))
            {
                return PoolDespawnResult.Despawned;
            }
            
            if (_isActive && _instancesComponents.ContainsKey(instance))
            {
                OnDespawn?.Invoke(this, instance);

                switch (_data.MessageType)
                {
                    case PoolMessageType.SendMessage:
                    {
                        instance.SendMessage(_data.DespawnMessage, _data.MessageOption);

                        break;
                    }

                    case PoolMessageType.BroadcastMessage:
                    {
                        instance.BroadcastMessage(_data.DespawnMessage, _data.MessageOption);

                        break;
                    }
                }

                if (_data.AllowInfinityInstances || _availableInstances.Count < _data.MaxCapacity)
                {
                    instance.SetActive(false);

                    if ((_data.AdvancedOptions & PoolAdvancedOptions.ReturnToContainerOnDespawn) == PoolAdvancedOptions.ReturnToContainerOnDespawn)
                    {
                        instance.transform.SetParent(_container, false);
                    }

                    _availableInstances.Push(instance);
                    _availableInstancesIds.Add(instanceId);

                    return PoolDespawnResult.Despawned;
                }

                Destroy(instance);

                return PoolDespawnResult.Destroyed;
            }

            return PoolDespawnResult.Aborted;
        }

        /// <summary>
        /// Spawn an object. Return null if <see cref="Pool"/> is inactive.
        /// </summary>
        public GameObject Spawn(Transform parent = null, bool spawnInWorldSpace = true)
        {
            if (_isActive)
            {
                while (_availableInstances.Count > 0)
                {
                    GameObject gameObject = _availableInstances.Pop();

                    if (gameObject != null)
                    {
                        _availableInstancesIds.Remove(gameObject.GetInstanceID());

                        ProcessSpawn(gameObject, parent, spawnInWorldSpace);

                        return gameObject;
                    }
                }

                if (_data.AllowInfinityInstances || _instancesComponents.Count < _data.MaxCapacity || _data.SpawnFallback == PoolSpawnFallback.UseInstantiate)
                {
                    GameObject gameObject = Instantiate(parent, spawnInWorldSpace);
                    ProcessSpawn(gameObject);

                    return gameObject;
                }

                DebugSpawnException(new OutOfRangePoolException());

                return null;
            }

            DebugSpawnException(new InactivePoolException());

            return null;
        }

        /// <summary>
        /// Spawn an object. Return null if <see cref="Pool"/> is inactive.
        /// </summary>
        public GameObject Spawn(Vector3 position, Vector3 eulerAngles, Transform parent = null)
        {
            return Spawn(position, Quaternion.Euler(eulerAngles), parent);
        }

        /// <summary>
        /// Spawn an object. Return null if <see cref="Pool"/> is inactive.
        /// </summary>
        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (_isActive)
            {
                while (_availableInstances.Count > 0)
                {
                    GameObject gameObject = _availableInstances.Pop();

                    if (gameObject != null)
                    {
                        _availableInstancesIds.Remove(gameObject.GetInstanceID());

                        ProcessSpawn(gameObject, position, rotation, parent);

                        return gameObject;
                    }
                }

                if (_data.AllowInfinityInstances || _instancesComponents.Count < _data.MaxCapacity || _data.SpawnFallback == PoolSpawnFallback.UseInstantiate)
                {
                    GameObject gameObject = Instantiate(position, rotation, parent);
                    ProcessSpawn(gameObject);

                    return gameObject;
                }

                DebugSpawnException(new OutOfRangePoolException());

                return null;
            }

            DebugSpawnException(new InactivePoolException());

            return null;
        }

        /// <summary>
        /// Spawn an object. Return null if <see cref="Pool"/> is inactive.
        /// </summary>
        public T Spawn<T>(T prefab, Transform parent = null, bool spawnInWorldSpace = true) where T : Component
        {
            if (_isActive)
            {
                while (_availableInstances.Count > 0)
                {
                    GameObject gameObject = _availableInstances.Pop();

                    if (gameObject != null)
                    {
                        _availableInstancesIds.Remove(gameObject.GetInstanceID());

                        ProcessSpawn(gameObject, parent, spawnInWorldSpace);

                        return GetComponent<T>(gameObject, prefab.GetInstanceID());
                    }
                }

                if (_data.AllowInfinityInstances || _instancesComponents.Count < _data.MaxCapacity || _data.SpawnFallback == PoolSpawnFallback.UseInstantiate)
                {
                    T component = Instantiate(prefab, parent, spawnInWorldSpace);
                    ProcessSpawn(component.gameObject);

                    return component;
                }

                DebugSpawnException(new OutOfRangePoolException());

                return null;
            }

            DebugSpawnException(new InactivePoolException());

            return null;
        }

        /// <summary>
        /// Spawn an object. Return null if <see cref="Pool"/> is inactive.
        /// </summary>
        public T Spawn<T>(T prefab, Vector3 position, Vector3 eulerAngles, Transform parent = null) where T : Component
        {
            return Spawn(prefab, position, Quaternion.Euler(eulerAngles), parent);
        }

        /// <summary>
        /// Spawn an object. Return null if <see cref="Pool"/> is inactive.
        /// </summary>
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            if (_isActive)
            {
                while (_availableInstances.Count > 0)
                {
                    GameObject gameObject = _availableInstances.Pop();

                    if (gameObject != null)
                    {
                        _availableInstancesIds.Remove(gameObject.GetInstanceID());

                        ProcessSpawn(gameObject, position, rotation, parent);

                        return GetComponent<T>(gameObject, prefab.GetInstanceID());
                    }
                }

                if (_data.AllowInfinityInstances || _instancesComponents.Count < _data.MaxCapacity || _data.SpawnFallback == PoolSpawnFallback.UseInstantiate)
                {
                    T component = Instantiate(prefab, position, rotation, parent);
                    ProcessSpawn(component.gameObject);

                    return component;
                }

                DebugSpawnException(new OutOfRangePoolException());

                return null;
            }

            DebugSpawnException(new InactivePoolException());

            return null;
        }

        /// <summary>
        /// Get all instances available to spawn. Return null if <see cref="Pool"/> is inactive.
        /// </summary>
        public GameObject[] GetAvailableInstances()
        {
            return _isActive ? _availableInstances.ToArray() : null;
        }

        private void DebugSpawnException(SpawnPoolException exception)
        {
            if (_data.SpawnFallback == PoolSpawnFallback.DebugException)
            {
                Debug.LogException(exception, _data.Prefab);
            }
        }

        private void Destroy(GameObject instance)
        {
            OnDestroy?.Invoke(this, instance);

            _instancesComponents.Remove(instance);

            Object.Destroy(instance);
        }

        private void ProcessInstantiate(GameObject gameObject)
        {
#if UNITY_EDITOR
            if ((_data.AdvancedOptions & PoolAdvancedOptions.ChangeNamesOnInstantiate) == PoolAdvancedOptions.ChangeNamesOnInstantiate)
            {
                gameObject.name = string.Format("{0} (Pool Instance #{1})", m_PrefabName, m_UniqueID++);
            }
#endif
            _instancesComponents.Add(gameObject.transform.gameObject, null);

            if ((_data.AdvancedOptions & PoolAdvancedOptions.BuildComponentCacheOnInstantiate) == PoolAdvancedOptions.BuildComponentCacheOnInstantiate)
            {
                BuildComponentCache(gameObject);
            }

            OnInstantiate?.Invoke(this, gameObject);
        }

        private void ProcessSpawn(GameObject instance)
        {
            instance.SetActive(true);

            OnSpawn?.Invoke(this, instance);

            switch (_data.MessageType)
            {
                case PoolMessageType.SendMessage:
                {
                    instance.SendMessage(_data.SpawnMessage, _data.MessageOption);

                    break;
                }

                case PoolMessageType.BroadcastMessage:
                {
                    instance.BroadcastMessage(_data.SpawnMessage, _data.MessageOption);

                    break;
                }
            }
        }

        private void ProcessSpawn(GameObject instance, Transform parent, bool spawnInWorldSpace)
        {
            Transform transform = instance.transform;

            if ((_data.AdvancedOptions & PoolAdvancedOptions.ResetScaleOnSpawn) == PoolAdvancedOptions.ResetScaleOnSpawn)
            {
                transform.SetParent(null, false);
                transform.localScale = _defaultScale;
            }

            transform.SetParent(parent, spawnInWorldSpace);
            ProcessSpawn(instance);
        }

        private void ProcessSpawn(GameObject instance, Vector3 position, Quaternion rotation, Transform parent)
        {
            Transform transform = instance.transform;

            if ((_data.AdvancedOptions & PoolAdvancedOptions.ResetScaleOnSpawn) == PoolAdvancedOptions.ResetScaleOnSpawn)
            {
                transform.SetParent(null, false);
                transform.localScale = _defaultScale;
            }

            transform.parent = parent;
            transform.position = position;
            transform.rotation = rotation;
            ProcessSpawn(instance);
        }

        private void SetAdvancedOption(PoolAdvancedOptions option, bool value)
        {
            if (_isActive == false)
            {
                _data.Validate(true);
            }

            _data.Overrides |= PoolDataOverrides.AdvancedOptions;

            if (value)
            {
                _data.AdvancedOptions |= option;
            }
            else
            {
                _data.AdvancedOptions &= ~option;
            }
        }

        private void TrimExcess()
        {
            if (_data.AllowInfinityInstances)
            {
                return;
            }

            while (_availableInstances.Count > _data.MaxCapacity)
            {
                GameObject gameObject = _availableInstances.Pop();

                if (gameObject != null)
                {
                    _availableInstancesIds.Remove(gameObject.GetInstanceID());

                    Destroy(gameObject);
                }
            }
        }

        private bool IsAdvancedOptionSet(PoolAdvancedOptions option)
        {
            if (_isActive == false)
            {
                _data.Validate(true);
            }

            return (_data.AdvancedOptions & option) == option;
        }

        private GameObject Instantiate(Transform parent, bool instantiateInWorldSpace)
        {
            GameObject gameObject = Object.Instantiate(_data.Prefab, parent, instantiateInWorldSpace);
            ProcessInstantiate(gameObject);

            return gameObject;
        }

        private GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject gameObject = Object.Instantiate(_data.Prefab, position, rotation, parent);
            ProcessInstantiate(gameObject);

            return gameObject;
        }

        private T GetComponent<T>(GameObject instance, int componentID) where T : Component
        {
            Dictionary<int, Component> dictionary = _instancesComponents[instance] ?? BuildComponentCache(instance);

            return dictionary[componentID] as T;
        }

        private T Instantiate<T>(T prefab, Transform parent, bool instantiateInWorldSpace) where T : Component
        {
            T component = Object.Instantiate(prefab, parent, instantiateInWorldSpace);
            ProcessInstantiate(component.gameObject);

            return component;
        }

        private T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            T component = Object.Instantiate(prefab, position, rotation, parent);
            ProcessInstantiate(component.gameObject);

            return component;
        }

        private Dictionary<int, Component> BuildComponentCache(GameObject instance)
        {
            var dictionary = new Dictionary<int, Component>();

            _data.Prefab.GetComponents(_prefabsComponentsCache);
            instance.GetComponents(_instancesComponentsCache);

            for (int i = 0, length = Mathf.Min(_prefabsComponentsCache.Count, _instancesComponentsCache.Count); i < length; i++)
            {
                dictionary.Add(_prefabsComponentsCache[i].GetInstanceID(), _instancesComponentsCache[i]);
            }

            _instancesComponents[instance] = dictionary;

            return dictionary;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (_isActive)
            {
                m_AvailableCount = _availableInstances.Count;
                m_TotalCount = _instancesComponents.Count;
            }
#endif
        }
    }
}
