using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunPreparationPresenterTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private GameplayStateId _runPreparationStateId;
    private GameplayStatId _statId;
    private Sprite _icon;
    private Sprite _coinIcon;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");
        _runPreparationStateId = CreateStateId("Run Preparation");
        _statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        _statId.SetValuesForTests("SlingshotLaunchPower");
        _icon = CreateIcon();
        _coinIcon = CreateIcon();
        _coins.SetIconForTests(_coinIcon);
    }

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void Initialize_CurrentRunPreparation_RendersBalanceAndUpgradePreviewsInCatalogOrder()
    {
        var firstDefinition = CreateDefinition(
            stableId: "launch-power",
            displayName: "Launch Power",
            maxLevel: 2,
            costProgression: LinearProgression(100f, 200f),
            effectProgression: LinearProgression(1f, 1.4f));

        var secondDefinition = CreateDefinition(
            stableId: "player-speed",
            displayName: "Player Speed",
            shortDisplayName: "SPEED",
            maxLevel: 3,
            costProgression: LinearProgression(50f, 150f),
            effectProgression: LinearProgression(1f, 1.6f));
        var catalog = CreateCatalog(firstDefinition, secondDefinition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        currencyStorage.Grant(_coins, 150);
        var view = new FakeRunPreparationView();
        var presenter = CreatePresenter(view, catalog, currencyStorage);

        ((IInitializable)presenter).Initialize();

        Assert.That(view.RenderedStates, Has.Count.EqualTo(1));
        var state = view.RenderedStates[^1];
        Assert.That(state.IsVisible, Is.True);
        Assert.That(state.CoinBalance, Is.EqualTo(150));
        Assert.That(state.CoinBalanceText, Is.EqualTo("150"));
        Assert.That(state.CurrencyIcon, Is.SameAs(_coinIcon));
        Assert.That(state.Upgrades, Has.Count.EqualTo(2));
        Assert.That(state.Upgrades[0].Definition, Is.SameAs(firstDefinition));
        Assert.That(state.Upgrades[0].StableId, Is.EqualTo("launch-power"));
        Assert.That(state.Upgrades[0].CardTitle, Is.EqualTo("POWER"));
        Assert.That(state.Upgrades[0].Icon, Is.SameAs(_icon));
        Assert.That(state.Upgrades[0].CostCurrencyIcon, Is.SameAs(_coinIcon));
        Assert.That(state.Upgrades[0].OwnedLevel, Is.EqualTo(0));
        Assert.That(state.Upgrades[0].MaxLevel, Is.EqualTo(2));
        Assert.That(state.Upgrades[0].OfferLevelText, Is.EqualTo("1"));
        Assert.That(state.Upgrades[0].OfferEffectText, Is.EqualTo("x1.2"));
        Assert.That(state.Upgrades[0].NextCost, Is.EqualTo(100));
        Assert.That(state.Upgrades[0].NextCostText, Is.EqualTo("100"));
        Assert.That(state.Upgrades[0].CanBuy, Is.True);
        Assert.That(state.Upgrades[0].ButtonText, Is.EqualTo("UPGRADE"));
        Assert.That(state.Upgrades[1].Definition, Is.SameAs(secondDefinition));
        Assert.That(state.Upgrades[1].StableId, Is.EqualTo("player-speed"));
    }

    [Test]
    public void BuyRequested_PurchaseSucceeds_RefreshesBalanceAndPreviewState()
    {
        var definition = CreateDefinition(
            maxLevel: 2,
            costProgression: LinearProgression(100f, 200f),
            effectProgression: LinearProgression(1f, 1.4f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        currencyStorage.Grant(_coins, 250);
        var view = new FakeRunPreparationView();
        var presenter = CreatePresenter(view, catalog, currencyStorage, progressStorage);
        ((IInitializable)presenter).Initialize();

        view.RequestBuy(definition);

        Assert.That(view.RenderedStates, Has.Count.EqualTo(2));
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(150));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(1));
        var upgrade = view.RenderedStates[^1].Upgrades[0];
        Assert.That(upgrade.OwnedLevel, Is.EqualTo(1));
        Assert.That(upgrade.OfferLevelText, Is.EqualTo("2"));
        Assert.That(upgrade.OfferEffectText, Is.EqualTo("x1.4"));
        Assert.That(upgrade.NextCost, Is.EqualTo(200));
        Assert.That(upgrade.NextCostText, Is.EqualTo("200"));
        Assert.That(upgrade.CanBuy, Is.False);
        Assert.That(upgrade.ButtonText, Is.EqualTo("UPGRADE"));
    }

    [Test]
    public void BuyRequested_SaveCommitFails_RefreshesFromActualCentralState()
    {
        var definition = CreateDefinition(
            maxLevel: 2,
            costProgression: LinearProgression(100f, 200f),
            effectProgression: LinearProgression(1f, 1.4f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());

        var committer = new RecordingEconomyCommitter
        {
            NextResult = new EconomyPersistenceResult(
                EconomyPersistenceStatus.Failed,
                "upgrade-purchase",
                "write failed",
                new InvalidOperationException("write failed"))
        };
        currencyStorage.Grant(_coins, 250);
        var view = new FakeRunPreparationView();
        var presenter = CreatePresenter(view, catalog, currencyStorage, progressStorage, economyCommitter: committer);
        ((IInitializable)presenter).Initialize();
        LogAssert.Expect(LogType.Error, new Regex("Economy save commit failed.*write failed"));

        view.RequestBuy(definition);

        Assert.That(view.RenderedStates, Has.Count.EqualTo(2));
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(150));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(1));
        var upgrade = view.RenderedStates[^1].Upgrades[0];
        Assert.That(upgrade.OwnedLevel, Is.EqualTo(1));
        Assert.That(upgrade.NextCost, Is.EqualTo(200));
        Assert.That(upgrade.CanBuy, Is.False);
    }

    [Test]
    public void BuyRequested_InsufficientBalance_LeavesLevelAndRendersUnaffordable()
    {
        var definition = CreateDefinition(
            maxLevel: 2,
            costProgression: LinearProgression(100f, 200f),
            effectProgression: LinearProgression(1f, 1.4f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        currencyStorage.Grant(_coins, 99);
        var view = new FakeRunPreparationView();
        var presenter = CreatePresenter(view, catalog, currencyStorage, progressStorage);
        ((IInitializable)presenter).Initialize();

        view.RequestBuy(definition);

        Assert.That(view.RenderedStates, Has.Count.EqualTo(2));
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(99));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(0));
        var upgrade = view.RenderedStates[^1].Upgrades[0];
        Assert.That(upgrade.OwnedLevel, Is.EqualTo(0));
        Assert.That(upgrade.OfferLevelText, Is.EqualTo("1"));
        Assert.That(upgrade.NextCost, Is.EqualTo(100));
        Assert.That(upgrade.CanBuy, Is.False);
        Assert.That(upgrade.IsMaxed, Is.False);
        Assert.That(upgrade.ButtonText, Is.EqualTo("UPGRADE"));
    }

    [Test]
    public void Initialize_MaxedUpgrade_RendersMaxedStateWithoutNextCost()
    {
        var definition = CreateDefinition(
            maxLevel: 2,
            costProgression: LinearProgression(100f, 200f),
            effectProgression: LinearProgression(1f, 1.4f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        progressStorage.SetLevel(definition, 2);
        currencyStorage.Grant(_coins, 999);
        var view = new FakeRunPreparationView();
        var presenter = CreatePresenter(view, catalog, currencyStorage, progressStorage);

        ((IInitializable)presenter).Initialize();

        var upgrade = view.RenderedStates[^1].Upgrades[0];
        Assert.That(upgrade.OwnedLevel, Is.EqualTo(2));
        Assert.That(upgrade.OfferLevelText, Is.EqualTo("MAX"));
        Assert.That(upgrade.IsMaxed, Is.True);
        Assert.That(upgrade.CanBuy, Is.False);
        Assert.That(upgrade.OfferEffectText, Is.EqualTo("x1.4"));
        Assert.That(upgrade.NextCost, Is.Null);
        Assert.That(upgrade.NextCostText, Is.EqualTo(string.Empty));
        Assert.That(upgrade.ButtonText, Is.EqualTo("MAX"));
    }

    [Test]
    public void ContinueRequested_CurrentRunPreparation_DelegatesToContinueCommand()
    {
        var definition = CreateDefinition();
        var catalog = CreateCatalog(definition);
        var view = new FakeRunPreparationView();
        var continueCommand = new FakeRunPreparationContinueCommand();

        var presenter = CreatePresenter(
            view,
            catalog,
            new CurrencyStorage(new PlayerEconomyState()),
            new UpgradeProgressStorage(new PlayerEconomyState()),
            continueCommand);
        ((IInitializable)presenter).Initialize();

        view.RequestContinue();

        Assert.That(continueCommand.CallCount, Is.EqualTo(1));
        Assert.That(view.RenderedStates, Has.Count.EqualTo(2));
    }

    private RunPreparationPresenter CreatePresenter(
        IRunPreparationView view,
        UpgradeCatalog catalog,
        ICurrencyStorage currencyStorage,
        IUpgradeProgressStorage progressStorage = null,
        IRunPreparationContinueCommand continueCommand = null,
        IGameplayStateService gameplayStateService = null,
        IEconomyCommitter economyCommitter = null)
    {
        progressStorage ??= new UpgradeProgressStorage(new PlayerEconomyState());
        continueCommand ??= new FakeRunPreparationContinueCommand();
        gameplayStateService ??= new FakeGameplayStateService(_runPreparationStateId);

        return new RunPreparationPresenter(
            view,
            currencyStorage,
            catalog,
            new UpgradePreviewService(
                catalog,
                progressStorage,
                currencyStorage,
                new UpgradePreviewBuilder(new UpgradeDefinitionEvaluator(), new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()))),
            new UpgradePurchaseService(
                catalog,
                currencyStorage,
                progressStorage,
                new UpgradeDefinitionEvaluator(),
                new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()),
                economyCommitter ?? new NoOpEconomyCommitter()),
            continueCommand,
            gameplayStateService,
            _runPreparationStateId);
    }

    private UpgradeCatalog CreateCatalog(params UpgradeDefinition[] definitions)
    {
        var catalog = Track(ScriptableObject.CreateInstance<UpgradeCatalog>());
        catalog.SetValuesForTests(_coins, definitions);
        return catalog;
    }

    private UpgradeDefinition CreateDefinition(
        string stableId = "launch-power",
        string displayName = "Launch Power",
        string shortDisplayName = "POWER",
        int maxLevel = 2,
        UpgradeProgression costProgression = null,
        UpgradeProgression effectProgression = null)
    {
        var definition = Track(ScriptableObject.CreateInstance<UpgradeDefinition>());

        definition.SetValuesForTests(
            stableId,
            displayName,
            "Upgrade description.",
            _icon,
            _statId,
            maxLevel,
            costProgression ?? LinearProgression(100f, 200f),
            effectProgression ?? LinearProgression(1f, 1.4f),
            UpgradeOperationType.MultiplicativeFactor,
            UpgradeValueFormat.Multiplier,
            1,
            shortDisplayName);
        return definition;
    }

    private UpgradeProgression LinearProgression(float minimumValue, float maximumValue)
    {
        return new UpgradeProgression(
            minimumValue,
            maximumValue,
            AnimationCurve.Linear(0f, 0f, 1f, 1f),
            UpgradeProgressionRoundingMode.None,
            0f);
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
        stateId.name = stateName;
        return stateId;
    }

    private Sprite CreateIcon()
    {
        var texture = Track(new Texture2D(1, 1));
        return Track(Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)));
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class FakeRunPreparationView : IRunPreparationView
    {
        public event Action<UpgradeDefinition> BuyRequested;
        public event Action ContinueRequested;

        public List<RunPreparationViewState> RenderedStates { get; } = new();

        public void Render(RunPreparationViewState state)
        {
            RenderedStates.Add(state);
        }

        public void RequestBuy(UpgradeDefinition definition)
        {
            BuyRequested?.Invoke(definition);
        }

        public void RequestContinue()
        {
            ContinueRequested?.Invoke();
        }
    }

    private sealed class FakeRunPreparationContinueCommand : IRunPreparationContinueCommand
    {
        public int CallCount { get; private set; }

        public bool TryContinue()
        {
            CallCount += 1;
            return true;
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
            return ReferenceEquals(CurrentStateId, stateId);
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            if (ReferenceEquals(CurrentStateId, nextStateId))
                return false;

            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
            return true;
        }
    }

    private sealed class RecordingEconomyCommitter : IEconomyCommitter
    {
        private readonly List<string> _commitReasons = new();

        public IReadOnlyList<string> CommitReasons => _commitReasons;

        public bool IsCommitPending { get; private set; }

        public EconomyPersistenceResult NextResult { get; set; } =
            new(EconomyPersistenceStatus.Saved, "upgrade-purchase", "saved", exception: null);

        public EconomyPersistenceResult CommitImportant(string reason)
        {
            IsCommitPending = true;
            _commitReasons.Add(reason);
            IsCommitPending = false;

            if (!NextResult.IsSuccess)
                Debug.LogError("Economy save commit failed. " + NextResult.Message);

            return NextResult;
        }

        public EconomyPersistenceResult RequestBestEffortFlush(string reason)
        {
            _commitReasons.Add(reason);
            return NextResult;
        }
    }
}
