using System;
using System.Collections.Generic;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Economy;
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
    public void PickupContacted_RunningState_RecordsRunCurrencyDisablesPickupAndPublishesEvent()
    {
        var pickup = CreatePickup("Regular Pickup", 3, new Vector3(2f, 0f, 5f));
        var fixture = CreateControllerFixture(new[] { pickup });
        PickupCollectedEventArgs? observedEvent = null;
        fixture.Controller.PickupCollected += pickupEvent => observedEvent = pickupEvent;
        Initialize(fixture.Controller);

        fixture.PickupContactSource.RaisePickupContact(pickup);

        Assert.That(fixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
        Assert.That(pickup.gameObject.activeSelf, Is.False);
        Assert.That(observedEvent.HasValue, Is.True);
        Assert.That(observedEvent.Value.CurrencyDefinition, Is.SameAs(_coins));
        Assert.That(observedEvent.Value.Amount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.BaseAmount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.FinalAmount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.BaseCurrencyGrant.CurrencyDefinition, Is.SameAs(_coins));
        Assert.That(observedEvent.Value.BaseCurrencyGrant.Amount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.FinalCurrencyGrant.CurrencyDefinition, Is.SameAs(_coins));
        Assert.That(observedEvent.Value.FinalCurrencyGrant.Amount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.Position, Is.EqualTo(new Vector3(2f, 0f, 5f)));
    }

    [Test]
    public void PickupContacted_ResolvedPickupGrant_WritesFinalAmountsAndPublishesBaseAndFinalEvent()
    {
        var pickup = CreatePickup("Regular Pickup", 3, new Vector3(2f, 0f, 5f));

        var resolver = new FakePickupCurrencyGrantResolver
        {
            NextFinalAmount = 4
        };
        var fixture = CreateControllerFixture(new[] { pickup }, pickupCurrencyGrantResolver: resolver);
        PickupCollectedEventArgs? observedEvent = null;
        fixture.Controller.PickupCollected += pickupEvent => observedEvent = pickupEvent;
        Initialize(fixture.Controller);

        fixture.PickupContactSource.RaisePickupContact(pickup);

        Assert.That(fixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(4));
        Assert.That(fixture.RunCurrencyAccumulator.CreateSnapshot().Amounts, Has.Length.EqualTo(1));
        Assert.That(observedEvent.HasValue, Is.True);
        Assert.That(observedEvent.Value.CurrencyDefinition, Is.SameAs(_coins));
        Assert.That(observedEvent.Value.Amount, Is.EqualTo(4));
        Assert.That(observedEvent.Value.BaseAmount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.FinalAmount, Is.EqualTo(4));
        Assert.That(observedEvent.Value.CurrencyGrant.Amount, Is.EqualTo(4));
        Assert.That(observedEvent.Value.BaseCurrencyGrant.Amount, Is.EqualTo(3));
        Assert.That(observedEvent.Value.FinalCurrencyGrant.Amount, Is.EqualTo(4));
    }

    [Test]
    public void PickupContacted_OutsideRunningState_IgnoresContact()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var fixture = CreateControllerFixture(new[] { pickup });
        var eventCount = 0;
        fixture.Controller.PickupCollected += _ => eventCount += 1;
        Initialize(fixture.Controller);
        _stateService.ChangeTo(_preLaunchStateId);

        fixture.PickupContactSource.RaisePickupContact(pickup);

        Assert.That(fixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(fixture.LevelPickupState.IsAvailable(pickup), Is.True);
        Assert.That(pickup.gameObject.activeSelf, Is.True);
        Assert.That(eventCount, Is.Zero);
    }

    [Test]
    public void PickupContacted_SensorContactWithoutPlayerTag_CollectsPickup()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var fixture = CreateControllerFixture(new[] { pickup });
        var eventCount = 0;
        fixture.Controller.PickupCollected += _ => eventCount += 1;
        Initialize(fixture.Controller);

        fixture.PickupContactSource.RaisePickupContact(pickup, "Untagged Sensor Contact");

        Assert.That(fixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
        Assert.That(fixture.LevelPickupState.IsAvailable(pickup), Is.False);
        Assert.That(pickup.gameObject.activeSelf, Is.False);
        Assert.That(eventCount, Is.EqualTo(1));
    }

    [Test]
    public void PickupContacted_AcceptedPickup_ConsumesStateBeforeRecordingRunCurrency()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        ILevelPickupState observedState = null;

        var runCurrencyAccumulator = new RecordingRunCurrencyAccumulator
        {
            BeforeGrant = (_, _) => Assert.That(observedState.IsAvailable(pickup), Is.False)
        };
        var fixture = CreateControllerFixture(new[] { pickup }, runCurrencyAccumulator: runCurrencyAccumulator);
        observedState = fixture.LevelPickupState;
        Initialize(fixture.Controller);

        fixture.PickupContactSource.RaisePickupContact(pickup);

        Assert.That(runCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
    }

    [Test]
    public void PickupContacted_DuplicateContact_GrantsOnlyOnce()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var fixture = CreateControllerFixture(new[] { pickup });
        var eventCount = 0;
        fixture.Controller.PickupCollected += _ => eventCount += 1;
        Initialize(fixture.Controller);

        fixture.PickupContactSource.RaisePickupContact(pickup);
        fixture.PickupContactSource.RaisePickupContact(pickup);

        Assert.That(fixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(3));
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

        fixture.PickupContactSource.RaisePickupContact(pickup);

        Assert.That(fixture.RunCurrencyAccumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
        Assert.That(fixture.LevelPickupState.IsAvailable(pickup), Is.True);
        Assert.That(eventCount, Is.Zero);
    }

    [Test]
    public void GameplayStateChanged_ResetState_ResetsPickupCurrencyGrantResolver()
    {
        var pickup = CreatePickup("Regular Pickup", 3, Vector3.zero);
        var resolver = new FakePickupCurrencyGrantResolver();
        var fixture = CreateControllerFixture(new[] { pickup }, pickupCurrencyGrantResolver: resolver);
        Initialize(fixture.Controller);

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);

        Assert.That(resolver.ResetCallCount, Is.EqualTo(1));
    }

    private ControllerFixture CreateControllerFixture(
        IReadOnlyList<Pickup> pickups,
        IRunCurrencyAccumulator runCurrencyAccumulator = null,
        IPickupCurrencyGrantResolver pickupCurrencyGrantResolver = null)
    {
        var levelPickupState = new LevelPickupState(new FixedLevelPickupSource(pickups));
        var accumulator = runCurrencyAccumulator ?? new RunCurrencyAccumulator();
        var resolver = pickupCurrencyGrantResolver ?? new FakePickupCurrencyGrantResolver();
        var pickupContactSource = new FakePickupContactSource(CreateCollider);

        var controller = new PickupCollectionController(
            pickupContactSource,
            levelPickupState,
            accumulator,
            new RunRewardSourceCatalog(),
            resolver,
            _stateService,
            _runningStateId,
            _preLaunchStateId);
        _disposables.Add(controller);
        return new ControllerFixture(controller, pickupContactSource, levelPickupState, accumulator);
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

    private PickupDefinition CreatePickupDefinition(CurrencyDefinition currencyDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(currencyDefinition, amount);
        return definition;
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

    private sealed class RecordingRunCurrencyAccumulator : IRunCurrencyAccumulator
    {
        private readonly Dictionary<CurrencyDefinition, int> _amountsByCurrency = new();

        public Action<CurrencyDefinition, int> BeforeGrant { get; set; }

        public void Grant(CurrencyDefinition currencyDefinition, int amount)
        {
            BeforeGrant?.Invoke(currencyDefinition, amount);
            var currentAmount = _amountsByCurrency.GetValueOrDefault(currencyDefinition, 0);
            _amountsByCurrency[currencyDefinition] = currentAmount + amount;
        }

        public void Grant(RunRewardSource source, CurrencyDefinition currencyDefinition, int amount)
        {
            Grant(currencyDefinition, amount);
        }

        public void Reset()
        {
            _amountsByCurrency.Clear();
        }

        public RunCurrencySnapshot CreateSnapshot()
        {
            var amounts = new List<RunCurrencyAmount>();

            foreach (var pair in _amountsByCurrency)
            {
                amounts.Add(new RunCurrencyAmount(pair.Key, pair.Value));
            }

            return new RunCurrencySnapshot(amounts);
        }
    }

    private sealed class FakePickupCurrencyGrantResolver : IPickupCurrencyGrantResolver
    {
        public int? NextFinalAmount { get; set; }
        public int ResetCallCount { get; private set; }

        public PickupCurrencyGrantResolution Resolve(CurrencyGrant baseCurrencyGrant)
        {
            var finalAmount = NextFinalAmount ?? baseCurrencyGrant.Amount;

            return new PickupCurrencyGrantResolution(
                baseCurrencyGrant,
                new CurrencyGrant(baseCurrencyGrant.CurrencyDefinition, finalAmount));
        }

        public void Reset()
        {
            ResetCallCount += 1;
        }
    }

    private sealed class FakePickupContactSource : IPickupContactSource
    {
        private readonly Func<string, Collider> _colliderFactory;

        public event Action<PickupContact> PickupContacted;

        public FakePickupContactSource(Func<string, Collider> colliderFactory)
        {
            _colliderFactory = colliderFactory ?? throw new ArgumentNullException(nameof(colliderFactory));
        }

        public void RaisePickupContact(Pickup pickup, string contactObjectName = "Sensor Contact")
        {
            PickupContacted?.Invoke(new PickupContact(pickup, _colliderFactory(contactObjectName), null, contactObjectName));
        }
    }

    private readonly struct ControllerFixture
    {
        public PickupCollectionController Controller { get; }
        public FakePickupContactSource PickupContactSource { get; }
        public ILevelPickupState LevelPickupState { get; }
        public IRunCurrencyAccumulator RunCurrencyAccumulator { get; }

        public ControllerFixture(
            PickupCollectionController controller,
            FakePickupContactSource pickupContactSource,
            ILevelPickupState levelPickupState,
            IRunCurrencyAccumulator runCurrencyAccumulator)
        {
            Controller = controller;
            PickupContactSource = pickupContactSource;
            LevelPickupState = levelPickupState;
            RunCurrencyAccumulator = runCurrencyAccumulator;
        }
    }
}
