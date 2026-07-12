using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunBodyMovementControllerBehaviorTests : RunBodyMovementControllerBehaviorTestFixture
{
    [Test]
    public void BeforeLaunch_DoesNotEnableInputOrSteer()
    {
        _input.Press(pointerId: 1, new Vector2(x: 1000f, y: 100f));

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

        Assert.That(_input.ActiveHandleCount, Is.EqualTo(expected: 1));
    }

    [Test]
    public void LaunchApplied_WhenRunning_ResetsSteeringFrameFromLaunchUp()
    {
        var launchUp = new Vector3(x: 0f, y: 1f, z: 1f).normalized;
        _stateService.ChangeTo(_runningStateId);

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(launchUp));

        Assert.That(_steeringFrameSource.ResetCallCount, Is.EqualTo(expected: 1));
        AssertVectorEqual(_steeringFrameSource.LastResetLaunchUpDirection, launchUp);
    }

    [Test]
    public void LaunchApplied_BeforeRunning_DoesNotResetSteeringFrameUntilRunningActivates()
    {
        var launchUp = new Vector3(x: 1f, y: 1f, z: 0f).normalized;

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(launchUp));

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        Assert.That(_steeringFrameSource.ResetCallCount, Is.Zero);

        _stateService.ChangeTo(_runningStateId);

        Assert.That(_input.ActiveHandleCount, Is.EqualTo(expected: 1));
        Assert.That(_steeringFrameSource.ResetCallCount, Is.EqualTo(expected: 1));
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
        _input.Press(pointerId: 1, new Vector2(x: 1000f, y: 100f));

        _stateService.ChangeTo(_preLaunchStateId);
        _input.Move(pointerId: 1, new Vector2(x: 0f, y: 100f));
        FixedTick();

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void LeavingRunning_WithActiveGesture_ResetsRunSteeringAffordanceImmediately()
    {
        ActivateSteering();
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 620f, y: 900f));
        var resetCallCountBeforeLeavingRunning = _runSteeringAffordancePresenter.ResetCallCount;

        _stateService.ChangeTo(_preLaunchStateId);

        Assert.That(_runSteeringAffordancePresenter.ResetCallCount, Is.EqualTo(resetCallCountBeforeLeavingRunning + 1));
        Assert.That(_runSteeringAffordancePresenter.HideStates, Is.Empty);
    }

    [Test]
    public void FixedTick_LeftTouch_TurnsVelocityLeftAndPreservesSpeedComponents()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 400f, y: 100f));
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.LessThan(expected: 0f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_RightTouch_TurnsVelocityRightAndPreservesSpeedComponents()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(expected: 0f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_GroundedTiltedSteeringFrame_RotatesVelocityAroundSurfaceUp()
    {
        var surfaceUp = new Vector3(x: 0f, y: 1f, z: 1f).normalized;
        var initialPlanarVelocity = ProjectPlanar(Vector3.forward, surfaceUp).normalized * DefaultPlanarSpeed;
        _steeringFrameSource.UpDirection = surfaceUp;
        ActivateSteering(surfaceUp);
        SetGroundedSurface(surfaceUp);
        _steeringTarget.LinearVelocity = initialPlanarVelocity;

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        var steeredPlanarVelocity = ProjectPlanar(_steeringTarget.LinearVelocity, surfaceUp);
        Assert.That(_steeringFrameSource.GetUpDirectionCallCount, Is.EqualTo(expected: 1));
        Assert.That(Vector3.Dot(steeredPlanarVelocity.normalized, initialPlanarVelocity.normalized), Is.LessThan(expected: 0.9999f));
        Assert.That(Vector3.Dot(_steeringTarget.LinearVelocity, surfaceUp), Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        Assert.That(steeredPlanarVelocity.magnitude, Is.EqualTo(DefaultPlanarSpeed).Within(amount: 0.0001f));
        Assert.That(Vector3.Dot(_steeringTarget.Rotation * Vector3.up, surfaceUp), Is.GreaterThan(expected: 0.999f));
    }

    [Test]
    public void FixedTick_GroundedNeutralInputWithLaggingSteeringFrame_PreservesPhysicalVelocity()
    {
        var groundNormal = new Vector3(x: 0f, y: 1f, z: 1f).normalized;
        var tangentDirection = (Vector3.right + Vector3.ProjectOnPlane(Vector3.forward, groundNormal)).normalized;
        var tangentVelocity = tangentDirection * DefaultPlanarSpeed;
        var normalVelocity = groundNormal * -DefaultVerticalSpeed;
        var expectedVelocity = tangentVelocity + normalVelocity;
        ActivateSteering(Vector3.up);
        _steeringFrameSource.UpDirection = Vector3.up;
        SetGroundedSurface(groundNormal);
        _steeringTarget.LinearVelocity = expectedVelocity;

        FixedTick();

        var actualVelocity = _steeringTarget.LinearVelocity;
        var actualTangentVelocity = Vector3.ProjectOnPlane(actualVelocity, groundNormal);
        Assert.That(actualTangentVelocity.magnitude, Is.EqualTo(DefaultPlanarSpeed).Within(amount: 0.0001f));
        Assert.That(Vector3.Dot(actualVelocity, groundNormal), Is.EqualTo(-DefaultVerticalSpeed).Within(amount: 0.0001f));
        AssertVectorEqual(actualTangentVelocity.normalized, tangentDirection);
        AssertVectorEqual(actualVelocity, expectedVelocity);
    }

    [Test]
    public void FixedTick_InvalidSteeringFrame_UsesLaunchUpFallback()
    {
        var launchUp = new Vector3(x: 0f, y: 1f, z: 1f).normalized;
        var initialPlanarVelocity = ProjectPlanar(Vector3.forward, launchUp).normalized * DefaultPlanarSpeed;
        _steeringFrameSource.UpDirection = Vector3.zero;
        _steeringTarget.LinearVelocity = initialPlanarVelocity + launchUp * DefaultVerticalSpeed;
        ActivateSteering(launchUp);

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        Assert.That(_steeringFrameSource.GetUpDirectionCallCount, Is.EqualTo(expected: 1));
        AssertVectorEqual(_steeringFrameSource.LastFallbackUpDirection, launchUp);
        AssertSpeedComponentsPreservedAround(_steeringTarget.LinearVelocity, launchUp);
        Assert.That(Vector3.Dot(_steeringTarget.Rotation * Vector3.up, launchUp), Is.GreaterThan(expected: 0.999f));
    }

    [Test]
    public void FixedTick_NeutralMovementStats_ResolvesBaseValuesAndPreservesMovement()
    {
        ActivateSteering();
        var resolveRequestCountBeforeTick = _statResolver.ResolveRequests.Count;

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        AssertResolved(_playerSteeringResponsivenessStatId, baseValue: 100f);
        AssertResolved(_playerMaxSpeedStatId, _config.BaseSoftMaximumSpeed);
        Assert.That(_statResolver.ResolveRequests, Has.Count.EqualTo(resolveRequestCountBeforeTick + 2));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_NonLaunchHighSpeed_DoesNotClampPlanarSpeed()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, z: 24f);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 24f);
    }

    [Test]
    public void FixedTick_AirWithoutActiveGesture_WritesUnchangedVelocityWithoutFacing()
    {
        SetUngroundedSurface();
        var launchVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, z: 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, launchVelocity);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(expected: 1));
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_AirWithActiveGesture_SteersDirectionAndPreservesSpeedComponents()
    {
        SetUngroundedSurface();
        var launchVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, z: 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 800f, y: 100f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(expected: 0f));
        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 35f);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(expected: 1));
    }

    [Test]
    public void FixedTick_AirWithActiveGesture_UsesAirTurnAuthority()
    {
        SetUngroundedSurface();
        var launchVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, DefaultPlanarSpeed);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));

        FixedTick();

        AssertPlanarTurnAngleAround(
            Vector3.forward,
            _steeringTarget.LinearVelocity,
            Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_GroundedWithActiveGesture_UsesGroundedTurnAuthority()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 0f, DefaultPlanarSpeed);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));

        FixedTick();

        AssertPlanarTurnAngleAround(
            Vector3.forward,
            _steeringTarget.LinearVelocity,
            Vector3.up,
            _config.MaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_PostLaunchStaleGroundedWithPositiveLift_UsesAirSteeringBeforeUnsupportedSample()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, z: 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 35f);

        AssertPlanarTurnAngleAround(
            Vector3.forward,
            _steeringTarget.LinearVelocity,
            Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_PostLaunchUngroundedThenGrounded_EnablesSteeringAndPreservesLandingSpeed()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, z: 35f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);

        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(Vector3.up, RunSurfaceTransition.SupportAcquired);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 35f);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(expected: 1));
    }

    [Test]
    public void FixedTick_PostLandingAfterLongTime_DoesNotDecayOrClampSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 0f, z: 35f);
        ActivateSteeringWithLaunchVelocity(new Vector3(x: 0f, y: 0f, z: 35f));
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(Vector3.up, RunSurfaceTransition.SupportAcquired);
        FixedTick();

        _clock.FixedDeltaTime = 2f;
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 0f, z: 35f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 35f);
    }

    [Test]
    public void FixedTick_NoTakeoffGroundedWithoutLift_UsesGroundedSteeringImmediately()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(x: 0f, y: 0f, z: 12f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 12f);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(expected: 1));

        AssertPlanarTurnAngleAround(
            Vector3.forward,
            _steeringTarget.LinearVelocity,
            Vector3.up,
            _config.MaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_NoTakeoffGroundedWithPositiveLiftWithoutGesture_WritesUnchangedVelocityWithoutFacing()
    {
        SetGroundedSurface(Vector3.up);
        var launchVelocity = new Vector3(x: 0f, y: 2f, z: 12f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _clock.FixedDeltaTime = 5f;

        FixedTick();
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, launchVelocity);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(expected: 2));
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_InvalidGroundedSurfaceWithActiveGesture_UsesAirSteering()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(isGrounded: true, Vector3.zero, forwardDownhillDegrees: 0f);
        var launchVelocity = new Vector3(x: 0f, y: 0f, z: 12f);
        _steeringTarget.LinearVelocity = launchVelocity;
        ActivateSteeringWithLaunchVelocity(launchVelocity);
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 12f);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(expected: 1));

        AssertPlanarTurnAngleAround(
            Vector3.forward,
            _steeringTarget.LinearVelocity,
            Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_NonLaunchUnsupportedRunningBump_UsesAirSteering()
    {
        ActivateSteering();
        SetUngroundedSurface();
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, DefaultPlanarSpeed);
        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, DefaultPlanarSpeed);

        AssertPlanarTurnAngleAround(
            Vector3.forward,
            _steeringTarget.LinearVelocity,
            Vector3.up,
            _config.RunAirSteeringMaximumTurnDegreesPerSecond * _clock.FixedDeltaTime);
    }

    [Test]
    public void FixedTick_NonLaunchOverspeed_DoesNotClamp()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, z: 24f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 24f);
    }

    [Test]
    public void FixedTick_AbsurdVelocity_ClampsToRunBodySanityGuard()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 0f, z: 500f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, _config.RunBodySpeedSanityGuardMetersPerSecond);
    }

    [Test]
    public void FixedTick_NonFiniteVelocity_AppliesZeroVelocityWithoutSteering()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(float.NaN, y: 0f, z: 10f);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, Vector3.zero);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(expected: 1));
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_PostLaunchLandingHighTangentSpeed_PreservesTangentSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 0f, z: 70f);
        ActivateSteeringWithLaunchVelocity(new Vector3(x: 0f, y: 0f, z: 70f));
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(Vector3.up, RunSurfaceTransition.SupportAcquired);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 70f);
    }

    [Test]
    public void FixedTick_PostLaunchFlatSurfaceLanding_RemovesLiftAndPreservesTangentSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(x: 3f, y: 4f, z: 12f);
        ActivateSteeringWithLaunchVelocity(new Vector3(x: 3f, y: 4f, z: 12f));
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(Vector3.up, RunSurfaceTransition.SupportAcquired);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, new Vector3(x: 3f, y: 0f, z: 12f).magnitude);
    }

    [Test]
    public void FixedTick_PostLaunchStaleGroundedBeforeTakeoff_PreservesLiftVelocity()
    {
        SetGroundedSurface(Vector3.up);
        var expectedVelocity = new Vector3(x: 3f, y: 4f, z: 12f);
        _steeringTarget.LinearVelocity = expectedVelocity;
        ActivateSteeringWithLaunchVelocity(expectedVelocity);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, expectedVelocity);
    }

    [Test]
    public void FixedTick_PostLaunchLandingBelowMinimumSteerSpeed_SuppressesLiftWithoutSteeringRotation()
    {
        SetGroundedSurface(Vector3.up);
        var lowTangentLiftVelocity = new Vector3(x: 0f, y: 3f, z: 0.1f);
        _steeringTarget.LinearVelocity = lowTangentLiftVelocity;
        ActivateSteeringWithLaunchVelocity(lowTangentLiftVelocity);
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(Vector3.up, RunSurfaceTransition.SupportAcquired);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 0.1f);
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(expected: 3));
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
        Assert.That(_steeringTarget.Rotation, Is.EqualTo(Quaternion.identity));
    }

    [Test]
    public void FixedTick_PostLaunchTiltedSurfaceLanding_UsesRunSurfaceNormal()
    {
        var groundNormal = new Vector3(x: 0f, y: 1f, z: 1f).normalized;
        var tangentVelocity = Vector3.ProjectOnPlane(Vector3.forward, groundNormal).normalized * 4f;
        var liftVelocity = groundNormal * 2f;
        SetGroundedSurface(groundNormal);
        _steeringTarget.LinearVelocity = tangentVelocity + liftVelocity;
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(groundNormal, RunSurfaceTransition.SupportAcquired);

        FixedTick();

        Assert.That(Vector3.Dot(_steeringTarget.LinearVelocity, groundNormal), Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        AssertVectorEqual(_steeringTarget.LinearVelocity, tangentVelocity);
    }

    [Test]
    public void FixedTick_PostLaunchDownwardSurfaceVelocity_DoesNotModifyNormalVelocity()
    {
        var groundNormal = Vector3.up;
        var expectedVelocity = new Vector3(x: 0f, y: -2f, z: 8f);
        SetGroundedSurface(groundNormal);
        _steeringTarget.LinearVelocity = expectedVelocity;
        ActivateSteeringWithLaunchVelocity(expectedVelocity);
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(groundNormal, RunSurfaceTransition.SupportAcquired);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, expectedVelocity);
    }

    [Test]
    public void FixedTick_PostLaunchUngrounded_DoesNotModifyLiftVelocity()
    {
        SetUngroundedSurface();
        var expectedVelocity = new Vector3(x: 0f, y: 4f, z: 8f);
        _steeringTarget.LinearVelocity = expectedVelocity;
        ActivateSteeringWithLaunchVelocity(expectedVelocity);

        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, expectedVelocity);
    }

    [Test]
    public void FixedTick_GroundedWithoutLaunch_DoesNotSuppressLiftVelocity()
    {
        SetGroundedSurface(Vector3.up);
        var expectedVelocity = new Vector3(x: 0f, y: 4f, z: 8f);
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
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 4f, z: 8f);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(Vector3.up, RunSurfaceTransition.SupportAcquired);
        FixedTick();

        _clock.FixedDeltaTime = _config.LaunchLandingStabilizationSeconds + 0.01f;
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 4f, z: 8f);
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(expected: 4f).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 8f);
    }

    [Test]
    public void LeavingRunning_ClearsLaunchLandingStabilization()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 4f, z: 8f);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        var applyCallCountBeforeLeaving = _steeringTarget.ApplyCallCount;
        var applyVelocityCallCountBeforeLeaving = _steeringTarget.ApplyVelocityCallCount;

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 4f, z: 8f);
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(expected: 4f).Within(amount: 0.0001f));
        Assert.That(_steeringTarget.ApplyCallCount, Is.EqualTo(applyCallCountBeforeLeaving));
        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(applyVelocityCallCountBeforeLeaving));
    }

    [Test]
    public void FixedTick_PostLaunchLandingHighTangentSpeed_RemovesLiftAndPreservesTangentSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, y: 5f, z: 60f);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);
        FixedTick();

        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();

        SetGroundedSurface(Vector3.up, RunSurfaceTransition.SupportAcquired);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, expectedPlanarSpeed: 60f);
    }

    [Test]
    public void FixedTick_PlayerSteeringResponsivenessModifier_IncreasesSteeringResponse()
    {
        _config.RunSteeringResponsiveness = 5f;
        _statResolver.SetResolvedValue(_playerSteeringResponsivenessStatId, resolvedValue: 20f);
        ActivateSteering();
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = Vector3.forward * DefaultPlanarSpeed;

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        AssertResolved(_playerSteeringResponsivenessStatId, baseValue: 5f);
        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(expected: 0.1f));
        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, DefaultPlanarSpeed);
    }

    [Test]
    public void FixedTick_PressWithoutHorizontalMovement_DoesNotChangeVelocity()
    {
        ActivateSteering();
        var initialVelocity = _steeringTarget.LinearVelocity;

        _input.Press(pointerId: 1, new Vector2(x: 1000f, y: 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, initialVelocity);
    }

    [Test]
    public void PointerReleased_ReturnsDesiredSteeringToZero()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();
        var steeredVelocity = _steeringTarget.LinearVelocity;

        _input.Release(pointerId: 1, new Vector2(x: 1000f, y: 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, steeredVelocity);
    }

    [Test]
    public void PointerCanceled_ReturnsDesiredSteeringToZero()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 400f, y: 100f));
        FixedTick();
        var steeredVelocity = _steeringTarget.LinearVelocity;

        _input.Cancel(pointerId: 1, new Vector2(x: 0f, y: 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, steeredVelocity);
    }

    [Test]
    public void NonActivePointerMoveAndRelease_AreIgnored()
    {
        ActivateSteering();
        var initialVelocity = _steeringTarget.LinearVelocity;

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 2, new Vector2(x: 1000f, y: 100f));
        _input.Release(pointerId: 2, new Vector2(x: 1000f, y: 100f));
        FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, initialVelocity);
    }

    [Test]
    public void PointerPressed_DuringRunning_UsesScreenDpiForCapturedRange()
    {
        _screen.Dpi = 96f;
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));

        Assert.That(_inputMetricsResolver.RawDpiRequests, Contains.Item(expected: 96f));
    }

    [Test]
    public void PointerPressed_DuringRunning_PresentsLayoutStateFromGestureSnapshot()
    {
        _screen.Dpi = 100f;
        _config.RunSteeringDeadzoneFraction = 0.25f;
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));

        Assert.That(_runSteeringPointerPressGuard.Requests, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordanceLayout.Snapshots, Has.Count.EqualTo(expected: 1));
        var snapshot = _runSteeringAffordanceLayout.Snapshots[index: 0];
        Assert.That(snapshot.IsActive, Is.True);
        Assert.That(snapshot.PointerId, Is.EqualTo(expected: 1));
        AssertVector2(snapshot.OriginScreenPosition, new Vector2(x: 500f, y: 100f));
        AssertVector2(snapshot.CurrentScreenPosition, new Vector2(x: 500f, y: 100f));
        Assert.That(snapshot.CapturedRangePixels, Is.EqualTo(expected: 100f).Within(amount: 0.0001f));
        Assert.That(snapshot.CapturedDeadzoneFraction, Is.EqualTo(expected: 0.25f).Within(amount: 0.0001f));
        Assert.That(_runSteeringAffordancePresenter.ShowStates, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordancePresenter.ShowStates[index: 0], Is.EqualTo(_runSteeringAffordanceLayout.Result));
    }

    [Test]
    public void PointerMoved_ActivePointer_UpdatesRunSteeringAffordanceImmediately()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 650f, y: 900f));

        Assert.That(_runSteeringAffordanceLayout.Snapshots, Has.Count.EqualTo(expected: 2));
        AssertVector2(_runSteeringAffordanceLayout.Snapshots[index: 1].CurrentScreenPosition, new Vector2(x: 650f, y: 900f));
        Assert.That(_runSteeringAffordancePresenter.UpdateStates, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordancePresenter.UpdateStates[index: 0], Is.EqualTo(_runSteeringAffordanceLayout.Result));
    }

    [Test]
    public void PointerReleased_ActivePointer_HidesRunSteeringAffordanceFromReleasePosition()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 540f, y: 100f));
        _input.Release(pointerId: 1, new Vector2(x: 650f, y: 900f));

        Assert.That(_runSteeringAffordanceLayout.Snapshots, Has.Count.EqualTo(expected: 3));
        AssertVector2(_runSteeringAffordanceLayout.Snapshots[index: 2].CurrentScreenPosition, new Vector2(x: 650f, y: 900f));
        Assert.That(_runSteeringAffordancePresenter.HideStates, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordancePresenter.HideStates[index: 0], Is.EqualTo(_runSteeringAffordanceLayout.Result));
    }

    [Test]
    public void PointerCanceled_ActivePointer_HidesRunSteeringAffordanceFromCancelPosition()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 540f, y: 100f));
        _input.Cancel(pointerId: 1, new Vector2(x: 350f, y: -200f));

        Assert.That(_runSteeringAffordanceLayout.Snapshots, Has.Count.EqualTo(expected: 3));
        AssertVector2(_runSteeringAffordanceLayout.Snapshots[index: 2].CurrentScreenPosition, new Vector2(x: 350f, y: -200f));
        Assert.That(_runSteeringAffordancePresenter.HideStates, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordancePresenter.HideStates[index: 0], Is.EqualTo(_runSteeringAffordanceLayout.Result));
    }

    [Test]
    public void PointerPressed_WhenPressGuardRejects_DoesNotBeginGestureOrShowAffordance()
    {
        _runSteeringPointerPressGuard.CanBegin = false;
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        Assert.That(_runSteeringPointerPressGuard.Requests, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordanceLayout.Snapshots, Is.Empty);
        Assert.That(_runSteeringAffordancePresenter.ShowStates, Is.Empty);
        Assert.That(_runSteeringAffordancePresenter.UpdateStates, Is.Empty);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void PointerMoved_AfterBegin_DoesNotRecheckPressGuardAndKeepsUpdatingAffordance()
    {
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _runSteeringPointerPressGuard.CanBegin = false;
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));

        Assert.That(_runSteeringPointerPressGuard.Requests, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordanceLayout.Snapshots, Has.Count.EqualTo(expected: 2));
        Assert.That(_runSteeringAffordancePresenter.UpdateStates, Has.Count.EqualTo(expected: 1));
        Assert.That(_runSteeringAffordancePresenter.UpdateStates[index: 0], Is.EqualTo(_runSteeringAffordanceLayout.Result));
    }

    [Test]
    public void FixedTick_LowResponsiveness_SmoothsRequestedSteering()
    {
        _config.RunSteeringResponsiveness = 5f;
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(expected: 0f));
        Assert.That(_steeringTarget.LinearVelocity.x, Is.LessThan(expected: 0.1f));
    }

    [Test]
    public void FixedTick_BelowMinimumSteerSpeed_WritesUnchangedVelocityWithoutFacing()
    {
        _steeringTarget.LinearVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, z: 0.1f);
        ActivateSteering();

        _input.Press(pointerId: 1, new Vector2(x: 500f, y: 100f));
        _input.Move(pointerId: 1, new Vector2(x: 600f, y: 100f));
        FixedTick();

        Assert.That(_steeringTarget.ApplyVelocityCallCount, Is.EqualTo(expected: 1));
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

        Assert.That(Vector3.Angle(originalPlanar, steeredPlanar), Is.EqualTo(expectedDegrees).Within(amount: 0.001f));
    }

    private static void AssertVector2(Vector2 actual, Vector2 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(amount: 0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(amount: 0.0001f));
    }
}
