using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunEndedResultStatsBuilderTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private RunSessionBestDistanceTracker _bestDistanceTracker;
    private RunEndedResultStatsBuilder _builder;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");
        _bestDistanceTracker = new RunSessionBestDistanceTracker();
        _builder = new RunEndedResultStatsBuilder(_coins, _bestDistanceTracker);
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
    public void Build_FirstRun_UsesAcceptedSnapshotCoinsAndFloorsDistance()
    {
        var result = CreateRunResult(12.99f, 7);

        var stats = _builder.Build(result);

        Assert.That(stats.EarnedCoins, Is.EqualTo(7));
        Assert.That(stats.ReachedMeters, Is.EqualTo(12));
        Assert.That(stats.HasBestImprovement, Is.True);
        Assert.That(stats.BestImprovementMeters, Is.EqualTo(12));
    }

    [Test]
    public void Build_LongerThanSessionBest_ShowsWholeMeterImprovementAndUpdatesBest()
    {
        _builder.Build(CreateRunResult(12.9f, 1));

        var stats = _builder.Build(CreateRunResult(16.8f, 2));

        Assert.That(stats.ReachedMeters, Is.EqualTo(16));
        Assert.That(stats.HasBestImprovement, Is.True);
        Assert.That(stats.BestImprovementMeters, Is.EqualTo(4));
        Assert.That(_bestDistanceTracker.BestDistance, Is.EqualTo(16.8f).Within(0.0001f));
    }

    [Test]
    public void Build_NotLongerThanSessionBest_HidesImprovementButKeepsBest()
    {
        _builder.Build(CreateRunResult(16.8f, 1));

        var stats = _builder.Build(CreateRunResult(16.1f, 2));

        Assert.That(stats.ReachedMeters, Is.EqualTo(16));
        Assert.That(stats.HasBestImprovement, Is.False);
        Assert.That(stats.BestImprovementMeters, Is.Zero);
        Assert.That(_bestDistanceTracker.BestDistance, Is.EqualTo(16.8f).Within(0.0001f));
    }

    [Test]
    public void Build_NoCoinEntry_ReportsZeroEarnedCoins()
    {
        var result = new RunResult(
            reason: RunEndReason.ObstacleHit,
            elapsedTime: 1f,
            distanceTravelled: 5.5f,
            finalPosition: Vector3.zero,
            finalSpeed: 0f,
            rewardBreakdown: new RunRewardBreakdown(Array.Empty<RunRewardSourceAmount>()));

        var stats = _builder.Build(result);

        Assert.That(stats.EarnedCoins, Is.Zero);
    }

    private RunResult CreateRunResult(float distance, int coins)
    {
        return new RunResult(
            reason: RunEndReason.ObstacleHit,
            elapsedTime: 1f,
            distanceTravelled: distance,
            finalPosition: Vector3.zero,
            finalSpeed: 0f,
            rewardBreakdown: new RunRewardBreakdown(new[]
            {
                new RunRewardSourceAmount(new RunRewardSource("test-reward", "Test Reward", 0, showWhenZero: false), _coins, coins)
            }));
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }
}
