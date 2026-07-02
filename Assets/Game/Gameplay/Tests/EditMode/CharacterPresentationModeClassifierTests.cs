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
    public void Classify_AcceptedSuccessfulRunResult_ReturnsVictory()
    {
        var input = CreateInput(hasAcceptedRunResult: true, acceptedRunResultSucceeded: true);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Victory));
    }

    [Test]
    public void Classify_AcceptedFailedRunResult_ReturnsDefeat()
    {
        var input = CreateInput(hasAcceptedRunResult: true, acceptedRunResultSucceeded: false);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Defeat));
    }

    [Test]
    public void Classify_PreLaunch_ReturnsIdle()
    {
        var input = CreateInput(isPreLaunch: true, isRunActive: false, surfaceContext: GroundedDownhill(20f), courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_RunInactive_ReturnsIdle()
    {
        var input = CreateInput(isRunActive: false, surfaceContext: GroundedDownhill(20f), courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_ActivePullOnDownhillSurface_ReturnsPullAnticipation()
    {
        var input = CreateInput(hasActivePull: true, surfaceContext: GroundedDownhill(20f), courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.PullAnticipation));
    }

    [Test]
    public void Classify_ActivePullOnFlatForwardSurface_ReturnsPullAnticipation()
    {
        var input = CreateInput(hasActivePull: true, surfaceContext: GroundedDownhill(0f), courseForwardSpeed: 10f);

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
            surfaceContext: GroundedDownhill(20f),
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.PullAnticipation));
    }

    [Test]
    public void Classify_ActivePullWithAcceptedSuccessfulRunResult_ReturnsVictory()
    {
        var input = CreateInput(hasActivePull: true, hasAcceptedRunResult: true, acceptedRunResultSucceeded: true);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Victory));
    }

    [Test]
    public void Classify_LaunchPushWithAcceptedFailedRunResult_ReturnsDefeat()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            hasAcceptedRunResult: true,
            acceptedRunResultSucceeded: false);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Defeat));
    }

    [Test]
    public void Classify_LaunchPushBeforeMinimumOnDownhillSurface_ReturnsLaunchPush()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            surfaceContext: GroundedDownhill(20f),
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchPush));
    }

    [Test]
    public void Classify_LaunchPushBeforeMinimumOnFlatForwardSurface_ReturnsLaunchPush()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds * 0.5f,
            surfaceContext: GroundedDownhill(0f),
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.LaunchPush));
    }

    [Test]
    public void Classify_LaunchPushAtMinimumOnFlatForwardSurface_ReturnsRun()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds,
            surfaceContext: GroundedDownhill(0f),
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Run));
    }

    [Test]
    public void Classify_LaunchPushAfterMinimumOnDownhillSurface_ReturnsSlide()
    {
        var input = CreateInput(
            hasLaunchPush: true,
            launchPushElapsedSeconds: _tuning.LaunchPushMinimumSeconds + 0.01f,
            surfaceContext: GroundedDownhill(20f),
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_GroundedDownhill_ReturnsSlide()
    {
        var input = CreateInput(surfaceContext: GroundedDownhill(12f), courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_GroundedMildDownhillMovingForward_ReturnsRun()
    {
        var input = CreateInput(
            surfaceContext: GroundedDownhill(_tuning.SlideEnterDownhillDegrees - 0.5f),
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Run));
    }

    [Test]
    public void Classify_GroundedAtSlideEnterThreshold_ReturnsSlide()
    {
        var input = CreateInput(
            surfaceContext: GroundedDownhill(_tuning.SlideEnterDownhillDegrees),
            courseForwardSpeed: 10f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_GroundedFlatMovingForward_ReturnsRun()
    {
        var input = CreateInput(surfaceContext: GroundedDownhill(0f), courseForwardSpeed: 3f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Run));
    }

    [Test]
    public void Classify_GroundedFlatStopped_ReturnsIdle()
    {
        var input = CreateInput(surfaceContext: GroundedDownhill(0f), courseForwardSpeed: 0f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_GroundedFlatMovingBackward_ReturnsIdle()
    {
        var input = CreateInput(surfaceContext: GroundedDownhill(0f), courseForwardSpeed: -3f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Idle));
    }

    [Test]
    public void Classify_ShortUngroundedSlide_PreservesSlide()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.AirborneDelaySeconds * 0.5f,
            courseForwardSpeed: 4f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_LongUngrounded_ReturnsAirborne()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            surfaceContext: Ungrounded(),
            ungroundedElapsedSeconds: _tuning.AirborneDelaySeconds + 0.01f,
            courseForwardSpeed: 4f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Airborne));
    }

    [Test]
    public void Classify_AirborneGroundedDownhill_ReturnsSlide()
    {
        var input = CreateInput(currentMode: CharacterPresentationMode.Airborne, surfaceContext: GroundedDownhill(degrees: 12f),
            courseForwardSpeed: 4f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_SlideWithinMinimumModeDuration_PreservesSlideWhenSurfaceBecomesFlat()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration * 0.5f,
            surfaceContext: GroundedDownhill(0f),
            courseForwardSpeed: 5f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_RunWithinMinimumModeDuration_PreservesRunWhenSurfaceBecomesDownhill()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Run,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration * 0.5f,
            surfaceContext: GroundedDownhill(_tuning.SlideEnterDownhillDegrees + 1f),
            courseForwardSpeed: 5f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Run));
    }

    [Test]
    public void Classify_RunAboveSlideExitButBelowSlideEnter_PreservesRun()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Run,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration + 0.01f,
            surfaceContext: GroundedDownhill(_tuning.RunFlatMaximumAbsSlopeDegrees + 0.5f),
            courseForwardSpeed: 5f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Run));
    }

    [Test]
    public void Classify_SlideAboveSlideExitButInsideRunFlatThreshold_PreservesSlide()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration + 0.01f,
            surfaceContext: GroundedDownhill(_tuning.SlideExitDownhillDegrees + 0.5f),
            courseForwardSpeed: 5f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
    }

    [Test]
    public void Classify_SlideAtSlideExitWithForwardSpeed_ReturnsRun()
    {
        var input = CreateInput(
            currentMode: CharacterPresentationMode.Slide,
            currentModeElapsedSeconds: _tuning.MinimumLocomotionModeDuration + 0.01f,
            surfaceContext: GroundedDownhill(_tuning.SlideExitDownhillDegrees),
            courseForwardSpeed: 5f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Run));
    }

    [Test]
    public void CharacterPresentationView_DefaultTuning_UsesStableRunSlideThresholds()
    {
        var gameObject = new GameObject("Character Presentation View");

        try
        {
            var view = gameObject.AddComponent<CharacterPresentationView>();

            Assert.That(view.SlideEnterDownhillDegrees, Is.EqualTo(9f).Within(0.0001f));
            Assert.That(view.SlideExitDownhillDegrees, Is.EqualTo(3.5f).Within(0.0001f));
            Assert.That(view.MinimumLocomotionModeDuration, Is.EqualTo(0.35f).Within(0.0001f));
            Assert.That(view.RunFlatMaximumAbsSlopeDegrees, Is.EqualTo(4f).Within(0.0001f));
            Assert.That(view.RunMinimumForwardSpeed, Is.EqualTo(0.5f).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
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
        float launchPushElapsedSeconds = 0f,
        RunSurfaceContext? surfaceContext = null,
        float coursePlanarSpeed = 4f,
        float courseForwardSpeed = 4f)
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
            launchPushElapsedSeconds,
            surfaceContext.GetValueOrDefault(GroundedDownhill(0f)),
            coursePlanarSpeed,
            courseForwardSpeed,
            Vector3.zero);
    }

    private RunSurfaceContext GroundedDownhill(float degrees)
    {
        return new RunSurfaceContext(true, Vector3.up, degrees);
    }

    private RunSurfaceContext Ungrounded()
    {
        return new RunSurfaceContext(false, Vector3.up, 0f);
    }

    private sealed class FakeCharacterPresentationTuning : ICharacterPresentationTuning
    {
        public float AirborneDelaySeconds { get; set; } = 0.12f;
        public float SlideEnterDownhillDegrees { get; set; } = 9f;
        public float SlideExitDownhillDegrees { get; set; } = 3.5f;
        public float RunFlatMaximumAbsSlopeDegrees { get; set; } = 4f;
        public float RunMinimumForwardSpeed { get; set; } = 0.5f;
        public float MinimumLocomotionModeDuration { get; set; } = 0.35f;
        public float LaunchPushMinimumSeconds { get; set; } = 0.25f;
        public float SlideReferenceSpeed { get; set; } = 8f;
        public float RunReferenceSpeed { get; set; } = 8f;
        public float MinimumPlaybackSpeedMultiplier { get; set; } = 0.5f;
        public float MaximumPlaybackSpeedMultiplier { get; set; } = 1.5f;
    }
}
