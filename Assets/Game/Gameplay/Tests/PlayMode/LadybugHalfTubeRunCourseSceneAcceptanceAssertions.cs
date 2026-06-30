using System;
using System.Collections;
using System.IO;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Pickups;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private const string AcceptanceProfileTypeName = "LadybugHalfTubeRunCourseAcceptanceProfile";

    private const string AcceptanceQaReportPath =
        "Assets/Game/Level/RunCourses/LadybugRooftopHalfTube/Design/GRAYBOX_ACCEPTANCE_QA.md";

    private const int FirstRunMinimumUsefulCoins = 8;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_FullCourseProgressionAcceptanceProfileIsTraceable()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var profile = AssertAcceptanceProfile(courseRoot);

        AssertAcceptanceTarget(profile, "CourseLengthMeters", 420f, 0.01f);
        AssertAcceptanceTarget(profile, "RequiredRampStartProgress", 170f, 0.01f);
        AssertAcceptanceTarget(profile, "RequiredRampEndProgress", 200f, 0.01f);
        AssertAcceptanceTarget(profile, "OptionalRampStartProgress", 310f, 0.01f);
        AssertAcceptanceTarget(profile, "OptionalRampEndProgress", 350f, 0.01f);
        AssertAcceptanceTarget(profile, "FinishProgress", 416f, 0.01f);
        AssertAcceptanceTarget(profile, "UpgradedCompletionSecondsMin", 55f, 0.01f);
        AssertAcceptanceTarget(profile, "UpgradedCompletionSecondsMax", 65f, 0.01f);
        AssertAcceptanceTarget(profile, "FirstUsefulFailureProgressMin", 85f, 0.01f);
        AssertAcceptanceTarget(profile, "FirstUsefulFailureProgressMax", 125f, 0.01f);
        AssertAcceptanceTarget(profile, "FirstCompletionRunCountMin", 5f, 0.01f);
        AssertAcceptanceTarget(profile, "FirstCompletionRunCountMax", 10f, 0.01f);
        AssertAcceptanceTarget(profile, "SafePickupRatioMin", 0.65f, 0.001f);
        AssertAcceptanceTarget(profile, "SafePickupRatioMax", 0.75f, 0.001f);
        AssertAcceptanceTarget(profile, "RiskRewardPickupRatioMin", 0.25f, 0.001f);
        AssertAcceptanceTarget(profile, "RiskRewardPickupRatioMax", 0.35f, 0.001f);
        var manualQaReportPath = GetStringTarget(profile, "ManualQaReportPath");

        Assert.That(manualQaReportPath, Is.EqualTo(AcceptanceQaReportPath));

        Assert.That(File.Exists(ToProjectAbsolutePath(manualQaReportPath)), Is.True,
            "Manual QA report should be recorded next to the course design docs.");
        Assert.That(GetBoolTarget(profile, "MovingObstaclesAllowed"), Is.False);
        Assert.That(GetBoolTarget(profile, "RouteBranchingAllowed"), Is.False);
        Assert.That(GetBoolTarget(profile, "HiddenContainmentAllowed"), Is.False);

        AssertUpgradedCompletionPacing(profile);
        AssertNoMandatoryPreRampBlockers(scene, runProgressFrameSource, courseRoot, profile);
        AssertEarlySafePickupEconomy(scene, runProgressFrameSource, courseRoot, profile);
        AssertNoNewHazardTypeInFinalFunnel(scene, runProgressFrameSource, profile);
        AssertCourseSurfacesUseLowFrictionPhysics(scene, courseRoot);
    }

    [UnityTest]
    public IEnumerator
        given_GameplayScene_when_ReferenceProgressionRunsArePlayed_then_FirstRunFailsUsefulAndUpgradedRunFinishesNearOneMinute()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Ladybug Half-Tube Progression Mouse");
        var originalTimeScale = Time.timeScale;
        var originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = 8f;
        Time.fixedDeltaTime = 0.02f;

        try
        {
            SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
            yield return null;
            Physics.SyncTransforms();

            var firstRunContext = CreateRunPlaybackContext(SceneManager.GetActiveScene());
            var firstRunProfile = AssertAcceptanceProfile(firstRunContext.CourseRoot);
            var firstRunSnapshotProvider = firstRunContext.LifetimeScope.Container.Resolve<IRunModifierSnapshotProvider>();
            var firstRunCapture = new RunPlaybackCapture();

            ResetReferenceUpgradeLevels(firstRunContext);
            yield return PlayReferenceRouteRun(mouse, firstRunContext, firstRunCapture, "first reference run");

            var firstRunResult = AssertCapturedRunResult(firstRunCapture);
            var requiredRampStartProgress = GetFloatTarget(firstRunProfile, "RequiredRampStartProgress");
            var firstFailureProgressMin = GetFloatTarget(firstRunProfile, "FirstUsefulFailureProgressMin");
            var firstFailureProgressMax = GetFloatTarget(firstRunProfile, "FirstUsefulFailureProgressMax");
            var firstRunCoins = firstRunResult.CurrencySnapshot.GetAmount(firstRunContext.Catalog.PurchaseCurrency);
            var firstRunDiagnostics = FormatRunProgressionDiagnostics("first reference run", firstRunResult, firstRunCoins);

            SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
            yield return null;
            Physics.SyncTransforms();

            var upgradedRunContext = CreateRunPlaybackContext(SceneManager.GetActiveScene());
            var upgradedRunProfile = AssertAcceptanceProfile(upgradedRunContext.CourseRoot);
            var snapshotProvider = upgradedRunContext.LifetimeScope.Container.Resolve<IRunModifierSnapshotProvider>();
            var upgradedRunCapture = new RunPlaybackCapture();

            ResetReferenceUpgradeLevels(upgradedRunContext);
            SetReferenceCompletionUpgradeLevels(upgradedRunContext);

            yield return PlayReferenceRouteRun(mouse, upgradedRunContext, upgradedRunCapture, "upgraded reference run");

            var upgradedRunResult = AssertCapturedRunResult(upgradedRunCapture);
            var upgradedCompletionSecondsMin = GetFloatTarget(upgradedRunProfile, "UpgradedCompletionSecondsMin");
            var upgradedCompletionSecondsMax = GetFloatTarget(upgradedRunProfile, "UpgradedCompletionSecondsMax");
            var finishProgress = GetFloatTarget(upgradedRunProfile, "FinishProgress");
            var upgradedRunCoins = upgradedRunResult.CurrencySnapshot.GetAmount(upgradedRunContext.Catalog.PurchaseCurrency);

            var upgradedRunDiagnostics = FormatRunProgressionDiagnostics(
                "upgraded reference run",
                upgradedRunResult,
                upgradedRunCoins);

            Assert.That(firstRunSnapshotProvider.CurrentSnapshot.Modifiers, Is.Empty, firstRunDiagnostics);
            Assert.That(snapshotProvider.CurrentSnapshot.Modifiers, Has.Count.EqualTo(3));

            Assert.That(firstRunResult.Reason, Is.EqualTo(RunEndReason.LostMomentum),
                "The first reference run should fail from reach pressure, not obstacle impact or finish contact. "
                + firstRunDiagnostics
                + " "
                + upgradedRunDiagnostics);

            Assert.That(firstRunResult.DistanceTravelled, Is.InRange(firstFailureProgressMin, firstFailureProgressMax),
                firstRunDiagnostics + " " + upgradedRunDiagnostics);
            Assert.That(firstRunResult.DistanceTravelled, Is.LessThan(requiredRampStartProgress),
                firstRunDiagnostics + " " + upgradedRunDiagnostics);
            Assert.That(firstRunCoins, Is.GreaterThanOrEqualTo(FirstRunMinimumUsefulCoins),
                firstRunDiagnostics + " " + upgradedRunDiagnostics);
            Assert.That(upgradedRunResult.Reason, Is.EqualTo(RunEndReason.Finished), upgradedRunDiagnostics);

            Assert.That(
                upgradedRunResult.DistanceTravelled,
                Is.GreaterThanOrEqualTo(finishProgress - 1f),
                upgradedRunDiagnostics);

            Assert.That(
                upgradedRunResult.ElapsedTime,
                Is.InRange(upgradedCompletionSecondsMin, upgradedCompletionSecondsMax),
                upgradedRunDiagnostics);
            Assert.That(upgradedRunCoins, Is.GreaterThan(firstRunCoins), upgradedRunDiagnostics);
        }
        finally
        {
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            InputSystem.RemoveDevice(mouse);
        }
    }

    private Component AssertAcceptanceProfile(GameObject courseRoot)
    {
        var profiles = courseRoot.GetComponents<Component>()
            .Where(component => component != null && component.GetType().Name == AcceptanceProfileTypeName)
            .ToArray();

        Assert.That(profiles, Has.Length.EqualTo(1), $"Expected one {AcceptanceProfileTypeName} on the course root.");
        return profiles[0];
    }

    private void AssertAcceptanceTarget(Component profile, string propertyName, float expectedValue, float tolerance)
    {
        var actualValue = GetFloatTarget(profile, propertyName);

        Assert.That(actualValue, Is.EqualTo(expectedValue).Within(tolerance), propertyName);
    }

    private void AssertUpgradedCompletionPacing(Component profile)
    {
        var courseLength = GetFloatTarget(profile, "CourseLengthMeters");
        var minimumCompletionSeconds = GetFloatTarget(profile, "UpgradedCompletionSecondsMin");
        var maximumCompletionSeconds = GetFloatTarget(profile, "UpgradedCompletionSecondsMax");
        var minimumAverageSpeed = courseLength / maximumCompletionSeconds;
        var maximumAverageSpeed = courseLength / minimumCompletionSeconds;

        Assert.That(minimumAverageSpeed, Is.InRange(6.4f, 6.6f), "55-65s target should require a competent upgraded speed floor.");
        Assert.That(maximumAverageSpeed, Is.InRange(7.6f, 7.8f), "55-65s target should not imply exploit-level speed.");
    }

    private void AssertNoMandatoryPreRampBlockers(
        Scene scene,
        RunProgressFrameSource runProgressFrameSource,
        GameObject courseRoot,
        Component profile)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var requiredRampStartProgress = GetFloatTarget(profile, "RequiredRampStartProgress");

        var preRampBlockers = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.transform.IsChildOf(courseRoot.transform))
            .Where(contact => contact.Category == RunContactCategory.Obstacle)
            .Select(contact => new
            {
                Contact = contact,
                Collider = contact.GetComponent<Collider>()
            })
            .Where(candidate => candidate.Collider != null)
            .Where(candidate => frame.GetForwardProgress(candidate.Collider.bounds.center) < requiredRampStartProgress)
            .ToArray();

        Assert.That(preRampBlockers, Has.Length.EqualTo(2), "Band 2 should teach choice without becoming a blocker wall.");

        foreach (var blocker in preRampBlockers)
        {
            var leftGap = blocker.Collider.bounds.min.x + 5f;
            var rightGap = 5f - blocker.Collider.bounds.max.x;
            var centerFallbackClear = blocker.Collider.bounds.max.x <= -1.25f || blocker.Collider.bounds.min.x >= 1.25f;
            var hasReadableTraversalLine = leftGap >= 2.5f || rightGap >= 2.5f || centerFallbackClear;

            Assert.That(hasReadableTraversalLine, Is.True, blocker.Contact.name);
        }
    }

    private void AssertEarlySafePickupEconomy(
        Scene scene,
        RunProgressFrameSource runProgressFrameSource,
        GameObject courseRoot,
        Component profile)
    {
        Assert.That(runProgressFrameSource.TryCreateSnapshot(Vector3.zero, out var frame, out var frameError), Is.True, frameError);

        var requiredRampStartProgress = GetFloatTarget(profile, "RequiredRampStartProgress");

        var safePreRampPickupValue = FindComponentsInScene<Pickup>(scene)
            .Where(pickup => pickup.transform.IsChildOf(courseRoot.transform))
            .Where(pickup => !IsRiskRewardPickup(pickup))
            .Where(pickup => frame.GetForwardProgress(pickup.Position) < requiredRampStartProgress)
            .Sum(pickup => pickup.Definition.Amount);

        Assert.That(safePreRampPickupValue, Is.GreaterThanOrEqualTo(25), "A failed Band 2 run should still feel useful.");
        Assert.That(safePreRampPickupValue * 3, Is.GreaterThanOrEqualTo(75), "Three safe failed runs should fund an early reach upgrade path.");
    }

    private void AssertNoNewHazardTypeInFinalFunnel(
        Scene scene,
        RunProgressFrameSource runProgressFrameSource,
        Component profile)
    {
        var courseLength = GetFloatTarget(profile, "CourseLengthMeters");
        var finalFunnelStartProgress = courseLength - 30f;

        AssertNoObstaclesInProgressRange(
            scene,
            runProgressFrameSource,
            finalFunnelStartProgress,
            courseLength,
            "Final 30m Run Finish funnel should introduce no new hazard type.");
    }

    private void AssertCourseSurfacesUseLowFrictionPhysics(Scene scene, GameObject courseRoot)
    {
        var surfaceColliders = FindComponentsInScene<RunContact>(scene)
            .Where(contact => contact.transform.IsChildOf(courseRoot.transform))
            .Where(contact => contact.Category == RunContactCategory.Surface)
            .Select(contact => new
            {
                Contact = contact,
                Collider = contact.GetComponent<Collider>()
            })
            .Where(candidate => candidate.Collider != null)
            .ToArray();

        Assert.That(surfaceColliders, Has.Length.GreaterThan(0), "Course should generate physical sliding surfaces.");

        foreach (var surface in surfaceColliders)
        {
            var physicsMaterial = surface.Collider.sharedMaterial;

            Assert.That(physicsMaterial, Is.Not.Null, surface.Contact.name);
            Assert.That(physicsMaterial.staticFriction, Is.InRange(0.05f, 0.2f), surface.Contact.name);
            Assert.That(physicsMaterial.dynamicFriction, Is.InRange(0.05f, 0.2f), surface.Contact.name);
            Assert.That(physicsMaterial.frictionCombine, Is.EqualTo(PhysicsMaterialCombine.Minimum), surface.Contact.name);
        }
    }

    private string FormatRunProgressionDiagnostics(string label, RunResult result, int coins)
    {
        return $"{label}: {result}, Coins={coins}";
    }

    private float GetFloatTarget(Component profile, string propertyName)
    {
        var value = GetAcceptancePropertyValue(profile, propertyName);

        return Convert.ToSingle(value);
    }

    private string GetStringTarget(Component profile, string propertyName)
    {
        var value = GetAcceptancePropertyValue(profile, propertyName);

        return Convert.ToString(value);
    }

    private bool GetBoolTarget(Component profile, string propertyName)
    {
        var value = GetAcceptancePropertyValue(profile, propertyName);

        return Convert.ToBoolean(value);
    }

    private object GetAcceptancePropertyValue(Component profile, string propertyName)
    {
        var property = profile.GetType().GetProperty(propertyName);

        Assert.That(property, Is.Not.Null, $"{AcceptanceProfileTypeName}.{propertyName} should exist.");
        return property.GetValue(profile);
    }

    private string ToProjectAbsolutePath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }
}
