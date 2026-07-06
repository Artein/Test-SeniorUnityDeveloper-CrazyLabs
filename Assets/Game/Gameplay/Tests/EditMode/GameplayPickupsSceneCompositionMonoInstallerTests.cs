using System;
using System.Collections.Generic;
using System.Linq;
using Game.Foundation.Physics;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplayPickupsSceneCompositionMonoInstallerTests
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
    public void Install_ValidPickupSetup_RegistersPickupComposition()
    {
        var pickup = CreatePickup("Level Pickup");
        var source = CreatePickupSensorSource();
        var installer = CreateInstaller(new[] { pickup }, source);
        var builder = new ContainerBuilder();

        installer.Install(builder);

        using var container = builder.Build();
        var resolvedPickups = container.Resolve<IReadOnlyList<Pickup>>(Game.Gameplay.GameplayState.InjectKey.Pickups.LevelPickups);
        var resolvedPickupSource = container.Resolve<ILevelPickupSource>();
        var resolvedContactSource = container.Resolve<IPickupContactSource>();

        Assert.That(resolvedPickups, Is.SameAs(installer.LevelPickupsForTests));
        Assert.That(resolvedPickupSource.GetLevelPickups(), Is.SameAs(installer.LevelPickupsForTests));
        Assert.That(resolvedContactSource, Is.SameAs(source));
    }

    [Test]
    public void Install_EmptyPickupList_RegistersEmptyPickupSourceAndNoopContactSource()
    {
        var installer = CreateInstaller(Array.Empty<Pickup>(), null);
        var builder = new ContainerBuilder();

        installer.Install(builder);

        using var container = builder.Build();
        var resolvedPickups = container.Resolve<IReadOnlyList<Pickup>>(Game.Gameplay.GameplayState.InjectKey.Pickups.LevelPickups);
        var resolvedPickupSource = container.Resolve<ILevelPickupSource>();
        var resolvedContactSource = container.Resolve<IPickupContactSource>();

        Assert.That(resolvedPickups, Is.Empty);
        Assert.That(resolvedPickupSource.GetLevelPickups(), Is.Empty);
        Assert.That(resolvedContactSource, Is.TypeOf<EmptyPickupContactSource>());
    }

    [Test]
    public void GetReferenceValidationErrors_PickupsWithoutSensorSource_ReturnsSensorSourceError()
    {
        var pickup = CreatePickup("Level Pickup");
        var installer = CreateInstaller(new[] { pickup }, null);

        var errors = installer.GetReferenceValidationErrorsForTests().ToArray();

        Assert.That(errors.Any(error => error.Contains("Pickup Sensor Source")), Is.True);
    }

    [Test]
    public void GetReferenceValidationErrors_EmptyPickupList_AllowsMissingSensorSource()
    {
        var installer = CreateInstaller(Array.Empty<Pickup>(), null);

        var errors = installer.GetReferenceValidationErrorsForTests().ToArray();

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void GetReferenceValidationErrors_PlayerBodyPartCannotInteractWithPickup_ReturnsLayerMatrixError()
    {
        var playerBodyPartLayer = GetRequiredLayer(PlayerBodyPartLayerName);
        var pickupLayer = GetRequiredLayer(PickupLayerName);
        var pickup = CreatePickup("Level Pickup");
        var source = CreatePickupSensorSource();
        var installer = CreateInstaller(new[] { pickup }, source);
        installer.SetLayerCollisionIgnoredForTests(true);

        var errors = installer.GetReferenceValidationErrorsForTests().ToArray();

        Assert.That(errors.Any(error => error.Contains("PlayerBodyPart") && error.Contains("Pickup")), Is.True);
    }

    private GameplayPickupsSceneCompositionMonoInstaller CreateInstaller(IReadOnlyList<Pickup> pickups, PickupSensorSource source)
    {
        var installer = CreateGameObject("Gameplay Pickups Scene Composition Installer")
            .AddComponent<GameplayPickupsSceneCompositionMonoInstaller>();
        installer.SetReferencesForTests(pickups.ToArray(), source, PickupLayerName, PlayerBodyPartLayerName);
        return installer;
    }

    private PickupSensorSource CreatePickupSensorSource()
    {
        var root = CreateGameObject("Pickup Sensor Source");
        var sensorObject = CreateGameObject("Body Sensor");
        sensorObject.transform.SetParent(root.transform, false);
        sensorObject.layer = GetRequiredLayer(PlayerBodyPartLayerName);
        var collider = sensorObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        var sensor = sensorObject.AddComponent<TriggerNotifier>();
        var source = root.AddComponent<PickupSensorSource>();
        source.SetSensorEntriesForTests(sensor);
        return source;
    }

    private Pickup CreatePickup(string objectName)
    {
        var pickup = CreateGameObject(objectName).AddComponent<Pickup>();
        pickup.SetDefinitionForTests(_pickupDefinition);
        var triggerObject = CreateGameObject($"{objectName} Trigger");
        triggerObject.transform.SetParent(pickup.transform, false);
        triggerObject.layer = GetRequiredLayer(PickupLayerName);
        var collider = triggerObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        return pickup;
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);
        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist for pickup composition tests.");
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
