using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class GameplaySceneHighSpeedRunContactSafetyTests : BaseGameplayScenePlayModeFixture
{
    private readonly float _expectedProductionSphereRadius = 0.35f;
    private readonly int _fixedFrameObservationLimit = 6;
    private readonly float _stressSpeedMetersPerSecond = 80f;
    private readonly float _supportedSpeedMetersPerSecond = 40f;
    private readonly float _thinObstacleThickness = 0.01f;

    [UnityTest]
    public IEnumerator given_MaxUpgradedSpeedProductionRunBody_when_CrossingAdversarialThinObstacle_then_ObstacleHitEndsRun()
    {
        yield return RunObstacleScenario();
    }

    [UnityTest]
    public IEnumerator given_MaxUpgradedSpeedProductionRunBody_when_CrossingAuthoredRunFinish_then_FinishedEndsRun()
    {
        yield return RunTriggerScenario(
            objectName: "Band 5 Run Finish",
            RunContactCategory.Finish,
            RunEndReason.Finished,
            _supportedSpeedMetersPerSecond,
            speedTier: "acceptance-40-mps",
            preferBelowCourseDirection: false);
    }

    [UnityTest, Category(name: "HighSpeedContactStress")]
    public IEnumerator given_StressSpeedProductionRunBody_when_CrossingAuthoredRunFinish_then_FinishedEndsRun()
    {
        yield return RunTriggerScenario(
            objectName: "Band 5 Run Finish",
            RunContactCategory.Finish,
            RunEndReason.Finished,
            _stressSpeedMetersPerSecond,
            speedTier: "diagnostic-stress-80-mps",
            preferBelowCourseDirection: false);
    }

    [UnityTest]
    public IEnumerator given_ProductionRunBodyAtAcceptedFallingSpeed_when_CrossingAuthoredRunSafetyNet_then_OutOfBoundsEndsRun()
    {
        yield return RunTriggerScenario(
            objectName: "Run Safety Net",
            RunContactCategory.SafetyNet,
            RunEndReason.OutOfBounds,
            _supportedSpeedMetersPerSecond,
            speedTier: "acceptance-40-mps",
            preferBelowCourseDirection: true);
    }

    [UnityTest, Category(name: "HighSpeedContactStress")]
    public IEnumerator given_StressFallingSpeedProductionRunBody_when_CrossingAuthoredRunSafetyNet_then_OutOfBoundsEndsRun()
    {
        yield return RunTriggerScenario(
            objectName: "Run Safety Net",
            RunContactCategory.SafetyNet,
            RunEndReason.OutOfBounds,
            _stressSpeedMetersPerSecond,
            speedTier: "diagnostic-stress-80-mps",
            preferBelowCourseDirection: true);
    }
}
