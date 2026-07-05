using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class CharacterVisualPoseSmootherTests
{
    private FakeCharacterVisualFollowTuning _tuning;
    private CharacterVisualPoseSmoother _smoother;

    [SetUp]
    public void OnSetUp()
    {
        _tuning = new FakeCharacterVisualFollowTuning
        {
            VisualPositionResponseRate = 60f,
            VisualHeadingResponseRate = 45f,
            VisualUpTiltResponseRate = 18f,
            VisualMaxPositionLag = 0.06f,
            VisualSnapDistance = 0.75f,
            VisualSnapAngleDegrees = 45f
        };

        _smoother = new CharacterVisualPoseSmoother();
    }

    [Test]
    public void Update_FirstSample_SnapsToTargetPose()
    {
        var currentPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        var targetPose = new CharacterVisualPose(new Vector3(4f, 2f, -3f), Quaternion.Euler(8f, 30f, 12f));

        var smoothedPose = _smoother.Update(currentPose, targetPose, _tuning, 0.02f, false);

        AssertPose(smoothedPose, targetPose);
    }

    [Test]
    public void Update_PositionChangeWithinSnapDistance_BoundsMaximumLagWithoutSnapping()
    {
        var startPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        _smoother.Update(startPose, startPose, _tuning, 0.02f, false);
        var targetPose = new CharacterVisualPose(new Vector3(0.5f, 0f, 0f), Quaternion.identity);

        var smoothedPose = _smoother.Update(startPose, targetPose, _tuning, 0.02f, false);

        Assert.That(Vector3.Distance(smoothedPose.Position, targetPose.Position), Is.LessThanOrEqualTo(_tuning.VisualMaxPositionLag + 0.0001f));
        Assert.That(Vector3.Distance(smoothedPose.Position, targetPose.Position), Is.GreaterThan(0.0001f));
        Assert.That(smoothedPose.Position.x, Is.GreaterThan(startPose.Position.x));
    }

    [Test]
    public void Update_MovingTargetBeyondSnapDistance_BoundsMaximumLagWithoutSnapping()
    {
        var startPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        _smoother.Update(startPose, startPose, _tuning, 0.02f, false);
        var targetPose = new CharacterVisualPose(new Vector3(1f, 0f, 0f), Quaternion.identity);

        var smoothedPose = _smoother.Update(startPose, targetPose, _tuning, 0.02f, false);

        Assert.That(Vector3.Distance(smoothedPose.Position, targetPose.Position), Is.GreaterThan(0.0001f));
        Assert.That(Vector3.Distance(smoothedPose.Position, targetPose.Position), Is.LessThanOrEqualTo(_tuning.VisualMaxPositionLag + 0.0001f));
    }

    [Test]
    public void Update_ContinuousHighSpeedTargetMotionBeyondSnapDistance_BoundsMaximumLagWithoutSnapping()
    {
        var currentPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        _smoother.Update(currentPose, currentPose, _tuning, 0.02f, false);

        for (var frameIndex = 1; frameIndex <= 4; frameIndex += 1)
        {
            var targetPose = new CharacterVisualPose(new Vector3(0.84f * frameIndex, 0f, 0f), Quaternion.identity);

            currentPose = _smoother.Update(currentPose, targetPose, _tuning, 0.02f, false);

            var lag = Vector3.Distance(currentPose.Position, targetPose.Position);
            Assert.That(lag, Is.GreaterThan(0.0001f));
            Assert.That(lag, Is.LessThanOrEqualTo(_tuning.VisualMaxPositionLag + 0.0001f));
        }
    }

    [Test]
    public void Update_MovingTargetBeyondSnapAngle_SmoothsRotationWithoutSnapping()
    {
        var startPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        _smoother.Update(startPose, startPose, _tuning, 0.02f, false);
        var targetPose = new CharacterVisualPose(Vector3.zero, Quaternion.Euler(0f, 90f, 0f));

        var smoothedPose = _smoother.Update(startPose, targetPose, _tuning, 0.02f, false);

        Assert.That(Quaternion.Angle(smoothedPose.Rotation, targetPose.Rotation), Is.GreaterThan(0.0001f));
        Assert.That(Quaternion.Angle(smoothedPose.Rotation, targetPose.Rotation), Is.LessThan(90f));
    }

    [Test]
    public void Update_SmallHeadingAndUpTiltChange_SmoothsHeadingMoreAggressivelyThanUpTilt()
    {
        var startPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        _smoother.Update(startPose, startPose, _tuning, 0.02f, false);
        var targetPose = new CharacterVisualPose(Vector3.zero, Quaternion.Euler(20f, 20f, 20f));

        var smoothedPose = _smoother.Update(startPose, targetPose, _tuning, 0.02f, false);

        var forwardMovedDegrees = Vector3.Angle(Vector3.forward, smoothedPose.Rotation * Vector3.forward);
        var upMovedDegrees = Vector3.Angle(Vector3.up, smoothedPose.Rotation * Vector3.up);
        Assert.That(forwardMovedDegrees, Is.GreaterThan(upMovedDegrees));
        Assert.That(Quaternion.Angle(smoothedPose.Rotation, targetPose.Rotation), Is.GreaterThan(0.0001f));
    }

    [Test]
    public void Update_ForcedSnap_SnapsEvenWithinThresholds()
    {
        var startPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        _smoother.Update(startPose, startPose, _tuning, 0.02f, false);
        var targetPose = new CharacterVisualPose(new Vector3(0.2f, 0f, 0f), Quaternion.Euler(0f, 10f, 0f));

        var smoothedPose = _smoother.Update(startPose, targetPose, _tuning, 0.02f, true);

        AssertPose(smoothedPose, targetPose);
    }

    private static void AssertPose(CharacterVisualPose actual, CharacterVisualPose expected)
    {
        Assert.That(actual.Position.x, Is.EqualTo(expected.Position.x).Within(0.0001f));
        Assert.That(actual.Position.y, Is.EqualTo(expected.Position.y).Within(0.0001f));
        Assert.That(actual.Position.z, Is.EqualTo(expected.Position.z).Within(0.0001f));
        Assert.That(Quaternion.Angle(actual.Rotation, expected.Rotation), Is.EqualTo(0f).Within(0.0001f));
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
