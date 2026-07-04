using System;
using System.Collections.Generic;
using Game.Foundation.Physics;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PickupAdapterAndStateTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private CurrencyDefinition _coins;
    private PickupDefinition _pickupDefinition;

    [SetUp]
    public void OnSetUp()
    {
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
    public void Validate_MissingPickupDefinition_Throws()
    {
        var pickup = CreatePickup("Pickup", null);

        Assert.That(
            pickup.Validate,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Pickup Definition"));
    }

    [Test]
    public void Validate_MissingTriggerNotifier_Throws()
    {
        var pickup = CreatePickup("Pickup", _pickupDefinition, false);

        Assert.That(
            pickup.Validate,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Trigger Notifier"));
    }

    [Test]
    public void TriggerEntered_DirectPickupContact_PublishesPickupAndCollider()
    {
        var pickup = CreatePickup("Pickup", _pickupDefinition);
        var collider = CreateCollider("Player Contact");
        Pickup observedPickup = null;
        Collider observedCollider = null;

        pickup.TriggerEntered += (triggeredPickup, other) =>
        {
            observedPickup = triggeredPickup;
            observedCollider = other;
        };

        pickup.RaiseTriggerEnteredForTests(collider);

        Assert.That(observedPickup, Is.SameAs(pickup));
        Assert.That(observedCollider, Is.SameAs(collider));
    }

    [Test]
    public void TriggerEntered_DisabledPickup_DoesNotPublishContact()
    {
        var pickup = CreatePickup("Pickup", _pickupDefinition);
        var collider = CreateCollider("Player Contact");
        var eventCount = 0;
        pickup.TriggerEntered += (_, _) => eventCount += 1;
        pickup.SetAvailable(false);

        pickup.RaiseTriggerEnteredForTests(collider);

        Assert.That(eventCount, Is.Zero);
    }

    [Test]
    public void TryConsume_AvailablePickup_SucceedsOnlyOnce()
    {
        var pickup = CreatePickup("Pickup", _pickupDefinition);
        ILevelPickupState state = new LevelPickupState(new FixedLevelPickupSource(new[] { pickup }));

        var firstConsume = state.TryConsume(pickup);
        var secondConsume = state.TryConsume(pickup);

        Assert.That(firstConsume, Is.True);
        Assert.That(secondConsume, Is.False);
        Assert.That(state.IsAvailable(pickup), Is.False);
    }

    [Test]
    public void ResetForLevelSession_ConsumedPickup_RestoresAvailabilityAndEnablesRoot()
    {
        var pickup = CreatePickup("Pickup", _pickupDefinition);
        ILevelPickupState state = new LevelPickupState(new FixedLevelPickupSource(new[] { pickup }));
        state.TryConsume(pickup);
        pickup.SetAvailable(false);

        state.ResetForLevelSession();

        Assert.That(state.IsAvailable(pickup), Is.True);
        Assert.That(pickup.gameObject.activeSelf, Is.True);
    }

    [Test]
    public void Constructor_NullPickupReference_Throws()
    {
        Assert.That(
            () => new LevelPickupState(new FixedLevelPickupSource(new Pickup[] { null })),
            Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("pickups"));
    }

    [Test]
    public void Constructor_DuplicatePickupReference_Throws()
    {
        var pickup = CreatePickup("Pickup", _pickupDefinition);

        Assert.That(
            () => new LevelPickupState(new FixedLevelPickupSource(new[] { pickup, pickup })),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("duplicate"));
    }

    [Test]
    public void TryConsume_UnknownPickup_ReturnsFalse()
    {
        var knownPickup = CreatePickup("Known Pickup", _pickupDefinition);
        var unknownPickup = CreatePickup("Unknown Pickup", _pickupDefinition);
        var state = new LevelPickupState(new FixedLevelPickupSource(new[] { knownPickup }));

        Assert.That(state.TryConsume(unknownPickup), Is.False);
    }

    private Pickup CreatePickup(string objectName, PickupDefinition definition, bool wireNotifier = true)
    {
        var pickup = CreateGameObject(objectName).AddComponent<Pickup>();
        pickup.SetDefinitionForTests(definition);

        if (wireNotifier)
            pickup.SetTriggerNotifierForTests(CreateTriggerNotifier($"{objectName} Trigger"));

        return pickup;
    }

    private TriggerNotifier CreateTriggerNotifier(string objectName)
    {
        var triggerObject = CreateGameObject(objectName);
        triggerObject.AddComponent<SphereCollider>().isTrigger = true;
        return triggerObject.AddComponent<TriggerNotifier>();
    }

    private PickupDefinition CreatePickupDefinition(CurrencyDefinition currencyDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(currencyDefinition, amount);
        return definition;
    }

    private Collider CreateCollider(string objectName)
    {
        return CreateGameObject(objectName).AddComponent<SphereCollider>();
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
