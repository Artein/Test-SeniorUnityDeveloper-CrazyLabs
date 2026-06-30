using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class PickupPhysicsIntegrationTests
{
    private const string PlayerTag = "Player";
    private const string PlayerLayerName = "Player";
    private const string PickupLayerName = "Pickup";

    private readonly List<IDisposable> _disposables = new();
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private FakeGameplayStateService _stateService;
    private CurrencyDefinition _coins;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId("Pre-Launch");
        _runningStateId = CreateStateId("Running");
        _stateService = new FakeGameplayStateService(_runningStateId);
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");

        Assert.That(GetRequiredLayer(PlayerLayerName), Is.GreaterThanOrEqualTo(0));
        Assert.That(GetRequiredLayer(PickupLayerName), Is.GreaterThanOrEqualTo(0));
        Assert.That(Physics.GetIgnoreLayerCollision(GetRequiredLayer(PlayerLayerName), GetRequiredLayer(PickupLayerName)), Is.False);
    }

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
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [UnityTest]
    public IEnumerator given_RunningStateAndTaggedPlayerCollider_when_PlayerEntersPickupTrigger_then_PickupIsCollectedOnce()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var controllerFixture = CreateControllerFixture(new[] { pickup });
        Initialize(controllerFixture.Controller);
        var player = CreatePlayerContact("Player Contact", new Vector3(3f, 0f, 0f), PlayerTag);

        player.Rigidbody.MovePosition(Vector3.zero);
        yield return WaitForFixedUpdatesUntil(() => !pickup.gameObject.activeSelf);

        Assert.That(controllerFixture.CurrencyStorage.GetAmount(_coins), Is.Zero);
        Assert.That(controllerFixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
        Assert.That(controllerFixture.LevelPickupState.IsAvailable(pickup), Is.False);
        Assert.That(pickup.gameObject.activeSelf, Is.False);
    }

    [UnityTest]
    public IEnumerator given_RunningStateAndResolvedPickupGrant_when_PlayerEntersPickupTrigger_then_FinalGrantIsRecorded()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var resolver = new FixedPickupCurrencyGrantResolver(4);
        var controllerFixture = CreateControllerFixture(new[] { pickup }, resolver);
        PickupCollectedEventArgs? observedEvent = null;
        controllerFixture.Controller.PickupCollected += pickupEvent => observedEvent = pickupEvent;
        Initialize(controllerFixture.Controller);
        var player = CreatePlayerContact("Player Contact", new Vector3(3f, 0f, 0f), PlayerTag);

        player.Rigidbody.MovePosition(Vector3.zero);
        yield return WaitForFixedUpdatesUntil(() => !pickup.gameObject.activeSelf);

        Assert.That(controllerFixture.CurrencyStorage.GetAmount(_coins), Is.Zero);
        Assert.That(controllerFixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(4));
        Assert.That(observedEvent.HasValue, Is.True);
        Assert.That(observedEvent.Value.BaseAmount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.FinalAmount, Is.EqualTo(4));
    }

    [UnityTest]
    public IEnumerator given_PreLaunchState_when_PlayerEntersPickupTrigger_then_PickupIsIgnored()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var controllerFixture = CreateControllerFixture(new[] { pickup });
        Initialize(controllerFixture.Controller);
        _stateService.ChangeTo(_preLaunchStateId);
        var player = CreatePlayerContact("Player Contact", new Vector3(3f, 0f, 0f), PlayerTag);

        player.Rigidbody.MovePosition(Vector3.zero);
        yield return WaitForFixedUpdates(3);

        Assert.That(controllerFixture.CurrencyStorage.GetAmount(_coins), Is.Zero);
        Assert.That(controllerFixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(controllerFixture.LevelPickupState.IsAvailable(pickup), Is.True);
        Assert.That(pickup.gameObject.activeSelf, Is.True);
    }

    [UnityTest]
    public IEnumerator given_PlayerTagOnlyOnRoot_when_ChildColliderEntersPickupTrigger_then_PickupIsIgnored()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var controllerFixture = CreateControllerFixture(new[] { pickup });
        Initialize(controllerFixture.Controller);
        var playerRoot = CreateGameObject("Player Root");
        playerRoot.tag = PlayerTag;
        playerRoot.layer = GetRequiredLayer(PlayerLayerName);
        var playerChild = CreatePlayerContact("Player Child Contact", new Vector3(3f, 0f, 0f), "Untagged");
        playerChild.GameObject.transform.SetParent(playerRoot.transform, true);

        playerChild.Rigidbody.MovePosition(Vector3.zero);
        yield return WaitForFixedUpdates(3);

        Assert.That(controllerFixture.CurrencyStorage.GetAmount(_coins), Is.Zero);
        Assert.That(controllerFixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(controllerFixture.LevelPickupState.IsAvailable(pickup), Is.True);
        Assert.That(pickup.gameObject.activeSelf, Is.True);
    }

    [UnityTest]
    public IEnumerator given_ConsumedPickupRootDisabled_when_PlayerContactsSamePositionAgain_then_NoSecondGrantOccurs()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var controllerFixture = CreateControllerFixture(new[] { pickup });
        Initialize(controllerFixture.Controller);
        var firstPlayer = CreatePlayerContact("First Player Contact", new Vector3(3f, 0f, 0f), PlayerTag);

        firstPlayer.Rigidbody.MovePosition(Vector3.zero);
        yield return WaitForFixedUpdatesUntil(() => !pickup.gameObject.activeSelf);

        var secondPlayer = CreatePlayerContact("Second Player Contact", new Vector3(3f, 0f, 0f), PlayerTag);
        secondPlayer.Rigidbody.MovePosition(Vector3.zero);
        yield return WaitForFixedUpdates(3);

        Assert.That(controllerFixture.CurrencyStorage.GetAmount(_coins), Is.Zero);
        Assert.That(controllerFixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
        Assert.That(controllerFixture.LevelPickupState.IsAvailable(pickup), Is.False);
        Assert.That(pickup.gameObject.activeSelf, Is.False);
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

    private IEnumerator WaitForFixedUpdates(int count)
    {
        for (var fixedUpdateIndex = 0; fixedUpdateIndex < count; fixedUpdateIndex += 1)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    private ControllerFixture CreateControllerFixture(
        IReadOnlyList<Pickup> pickups,
        IPickupCurrencyGrantResolver pickupCurrencyGrantResolver = null)
    {
        var levelPickupState = new LevelPickupState(new FixedLevelPickupSource(pickups));
        var storage = new CurrencyStorage(new PlayerEconomyState());
        var accumulator = new RunCurrencyAccumulator();
        var resolver = pickupCurrencyGrantResolver ?? new FixedPickupCurrencyGrantResolver();

        var controller = new PickupCollectionController(
            new FixedLevelPickupSource(pickups),
            levelPickupState,
            accumulator,
            resolver,
            _stateService,
            _runningStateId,
            _preLaunchStateId,
            PlayerTag);
        _disposables.Add(controller);

        return new ControllerFixture(controller, levelPickupState, storage, accumulator);
    }

    private void Initialize(PickupCollectionController controller)
    {
        ((IInitializable)controller).Initialize();
    }

    private Pickup CreatePickup(string objectName, int amount, Vector3 position)
    {
        var pickup = CreateGameObject(objectName).AddComponent<Pickup>();
        pickup.transform.position = position;
        pickup.gameObject.layer = GetRequiredLayer(PickupLayerName);
        var collider = pickup.gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        pickup.SetDefinitionForTests(CreatePickupDefinition(_coins, amount));
        return pickup;
    }

    private PlayerContact CreatePlayerContact(string objectName, Vector3 position, string tag)
    {
        var gameObject = CreateGameObject(objectName);
        gameObject.transform.position = position;
        gameObject.layer = GetRequiredLayer(PlayerLayerName);
        gameObject.tag = tag;
        gameObject.AddComponent<SphereCollider>();
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        return new PlayerContact(gameObject, rigidbody);
    }

    private PickupDefinition CreatePickupDefinition(CurrencyDefinition currencyDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(currencyDefinition, amount);
        return definition;
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
        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist for pickup tests.");
        return layer;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
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
            return ReferenceEquals(CurrentStateId, stateId);
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            ChangeTo(nextStateId);
            return true;
        }

        public void ChangeTo(GameplayStateId nextStateId)
        {
            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
        }
    }

    private readonly struct PlayerContact
    {
        public GameObject GameObject { get; }
        public Rigidbody Rigidbody { get; }

        public PlayerContact(GameObject gameObject, Rigidbody rigidbody)
        {
            GameObject = gameObject;
            Rigidbody = rigidbody;
        }
    }

    private sealed class FixedPickupCurrencyGrantResolver : IPickupCurrencyGrantResolver
    {
        private readonly int? _fixedFinalAmount;

        public FixedPickupCurrencyGrantResolver(int? fixedFinalAmount = null)
        {
            _fixedFinalAmount = fixedFinalAmount;
        }

        public PickupCurrencyGrantResolution Resolve(CurrencyGrant baseCurrencyGrant)
        {
            var finalAmount = _fixedFinalAmount ?? baseCurrencyGrant.Amount;

            return new PickupCurrencyGrantResolution(
                baseCurrencyGrant,
                new CurrencyGrant(baseCurrencyGrant.CurrencyDefinition, finalAmount));
        }

        public void Reset()
        {
        }
    }

    private readonly struct ControllerFixture
    {
        public PickupCollectionController Controller { get; }
        public ILevelPickupState LevelPickupState { get; }
        public ICurrencyStorage CurrencyStorage { get; }
        public IRunCurrencyAccumulator RunCurrencyAccumulator { get; }

        public ControllerFixture(
            PickupCollectionController controller,
            ILevelPickupState levelPickupState,
            ICurrencyStorage currencyStorage,
            IRunCurrencyAccumulator runCurrencyAccumulator)
        {
            Controller = controller;
            LevelPickupState = levelPickupState;
            CurrencyStorage = currencyStorage;
            RunCurrencyAccumulator = runCurrencyAccumulator;
        }
    }
}
