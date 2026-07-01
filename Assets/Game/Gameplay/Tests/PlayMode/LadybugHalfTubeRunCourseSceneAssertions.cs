using System;
using System.Linq;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private void AssertTerrainSurfaceContracts(Scene scene, GameObject courseRoot, int cameraTerrainLayer)
    {
        var surfaceColliders = courseRoot.GetComponentsInChildren<TerrainCollider>(true);

        Assert.That(
            surfaceColliders,
            Has.Length.GreaterThan(0),
            "Ladybug course must contain at least one authored terrain surface.");

        foreach (var terrainCollider in surfaceColliders)
        {
            AssertTerrainSurfaceContract(scene, courseRoot, terrainCollider, cameraTerrainLayer);
        }
    }

    private void AssertTerrainSurfaceContract(
        Scene scene,
        GameObject courseRoot,
        TerrainCollider terrainCollider,
        int cameraTerrainLayer)
    {
        var surfaceObject = terrainCollider.gameObject;
        var terrain = surfaceObject.GetComponent<Terrain>();
        var runContact = surfaceObject.GetComponent<RunContact>();

        Assert.That(surfaceObject.scene, Is.EqualTo(scene), surfaceObject.name);
        Assert.That(surfaceObject.transform.IsChildOf(courseRoot.transform), Is.True, surfaceObject.name);
        Assert.That(surfaceObject.layer, Is.EqualTo(cameraTerrainLayer), surfaceObject.name);
        Assert.That(terrain, Is.Not.Null, surfaceObject.name);
        Assert.That(terrainCollider.enabled, Is.True, surfaceObject.name);
        Assert.That(terrainCollider.sharedMaterial, Is.Not.Null, surfaceObject.name);
        Assert.That(terrain!.terrainData, Is.Not.Null, surfaceObject.name);
        Assert.That(terrainCollider.terrainData, Is.SameAs(terrain.terrainData), surfaceObject.name);
        Assert.That(surfaceObject.GetComponent<MeshFilter>(), Is.Null, surfaceObject.name);
        Assert.That(surfaceObject.GetComponent<MeshRenderer>(), Is.Null, surfaceObject.name);
        Assert.That(surfaceObject.GetComponent<MeshCollider>(), Is.Null, surfaceObject.name);
        Assert.That(runContact, Is.Not.Null, surfaceObject.name);
        Assert.That(runContact!.Category, Is.EqualTo(RunContactCategory.Surface), surfaceObject.name);
    }

    private void AssertObstacleContracts(Scene scene, GameObject courseRoot, int cameraObstacleLayer)
    {
        var obstacles = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.transform.IsChildOf(courseRoot.transform))
            .Where(contact => contact.Category == RunContactCategory.Obstacle)
            .ToArray();

        foreach (var obstacle in obstacles)
        {
            var collider = obstacle.GetComponent<Collider>();

            Assert.That(obstacle.gameObject.scene, Is.EqualTo(scene), obstacle.name);
            Assert.That(obstacle.gameObject.layer, Is.EqualTo(cameraObstacleLayer), obstacle.name);
            Assert.That(collider, Is.Not.Null, obstacle.name);
            Assert.That(collider!.enabled, Is.True, obstacle.name);
            Assert.That(collider.isTrigger, Is.False, obstacle.name);
        }
    }

    private void AssertSingleRunFinishContact(Scene scene, GameObject courseRoot)
    {
        var finishContacts = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.transform.IsChildOf(courseRoot.transform))
            .Where(contact => contact.Category == RunContactCategory.Finish)
            .ToArray();

        Assert.That(finishContacts, Has.Length.EqualTo(1), "The Ladybug course should expose one finish contact.");

        var finishContact = finishContacts[0];
        var collider = finishContact.GetComponent<Collider>();
        var classifier = new RunContactClassifier(new FakeRunEndConfig());

        Assert.That(finishContact.gameObject.scene, Is.EqualTo(scene), finishContact.name);
        Assert.That(collider, Is.Not.Null, finishContact.name);
        Assert.That(collider!.enabled, Is.True, finishContact.name);
        Assert.That(collider.isTrigger, Is.True, finishContact.name);
        Assert.That(classifier.TryClassify(new RigidbodyTriggerNotification(collider), out var candidate), Is.True, finishContact.name);
        Assert.That(candidate.Reason, Is.EqualTo(RunEndReason.Finished), finishContact.name);
    }

    private void AssertNoGeneratedMeshCourseSurfaces(Scene scene, GameObject courseRoot)
    {
        var generatedMeshSurfaceNames = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.transform.IsChildOf(courseRoot.transform))
            .Where(contact => contact.Category == RunContactCategory.Surface)
            .Where(contact =>
                contact.GetComponent<MeshFilter>() != null ||
                contact.GetComponent<MeshRenderer>() != null ||
                contact.GetComponent<MeshCollider>() != null ||
                contact.name.Contains("Generated", StringComparison.OrdinalIgnoreCase))
            .Select(contact => contact.name)
            .ToArray();

        Assert.That(generatedMeshSurfaceNames, Is.Empty, "Play Mode should use authored Terrain surfaces, not generated mesh sections.");
    }

    private T FindSingleInScene<T>(Scene scene, string objectDescription)
        where T : Component
    {
        var results = FindComponentsInScene<T>(scene);

        Assert.That(results, Has.Length.EqualTo(1), objectDescription);
        return results[0];
    }

    private T[] FindComponentsInScene<T>(Scene scene)
        where T : Component
    {
        return scene.GetRootGameObjects()
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<T>(true))
            .ToArray();
    }

    private GameObject FindGameObjectByName(Scene scene, string objectName)
    {
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            var transforms = rootGameObject.GetComponentsInChildren<Transform>(true);

            foreach (var childTransform in transforms)
            {
                if (childTransform.name == objectName)
                    return childTransform.gameObject;
            }
        }

        Assert.Fail($"Expected scene object '{objectName}' to exist.");
        return null;
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);

        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist.");
        return layer;
    }

    private sealed class FakeRunEndConfig : IRunEndConfig
    {
        public float ObstacleImpactSpeedThreshold => 5f;
        public float LostMomentumLaunchGraceDuration => 0.1f;
        public float LostMomentumDuration => 0.2f;
        public float LostMomentumPlanarSpeedThreshold => 0.5f;
        public float LostMomentumProgressThreshold => 0.05f;
        public float RunEndedAcknowledgeGuardDuration => 0.25f;
    }
}
