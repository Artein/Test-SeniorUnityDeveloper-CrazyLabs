using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Diagnostics;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunDiagnosticsOverlaySnapEstimatorTests
{
    private RunDiagnosticsOverlaySnapEstimator _estimator;
    private FakeCharacterVisualFollowTuning _tuning;

    [SetUp]
    public void OnSetUp()
    {
        _estimator = new RunDiagnosticsOverlaySnapEstimator();
        _tuning = new FakeCharacterVisualFollowTuning
        {
            VisualSnapDistance = 0.75f,
            VisualSnapAngleDegrees = 45f
        };
    }

    [Test]
    public void Estimate_TargetStepCrossesSnapDistanceWithZeroLag_ReturnsPosition()
    {
        var snapReason = _estimator.Estimate(0.84f, 0f, 0f, _tuning);

        Assert.That(snapReason, Is.EqualTo(RunDiagnosticsOverlaySnapReason.Position));
    }

    [Test]
    public void Estimate_TargetRotationCrossesSnapAngleWithZeroLag_ReturnsRotation()
    {
        var snapReason = _estimator.Estimate(0f, 60f, 0f, _tuning);

        Assert.That(snapReason, Is.EqualTo(RunDiagnosticsOverlaySnapReason.Rotation));
    }

    [Test]
    public void Estimate_BoundedVisualLag_ReturnsNone()
    {
        var snapReason = _estimator.Estimate(0.84f, 60f, 6f, _tuning);

        Assert.That(snapReason, Is.EqualTo(RunDiagnosticsOverlaySnapReason.None));
    }

    private sealed class FakeCharacterVisualFollowTuning : ICharacterVisualFollowTuning
    {
        public float VisualPositionResponseRate { get; set; }
        public float VisualHeadingResponseRate { get; set; }
        public float VisualUpTiltResponseRate { get; set; }
        public float VisualMaxPositionLag { get; set; }
        public float VisualSnapDistance { get; set; }
        public float VisualSnapAngleDegrees { get; set; }
    }
}
