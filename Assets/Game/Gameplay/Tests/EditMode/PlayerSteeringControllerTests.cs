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
        ActivateSteering();

        FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 5f);
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

        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }
}
