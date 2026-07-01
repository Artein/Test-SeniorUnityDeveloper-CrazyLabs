using System.Collections.Generic;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunRewardContributorTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private RunRewardSourceCatalog _catalog;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");
        _catalog = new RunRewardSourceCatalog();
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
    public void Build_ContributorAmounts_DerivesSnapshotFromVisibleSourceEntries()
    {
        IRunCurrencyAccumulator accumulator = new RunCurrencyAccumulator();
        accumulator.Grant(_catalog.PickedUpCoins, _coins, 7);

        var builder = new RunRewardBreakdownBuilder(new IRunRewardContributor[]
        {
            new AccumulatedRunRewardContributor((IRunRewardSourceLedger)accumulator),
            new DistanceBonusRunRewardContributor(_coins, _catalog, new FakeRunRewardConfig
            {
                DistanceBonusCoinsPerMeter = 0.2f
            }),
            new AirTimeBonusRunRewardContributor(_coins, _catalog, new FakeRunRewardConfig
            {
                AirTimeBonusCoinsPerSecond = 3f
            })
        });

        var breakdown = builder.Build(new RunRewardContributorContext(
            RunEndReason.ObstacleHit,
            elapsedTime: 4f,
            distanceTravelled: 42.9f,
            finalPosition: Vector3.forward,
            finalSpeed: 1f,
            airTimeSeconds: 1.5f));

        var sourceAmounts = breakdown.SourceAmounts.ToArray();
        Assert.That(sourceAmounts, Has.Length.EqualTo(3));
        Assert.That(sourceAmounts[0].Source.StableId, Is.EqualTo("picked-up-coins"));
        Assert.That(sourceAmounts[0].Amount, Is.EqualTo(7));
        Assert.That(sourceAmounts[1].Source.StableId, Is.EqualTo("distance-bonus"));
        Assert.That(sourceAmounts[1].Amount, Is.EqualTo(8));
        Assert.That(sourceAmounts[2].Source.StableId, Is.EqualTo("air-time-bonus"));
        Assert.That(sourceAmounts[2].Amount, Is.EqualTo(4));
        Assert.That(breakdown.CurrencySnapshot.GetAmount(_coins), Is.EqualTo(19));
    }

    [Test]
    public void Accumulator_SourceGrant_AggregatesBySourceAndCanResetLedger()
    {
        IRunCurrencyAccumulator accumulator = new RunCurrencyAccumulator();
        accumulator.Grant(_catalog.PickedUpCoins, _coins, 2);
        accumulator.Grant(_catalog.PickedUpCoins, _coins, 5);

        var ledgerAmounts = ((IRunRewardSourceLedger)accumulator).CreateRewardSourceAmounts().ToArray();
        Assert.That(ledgerAmounts, Has.Length.EqualTo(1));
        Assert.That(ledgerAmounts[0].Source.StableId, Is.EqualTo("picked-up-coins"));
        Assert.That(ledgerAmounts[0].Amount, Is.EqualTo(7));
        Assert.That(accumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(7));

        accumulator.Reset();

        Assert.That(((IRunRewardSourceLedger)accumulator).CreateRewardSourceAmounts(), Is.Empty);
        Assert.That(accumulator.CreateSnapshot().Amounts, Is.Empty);
    }

    [Test]
    public void DistanceBonus_UsesWholeCoinsFromConfiguredDistanceRate()
    {
        var contributor = new DistanceBonusRunRewardContributor(_coins, _catalog, new FakeRunRewardConfig
        {
            DistanceBonusCoinsPerMeter = 0.5f
        });

        var amounts = contributor.CreateSourceAmounts(new RunRewardContributorContext(
            RunEndReason.Finished,
            elapsedTime: 5f,
            distanceTravelled: 15.9f,
            finalPosition: Vector3.zero,
            finalSpeed: 0f,
            airTimeSeconds: 0f)).ToArray();

        Assert.That(amounts, Has.Length.EqualTo(1));
        Assert.That(amounts[0].Source.Label, Is.EqualTo("Distance Bonus"));
        Assert.That(amounts[0].Amount, Is.EqualTo(7));
    }

    [Test]
    public void AirTimeBonus_UsesWholeCoinsFromConfiguredUngroundedSeconds()
    {
        var contributor = new AirTimeBonusRunRewardContributor(_coins, _catalog, new FakeRunRewardConfig
        {
            AirTimeBonusCoinsPerSecond = 2.5f
        });

        var amounts = contributor.CreateSourceAmounts(new RunRewardContributorContext(
            RunEndReason.ObstacleHit,
            elapsedTime: 5f,
            distanceTravelled: 15f,
            finalPosition: Vector3.zero,
            finalSpeed: 0f,
            airTimeSeconds: 2.4f)).ToArray();

        Assert.That(amounts, Has.Length.EqualTo(1));
        Assert.That(amounts[0].Source.Label, Is.EqualTo("Air Time Bonus"));
        Assert.That(amounts[0].Amount, Is.EqualTo(6));
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class FakeRunRewardConfig : IRunRewardConfig
    {
        public float DistanceBonusCoinsPerMeter { get; set; }
        public float AirTimeBonusCoinsPerSecond { get; set; }
    }
}
