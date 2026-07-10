using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunBodyMovementControllerTests
{
    private readonly List<UnityEngine.Object> _objects = new();

    private FakeGameplayStateService _stateService;
    private FakeLaunchAppliedNotifier _launchNotifier;
    private FakeMovementTarget _target;
    private FakeSteeringInputSource _inputSource;
    private FakeRunBodySpeedEvaluator _speedEvaluator;
    private FakeSteeringEvaluator _steeringEvaluator;
    private FakeLaunchLandingStabilizer _stabilizer;
    private FakeSteeringFrame _steeringFrame;
    private FakeSurfaceContextSource _surfaceContextSource;
    private FakeRunProgressService _runProgressService;
    private FakeRunGameplayStatResolver _runGameplayStatResolver;
    private RunBodySpeedDiagnostics _speedDiagnostics;
    private FakeMovementConfig _config;
    private RunBodySpeedEnvelopeValidator _speedEnvelopeValidator;
    private FakeTime _clock;
    private GameplayStatId _playerMaxSpeedStatId;
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private RunBodyMovementController _controller;
    private List<string> _callOrder;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId("PreLaunch");
        _runningStateId = CreateStateId("Running");
        _stateService = new FakeGameplayStateService(_runningStateId);
        _launchNotifier = new FakeLaunchAppliedNotifier();
        _callOrder = new List<string>();
        _target = new FakeMovementTarget(_callOrder);
        _inputSource = new FakeSteeringInputSource(_callOrder);
        _speedEvaluator = new FakeRunBodySpeedEvaluator(_callOrder);
        _steeringEvaluator = new FakeSteeringEvaluator(_callOrder);
        _stabilizer = new FakeLaunchLandingStabilizer(_callOrder);
        _steeringFrame = new FakeSteeringFrame();
        _surfaceContextSource = new FakeSurfaceContextSource();
        _runProgressService = new FakeRunProgressService();
        _runGameplayStatResolver = new FakeRunGameplayStatResolver();
        _speedDiagnostics = new RunBodySpeedDiagnostics();
        _config = new FakeMovementConfig();
        _speedEnvelopeValidator = new RunBodySpeedEnvelopeValidator(_config);
        _clock = new FakeTime { FixedDeltaTime = 0.02f };
        _playerMaxSpeedStatId = CreateStatId("PlayerMaxSpeed");

        _controller = new RunBodyMovementController(
            _stateService,
            _launchNotifier,
            _target,
            _inputSource,
            _speedEvaluator,
            _steeringEvaluator,
            _stabilizer,
            _steeringFrame,
            _steeringFrame,
            _surfaceContextSource,
            _runProgressService,
            _runGameplayStatResolver,
            _speedDiagnostics,
            _config,
            _config,
            _config,
            _speedEnvelopeValidator,
            _clock,
            _playerMaxSpeedStatId,
            _runningStateId);
        ((IInitializable)_controller).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_controller).Dispose();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void FixedTick_ActiveRun_UsesOrderedCorrectionsAndWritesOneTargetState()
    {
        _target.CurrentVelocity = Vector3.forward * 100f;
        _config.RunBodySpeedSanityGuardMetersPerSecond = 25f;

        _stabilizer.Transform = context =>
        {
            Assert.That(context.CurrentVelocity.magnitude, Is.EqualTo(25f).Within(0.0001f));
            return context.CurrentVelocity + Vector3.up * 2f;
        };

        ActivateMovement();
        FixedTick();

        Assert.That(_callOrder, Is.EqualTo(new[] { "input", "read", "stabilize", "speed", "steering", "write" }));
        Assert.That(_inputSource.AdvanceCallCount, Is.EqualTo(1));
        Assert.That(_target.ReadCallCount, Is.EqualTo(1));
        Assert.That(_target.ApplyCallCount, Is.EqualTo(1));
        Assert.That(_steeringEvaluator.LastContext.TangentSpeed, Is.EqualTo(25f).Within(0.0001f));
        Assert.That(_steeringEvaluator.LastContext.HasUsableTangentDirection, Is.True);
        AssertVector(_target.LastTargetState.LinearVelocity, Vector3.forward * 25f + Vector3.up * 2f);
    }

    [Test]
    public void FixedTick_ActiveRun_ResolvesPlayerMaxSpeedEveryTickAndPassesItToEvaluator()
    {
        _runGameplayStatResolver.ResolvedValue = 32f;

        ActivateMovement();
        FixedTick();
        FixedTick();

        Assert.That(_runGameplayStatResolver.ResolveCallCount, Is.EqualTo(2));
        Assert.That(_runGameplayStatResolver.LastStatId, Is.SameAs(_playerMaxSpeedStatId));
        Assert.That(_runGameplayStatResolver.LastBaseValue, Is.EqualTo(_config.BaseSoftMaximumSpeed));
        Assert.That(_speedEvaluator.LastContext.ResolvedSoftMaximumSpeed, Is.EqualTo(32f));
        Assert.That(_speedEvaluator.EvaluateCallCount, Is.EqualTo(2));
        Assert.That(_target.ApplyCallCount, Is.EqualTo(2));
    }

    [Test]
    public void FixedTick_InvalidResolvedEnvelope_ThrowsBeforeMovementWork()
    {
        _runGameplayStatResolver.ResolvedValue = _config.RunBodySpeedSanityGuardMetersPerSecond;

        ActivateMovement();

        Assert.That(FixedTick, Throws.TypeOf<InvalidOperationException>());
        Assert.That(_runGameplayStatResolver.ResolveCallCount, Is.EqualTo(1));
        Assert.That(_inputSource.AdvanceCallCount, Is.Zero);
        Assert.That(_target.ReadCallCount, Is.Zero);
        Assert.That(_speedEvaluator.EvaluateCallCount, Is.Zero);
        Assert.That(_target.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_ValidGroundedDownhill_IntegratesAccelerationOnceAndPreservesNormalVelocity()
    {
        var surfaceNormal = new Vector3(0f, 1f, 1f).normalized;
        var tangentDirection = Vector3.ProjectOnPlane(Vector3.forward, surfaceNormal).normalized;
        _target.CurrentVelocity = tangentDirection * 10f + surfaceNormal * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, surfaceNormal, 30f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 4f,
            tangentDrag: 0f,
            lowSpeedAssistTargetSpeed: 0f,
            lowSpeedAssistAcceleration: 0f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.DownhillAcceleration);
        _clock.FixedDeltaTime = 0.25f;

        ActivateMovement();
        FixedTick();

        var finalVelocity = _target.LastTargetState.LinearVelocity;
        Assert.That(Vector3.ProjectOnPlane(finalVelocity, surfaceNormal).magnitude, Is.EqualTo(11f).Within(0.0001f));
        Assert.That(Vector3.Dot(finalVelocity, surfaceNormal), Is.EqualTo(2f).Within(0.0001f));
        Assert.That(_target.ApplyCallCount, Is.EqualTo(1));
        Assert.That(_speedEvaluator.EvaluateCallCount, Is.EqualTo(1));
        Assert.That(_speedEvaluator.LastContext.HasValidGroundedRunSurface, Is.True);
        Assert.That(_speedEvaluator.LastContext.ForwardDownhillDegrees, Is.EqualTo(30f));
        Assert.That(_speedEvaluator.LastContext.CourseForwardAlignment, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void FixedTick_PositiveAccelerationNearSoftEnvelope_StopsAtEnvelope()
    {
        _target.CurrentVelocity = Vector3.forward * 19.5f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 45f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 10f,
            tangentDrag: 0f,
            lowSpeedAssistTargetSpeed: 0f,
            lowSpeedAssistAcceleration: 0f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.DownhillAcceleration);
        _clock.FixedDeltaTime = 0.1f;

        ActivateMovement();
        FixedTick();

        Assert.That(_target.LastTargetState.LinearVelocity.magnitude, Is.EqualTo(20f).Within(0.0001f));
    }

    [Test]
    public void FixedTick_PreExistingOverspeed_DoesNotClampToSoftEnvelope()
    {
        _target.CurrentVelocity = Vector3.forward * 24f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 45f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 10f,
            tangentDrag: 0f,
            lowSpeedAssistTargetSpeed: 0f,
            lowSpeedAssistAcceleration: 0f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.DownhillAcceleration);

        ActivateMovement();
        FixedTick();

        Assert.That(_target.LastTargetState.LinearVelocity.magnitude, Is.EqualTo(24f).Within(0.0001f));
    }

    [Test]
    public void FixedTick_AccelerationAndDrag_AppliesAccelerationBeforeDragAndWritesOnce()
    {
        _target.CurrentVelocity = Vector3.forward * 9.8f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 30f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 4f,
            tangentDrag: 2f,
            lowSpeedAssistTargetSpeed: 0f,
            lowSpeedAssistAcceleration: 0f,
            softMaximumSpeed: 10f,
            contributors: RunBodySpeedDecisionContributors.DownhillAcceleration
                          | RunBodySpeedDecisionContributors.SurfaceSlowdown);
        _clock.FixedDeltaTime = 0.1f;

        ActivateMovement();
        FixedTick();

        Assert.That(_target.LastTargetState.LinearVelocity.magnitude, Is.EqualTo(9.8f).Within(0.0001f));
        Assert.That(_target.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_DragExceedsTangentSpeed_StopsAtZeroAndPreservesNormalVelocity()
    {
        _target.CurrentVelocity = Vector3.forward * 0.1f + Vector3.down;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 0f,
            tangentDrag: 2f,
            lowSpeedAssistTargetSpeed: 0f,
            lowSpeedAssistAcceleration: 0f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.SurfaceSlowdown);
        _clock.FixedDeltaTime = 1f;

        ActivateMovement();
        FixedTick();

        AssertVector(_target.LastTargetState.LinearVelocity, Vector3.down);
        Assert.That(_target.LastTargetState.HasRotation, Is.False);
        Assert.That(_target.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_EligibleLowSpeedAssist_AppliesAfterOrdinarySpeedIntegration()
    {
        _target.CurrentVelocity = Vector3.forward * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 1f,
            tangentDrag: 0.5f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 2f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.DownhillAcceleration
                          | RunBodySpeedDecisionContributors.SurfaceSlowdown
                          | RunBodySpeedDecisionContributors.LowSpeedAssist);
        _clock.FixedDeltaTime = 1f;

        ActivateMovement();
        FixedTick();

        Assert.That(_target.LastTargetState.LinearVelocity.magnitude, Is.EqualTo(4.5f).Within(0.0001f));
        Assert.That(_target.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_SolverCancelsAssist_ExhaustsInitialDeficitWithoutContinuousPropulsion()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 0f,
            tangentDrag: 0f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 1f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.LowSpeedAssist);
        _clock.FixedDeltaTime = 1f;

        ActivateMovement();
        var writtenSpeeds = new List<float>();

        for (var passIndex = 0; passIndex < 4; passIndex++)
        {
            _target.CurrentVelocity = Vector3.forward * 2f;
            FixedTick();
            writtenSpeeds.Add(_target.LastTargetState.LinearVelocity.magnitude);
        }

        Assert.That(writtenSpeeds, Is.EqualTo(new[] { 3f, 3f, 3f, 2f }));
    }

    [Test]
    public void FixedTick_SupportLoss_PausesAndResumesSameAssistAttempt()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 0f,
            tangentDrag: 0f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 1f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.LowSpeedAssist);
        _clock.FixedDeltaTime = 1f;

        ActivateMovement();
        _target.CurrentVelocity = Vector3.forward * 2f;
        FixedTick();

        _target.CurrentVelocity = Vector3.forward * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        FixedTick();
        var unsupportedWrittenSpeed = _target.LastTargetState.LinearVelocity.magnitude;

        _target.CurrentVelocity = Vector3.forward * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);
        FixedTick();

        Assert.That(unsupportedWrittenSpeed, Is.EqualTo(2f));
        Assert.That(_target.LastTargetState.LinearVelocity.magnitude, Is.EqualTo(3f));
    }

    [Test]
    public void FixedTick_ActivePass_PublishesExactSpeedContextDecisionAndAssistState()
    {
        var contributors = RunBodySpeedDecisionContributors.DownhillAcceleration
                           | RunBodySpeedDecisionContributors.SurfaceSlowdown
                           | RunBodySpeedDecisionContributors.AboveEnvelopeResistance
                           | RunBodySpeedDecisionContributors.LowSpeedAssist;
        _target.CurrentVelocity = (Vector3.forward + Vector3.right).normalized * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 28f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);
        _runGameplayStatResolver.ResolvedValue = 24f;

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 0f,
            tangentDrag: 0f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 1f,
            softMaximumSpeed: 24f,
            contributors);
        _clock.FixedDeltaTime = 1f;

        ActivateMovement();
        FixedTick();

        var snapshot = _speedDiagnostics.Current;
        Assert.That(snapshot.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Active));
        Assert.That(snapshot.IsRunSurfaceGrounded, Is.True);
        Assert.That(snapshot.HasValidGroundedRunSurface, Is.True);
        Assert.That(snapshot.HasUsableTangentDirection, Is.True);
        Assert.That(snapshot.SampledTangentSpeed, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(snapshot.EffectiveSoftMaximumSpeed, Is.EqualTo(24f));
        Assert.That(snapshot.ForwardDownhillDegrees, Is.EqualTo(28f));
        Assert.That(snapshot.CourseForwardAlignment, Is.EqualTo(Mathf.Sqrt(0.5f)).Within(0.0001f));
        Assert.That(snapshot.Contributors, Is.EqualTo(contributors));
        Assert.That(snapshot.EffectiveLowSpeedAssistTargetSpeed, Is.EqualTo(5f));
        Assert.That(snapshot.LowSpeedAssistAttemptState, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Active));
        Assert.That(snapshot.IsLowSpeedAssistEligible, Is.True);

        Assert.That(
            snapshot.RemainingRequestedLowSpeedAssistVelocityBudget,
            Is.EqualTo(2f).Within(0.0001f));
        Assert.That(_target.LastTargetState.LinearVelocity.magnitude, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(_target.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_UnsupportedDirectionlessPass_PublishesExplicitUnavailableFacts()
    {
        _target.CurrentVelocity = Vector3.up * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 45f);

        ActivateMovement();
        FixedTick();

        var snapshot = _speedDiagnostics.Current;
        Assert.That(snapshot.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Active));
        Assert.That(snapshot.IsRunSurfaceGrounded, Is.False);
        Assert.That(snapshot.HasValidGroundedRunSurface, Is.False);
        Assert.That(snapshot.HasUsableTangentDirection, Is.False);
        Assert.That(snapshot.SampledTangentSpeed, Is.Zero);
        Assert.That(snapshot.ForwardDownhillDegrees, Is.Zero);
        Assert.That(snapshot.CourseForwardAlignment, Is.Zero);
        Assert.That(snapshot.Contributors, Is.EqualTo(RunBodySpeedDecisionContributors.None));
        Assert.That(snapshot.EffectiveLowSpeedAssistTargetSpeed, Is.Zero);

        Assert.That(
            snapshot.LowSpeedAssistAttemptState,
            Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Unavailable));
        Assert.That(snapshot.IsLowSpeedAssistEligible, Is.False);
        Assert.That(snapshot.RemainingRequestedLowSpeedAssistVelocityBudget, Is.Zero);
    }

    [Test]
    public void NewRun_PreviousAttemptExhausted_RearmsLowSpeedAssist()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 0f);

        _speedEvaluator.Decision = new RunBodySpeedDecision(
            tangentAcceleration: 0f,
            tangentDrag: 0f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 3f,
            softMaximumSpeed: 20f,
            contributors: RunBodySpeedDecisionContributors.LowSpeedAssist);
        _clock.FixedDeltaTime = 1f;

        ActivateMovement();
        _target.CurrentVelocity = Vector3.forward * 2f;
        FixedTick();

        _target.CurrentVelocity = Vector3.forward * 2f;
        FixedTick();
        var exhaustedAttemptWrittenSpeed = _target.LastTargetState.LinearVelocity.magnitude;

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);
        ActivateMovement();
        _target.CurrentVelocity = Vector3.forward * 2f;
        FixedTick();

        Assert.That(exhaustedAttemptWrittenSpeed, Is.EqualTo(2f));
        Assert.That(_target.LastTargetState.LinearVelocity.magnitude, Is.EqualTo(5f));
    }

    [Test]
    public void FixedTick_MeaningfulSurfaceNormalLift_RejectsGroundedSpeedPolicy()
    {
        _target.CurrentVelocity = Vector3.forward * 8f + Vector3.up * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 30f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);
        _config.MaximumSupportedSurfaceNormalLiftSpeed = 1f;

        ActivateMovement();
        FixedTick();

        Assert.That(_speedEvaluator.LastContext.HasValidGroundedRunSurface, Is.False);
        AssertVector(_target.LastTargetState.LinearVelocity, Vector3.forward * 8f + Vector3.up * 2f);
    }

    [Test]
    public void FixedTick_SurfaceTangentFloatingPointNoise_DoesNotRejectGroundedSpeedPolicy()
    {
        var surfaceNormal = Quaternion.AngleAxis(30f, Vector3.right) * Vector3.up;
        var tangentDirection = Vector3.ProjectOnPlane(Vector3.forward, surfaceNormal).normalized;
        _target.CurrentVelocity = tangentDirection * 5f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, surfaceNormal, 30f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);
        _config.MaximumSupportedSurfaceNormalLiftSpeed = 0f;

        ActivateMovement();
        FixedTick();

        Assert.That(_speedEvaluator.LastContext.HasValidGroundedRunSurface, Is.True);
        Assert.That(_steeringEvaluator.LastContext.SteeringMode, Is.EqualTo(RunSteeringMode.Grounded));
    }

    [Test]
    public void FixedTick_DiagonalTangentTravel_ComputesProportionalCourseForwardAlignment()
    {
        _target.CurrentVelocity = (Vector3.forward + Vector3.right).normalized * 8f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.up, 30f);
        _runProgressService.SetFrame(Vector3.forward, Vector3.up);

        ActivateMovement();
        FixedTick();

        Assert.That(
            _speedEvaluator.LastContext.CourseForwardAlignment,
            Is.EqualTo(Mathf.Sqrt(0.5f)).Within(0.0001f));
    }

    [Test]
    public void FixedTick_GroundedSteering_ProjectsIntentAndPreservesSurfaceNormalVelocity()
    {
        var surfaceNormal = new Vector3(0f, 1f, 1f).normalized;
        var originalTangent = Vector3.right * 10f;
        var originalNormal = surfaceNormal * 2f;
        _target.CurrentVelocity = originalTangent + originalNormal;
        _surfaceContextSource.Current = new RunSurfaceContext(true, surfaceNormal, 20f);
        _steeringEvaluator.Decision = new RunSteeringDecision(true, 35f, true);

        ActivateMovement();
        FixedTick();

        var expectedDirection = Vector3.ProjectOnPlane(
            Quaternion.AngleAxis(35f, Vector3.up) * originalTangent.normalized,
            surfaceNormal).normalized;
        var finalVelocity = _target.LastTargetState.LinearVelocity;
        var finalTangent = Vector3.ProjectOnPlane(finalVelocity, surfaceNormal);

        Assert.That(Vector3.Dot(finalVelocity, surfaceNormal), Is.EqualTo(2f).Within(0.0001f));
        Assert.That(finalTangent.magnitude, Is.EqualTo(10f).Within(0.0001f));
        Assert.That(Vector3.Dot(finalTangent.normalized, expectedDirection), Is.GreaterThan(0.9999f));
        Assert.That(_target.LastTargetState.HasRotation, Is.True);

        Assert.That(
            Vector3.Dot(_target.LastTargetState.Rotation * Vector3.forward, expectedDirection),
            Is.GreaterThan(0.9999f));
    }

    [Test]
    public void FixedTick_InvalidProjectedIntent_FallsBackToExistingTangentDirection()
    {
        var surfaceNormal = Vector3.forward;
        _target.CurrentVelocity = Vector3.right * 6f + surfaceNormal * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, surfaceNormal, 0f);
        _steeringEvaluator.Decision = new RunSteeringDecision(true, -90f, true);

        ActivateMovement();
        FixedTick();

        AssertVector(_target.LastTargetState.LinearVelocity, Vector3.right * 6f + surfaceNormal * 2f);
        Assert.That(_target.LastTargetState.HasRotation, Is.True);

        Assert.That(
            Vector3.Dot(_target.LastTargetState.Rotation * Vector3.forward, Vector3.right),
            Is.GreaterThan(0.9999f));
    }

    [Test]
    public void FixedTick_DirectionlessTangentMotion_DoesNotSynthesizeHeading()
    {
        var surfaceNormal = Vector3.forward;
        _target.CurrentVelocity = surfaceNormal * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(true, surfaceNormal, 0f);
        _steeringEvaluator.Decision = new RunSteeringDecision(true, 90f, true);

        ActivateMovement();
        FixedTick();

        AssertVector(_target.LastTargetState.LinearVelocity, surfaceNormal * 2f);
        Assert.That(_target.LastTargetState.HasRotation, Is.False);
    }

    [Test]
    public void FixedTick_UnsupportedMotion_KeepsSpeedNeutralAndAirSteeringDirectionOnly()
    {
        _target.CurrentVelocity = Vector3.forward * 7f + Vector3.up * 2f;
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
        _steeringEvaluator.Decision = new RunSteeringDecision(true, 90f, true);

        ActivateMovement();
        FixedTick();

        AssertVector(_target.LastTargetState.LinearVelocity, Vector3.right * 7f + Vector3.up * 2f);
        Assert.That(_steeringEvaluator.LastContext.SteeringMode, Is.EqualTo(RunSteeringMode.Air));
        Assert.That(_target.ApplyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void FixedTick_InactiveRun_DoesNotReadInputOrWriteMovement()
    {
        FixedTick();

        Assert.That(_inputSource.AdvanceCallCount, Is.Zero);
        Assert.That(_target.ReadCallCount, Is.Zero);
        Assert.That(_target.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void LeavingRunning_ClearsStabilizationAndFrameThenStopsWrites()
    {
        ActivateMovement();
        FixedTick();
        _stateService.ChangeTo(_preLaunchStateId);
        _target.ResetCounts();
        _inputSource.ResetCounts();

        FixedTick();

        Assert.That(_stabilizer.ResetCallCount, Is.EqualTo(1));
        Assert.That(_steeringFrame.ClearCallCount, Is.EqualTo(1));
        Assert.That(_inputSource.AdvanceCallCount, Is.Zero);
        Assert.That(_target.ApplyCallCount, Is.Zero);
        Assert.That(_speedDiagnostics.Current.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Inactive));
    }

    [Test]
    public void LaunchApplied_ArmsStabilizerAndResetsFrameForEveryLaunch()
    {
        ActivateMovement(Vector3.up);
        ActivateMovement(Vector3.right);

        Assert.That(_stabilizer.ArmCallCount, Is.EqualTo(2));
        Assert.That(_steeringFrame.ResetCallCount, Is.EqualTo(2));
        AssertVector(_steeringFrame.LastResetUp, Vector3.right);
    }

    private void ActivateMovement()
    {
        ActivateMovement(Vector3.up);
    }

    private void ActivateMovement(Vector3 launchUp)
    {
        _launchNotifier.Apply(CreateLaunchAppliedEvent(launchUp));
    }

    private void FixedTick()
    {
        ((IFixedTickable)_controller).FixedTick();
    }

    private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent(Vector3 launchUp)
    {
        var request = new SlingshotLaunchRequest(
            1f,
            1f,
            0f,
            0f,
            Vector3.zero,
            Vector3.forward,
            Vector3.up);

        return new SlingshotLaunchAppliedEvent(
            request,
            Vector3.forward * 10f,
            Vector3.forward,
            launchUp);
    }

    private GameplayStateId CreateStateId(string name)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = name;
        _objects.Add(stateId);
        return stateId;
    }

    private GameplayStatId CreateStatId(string name)
    {
        var statId = ScriptableObject.CreateInstance<GameplayStatId>();
        statId.name = name;
        _objects.Add(statId);
        return statId;
    }

    private void AssertVector(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
    }

    private sealed class FakeGameplayStateService : IGameplayStateService
    {
        public GameplayStateId CurrentStateId { get; private set; }

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        public FakeGameplayStateService(GameplayStateId initialStateId)
        {
            CurrentStateId = initialStateId;
        }

        public bool IsCurrent(GameplayStateId stateId)
        {
            return CurrentStateId == stateId;
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            ChangeTo(nextStateId);
            return true;
        }

        public void ChangeTo(GameplayStateId nextStateId)
        {
            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
        }
    }

    private sealed class FakeLaunchAppliedNotifier : ISlingshotLaunchAppliedNotifier
    {
        public event Action<SlingshotLaunchAppliedEvent> LaunchApplied;

        public void Apply(SlingshotLaunchAppliedEvent launchAppliedEvent)
        {
            LaunchApplied?.Invoke(launchAppliedEvent);
        }
    }

    private sealed class FakeMovementTarget : IRunBodyMovementTarget
    {
        private readonly List<string> _callOrder;
        private Vector3 _currentVelocity;

        public Vector3 CurrentVelocity
        {
            get => _currentVelocity;
            set => _currentVelocity = value;
        }

        public int ReadCallCount { get; private set; }
        public int ApplyCallCount { get; private set; }
        public RunBodyMovementTargetState LastTargetState { get; private set; }

        Vector3 IRunBodyMovementTarget.LinearVelocity
        {
            get
            {
                _callOrder.Add("read");
                ReadCallCount += 1;
                return _currentVelocity;
            }
        }

        public FakeMovementTarget(List<string> callOrder)
        {
            _callOrder = callOrder;
        }

        public void ApplyTargetState(RunBodyMovementTargetState targetState)
        {
            _callOrder.Add("write");
            LastTargetState = targetState;
            _currentVelocity = targetState.LinearVelocity;
            ApplyCallCount += 1;
        }

        public void ResetCounts()
        {
            ReadCallCount = 0;
            ApplyCallCount = 0;
        }
    }

    private sealed class FakeSteeringInputSource : IRunSteeringInputSource
    {
        private readonly List<string> _callOrder;

        public int AdvanceCallCount { get; private set; }
        public RunSteeringInputState State { get; set; }

        public FakeSteeringInputSource(List<string> callOrder)
        {
            _callOrder = callOrder;
        }

        public RunSteeringInputState AdvanceAndRead(float fixedDeltaTime)
        {
            _callOrder.Add("input");
            AdvanceCallCount += 1;
            return State;
        }

        public void ResetCounts()
        {
            AdvanceCallCount = 0;
        }
    }

    private sealed class FakeRunBodySpeedEvaluator : IRunBodySpeedEvaluator
    {
        private readonly List<string> _callOrder;

        public int EvaluateCallCount { get; private set; }
        public RunBodySpeedContext LastContext { get; private set; }
        public RunBodySpeedDecision Decision { get; set; }

        public FakeRunBodySpeedEvaluator(List<string> callOrder)
        {
            _callOrder = callOrder;
        }

        public RunBodySpeedDecision Evaluate(RunBodySpeedContext context)
        {
            _callOrder.Add("speed");
            EvaluateCallCount += 1;
            LastContext = context;
            return context.HasValidGroundedRunSurface ? Decision : default;
        }
    }

    private sealed class FakeSteeringEvaluator : IRunSteeringEvaluator
    {
        private readonly List<string> _callOrder;

        public RunSteeringDecision Decision { get; set; }
        public RunSteeringContext LastContext { get; private set; }

        public FakeSteeringEvaluator(List<string> callOrder)
        {
            _callOrder = callOrder;
        }

        public RunSteeringDecision Evaluate(RunSteeringContext context)
        {
            _callOrder.Add("steering");
            LastContext = context;
            return Decision;
        }
    }

    private sealed class FakeLaunchLandingStabilizer : IRunLaunchLandingStabilizer
    {
        private readonly List<string> _callOrder;

        public int ArmCallCount { get; private set; }
        public int ResetCallCount { get; private set; }
        public Func<RunLaunchLandingStabilizationContext, Vector3> Transform { get; set; }

        public FakeLaunchLandingStabilizer(List<string> callOrder)
        {
            _callOrder = callOrder;
            Transform = context => context.CurrentVelocity;
        }

        public void ArmForLaunch()
        {
            ArmCallCount += 1;
        }

        public void Reset()
        {
            ResetCallCount += 1;
        }

        public Vector3 Stabilize(RunLaunchLandingStabilizationContext context)
        {
            _callOrder.Add("stabilize");
            return Transform(context);
        }
    }

    private sealed class FakeSteeringFrame : IRunSteeringFrameSource, IRunSteeringFrameResetter
    {
        public int ResetCallCount { get; private set; }
        public int ClearCallCount { get; private set; }
        public Vector3 LastResetUp { get; private set; }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            return fallbackUpDirection;
        }

        public void Reset(Vector3 launchUpDirection)
        {
            LastResetUp = launchUpDirection;
            ResetCallCount += 1;
        }

        public void Clear()
        {
            ClearCallCount += 1;
        }
    }

    private sealed class FakeSurfaceContextSource : IRunSurfaceContextSource
    {
        public RunSurfaceContext Current { get; set; } = new(false, Vector3.up, 0f);
    }

    private sealed class FakeRunProgressService : IRunProgressService
    {
        public bool HasValidSnapshot { get; private set; }
        public string SnapshotError => string.Empty;
        public RunProgressFrameSnapshot Snapshot { get; private set; }
        public float CurrentForwardProgress => 0f;
        public float MaximumForwardProgress => 0f;
        public RunProgressSample CurrentSample => default;

        public bool TryBeginRun(Vector3 origin, out string error)
        {
            error = string.Empty;
            return HasValidSnapshot;
        }

        public void SamplePosition(Vector3 position)
        {
        }

        public void Reset()
        {
            HasValidSnapshot = false;
            Snapshot = default;
        }

        public void SetFrame(Vector3 forward, Vector3 up)
        {
            HasValidSnapshot = RunProgressFrameSnapshot.TryCreate(
                Vector3.zero,
                forward,
                up,
                out var snapshot,
                out _);
            Snapshot = snapshot;
        }
    }

    private sealed class FakeRunGameplayStatResolver : IRunGameplayStatResolver
    {
        public int ResolveCallCount { get; private set; }
        public GameplayStatId LastStatId { get; private set; }
        public float LastBaseValue { get; private set; }
        public float? ResolvedValue { get; set; }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            ResolveCallCount += 1;
            LastStatId = statId;
            LastBaseValue = baseValue;
            return ResolvedValue ?? baseValue;
        }
    }

    private sealed class FakeMovementConfig : IRunBodySpeedConfig, IRunBodyMovementValidityConfig, IRunSteeringConfig
    {
        public float DownhillAcceleration { get; set; } = 8f;
        public float SurfaceSlowdown { get; set; }
        public float LowSpeedAssistTargetSpeed { get; set; }
        public float LowSpeedAssistAcceleration { get; set; }
        public float BaseSoftMaximumSpeed { get; set; } = 20f;
        public float AboveMaximumSpeedResistance { get; set; }
        public float MaximumSupportedSurfaceNormalLiftSpeed { get; set; } = 5f;
        public float RunBodySpeedSanityGuardMetersPerSecond { get; set; } = 250f;
        public float RunSteeringRangeCentimeters { get; set; } = 2.54f;
        public float RunSteeringDeadzoneFraction { get; set; }
        public float RunSteeringResponsiveness { get; set; } = 100f;
        public float FallbackDpi { get; set; } = 100f;
        public float MinimumAcceptedDpi { get; set; } = 1f;
        public float MaximumAcceptedDpi { get; set; } = 1000f;
        public float MaximumTurnDegreesPerSecond { get; set; } = 90f;
        public float RunAirSteeringMaximumTurnDegreesPerSecond { get; set; } = 30f;
        public float MinimumSteerSpeed { get; set; } = 0.25f;
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
