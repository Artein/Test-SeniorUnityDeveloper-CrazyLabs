using System.Collections;
using System.Linq;
using Game.Gameplay;
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
    private const string PlayerLayerName = "Player";
    private const string PickupLayerName = "Pickup";
    private const string PlayerTag = "Player";

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_PickupCurrencyCompositionIsReady()
    {
        yield return LoadGameplaySceneWithIsolatedSaves();

        var scene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var playerLayer = GetRequiredLayer(PlayerLayerName);
        var pickupLayer = GetRequiredLayer(PickupLayerName);
        var allScenePickups = FindComponentsInScene<Pickup>(scene);
        var configuredPickups = lifetimeScope.LevelPickupsForTests.ToArray();
        var playerContactColliders = lifetimeScope.PlayerPickupContactCollidersForTests.ToArray();
        var levelPickupStateService = lifetimeScope.Container.Resolve<ILevelPickupState>();
        var levelPickupState = lifetimeScope.Container.Resolve<ILevelPickupState>();
        var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();
        var runCurrencyAccumulator = lifetimeScope.Container.Resolve<IRunCurrencyAccumulator>();
        var pickupCurrencyGrantResolver = lifetimeScope.Container.Resolve<IPickupCurrencyGrantResolver>();
        var pickupCollectionNotifier = lifetimeScope.Container.Resolve<IPickupCollectionNotifier>();

        Assert.That(scene.path, Is.EqualTo(TestAssets.GameplaySceneRef.Path));
        Assert.That(lifetimeScope.PickupSetupValidationErrorsForTests, Is.Empty);
        Assert.That(Physics.GetIgnoreLayerCollision(playerLayer, pickupLayer), Is.False);
        Assert.That(configuredPickups, Has.Length.EqualTo(2));
        Assert.That(playerContactColliders, Has.Length.EqualTo(1));
        Assert.That(levelPickupStateService, Is.SameAs(levelPickupState));
        Assert.That(levelPickupState, Is.Not.Null);
        Assert.That(currencyStorage, Is.Not.Null);
        Assert.That(runCurrencyAccumulator, Is.Not.Null);
        Assert.That(pickupCurrencyGrantResolver, Is.Not.Null);
        Assert.That(pickupCollectionNotifier, Is.Not.Null);

        foreach (var configuredPickup in configuredPickups)
        {
            Assert.That(allScenePickups, Does.Contain(configuredPickup));
            Assert.That(levelPickupState.IsAvailable(configuredPickup), Is.True, configuredPickup.name);
        }

        var regularCoinPickup = FindConfiguredPickup(configuredPickups, 1, "regular coin pickup");
        var bigCoinPickup = FindConfiguredPickup(configuredPickups, 5, "big coin pickup");

        AssertPickupAuthoring(regularCoinPickup, pickupLayer, 1);
        AssertPickupAuthoring(bigCoinPickup, pickupLayer, 5);
        Assert.That(regularCoinPickup.Definition.CurrencyDefinition, Is.SameAs(bigCoinPickup.Definition.CurrencyDefinition));

        var playerContactCollider = playerContactColliders[0];
        Assert.That(playerContactCollider.gameObject.layer, Is.EqualTo(playerLayer));
        Assert.That(playerContactCollider.gameObject.CompareTag(PlayerTag), Is.True);
    }

    private Pickup FindConfiguredPickup(Pickup[] configuredPickups, int expectedAmount, string description)
    {
        var matches = configuredPickups
            .Where(candidate => candidate != null
                                && candidate.Definition != null
                                && candidate.Definition.Amount == expectedAmount)
            .ToArray();

        Assert.That(matches, Has.Length.EqualTo(1), $"Expected one configured {description} with amount {expectedAmount}.");
        return matches[0];
    }

    private void AssertPickupAuthoring(Pickup pickup, int pickupLayer, int expectedAmount)
    {
        var colliders = pickup.GetComponentsInChildren<Collider>(true);

        Assert.That(pickup.gameObject.activeSelf, Is.True, pickup.name);
        Assert.That(pickup.gameObject.layer, Is.EqualTo(pickupLayer), pickup.name);
        Assert.That(pickup.Definition, Is.Not.Null, pickup.name);
        Assert.That(pickup.Definition.Amount, Is.EqualTo(expectedAmount), pickup.name);
        Assert.That(pickup.Definition.CurrencyDefinition, Is.Not.Null, pickup.name);
        Assert.That(colliders, Has.Length.GreaterThanOrEqualTo(1), pickup.name);

        pickup.Definition.Validate();

        foreach (var collider in colliders)
        {
            Assert.That(collider.isTrigger, Is.True, collider.name);
            Assert.That(collider.gameObject.layer, Is.EqualTo(pickupLayer), collider.name);
        }
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);

        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist.");
        return layer;
    }
}
