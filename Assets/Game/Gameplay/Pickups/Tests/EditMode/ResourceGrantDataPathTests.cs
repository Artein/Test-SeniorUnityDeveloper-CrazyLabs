using System;
using System.Collections.Generic;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class ResourceGrantDataPathTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private ResourceDefinition _coins;
    private ResourceDefinition _gems;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<ResourceDefinition>());
        _coins.name = "Coins";
        _gems = Track(ScriptableObject.CreateInstance<ResourceDefinition>());
        _gems.name = "Gems";
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
    public void Grant_ValidResourceAmount_AddsToResourceBalance()
    {
        var storage = new ResourceStorage();

        storage.Grant(_coins, 3);
        storage.Grant(_coins, 4);

        Assert.That(storage.GetAmount(_coins), Is.EqualTo(7));
    }

    [Test]
    public void GetAmount_MissingResource_ReturnsZero()
    {
        var storage = new ResourceStorage();

        Assert.That(storage.GetAmount(_coins), Is.Zero);
    }

    [Test]
    public void Grant_NullResource_Throws()
    {
        var storage = new ResourceStorage();

        Assert.That(
            () => storage.Grant(null, 1),
            Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("resourceDefinition"));
    }

    [Test]
    public void Grant_NonPositiveAmount_Throws()
    {
        var storage = new ResourceStorage();

        Assert.That(
            () => storage.Grant(_coins, 0),
            Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("amount"));
    }

    [Test]
    public void Grant_ValidRunResourceAmount_AccumulatesByResource()
    {
        IRunResourceAccumulator accumulator = new RunResourceAccumulator();

        accumulator.Grant(_coins, 2);
        accumulator.Grant(_gems, 5);
        accumulator.Grant(_coins, 3);

        Assert.That(accumulator.CreateSnapshot().GetAmount(_coins), Is.EqualTo(5));
        Assert.That(accumulator.CreateSnapshot().GetAmount(_gems), Is.EqualTo(5));
    }

    [Test]
    public void Reset_AfterRunGrants_ClearsCurrentRunDeltas()
    {
        IRunResourceAccumulator accumulator = new RunResourceAccumulator();
        accumulator.Grant(_coins, 2);

        accumulator.Reset();

        Assert.That(accumulator.CreateSnapshot().GetAmount(_coins), Is.Zero);
    }

    [Test]
    public void CreateSnapshot_LaterAccumulatorChanges_DoesNotMutateSnapshot()
    {
        IRunResourceAccumulator accumulator = new RunResourceAccumulator();
        accumulator.Grant(_coins, 2);
        var snapshot = accumulator.CreateSnapshot();

        accumulator.Grant(_coins, 3);
        accumulator.Reset();

        Assert.That(snapshot.GetAmount(_coins), Is.EqualTo(2));
    }

    [Test]
    public void GetAmount_MissingSnapshotResource_ReturnsZero()
    {
        IRunResourceAccumulator accumulator = new RunResourceAccumulator();
        accumulator.Grant(_coins, 2);
        var snapshot = accumulator.CreateSnapshot();

        Assert.That(snapshot.GetAmount(_gems), Is.Zero);
    }

    [Test]
    public void Validate_ValidPickupDefinition_DoesNotThrow()
    {
        var definition = CreatePickupDefinition(_coins, 5);

        Assert.That(definition.Validate, Throws.Nothing);
    }

    [Test]
    public void Validate_MissingResourceDefinition_Throws()
    {
        var definition = CreatePickupDefinition(null, 5);

        Assert.That(
            definition.Validate,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Resource Definition"));
    }

    [Test]
    public void Validate_NonPositiveAmount_Throws()
    {
        var definition = CreatePickupDefinition(_coins, 0);

        Assert.That(
            definition.Validate,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("positive"));
    }

    private PickupDefinition CreatePickupDefinition(ResourceDefinition resourceDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(resourceDefinition, amount);
        return definition;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }
}
