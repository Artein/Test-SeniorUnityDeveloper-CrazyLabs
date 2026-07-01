using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunCameraControllerTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private FakeGameplayStateService _stateService;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeRunCameraSource _source;
    private FakeRunCameraAnchor _anchor;
    private FakeRunCameraRig _rig;
    private FakeRunCameraConfig _config;
    private FakeTime _clock;
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private GameplayStateId _runEndedStateId;
    private RunCameraController _controller;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("RunPreparation");
        _preLaunchStateId = CreateStateId("PreLaunch");
        _runningStateId = CreateStateId("Running");
        _runEndedStateId = CreateStateId("RunEnded");
        _stateService = new FakeGameplayStateService(_runPreparationStateId);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();

        _source = new FakeRunCameraSource
        {
            Position = new Vector3(1f, 0f, 2f),
            LinearVelocity = Vector3.forward * 10f
        };

        _anchor = new FakeRunCameraAnchor
        {
            Position = Vector3.zero,
            Rotation = Quaternion.identity
        };
        _rig = new FakeRunCameraRig();

        _config = new FakeRunCameraConfig
        {
            AnchorOffset = new Vector3(0f, 1.5f, 0f),
            PositionResponseRate = 10f,
            YawResponseRate = 10f,
            MinimumYawSpeed = 0.25f
        };

        _clock = new FakeTime
        {
            DeltaTime = 0.1f,
            FixedDeltaTime = 0.02f
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
    public void Initialize_ActivatesRunPreparationCameraAndPrimesAnchor()
    {
        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.RunPreparation));
        Assert.That(_anchor.Position, Is.EqualTo(_source.Position + _config.AnchorOffset));
        AssertForward(_anchor.Rotation, Vector3.forward);
    }

    [Test]
    public void PreLaunch_ActivatesPreLaunchCamera()
    {
        _stateService.ChangeTo(_preLaunchStateId);

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.PreLaunch));
    }

    [Test]
    public void RunningWithoutLaunchApplied_DoesNotActivateRunCamera()
    {
        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);
        ((ILateTickable)_controller).LateTick();

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.PreLaunch));
    }

    [Test]
    public void LaunchApplied_WhenRunning_ActivatesRunCamera()
    {
        _stateService.ChangeTo(_runningStateId);

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.forward, Vector3.up));

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.Run));
    }

    [Test]
    public void LaunchAppliedOutsideRunning_ActivatesWhenRunningIsReached()
    {
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.forward, Vector3.up));

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.RunPreparation));

        _stateService.ChangeTo(_runningStateId);

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.Run));
    }

    [Test]
    public void PreLaunchAfterRunning_DeactivatesRunCameraAndClearsLaunchGate()
    {
        ActivateRunCamera();

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.PreLaunch));
    }

    [Test]
    public void RunEndedAfterRunning_KeepsRunCameraActiveAndLaunchGate()
    {
        ActivateRunCamera();

        _stateService.ChangeTo(_runEndedStateId);
        _stateService.ChangeTo(_runningStateId);

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.Run));
    }

    [Test]
    public void RunPreparationAfterRunEnded_ActivatesRunPreparationCameraAndClearsLaunchGate()
    {
        ActivateRunCamera();

        _stateService.ChangeTo(_runEndedStateId);
        _stateService.ChangeTo(_runPreparationStateId);
        _stateService.ChangeTo(_runningStateId);

        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.RunPreparation));
    }

    [Test]
    public void RepeatedLaunchAndStateEvents_AreIdempotent()
    {
        ActivateRunCamera();
        var activationCallCount = _rig.ActivationCallCount;

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.forward, Vector3.up));
        _stateService.ChangeTo(_runningStateId);

        Assert.That(_rig.ActivationCallCount, Is.EqualTo(activationCallCount));
        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.Run));
    }

    [Test]
    public void LateTick_UsesConfiguredOffsetAndSmoothsPosition()
    {
        ActivateRunCamera();
        _clock.DeltaTime = 0.05f;
        _config.PositionResponseRate = 2f;
        var previousPosition = _anchor.Position;
        _source.Position = new Vector3(11f, 0f, 2f);

        ((ILateTickable)_controller).LateTick();

        var targetPosition = _source.Position + _config.AnchorOffset;
        var expectedPosition = Vector3.Lerp(previousPosition, targetPosition, 0.1f);
        Assert.That(_anchor.Position.x, Is.EqualTo(expectedPosition.x).Within(0.0001f));
        Assert.That(_anchor.Position.y, Is.EqualTo(expectedPosition.y).Within(0.0001f));
        Assert.That(_anchor.Position.z, Is.EqualTo(expectedPosition.z).Within(0.0001f));
    }

    [Test]
    public void LateTick_UsesPlanarVelocityForYaw()
    {
        ActivateRunCamera();
        _source.LinearVelocity = Vector3.right * 8f;

        ((ILateTickable)_controller).LateTick();

        AssertForward(_anchor.Rotation, Vector3.right);
    }

    [Test]
    public void LateTick_LowPlanarVelocity_PreservesLastValidYaw()
    {
        ActivateRunCamera();
        _source.LinearVelocity = Vector3.right * 8f;
        ((ILateTickable)_controller).LateTick();
        var yawAfterValidVelocity = _anchor.Rotation;

        _source.LinearVelocity = Vector3.zero;
        ((ILateTickable)_controller).LateTick();

        AssertForward(yawAfterValidVelocity, _anchor.Rotation * Vector3.forward);
    }

    [Test]
    public void LateTick_InvalidVelocity_DoesNotProduceInvalidRotation()
    {
        ActivateRunCamera();
        _source.LinearVelocity = new Vector3(float.NaN, 0f, 1f);

        ((ILateTickable)_controller).LateTick();

        Assert.That(float.IsNaN(_anchor.Rotation.x), Is.False);
        Assert.That(float.IsNaN(_anchor.Rotation.y), Is.False);
        Assert.That(float.IsNaN(_anchor.Rotation.z), Is.False);
        Assert.That(float.IsNaN(_anchor.Rotation.w), Is.False);
    }

    private RunCameraController CreateController()
    {
        return new RunCameraController(
            _stateService,
            _launchAppliedNotifier,
            _source,
            _anchor,
            _rig,
            _config,
            _clock,
            _runPreparationStateId,
            _preLaunchStateId,
            _runningStateId,
            _runEndedStateId);
    }

    private void ActivateRunCamera()
    {
        _stateService.ChangeTo(_runningStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent(Vector3.forward, Vector3.up));
        Assert.That(_rig.ActiveCamera, Is.EqualTo(FakeRunCameraRig.CameraId.Run));
    }

    private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent(Vector3 launchDirection, Vector3 upDirection)
    {
        var normalizedLaunchDirection = launchDirection.normalized;
        var normalizedUpDirection = upDirection.normalized;

        var request = new SlingshotLaunchRequest(
            1f,
            1f,
            0f,
            0f,
            Vector3.zero,
            normalizedLaunchDirection,
            normalizedUpDirection);

        return new SlingshotLaunchAppliedEvent(
            request,
            normalizedLaunchDirection * 10f,
            normalizedLaunchDirection,
            normalizedUpDirection);
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);

        return stateId;
    }

    private void AssertForward(Quaternion rotation, Vector3 expectedForward)
    {
        Assert.That(Vector3.Dot(rotation * Vector3.forward, expectedForward.normalized), Is.GreaterThan(0.999f));
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

        public void Apply(SlingshotLaunchAppliedEvent launchApplied)
        {
            LaunchApplied?.Invoke(launchApplied);
        }
    }

    private sealed class FakeRunCameraSource : IRunCameraSource
    {
        public Vector3 Position { get; set; }
        public Vector3 LinearVelocity { get; set; }
    }

    private sealed class FakeRunCameraAnchor : IRunCameraAnchor
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    private sealed class FakeRunCameraRig : IRunCameraRig
    {
        public enum CameraId
        {
            None = 0,
            RunPreparation = 1,
            PreLaunch = 2,
            Run = 3
        }

        public CameraId ActiveCamera { get; private set; }
        public int ActivationCallCount { get; private set; }

        public void ActivateRunPreparationCamera()
        {
            Activate(CameraId.RunPreparation);
        }

        public void ActivatePreLaunchCamera()
        {
            Activate(CameraId.PreLaunch);
        }

        public void ActivateRunCamera()
        {
            Activate(CameraId.Run);
        }

        private void Activate(CameraId cameraId)
        {
            if (ActiveCamera == cameraId)
            {
                return;
            }

            ActiveCamera = cameraId;
            ActivationCallCount += 1;
        }
    }

    private sealed class FakeRunCameraConfig : IRunCameraConfig
    {
        public Vector3 AnchorOffset { get; set; }
        public float PositionResponseRate { get; set; }
        public float YawResponseRate { get; set; }
        public float MinimumYawSpeed { get; set; }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
