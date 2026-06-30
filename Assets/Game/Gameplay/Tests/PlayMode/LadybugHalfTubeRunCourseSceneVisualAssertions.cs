using System.Collections;
using System.Linq;
using Game.Gameplay;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private const string CameraObstacleLayerName = "CameraObstacle";

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_RooftopVisualDressingAndCameraLayersAreReadable()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var cameraObstacleLayer = GetRequiredLayer(CameraObstacleLayerName);

        AssertRunCameraLayerIntent(scene, cameraTerrainLayer, cameraObstacleLayer);
        AssertRooftopVisualDressing(scene, courseRoot, cameraTerrainLayer, cameraObstacleLayer);
        AssertObstacleVisualPairing(scene, courseRoot, cameraTerrainLayer, cameraObstacleLayer);
        AssertFinishVisualMarker(scene, courseRoot, runProgressFrameSource, cameraTerrainLayer, cameraObstacleLayer);
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_ReadabilityCameraSamplesMajorBeats_then_KeyObstaclesRampsAndFinishStayFramed()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var cameraObject = new GameObject("Ladybug Course Readability Test Camera");
        var camera = cameraObject.AddComponent<Camera>();

        camera.enabled = false;
        camera.fieldOfView = 62f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 90f;
        camera.aspect = 9f / 16f;

        try
        {
            AssertMajorBeatReadableFromApproach(scene, camera, cameraTerrainLayer, "Band 2 Obstacle 01 Center AC Blocker", 58f, 0f);
            AssertMajorBeatReadableFromApproach(scene, camera, cameraTerrainLayer, "Band 3 Section 08 Required Tutorial Ramp Surface", 145f, 0f);
            AssertMajorBeatReadableFromApproach(scene, camera, cameraTerrainLayer, "Band 4 Section 13 Optional Bank Ramp Surface", 290f, 0f);
            AssertMajorBeatReadableFromApproach(scene, camera, cameraTerrainLayer, "Band 5 Run Finish Visual Marker", 388f, 0f);
        }
        finally
        {
            Object.Destroy(cameraObject);
        }
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

    private void AssertRooftopVisualDressing(
        Scene scene,
        GameObject courseRoot,
        int cameraTerrainLayer,
        int cameraObstacleLayer)
    {
        var dressingRoot = FindGameObjectByName(scene, "Rooftop Visual Dressing");
        var renderers = dressingRoot.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.enabled).ToArray();

        Assert.That(dressingRoot.transform.IsChildOf(courseRoot.transform), Is.True);
        Assert.That(renderers, Has.Length.GreaterThanOrEqualTo(8), "Rooftop dressing should read as more than isolated props.");
        AssertVisualOnlyRoot(dressingRoot, cameraTerrainLayer, cameraObstacleLayer, "Rooftop Visual Dressing");
        AssertNamedDecoration(scene, dressingRoot, "Rooftop_Chunk_01 Edge Visual 030L", cameraTerrainLayer, cameraObstacleLayer);
        AssertNamedDecoration(scene, dressingRoot, "Rooftop_Chunk_02 Edge Visual 110R", cameraTerrainLayer, cameraObstacleLayer);
        AssertNamedDecoration(scene, dressingRoot, "Rooftop_Chunk_03_Drop Edge Visual 190L", cameraTerrainLayer, cameraObstacleLayer);
        AssertNamedDecoration(scene, dressingRoot, "Rooftop_Chunk_05_Step Edge Visual 330R", cameraTerrainLayer, cameraObstacleLayer);
        AssertNamedDecoration(scene, dressingRoot, "Obstacle_WaterTank Rooftop Prop 300L", cameraTerrainLayer, cameraObstacleLayer);
        AssertNamedDecoration(scene, dressingRoot, "Obstacle_SatDish Rooftop Prop 145R", cameraTerrainLayer, cameraObstacleLayer);
    }

    private void AssertNamedDecoration(
        Scene scene,
        GameObject dressingRoot,
        string decorationName,
        int cameraTerrainLayer,
        int cameraObstacleLayer)
    {
        var decoration = FindGameObjectByName(scene, decorationName);

        Assert.That(decoration.transform.IsChildOf(dressingRoot.transform), Is.True, decorationName);
        Assert.That(Mathf.Abs(decoration.transform.localPosition.x), Is.GreaterThanOrEqualTo(6f), decorationName);
        Assert.That(GetRendererBounds(decoration, decorationName).size.magnitude, Is.GreaterThan(0.5f), decorationName);
        AssertVisualOnlyRoot(decoration, cameraTerrainLayer, cameraObstacleLayer, decorationName);
    }

    private void AssertObstacleVisualPairing(
        Scene scene,
        GameObject courseRoot,
        int cameraTerrainLayer,
        int cameraObstacleLayer)
    {
        AssertObstacleVisual(scene, courseRoot, "Band 2 Obstacle 01 Center AC Blocker", "Obstacle_AC1 Visual", cameraTerrainLayer,
            cameraObstacleLayer);

        AssertObstacleVisual(scene, courseRoot, "Band 2 Obstacle 02 Left Offset Sunroof Blocker", "Obstacle_SunRoof Visual", cameraTerrainLayer,
            cameraObstacleLayer);

        AssertObstacleVisual(scene, courseRoot, "Band 3 Obstacle 03 Right Low Solar Blocker", "Obstacle_SolarPanels Visual", cameraTerrainLayer,
            cameraObstacleLayer);

        AssertObstacleVisual(scene, courseRoot, "Band 3 Obstacle 04 Left Bank AC Blocker", "Obstacle_AC2 Visual", cameraTerrainLayer,
            cameraObstacleLayer);

        AssertObstacleVisual(scene, courseRoot, "Band 4 Obstacle 05 Right Bank Billboard Blocker", "Obstacle_Billboard Visual", cameraTerrainLayer,
            cameraObstacleLayer);

        AssertObstacleVisual(scene, courseRoot, "Band 5 Obstacle 06 Left Finish Approach Solar Blocker", "Obstacle_SolarPanels Visual",
            cameraTerrainLayer, cameraObstacleLayer);
    }

    private void AssertObstacleVisual(
        Scene scene,
        GameObject courseRoot,
        string obstacleName,
        string visualName,
        int cameraTerrainLayer,
        int cameraObstacleLayer)
    {
        var obstacle = FindGameObjectByName(scene, obstacleName);
        var collider = obstacle.GetComponent<Collider>();
        var runContact = obstacle.GetComponent<RunContact>();
        var visual = FindDirectChild(obstacle, visualName);
        var visualBounds = GetRendererBounds(visual, $"{obstacleName} {visualName}");

        Assert.That(obstacle.transform.IsChildOf(courseRoot.transform), Is.True, obstacleName);
        Assert.That(obstacle.layer, Is.EqualTo(cameraObstacleLayer), obstacleName);
        Assert.That(collider, Is.Not.Null, obstacleName);
        Assert.That(collider.enabled, Is.True, obstacleName);
        Assert.That(collider.isTrigger, Is.False, obstacleName);
        Assert.That(runContact, Is.Not.Null, obstacleName);
        Assert.That(runContact.Category, Is.EqualTo(RunContactCategory.Obstacle), obstacleName);
        Assert.That(visualBounds.center.x, Is.EqualTo(collider.bounds.center.x).Within(1.25f), obstacleName);
        Assert.That(visualBounds.center.z, Is.EqualTo(collider.bounds.center.z).Within(1.25f), obstacleName);
        Assert.That(visualBounds.size.y, Is.GreaterThan(0.25f), obstacleName);
        AssertVisualOnlyRoot(visual, cameraTerrainLayer, cameraObstacleLayer, visualName);
    }

    private void AssertFinishVisualMarker(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource,
        int cameraTerrainLayer,
        int cameraObstacleLayer)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var marker = FindGameObjectByName(scene, "Band 5 Run Finish Visual Marker");
        var markerBounds = GetRendererBounds(marker, marker.name);
        var progress = frame.GetForwardProgress(markerBounds.center);

        Assert.That(marker.transform.IsChildOf(courseRoot.transform), Is.True);
        Assert.That(marker.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.enabled).ToArray(), Has.Length.GreaterThanOrEqualTo(3));
        Assert.That(markerBounds.size.x, Is.GreaterThanOrEqualTo(7f), marker.name);
        Assert.That(markerBounds.size.y, Is.GreaterThanOrEqualTo(2f), marker.name);
        Assert.That(progress, Is.InRange(410f, 420f), marker.name);
        AssertVisualOnlyRoot(marker, cameraTerrainLayer, cameraObstacleLayer, marker.name);
    }

    private void AssertVisualOnlyRoot(GameObject root, int cameraTerrainLayer, int cameraObstacleLayer, string description)
    {
        var runContacts = root.GetComponentsInChildren<RunContact>(true);
        var activeColliders = root.GetComponentsInChildren<Collider>(true).Where(collider => collider.enabled).ToArray();

        var cameraLayerObjects = root.GetComponentsInChildren<Transform>(true)
            .Where(child => child.gameObject.layer == cameraTerrainLayer || child.gameObject.layer == cameraObstacleLayer)
            .Select(child => child.name)
            .ToArray();

        Assert.That(runContacts, Is.Empty, description);
        Assert.That(activeColliders, Is.Empty, description);
        Assert.That(cameraLayerObjects, Is.Empty, description);
    }

    private void AssertMajorBeatReadableFromApproach(
        Scene scene,
        Camera camera,
        int cameraTerrainLayer,
        string targetName,
        float cameraProgress,
        float cameraLateral)
    {
        var target = FindGameObjectByName(scene, targetName);
        var targetBounds = GetRendererBounds(target, targetName);
        var cameraSurfacePoint = SampleCourseSurfacePoint(cameraTerrainLayer, cameraProgress, cameraLateral, targetName);
        var cameraPosition = cameraSurfacePoint + Vector3.up * 2.35f;
        var lookDirection = targetBounds.center - cameraPosition;

        Assert.That(lookDirection.sqrMagnitude, Is.GreaterThan(0.01f), targetName);

        camera.transform.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(lookDirection.normalized, Vector3.up));
        AssertViewportReadable(camera, targetBounds, targetName);
    }

    private Vector3 SampleCourseSurfacePoint(int cameraTerrainLayer, float progress, float lateral, string description)
    {
        var rayOrigin = new Vector3(lateral, 64f, progress);
        var terrainLayerMask = 1 << cameraTerrainLayer;

        Assert.That(
            Physics.Raycast(rayOrigin, Vector3.down, out var hit, 512f, terrainLayerMask, QueryTriggerInteraction.Ignore),
            Is.True,
            $"Expected readability camera approach point for {description} to hit the course surface.");

        return hit.point;
    }

    private void AssertViewportReadable(Camera camera, Bounds bounds, string description)
    {
        var center = camera.WorldToViewportPoint(bounds.center);
        var points = GetBoundsCorners(bounds).Select(camera.WorldToViewportPoint).ToArray();

        Assert.That(center.z, Is.GreaterThan(0f), description);
        Assert.That(center.x, Is.InRange(0.08f, 0.92f), description);
        Assert.That(center.y, Is.InRange(0.08f, 0.92f), description);
        Assert.That(points.All(point => point.z > 0f), Is.True, description);

        var visibleMinX = Mathf.Clamp01(points.Min(point => point.x));
        var visibleMaxX = Mathf.Clamp01(points.Max(point => point.x));
        var visibleMinY = Mathf.Clamp01(points.Min(point => point.y));
        var visibleMaxY = Mathf.Clamp01(points.Max(point => point.y));

        Assert.That(visibleMaxX - visibleMinX, Is.GreaterThanOrEqualTo(0.035f), description);
        Assert.That(visibleMaxY - visibleMinY, Is.GreaterThanOrEqualTo(0.035f), description);
    }

    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        var min = bounds.min;
        var max = bounds.max;

        return new[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };
    }

    private GameObject FindDirectChild(GameObject parent, string childName)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == childName)
                return child.gameObject;
        }

        Assert.Fail($"Expected '{parent.name}' to have direct child '{childName}'.");
        return null;
    }

    private Bounds GetRendererBounds(GameObject root, string description)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.enabled).ToArray();

        Assert.That(renderers, Has.Length.GreaterThanOrEqualTo(1), description);

        var bounds = renderers[0].bounds;

        for (var rendererIndex = 1; rendererIndex < renderers.Length; rendererIndex += 1)
        {
            bounds.Encapsulate(renderers[rendererIndex].bounds);
        }

        return bounds;
    }
}
