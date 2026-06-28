using System;
using System.Linq;
using System.Reflection;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PickupApiShapeTests
{
    [Test]
    public void PickupCollectedEventArgs_PublicApi_IsDataOnly()
    {
        var eventType = typeof(PickupCollectedEventArgs);

        var publicProperties = eventType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .ToArray();
        var constructor = eventType.GetConstructor(new[] { typeof(ResourceDefinition), typeof(int), typeof(Vector3) });

        Assert.That(publicProperties, Is.EquivalentTo(new[]
        {
            nameof(PickupCollectedEventArgs.ResourceDefinition),
            nameof(PickupCollectedEventArgs.Amount),
            nameof(PickupCollectedEventArgs.Position)
        }));
        Assert.That(constructor, Is.Not.Null);
    }

    [Test]
    public void LevelPickupState_PublicApi_ExposesCollectionStateInterface()
    {
        var interfaceType = typeof(ILevelPickupState);

        Assert.That(interfaceType, Is.Not.Null);
        Assert.That(interfaceType.IsInterface, Is.True);
        Assert.That(interfaceType.IsAssignableFrom(typeof(LevelPickupState)), Is.True);
    }

    [Test]
    public void Constructor_ValidDataOnlyValues_StoresValues()
    {
        var resourceDefinition = ScriptableObject.CreateInstance<ResourceDefinition>();
        var position = new Vector3(1f, 2f, 3f);

        try
        {
            var pickupEvent = new PickupCollectedEventArgs(resourceDefinition, 5, position);

            Assert.That(pickupEvent.ResourceDefinition, Is.SameAs(resourceDefinition));
            Assert.That(pickupEvent.Amount, Is.EqualTo(5));
            Assert.That(pickupEvent.Position, Is.EqualTo(position));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(resourceDefinition);
        }
    }

    [Test]
    public void Constructor_NullResource_Throws()
    {
        Assert.That(
            () => new PickupCollectedEventArgs(null, 1, Vector3.zero),
            Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("resourceDefinition"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_NonPositiveAmount_Throws(int amount)
    {
        var resourceDefinition = ScriptableObject.CreateInstance<ResourceDefinition>();

        try
        {
            Assert.That(
                () => new PickupCollectedEventArgs(resourceDefinition, amount, Vector3.zero),
                Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("amount"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(resourceDefinition);
        }
    }
}
