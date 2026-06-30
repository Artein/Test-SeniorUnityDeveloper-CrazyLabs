using System;
using System.Collections.Generic;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class CurrencyGrantDataPathTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private CurrencyDefinition _gems;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");
        _gems = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _gems.name = "Gems";
        _gems.SetSaveIdForTests("currency-gems");
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
    public void Constructor_ValidCurrencyGrant_StoresCurrencyAndAmount()
    {
        var grant = new CurrencyGrant(_coins, 3);

        Assert.That(grant.CurrencyDefinition, Is.SameAs(_coins));
        Assert.That(grant.Amount, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_NullCurrencyGrantCurrency_Throws()
    {
        Assert.That(
            () => new CurrencyGrant(null, 1),
            Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("currencyDefinition"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_NonPositiveCurrencyGrantAmount_Throws(int amount)
    {
        Assert.That(
            () => new CurrencyGrant(_coins, amount),
            Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("amount"));
    }

    [Test]
    public void Grant_ValidCurrencyGrant_AddsToCurrencyBalance()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());

        storage.Grant(_coins, 3);
        storage.Grant(_coins, 4);

        Assert.That(storage.GetAmount(_coins), Is.EqualTo(7));
    }

    [Test]
    public void GetAmount_MissingCurrency_ReturnsZero()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());

        Assert.That(storage.GetAmount(_coins), Is.Zero);
    }

    [Test]
    public void TrySpend_AffordableAmount_SubtractsBalanceAndReturnsTrue()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());
        storage.Grant(_coins, 7);

        var result = storage.TrySpend(_coins, 4);

        Assert.That(result, Is.True);
        Assert.That(storage.GetAmount(_coins), Is.EqualTo(3));
    }

    [Test]
    public void TrySpend_InsufficientBalance_DoesNotMutateBalanceAndReturnsFalse()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());
        storage.Grant(_coins, 3);

        var result = storage.TrySpend(_coins, 4);

        Assert.That(result, Is.False);
        Assert.That(storage.GetAmount(_coins), Is.EqualTo(3));
    }

    [Test]
    public void Grant_NullCurrency_Throws()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());

        Assert.That(
            () => storage.Grant(null, 1),
            Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("currencyDefinition"));
    }

    [Test]
    public void Grant_NonPositiveAmount_Throws()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());

        Assert.That(
            () => storage.Grant(_coins, 0),
            Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("amount"));
    }

    [Test]
    public void TrySpend_NullCurrency_Throws()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());

        Assert.That(
            () => storage.TrySpend(null, 1),
            Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("currencyDefinition"));
    }

    [Test]
    public void TrySpend_NonPositiveAmount_Throws()
    {
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());

        Assert.That(
            () => storage.TrySpend(_coins, 0),
            Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("amount"));
    }

    [Test]
    public void Grant_ValidRunCurrencyAmount_AccumulatesByCurrency()
    {
        IRunCurrencyAccumulator accumulator = new RunCurrencyAccumulator();

        accumulator.Grant(_coins, 2);
        accumulator.Grant(_gems, 5);
        accumulator.Grant(_coins, 3);

        Assert.That(accumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(5));
        Assert.That(accumulator.CreateSnapshot().GetAmount(_gems), Is.EqualTo(5));
    }

    [Test]
    public void Reset_AfterRunGrants_ClearsCurrentRunDeltas()
    {
        IRunCurrencyAccumulator accumulator = new RunCurrencyAccumulator();
        accumulator.Grant(_coins, 2);

        accumulator.Reset();

        Assert.That(accumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
    }

    [Test]
    public void CreateSnapshot_LaterAccumulatorChanges_DoesNotMutateSnapshot()
    {
        IRunCurrencyAccumulator accumulator = new RunCurrencyAccumulator();
        accumulator.Grant(_coins, 2);
        var snapshot = accumulator.CreateSnapshot();

        accumulator.Grant(_coins, 3);
        accumulator.Reset();

        Assert.That(snapshot.GetAmount(_coins), Is.EqualTo(2));
    }

    [Test]
    public void GetAmount_MissingSnapshotCurrency_ReturnsZero()
    {
        IRunCurrencyAccumulator accumulator = new RunCurrencyAccumulator();
        accumulator.Grant(_coins, 2);
        var snapshot = accumulator.CreateSnapshot();

        Assert.That(snapshot.GetAmount(_gems), Is.Zero);
    }

    [Test]
    public void Validate_ValidPickupDefinition_DoesNotThrow()
    {
        var definition = CreatePickupDefinition(new CurrencyGrant(_coins, 5));

        Assert.That(definition.Validate, Throws.Nothing);
    }

    [Test]
    public void Validate_MissingCurrencyDefinition_Throws()
    {
        var definition = CreatePickupDefinition(default);

        Assert.That(
            definition.Validate,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Currency Definition"));
    }

    [Test]
    public void Validate_NonPositiveAmount_Throws()
    {
        var definition = CreatePickupDefinitionForTests(_coins, 0);

        Assert.That(
            definition.Validate,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("positive"));
    }

    private PickupDefinition CreatePickupDefinition(CurrencyGrant currencyGrant)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetCurrencyGrantForTests(currencyGrant);
        return definition;
    }

    private PickupDefinition CreatePickupDefinitionForTests(CurrencyDefinition currencyDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(currencyDefinition, amount);
        return definition;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }
}
