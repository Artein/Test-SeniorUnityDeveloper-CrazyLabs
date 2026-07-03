using System.Collections;
using System.Linq;
using Game.Foundation.Physics;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_AuthoredPickupsSatisfyRuntimeContract()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;

        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var pickupLayer = GetRequiredLayer(_pickupLayerName);

        AssertPickupContracts(scene, courseRoot, pickupLayer);
    }

    private void AssertPickupContracts(Scene scene, GameObject courseRoot, int pickupLayer)
    {
        var coursePickups = FindComponentsInScene<Pickup>(scene)
            .Where(pickup => pickup.transform.IsChildOf(courseRoot.transform))
            .ToArray();

        foreach (var pickup in coursePickups)
        {
            var colliders = pickup.GetComponentsInChildren<Collider>(true);
            var triggerNotifier = pickup.TriggerNotifierForTests;

            Assert.That(pickup.gameObject.activeSelf, Is.True, pickup.name);
            Assert.That(pickup.gameObject.layer, Is.EqualTo(pickupLayer), pickup.name);
            Assert.That(pickup.Definition, Is.Not.Null, pickup.name);
            Assert.That(pickup.Definition!.CurrencyDefinition, Is.Not.Null, pickup.name);
            Assert.That(triggerNotifier, Is.Not.Null, pickup.name);
            Assert.That(triggerNotifier.transform.IsChildOf(pickup.transform), Is.True, pickup.name);
            Assert.That(colliders, Has.Length.GreaterThanOrEqualTo(1), pickup.name);

            pickup.Definition.Validate();
            AssertPickupNotifierContract(pickup, triggerNotifier, pickupLayer);

            foreach (var collider in colliders)
            {
                Assert.That(collider.isTrigger, Is.True, collider.name);
                Assert.That(collider.gameObject.layer, Is.EqualTo(pickupLayer), collider.name);
            }
        }
    }

    private void AssertPickupNotifierContract(Pickup pickup, TriggerNotifier triggerNotifier, int pickupLayer)
    {
        var notifierColliders = triggerNotifier.GetComponents<Collider>();

        Assert.That(notifierColliders, Has.Length.GreaterThanOrEqualTo(1), pickup.name);

        foreach (var collider in notifierColliders)
        {
            Assert.That(collider.enabled, Is.True, collider.name);
            Assert.That(collider.isTrigger, Is.True, collider.name);
            Assert.That(collider.gameObject.layer, Is.EqualTo(pickupLayer), collider.name);
        }
    }
}
