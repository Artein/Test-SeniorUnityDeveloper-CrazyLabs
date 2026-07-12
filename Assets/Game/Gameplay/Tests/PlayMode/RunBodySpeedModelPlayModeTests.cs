using System;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunBodySpeedModelPlayModeTests
{
    [Test]
    public void given_EquivalentGroundedMotion_when_SurfaceIsDownhill_then_MoreTangentSpeedIsGainedThanOnFlat()
    {
        using var scenario = new MovementScenario();
        const float initialSpeed = 5f;

        var flatVelocity = scenario.Execute(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * initialSpeed);

        var downhillNormal = Quaternion.AngleAxis(30f, Vector3.right) * Vector3.up;
        var downhillDirection = Vector3.ProjectOnPlane(Vector3.forward, downhillNormal).normalized;

        var downhillVelocity = scenario.Execute(
            new RunSurfaceContext(true, downhillNormal, 30f),
            downhillDirection * initialSpeed);

        var flatTangentSpeed = Vector3.ProjectOnPlane(flatVelocity, Vector3.up).magnitude;
        var downhillTangentSpeed = Vector3.ProjectOnPlane(downhillVelocity, downhillNormal).magnitude;

        Assert.That(flatTangentSpeed, Is.EqualTo(initialSpeed).Within(0.0001f));
        Assert.That(downhillTangentSpeed, Is.EqualTo(6f).Within(0.0001f));
        Assert.That(downhillTangentSpeed, Is.GreaterThan(flatTangentSpeed));
    }

    [Test]
    public void given_UnsupportedLaunchFlight_when_MovementTicks_then_GroundedSpeedPolicyDoesNotChangeVelocity()
    {
        using var scenario = new MovementScenario(surfaceSlowdown: 2f);
        var launchVelocity = Vector3.forward * 7f + Vector3.up * 2f;

        var finalVelocity = scenario.Execute(
            new RunSurfaceContext(false, Vector3.up, 45f),
            launchVelocity);

        AssertVector(finalVelocity, launchVelocity);
    }

    [Test]
    public void given_GroundedFlatMotionWithoutContactFriction_when_SurfaceSlowdownEnabled_then_ModelRemovesExpectedTangentSpeed()
    {
        using var scenario = new MovementScenario(surfaceSlowdown: 2f);

        var finalVelocity = scenario.Execute(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * 5f);

        Assert.That(finalVelocity.magnitude, Is.EqualTo(4.5f).Within(0.0001f));
        AssertVector(finalVelocity.normalized, Vector3.forward);
    }

    [Test]
    public void given_EquivalentGroundedDownhillMotion_when_PlayerMaxSpeedUpgradeApplied_then_UpgradedRunBuildsMoreSpeed()
    {
        using var neutralScenario = new MovementScenario(resolvedSoftMaximumSpeed: 20f);
        using var upgradedScenario = new MovementScenario(resolvedSoftMaximumSpeed: 30f);
        var downhillNormal = Quaternion.AngleAxis(30f, Vector3.right) * Vector3.up;
        var downhillDirection = Vector3.ProjectOnPlane(Vector3.forward, downhillNormal).normalized;
        var surfaceContext = new RunSurfaceContext(true, downhillNormal, 30f);
        var initialVelocity = downhillDirection * 19f;

        var neutralVelocity = neutralScenario.ExecuteTicks(surfaceContext, initialVelocity, 8);
        var upgradedVelocity = upgradedScenario.ExecuteTicks(surfaceContext, initialVelocity, 8);

        Assert.That(neutralVelocity.magnitude, Is.EqualTo(20f).Within(0.0001f));
        Assert.That(upgradedVelocity.magnitude, Is.EqualTo(27f).Within(0.0001f));
        Assert.That(upgradedVelocity.magnitude, Is.GreaterThan(neutralVelocity.magnitude));
    }

    [Test]
    public void given_GroundedOverspeed_when_SpeedModelTicks_then_ResistanceSettlesWithoutHardClamp()
    {
        using var scenario = new MovementScenario(
            resolvedSoftMaximumSpeed: 20f,
            aboveMaximumSpeedResistance: 12f);

        var finalVelocity = scenario.Execute(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * 30f);

        Assert.That(finalVelocity.magnitude, Is.EqualTo(28.5f).Within(0.0001f));
        Assert.That(finalVelocity.magnitude, Is.GreaterThan(20f));
        Assert.That(finalVelocity.magnitude, Is.LessThan(30f));
    }

    [Test]
    public void given_UnsupportedOverspeed_when_SpeedModelTicks_then_EnvelopeResistanceDoesNotChangeVelocity()
    {
        using var scenario = new MovementScenario(
            resolvedSoftMaximumSpeed: 20f,
            aboveMaximumSpeedResistance: 12f);
        var initialVelocity = Vector3.forward * 30f + Vector3.up * 2f;

        var finalVelocity = scenario.Execute(
            new RunSurfaceContext(false, Vector3.up, 30f),
            initialVelocity);

        AssertVector(finalVelocity, initialVelocity);
    }

    [Test]
    public void given_RecoverableMovingNearStall_when_LowSpeedAssistTicks_then_SpeedApproachesTargetWithoutCrossing()
    {
        using var scenario = new MovementScenario(
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 4f);

        var finalVelocity = scenario.ExecuteTicks(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * 2f,
            4);

        Assert.That(finalVelocity.magnitude, Is.EqualTo(5f).Within(0.0001f));
        AssertVector(finalVelocity.normalized, Vector3.forward);
    }

    [Test]
    public void given_BlockingResolutionCancelsEveryAssistWrite_when_AttemptBudgetExhausts_then_PropulsionStops()
    {
        using var scenario = new MovementScenario(
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 4f);

        var writtenSpeeds = scenario.ExecuteTicksWithCancelledVelocity(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * 2f,
            4);

        Assert.That(writtenSpeeds, Is.EqualTo(new[] { 3f, 3f, 3f, 2f }));
    }

    [Test]
    public void given_ActiveAssistAttempt_when_SupportIsLostAndResumed_then_AirbornePassIsNeutralAndBudgetIsNotReset()
    {
        using var scenario = new MovementScenario(
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 4f);

        var firstSupportedVelocity = scenario.Execute(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * 2f);

        var unsupportedVelocity = scenario.Execute(
            new RunSurfaceContext(false, Vector3.up, 0f),
            Vector3.forward * 2f);

        var resumedWrittenSpeeds = scenario.ExecuteTicksWithCancelledVelocity(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * 2f,
            3);

        Assert.That(firstSupportedVelocity.magnitude, Is.EqualTo(3f));
        Assert.That(unsupportedVelocity.magnitude, Is.EqualTo(2f));
        Assert.That(resumedWrittenSpeeds, Is.EqualTo(new[] { 3f, 3f, 2f }));
    }

    [Test]
    public void given_DownhillOverspeedWithSlowdown_when_MovementTicks_then_DiagnosticsPublishExactCombinedDecision()
    {
        using var scenario = new MovementScenario(
            resolvedSoftMaximumSpeed: 20f,
            surfaceSlowdown: 2f,
            aboveMaximumSpeedResistance: 12f);
        var downhillNormal = Quaternion.AngleAxis(30f, Vector3.right) * Vector3.up;
        var downhillDirection = Vector3.ProjectOnPlane(Vector3.forward, downhillNormal).normalized;

        scenario.Execute(
            new RunSurfaceContext(true, downhillNormal, 30f),
            downhillDirection * 30f);

        var snapshot = scenario.CurrentSpeedDiagnostics;

        var expectedContributors = RunBodySpeedDecisionContributors.SurfaceSlowdown
                                   | RunBodySpeedDecisionContributors.AboveEnvelopeResistance;

        Assert.That(snapshot.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Active));
        Assert.That(snapshot.IsRunSurfaceGrounded, Is.True);
        Assert.That(snapshot.HasValidGroundedRunSurface, Is.True);
        Assert.That(snapshot.HasUsableTangentDirection, Is.True);
        Assert.That(snapshot.SampledTangentSpeed, Is.EqualTo(30f).Within(0.0001f));
        Assert.That(snapshot.EffectiveSoftMaximumSpeed, Is.EqualTo(20f));
        Assert.That(snapshot.ForwardDownhillDegrees, Is.EqualTo(30f));
        Assert.That(snapshot.CourseForwardAlignment, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(snapshot.PolicyContributors, Is.EqualTo(expectedContributors));
        Assert.That(snapshot.RequestedContributors, Is.EqualTo(expectedContributors));

        Assert.That(
            snapshot.LowSpeedAssistAttemptState,
            Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Unavailable));
    }

    [Test]
    public void given_RecoverableNearStall_when_MovementTicks_then_DiagnosticsPublishAssistAttemptBudget()
    {
        using var scenario = new MovementScenario(
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 4f);

        scenario.Execute(
            new RunSurfaceContext(true, Vector3.up, 0f),
            Vector3.forward * 2f);

        var snapshot = scenario.CurrentSpeedDiagnostics;

        Assert.That(
            snapshot.PolicyContributors,
            Is.EqualTo(RunBodySpeedDecisionContributors.LowSpeedAssist));

        Assert.That(
            snapshot.RequestedContributors,
            Is.EqualTo(RunBodySpeedDecisionContributors.LowSpeedAssist));
        Assert.That(snapshot.RequestedLowSpeedAssistVelocityDelta, Is.EqualTo(1f));
        Assert.That(snapshot.EffectiveLowSpeedAssistTargetSpeed, Is.EqualTo(5f));
        Assert.That(snapshot.LowSpeedAssistAttemptState, Is.EqualTo(RunBodyLowSpeedAssistAttemptState.Active));
        Assert.That(snapshot.MeetsLowSpeedAssistPolicyConditions, Is.True);

        Assert.That(
            snapshot.RemainingRequestedLowSpeedAssistVelocityBudget,
            Is.EqualTo(2f).Within(0.0001f));
    }

    [Test]
    public void given_EquivalentRuns_when_DiagnosticsSourceIsObservedOrIgnored_then_MovementOutputIsIdentical()
    {
        using var observedScenario = new MovementScenario(
            surfaceSlowdown: 2f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 4f);

        using var ignoredScenario = new MovementScenario(
            surfaceSlowdown: 2f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 4f);
        var surfaceContext = new RunSurfaceContext(true, Vector3.up, 0f);
        var initialVelocity = Vector3.forward * 2f;

        var observedVelocity = observedScenario.Execute(surfaceContext, initialVelocity);
        _ = observedScenario.CurrentSpeedDiagnostics;
        var ignoredVelocity = ignoredScenario.Execute(surfaceContext, initialVelocity);

        AssertVector(observedVelocity, ignoredVelocity);
    }

    private void AssertVector(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
    }

    private sealed class MovementScenario : IDisposable
    {
        private readonly GameObject _gameObject;
        private readonly Rigidbody _body;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStatId _playerMaxSpeedStatId;
        private readonly RunBodyMovementController _controller;
        private readonly FakeSurfaceContextSource _surfaceContextSource;
        private readonly RunBodySpeedDiagnostics _speedDiagnostics;
        private bool _isDisposed;

        public MovementScenario(
            float surfaceSlowdown = 0f,
            float resolvedSoftMaximumSpeed = 20f,
            float aboveMaximumSpeedResistance = 0f,
            float lowSpeedAssistTargetSpeed = 0f,
            float lowSpeedAssistAcceleration = 0f)
        {
            _gameObject = new GameObject("Run Body Speed Model PlayMode Test");
            _body = _gameObject.AddComponent<Rigidbody>();
            _body.useGravity = false;

            var movementTarget = _gameObject.AddComponent<RigidbodyRunBodyMovementTarget>();
            movementTarget.SetRigidbodyForTests(_body);

            _runningStateId = ScriptableObject.CreateInstance<GameplayStateId>();
            _runningStateId.name = "Running";
            _playerMaxSpeedStatId = ScriptableObject.CreateInstance<GameplayStatId>();
            _playerMaxSpeedStatId.name = "PlayerMaxSpeed";

            var stateService = new FakeGameplayStateService(_runningStateId);
            var launchNotifier = new FakeLaunchAppliedNotifier();

            var config = new FakeMovementConfig(
                surfaceSlowdown,
                aboveMaximumSpeedResistance,
                lowSpeedAssistTargetSpeed,
                lowSpeedAssistAcceleration);
            var steeringFrame = new FakeSteeringFrame();
            var progressService = new FakeRunProgressService();
            progressService.SetFrame(Vector3.forward, Vector3.up);
            _surfaceContextSource = new FakeSurfaceContextSource();
            _speedDiagnostics = new RunBodySpeedDiagnostics();

            _controller = new RunBodyMovementController(
                stateService,
                launchNotifier,
                movementTarget,
                new NeutralSteeringInputSource(),
                new DefaultRunBodySpeedEvaluator(config),
                new DefaultRunSteeringEvaluator(),
                new PassThroughLaunchLandingStabilizer(),
                steeringFrame,
                steeringFrame,
                _surfaceContextSource,
                progressService,
                new FakeRunGameplayStatResolver(resolvedSoftMaximumSpeed),
                _speedDiagnostics,
                config,
                config,
                config,
                new RunBodySpeedEnvelopeValidator(config),
                new FakeTime { FixedDeltaTime = 0.25f },
                _playerMaxSpeedStatId,
                _runningStateId);

            ((IInitializable)_controller).Initialize();
            launchNotifier.Apply(CreateLaunchAppliedEvent());
        }

        public Vector3 Execute(RunSurfaceContext surfaceContext, Vector3 initialVelocity)
        {
            return ExecuteTicks(surfaceContext, initialVelocity, 1);
        }

        public RunBodySpeedDiagnosticsSnapshot CurrentSpeedDiagnostics => _speedDiagnostics.Current;

        public Vector3 ExecuteTicks(RunSurfaceContext surfaceContext, Vector3 initialVelocity, int tickCount)
        {
            _surfaceContextSource.Current = surfaceContext;
            _body.linearVelocity = initialVelocity;

            for (var tickIndex = 0; tickIndex < tickCount; tickIndex++)
                ((IFixedTickable)_controller).FixedTick();

            return _body.linearVelocity;
        }

        public float[] ExecuteTicksWithCancelledVelocity(
            RunSurfaceContext surfaceContext,
            Vector3 cancelledVelocity,
            int tickCount)
        {
            _surfaceContextSource.Current = surfaceContext;
            var writtenSpeeds = new float[tickCount];

            for (var tickIndex = 0; tickIndex < tickCount; tickIndex++)
            {
                _body.linearVelocity = cancelledVelocity;
                ((IFixedTickable)_controller).FixedTick();
                writtenSpeeds[tickIndex] = _body.linearVelocity.magnitude;
            }

            return writtenSpeeds;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            ((IDisposable)_controller).Dispose();
            UnityEngine.Object.DestroyImmediate(_playerMaxSpeedStatId);
            UnityEngine.Object.DestroyImmediate(_runningStateId);
            UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent()
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
                Vector3.up);
        }
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
            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
            return true;
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

    private sealed class NeutralSteeringInputSource : IRunSteeringInputSource
    {
        public RunSteeringInputState AdvanceAndRead(float fixedDeltaTime)
        {
            return default;
        }
    }

    private sealed class FakeRunGameplayStatResolver : IRunGameplayStatResolver
    {
        private readonly float _resolvedValue;

        public FakeRunGameplayStatResolver(float resolvedValue)
        {
            _resolvedValue = resolvedValue;
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            return _resolvedValue;
        }
    }

    private sealed class PassThroughLaunchLandingStabilizer : IRunLaunchLandingStabilizer
    {
        public void ArmForLaunch()
        {
        }

        public void Reset()
        {
        }

        public Vector3 Stabilize(RunLaunchLandingStabilizationContext context)
        {
            return context.CurrentVelocity;
        }
    }

    private sealed class FakeSteeringFrame : IRunSteeringFrameSource, IRunSteeringFrameResetter
    {
        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            return fallbackUpDirection;
        }

        public void Reset(Vector3 launchUpDirection)
        {
        }

        public void Clear()
        {
        }
    }

    private sealed class FakeSurfaceContextSource : IRunSurfaceFrameSource
    {
        public RunSurfaceContext Current { get; set; } = new(false, Vector3.up, 0f);

        RunSurfaceFrameSnapshot IRunSurfaceFrameSource.Current =>
            new(default, Current, RunSurfaceTransition.None, false, false, default);
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

    private sealed class FakeMovementConfig : IRunBodySpeedConfig, IRunBodyMovementValidityConfig, IRunSteeringConfig
    {
        private readonly float _surfaceSlowdown;
        private readonly float _aboveMaximumSpeedResistance;
        private readonly float _lowSpeedAssistTargetSpeed;
        private readonly float _lowSpeedAssistAcceleration;

        public FakeMovementConfig(
            float surfaceSlowdown,
            float aboveMaximumSpeedResistance,
            float lowSpeedAssistTargetSpeed,
            float lowSpeedAssistAcceleration)
        {
            _surfaceSlowdown = surfaceSlowdown;
            _aboveMaximumSpeedResistance = aboveMaximumSpeedResistance;
            _lowSpeedAssistTargetSpeed = lowSpeedAssistTargetSpeed;
            _lowSpeedAssistAcceleration = lowSpeedAssistAcceleration;
        }

        public float DownhillAcceleration => 8f;
        public float SurfaceSlowdown => _surfaceSlowdown;
        public float LowSpeedAssistTargetSpeed => _lowSpeedAssistTargetSpeed;
        public float LowSpeedAssistAcceleration => _lowSpeedAssistAcceleration;
        public float BaseSoftMaximumSpeed => 20f;
        public float AboveMaximumSpeedResistance => _aboveMaximumSpeedResistance;
        public float MaximumSupportedSurfaceNormalLiftSpeed => 0f;
        public float RunBodySpeedSanityGuardMetersPerSecond => 250f;
        public float RunSteeringRangeCentimeters => 2.54f;
        public float RunSteeringDeadzoneFraction => 0f;
        public float RunSteeringResponsiveness => 100f;
        public float FallbackDpi => 100f;
        public float MinimumAcceptedDpi => 1f;
        public float MaximumAcceptedDpi => 1000f;
        public float MaximumTurnDegreesPerSecond => 90f;
        public float RunAirSteeringMaximumTurnDegreesPerSecond => 30f;
        public float MinimumSteerSpeed => 0.25f;
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
