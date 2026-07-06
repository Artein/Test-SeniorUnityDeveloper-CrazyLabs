using System.Collections.Generic;
using System.Linq;
using Game.Foundation.Physics;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PickupSensorSourceTests
{
    private const string PlayerBodyPartLayerName = "PlayerBodyPart";
    private const string PickupLayerName = "Pickup";

    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private PickupDefinition _pickupDefinition;

    [SetUp]
    public void OnSetUp()
    {
        _coins = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        _coins.name = "Coins";
        _coins.SetSaveIdForTests("currency-coins");
        _pickupDefinition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        _pickupDefinition.SetValuesForTests(_coins, 1);
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
    public void Validate_ValidSensorEntry_ReturnsNoErrors()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensor = CreateSensor(sourceRoot.transform, "Left Hand Sensor", PlayerBodyPartLayerName);
        var source = CreateSource(sourceRoot, sensor);

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_MissingSensorEntry_ReturnsReferenceError()
    {
        var source = CreateSource(CreateGameObject("Pickup Sensor Source"));

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors.Any(error => error.Contains("Sensor Entry")), Is.True);
    }

    [Test]
    public void Validate_SensorWithoutSameGameObjectTriggerCollider_ReturnsColliderError()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensorObject = CreateGameObject("Left Hand Sensor");
        sensorObject.transform.SetParent(sourceRoot.transform, false);
        sensorObject.layer = GetRequiredLayer(PlayerBodyPartLayerName);
        var sensor = sensorObject.AddComponent<TriggerNotifier>();
        var source = CreateSource(sourceRoot, sensor);

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors.Any(error => error.Contains("trigger Collider")), Is.True);
    }

    [Test]
    public void Validate_SensorColliderWrongLayer_ReturnsPlayerBodyPartLayerError()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensor = CreateSensor(sourceRoot.transform, "Left Hand Sensor", "Default");
        var source = CreateSource(sourceRoot, sensor);

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors.Any(error => error.Contains("Player Body Part Layer")), Is.True);
    }

    [Test]
    public void Validate_InactiveSensorGameObject_ReturnsInactiveSensorError()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensor = CreateSensor(sourceRoot.transform, "Left Hand Sensor", PlayerBodyPartLayerName);
        sensor.gameObject.SetActive(false);
        var source = CreateSource(sourceRoot, sensor);

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors.Any(error => error.Contains("active in hierarchy")), Is.True);
    }

    [Test]
    public void Validate_SensorUnderInactiveParent_ReturnsInactiveSensorError()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var inactiveParent = CreateGameObject("Inactive Sensor Parent");
        inactiveParent.transform.SetParent(sourceRoot.transform, false);
        inactiveParent.SetActive(false);
        var sensor = CreateSensor(inactiveParent.transform, "Left Hand Sensor", PlayerBodyPartLayerName);
        var source = CreateSource(sourceRoot, sensor);

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors.Any(error => error.Contains("active in hierarchy")), Is.True);
    }

    [Test]
    public void Validate_DisabledTriggerNotifier_ReturnsDisabledSensorError()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensor = CreateSensor(sourceRoot.transform, "Left Hand Sensor", PlayerBodyPartLayerName);
        sensor.enabled = false;
        var source = CreateSource(sourceRoot, sensor);

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors.Any(error => error.Contains("TriggerNotifier")), Is.True);
    }

    [Test]
    public void Validate_DuplicateTriggerNotifier_ReturnsDuplicateError()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensor = CreateSensor(sourceRoot.transform, "Left Hand Sensor", PlayerBodyPartLayerName);
        var source = CreateSource(sourceRoot, sensor, sensor);

        var errors = source.GetReferenceValidationErrorsForTests(PlayerBodyPartLayerName, PickupLayerName).ToArray();

        Assert.That(errors.Any(error => error.Contains("duplicate")), Is.True);
    }

    [Test]
    public void TriggerEntered_PickupCollider_PublishesPickupContactWithSensorIdentity()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensor = CreateSensor(sourceRoot.transform, "Left Hand Sensor", PlayerBodyPartLayerName);
        var source = CreateSource(sourceRoot, sensor);
        var pickup = CreatePickup("Coin Pickup", out var pickupCollider);
        PickupContact? observedContact = null;
        source.PickupContacted += contact => observedContact = contact;

        sensor.NotifyTriggerEnteredForTests(pickupCollider);

        Assert.That(observedContact.HasValue, Is.True);
        Assert.That(observedContact.Value.Pickup, Is.SameAs(pickup));
        Assert.That(observedContact.Value.ContactCollider, Is.SameAs(pickupCollider));
        Assert.That(observedContact.Value.Sensor, Is.SameAs(sensor));
        Assert.That(observedContact.Value.SensorId, Is.EqualTo("Pickup Sensor Source/Left Hand Sensor"));
    }

    [Test]
    public void TriggerEntered_NonPickupCollider_DoesNotPublishContact()
    {
        var sourceRoot = CreateGameObject("Pickup Sensor Source");
        var sensor = CreateSensor(sourceRoot.transform, "Left Hand Sensor", PlayerBodyPartLayerName);
        var source = CreateSource(sourceRoot, sensor);
        var nonPickupCollider = CreateGameObject("Obstacle").AddComponent<SphereCollider>();
        var eventCount = 0;
        source.PickupContacted += _ => eventCount += 1;

        sensor.NotifyTriggerEnteredForTests(nonPickupCollider);

        Assert.That(eventCount, Is.Zero);
    }

    private PickupSensorSource CreateSource(GameObject sourceRoot, params TriggerNotifier[] sensors)
    {
        sourceRoot.SetActive(false);
        var source = sourceRoot.AddComponent<PickupSensorSource>();
        source.SetSensorEntriesForTests(sensors);
        sourceRoot.SetActive(true);
        return source;
    }

    private TriggerNotifier CreateSensor(Transform parent, string objectName, string layerName)
    {
        var sensorObject = CreateGameObject(objectName);
        sensorObject.transform.SetParent(parent, false);
        sensorObject.layer = GetRequiredLayer(layerName);
        var collider = sensorObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        return sensorObject.AddComponent<TriggerNotifier>();
    }

    private Pickup CreatePickup(string objectName, out Collider pickupCollider)
    {
        var pickup = CreateGameObject(objectName).AddComponent<Pickup>();
        pickup.SetDefinitionForTests(_pickupDefinition);

        var colliderObject = CreateGameObject($"{objectName} Trigger");
        colliderObject.transform.SetParent(pickup.transform, false);
        colliderObject.layer = GetRequiredLayer(PickupLayerName);
        pickupCollider = colliderObject.AddComponent<SphereCollider>();
        pickupCollider.isTrigger = true;
        return pickup;
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);
        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist for pickup sensor tests.");
        return layer;
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
