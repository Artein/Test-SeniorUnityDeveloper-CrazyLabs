using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class CharacterPresentationSupportTrackerTests
{
    private ICharacterPresentationSupportTracker _tracker;
    private FakeCharacterPresentationTuning _tuning;

    [SetUp]
    public void OnSetUp()
    {
        _tracker = new CharacterPresentationSupportTracker();
        _tuning = new FakeCharacterPresentationTuning();
    }

    [Test]
    public void Update_GroundedWithSurfaceLiftBelowThreshold_ReturnsSupported()
    {
        var sample = Update(
            new RunSurfaceContext(true, Vector3.up, 12f),
            Vector3.zero,
            new Vector3(0f, _tuning.PresentationSupportMaximumSurfaceLiftSpeed, 8f),
            deltaTime: 0.02f);

        Assert.That(sample.SurfaceContext.IsGrounded, Is.True);
        Assert.That(sample.SurfaceContext.ForwardDownhillDegrees, Is.EqualTo(12f).Within(0.0001f));
        Assert.That(sample.UngroundedElapsedSeconds, Is.Zero);
        Assert.That(sample.UngroundedVerticalSeparation, Is.Zero);
    }

    [Test]
    public void Update_GroundedWithSurfaceLiftAboveThreshold_ReturnsUnsupported()
    {
        var sample = Update(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.zero,
            new Vector3(0f, _tuning.PresentationSupportMaximumSurfaceLiftSpeed + 0.01f, 8f),
            deltaTime: 0.02f);

        Assert.That(sample.SurfaceContext.IsGrounded, Is.False);
        Assert.That(sample.UngroundedElapsedSeconds, Is.EqualTo(0.02f).Within(0.0001f));
        Assert.That(sample.UngroundedVerticalSeparation, Is.Zero);
    }

    [Test]
    public void Update_DownhillCourseVerticalSpeedWithSupportedSurface_ReturnsSupported()
    {
        var sample = Update(
            new RunSurfaceContext(true, Vector3.up, 20f),
            Vector3.zero,
            new Vector3(0f, -2f, 8f),
            deltaTime: 0.02f);

        Assert.That(sample.SurfaceContext.IsGrounded, Is.True);
        Assert.That(sample.UngroundedElapsedSeconds, Is.Zero);
    }

    [Test]
    public void Update_SupportedFlickerBeforeReacquire_DoesNotResetUnsupportedState()
    {
        Update(
            new RunSurfaceContext(false, Vector3.up, 0f),
            new Vector3(0f, 3f, 0f),
            new Vector3(0f, -1f, 8f),
            deltaTime: 0.02f);

        var sample = Update(
            new RunSurfaceContext(true, Vector3.up, 0f),
            new Vector3(0f, 2.9f, 0f),
            new Vector3(0f, -0.1f, 8f),
            deltaTime: 0.02f);

        Assert.That(sample.SurfaceContext.IsGrounded, Is.False);
        Assert.That(sample.UngroundedElapsedSeconds, Is.EqualTo(0.04f).Within(0.0001f));
        Assert.That(sample.UngroundedVerticalSeparation, Is.EqualTo(-0.1f).Within(0.0001f));
    }

    [Test]
    public void Update_SupportedForReacquireSeconds_ResetsUnsupportedState()
    {
        Update(
            new RunSurfaceContext(false, Vector3.up, 0f),
            new Vector3(0f, 3f, 0f),
            new Vector3(0f, -1f, 8f),
            deltaTime: 0.02f);

        var sample = Update(
            new RunSurfaceContext(true, Vector3.up, 0f),
            new Vector3(0f, 2.9f, 0f),
            new Vector3(0f, -0.1f, 8f),
            deltaTime: _tuning.PresentationSupportReacquireSeconds);

        Assert.That(sample.SurfaceContext.IsGrounded, Is.True);
        Assert.That(sample.UngroundedElapsedSeconds, Is.Zero);
        Assert.That(sample.UngroundedVerticalSeparation, Is.Zero);
    }

    [Test]
    public void Update_Reset_ClearsUnsupportedState()
    {
        Update(
            new RunSurfaceContext(false, Vector3.up, 0f),
            new Vector3(0f, 3f, 0f),
            new Vector3(0f, -1f, 8f),
            deltaTime: 0.02f);

        var sample = _tracker.Update(
            new RunSurfaceContext(false, Vector3.up, 0f),
            new Vector3(0f, 2f, 0f),
            new Vector3(0f, -1f, 8f),
            Vector3.up,
            _tuning,
            deltaTime: 0.02f,
            reset: true);

        Assert.That(sample.SurfaceContext.IsGrounded, Is.True);
        Assert.That(sample.UngroundedElapsedSeconds, Is.Zero);
        Assert.That(sample.UngroundedVerticalSeparation, Is.Zero);
    }

    private CharacterPresentationSupportSample Update(
        RunSurfaceContext surfaceContext,
        Vector3 position,
        Vector3 velocity,
        float deltaTime)
    {
        return _tracker.Update(
            surfaceContext,
            position,
            velocity,
            Vector3.up,
            _tuning,
            deltaTime,
            reset: false);
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
