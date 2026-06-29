using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Economy;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class GameplayStatResolverTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private Sprite _icon;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
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
    public void CreateSnapshot_CurrentUpgradeLevels_AddsExpectedMultiplicativeModifiers()
    {
        var launchPower = CreateStat("SlingshotLaunchPower");
        var playerMaxSpeed = CreateStat("PlayerMaxSpeed");
        var steeringResponsiveness = CreateStat("PlayerSteeringResponsiveness");
        var coinPickupMultiplier = CreateStat("CoinPickupMultiplier");
        var launchPowerUpgrade = CreateDefinition("launch-power", launchPower, 10, LinearProgression(1f, 2f));
        var playerMaxSpeedUpgrade = CreateDefinition("player-max-speed", playerMaxSpeed, 10, LinearProgression(1f, 1.8f));
        var steeringUpgrade = CreateDefinition("steering-responsiveness", steeringResponsiveness, 10, LinearProgression(1f, 2.2f));
        var coinMultiplierUpgrade = CreateDefinition("coin-pickup-multiplier", coinPickupMultiplier, 10, LinearProgression(1f, 10f));
        var catalog = CreateCatalog(launchPowerUpgrade, playerMaxSpeedUpgrade, steeringUpgrade, coinMultiplierUpgrade);
        var progressStorage = new UpgradeProgressStorage();
        progressStorage.SetLevel(launchPowerUpgrade, 5);
        progressStorage.SetLevel(playerMaxSpeedUpgrade, 5);
        progressStorage.SetLevel(steeringUpgrade, 5);
        progressStorage.SetLevel(coinMultiplierUpgrade, 5);
        var factory = CreateSnapshotFactory(catalog, progressStorage);

        var snapshot = factory.CreateSnapshot();

        Assert.That(snapshot.Modifiers, Has.Count.EqualTo(4));
        AssertSnapshotModifier(snapshot, launchPower, GameplayStatModifierOperation.MultiplicativeFactor, 1.5f);
        AssertSnapshotModifier(snapshot, playerMaxSpeed, GameplayStatModifierOperation.MultiplicativeFactor, 1.4f);
        AssertSnapshotModifier(snapshot, steeringResponsiveness, GameplayStatModifierOperation.MultiplicativeFactor, 1.6f);
        AssertSnapshotModifier(snapshot, coinPickupMultiplier, GameplayStatModifierOperation.MultiplicativeFactor, 5.5f);
    }

    [Test]
    public void CreateSnapshot_LaterProgressChanges_DoesNotMutateSnapshot()
    {
        var launchPower = CreateStat("SlingshotLaunchPower");
        var upgrade = CreateDefinition("launch-power", launchPower, 10, LinearProgression(1f, 2f));
        var catalog = CreateCatalog(upgrade);
        var progressStorage = new UpgradeProgressStorage();
        progressStorage.SetLevel(upgrade, 2);
        var factory = CreateSnapshotFactory(catalog, progressStorage);

        var snapshot = factory.CreateSnapshot();
        progressStorage.SetLevel(upgrade, 8);
        var laterSnapshot = factory.CreateSnapshot();

        AssertSnapshotModifier(snapshot, launchPower, GameplayStatModifierOperation.MultiplicativeFactor, 1.2f);
        AssertSnapshotModifier(laterSnapshot, launchPower, GameplayStatModifierOperation.MultiplicativeFactor, 1.8f);
    }

    [Test]
    public void SetSnapshot_AssignedSnapshot_ExposesActiveSnapshotThroughProvider()
    {
        var statId = CreateStat("SlingshotLaunchPower");
        var holder = new RunModifierSnapshotHolder();

        var snapshot = new RunModifierSnapshot(new[]
        {
            new GameplayStatModifier(statId, GameplayStatModifierOperation.FlatAdd, 2f)
        });

        holder.SetSnapshot(snapshot);
        IRunModifierSnapshotProvider provider = holder;

        Assert.That(provider.CurrentSnapshot, Is.SameAs(snapshot));
        Assert.That(provider.CurrentSnapshot.Modifiers, Has.Count.EqualTo(1));
    }

    [Test]
    public void Resolve_NoMatchingModifiers_ReturnsBaseValue()
    {
        var statId = CreateStat("SlingshotLaunchPower");

        var resolver = new GameplayStatResolver(new IGameplayStatModifierSource[]
        {
            new RunModifierSnapshot(Array.Empty<GameplayStatModifier>())
        });

        var value = resolver.Resolve(statId, 12.5f);

        Assert.That(value, Is.EqualTo(12.5f));
    }

    [Test]
    public void Resolve_MultipleOperations_AppliesRequiredOperationOrder()
    {
        var statId = CreateStat("SlingshotLaunchPower");

        var source = new FakeModifierSource(
            new GameplayStatModifier(statId, GameplayStatModifierOperation.ClampMax, 35f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 2f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.ClampMin, 40f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.AdditivePercent, 0.5f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.FlatAdd, 2f));
        var resolver = new GameplayStatResolver(new IGameplayStatModifierSource[] { source });

        var value = resolver.Resolve(statId, 10f);

        Assert.That(value, Is.EqualTo(35f).Within(0.0001f));
    }

    [Test]
    public void Resolve_MixedStatIds_AppliesOnlyRequestedStatId()
    {
        var launchPower = CreateStat("SlingshotLaunchPower");
        var playerMaxSpeed = CreateStat("PlayerMaxSpeed");

        var source = new FakeModifierSource(
            new GameplayStatModifier(playerMaxSpeed, GameplayStatModifierOperation.FlatAdd, 999f),
            new GameplayStatModifier(launchPower, GameplayStatModifierOperation.MultiplicativeFactor, 2f));
        var resolver = new GameplayStatResolver(new IGameplayStatModifierSource[] { source });

        var value = resolver.Resolve(launchPower, 10f);

        Assert.That(value, Is.EqualTo(20f).Within(0.0001f));
    }

    [Test]
    public void Resolve_SnapshotAndLiveSources_CombinesSourcesDeterministically()
    {
        var statId = CreateStat("PlayerMaxSpeed");

        var snapshot = new RunModifierSnapshot(new[]
        {
            new GameplayStatModifier(statId, GameplayStatModifierOperation.FlatAdd, 2f)
        });

        var liveSource = new FakeModifierSource(
            new GameplayStatModifier(statId, GameplayStatModifierOperation.AdditivePercent, 0.5f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 2f));
        var resolver = new GameplayStatResolver(new IGameplayStatModifierSource[] { snapshot, liveSource });

        var value = resolver.Resolve(statId, 10f);

        Assert.That(value, Is.EqualTo(36f).Within(0.0001f));
    }

    [Test]
    public void Resolve_IdenticalInputs_ReturnsSameValue()
    {
        var statId = CreateStat("CoinPickupMultiplier");

        var source = new FakeModifierSource(
            new GameplayStatModifier(statId, GameplayStatModifierOperation.FlatAdd, 1f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.AdditivePercent, 0.25f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 3f));
        var resolver = new GameplayStatResolver(new IGameplayStatModifierSource[] { source });

        var firstValue = resolver.Resolve(statId, 4f);
        var secondValue = resolver.Resolve(statId, 4f);

        Assert.That(firstValue, Is.EqualTo(secondValue).Within(0.0001f));
    }

    [Test]
    public void Resolve_RepeatedCalls_DoNotAllocateManagedMemoryAfterWarmup()
    {
        var statId = CreateStat("PlayerMaxSpeed");

        var source = new FakeModifierSource(
            new GameplayStatModifier(statId, GameplayStatModifierOperation.FlatAdd, 2f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.AdditivePercent, 0.5f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 2f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.ClampMin, 20f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.ClampMax, 40f));
        var resolver = new GameplayStatResolver(new IGameplayStatModifierSource[] { source });

        _ = resolver.Resolve(statId, 10f);

        var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();

        var value = 0f;

        for (var iteration = 0; iteration < 32; iteration++)
        {
            value = resolver.Resolve(statId, 10f);
        }

        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBytesBefore;

        Assert.That(value, Is.EqualTo(36f).Within(0.0001f));
        Assert.That(allocatedBytes, Is.Zero);
    }

    private RunModifierSnapshotFactory CreateSnapshotFactory(
        UpgradeCatalog catalog,
        IUpgradeProgressStorage progressStorage)
    {
        return new RunModifierSnapshotFactory(
            catalog,
            progressStorage,
            new UpgradeDefinitionEvaluator(),
            new UpgradeCatalogValidator(new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator())));
    }

    private UpgradeCatalog CreateCatalog(params UpgradeDefinition[] definitions)
    {
        var catalog = Track(ScriptableObject.CreateInstance<UpgradeCatalog>());
        catalog.SetValuesForTests(_coins, definitions);
        return catalog;
    }

    private UpgradeDefinition CreateDefinition(
        string stableId,
        GameplayStatId statId,
        int maxLevel,
        UpgradeProgression effectProgression,
        UpgradeOperationType operationType = UpgradeOperationType.MultiplicativeFactor)
    {
        var definition = Track(ScriptableObject.CreateInstance<UpgradeDefinition>());

        definition.SetValuesForTests(
            stableId,
            stableId,
            "Test upgrade.",
            _icon,
            statId,
            maxLevel,
            LinearProgression(100f, 500f),
            effectProgression,
            operationType,
            UpgradeValueFormat.Multiplier,
            1);
        return definition;
    }

    private GameplayStatId CreateStat(string id)
    {
        var statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        statId.SetValuesForTests(id);
        return statId;
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

    private Sprite CreateIcon()
    {
        var texture = Track(new Texture2D(1, 1));
        return Track(Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f)));
    }

    private void AssertSnapshotModifier(
        RunModifierSnapshot snapshot,
        GameplayStatId statId,
        GameplayStatModifierOperation operation,
        float expectedValue)
    {
        var modifier = snapshot.Modifiers.Single(value => value.StatId == statId);

        Assert.That(modifier.Operation, Is.EqualTo(operation));
        Assert.That(modifier.Value, Is.EqualTo(expectedValue).Within(0.0001f));
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class FakeModifierSource : IGameplayStatModifierSource
    {
        public IReadOnlyList<GameplayStatModifier> Modifiers { get; }

        public FakeModifierSource(params GameplayStatModifier[] modifiers)
        {
            Modifiers = modifiers;
        }
    }
}
