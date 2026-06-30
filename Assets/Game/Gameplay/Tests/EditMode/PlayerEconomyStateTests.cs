using System;
using System.Collections.Generic;
using Game.Gameplay.Economy;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PlayerEconomyStateTests
{
    private readonly List<UnityEngine.Object> _objects = new();

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
    public void CurrencyStorage_SameCurrencySaveId_ReadsAndSpendsSharedBalance()
    {
        var state = new PlayerEconomyState();
        ICurrencyStorage storage = new CurrencyStorage(state);
        var coins = CreateCurrencyDefinition("Coins", "currency-coins");
        var renamedCoins = CreateCurrencyDefinition("Renamed Coins", "currency-coins");

        storage.Grant(coins, 10);
        var spent = storage.TrySpend(renamedCoins, 4);

        Assert.That(spent, Is.True);
        Assert.That(storage.GetAmount(coins), Is.EqualTo(6));
        Assert.That(storage.GetAmount(renamedCoins), Is.EqualTo(6));
        Assert.That(state.GetCurrencyBalance("currency-coins"), Is.EqualTo(6));
    }

    [Test]
    public void UpgradeProgressStorage_SharedEconomyState_ReadsStoredLevelAcrossDefinitionInstances()
    {
        var state = new PlayerEconomyState();
        IUpgradeProgressStorage storage = new UpgradeProgressStorage(state);
        var definition = CreateUpgradeDefinition("launch-power", maxLevel: 3);
        var sameStableId = CreateUpgradeDefinition("launch-power", maxLevel: 3);

        storage.SetLevel(definition, 2);

        Assert.That(storage.GetLevel(sameStableId), Is.EqualTo(2));
        Assert.That(state.GetUpgradeLevel("launch-power"), Is.EqualTo(2));
    }

    [Test]
    public void ReplaceWith_UnknownIds_PreservesIdsInSnapshot()
    {
        var state = new PlayerEconomyState();

        var snapshot = new PlayerEconomySnapshot(
            revision: 12,
            new[] { new PlayerCurrencyBalance("legacy-currency", 17) },
            new[] { new PlayerUpgradeLevel("legacy-upgrade", 4) });

        state.ReplaceWith(snapshot);
        var restoredSnapshot = state.CreateSnapshot();

        Assert.That(restoredSnapshot.Revision, Is.EqualTo(12));
        Assert.That(restoredSnapshot.GetCurrencyBalance("legacy-currency"), Is.EqualTo(17));
        Assert.That(restoredSnapshot.GetUpgradeLevel("legacy-upgrade"), Is.EqualTo(4));
    }

    [Test]
    public void CurrencyStorage_GrantMissingSaveId_ThrowsArgumentException()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());
        var definition = CreateCurrencyDefinition("Broken Currency", string.Empty);

        Assert.That(
            () => storage.Grant(definition, 1),
            Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("currencyDefinition"));
    }

    private CurrencyDefinition CreateCurrencyDefinition(string objectName, string saveId)
    {
        var definition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        definition.name = objectName;
        definition.SetSaveIdForTests(saveId);
        return definition;
    }

    private UpgradeDefinition CreateUpgradeDefinition(string stableId, int maxLevel)
    {
        var definition = Track(ScriptableObject.CreateInstance<UpgradeDefinition>());

        definition.SetValuesForTests(
            stableId,
            displayName: "Launch Power",
            description: "Launches harder.",
            CreateIcon(),
            CreateStatId("SlingshotLaunchPower"),
            maxLevel,
            new UpgradeProgression(100f, 300f, AnimationCurve.Linear(0f, 0f, 1f, 1f), UpgradeProgressionRoundingMode.None, 0f),
            new UpgradeProgression(1f, 2f, AnimationCurve.Linear(0f, 0f, 1f, 1f), UpgradeProgressionRoundingMode.None, 0f),
            UpgradeOperationType.MultiplicativeFactor,
            UpgradeValueFormat.Multiplier,
            displayDecimalPlaces: 1);
        return definition;
    }

    private GameplayStatId CreateStatId(string id)
    {
        var statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        statId.SetValuesForTests(id);
        return statId;
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
}
