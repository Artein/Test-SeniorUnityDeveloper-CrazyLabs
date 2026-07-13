using System;
using System.Collections;
using System.Linq;
using Game.Foundation.Physics;
using Game.Foundation.Presentation;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class PickupSceneCompositionTests : BaseGameplayScenePlayModeFixture
{
    // TODO: Instead of hardcode — use EditorAssetsProvider + Layer/Tag attributes on fields
    private const string PlayerBodyPartLayerName = "PlayerBodyPart";
    private const string PickupLayerName = "Pickup";

    private static readonly ExpectedPickupSensor[] ExpectedPickupSensors =
    {
        new("Head Pickup Sensor", typeof(SphereCollider), "Bip001 Head"),
        new("Torso Pickup Sensor", typeof(CapsuleCollider), "Bip001 Spine1"),
        new("L Upper Arm Pickup Sensor", typeof(CapsuleCollider), "Bip001 L UpperArm"),
        new("L Forearm Pickup Sensor", typeof(CapsuleCollider), "Bip001 L Forearm"),
        new("R Upper Arm Pickup Sensor", typeof(CapsuleCollider), "Bip001 R UpperArm"),
        new("R Forearm Pickup Sensor", typeof(CapsuleCollider), "Bip001 R Forearm"),
        new("L Thigh Pickup Sensor", typeof(CapsuleCollider), "Bip001 L Thigh"),
        new("L Calf Pickup Sensor", typeof(CapsuleCollider), "Bip001 L Calf"),
        new("R Thigh Pickup Sensor", typeof(CapsuleCollider), "Bip001 R Thigh"),
        new("R Calf Pickup Sensor", typeof(CapsuleCollider), "Bip001 R Calf")
    };

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_PickupCurrencyCompositionIsReady()
    {
        yield return LoadGameplaySceneWithIsolatedSaves();

        var scene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var pickupsInstaller = FindSingleInScene<GameplayPickupsSceneCompositionMonoInstaller>(scene, "GameplayPickupsSceneCompositionMonoInstaller");

        var animatedContactSensorPoseSyncView =
            FindSingleInScene<AnimatedContactSensorPoseSyncView>(scene, "AnimatedContactSensorPoseSyncView");

        var player = FindGameObjectByName(scene, "Player");
        var movementPhysicsRoot = FindGameObjectByName(scene, "MovementPhysicsRoot");
        var characterVisualAnchor = FindGameObjectByName(scene, "CharacterVisualAnchor");
        var launchTargetColliderRoot = FindGameObjectByName(scene, "LaunchTargetColliderRoot");
        var runBodyContactColliderRoot = FindGameObjectByName(scene, "RunBodyContactColliderRoot");
        var bandCenter = FindGameObjectByName(scene, "Band Center");
        var playerBodyPartLayer = GetRequiredLayer(PlayerBodyPartLayerName);
        var pickupLayer = GetRequiredLayer(PickupLayerName);
        var allScenePickups = FindComponentsInScene<Pickup>(scene);
        var configuredPickups = pickupsInstaller.LevelPickupsForTests.ToArray();
        var levelPickupState = lifetimeScope.Container.Resolve<ILevelPickupState>();
        var levelPickupStateSnapshot = (LevelPickupState)levelPickupState;
        var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();
        var runCurrencyAccumulator = lifetimeScope.Container.Resolve<IRunCurrencyAccumulator>();
        var pickupCurrencyGrantResolver = lifetimeScope.Container.Resolve<IPickupCurrencyGrantResolver>();
        var pickupCollectionNotifier = lifetimeScope.Container.Resolve<IPickupCollectionNotifier>();
        var pickupContactSource = lifetimeScope.Container.Resolve<IPickupContactSource>();
        var pickupSensorSource = pickupContactSource as PickupSensorSource;

        var pickupSensorNotifiers = pickupSensorSource != null
            ? pickupSensorSource.GetComponentsInChildren<TriggerNotifier>(true)
            : new TriggerNotifier[] { };

        Assert.That(scene.path, Is.EqualTo(TestAssets.GameplaySceneRef.Path));
        Assert.That(lifetimeScope.SceneCompositionInstallersForTests, Has.Exactly(1).SameAs(pickupsInstaller));
        Assert.That(Physics.GetIgnoreLayerCollision(playerBodyPartLayer, pickupLayer), Is.False);
        Assert.That(allScenePickups, Has.Length.GreaterThan(2));
        Assert.That(configuredPickups, Has.Length.EqualTo(allScenePickups.Length));
        Assert.That(levelPickupStateSnapshot.PickupsForTests, Has.Length.EqualTo(configuredPickups.Length));
        Assert.That(levelPickupState, Is.Not.Null);
        Assert.That(currencyStorage, Is.Not.Null);
        Assert.That(runCurrencyAccumulator, Is.Not.Null);
        Assert.That(pickupCurrencyGrantResolver, Is.Not.Null);
        Assert.That(pickupCollectionNotifier, Is.Not.Null);
        Assert.That(pickupSensorSource, Is.Not.Null);
        Assert.That(pickupSensorNotifiers, Has.Length.EqualTo(ExpectedPickupSensors.Length));
        Assert.That(TryFindGameObjectByName(scene, "Body Pickup Sensor", out _), Is.False);

        foreach (var scenePickup in allScenePickups)
        {
            Assert.That(configuredPickups, Does.Contain(scenePickup), scenePickup.name);
        }

        foreach (var configuredPickup in configuredPickups)
        {
            Assert.That(allScenePickups, Does.Contain(configuredPickup));
            Assert.That(levelPickupState.IsAvailable(configuredPickup), Is.True, configuredPickup.name);
            AssertPickupAuthoring(configuredPickup, pickupLayer);
        }

        var regularCoinPickup = FindConfiguredPickup(configuredPickups, 5, "regular coin pickup");
        var bigCoinPickup = FindConfiguredPickup(configuredPickups, 25, "big coin pickup");

        Assert.That(regularCoinPickup.Definition.CurrencyDefinition, Is.SameAs(bigCoinPickup.Definition.CurrencyDefinition));

        AssertPlayerTransformAuthority(
            player,
            movementPhysicsRoot,
            characterVisualAnchor,
            animatedContactSensorPoseSyncView,
            pickupSensorSource,
            launchTargetColliderRoot,
            runBodyContactColliderRoot,
            bandCenter);

        AssertPickupSensorAuthoring(pickupSensorSource, pickupSensorNotifiers, playerBodyPartLayer);
        AssertPickupSensorPoseBindings(animatedContactSensorPoseSyncView, pickupSensorNotifiers);
    }

    private Pickup FindConfiguredPickup(Pickup[] configuredPickups, int expectedAmount, string description)
    {
        var matches = configuredPickups
            .Where(candidate => candidate != null
                                && candidate.Definition != null
                                && candidate.Definition.Amount == expectedAmount)
            .ToArray();

        Assert.That(matches, Has.Length.GreaterThanOrEqualTo(1), $"Expected configured {description} with amount {expectedAmount}.");
        return matches[0];
    }

    private void AssertPickupAuthoring(Pickup pickup, int pickupLayer)
    {
        var colliders = pickup.GetComponentsInChildren<Collider>(true);

        var pickupLayerColliders = colliders
            .Where(collider => collider.gameObject.layer == pickupLayer)
            .ToArray();

        Assert.That(pickup.gameObject.activeSelf, Is.True, pickup.name);
        Assert.That(pickup.gameObject.layer, Is.EqualTo(pickupLayer), pickup.name);
        Assert.That(pickup.Definition, Is.Not.Null, pickup.name);
        Assert.That(new[] { 5, 25 }, Does.Contain(pickup.Definition.Amount), pickup.name);
        Assert.That(pickup.Definition.CurrencyDefinition, Is.Not.Null, pickup.name);
        Assert.That(pickupLayerColliders, Has.Length.EqualTo(1), pickup.name);

        pickup.Definition.Validate();
        AssertPickupTriggerColliderAuthoring(pickup, pickupLayerColliders[0], pickupLayer);
        AssertPickupVisualAuthoring(pickup);
    }

    private void AssertPickupTriggerColliderAuthoring(Pickup pickup, Collider collider, int pickupLayer)
    {
        Assert.That(collider.enabled, Is.True, pickup.name);
        Assert.That(collider.isTrigger, Is.True, pickup.name);
        Assert.That(collider.gameObject.layer, Is.EqualTo(pickupLayer), pickup.name);
        Assert.That(collider.transform.IsChildOf(pickup.transform), Is.True, pickup.name);
    }

    private void AssertPickupSensorAuthoring(PickupSensorSource sensorSource, TriggerNotifier[] sensorNotifiers, int playerBodyPartLayer)
    {
        var registeredSensorEntries = sensorSource.SensorEntriesForTests.ToArray();

        Assert.That(registeredSensorEntries, Is.EquivalentTo(sensorNotifiers));

        foreach (var expectedSensor in ExpectedPickupSensors)
        {
            var sensorNotifier = sensorNotifiers.SingleOrDefault(notifier => notifier.name == expectedSensor.SensorName);

            Assert.That(sensorNotifier, Is.Not.Null, expectedSensor.SensorName);

            var sensorColliders = sensorNotifier.GetComponents<Collider>();

            Assert.That(sensorNotifier.transform.IsChildOf(sensorSource.transform), Is.True, sensorNotifier.name);
            Assert.That(sensorColliders, Has.Length.EqualTo(1), sensorNotifier.name);
            Assert.That(sensorColliders[0], Is.TypeOf(expectedSensor.ColliderType), sensorNotifier.name);

            foreach (var collider in sensorColliders)
            {
                Assert.That(collider.enabled, Is.True, collider.name);
                Assert.That(collider.isTrigger, Is.True, collider.name);
                Assert.That(collider.gameObject.layer, Is.EqualTo(playerBodyPartLayer), collider.name);
            }

            if (sensorColliders[0] is CapsuleCollider capsuleCollider)
                Assert.That(capsuleCollider.direction, Is.EqualTo(0), sensorNotifier.name);
        }
    }

    private void AssertPickupSensorPoseBindings(
        AnimatedContactSensorPoseSyncView poseSyncView,
        TriggerNotifier[] sensorNotifiers)
    {
        var bindings = poseSyncView.Bindings.ToArray();

        Assert.That(bindings, Has.Length.EqualTo(ExpectedPickupSensors.Length));

        foreach (var expectedSensor in ExpectedPickupSensors)
        {
            var sensorNotifier = sensorNotifiers.Single(notifier => notifier.name == expectedSensor.SensorName);
            var binding = bindings.SingleOrDefault(candidate => candidate.Target == sensorNotifier.transform);

            Assert.That(binding.Target, Is.SameAs(sensorNotifier.transform), expectedSensor.SensorName);
            Assert.That(binding.Source, Is.Not.Null, expectedSensor.SensorName);
            Assert.That(binding.Source.name, Is.EqualTo(expectedSensor.SourceBoneName), expectedSensor.SensorName);
        }
    }

    private void AssertPlayerTransformAuthority(
        GameObject player,
        GameObject movementPhysicsRoot,
        GameObject characterVisualAnchor,
        AnimatedContactSensorPoseSyncView poseSyncView,
        PickupSensorSource pickupSensorSource,
        GameObject launchTargetColliderRoot,
        GameObject runBodyContactColliderRoot,
        GameObject bandCenter)
    {
        var movementRigidbody = movementPhysicsRoot.GetComponent<Rigidbody>();
        var sensorRoot = poseSyncView.RootRigidbody.gameObject;

        Assert.That(player.transform.parent, Is.Null, player.name);
        Assert.That(player.GetComponent<Rigidbody>(), Is.Null, player.name);
        Assert.That(player.GetComponent<Collider>(), Is.Null, player.name);
        Assert.That(movementPhysicsRoot.transform.parent, Is.SameAs(player.transform), movementPhysicsRoot.name);
        Assert.That(movementRigidbody, Is.Not.Null, movementPhysicsRoot.name);
        Assert.That(characterVisualAnchor.transform.parent, Is.SameAs(player.transform), characterVisualAnchor.name);
        Assert.That(sensorRoot.transform.parent, Is.SameAs(player.transform), sensorRoot.name);
        Assert.That(pickupSensorSource.transform, Is.SameAs(sensorRoot.transform), pickupSensorSource.name);
        Assert.That(sensorRoot.transform.IsChildOf(movementPhysicsRoot.transform), Is.False, sensorRoot.name);
        Assert.That(launchTargetColliderRoot.transform.parent, Is.SameAs(movementPhysicsRoot.transform), launchTargetColliderRoot.name);
        Assert.That(runBodyContactColliderRoot.transform.parent, Is.SameAs(movementPhysicsRoot.transform), runBodyContactColliderRoot.name);
        Assert.That(bandCenter.transform.parent, Is.SameAs(movementPhysicsRoot.transform), bandCenter.name);
        Assert.That(FindNonKinematicRigidbodyAncestor(sensorRoot.transform), Is.Null, sensorRoot.name);
    }

    private Rigidbody FindNonKinematicRigidbodyAncestor(Transform transform)
    {
        var current = transform.parent;

        while (current != null)
        {
            var rigidbody = current.GetComponent<Rigidbody>();

            if (rigidbody != null && !rigidbody.isKinematic)
                return rigidbody;

            current = current.parent;
        }

        return null;
    }

    private void AssertPickupVisualAuthoring(Pickup pickup)
    {
        var renderers = pickup.GetComponentsInChildren<MeshRenderer>(true);
        var meshFilters = pickup.GetComponentsInChildren<MeshFilter>(true);
        var spinner = pickup.GetComponentInChildren<Spinner>(true);

        Assert.That(renderers, Has.Length.EqualTo(1), pickup.name);
        Assert.That(meshFilters, Has.Length.EqualTo(1), pickup.name);
        Assert.That(spinner, Is.Not.Null, pickup.name);
        Assert.That(spinner.transform, Is.SameAs(renderers[0].transform), pickup.name);
        Assert.That(meshFilters[0].sharedMesh, Is.SameAs(TestAssets.CoinPickupMesh), pickup.name);
        Assert.That(renderers[0].sharedMaterials, Has.Length.EqualTo(1), pickup.name);
        Assert.That(renderers[0].sharedMaterials[0], Is.SameAs(TestAssets.CoinPickupMaterial), pickup.name);
        Assert.That(renderers[0].sharedMaterials[0].shader, Is.Not.Null, pickup.name);
        Assert.That(renderers[0].sharedMaterials[0].shader.name, Does.Not.Contain("InternalErrorShader"), pickup.name);
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);

        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist.");
        return layer;
    }

    private readonly struct ExpectedPickupSensor
    {
        public string SensorName { get; }
        public Type ColliderType { get; }
        public string SourceBoneName { get; }

        public ExpectedPickupSensor(string sensorName, Type colliderType, string sourceBoneName)
        {
            SensorName = sensorName;
            ColliderType = colliderType;
            SourceBoneName = sourceBoneName;
        }
    }
}
