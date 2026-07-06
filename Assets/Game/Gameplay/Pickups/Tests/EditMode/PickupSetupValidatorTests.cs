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
    public void Validate_EmptyPickupReferences_ReturnsNoErrors()
    {
        var errors = Validate(Array.Empty<Pickup>());

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_ValidPickupWithSinglePickupLayerTriggerCollider_ReturnsNoErrors()
    {
        var pickup = CreateValidPickup("Pickup");

        var errors = Validate(new[] { pickup });

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_PickupColliderWrongLayer_ReturnsSingleColliderError()
    {
        var pickup = CreatePickup("Pickup");
        AddPickupCollider(pickup.transform, "Pickup Trigger", "Default", isTrigger: true);

        var errors = Validate(new[] { pickup });

        Assert.That(errors.Any(error => error.Contains("exactly one") && error.Contains("Pickup Layer")), Is.True);
    }

    [Test]
    public void Validate_PickupColliderNotTrigger_ReturnsTriggerError()
    {
        var pickup = CreatePickup("Pickup");
        AddPickupCollider(pickup.transform, "Pickup Trigger", "Pickup", isTrigger: false);

        var errors = Validate(new[] { pickup });

        Assert.That(errors.Any(error => error.Contains("Trigger")), Is.True);
    }

    [Test]
    public void Validate_DisabledPickupCollider_ReturnsEnabledError()
    {
        var pickup = CreatePickup("Pickup");
        var collider = AddPickupCollider(pickup.transform, "Pickup Trigger", "Pickup", isTrigger: true);
        collider.enabled = false;

        var errors = Validate(new[] { pickup });

        Assert.That(errors.Any(error => error.Contains("enabled")), Is.True);
    }

    [Test]
    public void Validate_MissingPickupLayerTriggerCollider_ReturnsSingleColliderError()
    {
        var pickup = CreatePickup("Pickup");

        var errors = Validate(new[] { pickup });

        Assert.That(errors.Any(error => error.Contains("exactly one") && error.Contains("Pickup Layer")), Is.True);
    }

    [Test]
    public void Validate_MultiplePickupLayerTriggerColliders_ReturnsSingleColliderError()
    {
        var pickup = CreatePickup("Pickup");
        AddPickupCollider(pickup.transform, "Pickup Trigger 1", "Pickup", isTrigger: true);
        AddPickupCollider(pickup.transform, "Pickup Trigger 2", "Pickup", isTrigger: true);

        var errors = Validate(new[] { pickup });

        Assert.That(errors.Any(error => error.Contains("exactly one") && error.Contains("Pickup Layer")), Is.True);
    }

    [Test]
    public void Validate_NonPickupLayerColliderAlongsidePickupTrigger_ReturnsNoErrors()
    {
        var pickup = CreateValidPickup("Pickup");
        AddPickupCollider(pickup.transform, "Visual Collider", "Default", isTrigger: false);

        var errors = Validate(new[] { pickup });

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_MissingPickupDefinition_ReturnsDefinitionError()
    {
        var pickup = CreateGameObject("Pickup").AddComponent<Pickup>();
        AddPickupCollider(pickup.transform, "Pickup Trigger", "Pickup", isTrigger: true);

        var errors = Validate(new[] { pickup });

        Assert.That(errors.Any(error => error.Contains("Pickup Definition")), Is.True);
    }

    [Test]
    public void Validate_DuplicatePickupReferences_ReturnsDuplicateError()
    {
        var pickup = CreateValidPickup("Pickup");

        var errors = Validate(new[] { pickup, pickup });

        Assert.That(errors.Any(error => error.Contains("duplicate")), Is.True);
    }

    [Test]
    public void Validate_NullPickupReference_ReturnsReferenceError()
    {
        var errors = Validate(new Pickup[] { null });

        Assert.That(errors.Any(error => error.Contains("Level Pickup at index 0")), Is.True);
    }

    private IReadOnlyList<string> Validate(IReadOnlyList<Pickup> pickups)
    {
        return _validator.Validate(pickups, "Pickup");
    }

    private Pickup CreateValidPickup(string objectName)
    {
        var pickup = CreatePickup(objectName);
        AddPickupCollider(pickup.transform, $"{objectName} Trigger", "Pickup", isTrigger: true);
        return pickup;
    }

    private Pickup CreatePickup(string objectName)
    {
        var pickup = CreateGameObject(objectName).AddComponent<Pickup>();
        pickup.SetDefinitionForTests(_pickupDefinition);
        return pickup;
    }

    private Collider AddPickupCollider(Transform parent, string objectName, string layerName, bool isTrigger)
    {
        var colliderObject = CreateGameObject(objectName);
        colliderObject.transform.SetParent(parent, false);
        colliderObject.layer = GetRequiredLayer(layerName);
        var collider = colliderObject.AddComponent<SphereCollider>();
        collider.isTrigger = isTrigger;
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
