using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunGameplayStatResolverTests
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
    public void Resolve_NoCurrentRunModifiers_ReturnsBaseValue()
    {
        var statId = CreateStatId("PlayerMaxSpeed");
        var provider = new MutableRunModifierSnapshotProvider(new RunModifierSnapshot(new GameplayStatModifier[0]));
        IRunGameplayStatResolver resolver = new RunGameplayStatResolver(provider);

        var resolvedValue = resolver.Resolve(statId, 10f);

        Assert.That(resolvedValue, Is.EqualTo(10f).Within(0.0001f));
    }

    [Test]
    public void Resolve_WhenCurrentSnapshotChanges_UsesLatestRunSnapshot()
    {
        var statId = CreateStatId("PlayerMaxSpeed");

        var provider = new MutableRunModifierSnapshotProvider(new RunModifierSnapshot(new[]
        {
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 1.5f)
        }));
        IRunGameplayStatResolver resolver = new RunGameplayStatResolver(provider);

        var firstResolvedValue = resolver.Resolve(statId, 10f);

        provider.CurrentSnapshot = new RunModifierSnapshot(new[]
        {
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 2f)
        });
        var secondResolvedValue = resolver.Resolve(statId, 10f);

        Assert.That(firstResolvedValue, Is.EqualTo(15f).Within(0.0001f));
        Assert.That(secondResolvedValue, Is.EqualTo(20f).Within(0.0001f));
    }

    [Test]
    public void Resolve_WhenSnapshotUnchanged_DoesNotAllocateManagedMemoryAfterWarmup()
    {
        var statId = CreateStatId("PlayerMaxSpeed");

        var provider = new MutableRunModifierSnapshotProvider(new RunModifierSnapshot(new[]
        {
            new GameplayStatModifier(statId, GameplayStatModifierOperation.FlatAdd, 2f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.AdditivePercent, 0.5f),
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 2f)
        }));
        IRunGameplayStatResolver resolver = new RunGameplayStatResolver(provider);

        _ = resolver.Resolve(statId, 10f);

        var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();

        var resolvedValue = 0f;

        for (var iteration = 0; iteration < 32; iteration++)
        {
            resolvedValue = resolver.Resolve(statId, 10f);
        }

        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBytesBefore;

        Assert.That(resolvedValue, Is.EqualTo(36f).Within(0.0001f));
        Assert.That(allocatedBytes, Is.Zero);
    }

    private GameplayStatId CreateStatId(string id)
    {
        var statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        statId.SetValuesForTests(id);
        return statId;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class MutableRunModifierSnapshotProvider : IRunModifierSnapshotProvider
    {
        public MutableRunModifierSnapshotProvider(RunModifierSnapshot currentSnapshot)
        {
            CurrentSnapshot = currentSnapshot;
        }

        public RunModifierSnapshot CurrentSnapshot { get; set; }
    }
}
