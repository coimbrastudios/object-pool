# Pool System

Collection of scripts to easily pool Unity GameObjects.

* [`Pool`](#pool)
* [`PoolData`](#pooldata)
* [`PoolDataAsset`](#pooldataasset)
* [`PoolDataCollection`](#pooldatacollection)
* [`PoolSystem`](#poolsystem)

## `Pool`

The actual pool for some object. You can expose it in the inspector to setup all the data needed.

##### Example

```c#
[SerializeField] private Pool _pool;

private void Awake()
{
    _pool.Activate();
}

private IEnumerator Start()
{
    while (true)
    {
        yield return new WaitForSeconds(1);

        _pool.Spawn();
    }
}
```

## `PoolData`

The data structure required for any [`Pool`](#pool).

##### Fields

_`Asset`_: the asset being used as base to override its values.

_`Overrides`_: which values of the `Asset` should be overriden.

_`Prefab`_: the GameObject to be pooled.

_`PreloadCount`_: amount of instances to create when activating the `Pool`.

_`MaxCapacity`_: max number of instances to be stored by the `Pool`.

_`AllowInfinityInstances`_: if true, `MaxCapacity` will be ignored.

_`SpawnFallback`_: what should happen when trying to spawn but there is no available instance and the `Pool` already reached its max capacity.

_`AdvancedOptions`_: some advanced settings for the `Pool`, changing those may increase or decrease the performance according to your use case.

_`MessageType`_: the type of message to be send when spawning or despawning the objects.

_`MessageOption`_: whether or not it should require a receiver for the message.

_`SpawnMessage`_: the message to be send when spawning the objects.

_`DespawnMessage`_: the message to be send when despawning the objects.

## `PoolDataAsset`

Asset to hold reusable [`PoolData`](#pooldata).

You can create a new instance through the `Assets/Create/Pool Data` menu item.

## `PoolDataCollection`

Asset to hold a reusable collection of [`PoolData`](#pooldata).

You can create a new instance through the `Assets/Create/Pool Data Collection` menu item.

## `PoolSystem`

The core class of the pooling system where you can add any created [`Pool`](#pool) in runtime or edit-time.

When you add a [`Pool`](#pool) in the PoolSystem you can spawn and despawn objects using the same Prefab that the [`Pool`](#pool) is using, allowing you to easily share the same [`Pool`](#pool) between different objects.

To configure the PoolSystem in edit-time, just go to `Edit/Project Setting...` and select `Pool System`, there you can edit:

* Add any [`Pool`](#pool) that should persist between scene loads by default.
* Add any [`Pool`](#pool) that should only be loaded for some scenes.

If you try to spawn or despawn an object with no [`Pool`](#pool) associated to it, is will automatically use Unity's own Instantiate and Destroy API. This way, you can safely use the PoolSystem for any operation related to any GameObject and its Components.

**WARNING**: if you want to actually remove a Component from a GameObject you should still use `Object.Destroy` (or the GameObject extension method `RemoveComponent`), as using `PoolSystem.Despawn` will cause the GameObject itself to be despawned or destroyed.

##### Example

```c#
[SerializeField] private GameObject _prefab;

private IEnumerator Start()
{
    while (true)
    {
        yield return new WaitForSeconds(1);

        PoolSystem.Spawn(_prefab);
    }
}
```
```c#
[SerializeField] private Rigidbody _prefab;

private IEnumerator Start()
{
    while (true)
    {
        yield return new WaitForSeconds(1);

        PoolSystem.Spawn(_prefab);
    }
}
```
