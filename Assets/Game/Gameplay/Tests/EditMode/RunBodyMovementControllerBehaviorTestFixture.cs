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

// ReSharper disable once CheckNamespace
public abstract class RunBodyMovementControllerBehaviorTestFixture
{
    protected const float DefaultPlanarSpeed = 10f, DefaultVerticalSpeed = 2f;

    private readonly List<UnityEngine.Object> _objects = new();

    protected FakeUnityInput _input;
    protected FakeGameplayStateService _stateService;
    protected FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    protected FakeRunBodyMovementTarget _steeringTarget;
    protected FakeRunBodyMovementConfig _config;
    protected FakeRunSurfaceContextSource _surfaceContextSource;
    protected FakeRunProgressService _runProgressService;
    protected FakeRunGameplayStatResolver _statResolver;
    protected FakeTime _clock;
    protected FakeScreen _screen;
    protected FakeRunSteeringFrameSource _steeringFrameSource;
    protected FakeRunSteeringAffordanceLayout _runSteeringAffordanceLayout;
    protected FakeRunSteeringAffordancePresenter _runSteeringAffordancePresenter;
    protected FakeRunSteeringPointerPressGuard _runSteeringPointerPressGuard;
    protected GameplayStateId _preLaunchStateId;
    protected GameplayStateId _runningStateId;
    protected GameplayStatId _playerMaxSpeedStatId;
    protected GameplayStatId _playerSteeringResponsivenessStatId;
    private RunSteeringInputController _inputController;
    private RunBodyMovementController _movementController;
    private IRunSteeringGesture _runSteeringGesture;
    protected RecordingRunSteeringInputMetricsResolver _inputMetricsResolver;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId("PreLaunch");
        _runningStateId = CreateStateId("Running");
        _playerMaxSpeedStatId = CreateStatId("PlayerMaxSpeed");
        _playerSteeringResponsivenessStatId = CreateStatId("PlayerSteeringResponsiveness");
        _input = new FakeUnityInput();
        _stateService = new FakeGameplayStateService(_preLaunchStateId);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _statResolver = new FakeRunGameplayStatResolver();

        _steeringTarget = new FakeRunBodyMovementTarget
        {
            LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, DefaultPlanarSpeed),
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

        _surfaceContextSource = new FakeRunSurfaceContextSource
        {
            Current = new RunSurfaceContext(false, Vector3.up, 0f)
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
            UnityEngine.Object.DestroyImmediate(unityObject);
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
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
        SettleLaunchStateForSteadySteering(launchUpDirection);
    }

    protected void ActivateSteeringWithLaunchVelocity(Vector3 launchVelocityChange)
    {
        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.up, launchVelocityChange));
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    protected void FixedTick()
    {
        ((IFixedTickable)_movementController).FixedTick();
    }

    private void SettleLaunchStateForSteadySteering(Vector3 launchUpDirection)
    {
        var previousFixedDeltaTime = _clock.FixedDeltaTime;
        var previousSurfaceContext = _surfaceContextSource.Current;
        var previousVelocity = _steeringTarget.LinearVelocity;
        var previousRotation = _steeringTarget.Rotation;

        SetGroundedSurface(launchUpDirection);
        FixedTick();
        SetUngroundedSurface();
        FixedTick();
        SetGroundedSurface(launchUpDirection);
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
            finalPullPoint: Vector3.zero,
            launchFrameForward: Vector3.forward,
            launchFrameUp: Vector3.up);

        return new SlingshotLaunchAppliedEvent(
            request,
            velocityChange,
            launchDirection: Vector3.forward,
            launchUpDirection: upDirection.normalized);
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
        Assert.That(velocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(velocity, DefaultPlanarSpeed);
    }

    protected void AssertPlanarSpeed(Vector3 velocity, float expectedPlanarSpeed)
    {
        Assert.That(new Vector3(velocity.x, 0f, velocity.z).magnitude, Is.EqualTo(expectedPlanarSpeed).Within(0.0001f));
    }

    protected void AssertSpeedComponentsPreservedAround(Vector3 velocity, Vector3 upDirection)
    {
        var normalizedUpDirection = upDirection.normalized;
        var planarVelocity = ProjectPlanar(velocity, normalizedUpDirection);

        Assert.That(Vector3.Dot(velocity, normalizedUpDirection), Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        Assert.That(planarVelocity.magnitude, Is.EqualTo(DefaultPlanarSpeed).Within(0.0001f));
    }

    protected Vector3 ProjectPlanar(Vector3 velocity, Vector3 upDirection)
    {
        return velocity - Vector3.Project(velocity, upDirection.normalized);
    }

    protected void SetGroundedSurface(Vector3 groundNormal)
    {
        _surfaceContextSource.Current = new RunSurfaceContext(true, groundNormal, 0f);
    }

    protected void SetUngroundedSurface()
    {
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);
    }

    protected void AssertVectorEqual(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
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

        public event Action<PointerInput> PointerPressed;
        public event Action<PointerInput> PointerMoved;
        public event Action<PointerInput> PointerReleased;
        public event Action<PointerInput> PointerCanceled;

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

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

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
        public Vector3 LinearVelocity { get; set; }
        public Quaternion Rotation { get; set; }
        public int ApplyVelocityCallCount { get; private set; }
        public int ApplyCallCount { get; private set; }

        public void ApplyTargetState(RunBodyMovementTargetState targetState)
        {
            LinearVelocity = targetState.LinearVelocity;

            if (targetState.HasRotation)
            {
                Rotation = targetState.Rotation;
                ApplyCallCount += 1;
            }
            else
            {
                ApplyVelocityCallCount += 1;
            }
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
        public float DownhillAcceleration { get; set; }
        public float SurfaceSlowdown { get; set; }
        public float LowSpeedAssistTargetSpeed { get; set; }
        public float LowSpeedAssistAcceleration { get; set; }
        public float BaseSoftMaximumSpeed { get; set; }
        public float AboveMaximumSpeedResistance { get; set; }
        public float RunSteeringRangeCentimeters { get; set; }
        public float RunSteeringDeadzoneFraction { get; set; }
        public float RunSteeringResponsiveness { get; set; }
        public float FallbackDpi { get; set; }
        public float MinimumAcceptedDpi { get; set; }
        public float MaximumAcceptedDpi { get; set; }
        public float MaximumTurnDegreesPerSecond { get; set; }
        public float RunAirSteeringMaximumTurnDegreesPerSecond { get; set; }
        public float MinimumSteerSpeed { get; set; }
        public float MaximumSupportedSurfaceNormalLiftSpeed { get; set; }
        public float RunBodySpeedSanityGuardMetersPerSecond { get; set; }
        public float LaunchLandingStabilizationSeconds { get; set; }
        public float LaunchLandingMaximumLiftSpeed { get; set; }
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

    protected sealed class FakeRunSurfaceContextSource : IRunSurfaceContextSource
    {
        public RunSurfaceContext Current { get; set; }
    }

    protected sealed class FakeRunProgressService : IRunProgressService
    {
        public bool HasValidSnapshot => false;
        public string SnapshotError => string.Empty;
        public RunProgressFrameSnapshot Snapshot => default;
        public float CurrentForwardProgress => 0f;
        public float MaximumForwardProgress => 0f;
        public RunProgressSample CurrentSample => default;

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

        public void SetResolvedValue(GameplayStatId statId, float resolvedValue)
        {
            _resolvedValues[statId] = resolvedValue;
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            ResolveRequests.Add(new ResolveRequest(statId, baseValue));
            return _resolvedValues.GetValueOrDefault(statId, baseValue);
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
        public int Width { get; set; }
        public int Height { get; set; }
        public float Dpi { get; set; }
    }

    protected sealed class FakeRunSteeringFrameSource : IRunSteeringFrameSource, IRunSteeringFrameResetter
    {
        public Vector3 UpDirection { get; set; } = Vector3.up;
        public Vector3 LastFallbackUpDirection { get; private set; }
        public Vector3 LastResetLaunchUpDirection { get; private set; }
        public int GetUpDirectionCallCount { get; private set; }
        public int ResetCallCount { get; private set; }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            LastFallbackUpDirection = fallbackUpDirection;
            GetUpDirectionCallCount += 1;

            return UpDirection;
        }

        public void Reset(Vector3 launchUpDirection)
        {
            LastResetLaunchUpDirection = launchUpDirection;
            ResetCallCount += 1;
        }

        public void Clear()
        {
        }

        public void ResetFixedTickCallCounts()
        {
            LastFallbackUpDirection = Vector3.zero;
            GetUpDirectionCallCount = 0;
        }
    }

    protected sealed class FakeRunSteeringAffordanceLayout : IRunSteeringAffordanceLayout
    {
        public FakeRunSteeringAffordanceLayout()
        {
            Result = new RunSteeringAffordancePresentationState(
                true,
                new Vector2(11f, 12f),
                new Vector2(21f, 22f),
                new Vector2(31f, 32f),
                new Vector2(41f, 42f),
                51f,
                0f,
                0f);
        }

        internal List<RunSteeringAffordanceSnapshot> Snapshots { get; } = new();
        internal RunSteeringAffordancePresentationState Result { get; }

        RunSteeringAffordancePresentationState IRunSteeringAffordanceLayout.Create(RunSteeringAffordanceSnapshot snapshot)
        {
            Snapshots.Add(snapshot);
            return Result;
        }
    }

    protected sealed class FakeRunSteeringAffordancePresenter : IRunSteeringAffordancePresenter
    {
        internal List<RunSteeringAffordancePresentationState> ShowStates { get; } = new();
        internal List<RunSteeringAffordancePresentationState> UpdateStates { get; } = new();
        internal List<RunSteeringAffordancePresentationState> HideStates { get; } = new();
        public int ResetCallCount { get; private set; }

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
