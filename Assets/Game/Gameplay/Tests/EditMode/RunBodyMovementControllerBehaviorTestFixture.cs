using System;
using System.Collections.Generic;
using Game.Foundation.Input;
using Game.Foundation.Screen;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public abstract class RunBodyMovementControllerBehaviorTestFixture
{
    protected const float DefaultPlanarSpeed = 10f, DefaultVerticalSpeed = 2f;

    private readonly List<Object> _objects = new();
    protected FakeTime _clock;
    protected FakeRunBodyMovementConfig _config;

    protected FakeUnityInput _input;
    private RunSteeringInputController _inputController;
    protected RecordingRunSteeringInputMetricsResolver _inputMetricsResolver;
    protected FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private RunBodyMovementController _movementController;
    protected GameplayStatId _playerMaxSpeedStatId;
    protected GameplayStatId _playerSteeringResponsivenessStatId;
    protected GameplayStateId _preLaunchStateId;
    protected GameplayStateId _runningStateId;
    protected FakeRunProgressService _runProgressService;
    protected FakeRunSteeringAffordanceLayout _runSteeringAffordanceLayout;
    protected FakeRunSteeringAffordancePresenter _runSteeringAffordancePresenter;
    private IRunSteeringGesture _runSteeringGesture;
    protected FakeRunSteeringPointerPressGuard _runSteeringPointerPressGuard;
    protected FakeScreen _screen;
    protected FakeGameplayStateService _stateService;
    protected FakeRunGameplayStatResolver _statResolver;
    protected FakeRunSteeringFrameSource _steeringFrameSource;
    protected FakeRunBodyMovementTarget _steeringTarget;
    protected FakeRunSurfaceFrameSource _surfaceContextSource;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId(stateName: "PreLaunch");
        _runningStateId = CreateStateId(stateName: "Running");
        _playerMaxSpeedStatId = CreateStatId(id: "PlayerMaxSpeed");
        _playerSteeringResponsivenessStatId = CreateStatId(id: "PlayerSteeringResponsiveness");
        _input = new FakeUnityInput();
        _stateService = new FakeGameplayStateService(_preLaunchStateId);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _statResolver = new FakeRunGameplayStatResolver();

        _steeringTarget = new FakeRunBodyMovementTarget
        {
            LinearVelocity = new Vector3(x: 0f, DefaultVerticalSpeed, DefaultPlanarSpeed),
            Rotation = Quaternion.identity
        };

        _config = new FakeRunBodyMovementConfig
        {
            RunSteeringRangeCentimeters = 2.54f,
            RunSteeringDeadzoneFraction = 0f,
            RunSteeringResponsiveness = 100f,
            FallbackDpi = 326f,
            MinimumAcceptedDpi = 1f,
            MaximumAcceptedDpi = 1000f,
            MaximumTurnDegreesPerSecond = 90f,
            RunAirSteeringMaximumTurnDegreesPerSecond = 30f,
            MinimumSteerSpeed = 0.25f,
            MaximumSupportedSurfaceNormalLiftSpeed = 0f,
            RunBodySpeedSanityGuardMetersPerSecond = 250f,
            LaunchLandingStabilizationSeconds = 0.3f,
            LaunchLandingMaximumLiftSpeed = 0f,
            BaseSoftMaximumSpeed = 20f
        };

        _surfaceContextSource = new FakeRunSurfaceFrameSource
        {
            Current = new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f)
        };

        _runProgressService = new FakeRunProgressService();

        _clock = new FakeTime
        {
            DeltaTime = 0.02f,
            FixedDeltaTime = 0.02f
        };

        _screen = new FakeScreen
        {
            Width = 1000,
            Height = 2000,
            Dpi = 100f
        };

        _steeringFrameSource = new FakeRunSteeringFrameSource();
        _inputMetricsResolver = new RecordingRunSteeringInputMetricsResolver(_config);
        _runSteeringGesture = new RunSteeringGesture(_inputMetricsResolver);
        _runSteeringAffordanceLayout = new FakeRunSteeringAffordanceLayout();
        _runSteeringAffordancePresenter = new FakeRunSteeringAffordancePresenter();
        _runSteeringPointerPressGuard = new FakeRunSteeringPointerPressGuard();
        _inputController = CreateInputController();
        _movementController = CreateMovementController(_inputController);
        ((IInitializable)_inputController).Initialize();
        ((IInitializable)_movementController).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_movementController).Dispose();
        ((IDisposable)_inputController).Dispose();

        foreach (var unityObject in _objects)
        {
            Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    protected void ActivateSteering()
    {
        ActivateSteering(Vector3.up);
    }

    protected void ActivateSteering(Vector3 launchUpDirection)
    {
        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(launchUpDirection));
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(expected: 1));
        SettleLaunchStateForSteadySteering(launchUpDirection);
    }

    protected void ActivateSteeringWithLaunchVelocity(Vector3 launchVelocityChange)
    {
        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.up, launchVelocityChange));
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(expected: 1));
    }

    protected void FixedTick()
    {
        ((IFixedTickable)_movementController).FixedTick();
        _surfaceContextSource.Transition = RunSurfaceTransition.None;
    }

    private void SettleLaunchStateForSteadySteering(Vector3 launchUpDirection)
    {
        var previousFixedDeltaTime = _clock.FixedDeltaTime;
        var previousSurfaceContext = _surfaceContextSource.Current;
        var previousVelocity = _steeringTarget.LinearVelocity;
        var previousRotation = _steeringTarget.Rotation;

        SetGroundedSurface(launchUpDirection);
        FixedTick();
        SetUngroundedSurface(RunSurfaceTransition.SupportLost);
        FixedTick();
        SetGroundedSurface(launchUpDirection, RunSurfaceTransition.SupportAcquired);
        FixedTick();

        _clock.FixedDeltaTime = _config.LaunchLandingStabilizationSeconds + 0.01f;

        FixedTick();

        _clock.FixedDeltaTime = previousFixedDeltaTime;
        _surfaceContextSource.Current = previousSurfaceContext;
        _steeringTarget.LinearVelocity = previousVelocity;
        _steeringTarget.Rotation = previousRotation;
        _steeringTarget.ResetApplyCallCounts();
        _steeringFrameSource.ResetFixedTickCallCounts();
    }

    protected SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent(Vector3 upDirection)
    {
        return CreateLaunchAppliedEvent(
            upDirection,
            Vector3.forward * DefaultPlanarSpeed + upDirection.normalized * DefaultVerticalSpeed);
    }

    protected SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent(Vector3 upDirection, Vector3 velocityChange)
    {
        var request = new SlingshotLaunchRequest(
            pullStrength: 1f,
            pullDistance: 1f,
            pullOffset: 0f,
            normalizedLateralPull: 0f,
            Vector3.zero,
            Vector3.forward,
            Vector3.up);

        return new SlingshotLaunchAppliedEvent(
            request,
            velocityChange,
            Vector3.forward,
            upDirection.normalized);
    }

    protected void AssertResolved(GameplayStatId statId, float baseValue)
    {
        Assert.That(
            _statResolver.ResolveRequests.Exists(request =>
                request.StatId == statId &&
                Mathf.Abs(request.BaseValue - baseValue) <= 0.0001f),
            Is.True);
    }

    protected void AssertPlanarAndVerticalSpeedPreserved(Vector3 velocity)
    {
        Assert.That(velocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(amount: 0.0001f));
        AssertPlanarSpeed(velocity, DefaultPlanarSpeed);
    }

    protected void AssertPlanarSpeed(Vector3 velocity, float expectedPlanarSpeed)
    {
        Assert.That(
            new Vector3(velocity.x, y: 0f, velocity.z).magnitude,
            Is.EqualTo(expectedPlanarSpeed).Within(amount: 0.0001f));
    }

    protected void AssertSpeedComponentsPreservedAround(Vector3 velocity, Vector3 upDirection)
    {
        var normalizedUpDirection = upDirection.normalized;
        var planarVelocity = ProjectPlanar(velocity, normalizedUpDirection);

        Assert.That(Vector3.Dot(velocity, normalizedUpDirection), Is.EqualTo(DefaultVerticalSpeed).Within(amount: 0.0001f));
        Assert.That(planarVelocity.magnitude, Is.EqualTo(DefaultPlanarSpeed).Within(amount: 0.0001f));
    }

    protected Vector3 ProjectPlanar(Vector3 velocity, Vector3 upDirection)
    {
        return velocity - Vector3.Project(velocity, upDirection.normalized);
    }

    protected void SetGroundedSurface(
        Vector3 groundNormal,
        RunSurfaceTransition transition = RunSurfaceTransition.None)
    {
        _surfaceContextSource.Current = new RunSurfaceContext(isGrounded: true, groundNormal, forwardDownhillDegrees: 0f);
        _surfaceContextSource.Transition = transition;
    }

    protected void SetUngroundedSurface(RunSurfaceTransition transition = RunSurfaceTransition.None)
    {
        _surfaceContextSource.Current = new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f);
        _surfaceContextSource.Transition = transition;
    }

    protected void AssertVectorEqual(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(amount: 0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(amount: 0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(amount: 0.0001f));
    }

    private RunSteeringInputController CreateInputController()
    {
        return new RunSteeringInputController(
            _input,
            _stateService,
            _launchAppliedNotifier,
            _config,
            _statResolver,
            _screen,
            _runSteeringGesture,
            _runSteeringAffordanceLayout,
            _runSteeringAffordancePresenter,
            _runSteeringPointerPressGuard,
            _runningStateId,
            _playerSteeringResponsivenessStatId);
    }

    private RunBodyMovementController CreateMovementController(IRunSteeringInputSource inputSource)
    {
        return new RunBodyMovementController(
            _stateService,
            _launchAppliedNotifier,
            _steeringTarget,
            inputSource,
            new DefaultRunBodySpeedEvaluator(_config),
            new DefaultRunSteeringEvaluator(),
            new RunLaunchLandingStabilizer(_config),
            _steeringFrameSource,
            _steeringFrameSource,
            _surfaceContextSource,
            _runProgressService,
            _statResolver,
            new RunBodySpeedDiagnostics(),
            _config,
            _config,
            _config,
            new RunBodySpeedEnvelopeValidator(_config),
            _clock,
            _playerMaxSpeedStatId,
            _runningStateId);
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);

        return stateId;
    }

    private GameplayStatId CreateStatId(string id)
    {
        var statId = ScriptableObject.CreateInstance<GameplayStatId>();
        statId.SetValuesForTests(id);
        _objects.Add(statId);

        return statId;
    }

    protected sealed class FakeUnityInput : IUnityInput
    {
        public int ActiveHandleCount { get; private set; }
        public event Action<PointerInput> PointerCanceled;
        public event Action<PointerInput> PointerMoved;

        public event Action<PointerInput> PointerPressed;
        public event Action<PointerInput> PointerReleased;

        public IDisposable Enable()
        {
            ActiveHandleCount += 1;
            return new EnableHandle(this);
        }

        public void Press(int pointerId, Vector2 screenPosition)
        {
            PointerPressed?.Invoke(new PointerInput(pointerId, screenPosition));
        }

        public void Move(int pointerId, Vector2 screenPosition)
        {
            PointerMoved?.Invoke(new PointerInput(pointerId, screenPosition));
        }

        public void Release(int pointerId, Vector2 screenPosition)
        {
            PointerReleased?.Invoke(new PointerInput(pointerId, screenPosition));
        }

        public void Cancel(int pointerId, Vector2 screenPosition)
        {
            PointerCanceled?.Invoke(new PointerInput(pointerId, screenPosition));
        }

        private sealed class EnableHandle : IDisposable
        {
            private FakeUnityInput _owner;

            public EnableHandle(FakeUnityInput owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                var owner = _owner;

                if (owner is null)
                    return;

                _owner = null;
                owner.ActiveHandleCount -= 1;
            }
        }
    }

    protected sealed class FakeGameplayStateService : IGameplayStateService
    {
        public GameplayStateId CurrentStateId { get; private set; }
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;

        public FakeGameplayStateService(GameplayStateId currentStateId)
        {
            CurrentStateId = currentStateId;
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

    protected sealed class FakeSlingshotLaunchAppliedNotifier : ISlingshotLaunchAppliedNotifier
    {
        public event Action<SlingshotLaunchAppliedEvent> LaunchApplied;

        public void Apply(SlingshotLaunchAppliedEvent appliedEvent)
        {
            LaunchApplied?.Invoke(appliedEvent);
        }
    }

    protected sealed class FakeRunBodyMovementTarget : IRunBodyMovementTarget
    {
        public int ApplyCallCount { get; private set; }
        public int ApplyVelocityCallCount { get; private set; }
        public Vector3 LinearVelocity { get; set; }
        public Quaternion Rotation { get; set; }

        public void ApplyTargetState(RunBodyMovementTargetState targetState)
        {
            LinearVelocity = targetState.LinearVelocity;

            if (targetState.HasRotation)
            {
                Rotation = targetState.Rotation;
                ApplyCallCount += 1;
            }
            else
                ApplyVelocityCallCount += 1;
        }

        public void ResetApplyCallCounts()
        {
            ApplyVelocityCallCount = 0;
            ApplyCallCount = 0;
        }
    }

    protected sealed class FakeRunBodyMovementConfig :
        IRunBodySpeedConfig,
        IRunSteeringConfig,
        IRunBodyMovementValidityConfig,
        IRunLaunchLandingStabilizationConfig
    {
        public float AboveMaximumSpeedResistance { get; set; }
        public float BaseSoftMaximumSpeed { get; set; }
        public float DownhillAcceleration { get; set; }
        public float FallbackDpi { get; set; }
        public float LaunchLandingMaximumLiftSpeed { get; set; }
        public float LaunchLandingStabilizationSeconds { get; set; }
        public float LowSpeedAssistAcceleration { get; set; }
        public float LowSpeedAssistTargetSpeed { get; set; }
        public float MaximumAcceptedDpi { get; set; }
        public float MaximumSupportedSurfaceNormalLiftSpeed { get; set; }
        public float MaximumTurnDegreesPerSecond { get; set; }
        public float MinimumAcceptedDpi { get; set; }
        public float MinimumSteerSpeed { get; set; }
        public float RunAirSteeringMaximumTurnDegreesPerSecond { get; set; }
        public float RunBodySpeedSanityGuardMetersPerSecond { get; set; }
        public float RunSteeringDeadzoneFraction { get; set; }
        public float RunSteeringRangeCentimeters { get; set; }
        public float RunSteeringResponsiveness { get; set; }
        public float SurfaceSlowdown { get; set; }
    }

    protected sealed class RecordingRunSteeringInputMetricsResolver : IRunSteeringInputMetricsResolver
    {
        private readonly DefaultRunSteeringInputMetricsResolver _resolver;

        public List<float> RawDpiRequests { get; } = new();

        public RecordingRunSteeringInputMetricsResolver(IRunSteeringConfig config)
        {
            _resolver = new DefaultRunSteeringInputMetricsResolver(config);
        }

        public RunSteeringInputMetrics Resolve(float rawDpi)
        {
            RawDpiRequests.Add(rawDpi);
            return _resolver.Resolve(rawDpi);
        }
    }

    protected sealed class FakeRunSurfaceFrameSource : IRunSurfaceFrameSource
    {
        public RunSurfaceContext Current { get; set; }

        RunSurfaceFrameSnapshot IRunSurfaceFrameSource.Current =>
            new(
                observedSupport: default,
                Current,
                Transition,
                isMissingSupportHeld: false,
                isConfirmingDiscontinuity: false,
                steeringFrame: default);

        public RunSurfaceTransition Transition { get; set; }
    }

    protected sealed class FakeRunProgressService : IRunProgressService
    {
        public float CurrentForwardProgress => 0f;
        public RunProgressSample CurrentSample => default;
        public bool HasValidSnapshot => false;
        public float MaximumForwardProgress => 0f;
        public RunProgressFrameSnapshot Snapshot => default;
        public string SnapshotError => string.Empty;

        public bool TryBeginRun(Vector3 origin, out string error)
        {
            error = string.Empty;
            return false;
        }

        public void SamplePosition(Vector3 position)
        {
        }

        public void Reset()
        {
        }
    }

    protected sealed class FakeRunGameplayStatResolver : IRunGameplayStatResolver
    {
        private readonly Dictionary<GameplayStatId, float> _resolvedValues = new();

        public List<ResolveRequest> ResolveRequests { get; } = new();

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            ResolveRequests.Add(new ResolveRequest(statId, baseValue));
            return _resolvedValues.GetValueOrDefault(statId, baseValue);
        }

        public void SetResolvedValue(GameplayStatId statId, float resolvedValue)
        {
            _resolvedValues[statId] = resolvedValue;
        }
    }

    protected readonly struct ResolveRequest
    {
        public GameplayStatId StatId { get; }
        public float BaseValue { get; }

        public ResolveRequest(GameplayStatId statId, float baseValue)
        {
            StatId = statId;
            BaseValue = baseValue;
        }
    }

    protected sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }

    protected sealed class FakeScreen : IScreen
    {
        public float Dpi { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }

    protected sealed class FakeRunSteeringFrameSource : IRunSteeringFrameSource, IRunSteeringFrameResetter
    {
        public int GetUpDirectionCallCount { get; private set; }
        public Vector3 LastFallbackUpDirection { get; private set; }
        public Vector3 LastResetLaunchUpDirection { get; private set; }
        public int ResetCallCount { get; private set; }
        public Vector3 UpDirection { get; set; } = Vector3.up;

        public void Reset(Vector3 launchUpDirection)
        {
            LastResetLaunchUpDirection = launchUpDirection;
            ResetCallCount += 1;
        }

        public void Clear()
        {
        }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            LastFallbackUpDirection = fallbackUpDirection;
            GetUpDirectionCallCount += 1;

            return UpDirection;
        }

        public void ResetFixedTickCallCounts()
        {
            LastFallbackUpDirection = Vector3.zero;
            GetUpDirectionCallCount = 0;
        }
    }

    protected sealed class FakeRunSteeringAffordanceLayout : IRunSteeringAffordanceLayout
    {
        internal RunSteeringAffordancePresentationState Result { get; }

        internal List<RunSteeringAffordanceSnapshot> Snapshots { get; } = new();

        public FakeRunSteeringAffordanceLayout()
        {
            Result = new RunSteeringAffordancePresentationState(
                isVisible: true,
                new Vector2(x: 11f, y: 12f),
                new Vector2(x: 21f, y: 22f),
                new Vector2(x: 31f, y: 32f),
                new Vector2(x: 41f, y: 42f),
                deadzoneDiameterPixels: 51f,
                leftRangeEndAlphaMultiplier: 0f,
                rightRangeEndAlphaMultiplier: 0f);
        }

        RunSteeringAffordancePresentationState IRunSteeringAffordanceLayout.Create(RunSteeringAffordanceSnapshot snapshot)
        {
            Snapshots.Add(snapshot);
            return Result;
        }
    }

    protected sealed class FakeRunSteeringAffordancePresenter : IRunSteeringAffordancePresenter
    {
        internal List<RunSteeringAffordancePresentationState> HideStates { get; } = new();
        public int ResetCallCount { get; private set; }
        internal List<RunSteeringAffordancePresentationState> ShowStates { get; } = new();
        internal List<RunSteeringAffordancePresentationState> UpdateStates { get; } = new();

        void IRunSteeringAffordancePresenter.Show(RunSteeringAffordancePresentationState state)
        {
            ShowStates.Add(state);
        }

        void IRunSteeringAffordancePresenter.Update(RunSteeringAffordancePresentationState state)
        {
            UpdateStates.Add(state);
        }

        void IRunSteeringAffordancePresenter.Hide(RunSteeringAffordancePresentationState state)
        {
            HideStates.Add(state);
        }

        public void Reset()
        {
            ResetCallCount += 1;
        }
    }

    protected sealed class FakeRunSteeringPointerPressGuard : IRunSteeringPointerPressGuard
    {
        public bool CanBegin { get; set; } = true;
        public List<PointerInput> Requests { get; } = new();

        public bool CanBeginRunSteering(PointerInput pointerInput)
        {
            Requests.Add(pointerInput);
            return CanBegin;
        }
    }
}
