using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_CourseVisualsAndCameraLayersSatisfyRuntimeContract()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;

        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var cameraObstacleLayer = GetRequiredLayer(_cameraObstacleLayerName);
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);

        AssertRunCameraLayerIntent(scene, cameraTerrainLayer, cameraObstacleLayer);
        AssertCourseRendererMaterialsAreValid(courseRoot);
    }

    private void AssertRunCameraLayerIntent(Scene scene, int cameraTerrainLayer, int cameraObstacleLayer)
    {
        var runCamera = FindGameObjectByName(scene, "Run Camera").GetComponent<CinemachineCamera>();
        var decollider = runCamera.GetComponent<CinemachineDecollider>();

        Assert.That(runCamera, Is.Not.Null);
        Assert.That(decollider, Is.Not.Null);
        Assert.That(decollider.TerrainResolution.Enabled, Is.True);
        Assert.That(decollider.TerrainResolution.TerrainLayers.value, Is.EqualTo(1 << cameraTerrainLayer));
        Assert.That(decollider.Decollision.Enabled, Is.True);
        Assert.That(decollider.Decollision.ObstacleLayers.value, Is.EqualTo((1 << cameraTerrainLayer) | (1 << cameraObstacleLayer)));
    }

    private void AssertCourseRendererMaterialsAreValid(GameObject courseRoot)
    {
        var enabledRenderers = courseRoot.GetComponentsInChildren<Renderer>(true)
            .Where(renderer => renderer.enabled)
            .ToArray();

        foreach (var renderer in enabledRenderers)
            AssertRendererMaterialsAreValid(renderer);
    }

    private void AssertRendererMaterialsAreValid(Renderer renderer)
    {
        if (renderer is ParticleSystemRenderer particleRenderer)
        {
            AssertParticleRendererMaterialsAreValid(particleRenderer);
            return;
        }

        AssertSharedMaterialsAreValid(renderer);
    }

    private void AssertParticleRendererMaterialsAreValid(ParticleSystemRenderer renderer)
    {
        var rendererPath = GetHierarchyPath(renderer.transform);
        var particleSystem = renderer.GetComponent<ParticleSystem>();

        Assert.That(particleSystem, Is.Not.Null, $"{rendererPath} particle system");
        AssertMaterialIsValid(renderer.sharedMaterial, $"{rendererPath} particle material");

        if (particleSystem!.trails.enabled)
            AssertMaterialIsValid(renderer.trailMaterial, $"{rendererPath} trail material");

        foreach (var material in renderer.sharedMaterials)
        {
            if (material != null)
                AssertMaterialIsValid(material, $"{rendererPath} assigned material");
        }
    }

    private void AssertSharedMaterialsAreValid(Renderer renderer)
    {
        var materials = renderer.sharedMaterials;
        var rendererPath = GetHierarchyPath(renderer.transform);

        Assert.That(materials, Has.Length.GreaterThanOrEqualTo(1), rendererPath);

        for (var materialIndex = 0; materialIndex < materials.Length; materialIndex += 1)
            AssertMaterialIsValid(materials[materialIndex], $"{rendererPath} material {materialIndex}");
    }

    private static void AssertMaterialIsValid(Material material, string description)
    {
        Assert.That(material, Is.Not.Null, description);
        Assert.That(material!.shader, Is.Not.Null, description);
        Assert.That(material.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"), description);
    }

    private string GetHierarchyPath(Transform transform)
    {
        var path = transform.name;
        var parent = transform.parent;

        while (parent != null)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }

        return path;
    }
}
