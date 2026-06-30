using System;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private void AssertSurfaceSections(
        Scene scene,
        GameObject courseRoot,
        int cameraTerrainLayer,
        (string Name, float StartProgress, float EndProgress, float MinimumPitch, float MaximumPitch)[] sections,
        float expectedStartProgress,
        float expectedEndProgress)
    {
        var previousEndProgress = expectedStartProgress;

        for (var sectionIndex = 0; sectionIndex < sections.Length; sectionIndex += 1)
        {
            var section = sections[sectionIndex];
            var meshCollider = AssertSurfaceObject(scene, courseRoot, cameraTerrainLayer, section.Name);

            Assert.That(meshCollider.bounds.size.x, Is.InRange(9.5f, 10.5f), section.Name);
            Assert.That(meshCollider.bounds.size.z, Is.EqualTo(section.EndProgress - section.StartProgress).Within(0.75f), section.Name);
            Assert.That(section.StartProgress, Is.EqualTo(previousEndProgress).Within(0.01f), section.Name);

            previousEndProgress = section.EndProgress;
        }

        Assert.That(previousEndProgress, Is.EqualTo(expectedEndProgress).Within(0.01f));
    }

    private MeshCollider AssertSurfaceObject(Scene scene, GameObject courseRoot, int cameraTerrainLayer, string surfaceName)
    {
        var surfaceObject = FindGameObjectByName(scene, surfaceName);
        var meshCollider = surfaceObject.GetComponent<MeshCollider>();
        var meshFilter = surfaceObject.GetComponent<MeshFilter>();
        var meshRenderer = surfaceObject.GetComponent<MeshRenderer>();
        var runContact = surfaceObject.GetComponent<RunContact>();

        Assert.That(surfaceObject.transform.IsChildOf(courseRoot.transform), Is.True, surfaceName);
        Assert.That(surfaceObject.layer, Is.EqualTo(cameraTerrainLayer), surfaceName);
        Assert.That(meshCollider, Is.Not.Null, surfaceName);
        Assert.That(meshCollider.sharedMesh, Is.Not.Null, surfaceName);
        Assert.That(meshFilter, Is.Not.Null, surfaceName);
        Assert.That(meshFilter.sharedMesh, Is.SameAs(meshCollider.sharedMesh), surfaceName);
        Assert.That(meshRenderer, Is.Not.Null, surfaceName);
        Assert.That(meshRenderer.sharedMaterial, Is.Not.Null, surfaceName);
        Assert.That(runContact, Is.Not.Null, surfaceName);
        Assert.That(runContact.Category, Is.EqualTo(RunContactCategory.Surface), surfaceName);

        return meshCollider;
    }

    private void AssertSurfaceBounds(
        RunProgressFrameSource runProgressFrameSource,
        Collider collider,
        float expectedStartProgress,
        float expectedEndProgress,
        string surfaceDescription)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var boundsStartProgress = frame.GetForwardProgress(collider.bounds.min);
        var boundsEndProgress = frame.GetForwardProgress(collider.bounds.max);
        var minimumProgress = Mathf.Min(boundsStartProgress, boundsEndProgress);
        var maximumProgress = Mathf.Max(boundsStartProgress, boundsEndProgress);

        Assert.That(minimumProgress, Is.EqualTo(expectedStartProgress).Within(0.75f), surfaceDescription);
        Assert.That(maximumProgress, Is.EqualTo(expectedEndProgress).Within(0.75f), surfaceDescription);
    }

    private void AssertNoUnsupportedRunContactCategories()
    {
        var categoryNames = Enum.GetNames(typeof(RunContactCategory));

        Assert.That(categoryNames, Does.Not.Contain("Ramp"));
        Assert.That(categoryNames, Does.Not.Contain("Boundary"));
    }

    private void AssertVisibleRampLineCue(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource)
    {
        var cuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 3 Required Ramp Coin Arc Cue",
            5,
            154f,
            164f,
            180f,
            190f);
        var cueProgresses = GetCueProgresses(runProgressFrameSource, cuePositions);

        Assert.That(cueProgresses.Min(), Is.InRange(154f, 164f), "Ramp cue should start before takeoff.");
        Assert.That(cueProgresses.Max(), Is.InRange(180f, 190f), "Ramp cue should carry the intended takeoff line.");
        Assert.That(cuePositions.Select(position => Mathf.Abs(position.x)).Max(), Is.LessThanOrEqualTo(0.75f));
        Assert.That(cuePositions.Max(position => position.y) - cuePositions.Min(position => position.y), Is.GreaterThan(0.4f));
    }

    private void AssertBandThreeCueLines(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource)
    {
        var centerCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 3 Center Fallback Coin Cue",
            3,
            204f,
            212f,
            218f,
            224f);

        var bankCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 3 Bank Reward Coin Cue",
            6,
            228f,
            234f,
            244f,
            250f);

        Assert.That(centerCuePositions.Select(position => Mathf.Abs(position.x)).Max(), Is.LessThanOrEqualTo(0.75f));
        Assert.That(bankCuePositions.Select(position => Mathf.Abs(position.x)).Min(), Is.GreaterThanOrEqualTo(3.4f));
        Assert.That(bankCuePositions.Length, Is.GreaterThan(centerCuePositions.Length), "Bank reward cue should read richer than center fallback.");
    }

    private void AssertBandFourCueLines(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource)
    {
        var centerCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 4 Center Fallback Coin Cue",
            5,
            250f,
            258f,
            274f,
            282f);

        var sideCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 4 Side Transfer Reward Coin Cue",
            5,
            252f,
            260f,
            274f,
            282f);

        var glideCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 4 Reach Pressure Sparse Coin Cue",
            3,
            284f,
            292f,
            304f,
            312f);

        var rampCuePositions = AssertCoinCueLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 4 Optional Ramp Reward Coin Cue",
            6,
            304f,
            312f,
            334f,
            344f);

        Assert.That(centerCuePositions.Select(position => Mathf.Abs(position.x)).Max(), Is.LessThanOrEqualTo(0.75f));
        Assert.That(sideCuePositions.Select(position => Mathf.Abs(position.x)).Min(), Is.GreaterThanOrEqualTo(3.2f));
        Assert.That(glideCuePositions.Select(position => Mathf.Abs(position.x)).Max(), Is.LessThanOrEqualTo(1.25f));
        Assert.That(glideCuePositions.Length, Is.LessThan(centerCuePositions.Length), "Reach-pressure glide should use sparse visible rewards.");
        Assert.That(rampCuePositions.Select(position => position.x).Min(), Is.GreaterThanOrEqualTo(3.2f));
        Assert.That(rampCuePositions.Max(position => position.y) - rampCuePositions.Min(position => position.y), Is.GreaterThan(0.45f));
    }

    private Vector3[] AssertCoinCueLine(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource,
        string cueRootName,
        int minimumCueCount,
        float minimumStartProgress,
        float maximumStartProgress,
        float minimumEndProgress,
        float maximumEndProgress)
    {
        var cueRoot = FindGameObjectByName(scene, cueRootName);
        var pickups = cueRoot.GetComponentsInChildren<Pickup>(true);
        var renderers = cueRoot.GetComponentsInChildren<MeshRenderer>(true);

        var cuePositions = pickups.Length > 0
            ? pickups.Select(pickup => pickup.Position).ToArray()
            : renderers.Select(renderer => renderer.bounds.center).ToArray();
        var cueProgresses = GetCueProgresses(runProgressFrameSource, cuePositions);

        Assert.That(cueRoot.transform.IsChildOf(courseRoot.transform), Is.True, cueRootName);
        Assert.That(cuePositions, Has.Length.GreaterThanOrEqualTo(minimumCueCount), cueRootName);
        Assert.That(cueProgresses.Min(), Is.InRange(minimumStartProgress, maximumStartProgress), cueRootName);
        Assert.That(cueProgresses.Max(), Is.InRange(minimumEndProgress, maximumEndProgress), cueRootName);

        return cuePositions;
    }

    private float[] GetCueProgresses(RunProgressFrameSource runProgressFrameSource, Vector3[] cuePositions)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        return cuePositions.Select(frame.GetForwardProgress).ToArray();
    }

    private Collider AssertObstacle(
        Scene scene,
        RunProgressFrameSource runProgressFrameSource,
        string obstacleName,
        float minimumProgress,
        float maximumProgress)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var obstacle = FindGameObjectByName(scene, obstacleName);
        var collider = obstacle.GetComponent<Collider>();
        var runContact = obstacle.GetComponent<RunContact>();

        Assert.That(collider, Is.Not.Null, obstacleName);
        Assert.That(collider.isTrigger, Is.False, obstacleName);
        Assert.That(runContact, Is.Not.Null, obstacleName);
        Assert.That(runContact.Category, Is.EqualTo(RunContactCategory.Obstacle), obstacleName);
        Assert.That(collider.gameObject.scene, Is.EqualTo(scene), obstacleName);
        Assert.That(frame.GetForwardProgress(collider.bounds.center), Is.InRange(minimumProgress, maximumProgress), obstacleName);

        return collider;
    }

    private void AssertLowReadableBlocker(Collider blocker, string obstacleDescription)
    {
        Assert.That(blocker.bounds.size.y, Is.LessThanOrEqualTo(1.25f), obstacleDescription);
        Assert.That(blocker.bounds.size.z, Is.LessThanOrEqualTo(4.5f), obstacleDescription);
    }

    private void AssertCenterFallbackClear(Collider blocker, string obstacleDescription)
    {
        var blocksLeftSideOnly = blocker.bounds.max.x <= -1.5f;
        var blocksRightSideOnly = blocker.bounds.min.x >= 1.5f;

        Assert.That(blocksLeftSideOnly || blocksRightSideOnly, Is.True, obstacleDescription);
    }

    private void AssertSideGaps(Collider obstacle, float minimumLeftGap, float minimumRightGap, string obstacleDescription)
    {
        var leftLip = -5f;
        var rightLip = 5f;
        var leftGap = obstacle.bounds.min.x - leftLip;
        var rightGap = rightLip - obstacle.bounds.max.x;

        Assert.That(leftGap, Is.GreaterThanOrEqualTo(minimumLeftGap), obstacleDescription);
        Assert.That(rightGap, Is.GreaterThanOrEqualTo(minimumRightGap), obstacleDescription);
    }

    private void AssertNoObstaclesInProgressRange(
        Scene scene,
        RunProgressFrameSource runProgressFrameSource,
        float minimumProgress,
        float maximumProgress,
        string rangeDescription)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var obstacleNames = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.Category == RunContactCategory.Obstacle)
            .Select(contact => new
            {
                Contact = contact,
                Collider = contact.GetComponent<Collider>()
            })
            .Where(candidate => candidate.Collider != null)
            .Where(candidate =>
            {
                var progress = frame.GetForwardProgress(candidate.Collider.bounds.center);
                return progress >= minimumProgress && progress <= maximumProgress;
            })
            .Select(candidate => candidate.Contact.name)
            .ToArray();

        Assert.That(obstacleNames, Is.Empty, rangeDescription);
    }

    private void AssertNoBandOneObstacle(Scene scene, RunProgressFrameSource runProgressFrameSource)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var bandOneObstacleNames = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.Category == RunContactCategory.Obstacle)
            .Select(contact => new
            {
                Contact = contact,
                Collider = contact.GetComponent<Collider>()
            })
            .Where(candidate => candidate.Collider != null)
            .Where(candidate => IsInBandOne(frame.GetForwardProgress(candidate.Collider.bounds.center)))
            .Select(candidate => candidate.Contact.name)
            .ToArray();

        Assert.That(bandOneObstacleNames, Is.Empty, "Band 1 must not contain blocking Run Obstacles.");
    }

    private void AssertSafetyNetCoverage(Scene scene, RunProgressFrameSource runProgressFrameSource, float requiredEndProgress)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var safetyNet = FindGameObjectByName(scene, "Run Safety Net");
        var safetyNetCollider = safetyNet.GetComponent<Collider>();
        var safetyNetContact = safetyNet.GetComponent<RunContact>();
        var startProgress = frame.GetForwardProgress(safetyNetCollider.bounds.min);
        var endProgress = frame.GetForwardProgress(safetyNetCollider.bounds.max);

        Assert.That(safetyNetCollider, Is.Not.Null);
        Assert.That(safetyNetCollider.isTrigger, Is.True);
        Assert.That(safetyNetContact, Is.Not.Null);
        Assert.That(safetyNetContact.Category, Is.EqualTo(RunContactCategory.SafetyNet));
        Assert.That(safetyNetCollider.bounds.center.y, Is.LessThan(-2f));
        Assert.That(safetyNetCollider.bounds.size.x, Is.GreaterThanOrEqualTo(18f));
        Assert.That(startProgress, Is.LessThanOrEqualTo(-2f));
        Assert.That(endProgress, Is.GreaterThanOrEqualTo(requiredEndProgress));
        AssertSafetyNetBelowCourseSurface(safetyNetCollider, requiredEndProgress);
    }

    private void AssertSafetyNetBelowCourseSurface(Collider safetyNetCollider, float requiredEndProgress)
    {
        var sampleProgress = Mathf.Clamp(requiredEndProgress - 2f, 4f, 416f);
        var rayOrigin = new Vector3(0f, 24f, sampleProgress);
        var terrainMask = 1 << GetRequiredLayer(_cameraTerrainLayerName);

        Assert.That(
            Physics.Raycast(rayOrigin, Vector3.down, out var hit, 128f, terrainMask, QueryTriggerInteraction.Ignore),
            Is.True,
            $"Safety net clearance sample at progress {sampleProgress:0.#}");
        Assert.That(
            safetyNetCollider.bounds.max.y,
            Is.LessThan(hit.point.y - 4f),
            $"Run Safety Net should sit below the downhill course at progress {sampleProgress:0.#}.");
    }

    private void AssertSlopeSample(
        Scene scene,
        RunProgressFrameSource runProgressFrameSource,
        float lateralPosition,
        float progress,
        float minimumDownhillDegrees,
        float maximumDownhillDegrees,
        string sampleDescription)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var rayOrigin = new Vector3(lateralPosition, 24f, progress);
        var terrainMask = 1 << GetRequiredLayer(_cameraTerrainLayerName);

        Assert.That(
            Physics.Raycast(rayOrigin, Vector3.down, out var hit, 128f, terrainMask, QueryTriggerInteraction.Ignore),
            Is.True,
            sampleDescription);

        var contact = hit.collider.GetComponent<RunContact>();
        var calculator = new RunSurfaceSlopeCalculator();
        var downhillDegrees = calculator.CalculateForwardDownhillDegrees(hit.normal, frame);

        Assert.That(contact, Is.Not.Null, sampleDescription);
        Assert.That(contact.Category, Is.EqualTo(RunContactCategory.Surface), sampleDescription);
        Assert.That(hit.collider.gameObject.scene, Is.EqualTo(scene), sampleDescription);
        Assert.That(downhillDegrees, Is.InRange(minimumDownhillDegrees, maximumDownhillDegrees), sampleDescription);
    }

    private bool IsInBandOne(float progress)
    {
        return progress >= 0f && progress <= 70f;
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
}
