using System.Collections;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Pickups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private const string PickupLayerName = "Pickup";

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_CourseWidePickupDistributionIsAuthored()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var pickupLayer = GetRequiredLayer(PickupLayerName);

        var coursePickups = FindComponentsInScene<Pickup>(scene)
            .Where(pickup => pickup.transform.IsChildOf(courseRoot.transform))
            .ToArray();

        Assert.That(coursePickups, Has.Length.GreaterThanOrEqualTo(70));
        AssertCoursePickupBandCoverage(runProgressFrameSource, coursePickups);
        AssertCoursePickupRiskSplit(coursePickups);
        AssertCoursePickupAuthoring(coursePickups, pickupLayer);

        var requiredRampPickups = AssertPickupLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 3 Required Ramp Coin Arc Cue",
            8,
            154f,
            164f,
            180f,
            190f);

        var optionalRampPickups = AssertPickupLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 4 Optional Ramp Reward Coin Cue",
            7,
            304f,
            312f,
            334f,
            344f);

        var finishApproachPickups = AssertPickupLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 5 Finish Approach Coin Cue",
            7,
            350f,
            360f,
            384f,
            392f);

        var finalFunnelPickups = AssertPickupLine(
            scene,
            courseRoot,
            runProgressFrameSource,
            "Band 5 Final Funnel Coin Cue",
            5,
            390f,
            400f,
            414f,
            420f);

        var requiredRampPositions = requiredRampPickups.Select(pickup => pickup.Position).ToArray();
        var optionalRampPositions = optionalRampPickups.Select(pickup => pickup.Position).ToArray();
        var finishApproachPositions = finishApproachPickups.Select(pickup => pickup.Position).ToArray();
        var finalFunnelPositions = finalFunnelPickups.Select(pickup => pickup.Position).ToArray();

        Assert.That(requiredRampPositions.Select(position => Mathf.Abs(position.x)).Max(), Is.LessThanOrEqualTo(0.75f));
        Assert.That(requiredRampPositions.Max(position => position.y) - requiredRampPositions.Min(position => position.y), Is.GreaterThan(0.4f));
        Assert.That(optionalRampPositions.Select(position => position.x).Min(), Is.GreaterThanOrEqualTo(3.2f));
        Assert.That(optionalRampPositions.Max(position => position.y) - optionalRampPositions.Min(position => position.y), Is.GreaterThan(0.45f));
        AssertCueLineMovesTowardCenter(runProgressFrameSource, finishApproachPositions, "Band 5 finish approach pickup line");
        Assert.That(finalFunnelPositions.Select(position => Mathf.Abs(position.x)).Max(), Is.LessThanOrEqualTo(0.75f));
    }

    private void AssertCoursePickupBandCoverage(RunProgressFrameSource runProgressFrameSource, Pickup[] coursePickups)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        Assert.That(CountPickupsInProgressRange(frame, coursePickups, 0f, 70f), Is.GreaterThanOrEqualTo(12), "Band 1 safe pickup baseline");
        Assert.That(CountPickupsInProgressRange(frame, coursePickups, 70f, 150f), Is.GreaterThanOrEqualTo(14), "Band 2 safe/risk pickup baseline");
        Assert.That(CountPickupsInProgressRange(frame, coursePickups, 150f, 250f), Is.GreaterThanOrEqualTo(20), "Band 3 ramp and post-ramp pickups");

        Assert.That(CountPickupsInProgressRange(frame, coursePickups, 250f, 350f), Is.GreaterThanOrEqualTo(18),
            "Band 4 reach and optional-ramp pickups");
        Assert.That(CountPickupsInProgressRange(frame, coursePickups, 350f, 420f), Is.GreaterThanOrEqualTo(12), "Band 5 finish funnel pickups");
    }

    private void AssertCoursePickupRiskSplit(Pickup[] coursePickups)
    {
        var riskRewardPickups = coursePickups.Where(IsRiskRewardPickup).ToArray();
        var safeOrNearSafePickups = coursePickups.Except(riskRewardPickups).ToArray();
        var safeRatio = safeOrNearSafePickups.Length / (float)coursePickups.Length;
        var riskRatio = riskRewardPickups.Length / (float)coursePickups.Length;

        Assert.That(safeRatio, Is.InRange(0.65f, 0.75f), $"safe={safeOrNearSafePickups.Length}, risk={riskRewardPickups.Length}");
        Assert.That(riskRatio, Is.InRange(0.25f, 0.35f), $"safe={safeOrNearSafePickups.Length}, risk={riskRewardPickups.Length}");
    }

    private void AssertCoursePickupAuthoring(Pickup[] coursePickups, int pickupLayer)
    {
        foreach (var pickup in coursePickups)
        {
            var colliders = pickup.GetComponentsInChildren<Collider>(true);

            Assert.That(pickup.gameObject.activeSelf, Is.True, pickup.name);
            Assert.That(pickup.gameObject.layer, Is.EqualTo(pickupLayer), pickup.name);
            Assert.That(pickup.Definition, Is.Not.Null, pickup.name);
            Assert.That(new[] { 1, 5 }, Does.Contain(pickup.Definition.Amount), pickup.name);
            Assert.That(pickup.Definition.CurrencyDefinition, Is.Not.Null, pickup.name);
            Assert.That(colliders, Has.Length.GreaterThanOrEqualTo(1), pickup.name);

            pickup.Definition.Validate();

            foreach (var collider in colliders)
            {
                Assert.That(collider.isTrigger, Is.True, collider.name);
                Assert.That(collider.gameObject.layer, Is.EqualTo(pickupLayer), collider.name);
            }
        }
    }

    private Pickup[] AssertPickupLine(
        Scene scene,
        GameObject courseRoot,
        RunProgressFrameSource runProgressFrameSource,
        string pickupRootName,
        int minimumPickupCount,
        float minimumStartProgress,
        float maximumStartProgress,
        float minimumEndProgress,
        float maximumEndProgress)
    {
        var pickupRoot = FindGameObjectByName(scene, pickupRootName);
        var pickups = pickupRoot.GetComponentsInChildren<Pickup>(true);
        var pickupPositions = pickups.Select(pickup => pickup.Position).ToArray();
        var pickupProgresses = GetCueProgresses(runProgressFrameSource, pickupPositions);

        Assert.That(pickupRoot.transform.IsChildOf(courseRoot.transform), Is.True, pickupRootName);
        Assert.That(pickups, Has.Length.GreaterThanOrEqualTo(minimumPickupCount), pickupRootName);
        Assert.That(pickupProgresses.Min(), Is.InRange(minimumStartProgress, maximumStartProgress), pickupRootName);
        Assert.That(pickupProgresses.Max(), Is.InRange(minimumEndProgress, maximumEndProgress), pickupRootName);

        return pickups;
    }

    private int CountPickupsInProgressRange(
        RunProgressFrameSnapshot frame,
        Pickup[] pickups,
        float minimumProgress,
        float maximumProgress)
    {
        return pickups.Count(pickup =>
        {
            var progress = frame.GetForwardProgress(pickup.Position);
            return progress >= minimumProgress && progress <= maximumProgress;
        });
    }

    private bool IsRiskRewardPickup(Pickup pickup)
    {
        var parentName = pickup.transform.parent != null ? pickup.transform.parent.name : string.Empty;

        return parentName.Contains("Risk")
               || parentName.Contains("Reward")
               || parentName.Contains("Optional Ramp");
    }
}
