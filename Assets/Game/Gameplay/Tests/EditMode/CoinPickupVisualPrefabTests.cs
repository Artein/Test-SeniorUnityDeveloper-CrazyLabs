using System.Linq;
using Game.Foundation.Physics;
using Game.Foundation.Presentation;
using Game.Gameplay.Pickups;
using Game.Gameplay.Tests.Common;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class CoinPickupVisualPrefabTests
{
    [Test]
    public void CoinPickupPrefabs_WhenLoadedFromProvider_UseProjectOwnedVisualAssets()
    {
        var provider = GameplayTestAssetsProvider.LoadSingleFromAssetDatabase();

        Assert.That(provider.CoinPickupPrefabs, Is.Not.Null.And.Not.Empty);

        foreach (var coinPickupPrefab in provider.CoinPickupPrefabs)
        {
            AssertCoinPickupVisualContract(coinPickupPrefab, provider);
        }
    }

    private void AssertCoinPickupVisualContract(GameObject coinPickupPrefab, GameplayTestAssetsProvider provider)
    {
        var pickup = coinPickupPrefab.GetComponent<Pickup>();
        var triggerColliders = coinPickupPrefab.GetComponentsInChildren<Collider>(true);
        var renderers = coinPickupPrefab.GetComponentsInChildren<MeshRenderer>(true);
        var meshFilters = coinPickupPrefab.GetComponentsInChildren<MeshFilter>(true);
        var spinner = coinPickupPrefab.GetComponentInChildren<Spinner>(true);

        Assert.That(pickup, Is.Not.Null, coinPickupPrefab.name);
        var triggerNotifier = pickup.TriggerNotifierForTests;
        Assert.That(pickup.Definition, Is.Not.Null, coinPickupPrefab.name);
        Assert.That(pickup.Definition.CurrencyDefinition, Is.Not.Null, coinPickupPrefab.name);
        Assert.That(triggerNotifier, Is.Not.Null, coinPickupPrefab.name);
        Assert.That(triggerNotifier.transform.IsChildOf(pickup.transform), Is.True, coinPickupPrefab.name);
        Assert.That(coinPickupPrefab.transform.localScale, Is.EqualTo(Vector3.one), coinPickupPrefab.name);
        Assert.That(triggerColliders, Has.Length.GreaterThanOrEqualTo(1), coinPickupPrefab.name);
        Assert.That(renderers, Has.Length.EqualTo(1), coinPickupPrefab.name);
        Assert.That(meshFilters, Has.Length.EqualTo(1), coinPickupPrefab.name);
        Assert.That(spinner, Is.Not.Null, coinPickupPrefab.name);
        Assert.That(spinner.transform, Is.SameAs(renderers[0].transform), coinPickupPrefab.name);

        pickup.Definition.Validate();
        AssertNotifierColliderContract(pickup, triggerNotifier);

        foreach (var collider in triggerColliders)
        {
            Assert.That(collider.enabled, Is.True, collider.name);
            Assert.That(collider.isTrigger, Is.True, collider.name);
            Assert.That(collider.transform.IsChildOf(pickup.transform), Is.True, collider.name);
        }

        var meshFilter = meshFilters[0];
        Assert.That(meshFilter.sharedMesh, Is.SameAs(provider.CoinPickupMesh), coinPickupPrefab.name);
        Assert.That(meshFilter.sharedMesh.bounds.size.sqrMagnitude, Is.GreaterThan(0f), coinPickupPrefab.name);

        var renderer = renderers[0];
        Assert.That(renderer.sharedMaterials, Has.Length.EqualTo(1), coinPickupPrefab.name);
        Assert.That(renderer.sharedMaterials[0], Is.SameAs(provider.CoinPickupMaterial), coinPickupPrefab.name);
        Assert.That(renderer.sharedMaterials[0].shader, Is.Not.Null, coinPickupPrefab.name);
        Assert.That(renderer.sharedMaterials[0].shader.name, Does.Not.Contain("InternalErrorShader"), coinPickupPrefab.name);

        Assert.That(coinPickupPrefab.GetComponentsInChildren<Spinner>(true).Where(candidate => candidate != null).ToArray(), Has.Length.EqualTo(1),
            coinPickupPrefab.name);
    }

    private void AssertNotifierColliderContract(Pickup pickup, TriggerNotifier triggerNotifier)
    {
        var notifierColliders = triggerNotifier.GetComponents<Collider>();

        Assert.That(notifierColliders, Has.Length.GreaterThanOrEqualTo(1), pickup.name);

        foreach (var collider in notifierColliders)
        {
            Assert.That(collider.enabled, Is.True, collider.name);
            Assert.That(collider.isTrigger, Is.True, collider.name);
        }
    }
}
