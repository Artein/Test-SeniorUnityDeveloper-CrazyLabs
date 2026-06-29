using System;
using System.Collections.Generic;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class PickupCollectionControllerTests
{
    private readonly List<IDisposable> _disposables = new();
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private FakeGameplayStateService _stateService;
    private ResourceDefinition _coins;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId("Pre-Launch");
        _runningStateId = CreateStateId("Running");
        _stateService = new FakeGameplayStateService(_runningStateId);
        _coins = Track(ScriptableObject.CreateInstance<ResourceDefinition>());
        _coins.name = "Coins";
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

    [Test]
    public void TriggerEntered_RunningPlayerCollider_GrantsResourcesDisablesPickupAndPublishesEvent()
    {
        var pickup = CreatePickup("Regular Pickup", 3, new Vector3(2f, 0f, 5f));
        var fixture = CreateControllerFixture(new[] { pickup });
        PickupCollectedEventArgs? observedEvent = null;
        fixture.Controller.PickupCollected += pickupEvent => observedEvent = pickupEvent;
        Initialize(fixture.Controller);

        pickup.RaiseTriggerEnteredForTests(CreatePlayerCollider());

        Assert.That(fixture.ResourceStorage.GetAmount(_coins), Is.EqualTo(3));
        Assert.That(fixture.RunResourceAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
        Assert.That(pickup.gameObject.activeSelf, Is.False);
        Assert.That(observedEvent.HasValue, Is.True);
        Assert.That(observedEvent.Value.ResourceDefinition, Is.SameAs(_coins));
        Assert.That(observedEvent.Value.Amount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.Position, Is.EqualTo(new Vector3(2f, 0f, 5f)));
    }

    [Test]
    public void TriggerEntered_OutsideRunningState_IgnoresContact()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var fixture = CreateControllerFixture(new[] { pickup });
        var eventCount = 0;
        fixture.Controller.PickupCollected += _ => eventCount += 1;
        Initialize(fixture.Controller);
        _stateService.ChangeTo(_preLaunchStateId);

        pickup.RaiseTriggerEnteredForTests(CreatePlayerCollider());

        Assert.That(fixture.ResourceStorage.GetAmount(_coins), Is.Zero);
        Assert.That(fixture.RunResourceAccumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(fixture.LevelPickupState.IsAvailable(pickup), Is.True);
        Assert.That(pickup.gameObject.activeSelf, Is.True);
        Assert.That(eventCount, Is.Zero);
    }

    [Test]
    public void TriggerEntered_ColliderWithoutPlayerTag_IgnoresContact()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var fixture = CreateControllerFixture(new[] { pickup });
        var eventCount = 0;
        fixture.Controller.PickupCollected += _ => eventCount += 1;
        Initialize(fixture.Controller);

        pickup.RaiseTriggerEnteredForTests(CreateCollider("Obstacle Contact"));

        Assert.That(fixture.ResourceStorage.GetAmount(_coins), Is.Zero);
        Assert.That(fixture.RunResourceAccumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(fixture.LevelPickupState.IsAvailable(pickup), Is.True);
        Assert.That(pickup.gameObject.activeSelf, Is.True);
        Assert.That(eventCount, Is.Zero);
    }

    [Test]
    public void TriggerEntered_AcceptedPickup_ConsumesStateBeforeGrantingStorage()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        LevelPickupState observedState = null;

        var resourceStorage = new RecordingResourceStorage
        {
            BeforeGrant = (_, _) => Assert.That(observedState.IsAvailable(pickup), Is.False)
        };
        var fixture = CreateControllerFixture(new[] { pickup }, resourceStorage);
        observedState = fixture.LevelPickupState;
        Initialize(fixture.Controller);

        pickup.RaiseTriggerEnteredForTests(CreatePlayerCollider());

        Assert.That(resourceStorage.GetAmount(_coins), Is.EqualTo(3));
    }

    [Test]
    public void TriggerEntered_DuplicateContact_GrantsOnlyOnce()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var fixture = CreateControllerFixture(new[] { pickup });
        var eventCount = 0;
        fixture.Controller.PickupCollected += _ => eventCount += 1;
        Initialize(fixture.Controller);
        var playerCollider = CreatePlayerCollider();

        pickup.RaiseTriggerEnteredForTests(playerCollider);
        pickup.RaiseTriggerEnteredForTests(playerCollider);

        Assert.That(fixture.ResourceStorage.GetAmount(_coins), Is.EqualTo(3));
        Assert.That(fixture.RunResourceAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
        Assert.That(eventCount, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_AfterInitialize_UnsubscribesFromPickupContactEvents()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var fixture = CreateControllerFixture(new[] { pickup });
        var eventCount = 0;
        fixture.Controller.PickupCollected += _ => eventCount += 1;
        Initialize(fixture.Controller);
        ((IDisposable)fixture.Controller).Dispose();

        pickup.RaiseTriggerEnteredForTests(CreatePlayerCollider());

        Assert.That(fixture.ResourceStorage.GetAmount(_coins), Is.Zero);
        Assert.That(fixture.RunResourceAccumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(fixture.LevelPickupState.IsAvailable(pickup), Is.True);
        Assert.That(eventCount, Is.Zero);
    }

    private ControllerFixture CreateControllerFixture(
        IReadOnlyList<Pickup> pickups,
        IResourceStorage resourceStorage = null,
        IRunResourceAccumulator runResourceAccumulator = null)
    {
        var levelPickupState = new LevelPickupState(pickups);
        var storage = resourceStorage ?? new ResourceStorage();
        var accumulator = runResourceAccumulator ?? new RunResourceAccumulator();
        var controller = new PickupCollectionController(pickups, levelPickupState, storage, accumulator, _stateService, _runningStateId, "Player");
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
        pickup.SetDefinitionForTests(CreatePickupDefinition(_coins, amount));
        return pickup;
    }

    private PickupDefinition CreatePickupDefinition(ResourceDefinition resourceDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(resourceDefinition, amount);
        return definition;
    }

    private Collider CreatePlayerCollider()
    {
        var collider = CreateCollider("Player Contact");
        collider.gameObject.tag = "Player";
        return collider;
    }

    private Collider CreateCollider(string objectName)
    {
        return CreateGameObject(objectName).AddComponent<SphereCollider>();
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

    private sealed class RecordingResourceStorage : IResourceStorage
    {
        private readonly Dictionary<ResourceDefinition, int> _amountsByResource = new();

        public Action<ResourceDefinition, int> BeforeGrant { get; set; }

        public void Grant(ResourceDefinition resourceDefinition, int amount)
        {
            BeforeGrant?.Invoke(resourceDefinition, amount);
            var currentAmount = GetAmount(resourceDefinition);
            _amountsByResource[resourceDefinition] = currentAmount + amount;
        }

        public int GetAmount(ResourceDefinition resourceDefinition)
        {
            return resourceDefinition == null ? 0 : _amountsByResource.GetValueOrDefault(resourceDefinition, 0);
        }
    }

    private readonly struct ControllerFixture
    {
        public PickupCollectionController Controller { get; }
        public LevelPickupState LevelPickupState { get; }
        public IResourceStorage ResourceStorage { get; }
        public IRunResourceAccumulator RunResourceAccumulator { get; }

        public ControllerFixture(
            PickupCollectionController controller,
            LevelPickupState levelPickupState,
            IResourceStorage resourceStorage,
            IRunResourceAccumulator runResourceAccumulator)
        {
            Controller = controller;
            LevelPickupState = levelPickupState;
            ResourceStorage = resourceStorage;
            RunResourceAccumulator = runResourceAccumulator;
        }
    }
}
