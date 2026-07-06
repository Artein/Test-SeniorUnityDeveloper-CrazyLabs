using System;
using System.Collections;
using System.Collections.Generic;
using Game.Foundation.Physics;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Pickups;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public sealed class CoinPickupMultiplierPlayModeTests
{
    private const string PlayerBodyPartLayerName = "PlayerBodyPart";
    private const string PickupLayerName = "Pickup";

    private readonly List<IDisposable> _disposables = new();
    private readonly List<Object> _objects = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        _disposables.Clear();

        foreach (var unityObject in _objects)
        {
            Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [UnityTest]
    public IEnumerator given_RunningStateAndCoinPickupMultiplier_when_BodyPartSensorEntersPickupTrigger_then_FinalCoinGrantIsRecorded()
    {
        var coins = CreateCurrencyDefinition("Coins");
        var coinPickupMultiplierStatId = CreateStatId("CoinPickupMultiplier");
        var runningStateId = CreateStateId("Running");
        var resetStateId = CreateStateId("Run Preparation");
        var stateService = new FakeGameplayStateService(runningStateId);
        var pickup = CreatePickup("Regular Pickup", coins, 2, Vector3.zero);
        var levelPickupState = new LevelPickupState(new FixedLevelPickupSource(new[] { pickup }));
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        IRunCurrencyAccumulator runCurrencyAccumulator = new RunCurrencyAccumulator();
        var statResolver = new FixedRunGameplayStatResolver(1.5f);
        var grantResolver = new CoinPickupCurrencyGrantResolver(statResolver, coins, coinPickupMultiplierStatId);
        var sensor = CreatePickupSensorContact("Player Body Sensor", new Vector3(3f, 0f, 0f));

        var controller = new PickupCollectionController(
            sensor.Source,
            levelPickupState,
            runCurrencyAccumulator,
            new RunRewardSourceCatalog(),
            grantResolver,
            stateService,
            runningStateId,
            resetStateId);
        _disposables.Add(controller);
        ((IInitializable)controller).Initialize();

        sensor.Rigidbody.MovePosition(Vector3.zero);
        yield return WaitForFixedUpdatesUntil(() => !pickup.gameObject.activeSelf);

        Assert.That(currencyStorage.GetAmount(coins), Is.Zero);
        Assert.That(runCurrencyAccumulator.CreateSnapshot().GetAmount(coins), Is.EqualTo(3));
        Assert.That(statResolver.ResolveCallCount, Is.EqualTo(1));
    }

    private IEnumerator WaitForFixedUpdatesUntil(Func<bool> condition)
    {
        for (var fixedUpdateIndex = 0; fixedUpdateIndex < 8; fixedUpdateIndex += 1)
        {
            if (condition())
                yield break;

            yield return new WaitForFixedUpdate();
        }

        Assert.That(condition(), Is.True);
    }

    private Pickup CreatePickup(string objectName, CurrencyDefinition currencyDefinition, int amount, Vector3 position)
    {
        var pickupObject = CreateGameObject(objectName);
        pickupObject.SetActive(false);
        var pickup = pickupObject.AddComponent<Pickup>();
        pickup.transform.position = position;
        pickup.gameObject.layer = GetRequiredLayer(PickupLayerName);
        var triggerObject = CreateGameObject($"{objectName} Visual Trigger");
        triggerObject.transform.SetParent(pickup.transform, false);
        triggerObject.layer = GetRequiredLayer(PickupLayerName);
        var notifier = triggerObject.AddComponent<TriggerNotifier>();
        var collider = triggerObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        pickup.SetDefinitionForTests(CreatePickupDefinition(currencyDefinition, amount));
        pickup.SetTriggerNotifierForTests(notifier);
        pickupObject.SetActive(true);
        return pickup;
    }

    private SensorContact CreatePickupSensorContact(string objectName, Vector3 position)
    {
        var root = CreateGameObject($"{objectName} Root");
        root.transform.position = position;
        var rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        var source = root.AddComponent<PickupSensorSource>();
        var sensorObject = CreateGameObject(objectName);
        sensorObject.transform.SetParent(root.transform, false);
        sensorObject.layer = GetRequiredLayer(PlayerBodyPartLayerName);
        var collider = sensorObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        var notifier = sensorObject.AddComponent<TriggerNotifier>();
        source.SetSensorEntriesForTests(notifier);

        return new SensorContact(rigidbody, source);
    }

    private PickupDefinition CreatePickupDefinition(CurrencyDefinition currencyDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(currencyDefinition, amount);
        return definition;
    }

    private CurrencyDefinition CreateCurrencyDefinition(string objectName)
    {
        var currencyDefinition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        currencyDefinition.name = objectName;
        currencyDefinition.SetSaveIdForTests("currency-" + objectName.ToLowerInvariant());
        return currencyDefinition;
    }

    private GameplayStatId CreateStatId(string id)
    {
        var statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        statId.SetValuesForTests(id);
        return statId;
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
        stateId.name = stateName;
        return stateId;
    }

    private GameObject CreateGameObject(string objectName)
    {
        return Track(new GameObject(objectName));
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);
        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist for coin pickup multiplier tests.");
        return layer;
    }

    private T Track<T>(T value)
        where T : Object
    {
        _objects.Add(value);
        return value;
    }

    private readonly struct SensorContact
    {
        public Rigidbody Rigidbody { get; }
        public PickupSensorSource Source { get; }

        public SensorContact(Rigidbody rigidbody, PickupSensorSource source)
        {
            Rigidbody = rigidbody;
            Source = source;
        }
    }

    private sealed class FixedRunGameplayStatResolver : IRunGameplayStatResolver
    {
        private readonly float _resolvedValue;

        public int ResolveCallCount { get; private set; }

        public FixedRunGameplayStatResolver(float resolvedValue)
        {
            _resolvedValue = resolvedValue;
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            ResolveCallCount += 1;
            return _resolvedValue;
        }
    }

    private sealed class FakeGameplayStateService : IGameplayStateService
    {
        public GameplayStateId CurrentStateId { get; private set; }

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        public FakeGameplayStateService(GameplayStateId currentStateId)
        {
            CurrentStateId = currentStateId;
        }

        public bool IsCurrent(GameplayStateId stateId)
        {
            return CurrentStateId == stateId;
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
            return true;
        }
    }
}
