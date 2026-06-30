using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private const float TraversalSampleSpacing = 3.5f;
    private const float PlayerClearanceRadius = 0.25f;
    private const float PlayerCapsuleBottomHeight = 0.25f;
    private const float PlayerCapsuleTopHeight = 1.75f;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_PlayerSizedSafeRouteIsSampled_then_FinishHasContinuousTraversableLane()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var cameraObstacleLayer = GetRequiredLayer(CameraObstacleLayerName);

        var routeAnchors = CreateReferenceSafeRouteAnchors();

        AssertContinuousPlayerClearance(scene, courseRoot, cameraTerrainLayer, cameraObstacleLayer, routeAnchors);
        AssertRunFinishContact(scene, courseRoot, runProgressFrameSource, "Band 5 Run Finish");
    }

    private void AssertContinuousPlayerClearance(
        Scene scene,
        GameObject courseRoot,
        int cameraTerrainLayer,
        int cameraObstacleLayer,
        (float Progress, float Lateral, string Description)[] routeAnchors)
    {
        Assert.That(routeAnchors, Has.Length.GreaterThan(1));

        for (var anchorIndex = 0; anchorIndex < routeAnchors.Length - 1; anchorIndex += 1)
        {
            var start = routeAnchors[anchorIndex];
            var end = routeAnchors[anchorIndex + 1];
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt((end.Progress - start.Progress) / TraversalSampleSpacing));

            for (var sampleIndex = 0; sampleIndex <= sampleCount; sampleIndex += 1)
            {
                var interpolation = sampleIndex / (float)sampleCount;
                var progress = Mathf.Lerp(start.Progress, end.Progress, interpolation);
                var lateral = Mathf.Lerp(start.Lateral, end.Lateral, interpolation);
                var description = $"{start.Description} to {end.Description} at {progress:0.#}m / x={lateral:0.#}";

                AssertPlayerClearanceAtRouteSample(scene, courseRoot, cameraTerrainLayer, cameraObstacleLayer, progress, lateral, description);
            }
        }
    }

    private void AssertPlayerClearanceAtRouteSample(
        Scene scene,
        GameObject courseRoot,
        int cameraTerrainLayer,
        int cameraObstacleLayer,
        float progress,
        float lateral,
        string description)
    {
        Assert.That(Mathf.Abs(lateral), Is.LessThanOrEqualTo(4.25f), description);

        var surfacePoint = SampleTraversableSurfacePoint(scene, courseRoot, cameraTerrainLayer, progress, lateral, description);
        var obstacleLayerMask = 1 << cameraObstacleLayer;
        var capsuleBottom = surfacePoint + (Vector3.up * PlayerCapsuleBottomHeight);
        var capsuleTop = surfacePoint + (Vector3.up * PlayerCapsuleTopHeight);

        var overlapsObstacle = Physics.CheckCapsule(
            capsuleBottom,
            capsuleTop,
            PlayerClearanceRadius,
            obstacleLayerMask,
            QueryTriggerInteraction.Ignore);

        Assert.That(overlapsObstacle, Is.False, description);
    }

    private Vector3 SampleTraversableSurfacePoint(
        Scene scene,
        GameObject courseRoot,
        int cameraTerrainLayer,
        float progress,
        float lateral,
        string description)
    {
        var rayOrigin = new Vector3(lateral, 64f, progress);
        var terrainLayerMask = 1 << cameraTerrainLayer;

        Assert.That(
            Physics.Raycast(rayOrigin, Vector3.down, out var hit, 512f, terrainLayerMask, QueryTriggerInteraction.Ignore),
            Is.True,
            description);

        var runContact = hit.collider.GetComponent<RunContact>();

        Assert.That(hit.collider.transform.IsChildOf(courseRoot.transform), Is.True, description);
        Assert.That(hit.collider.gameObject.scene, Is.EqualTo(scene), description);
        Assert.That(runContact, Is.Not.Null, description);
        Assert.That(runContact.Category, Is.EqualTo(RunContactCategory.Surface), description);

        return hit.point;
    }
}
