using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay.Economy;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class UpgradePurchaseFlowTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private GameplayStatId _statId;
    private Sprite _icon;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");
        _statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        _statId.SetValuesForTests("SlingshotLaunchPower");
        _icon = CreateIcon();
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
    public void GetLevel_UnknownUpgrade_ReturnsDefaultLevelZero()
    {
        IUpgradeProgressStorage storage = new UpgradeProgressStorage(new PlayerEconomyState());
        var definition = CreateValidDefinition();

        var level = storage.GetLevel(definition);

        Assert.That(level, Is.EqualTo(0));
    }

    [Test]
    public void SetLevel_SameStableUpgradeId_ReadsStoredLevelAcrossDefinitionInstances()
    {
        IUpgradeProgressStorage storage = new UpgradeProgressStorage(new PlayerEconomyState());
        var definition = CreateValidDefinition(stableId: "launch-power");
        var sameUpgradeId = CreateValidDefinition(stableId: "launch-power");

        storage.SetLevel(definition, 2);

        Assert.That(storage.GetLevel(sameUpgradeId), Is.EqualTo(2));
    }

    [Test]
    public void SetLevel_LevelOutsideDefinitionBounds_ThrowsAndLeavesExistingLevel()
    {
        IUpgradeProgressStorage storage = new UpgradeProgressStorage(new PlayerEconomyState());
        var definition = CreateValidDefinition(maxLevel: 3);
        storage.SetLevel(definition, 1);

        Assert.That(
            () => storage.SetLevel(definition, 4),
            Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("level"));

        Assert.That(
            () => storage.SetLevel(definition, -1),
            Throws.TypeOf<System.ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("level"));
        Assert.That(storage.GetLevel(definition), Is.EqualTo(1));
    }

    [Test]
    public void TryPurchase_ExactRequiredCurrency_SpendsCurrencyAndIncrementsLevel()
    {
        var definition = CreateValidDefinition(maxLevel: 3, costProgression: LinearProgression(100f, 300f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        var purchaseService = CreatePurchaseService(catalog, currencyStorage, progressStorage);
        currencyStorage.Grant(_coins, 100);

        var result = purchaseService.TryPurchase(definition);

        Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.Purchased));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.PreviousLevel, Is.EqualTo(0));
        Assert.That(result.NewLevel, Is.EqualTo(1));
        Assert.That(result.Cost, Is.EqualTo(100));
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(0));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(1));
    }

    [Test]
    public void TryPurchase_SaveCommitFails_ReturnsSaveFailedAndKeepsCentralState()
    {
        var definition = CreateValidDefinition(maxLevel: 3, costProgression: LinearProgression(100f, 300f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());

        var committer = new RecordingEconomyCommitter
        {
            NextResult = new EconomyPersistenceResult(
                EconomyPersistenceStatus.Failed,
                "upgrade-purchase",
                "write failed",
                new System.InvalidOperationException("write failed"))
        };
        var purchaseService = CreatePurchaseService(catalog, currencyStorage, progressStorage, committer);
        currencyStorage.Grant(_coins, 100);
        LogAssert.Expect(LogType.Error, new Regex("Economy save commit failed.*write failed"));

        var result = purchaseService.TryPurchase(definition);

        Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.SaveFailed));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.PersistenceResult.IsSuccess, Is.False);
        Assert.That(currencyStorage.GetAmount(_coins), Is.Zero);
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(1));
        Assert.That(committer.CommitReasons, Is.EqualTo(new[] { "upgrade-purchase" }));
    }

    [Test]
    public void TryPurchase_OneCurrencyLessThanRequired_DoesNotSpendOrIncrement()
    {
        var definition = CreateValidDefinition(maxLevel: 3, costProgression: LinearProgression(100f, 300f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        var purchaseService = CreatePurchaseService(catalog, currencyStorage, progressStorage);
        currencyStorage.Grant(_coins, 99);

        var result = purchaseService.TryPurchase(definition);

        Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.InsufficientCurrency));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.PreviousLevel, Is.EqualTo(0));
        Assert.That(result.NewLevel, Is.EqualTo(0));
        Assert.That(result.Cost, Is.EqualTo(100));
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(99));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(0));
    }

    [Test]
    public void TryPurchase_MaxLevel_DoesNotSpendOrIncrement()
    {
        var definition = CreateValidDefinition(maxLevel: 3, costProgression: LinearProgression(100f, 300f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        var purchaseService = CreatePurchaseService(catalog, currencyStorage, progressStorage);
        progressStorage.SetLevel(definition, 3);
        currencyStorage.Grant(_coins, 999);

        var result = purchaseService.TryPurchase(definition);

        Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.MaxLevelReached));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.PreviousLevel, Is.EqualTo(3));
        Assert.That(result.NewLevel, Is.EqualTo(3));
        Assert.That(result.Cost, Is.Null);
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(999));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(3));
    }

    [Test]
    public void TryPurchase_DefinitionOutsideCatalog_ReturnsMissingDefinitionAndDoesNotSpend()
    {
        var catalogDefinition = CreateValidDefinition(stableId: "launch-power");
        var outsideDefinition = CreateValidDefinition(stableId: "player-speed");
        var catalog = CreateCatalog(catalogDefinition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        var purchaseService = CreatePurchaseService(catalog, currencyStorage, progressStorage);
        currencyStorage.Grant(_coins, 500);

        var result = purchaseService.TryPurchase(outsideDefinition);

        Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.MissingDefinition));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.PreviousLevel, Is.EqualTo(0));
        Assert.That(result.NewLevel, Is.EqualTo(0));
        Assert.That(result.Cost, Is.Null);
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(500));
        Assert.That(progressStorage.GetLevel(outsideDefinition), Is.EqualTo(0));
    }

    [Test]
    public void TryPurchase_InvalidDefinition_ReturnsInvalidDefinitionAndDoesNotSpend()
    {
        var invalidDefinition = CreateValidDefinition(missingIcon: true);
        var catalog = CreateCatalog(invalidDefinition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        var purchaseService = CreatePurchaseService(catalog, currencyStorage, progressStorage);
        currencyStorage.Grant(_coins, 500);

        var result = purchaseService.TryPurchase(invalidDefinition);

        Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.InvalidDefinition));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.PreviousLevel, Is.EqualTo(0));
        Assert.That(result.NewLevel, Is.EqualTo(0));
        Assert.That(result.Cost, Is.Null);
        Assert.That(result.ValidationErrors, Is.Not.Empty);
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(500));
        Assert.That(progressStorage.GetLevel(invalidDefinition), Is.EqualTo(0));
    }

    [Test]
    public void TryPurchase_MultiplePurchases_AdvanceIncreasingCostsAndStoredPreview()
    {
        var definition = CreateValidDefinition(
            maxLevel: 3,
            costProgression: LinearProgression(100f, 300f),
            effectProgression: LinearProgression(1f, 1.6f));
        var catalog = CreateCatalog(definition);
        ICurrencyStorage currencyStorage = new CurrencyStorage(new PlayerEconomyState());
        var progressStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        var purchaseService = CreatePurchaseService(catalog, currencyStorage, progressStorage);
        var previewService = CreatePreviewService(catalog, currencyStorage, progressStorage);
        currencyStorage.Grant(_coins, 300);

        var firstPurchase = purchaseService.TryPurchase(definition);
        var secondPurchase = purchaseService.TryPurchase(definition);
        var preview = previewService.Build(definition);

        Assert.That(firstPurchase.Status, Is.EqualTo(UpgradePurchaseStatus.Purchased));
        Assert.That(firstPurchase.Cost, Is.EqualTo(100));
        Assert.That(secondPurchase.Status, Is.EqualTo(UpgradePurchaseStatus.Purchased));
        Assert.That(secondPurchase.Cost, Is.EqualTo(200));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(2));
        Assert.That(currencyStorage.GetAmount(_coins), Is.EqualTo(0));
        Assert.That(preview.State, Is.EqualTo(UpgradePreviewState.Unaffordable));
        Assert.That(preview.CurrentLevel, Is.EqualTo(2));
        Assert.That(preview.CurrentEffect, Is.EqualTo(1.4f).Within(0.0001f));
        Assert.That(preview.NextCost, Is.EqualTo(300));
        Assert.That(preview.IsAffordable, Is.False);
    }

    [Test]
    public void GetLevel_RecreatedStorageInstance_ReturnsDefaultLevelZero()
    {
        var definition = CreateValidDefinition();
        var firstStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        var secondStorage = new UpgradeProgressStorage(new PlayerEconomyState());
        firstStorage.SetLevel(definition, 2);

        var level = secondStorage.GetLevel(definition);

        Assert.That(level, Is.EqualTo(0));
    }

    private UpgradePurchaseService CreatePurchaseService(
        UpgradeCatalog catalog,
        ICurrencyStorage currencyStorage,
        IUpgradeProgressStorage progressStorage,
        IEconomyCommitter economyCommitter = null)
    {
        return new UpgradePurchaseService(
            catalog,
            currencyStorage,
            progressStorage,
            new UpgradeDefinitionEvaluator(),
            new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator()),
            economyCommitter ?? new NoOpEconomyCommitter());
    }

    private UpgradePreviewService CreatePreviewService(
        UpgradeCatalog catalog,
        ICurrencyStorage currencyStorage,
        IUpgradeProgressStorage progressStorage)
    {
        return new UpgradePreviewService(
            catalog,
            progressStorage,
            currencyStorage,
            new UpgradePreviewBuilder(new UpgradeDefinitionEvaluator(), new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator())));
    }

    private UpgradeCatalog CreateCatalog(params UpgradeDefinition[] definitions)
    {
        var catalog = Track(ScriptableObject.CreateInstance<UpgradeCatalog>());
        catalog.SetValuesForTests(_coins, definitions);
        return catalog;
    }

    private UpgradeDefinition CreateValidDefinition(
        string stableId = "launch-power",
        Sprite icon = null,
        bool missingIcon = false,
        GameplayStatId statId = null,
        bool missingStatId = false,
        int maxLevel = 5,
        UpgradeProgression costProgression = null,
        UpgradeProgression effectProgression = null)
    {
        var definition = Track(ScriptableObject.CreateInstance<UpgradeDefinition>());

        definition.SetValuesForTests(
            stableId,
            displayName: "Launch Power",
            description: "Launches harder.",
            missingIcon ? null : icon == null ? _icon : icon,
            missingStatId ? null : statId == null ? _statId : statId,
            maxLevel,
            costProgression ?? LinearProgression(100f, 500f),
            effectProgression ?? LinearProgression(1f, 2f),
            UpgradeOperationType.MultiplicativeFactor,
            UpgradeValueFormat.Multiplier,
            displayDecimalPlaces: 1);
        return definition;
    }

    private UpgradeProgression LinearProgression(float minimumValue, float maximumValue)
    {
        return new UpgradeProgression(
            minimumValue,
            maximumValue,
            AnimationCurve.Linear(0f, 0f, 1f, 1f),
            UpgradeProgressionRoundingMode.None,
            stepSize: 0f);
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
