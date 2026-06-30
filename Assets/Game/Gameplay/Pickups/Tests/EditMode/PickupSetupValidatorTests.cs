using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PickupSetupValidatorTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private PickupSetupValidator _validator;
    private CurrencyDefinition _coins;
    private PickupDefinition _pickupDefinition;

    [SetUp]
    public void OnSetUp()
    {
        _validator = new PickupSetupValidator();
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");
        _pickupDefinition = CreatePickupDefinition(_coins, 1);
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
    public void Validate_ValidPickupAndPlayerContactCollider_ReturnsNoErrors()
    {
        var pickup = CreateValidPickup("Pickup");
        var playerCollider = CreateValidPlayerContactCollider("Player Contact");

        var errors = Validate(new[] { pickup }, new[] { playerCollider });

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_PickupColliderWrongLayer_ReturnsLayerError()
    {
        var pickup = CreateValidPickup("Pickup");
        pickup.GetComponent<Collider>().gameObject.layer = 0;
        var playerCollider = CreateValidPlayerContactCollider("Player Contact");

        var errors = Validate(new[] { pickup }, new[] { playerCollider });

        Assert.That(errors.Any(error => error.Contains("Pickup Layer")), Is.True);
    }

    [Test]
    public void Validate_PickupColliderNotTrigger_ReturnsTriggerError()
    {
        var pickup = CreateValidPickup("Pickup");
        pickup.GetComponent<Collider>().isTrigger = false;
        var playerCollider = CreateValidPlayerContactCollider("Player Contact");

        var errors = Validate(new[] { pickup }, new[] { playerCollider });

        Assert.That(errors.Any(error => error.Contains("Trigger")), Is.True);
    }

    [Test]
    public void Validate_PlayerContactColliderWrongLayer_ReturnsLayerError()
    {
        var pickup = CreateValidPickup("Pickup");
        var playerCollider = CreateValidPlayerContactCollider("Player Contact");
        playerCollider.gameObject.layer = 0;

        var errors = Validate(new[] { pickup }, new[] { playerCollider });

        Assert.That(errors.Any(error => error.Contains("Player Layer")), Is.True);
    }

    [Test]
    public void Validate_PlayerContactColliderWithoutPlayerTag_ReturnsTagError()
    {
        var pickup = CreateValidPickup("Pickup");
        var playerCollider = CreateValidPlayerContactCollider("Player Contact");
        playerCollider.gameObject.tag = "Untagged";

        var errors = Validate(new[] { pickup }, new[] { playerCollider });

        Assert.That(errors.Any(error => error.Contains("Player Tag")), Is.True);
    }

    [Test]
    public void Validate_RootOnlyPlayerTag_ReturnsTagError()
    {
        var pickup = CreateValidPickup("Pickup");
        var playerRoot = CreateGameObject("Player Root");
        playerRoot.tag = "Player";
        playerRoot.layer = GetRequiredLayer("Player");
        var child = CreateGameObject("Player Child");
        child.transform.SetParent(playerRoot.transform, false);
        child.layer = GetRequiredLayer("Player");
        var playerCollider = child.AddComponent<SphereCollider>();

        var errors = Validate(new[] { pickup }, new[] { playerCollider });

        Assert.That(errors.Any(error => error.Contains("Player Tag")), Is.True);
    }

    [Test]
    public void Validate_MissingPickupReferences_ReturnsReferenceError()
    {
        var playerCollider = CreateValidPlayerContactCollider("Player Contact");

        var errors = Validate(Array.Empty<Pickup>(), new[] { playerCollider });

        Assert.That(errors.Any(error => error.Contains("Level Pickup")), Is.True);
    }

    [Test]
    public void Validate_DuplicatePickupReferences_ReturnsDuplicateError()
    {
        var pickup = CreateValidPickup("Pickup");
        var playerCollider = CreateValidPlayerContactCollider("Player Contact");

        var errors = Validate(new[] { pickup, pickup }, new[] { playerCollider });

        Assert.That(errors.Any(error => error.Contains("duplicate")), Is.True);
    }

    [Test]
    public void Validate_MissingPlayerContactColliderReferences_ReturnsReferenceError()
    {
        var pickup = CreateValidPickup("Pickup");

        var errors = Validate(new[] { pickup }, Array.Empty<Collider>());

        Assert.That(errors.Any(error => error.Contains("Player Pickup Contact Collider")), Is.True);
    }

    private IReadOnlyList<string> Validate(IReadOnlyList<Pickup> pickups, IReadOnlyList<Collider> playerContactColliders)
    {
        return _validator.Validate(pickups, playerContactColliders, "Player", "Player", "Pickup");
    }

    private Pickup CreateValidPickup(string objectName)
    {
        var pickup = CreateGameObject(objectName).AddComponent<Pickup>();
        pickup.SetDefinitionForTests(_pickupDefinition);
        var collider = pickup.gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.gameObject.layer = GetRequiredLayer("Pickup");
        return pickup;
    }

    private Collider CreateValidPlayerContactCollider(string objectName)
    {
        var collider = CreateGameObject(objectName).AddComponent<SphereCollider>();
        collider.gameObject.layer = GetRequiredLayer("Player");
        collider.gameObject.tag = "Player";
        return collider;
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);
        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist for pickup tests.");
        return layer;
    }

    private PickupDefinition CreatePickupDefinition(CurrencyDefinition currencyDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(currencyDefinition, amount);
        return definition;
    }

    private GameObject CreateGameObject(string objectName)
    {
        return Track(new GameObject(objectName));
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }
}
