using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class CharacterPresentationModeClassifierTests
{
    private FakeCharacterPresentationTuning _tuning;
    private CharacterPresentationModeClassifier _classifier;

    [SetUp]
    public void OnSetUp()
    {
        _tuning = new FakeCharacterPresentationTuning();
        _classifier = new CharacterPresentationModeClassifier(_tuning);
    }

    [Test]
    public void Classify_PullAnticipationMode_HasStableAnimatorValue()
    {
        Assert.That((int)CharacterPresentationMode.PullAnticipation, Is.EqualTo(1));
    }

    [Test]
    public void Classify_LaunchPushMode_HasStableAnimatorValue()
    {
        Assert.That((int)CharacterPresentationMode.LaunchPush, Is.EqualTo(2));
    }

    [Test]
    public void Classify_LaunchFlightMode_HasStableAnimatorValue()
    {
        Assert.That((int)CharacterPresentationMode.LaunchFlight, Is.EqualTo(8));
    }

    [Test]
    public void Classify_RunMode_HasStableReservedAnimatorValue()
    {
        Assert.That((int)CharacterPresentationMode.Run, Is.EqualTo(4));
    }

    [Test]
    public void Classify_AcceptedSuccessfulRunResult_ReturnsVictoryImmediately()
    {
        var input = CreateInput(
            hasAcceptedRunResult: true,
            acceptedRunResultSucceeded: true);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Victory));
    }

    [Test]
    public void Classify_AcceptedFailedRunResult_ReturnsDefeatImmediately()
    {
        var input = CreateInput(
            hasAcceptedRunResult: true,
            acceptedRunResultSucceeded: false);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Defeat));
    }

    [Test]
    public void Classify_PreLaunch_ReturnsIdle()
    {
        var input = CreateInput(isPreLaunch: true, isRunActive: false, surfaceContext: GroundedDownhill(20f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_RunInactive_ReturnsIdle()
    {
        var input = CreateInput(isRunActive: false, surfaceContext: GroundedDownhill(20f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_ActivePullOnDownhillSurface_ReturnsPullAnticipation()
    {
        var input = CreateInput(hasActivePull: true, surfaceContext: GroundedDownhill(20f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.PullAnticipation));
    }

    [Test]
    public void Classify_ActivePullOnFlatSurface_ReturnsPullAnticipation()
    {
        var input = CreateInput(hasActivePull: true, surfaceContext: GroundedDownhill(0f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.PullAnticipation));
    }

    [Test]
    public void Classify_ActivePullAndLaunchPush_ReturnsPullAnticipation()
    {
        var input = CreateInput(
            hasActivePull: true,
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            surfaceContext: GroundedDownhill(20f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.PullAnticipation));
    }

    [Test]
    public void Classify_ActivePullWithAcceptedSuccessfulRunResult_ReturnsVictory()
    {
        var input = CreateInput(
            hasActivePull: true,
            hasAcceptedRunResult: true,
            acceptedRunResultSucceeded: true);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Victory));
    }

    [Test]
    public void Classify_LaunchPushWithAcceptedFailedRunResult_ReturnsDefeat()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            hasLaunchFlight: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            hasAcceptedRunResult: true,
            acceptedRunResultSucceeded: false);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Defeat));
    }

    [Test]
    public void Classify_LaunchFlightWithActivePull_ReturnsPullAnticipation()
    {
        var input = CreateInput(
            hasActivePull: true,
            hasLaunchPush: true,
            hasLaunchFlight: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterHardUngroundedSeconds,
            courseVerticalSpeed: -_tuning.FallEnterMinimumDownwardSpeed);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.PullAnticipation));
    }

    [Test]
    public void Classify_LaunchPushBeforeMinimumOnDownhillSurface_ReturnsLaunchPush()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            surfaceContext: GroundedDownhill(20f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchPush));
    }

    [Test]
    public void Classify_LaunchPushBeforeMinimumOnFlatSurface_ReturnsLaunchPush()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            surfaceContext: GroundedDownhill(0f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchPush));
    }

    [Test]
    public void Classify_LaunchFlightBeforeLaunchPushMinimum_ReturnsLaunchFlight()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.LaunchPush,
            hasLaunchPush: true,
            hasLaunchFlight: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            surfaceContext: Ungrounded());

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
    }

    [Test]
    public void Classify_LaunchFlightOnGroundedMeaningfulMovement_ReturnsLaunchFlight()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.LaunchFlight,
            hasLaunchPush: true,
            hasLaunchFlight: true,
            surfaceContext: GroundedDownhill(0f),
            coursePlanarSpeed: 10f,
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
    }

    [Test]
    public void Classify_LaunchPushAtMinimumOnGroundedMeaningfulMovement_ReturnsSlide()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds,
            surfaceContext: GroundedDownhill(0f),
            coursePlanarSpeed: 10f,
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_LaunchPushAfterMinimumOnDownhillSurface_ReturnsSlide()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            surfaceContext: GroundedDownhill(20f));

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_LaunchFlightAfterLaunchPushMinimum_ReturnsLaunchFlight()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.LaunchPush,
            hasLaunchPush: true,
            hasLaunchFlight: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds,
            courseVerticalSpeed: -_tuning.FallEnterMinimumDownwardSpeed,
            ungroundedVerticalSeparation: -_tuning.FallEnterMinimumVerticalSeparation);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
    }

    [Test]
    public void Classify_LaunchFlightPastHardUngroundedTimeout_ReturnsLaunchFlight()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.LaunchFlight,
            hasLaunchPush: true,
            hasLaunchFlight: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterHardUngroundedSeconds);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchFlight));
    }

    [TestCase(0f, 3f, 3f)]
    [TestCase(4f, 3f, 3f)]
    [TestCase(12f, 3f, 3f)]
    [TestCase(-12f, 3f, 3f)]
    [TestCase(30f, 3f, 3f)]
    [TestCase(0f, 3f, 0f)]
    [TestCase(0f, 3f, -3f)]
    public void Classify_GroundedMeaningfulPlanarMovement_ReturnsSlide(
        float downhillDegrees,
        float coursePlanarSpeed,
        float courseForwardSpeed)
    {
        var input = CreateInput(
            surfaceContext: GroundedDownhill(downhillDegrees),
            coursePlanarSpeed: coursePlanarSpeed,
            courseForwardSpeed: courseForwardSpeed);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_GroundedBankedMeaningfulPlanarMovement_ReturnsSlide()
    {
        var input = CreateInput(
            surfaceContext: new RunSurfaceContext(true, new Vector3(0.4f, 0.9f, 0.1f), 0f),
            coursePlanarSpeed: 3f,
            courseForwardSpeed: 0f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_GroundedWithDownwardCourseVerticalSpeed_ReturnsSlide()
    {
        var input = CreateInput(
            surfaceContext: GroundedDownhill(20f),
            coursePlanarSpeed: 3f,
            courseForwardSpeed: 3f,
            courseVerticalSpeed: -_tuning.FallEnterMinimumDownwardSpeed);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_GroundedBelowMeaningfulPlanarMovementThreshold_ReturnsIdle()
    {
        var input = CreateInput(
            surfaceContext: GroundedDownhill(20f),
            coursePlanarSpeed: _tuning.MeaningfulGroundedMovementThreshold - 0.01f,
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_GroundedZeroPlanarSpeed_ReturnsIdle()
    {
        var input = CreateInput(
            surfaceContext: GroundedDownhill(0f),
            coursePlanarSpeed: 0f,
            courseForwardSpeed: 0f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_GroundedHighForwardButLowPlanarSpeed_ReturnsIdle()
    {
        var input = CreateInput(
            surfaceContext: GroundedDownhill(20f),
            coursePlanarSpeed: _tuning.MeaningfulGroundedMovementThreshold - 0.01f,
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_ChangingOnlySlopeWithMeaningfulMovement_DoesNotReturnRun()
    {
        var slopes = new[] { -30f, -4f, 0f, 4f, 9f, 30f };

        foreach (var slope in slopes)
        {
            var input = CreateInput(
                surfaceContext: GroundedDownhill(slope),
                coursePlanarSpeed: 3f,
                courseForwardSpeed: 3f);

            var result = _classifier.Classify(input);

            Assert.That(result.Mode, Is.Not.EqualTo(CharacterPresentationMode.Run));
        }
    }

    [Test]
    public void Classify_ShortUngroundedSlide_PreservesSlide()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds * 0.5f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_ShortUngroundedRun_NormalizesToSlide()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Run,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds * 0.5f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_UngroundedPastMinimumWithoutFallIntentAndMoving_PreservesSlide()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds + 0.01f,
            coursePlanarSpeed: _tuning.MeaningfulGroundedMovementThreshold + 0.01f,
            courseVerticalSpeed: 0f,
            ungroundedVerticalSeparation: 0f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_UngroundedPastMinimumWithDownwardSpeed_ReturnsAirborne()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds,
            courseVerticalSpeed: -_tuning.FallEnterMinimumDownwardSpeed);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Airborne));
    }

    [Test]
    public void Classify_UngroundedPastMinimumWithDownwardSeparation_ReturnsAirborne()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds,
            ungroundedVerticalSeparation: -_tuning.FallEnterMinimumVerticalSeparation);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Airborne));
    }

    [Test]
    public void Classify_UngroundedPastHardTimeout_ReturnsAirborne()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterHardUngroundedSeconds);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Airborne));
    }

    [Test]
    public void Classify_CurrentAirborneStillUngrounded_ReturnsAirborne()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Airborne,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds * 0.5f,
            coursePlanarSpeed: 4f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Airborne));
    }

    [Test]
    public void Classify_UngroundedPastMinimumWithoutFallIntentAndStopped_ReturnsIdle()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Idle,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.FallEnterMinimumUngroundedSeconds + 0.01f,
            coursePlanarSpeed: _tuning.MeaningfulGroundedMovementThreshold - 0.01f,
            courseVerticalSpeed: 0f,
            ungroundedVerticalSeparation: 0f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_AirborneGroundedMeaningfulMovement_ReturnsSlide()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Airborne,
            surfaceContext: GroundedDownhill(12f),
            coursePlanarSpeed: 4f,
            courseForwardSpeed: 4f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_SlideWithinMinimumModeDuration_PreservesSlideWhenMovementDropsBelowThreshold()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration * 0.5f,
            surfaceContext: GroundedDownhill(0f),
            coursePlanarSpeed: 0f,
            courseForwardSpeed: 0f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_RunWithinMinimumModeDurationAndMovementStops_ReturnsIdle()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Run,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration * 0.5f,
            surfaceContext: GroundedDownhill(20f),
            coursePlanarSpeed: 0f,
            courseForwardSpeed: 0f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_RunWithinMinimumModeDurationAndMeaningfulMovement_ReturnsSlide()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Run,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration * 0.5f,
            surfaceContext: GroundedDownhill(20f),
            coursePlanarSpeed: 4f,
            courseForwardSpeed: 4f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [TestCaseSource(nameof(NormalRuntimeInputs))]
    public void Classify_NormalRuntimePath_DoesNotReturnRun(CharacterPresentationClassificationInput input)
    {
        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.Not.EqualTo(CharacterPresentationMode.Run));
    }

    [Test]
    public void CharacterPresentationView_DefaultTuning_UsesStableSlideOnlyThresholds()
    {
        var gameObject = new GameObject("Character Presentation View");

        try
        {
            var view = gameObject.AddComponent<CharacterPresentationView>();

            Assert.That(view.FallEnterMinimumUngroundedSeconds, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(view.FallEnterMinimumDownwardSpeed, Is.EqualTo(1.5f).Within(0.0001f));
            Assert.That(view.FallEnterMinimumVerticalSeparation, Is.EqualTo(0.18f).Within(0.0001f));
            Assert.That(view.FallEnterHardUngroundedSeconds, Is.EqualTo(0.65f).Within(0.0001f));
            Assert.That(view.MeaningfulGroundedMovementThreshold, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(view.MinimumLocomotionModeDuration, Is.EqualTo(0.35f).Within(0.0001f));
            Assert.That(view.LaunchPushMinimumSeconds, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(view.LaunchFlightMaximumGroundedWaitSeconds, Is.EqualTo(0.35f).Within(0.0001f));
            Assert.That(view.PresentationSupportMaximumSurfaceLiftSpeed, Is.EqualTo(0.35f).Within(0.0001f));
            Assert.That(view.PresentationSupportReacquireSeconds, Is.EqualTo(0.08f).Within(0.0001f));
            Assert.That(view.SlideReferenceSpeed, Is.EqualTo(8f).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    private static IEnumerable<TestCaseData> NormalRuntimeInputs()
    {
        yield return new TestCaseData(CreateRuntimeInput(GroundedDownhillStatic(0f), 3f, 3f))
            .SetName("Flat meaningful movement");

        yield return new TestCaseData(CreateRuntimeInput(GroundedDownhillStatic(20f), 3f, 3f))
            .SetName("Downhill meaningful movement");

        yield return new TestCaseData(CreateRuntimeInput(GroundedDownhillStatic(-20f), 3f, 3f))
            .SetName("Uphill meaningful movement");

        yield return new TestCaseData(CreateRuntimeInput(GroundedDownhillStatic(0f), 0f, 0f))
            .SetName("Stopped grounded");

        yield return new TestCaseData(CreateRuntimeInput(UngroundedStatic(), 3f, 3f, CharacterPresentationMode.Slide, 1f, 1f))
            .SetName("Long ungrounded");

        yield return new TestCaseData(CreateRuntimeInput(UngroundedStatic(), 3f, 3f, CharacterPresentationMode.Run, 1f, 0.01f))
            .SetName("Short ungrounded reserved Run");
    }

    private CharacterPresentationClassificationInput CreateInput(
        CharacterPresentationMode currentMode = CharacterPresentationMode.Idle,
        float currentModeElapsedSeconds = 1f,
        float ungroundedElapsedSeconds = 0f,
        bool isPreLaunch = false,
        bool isRunActive = true,
        bool hasAcceptedRunResult = false,
        bool acceptedRunResultSucceeded = false,
        bool hasActivePull = false,
        bool hasLaunchPush = false,
        bool hasLaunchFlight = false,
        float launchPushElapsedSeconds = 0f,
        RunSurfaceContext? surfaceContext = null,
        float coursePlanarSpeed = 4f,
        float courseForwardSpeed = 4f,
        float courseVerticalSpeed = 0f,
        float ungroundedVerticalSeparation = 0f)
    {
        return new CharacterPresentationClassificationInput(
            currentMode,
            currentModeElapsedSeconds,
            ungroundedElapsedSeconds,
            isPreLaunch,
            isRunActive,
            hasAcceptedRunResult,
            acceptedRunResultSucceeded,
            hasActivePull,
            hasLaunchPush,
            hasLaunchFlight,
            launchPushElapsedSeconds,
            surfaceContext.GetValueOrDefault(GroundedDownhill(0f)),
            coursePlanarSpeed,
            courseForwardSpeed,
            courseVerticalSpeed,
            ungroundedVerticalSeparation,
            Vector3.zero);
    }

    private static CharacterPresentationClassificationInput CreateRuntimeInput(
        RunSurfaceContext surfaceContext,
        float coursePlanarSpeed,
        float courseForwardSpeed,
        CharacterPresentationMode currentMode = CharacterPresentationMode.Idle,
        float currentModeElapsedSeconds = 1f,
        float ungroundedElapsedSeconds = 0f,
        float courseVerticalSpeed = 0f,
        float ungroundedVerticalSeparation = 0f)
    {
        return new CharacterPresentationClassificationInput(
            currentMode,
            currentModeElapsedSeconds,
            ungroundedElapsedSeconds,
            isPreLaunch: false,
            isRunActive: true,
            hasAcceptedRunResult: false,
            acceptedRunResultSucceeded: false,
            hasActivePull: false,
            hasLaunchPush: false,
            hasLaunchFlight: false,
            launchPushElapsedSeconds: 0f,
            surfaceContext,
            coursePlanarSpeed,
            courseForwardSpeed,
            courseVerticalSpeed,
            ungroundedVerticalSeparation,
            Vector3.zero);
    }

    private RunSurfaceContext GroundedDownhill(float degrees)
    {
        return GroundedDownhillStatic(degrees);
    }

    private RunSurfaceContext Ungrounded()
    {
        return UngroundedStatic();
    }

    private static RunSurfaceContext GroundedDownhillStatic(float degrees)
    {
        return new RunSurfaceContext(true, Vector3.up, degrees);
    }

    private static RunSurfaceContext UngroundedStatic()
    {
        return new RunSurfaceContext(false, Vector3.up, 0f);
    }

    private sealed class FakeCharacterPresentationTuning : ICharacterPresentationTuning
    {
        public float FallEnterMinimumUngroundedSeconds { get; set; } = 0.3f;
        public float FallEnterMinimumDownwardSpeed { get; set; } = 1.5f;
        public float FallEnterMinimumVerticalSeparation { get; set; } = 0.18f;
        public float FallEnterHardUngroundedSeconds { get; set; } = 0.65f;
        public float MeaningfulGroundedMovementThreshold { get; set; } = 0.5f;
        public float MinimumLocomotionModeDuration { get; set; } = 0.35f;
        public float LaunchPushMinimumSeconds { get; set; } = 0.25f;
        public float LaunchFlightMaximumGroundedWaitSeconds { get; set; } = 0.35f;
        public float PresentationSupportMaximumSurfaceLiftSpeed { get; set; } = 0.35f;
        public float PresentationSupportReacquireSeconds { get; set; } = 0.08f;
        public float SlideReferenceSpeed { get; set; } = 8f;
        public float MinimumPlaybackSpeedMultiplier { get; set; } = 0.5f;
        public float MaximumPlaybackSpeedMultiplier { get; set; } = 1.5f;
    }
}
