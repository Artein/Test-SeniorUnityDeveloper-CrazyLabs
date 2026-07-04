using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PlayerSteeringControllerTests : PlayerSteeringControllerTestFixture
{
    [Test]
    public void BeforeLaunch_DoesNotEnableInputOrSteer()
    {
        _input.Press(1, new Vector2(1000f, 100f));

        FixedTick();

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
        Assert.That(_statResolver.ResolveRequests, Is.Empty);
    }

    [Test]
    public void LaunchApplied_WhenRunning_EnablesInput()
    {
        _stateService.ChangeTo(_runningStateId);

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.up));

        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    [Test]
    public void LaunchApplied_WhenRunning_ResetsSteeringFrameFromLaunchUp()
    {
        var launchUp = new Vector3(0f, 1f, 1f).normalized;
        _stateService.ChangeTo(_runningStateId);

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(launchUp));

        Assert.That(_steeringFrameSource.ResetCallCount, Is.EqualTo(1));
        AssertVectorEqual(_steeringFrameSource.LastResetLaunchUpDirection, launchUp);
    }

    [Test]
    public void LaunchApplied_BeforeRunning_DoesNotResetSteeringFrameUntilRunningActivates()
    {
        var launchUp = new Vector3(1f, 1f, 0f).normalized;

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(launchUp));

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        Assert.That(_steeringFrameSource.ResetCallCount, Is.Zero);

        _stateService.ChangeTo(_runningStateId);

        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
        Assert.That(_steeringFrameSource.ResetCallCount, Is.EqualTo(1));
        AssertVectorEqual(_steeringFrameSource.LastResetLaunchUpDirection, launchUp);
    }

    [Test]
    public void RunningEnteredBeforeLaunch_DoesNotResetSteeringFrame()
    {
        _stateService.ChangeTo(_runningStateId);

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        Assert.That(_steeringFrameSource.ResetCallCount, Is.Zero);
    }

    [Test]
    public void LeavingRunning_DisablesInputAndResetsPointerState()
    {
        ActivateSteering();
        _input.Press(1, new Vector2(1000f, 100f));

        _stateService.ChangeTo(_preLaunchStateId);
        _input.Move(1, new Vector2(0f, 100f));
        FixedTick();

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_LeftTouch_TurnsVelocityLeftAndPreservesSpeedComponents()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(400f, 100f));
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.LessThan(0f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_RightTouch_TurnsVelocityRightAndPreservesSpeedComponents()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(0f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_GroundedTiltedSteeringFrame_RotatesVelocityAroundSurfaceUp()
    {
        var surfaceUp = new Vector3(0f, 1f, 1f).normalized;
        var initialPlanarVelocity = ProjectPlanar(Vector3.forward, surfaceUp).normalized * DefaultPlanarSpeed;
        _steeringFrameSource.UpDirection = surfaceUp;
        ActivateSteering(surfaceUp);
        SetGroundedSurface(surfaceUp);
        _steeringTarget.LinearVelocity = initialPlanarVelocity;

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        var steeredPlanarVelocity = ProjectPlanar(_steeringTarget.LinearVelocity, surfaceUp);
        Assert.That(_steeringFrameSource.GetUpDirectionCallCount, Is.EqualTo(1));
        Assert.That(Vector3.Dot(steeredPlanarVelocity.normalized, initialPlanarVelocity.normalized), Is.LessThan(0.9999f));
        Assert.That(Vector3.Dot(_steeringTarget.LinearVelocity, surfaceUp), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(steeredPlanarVelocity.magnitude, Is.EqualTo(DefaultPlanarSpeed).Within(0.0001f));
        Assert.That(Vector3.Dot(_steeringTarget.Rotation * Vector3.up, surfaceUp), Is.GreaterThan(0.999f));
    }

    [Test]
    public void FixedTick_InvalidSteeringFrame_UsesLaunchUpFallback()
    {
        var launchUp = new Vector3(0f, 1f, 1f).normalized;
        var initialPlanarVelocity = ProjectPlanar(Vector3.forward, launchUp).normalized * DefaultPlanarSpeed;
        _steeringFrameSource.UpDirection = Vector3.zero;
        _steeringTarget.LinearVelocity = initialPlanarVelocity + launchUp * DefaultVerticalSpeed;
        ActivateSteering(launchUp);

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        Assert.That(_steeringFrameSource.GetUpDirectionCallCount, Is.EqualTo(1));
        AssertVectorEqual(_steeringFrameSource.LastFallbackUpDirection, launchUp);
        AssertSpeedComponentsPreservedAround(_steeringTarget.LinearVelocity, launchUp);
        Assert.That(Vector3.Dot(_steeringTarget.Rotation * Vector3.up, launchUp), Is.GreaterThan(0.999f));
    }

    [Test]
    public void FixedTick_NeutralMovementStats_ResolvesBaseValuesAndPreservesMovement()
    {
        ActivateSteering();
        var resolveRequestCountBeforeTick = _statResolver.ResolveRequests.Count;

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        AssertResolved(_playerSteeringResponsivenessStatId, 100f);
        Assert.That(_statResolver.ResolveRequests, Has.Count.EqualTo(resolveRequestCountBeforeTick + 1));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_NonLaunchHighSpeed_DoesNotClampPlanarSpeed()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 24f);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 24f);
    }

    [Test]
    public void FixedTick_AirWithoutActiveGesture_DoesNotWriteVelocityOrFacing()
    {
        SetUngroundedSurface();
        var launchVelocity = new Vector3(0f, DefaultVerticalSpeed, 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, launchVelocity);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_AirWithActiveGesture_SteersDirectionAndPreservesSpeedComponents()
    {
        SetUngroundedSurface();
        var launchVelocity = new Vector3(0f, DefaultVerticalSpeed, 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(800f, 100f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(0f));
        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 35f);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_AirWithActiveGesture_UsesAirTurnAuthority()
    {
        SetUngroundedSurface();
        var launchVelocity = new Vector3(0f, DefaultVerticalSpeed, DefaultPlanarSpeed);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));

        FixedTick();

        AssertPlanarTurnAngleAround(Vector3.forward, _steeringTarget.LinearVelocity, Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_GroundedWithActiveGesture_UsesGroundedTurnAuthority()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(0f, 0f, DefaultPlanarSpeed);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));

        FixedTick();

        AssertPlanarTurnAngleAround(Vector3.forward, _steeringTarget.LinearVelocity, Vector3.up,
            _config.MaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_PostLaunchStaleGroundedWithPositiveLift_UsesAirSteeringBeforeUnsupportedSample()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(0f, DefaultVerticalSpeed, 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 35f);

        AssertPlanarTurnAngleAround(Vector3.forward, _steeringTarget.LinearVelocity, Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_PostLaunchUngroundedThenGrounded_EnablesSteeringAndPreservesLandingSpeed()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(0f, DefaultVerticalSpeed, 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);

        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(Vector3.up);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(0f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 35f);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_PostLandingAfterLongTime_DoesNotDecayOrClampSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(0f, 0f, 35f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, 0f, 35f));
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(Vector3.up);
        FixedTick();

        _clock.FixedDeltaTime = 2f;
        _steeringTarget.LinearVelocity = new Vector3(0f, 0f, 35f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 35f);
    }

    [Test]
    public void FixedTick_NoTakeoffGroundedWithoutLift_UsesGroundedSteeringImmediately()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(0f, 0f, 12f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 12f);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(1));

        AssertPlanarTurnAngleAround(Vector3.forward, _steeringTarget.LinearVelocity, Vector3.up,
            _config.MaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_NoTakeoffGroundedWithPositiveLiftWithoutGesture_DoesNotApplyHiddenGuidance()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(0f, 2f, 12f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _clock.FixedDeltaTime = 5f;

        FixedTick();
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, launchVelocity);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_InvalidGroundedSurfaceWithActiveGesture_UsesAirSteering()
    {
        _surfaceContextSource.Current = new Game.Gameplay.RunSurfaceContext(true, Vector3.zero, 0f);
        var launchVelocity = new Vector3(0f, 0f, 12f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 12f);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(1));

        AssertPlanarTurnAngleAround(Vector3.forward, _steeringTarget.LinearVelocity, Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_NonLaunchUnsupportedRunningBump_UsesAirSteering()
    {
        ActivateSteering();
        SetUngroundedSurface();
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, DefaultPlanarSpeed);
        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, DefaultPlanarSpeed);

        AssertPlanarTurnAngleAround(Vector3.forward, _steeringTarget.LinearVelocity, Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_NonLaunchOverspeed_DoesNotClamp()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 24f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 24f);
    }

    [Test]
    public void FixedTick_AbsurdVelocity_ClampsToRunBodySanityGuard()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(0f, 0f, 500f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, _config.RunBodySpeedSanityGuardMetersPerSecond);
    }

    [Test]
    public void FixedTick_NonFiniteVelocity_AppliesZeroVelocityWithoutSteering()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(float.NaN, 0f, 10f);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, Vector3.zero);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(1));
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_PostLaunchLandingHighTangentSpeed_PreservesTangentSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(0f, 0f, 70f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, 0f, 70f));
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(Vector3.up);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 70f);
    }

    [Test]
    public void FixedTick_PostLaunchFlatSurfaceLanding_RemovesLiftAndPreservesTangentSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(3f, 4f, 12f);
        ActivateSteeringWithLaunchVelocity(new Vector3(3f, 4f, 12f));
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(Vector3.up);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(0f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, new Vector3(3f, 0f, 12f).magnitude);
    }

    [Test]
    public void FixedTick_PostLaunchStaleGroundedBeforeTakeoff_PreservesLiftVelocity()
    {
        SetGroundedSurface(Vector3.up);
        var expectedVelocity = new Vector3(3f, 4f, 12f);
        _steeringTarget.LinearVelocity = expectedVelocity;
        ActivateSteeringWithLaunchVelocity(expectedVelocity);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, expectedVelocity);
    }

    [Test]
    public void FixedTick_PostLaunchLandingBelowMinimumSteerSpeed_SuppressesLiftWithoutSteeringRotation()
    {
        SetGroundedSurface(Vector3.up);
        var lowTangentLiftVelocity = new Vector3(0f, 3f, 0.1f);
        _steeringTarget.LinearVelocity = lowTangentLiftVelocity;
        ActivateSteeringWithLaunchVelocity(lowTangentLiftVelocity);
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(Vector3.up);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(0f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 0.1f);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(1));
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
        Assert.That(_steeringTarget.Rotation, Is.EqualTo(Quaternion.identity));
    }

    [Test]
    public void FixedTick_PostLaunchTiltedSurfaceLanding_UsesRunSurfaceNormal()
    {
        var groundNormal = new Vector3(0f, 1f, 1f).normalized;
        var tangentVelocity = Vector3.ProjectOnPlane(Vector3.forward, groundNormal).normalized * 4f;
        var liftVelocity = groundNormal * 2f;
        SetGroundedSurface(groundNormal);
        _steeringTarget.LinearVelocity = tangentVelocity + liftVelocity;
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(groundNormal);

        FixedTick();

        Assert.That(Vector3.Dot(_steeringTarget.LinearVelocity, groundNormal), Is.EqualTo(0f).Within(0.0001f));
        AssertVectorEqual(_steeringTarget.LinearVelocity, tangentVelocity);
    }

    [Test]
    public void FixedTick_PostLaunchDownwardSurfaceVelocity_DoesNotModifyNormalVelocity()
    {
        var groundNormal = Vector3.up;
        var expectedVelocity = new Vector3(0f, -2f, 8f);
        SetGroundedSurface(groundNormal);
        _steeringTarget.LinearVelocity = expectedVelocity;
        ActivateSteeringWithLaunchVelocity(expectedVelocity);
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(groundNormal);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, expectedVelocity);
    }

    [Test]
    public void FixedTick_PostLaunchUngrounded_DoesNotModifyLiftVelocity()
    {
        SetUngroundedSurface();
        var expectedVelocity = new Vector3(0f, 4f, 8f);
        _steeringTarget.LinearVelocity = expectedVelocity;
        ActivateSteeringWithLaunchVelocity(expectedVelocity);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, expectedVelocity);
    }

    [Test]
    public void FixedTick_GroundedWithoutLaunch_DoesNotSuppressLiftVelocity()
    {
        SetGroundedSurface(Vector3.up);
        var expectedVelocity = new Vector3(0f, 4f, 8f);
        _stateService.ChangeTo(_runningStateId);
        _steeringTarget.LinearVelocity = expectedVelocity;

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, expectedVelocity);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_AfterLaunchLandingStabilizationWindow_DoesNotSuppressLiftVelocity()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(0f, 4f, 8f);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(Vector3.up);
        FixedTick();

        _clock.FixedDeltaTime = _config.LaunchLandingStabilizationSeconds + 0.01f;
        _steeringTarget.LinearVelocity = new Vector3(0f, 4f, 8f);
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(4f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 8f);
    }

    [Test]
    public void LeavingRunning_ClearsLaunchLandingStabilization()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(0f, 4f, 8f);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        var applyCallCountBeforeLeaving = _steeringTarget.ApplyCallCount;
        var applyVelocityCallCountBeforeLeaving = _steeringTarget.ApplyVelocityCallCount;

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);
        _steeringTarget.LinearVelocity = new Vector3(0f, 4f, 8f);
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(applyCallCountBeforeLeaving));
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(applyVelocityCallCountBeforeLeaving));
    }

    [Test]
    public void FixedTick_PostLaunchLandingHighTangentSpeed_RemovesLiftAndPreservesTangentSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(0f, 5f, 60f);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface();
        FixedTick();

        SetGroundedSurface(Vector3.up);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(0f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 60f);
    }

    [Test]
    public void FixedTick_PlayerSteeringResponsivenessModifier_IncreasesSteeringResponse()
    {
        _config.RunSteeringResponsiveness = 5f;
        _statResolver.SetResolvedValue(_playerSteeringResponsivenessStatId, 20f);
        ActivateSteering();
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = Vector3.forward * DefaultPlanarSpeed;

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        AssertResolved(_playerSteeringResponsivenessStatId, 5f);
        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(0.1f));
        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(0f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, DefaultPlanarSpeed);
    }

    [Test]
    public void FixedTick_PressWithoutHorizontalMovement_DoesNotChangeVelocity()
    {
        ActivateSteering();
        var initialVelocity = _steeringTarget.LinearVelocity;

        _input.Press(1, new Vector2(1000f, 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, initialVelocity);
    }

    [Test]
    public void PointerReleased_ReturnsDesiredSteeringToZero()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();
        var steeredVelocity = _steeringTarget.LinearVelocity;

        _input.Release(1, new Vector2(1000f, 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, steeredVelocity);
    }

    [Test]
    public void PointerCanceled_ReturnsDesiredSteeringToZero()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(400f, 100f));
        FixedTick();
        var steeredVelocity = _steeringTarget.LinearVelocity;

        _input.Cancel(1, new Vector2(0f, 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, steeredVelocity);
    }

    [Test]
    public void NonActivePointerMoveAndRelease_AreIgnored()
    {
        ActivateSteering();
        var initialVelocity = _steeringTarget.LinearVelocity;

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(2, new Vector2(1000f, 100f));
        _input.Release(2, new Vector2(1000f, 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, initialVelocity);
    }

    [Test]
    public void PointerPressed_DuringRunning_UsesScreenDpiForCapturedRange()
    {
        _screen.Dpi = 96f;
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));

        Assert.That(_config.RangePixelRawDpiRequests, Contains.Item(96f));
    }

    [Test]
    public void FixedTick_LowResponsiveness_SmoothsRequestedSteering()
    {
        _config.RunSteeringResponsiveness = 5f;
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(0f));
        Assert.That(_steeringTarget.LinearVelocity.x, Is.LessThan(0.1f));
    }

    [Test]
    public void FixedTick_BelowMinimumSteerSpeed_DoesNotApplySteering()
    {
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 0.1f);
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    private void AssertPlanarTurnAngleAround(
        Vector3 originalPlanarDirection,
        Vector3 steeredVelocity,
        Vector3 upDirection,
        float expectedDegrees)
    {
        var originalPlanar = ProjectPlanar(originalPlanarDirection, upDirection).normalized;
        var steeredPlanar = ProjectPlanar(steeredVelocity, upDirection).normalized;

        Assert.That(Vector3.Angle(originalPlanar, steeredPlanar), Is.EqualTo(expectedDegrees).Within(0.001f));
    }
}
