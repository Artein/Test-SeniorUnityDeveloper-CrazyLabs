using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private readonly int _gameplaySceneBuildIndex = 0;
    private readonly string _courseRootName = "Ladybug Rooftop Half-Tube Run Course";
    private readonly string _cameraTerrainLayerName = "CameraTerrain";

    private readonly (string Name, float StartProgress, float EndProgress, float MinimumPitch, float MaximumPitch)[] _bandOneSections =
    {
        ("Band 1 Section 01 Shallow Settle Surface", 0f, 20f, 3f, 5f),
        ("Band 1 Section 02 Gentle S Surface", 20f, 45f, 5f, 7f),
        ("Band 1 Section 03 Low Bank Touch Surface", 45f, 70f, 5f, 7f)
    };

    private readonly (string Name, float StartProgress, float EndProgress, float MinimumPitch, float MaximumPitch)[] _bandTwoSections =
    {
        ("Band 2 Section 04 First Choice Surface", 70f, 95f, 6f, 8f),
        ("Band 2 Section 05 Recovery Trough Surface", 95f, 120f, 5f, 7f),
        ("Band 2 Section 06 Offset Choice Surface", 120f, 150f, 7f, 9f)
    };

    private readonly (string Name, float StartProgress, float EndProgress, float MinimumPitch, float MaximumPitch)[] _bandThreeRampSetupSections =
    {
        ("Band 3 Section 07 Required Ramp Setup Surface", 150f, 170f, 8f, 10f)
    };

    private readonly (string Name, float StartProgress, float EndProgress, float MinimumPitch, float MaximumPitch)[] _bandThreePostRampSections =
    {
        ("Band 3 Section 09 Landing Recovery Surface", 200f, 225f, 6f, 7f),
        ("Band 3 Section 10 Bank Pressure Surface", 225f, 250f, 6f, 8f)
    };

    private readonly (string Name, float StartProgress, float EndProgress, float MinimumPitch, float MaximumPitch)[] _bandFourSections =
    {
        ("Band 4 Section 11 Transfer Lines Surface", 250f, 280f, 6f, 7f),
        ("Band 4 Section 12 Reach Pressure Glide Surface", 280f, 310f, 3f, 5f),
        ("Band 4 Section 13 Center Bypass Surface", 310f, 350f, 5f, 6f)
    };

    private readonly (string Name, float StartProgress, float EndProgress, float MinimumPitch, float MaximumPitch)[] _bandFiveSections =
    {
        ("Band 5 Section 14 Finish Approach Surface", 350f, 390f, 6f, 7f),
        ("Band 5 Section 15 Final Funnel Surface", 390f, 420f, 5f, 6f)
    };

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_BandOneHalfTubeCourseIsAuthored()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var courseRoot = FindGameObjectByName(scene, _courseRootName);

        Assert.That(courseRoot.activeInHierarchy, Is.True);
        AssertSurfaceSections(scene, courseRoot, cameraTerrainLayer, _bandOneSections, 0f, 70f);
        AssertNoBandOneObstacle(scene, runProgressFrameSource);
        AssertSafetyNetCoverage(scene, runProgressFrameSource, 72f);
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 10f, 3f, 5f, "center settle sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 32f, 5f, 7f, "center S sample");
        AssertSlopeSample(scene, runProgressFrameSource, -4.2f, 56f, 4f, 8f, "left bank sample");
        AssertSlopeSample(scene, runProgressFrameSource, 4.2f, 56f, 4f, 8f, "right bank sample");
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_BandTwoEarlyObstacleSliceIsAuthored()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var centerBlocker = AssertObstacle(scene, runProgressFrameSource, "Band 2 Obstacle 01 Center AC Blocker", 70f, 95f);
        var offsetBlocker = AssertObstacle(scene, runProgressFrameSource, "Band 2 Obstacle 02 Left Offset Sunroof Blocker", 120f, 150f);

        AssertSurfaceSections(scene, courseRoot, cameraTerrainLayer, _bandTwoSections, 70f, 150f);
        Assert.That(Mathf.Abs(centerBlocker.bounds.center.x), Is.LessThan(0.25f), "first blocker should teach symmetric line choice");
        AssertSideGaps(centerBlocker, 3f, 3f, "first center blocker");
        Assert.That(offsetBlocker.bounds.center.x, Is.LessThan(-1f), "second blocker should be visibly offset");
        AssertSideGaps(offsetBlocker, 1.25f, 4.5f, "second offset blocker");
        AssertNoObstaclesInProgressRange(scene, runProgressFrameSource, 95f, 120f, "Band 2 recovery trough");
        AssertSafetyNetCoverage(scene, runProgressFrameSource, 152f);
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 82f, 6f, 8f, "Band 2 first choice center sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 108f, 5f, 7f, "Band 2 recovery center sample");
        AssertSlopeSample(scene, runProgressFrameSource, 3.8f, 135f, 5f, 9f, "Band 2 obvious safe-side bank sample");
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_RequiredCenterTutorialRampSliceIsAuthored()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var courseRoot = FindGameObjectByName(scene, _courseRootName);

        AssertNoUnsupportedRunContactCategories();
        AssertSurfaceSections(scene, courseRoot, cameraTerrainLayer, _bandThreeRampSetupSections, 150f, 170f);

        var rampCollider = AssertSurfaceObject(
            scene,
            courseRoot,
            cameraTerrainLayer,
            "Band 3 Section 08 Required Tutorial Ramp Surface");

        AssertSurfaceBounds(runProgressFrameSource, rampCollider, 170f, 200f, "Band 3 required tutorial ramp");
        Assert.That(rampCollider.bounds.size.x, Is.InRange(9.5f, 10.5f), "Band 3 required tutorial ramp width");
        Assert.That(rampCollider.bounds.size.z, Is.EqualTo(30f).Within(0.75f), "Band 3 required tutorial ramp length");

        AssertVisibleRampLineCue(scene, courseRoot, runProgressFrameSource);
        AssertNoObstaclesInProgressRange(scene, runProgressFrameSource, 150f, 210f, "Band 3 required ramp approach, landing, and recovery");
        AssertSafetyNetCoverage(scene, runProgressFrameSource, 212f);
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 160f, 8f, 10f, "Band 3 required ramp setup sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 176f, 10f, 12f, "Band 3 required ramp takeoff sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 192f, 5f, 6f, "Band 3 required ramp landing sample");
        AssertSlopeSample(scene, runProgressFrameSource, -4.2f, 192f, 4f, 8f, "Band 3 left landing bank sample");
        AssertSlopeSample(scene, runProgressFrameSource, 4.2f, 192f, 4f, 8f, "Band 3 right landing bank sample");
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_BandThreePostRampBankPressureSliceIsAuthored()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var courseRoot = FindGameObjectByName(scene, _courseRootName);

        AssertSurfaceSections(scene, courseRoot, cameraTerrainLayer, _bandThreePostRampSections, 200f, 250f);
        AssertNoObstaclesInProgressRange(scene, runProgressFrameSource, 200f, 212f, "Band 3 landing recovery");

        var recoveryBlocker = AssertObstacle(
            scene,
            runProgressFrameSource,
            "Band 3 Obstacle 03 Right Low Solar Blocker",
            212f,
            225f);

        var bankBlocker = AssertObstacle(
            scene,
            runProgressFrameSource,
            "Band 3 Obstacle 04 Left Bank AC Blocker",
            225f,
            250f);

        AssertLowReadableBlocker(recoveryBlocker, "Band 3 recovery blocker");
        AssertLowReadableBlocker(bankBlocker, "Band 3 bank blocker");
        AssertCenterFallbackClear(recoveryBlocker, "Band 3 recovery blocker");
        AssertCenterFallbackClear(bankBlocker, "Band 3 bank blocker");
        AssertNoObstaclesInProgressRange(scene, runProgressFrameSource, 220f, 232f, "Band 3 post-blocker recovery window");
        AssertBandThreeCueLines(scene, courseRoot, runProgressFrameSource);
        AssertSafetyNetCoverage(scene, runProgressFrameSource, 252f);
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 210f, 6f, 7f, "Band 3 landing recovery center sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 236f, 6f, 8f, "Band 3 bank pressure center fallback sample");
        AssertSlopeSample(scene, runProgressFrameSource, -4.2f, 240f, 5f, 9f, "Band 3 bank reward line sample");
        AssertSlopeSample(scene, runProgressFrameSource, 4.2f, 216f, 4f, 8f, "Band 3 post-ramp side sample");
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_BandFourReachPressureAndOptionalRampSliceIsAuthored()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var courseRoot = FindGameObjectByName(scene, _courseRootName);

        AssertSurfaceSections(scene, courseRoot, cameraTerrainLayer, _bandFourSections, 250f, 350f);

        var optionalRamp = AssertSurfaceObject(
            scene,
            courseRoot,
            cameraTerrainLayer,
            "Band 4 Section 13 Optional Bank Ramp Surface");

        var bankBlocker = AssertObstacle(
            scene,
            runProgressFrameSource,
            "Band 4 Obstacle 05 Right Bank Billboard Blocker",
            250f,
            280f);

        AssertSurfaceBounds(runProgressFrameSource, optionalRamp, 310f, 350f, "Band 4 optional bank ramp");
        Assert.That(optionalRamp.bounds.center.x, Is.GreaterThan(2.5f), "Band 4 optional ramp should sit on a bank-side risk line.");
        Assert.That(optionalRamp.bounds.size.x, Is.InRange(2f, 3.5f), "Band 4 optional ramp should not replace the center fallback.");
        AssertLowReadableBlocker(bankBlocker, "Band 4 bank-side blocker");
        AssertCenterFallbackClear(bankBlocker, "Band 4 bank-side blocker");
        AssertNoObstaclesInProgressRange(scene, runProgressFrameSource, 280f, 350f, "Band 4 glide, optional ramp, landing, and recovery");
        AssertBandFourCueLines(scene, courseRoot, runProgressFrameSource);
        AssertSafetyNetCoverage(scene, runProgressFrameSource, 352f);
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 264f, 6f, 7f, "Band 4 transfer center sample");
        AssertSlopeSample(scene, runProgressFrameSource, 4.2f, 268f, 4f, 8f, "Band 4 side transfer sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 294f, 3f, 5f, "Band 4 reach-pressure glide sample");
        AssertSlopeSample(scene, runProgressFrameSource, 4.2f, 318f, 8f, 10f, "Band 4 optional ramp takeoff sample");
        AssertSlopeSample(scene, runProgressFrameSource, 4.2f, 342f, 5f, 6f, "Band 4 optional ramp landing sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 334f, 5f, 6f, "Band 4 center bypass sample");
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_BandFiveFinishApproachAndRunFinishSliceIsAuthored()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(scene, "RunProgressFrameSource");
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var courseRoot = FindGameObjectByName(scene, _courseRootName);

        AssertSurfaceSections(scene, courseRoot, cameraTerrainLayer, _bandFiveSections, 350f, 420f);

        var finishApproachBlocker = AssertObstacle(
            scene,
            runProgressFrameSource,
            "Band 5 Obstacle 06 Left Finish Approach Solar Blocker",
            350f,
            390f);

        AssertLowReadableBlocker(finishApproachBlocker, "Band 5 finish approach blocker");
        AssertCenterFallbackClear(finishApproachBlocker, "Band 5 finish approach blocker");
        AssertObstacleCountInProgressRange(scene, runProgressFrameSource, 350f, 390f, 1, "Band 5 finish approach");
        AssertNoObstaclesInProgressRange(scene, runProgressFrameSource, 390f, 420f, "Band 5 final funnel");
        AssertBandFiveCueLines(scene, courseRoot, runProgressFrameSource);

        var finishContact = AssertRunFinishContact(scene, courseRoot, runProgressFrameSource, "Band 5 Run Finish");

        Assert.That(finishContact.bounds.size.z, Is.GreaterThanOrEqualTo(1f), "Band 5 Run Finish");
        AssertSafetyNetCoverage(scene, runProgressFrameSource, 422f);
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 366f, 6f, 7f, "Band 5 finish approach center sample");
        AssertSlopeSample(scene, runProgressFrameSource, -4.2f, 370f, 4f, 8f, "Band 5 finish approach side sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 404f, 5f, 6f, "Band 5 final funnel center sample");
        AssertSlopeSample(scene, runProgressFrameSource, 0f, 416f, 5f, 6f, "Band 5 finish gate approach sample");
    }
}
