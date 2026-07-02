using Game.Foundation.Input;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSteeringGestureTests
{
    private FakePlayerSteeringConfig _config;
    private RunSteeringGesture _gesture;

    [SetUp]
    public void OnSetUp()
    {
        _config = new FakePlayerSteeringConfig
        {
            RunSteeringDeadzoneFraction = 0.2f,
            RunSteeringRangePixels = 100f
        };

        _gesture = new RunSteeringGesture(_config);
    }

    [Test]
    public void TryBegin_WhenInactive_CapturesOriginRangeAndReturnsNeutral()
    {
        var began = ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(25f, 40f)), 96f);

        Assert.That(began, Is.True);
        Assert.That(_gesture.isActive, Is.True);
        Assert.That(_gesture.origin, Is.EqualTo(new Vector2(25f, 40f)));
        Assert.That(_gesture.capturedRangePixels, Is.EqualTo(100f));
        Assert.That(_gesture.RequestedSteering, Is.Zero);
        Assert.That(_config.RangePixelRawDpiRequest, Is.EqualTo(96f));
    }

    [Test]
    public void TryMove_RightFromOrigin_ProducesPositiveSteering()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(110f, 10f)));

        Assert.That(_gesture.RequestedSteering, Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void TryMove_LeftFromOrigin_ProducesNegativeSteering()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(-10f, 90f)));

        Assert.That(_gesture.RequestedSteering, Is.EqualTo(-0.5f).Within(0.0001f));
    }

    [Test]
    public void TryMove_VerticalOnly_ReturnsNeutral()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(50f, 500f)));

        Assert.That(_gesture.RequestedSteering, Is.Zero);
    }

    [Test]
    public void TryMove_InsideDeadzone_ReturnsNeutral()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(69f, 50f)));

        Assert.That(_gesture.RequestedSteering, Is.Zero);
    }

    [Test]
    public void TryMove_AtRange_ReturnsFullSteering()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(150f, 50f)));

        Assert.That(_gesture.RequestedSteering, Is.EqualTo(1f));
    }

    [Test]
    public void TryMove_BeyondRange_ClampsToFullSteering()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(250f, 50f)));

        Assert.That(_gesture.RequestedSteering, Is.EqualTo(1f));
    }

    [Test]
    public void TryBegin_SecondPointerWhileActive_DoesNotStealControl()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        var began = ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(2, new Vector2(900f, 50f)), 96f);
        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(2, new Vector2(1000f, 50f)));

        Assert.That(began, Is.False);
        Assert.That(_gesture.origin, Is.EqualTo(new Vector2(50f, 50f)));
        Assert.That(_gesture.RequestedSteering, Is.Zero);
    }

    [Test]
    public void NonActivePointerMoveReleaseAndCancel_AreIgnored()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);

        var moved = ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(2, new Vector2(150f, 50f)));
        var released = ((IRunSteeringGesture)_gesture).TryRelease(new PointerInput(2, new Vector2(150f, 50f)));
        var canceled = ((IRunSteeringGesture)_gesture).TryCancel(new PointerInput(2, new Vector2(150f, 50f)));

        Assert.That(moved, Is.False);
        Assert.That(released, Is.False);
        Assert.That(canceled, Is.False);
        Assert.That(_gesture.isActive, Is.True);
        Assert.That(_gesture.RequestedSteering, Is.Zero);
    }

    [Test]
    public void TryRelease_ActivePointer_ClearsOutputAndEndsGesture()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);
        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(150f, 50f)));

        var released = ((IRunSteeringGesture)_gesture).TryRelease(new PointerInput(1, new Vector2(150f, 50f)));

        Assert.That(released, Is.True);
        Assert.That(_gesture.isActive, Is.False);
        Assert.That(_gesture.RequestedSteering, Is.Zero);
    }

    [Test]
    public void TryCancel_ActivePointer_ClearsOutputAndEndsGesture()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);
        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(150f, 50f)));

        var canceled = ((IRunSteeringGesture)_gesture).TryCancel(new PointerInput(1, new Vector2(150f, 50f)));

        Assert.That(canceled, Is.True);
        Assert.That(_gesture.isActive, Is.False);
        Assert.That(_gesture.RequestedSteering, Is.Zero);
    }

    [Test]
    public void Reset_ClearsActiveGestureState()
    {
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(50f, 50f)), 96f);
        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(150f, 50f)));

        ((IRunSteeringGesture)_gesture).Reset();

        Assert.That(_gesture.isActive, Is.False);
        Assert.That(_gesture.origin, Is.EqualTo(Vector2.zero));
        Assert.That(_gesture.capturedRangePixels, Is.Zero);
        Assert.That(_gesture.RequestedSteering, Is.Zero);
    }

    [Test]
    public void TryMove_ConfigRangeChangesAfterBegin_UsesCapturedRangeUntilNextGesture()
    {
        _config.RunSteeringDeadzoneFraction = 0f;
        _config.RunSteeringRangePixels = 100f;
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(0f, 0f)), 96f);
        _config.RunSteeringRangePixels = 200f;

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(100f, 0f)));

        Assert.That(_gesture.RequestedSteering, Is.EqualTo(1f));

        ((IRunSteeringGesture)_gesture).TryRelease(new PointerInput(1, new Vector2(100f, 0f)));
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(0f, 0f)), 96f);
        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(100f, 0f)));

        Assert.That(_gesture.RequestedSteering, Is.EqualTo(0.5f));
    }

    [Test]
    public void TryMove_EdgeOrigin_UsesStrictPhysicalRange()
    {
        _config.RunSteeringDeadzoneFraction = 0f;
        ((IRunSteeringGesture)_gesture).TryBegin(new PointerInput(1, new Vector2(0f, 50f)), 96f);

        ((IRunSteeringGesture)_gesture).TryMove(new PointerInput(1, new Vector2(100f, 50f)));

        Assert.That(_gesture.RequestedSteering, Is.EqualTo(1f));
    }

    private sealed class FakePlayerSteeringConfig : IPlayerSteeringConfig
    {
        public float RunSteeringRangeCentimeters { get; set; }
        public float RunSteeringDeadzoneFraction { get; set; }
        public float RunSteeringResponsiveness { get; set; }
        public float FallbackDpi { get; set; }
        public float MinimumAcceptedDpi { get; set; }
        public float MaximumAcceptedDpi { get; set; }
        public float MaximumTurnDegreesPerSecond { get; set; }
        public float MinimumSteerSpeed { get; set; }
        public float MaximumPlanarSpeed { get; set; }
        public float LaunchBurstPlanarSpeedGraceSeconds { get; set; }
        public float LaunchBurstPlanarSpeedRecoverySeconds { get; set; }
        public float LaunchBurstMaximumPlanarSpeedMultiplier { get; set; }
        public float LaunchLandingStabilizationSeconds { get; set; }
        public float LaunchLandingMaximumLiftSpeed { get; set; }
        public float RunSteeringFrameNormalSlewDegreesPerSecond { get; set; }
        public float RunSteeringFrameSnapDegrees { get; set; }
        public float RunSteeringFrameUngroundedGraceSeconds { get; set; }
        public float RunSteeringFrameSuspectNormalConfirmationSeconds { get; set; }
        public float RunSteeringRangePixels { get; set; }
        public float RangePixelRawDpiRequest { get; private set; }

        public float ResolveRunSteeringDpi(float rawDpi)
        {
            return rawDpi;
        }

        public float ResolveRunSteeringRangePixels(float rawDpi)
        {
            RangePixelRawDpiRequest = rawDpi;
            return RunSteeringRangePixels;
        }
    }
}
