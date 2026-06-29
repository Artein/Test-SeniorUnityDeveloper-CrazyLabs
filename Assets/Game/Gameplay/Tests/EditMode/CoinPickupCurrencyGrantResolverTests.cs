using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public sealed class CoinPickupCurrencyGrantResolverTests
{
    private readonly List<Object> _objects = new();
    private CurrencyDefinition _coins;
    private GameplayStatId _coinPickupMultiplierStatId;

    [SetUp]
    public void OnSetUp()
    {
        _coins = CreateCurrencyDefinition("Coins");
        _coinPickupMultiplierStatId = CreateStatId("CoinPickupMultiplier");
    }

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void Resolve_CoinGrantAtMultiplierOne_ReturnsBaseAmount()
    {
        var statResolver = new RecordingRunGameplayStatResolver(1f);
        var resolver = CreateResolver(statResolver);

        var resolution = resolver.Resolve(new CurrencyGrant(_coins, 5));

        AssertResolution(resolution, _coins, 5, 5);
        Assert.That(statResolver.ResolveCallCount, Is.EqualTo(1));
        Assert.That(statResolver.LastStatId, Is.SameAs(_coinPickupMultiplierStatId));
        Assert.That(statResolver.LastBaseValue, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void Resolve_CoinGrantWithFractionalMultiplier_CarriesRemainderWithinRun()
    {
        var resolver = CreateResolver(new RecordingRunGameplayStatResolver(1.5f));

        var firstResolution = resolver.Resolve(new CurrencyGrant(_coins, 1));
        var secondResolution = resolver.Resolve(new CurrencyGrant(_coins, 1));

        AssertResolution(firstResolution, _coins, 1, 1);
        AssertResolution(secondResolution, _coins, 1, 2);
    }

    [Test]
    public void Resolve_CoinGrantWithLargeMultiplier_ReturnsFlooredFinalAmount()
    {
        var resolver = CreateResolver(new RecordingRunGameplayStatResolver(10f));

        var resolution = resolver.Resolve(new CurrencyGrant(_coins, 5));

        AssertResolution(resolution, _coins, 5, 50);
    }

    [Test]
    public void Reset_AfterFractionalCarry_ClearsRemainder()
    {
        var resolver = CreateResolver(new RecordingRunGameplayStatResolver(1.5f));

        var firstResolution = resolver.Resolve(new CurrencyGrant(_coins, 1));
        resolver.Reset();
        var secondResolution = resolver.Resolve(new CurrencyGrant(_coins, 1));

        AssertResolution(firstResolution, _coins, 1, 1);
        AssertResolution(secondResolution, _coins, 1, 1);
    }

    [Test]
    public void Resolve_NonCoinGrant_BypassesMultiplierAndDoesNotResolveStat()
    {
        var gems = CreateCurrencyDefinition("Gems");
        var statResolver = new RecordingRunGameplayStatResolver(10f);
        var resolver = CreateResolver(statResolver);

        var resolution = resolver.Resolve(new CurrencyGrant(gems, 7));

        AssertResolution(resolution, gems, 7, 7);
        Assert.That(statResolver.ResolveCallCount, Is.Zero);
    }

    [Test]
    public void Grant_DirectCurrencyStorageGrant_DoesNotApplyCoinPickupMultiplier()
    {
        ICurrencyStorage storage = new CurrencyStorage();

        storage.Grant(_coins, 3);

        Assert.That(storage.GetAmount(_coins), Is.EqualTo(3));
    }

    private CoinPickupCurrencyGrantResolver CreateResolver(RecordingRunGameplayStatResolver statResolver)
    {
        return new CoinPickupCurrencyGrantResolver(statResolver, _coins, _coinPickupMultiplierStatId);
    }

    private void AssertResolution(
        PickupCurrencyGrantResolution resolution,
        CurrencyDefinition currencyDefinition,
        int expectedBaseAmount,
        int expectedFinalAmount)
    {
        Assert.That(resolution.BaseCurrencyGrant.CurrencyDefinition, Is.SameAs(currencyDefinition));
        Assert.That(resolution.BaseCurrencyGrant.Amount, Is.EqualTo(expectedBaseAmount));
        Assert.That(resolution.FinalCurrencyGrant.CurrencyDefinition, Is.SameAs(currencyDefinition));
        Assert.That(resolution.FinalCurrencyGrant.Amount, Is.EqualTo(expectedFinalAmount));
        Assert.That(resolution.CurrencyDefinition, Is.SameAs(currencyDefinition));
        Assert.That(resolution.BaseAmount, Is.EqualTo(expectedBaseAmount));
        Assert.That(resolution.FinalAmount, Is.EqualTo(expectedFinalAmount));
    }

    private CurrencyDefinition CreateCurrencyDefinition(string objectName)
    {
        var currencyDefinition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        currencyDefinition.name = objectName;
        return currencyDefinition;
    }

    private GameplayStatId CreateStatId(string id)
    {
        var statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        statId.SetValuesForTests(id);
        return statId;
    }

    private T Track<T>(T value)
        where T : Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class RecordingRunGameplayStatResolver : IRunGameplayStatResolver
    {
        private readonly float _resolvedValue;

        public int ResolveCallCount { get; private set; }
        public GameplayStatId LastStatId { get; private set; }
        public float LastBaseValue { get; private set; }

        public RecordingRunGameplayStatResolver(float resolvedValue)
        {
            _resolvedValue = resolvedValue;
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            ResolveCallCount += 1;
            LastStatId = statId;
            LastBaseValue = baseValue;
            return _resolvedValue;
        }
    }
}
