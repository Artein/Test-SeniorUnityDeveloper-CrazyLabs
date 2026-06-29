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
    public void Classify_GroundedDownhill_ReturnsSlide()
    {
        var input = CreateInput(surfaceContext: GroundedDownhill(12f), courseForwardSpeed: 10f);

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
    public void Classify_GroundedFlatMovingBackward_ReturnsSlide()
    {
        var input = CreateInput(surfaceContext: GroundedDownhill(0f), courseForwardSpeed: -3f);

        var result = _classifier.Classify(input);

        Assert.That(result.Mode, Is.EqualTo(CharacterPresentationMode.Slide));
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
        var input = CreateInput(currentMode: CharacterPresentationMode.Airborne, surfaceContext: GroundedDownhill(12f), courseForwardSpeed: 4f);

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

    private CharacterPresentationClassificationInput CreateInput(
        CharacterPresentationMode currentMode = CharacterPresentationMode.Idle,
        float currentModeElapsedSeconds = 1f,
        float ungroundedElapsedSeconds = 0f,
        bool isPreLaunch = false,
        bool isRunActive = true,
        bool hasAcceptedRunResult = false,
        bool acceptedRunResultSucceeded = false,
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
        public float SlideEnterDownhillDegrees { get; set; } = 6f;
        public float SlideExitDownhillDegrees { get; set; } = 3f;
        public float RunFlatMaximumAbsSlopeDegrees { get; set; } = 4f;
        public float RunMinimumForwardSpeed { get; set; } = 0.5f;
        public float MinimumLocomotionModeDuration { get; set; } = 0.15f;
        public float SlideReferenceSpeed { get; set; } = 8f;
        public float RunReferenceSpeed { get; set; } = 8f;
        public float MinimumPlaybackSpeedMultiplier { get; set; } = 0.5f;
        public float MaximumPlaybackSpeedMultiplier { get; set; } = 1.5f;
    }
}
