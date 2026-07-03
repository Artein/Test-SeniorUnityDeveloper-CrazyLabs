using Game.Gameplay;
using Game.Foundation.Time;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceSteeringFrameSourceTests
{
    private FakeRunSurfaceContextSource _surfaceContextSource;
    private FakePlayerSteeringConfig _config;
    private FakeTime _clock;
    private RunSurfaceSteeringFrameSource _source;
    private IRunSteeringFrameResetter _resetter;
    private IFixedTickable _fixedTickable;

    [SetUp]
    public void OnSetUp()
    {
        _surfaceContextSource = new FakeRunSurfaceContextSource();

        _config = new FakePlayerSteeringConfig
        {
            RunSteeringFrameNormalSlewDegreesPerSecond = 90f,
            RunSteeringFrameSnapDegrees = 60f,
            RunSteeringFrameUngroundedGraceSeconds = 0.08f,
            RunSteeringFrameSuspectNormalConfirmationSeconds = 0.04f
        };

        _clock = new FakeTime
        {
            FixedDeltaTime = 0.02f
        };
        _source = new RunSurfaceSteeringFrameSource(_surfaceContextSource, _config, _clock);
        _resetter = _source;
        _fixedTickable = _source;
    }

    [Test]
    public void GetUpDirection_BeforeReset_ReturnsFallbackAndDoesNotReadSurface()
    {
        var groundNormal = new Vector3(0f, 2f, 1f).normalized;
        var fallbackUp = new Vector3(1f, 1f, 0f).normalized;
        _surfaceContextSource.Current = Grounded(groundNormal);

        _fixedTickable.FixedTick();
        var upDirection = _source.GetUpDirection(fallbackUp);

        AssertVectorNear(upDirection, fallbackUp);
        Assert.That(_surfaceContextSource.CurrentReadCount, Is.Zero);
    }

    [Test]
    public void Reset_ValidLaunchUp_PrimesFrameFromLaunchUp()
    {
        var launchUp = new Vector3(0f, 1f, 1f).normalized;

        _resetter.Reset(launchUp);

        AssertVectorNear(_source.GetUpDirection(Vector3.up), launchUp);
    }

    [Test]
    public void Reset_InvalidLaunchUp_FallsBackSafely()
    {
        _resetter.Reset(Vector3.zero);

        AssertVectorNear(_source.GetUpDirection(Vector3.right), Vector3.up);
    }

    [Test]
    public void Reset_PreviousStateExists_ClearsStableGraceAndSuspectState()
    {
        var previousUp = CreateTiltedUp(30f, Vector3.right);
        var launchUp = new Vector3(1f, 1f, 0f).normalized;
        ResetAndSampleGround(previousUp);
        _surfaceContextSource.Current = Ungrounded();
        _fixedTickable.FixedTick();
        _surfaceContextSource.Current = Grounded(CreateTiltedUp(80f, Vector3.forward));
        _fixedTickable.FixedTick();

        _resetter.Reset(launchUp);

        AssertVectorNear(_source.GetUpDirection(Vector3.up), launchUp);
    }

    [Test]
    public void FixedTick_GroundedValidSupportAfterReset_BecomesSteeringFrame()
    {
        var groundNormal = new Vector3(0f, 2f, 1f).normalized;
        _resetter.Reset(Vector3.up);
        _surfaceContextSource.Current = Grounded(groundNormal);

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.up), groundNormal);
        Assert.That(_surfaceContextSource.CurrentReadCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_SmallGroundedNormalChange_SlewsByConfiguredDegreesPerSecond()
    {
        var targetUp = CreateTiltedUp(30f, Vector3.right);
        _clock.FixedDeltaTime = 0.1f;
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Grounded(targetUp);

        _fixedTickable.FixedTick();

        var upDirection = _source.GetUpDirection(Vector3.forward);
        Assert.That(Vector3.Angle(Vector3.up, upDirection), Is.EqualTo(9f).Within(0.1f));
        Assert.That(Vector3.Angle(upDirection, targetUp), Is.EqualTo(21f).Within(0.1f));
    }

    [Test]
    public void FixedTick_LargerFixedDeltaTime_AdvancesStableFrameFarther()
    {
        var targetUp = CreateTiltedUp(30f, Vector3.right);
        var smallDeltaAngle = SampleSlewAngleAfterDelta(targetUp, 0.02f);
        var largeDeltaAngle = SampleSlewAngleAfterDelta(targetUp, 0.1f);

        Assert.That(largeDeltaAngle, Is.GreaterThan(smallDeltaAngle));
    }

    [Test]
    public void FixedTick_SlewWouldPassTarget_ClampsToTargetWithoutOvershoot()
    {
        var targetUp = CreateTiltedUp(10f, Vector3.right);
        _clock.FixedDeltaTime = 1f;
        _config.RunSteeringFrameNormalSlewDegreesPerSecond = 1000f;
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Grounded(targetUp);

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), targetUp);
    }

    [Test]
    public void FixedTick_GroundedInvalidNormal_KeepsLastStableFrame()
    {
        var stableUp = CreateTiltedUp(20f, Vector3.right);
        ResetAndSampleGround(stableUp);
        _surfaceContextSource.Current = Grounded(Vector3.zero);

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), stableUp);
    }

    [Test]
    public void GetUpDirection_MultipleReadsInsideFixedTick_ReturnSameStableFrame()
    {
        var targetUp = CreateTiltedUp(30f, Vector3.right);
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Grounded(targetUp);
        _fixedTickable.FixedTick();

        var firstRead = _source.GetUpDirection(Vector3.forward);
        var secondRead = _source.GetUpDirection(Vector3.left);

        AssertVectorNear(firstRead, secondRead);
        Assert.That(_surfaceContextSource.CurrentReadCount, Is.EqualTo(2));
    }

    [Test]
    public void FixedTick_BriefUngroundedSample_KeepsLastStableFrame()
    {
        var stableUp = CreateTiltedUp(20f, Vector3.right);
        ResetAndSampleGround(stableUp);
        _surfaceContextSource.Current = Ungrounded();

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), stableUp);
    }

    [Test]
    public void FixedTick_UngroundedGraceExpires_ReturnsFallback()
    {
        var stableUp = CreateTiltedUp(20f, Vector3.right);
        var fallbackUp = new Vector3(1f, 1f, 0f).normalized;
        ResetAndSampleGround(stableUp);
        _surfaceContextSource.Current = Ungrounded();

        Tick(5);

        AssertVectorNear(_source.GetUpDirection(fallbackUp), fallbackUp);
    }

    [Test]
    public void FixedTick_RecontactAfterGraceExpiry_ReinitializesInsteadOfSlewingFromStaleFrame()
    {
        var recontactUp = CreateTiltedUp(45f, Vector3.right);
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Ungrounded();
        Tick(5);
        _surfaceContextSource.Current = Grounded(recontactUp);

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), recontactUp);
    }

    [Test]
    public void FixedTick_OneFrameLargeNormalSpike_IsIgnored()
    {
        var spikeUp = CreateTiltedUp(80f, Vector3.right);
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Grounded(spikeUp);

        _fixedTickable.FixedTick();
        AssertVectorNear(_source.GetUpDirection(Vector3.forward), Vector3.up);

        _surfaceContextSource.Current = Grounded(Vector3.up);
        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), Vector3.up);
    }

    [Test]
    public void FixedTick_PersistentLargeNormalJump_ConfirmsAndSnaps()
    {
        var confirmedUp = CreateTiltedUp(80f, Vector3.right);
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Grounded(confirmedUp);

        _fixedTickable.FixedTick();
        AssertVectorNear(_source.GetUpDirection(Vector3.forward), Vector3.up);

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), confirmedUp);
    }

    [Test]
    public void FixedTick_SuspectCandidateChangesBeforeConfirmation_RestartsConfirmation()
    {
        var firstCandidate = CreateTiltedUp(80f, Vector3.right);
        var secondCandidate = CreateTiltedUp(80f, Vector3.forward);
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Grounded(firstCandidate);
        _fixedTickable.FixedTick();
        _surfaceContextSource.Current = Grounded(secondCandidate);

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), Vector3.up);

        _fixedTickable.FixedTick();

        AssertVectorNear(_source.GetUpDirection(Vector3.forward), secondCandidate);
    }

    private void ResetAndSampleGround(Vector3 groundNormal)
    {
        _resetter.Reset(Vector3.up);
        _surfaceContextSource.Current = Grounded(groundNormal);
        _fixedTickable.FixedTick();
    }

    private void Tick(int count)
    {
        for (var tickIndex = 0; tickIndex < count; tickIndex += 1)
        {
            _fixedTickable.FixedTick();
        }
    }

    private float SampleSlewAngleAfterDelta(Vector3 targetUp, float fixedDeltaTime)
    {
        _clock.FixedDeltaTime = fixedDeltaTime;
        _config.RunSteeringFrameNormalSlewDegreesPerSecond = 90f;
        ResetAndSampleGround(Vector3.up);
        _surfaceContextSource.Current = Grounded(targetUp);
        _fixedTickable.FixedTick();

        return Vector3.Angle(Vector3.up, _source.GetUpDirection(Vector3.forward));
    }

    private static RunSurfaceContext Grounded(Vector3 groundNormal)
    {
        return new RunSurfaceContext(true, groundNormal, 0f);
    }

    private static RunSurfaceContext Ungrounded()
    {
        return new RunSurfaceContext(false, Vector3.up, 0f);
    }

    private static Vector3 CreateTiltedUp(float degrees, Vector3 axis)
    {
        return Quaternion.AngleAxis(degrees, axis) * Vector3.up;
    }

    private static void AssertVectorNear(Vector3 actual, Vector3 expected, float toleranceDegrees = 0.1f)
    {
        Assert.That(Vector3.Angle(actual, expected), Is.LessThanOrEqualTo(toleranceDegrees));
    }

    private sealed class FakeRunSurfaceContextSource : IRunSurfaceContextSource
    {
        private RunSurfaceContext _current;

        public int CurrentReadCount { get; private set; }

        public RunSurfaceContext Current
        {
            get
            {
                CurrentReadCount += 1;
                return _current;
            }
            set => _current = value;
        }
    }

    private sealed class FakePlayerSteeringConfig : IPlayerSteeringConfig
    {
        public float RunSteeringRangeCentimeters { get; set; } = 1.5f;
        public float RunSteeringDeadzoneFraction { get; set; } = 0.15f;
        public float RunSteeringResponsiveness { get; set; } = 8f;
        public float FallbackDpi { get; set; } = 326f;
        public float MinimumAcceptedDpi { get; set; } = 1f;
        public float MaximumAcceptedDpi { get; set; } = 1000f;
        public float MaximumTurnDegreesPerSecond { get; set; } = 120f;
        public float MinimumSteerSpeed { get; set; } = 0.25f;
        public float RunBodySpeedSanityGuardMetersPerSecond { get; set; } = 250f;
        public float LaunchLandingStabilizationSeconds { get; set; } = 0.3f;
        public float LaunchLandingMaximumLiftSpeed { get; set; } = 0f;
        public float RunSteeringFrameNormalSlewDegreesPerSecond { get; set; }
        public float RunSteeringFrameSnapDegrees { get; set; }
        public float RunSteeringFrameUngroundedGraceSeconds { get; set; }
        public float RunSteeringFrameSuspectNormalConfirmationSeconds { get; set; }

        public float ResolveRunSteeringDpi(float rawDpi)
        {
            return rawDpi;
        }

        public float ResolveRunSteeringRangePixels(float rawDpi)
        {
            return RunSteeringRangeCentimeters / 2.54f * rawDpi;
        }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
