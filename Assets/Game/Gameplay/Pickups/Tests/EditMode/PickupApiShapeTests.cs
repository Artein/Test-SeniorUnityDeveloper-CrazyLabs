using System;
using System.Linq;
using System.Reflection;
using Game.Gameplay.Economy;
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
        var constructor = eventType.GetConstructor(new[] { typeof(CurrencyDefinition), typeof(int), typeof(Vector3) });

        Assert.That(publicProperties, Is.EquivalentTo(new[]
        {
            nameof(PickupCollectedEventArgs.CurrencyGrant),
            nameof(PickupCollectedEventArgs.CurrencyDefinition),
            nameof(PickupCollectedEventArgs.Amount),
            nameof(PickupCollectedEventArgs.BaseCurrencyGrant),
            nameof(PickupCollectedEventArgs.FinalCurrencyGrant),
            nameof(PickupCollectedEventArgs.BaseAmount),
            nameof(PickupCollectedEventArgs.FinalAmount),
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
        var currencyDefinition = ScriptableObject.CreateInstance<CurrencyDefinition>();
        var position = new Vector3(1f, 2f, 3f);

        try
        {
            var pickupEvent = new PickupCollectedEventArgs(currencyDefinition, 5, position);

            Assert.That(pickupEvent.CurrencyDefinition, Is.SameAs(currencyDefinition));
            Assert.That(pickupEvent.Amount, Is.EqualTo(5));
            Assert.That(pickupEvent.BaseAmount, Is.EqualTo(5));
            Assert.That(pickupEvent.FinalAmount, Is.EqualTo(5));
            Assert.That(pickupEvent.CurrencyGrant.Amount, Is.EqualTo(5));
            Assert.That(pickupEvent.BaseCurrencyGrant.Amount, Is.EqualTo(5));
            Assert.That(pickupEvent.FinalCurrencyGrant.Amount, Is.EqualTo(5));
            Assert.That(pickupEvent.Position, Is.EqualTo(position));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(currencyDefinition);
        }
    }

    [Test]
    public void Constructor_BaseAndFinalGrants_StoresLegacyAmountAsFinalAmount()
    {
        var currencyDefinition = ScriptableObject.CreateInstance<CurrencyDefinition>();
        var position = new Vector3(1f, 2f, 3f);

        try
        {
            var baseGrant = new CurrencyGrant(currencyDefinition, 5);
            var finalGrant = new CurrencyGrant(currencyDefinition, 7);
            var pickupEvent = new PickupCollectedEventArgs(baseGrant, finalGrant, position);

            Assert.That(pickupEvent.CurrencyDefinition, Is.SameAs(currencyDefinition));
            Assert.That(pickupEvent.Amount, Is.EqualTo(7));
            Assert.That(pickupEvent.BaseAmount, Is.EqualTo(5));
            Assert.That(pickupEvent.FinalAmount, Is.EqualTo(7));
            Assert.That(pickupEvent.CurrencyGrant.Amount, Is.EqualTo(7));
            Assert.That(pickupEvent.BaseCurrencyGrant.Amount, Is.EqualTo(5));
            Assert.That(pickupEvent.FinalCurrencyGrant.Amount, Is.EqualTo(7));
            Assert.That(pickupEvent.Position, Is.EqualTo(position));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(currencyDefinition);
        }
    }

    [Test]
    public void Constructor_NullCurrency_Throws()
    {
        Assert.That(
            () => new PickupCollectedEventArgs(null, 1, Vector3.zero),
            Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("currencyDefinition"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_NonPositiveAmount_Throws(int amount)
    {
        var currencyDefinition = ScriptableObject.CreateInstance<CurrencyDefinition>();

        try
        {
            Assert.That(
                () => new PickupCollectedEventArgs(currencyDefinition, amount, Vector3.zero),
                Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("amount"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(currencyDefinition);
        }
    }
}
