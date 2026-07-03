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
        _steeringTarget.LinearVelocity = initialPlanarVelocity + surfaceUp * DefaultVerticalSpeed;
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        var steeredPlanarVelocity = ProjectPlanar(_steeringTarget.LinearVelocity, surfaceUp);
        Assert.That(_steeringFrameSource.GetUpDirectionCallCount, Is.EqualTo(1));
        Assert.That(Vector3.Dot(steeredPlanarVelocity.normalized, initialPlanarVelocity.normalized), Is.LessThan(0.9999f));
        AssertSpeedComponentsPreservedAround(_steeringTarget.LinearVelocity, surfaceUp);
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

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        AssertResolved(_playerMaxSpeedStatId, DefaultPlanarSpeed);
        AssertResolved(_playerSteeringResponsivenessStatId, 100f);
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_PlayerMaxSpeedModifier_ClampsPlanarSpeed()
    {
        _statResolver.SetResolvedValue(_playerMaxSpeedStatId, 5f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, DefaultVerticalSpeed, 5f));
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, DefaultPlanarSpeed);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 5f);
    }

    [Test]
    public void FixedTick_LaunchBurstWithinGrace_PreservesLaunchPlanarSpeedAbovePlayerMaxSpeed()
    {
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 24f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, DefaultVerticalSpeed, 24f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 24f);
    }

    [Test]
    public void FixedTick_LaunchBurstAboveMultiplier_ClampsToBurstMultiplierCap()
    {
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 60f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, DefaultVerticalSpeed, 60f));

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 30f);
    }

    [Test]
    public void FixedTick_LaunchBurstAfterGrace_DecaysCapTowardPlayerMaxSpeed()
    {
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 30f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, DefaultVerticalSpeed, 30f));
        _clock.FixedDeltaTime = _config.LaunchBurstPlanarSpeedGraceSeconds + (_config.LaunchBurstPlanarSpeedRecoverySeconds * 0.5f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 20f);
    }

    [Test]
    public void FixedTick_LaunchBurstAfterRecovery_ClampsToPlayerMaxSpeed()
    {
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 30f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, DefaultVerticalSpeed, 30f));
        _clock.FixedDeltaTime = _config.LaunchBurstPlanarSpeedGraceSeconds + _config.LaunchBurstPlanarSpeedRecoverySeconds + 0.01f;

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, DefaultPlanarSpeed);
    }

    [Test]
    public void LeavingRunning_ClearsLaunchBurstAllowance()
    {
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 24f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, DefaultVerticalSpeed, 24f));

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.up));
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 24f);
        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, DefaultPlanarSpeed);
    }

    [Test]
    public void FixedTick_NonLaunchOverspeed_ClampsImmediately()
    {
        ActivateSteering();
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 24f);

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, DefaultPlanarSpeed);
    }

    [Test]
    public void FixedTick_UpgradedPlayerMaxSpeed_RaisesNormalAndBurstCaps()
    {
        _statResolver.SetResolvedValue(_playerMaxSpeedStatId, 15f);
        _steeringTarget.LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, 60f);
        ActivateSteeringWithLaunchVelocity(new Vector3(0f, DefaultVerticalSpeed, 60f));

        FixedTick();

        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 45f);
    }

    [Test]
    public void FixedTick_PostLaunchFlatSurfaceLanding_RemovesLiftAndPreservesTangentSpeed()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(3f, 4f, 12f);
        ActivateSteeringWithLaunchVelocity(new Vector3(3f, 4f, 12f));

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(0f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, new Vector3(3f, 0f, 12f).magnitude);
    }

    [Test]
    public void FixedTick_PostLaunchLandingBelowMinimumSteerSpeed_SuppressesLiftWithoutSteeringRotation()
    {
        SetGroundedSurface(Vector3.up);
        var lowTangentLiftVelocity = new Vector3(0f, 3f, 0.1f);
        _steeringTarget.LinearVelocity = lowTangentLiftVelocity;
        ActivateSteeringWithLaunchVelocity(lowTangentLiftVelocity);

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

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);
        _steeringTarget.LinearVelocity = new Vector3(0f, 4f, 8f);
        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_PostLaunchLandingWithBurstOverspeed_ClampsTangentSpeedAndRemovesLift()
    {
        SetGroundedSurface(Vector3.up);
        _steeringTarget.LinearVelocity = new Vector3(0f, 5f, 60f);
        ActivateSteeringWithLaunchVelocity(_steeringTarget.LinearVelocity);

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(0f).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 30f);
    }

    [Test]
    public void FixedTick_PlayerSteeringResponsivenessModifier_IncreasesSteeringResponse()
    {
        _config.RunSteeringResponsiveness = 5f;
        _statResolver.SetResolvedValue(_playerSteeringResponsivenessStatId, 20f);
        ActivateSteering();

        _input.Press(1, new Vector2(500f, 100f));
        _input.Move(1, new Vector2(600f, 100f));
        FixedTick();

        AssertResolved(_playerSteeringResponsivenessStatId, 5f);
        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(0.1f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
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
}
