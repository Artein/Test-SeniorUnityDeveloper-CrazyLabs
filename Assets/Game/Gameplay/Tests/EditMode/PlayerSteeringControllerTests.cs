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
public sealed class PlayerSteeringControllerTests
{
    private const float DefaultPlanarSpeed = 10f, DefaultVerticalSpeed = 2f;

    private readonly List<UnityEngine.Object> _objects = new();
    private FakeUnityInput _input;
    private FakeGameplayStateService _stateService;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakePlayerSteeringTarget _steeringTarget;
    private FakePlayerSteeringConfig _config;
    private FakeRunGameplayStatResolver _statResolver;
    private FakeTime _clock;
    private FakeScreen _screen;
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private GameplayStatId _playerMaxSpeedStatId;
    private GameplayStatId _playerSteeringResponsivenessStatId;
    private PlayerSteeringController _controller;

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

        _steeringTarget = new FakePlayerSteeringTarget
        {
            LinearVelocity = new Vector3(0f, DefaultVerticalSpeed, DefaultPlanarSpeed),
            Rotation = Quaternion.identity
        };

        _config = new FakePlayerSteeringConfig
        {
            SteeringDeadzone = 0.1f,
            SteeringSensitivity = 1f,
            SteeringResponseRate = 100f,
            MaximumTurnDegreesPerSecond = 90f,
            MinimumSteerSpeed = 0.25f,
            MaximumPlanarSpeed = DefaultPlanarSpeed
        };

        _clock = new FakeTime
        {
            DeltaTime = 0.02f,
            FixedDeltaTime = 0.02f
        };

        _screen = new FakeScreen
        {
            Width = 1000,
            Height = 2000
        };
        _controller = CreateController();
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
    public void BeforeLaunch_DoesNotEnableInputOrSteer()
    {
        _input.Press(1, new Vector2(1000f, 100f));

        ((IFixedTickable)_controller).FixedTick();

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
        ((IFixedTickable)_controller).FixedTick();

        Assert.That(_input.ActiveHandleCount, Is.Zero);
        Assert.That(_steeringTarget.ApplyCallCount, Is.Zero);
    }

    [Test]
    public void FixedTick_LeftTouch_TurnsVelocityLeftAndPreservesSpeedComponents()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(0f, 100f));
        ((IFixedTickable)_controller).FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.LessThan(0f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_RightTouch_TurnsVelocityRightAndPreservesSpeedComponents()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(1000f, 100f));
        ((IFixedTickable)_controller).FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(0f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_NeutralMovementStats_ResolvesBaseValuesAndPreservesMovement()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(1000f, 100f));
        ((IFixedTickable)_controller).FixedTick();

        AssertResolved(_playerMaxSpeedStatId, DefaultPlanarSpeed);
        AssertResolved(_playerSteeringResponsivenessStatId, 100f);
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_PlayerMaxSpeedModifier_ClampsPlanarSpeed()
    {
        _statResolver.SetResolvedValue(_playerMaxSpeedStatId, 5f);
        ActivateSteering();

        ((IFixedTickable)_controller).FixedTick();

        Assert.That(_steeringTarget.LinearVelocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(_steeringTarget.LinearVelocity, 5f);
    }

    [Test]
    public void FixedTick_PlayerSteeringResponsivenessModifier_IncreasesSteeringResponse()
    {
        _config.SteeringResponseRate = 5f;
        _statResolver.SetResolvedValue(_playerSteeringResponsivenessStatId, 20f);
        ActivateSteering();

        _input.Press(1, new Vector2(1000f, 100f));
        ((IFixedTickable)_controller).FixedTick();

        AssertResolved(_playerSteeringResponsivenessStatId, 5f);
        Assert.That(_steeringTarget.LinearVelocity.x, Is.GreaterThan(0.1f));
        AssertPlanarAndVerticalSpeedPreserved(_steeringTarget.LinearVelocity);
    }

    [Test]
    public void FixedTick_CenterDeadzoneTouch_DoesNotChangeVelocity()
    {
        ActivateSteering();
        var initialVelocity = _steeringTarget.LinearVelocity;

        _input.Press(1, new Vector2(525f, 100f));
        ((IFixedTickable)_controller).FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, initialVelocity);
    }

    [Test]
    public void PointerReleased_ReturnsDesiredSteeringToZero()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(1000f, 100f));
        ((IFixedTickable)_controller).FixedTick();
        var steeredVelocity = _steeringTarget.LinearVelocity;

        _input.Release(1, new Vector2(1000f, 100f));
        ((IFixedTickable)_controller).FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, steeredVelocity);
    }

    [Test]
    public void PointerCanceled_ReturnsDesiredSteeringToZero()
    {
        ActivateSteering();

        _input.Press(1, new Vector2(0f, 100f));
        ((IFixedTickable)_controller).FixedTick();
        var steeredVelocity = _steeringTarget.LinearVelocity;

        _input.Cancel(1, new Vector2(0f, 100f));
        ((IFixedTickable)_controller).FixedTick();

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
        ((IFixedTickable)_controller).FixedTick();

        AssertVectorEqual(_steeringTarget.LinearVelocity, initialVelocity);
    }

    private PlayerSteeringController CreateController()
    {
        return new PlayerSteeringController(
            _input,
            _stateService,
            _launchAppliedNotifier,
            _steeringTarget,
            _config,
            _statResolver,
            _clock,
            _screen,
            _runningStateId,
            _playerMaxSpeedStatId,
            _playerSteeringResponsivenessStatId);
    }

    private void ActivateSteering()
    {
        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.up));
        Assert.That(_input.ActiveHandleCount, Is.EqualTo(1));
    }

    private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent(Vector3 upDirection)
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
            Vector3.forward * DefaultPlanarSpeed + upDirection.normalized * DefaultVerticalSpeed,
            Vector3.forward,
            upDirection.normalized);
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

    private void AssertResolved(GameplayStatId statId, float baseValue)
    {
        Assert.That(
            _statResolver.ResolveRequests.Exists(request =>
                ReferenceEquals(request.StatId, statId) &&
                Mathf.Abs(request.BaseValue - baseValue) <= 0.0001f),
            Is.True);
    }

    private void AssertPlanarAndVerticalSpeedPreserved(Vector3 velocity)
    {
        Assert.That(velocity.y, Is.EqualTo(DefaultVerticalSpeed).Within(0.0001f));
        AssertPlanarSpeed(velocity, DefaultPlanarSpeed);
    }

    private void AssertPlanarSpeed(Vector3 velocity, float expectedPlanarSpeed)
    {
        Assert.That(new Vector3(velocity.x, 0f, velocity.z).magnitude, Is.EqualTo(expectedPlanarSpeed).Within(0.0001f));
    }

    private void AssertVectorEqual(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
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

    private sealed class FakeGameplayStateService : IGameplayStateService
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
            return ReferenceEquals(CurrentStateId, stateId);
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

    private sealed class FakeSlingshotLaunchAppliedNotifier : ISlingshotLaunchAppliedNotifier
    {
        public event Action<SlingshotLaunchAppliedEvent> LaunchApplied;

        public void Apply(SlingshotLaunchAppliedEvent appliedEvent)
        {
            LaunchApplied?.Invoke(appliedEvent);
        }
    }

    private sealed class FakePlayerSteeringTarget : IPlayerSteeringTarget
    {
        public Vector3 LinearVelocity { get; set; }
        public Quaternion Rotation { get; set; }
        public int ApplyCallCount { get; private set; }

        public void ApplySteering(Vector3 linearVelocity, Quaternion rotation)
        {
            LinearVelocity = linearVelocity;
            Rotation = rotation;
            ApplyCallCount += 1;
        }
    }

    private sealed class FakePlayerSteeringConfig : IPlayerSteeringConfig
    {
        public float SteeringDeadzone { get; set; }
        public float SteeringSensitivity { get; set; }
        public float SteeringResponseRate { get; set; }
        public float MaximumTurnDegreesPerSecond { get; set; }
        public float MinimumSteerSpeed { get; set; }
        public float MaximumPlanarSpeed { get; set; }
    }

    private sealed class FakeRunGameplayStatResolver : IRunGameplayStatResolver
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

    private readonly struct ResolveRequest
    {
        public GameplayStatId StatId { get; }
        public float BaseValue { get; }

        public ResolveRequest(GameplayStatId statId, float baseValue)
        {
            StatId = statId;
            BaseValue = baseValue;
        }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }

    private sealed class FakeScreen : IScreen
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
