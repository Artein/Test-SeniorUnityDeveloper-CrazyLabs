using System;
using System.Collections.Generic;
using Game.Foundation.Input;
using Game.Foundation.Screen;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunSteeringInputControllerTests
{
    private readonly List<UnityEngine.Object> _objects = new();

    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private GameplayStatId _responsivenessStatId;
    private FakeUnityInput _input;
    private FakeGameplayStateService _stateService;
    private FakeLaunchAppliedNotifier _launchNotifier;
    private FakeSteeringConfig _config;
    private FakeStatResolver _statResolver;
    private FakeScreen _screen;
    private IRunSteeringGesture _gesture;
    private FakeAffordancePresenter _presenter;
    private RunSteeringInputController _controller;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId("PreLaunch");
        _runningStateId = CreateStateId("Running");
        _responsivenessStatId = CreateStatId("PlayerSteeringResponsiveness");
        _input = new FakeUnityInput();
        _stateService = new FakeGameplayStateService(_preLaunchStateId);
        _launchNotifier = new FakeLaunchAppliedNotifier();
        _config = new FakeSteeringConfig { RunSteeringResponsiveness = 2f };
        _statResolver = new FakeStatResolver();
        _screen = new FakeScreen { Dpi = 100f };
        _gesture = new RunSteeringGesture(new FixedMetricsResolver());
        _presenter = new FakeAffordancePresenter();

        _controller = new RunSteeringInputController(
            _input,
            _stateService,
            _launchNotifier,
            _config,
            _statResolver,
            _screen,
            _gesture,
            new PassThroughAffordanceLayout(),
            _presenter,
            new AllowPointerPressGuard(),
            _runningStateId,
            _responsivenessStatId);
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
    public void LaunchBeforeRunning_EnteringRunningEnablesInput()
    {
        _launchNotifier.Apply(CreateLaunchAppliedEvent());

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        AssertNeutral(_controller.AdvanceAndRead(0.02f));

        _stateService.ChangeTo(_runningStateId);

        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    [Test]
    public void RunningBeforeLaunch_LaunchEnablesInput()
    {
        _stateService.ChangeTo(_runningStateId);

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        AssertNeutral(_controller.AdvanceAndRead(0.02f));

        _launchNotifier.Apply(CreateLaunchAppliedEvent());

        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    [Test]
    public void AdvanceAndRead_ActiveGesture_ReturnsOneCoherentSmoothedSnapshot()
    {
        ActivateInput();
        _input.Press(1, Vector2.zero);
        _input.Move(1, Vector2.right * 100f);

        var state = _controller.AdvanceAndRead(0.02f);

        Assert.That(state.IsGestureActive, Is.True);
        Assert.That(state.DesiredSteer, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(state.SmoothedSteer, Is.EqualTo(0.04f).Within(0.0001f));
        Assert.That(state.HasCapturedMetrics, Is.True);
        Assert.That(state.CapturedMetrics.ResolvedDpi, Is.EqualTo(100f));
        Assert.That(state.CapturedMetrics.RangePixels, Is.EqualTo(100f));
        Assert.That(state.CapturedMetrics.DeadzoneFraction, Is.Zero);
    }

    [Test]
    public void AdvanceAndRead_ResponsivenessStat_ControlsSmoothing()
    {
        _statResolver.ResolvedValue = 10f;
        ActivateInput();
        _input.Press(1, Vector2.zero);
        _input.Move(1, Vector2.right * 100f);

        var state = _controller.AdvanceAndRead(0.02f);

        Assert.That(state.SmoothedSteer, Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(_statResolver.LastStatId, Is.EqualTo(_responsivenessStatId));
        Assert.That(_statResolver.LastBaseValue, Is.EqualTo(2f));
    }

    [Test]
    public void LeavingRunning_DisablesInputAndReturnsNeutralState()
    {
        ActivateInput();
        _input.Press(1, Vector2.zero);
        _input.Move(1, Vector2.right * 100f);
        _controller.AdvanceAndRead(0.02f);

        _stateService.ChangeTo(_preLaunchStateId);
        var state = _controller.AdvanceAndRead(0.02f);

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        AssertNeutral(state);
        Assert.That(_presenter.ResetCallCount, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public void NewLaunchAfterRunExit_RequiresRunningAgain()
    {
        ActivateInput();
        _stateService.ChangeTo(_preLaunchStateId);
        _launchNotifier.Apply(CreateLaunchAppliedEvent());

        Assert.That(_input.ActiveHandleCount, Is.Zero);

        _stateService.ChangeTo(_runningStateId);

        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    private void ActivateInput()
    {
        _stateService.ChangeTo(_runningStateId);
        _launchNotifier.Apply(CreateLaunchAppliedEvent());
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    private void AssertNeutral(RunSteeringInputState state)
    {
        Assert.That(state.IsGestureActive, Is.False);
        Assert.That(state.DesiredSteer, Is.Zero);
        Assert.That(state.SmoothedSteer, Is.Zero);
        Assert.That(state.HasCapturedMetrics, Is.False);
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

    private GameplayStateId CreateStateId(string name)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = name;
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

    private sealed class FakeUnityInput : IUnityInput
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

        public void Press(int pointerId, Vector2 position)
        {
            PointerPressed?.Invoke(new PointerInput(pointerId, position));
        }

        public void Move(int pointerId, Vector2 position)
        {
            PointerMoved?.Invoke(new PointerInput(pointerId, position));
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

    private sealed class FakeSteeringConfig : IRunSteeringConfig
    {
        public float RunSteeringRangeCentimeters => 2.54f;
        public float RunSteeringDeadzoneFraction => 0f;
        public float RunSteeringResponsiveness { get; set; }
        public float FallbackDpi => 100f;
        public float MinimumAcceptedDpi => 1f;
        public float MaximumAcceptedDpi => 1000f;
        public float MaximumTurnDegreesPerSecond => 90f;
        public float RunAirSteeringMaximumTurnDegreesPerSecond => 30f;
        public float MinimumSteerSpeed => 0.25f;
    }

    private sealed class FakeStatResolver : IRunGameplayStatResolver
    {
        public float ResolvedValue { get; set; } = float.NaN;
        public GameplayStatId LastStatId { get; private set; }
        public float LastBaseValue { get; private set; }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            LastStatId = statId;
            LastBaseValue = baseValue;
            return float.IsNaN(ResolvedValue) ? baseValue : ResolvedValue;
        }
    }

    private sealed class FakeScreen : IScreen
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float Dpi { get; set; }
    }

    private sealed class FixedMetricsResolver : IRunSteeringInputMetricsResolver
    {
        public RunSteeringInputMetrics Resolve(float rawDpi)
        {
            return new RunSteeringInputMetrics(rawDpi, 100f, 0f);
        }
    }

    private sealed class PassThroughAffordanceLayout : IRunSteeringAffordanceLayout
    {
        public RunSteeringAffordancePresentationState Create(RunSteeringAffordanceSnapshot snapshot)
        {
            return new RunSteeringAffordancePresentationState(
                snapshot.IsActive,
                snapshot.OriginScreenPosition,
                snapshot.CurrentScreenPosition,
                snapshot.OriginScreenPosition,
                snapshot.CurrentScreenPosition,
                snapshot.CapturedRangePixels,
                0f,
                0f);
        }
    }

    private sealed class FakeAffordancePresenter : IRunSteeringAffordancePresenter
    {
        public int ResetCallCount { get; private set; }

        public void Show(RunSteeringAffordancePresentationState state)
        {
        }

        public void Update(RunSteeringAffordancePresentationState state)
        {
        }

        public void Hide(RunSteeringAffordancePresentationState state)
        {
        }

        public void Reset()
        {
            ResetCallCount += 1;
        }
    }

    private sealed class AllowPointerPressGuard : IRunSteeringPointerPressGuard
    {
        public bool CanBeginRunSteering(PointerInput pointerInput)
        {
            return true;
        }
    }
}
