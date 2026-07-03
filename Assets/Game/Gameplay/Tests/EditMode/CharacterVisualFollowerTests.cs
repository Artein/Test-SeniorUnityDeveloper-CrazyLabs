using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class CharacterVisualFollowerTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private GameplayStateId _runPreparationStateId;
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private GameplayStateId _runEndedStateId;
    private FakeGameplayStateService _stateService;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeRunResultNotifier _runResultNotifier;
    private FakeRunCameraLens _runCameraLens;
    private FakeCharacterVisualTargetPoseSource _targetPoseSource;
    private FakeCharacterVisualFollowView _view;
    private FakeCharacterVisualFollowTuning _tuning;
    private ICharacterVisualPoseSmoother _smoother;
    private FakeTime _clock;
    private CharacterVisualFollower _follower;

    [SetUp]
    public void OnSetUp()
    {
        _runPreparationStateId = CreateStateId("RunPreparation");
        _preLaunchStateId = CreateStateId("PreLaunch");
        _runningStateId = CreateStateId("Running");
        _runEndedStateId = CreateStateId("RunEnded");
        _stateService = new FakeGameplayStateService(_runPreparationStateId);
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _runResultNotifier = new FakeRunResultNotifier();
        _runCameraLens = new FakeRunCameraLens();

        _targetPoseSource = new FakeCharacterVisualTargetPoseSource
        {
            CurrentPose = new CharacterVisualPose(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 15f, 0f))
        };
        _runCameraLens.Position = _targetPoseSource.CurrentPose.Position + Vector3.right * 4f;
        _runCameraLens.Rotation = Quaternion.identity;

        _view = new FakeCharacterVisualFollowView
        {
            CurrentVisualPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity)
        };

        _tuning = new FakeCharacterVisualFollowTuning
        {
            VisualPositionResponseRate = 60f,
            VisualHeadingResponseRate = 45f,
            VisualUpTiltResponseRate = 18f,
            VisualMaxPositionLag = 0.06f,
            VisualSnapDistance = 0.75f,
            VisualSnapAngleDegrees = 45f
        };

        _smoother = new CharacterVisualPoseSmoother();

        _clock = new FakeTime
        {
            DeltaTime = 0.02f,
            FixedDeltaTime = 0.02f
        };
        _follower = CreateFollower();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_follower).Dispose();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void Initialize_SnapsVisualAnchorToCurrentTargetPose()
    {
        ((IInitializable)_follower).Initialize();

        AssertPose(_view.CurrentVisualPose, _targetPoseSource.CurrentPose);
        Assert.That(_view.AppliedPoseCount, Is.EqualTo(1));
    }

    [Test]
    public void LateTick_TargetMovesWithinSnapThreshold_SmoothsPose()
    {
        ((IInitializable)_follower).Initialize();
        var targetPose = new CharacterVisualPose(new Vector3(1.4f, 2f, 3f), Quaternion.Euler(0f, 25f, 0f));
        _targetPoseSource.CurrentPose = targetPose;

        ((ILateTickable)_follower).LateTick();

        Assert.That(Vector3.Distance(_view.CurrentVisualPose.Position, targetPose.Position),
            Is.LessThanOrEqualTo(_tuning.VisualMaxPositionLag + 0.0001f));
        Assert.That(Vector3.Distance(_view.CurrentVisualPose.Position, targetPose.Position), Is.GreaterThan(0.0001f));
        Assert.That(Quaternion.Angle(_view.CurrentVisualPose.Rotation, targetPose.Rotation), Is.GreaterThan(0.0001f));
    }

    [Test]
    public void GameplayStateChanged_ToPreLaunch_SnapsPose()
    {
        ((IInitializable)_follower).Initialize();
        _targetPoseSource.CurrentPose = new CharacterVisualPose(new Vector3(1.4f, 2f, 3f), Quaternion.Euler(0f, 25f, 0f));
        ((ILateTickable)_follower).LateTick();

        _stateService.ChangeTo(_preLaunchStateId);

        AssertPose(_view.CurrentVisualPose, _targetPoseSource.CurrentPose);
    }

    [Test]
    public void GameplayStateChanged_ToRunEnded_DoesNotSnapAndNextLateTickSmoothsPose()
    {
        ((IInitializable)_follower).Initialize();
        _stateService.ChangeTo(_runningStateId);
        _targetPoseSource.CurrentPose = new CharacterVisualPose(new Vector3(1.4f, 2f, 3f), Quaternion.Euler(0f, 25f, 0f));
        ((ILateTickable)_follower).LateTick();
        var poseBeforeRunEnded = _view.CurrentVisualPose;
        var appliedPoseCountBeforeRunEnded = _view.AppliedPoseCount;
        var terminalPose = new CharacterVisualPose(new Vector3(1.45f, 2f, 3f), Quaternion.Euler(0f, 30f, 0f));
        _targetPoseSource.CurrentPose = terminalPose;

        _stateService.ChangeTo(_runEndedStateId);

        AssertPose(_view.CurrentVisualPose, poseBeforeRunEnded);
        Assert.That(_view.AppliedPoseCount, Is.EqualTo(appliedPoseCountBeforeRunEnded));

        ((ILateTickable)_follower).LateTick();

        Assert.That(Vector3.Distance(_view.CurrentVisualPose.Position, terminalPose.Position),
            Is.LessThanOrEqualTo(_tuning.VisualMaxPositionLag + 0.0001f));
        Assert.That(Vector3.Distance(_view.CurrentVisualPose.Position, terminalPose.Position), Is.GreaterThan(0.0001f));
        Assert.That(Quaternion.Angle(_view.CurrentVisualPose.Rotation, terminalPose.Rotation), Is.GreaterThan(0.0001f));
    }

    [Test]
    public void LaunchApplied_SnapsPose()
    {
        ((IInitializable)_follower).Initialize();
        _targetPoseSource.CurrentPose = new CharacterVisualPose(new Vector3(1.4f, 2f, 3f), Quaternion.Euler(0f, 25f, 0f));
        ((ILateTickable)_follower).LateTick();

        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());

        AssertPose(_view.CurrentVisualPose, _targetPoseSource.CurrentPose);
    }

    [Test]
    public void RunResultAccepted_SuccessfulResult_BlendsVisualFrontTowardRunCamera()
    {
        ((IInitializable)_follower).Initialize();
        _stateService.ChangeTo(_runningStateId);
        _tuning.VisualSnapAngleDegrees = 180f;
        _tuning.VisualHeadingResponseRate = 1f;
        var poseBeforeResult = _view.CurrentVisualPose;
        var frontFacingCameraRotation = GetFrontFacingCameraRotation(_targetPoseSource.CurrentPose);

        _runResultNotifier.Raise(CreateRunResult(RunEndReason.Finished));
        ((ILateTickable)_follower).LateTick();

        Assert.That(_view.CurrentVisualPose.Position, Is.EqualTo(_targetPoseSource.CurrentPose.Position));

        Assert.That(
            Quaternion.Angle(_view.CurrentVisualPose.Rotation, frontFacingCameraRotation),
            Is.LessThan(Quaternion.Angle(poseBeforeResult.Rotation, frontFacingCameraRotation)));
        Assert.That(Quaternion.Angle(_view.CurrentVisualPose.Rotation, frontFacingCameraRotation), Is.GreaterThan(0.1f));
        Assert.That(Quaternion.Angle(_view.CurrentVisualPose.Rotation, poseBeforeResult.Rotation), Is.GreaterThan(0.1f));
    }

    [Test]
    public void RunResultAccepted_FailedResult_KeepsTargetHeading()
    {
        ((IInitializable)_follower).Initialize();
        _stateService.ChangeTo(_runningStateId);
        var poseBeforeResult = _view.CurrentVisualPose;

        _runResultNotifier.Raise(CreateRunResult(RunEndReason.ObstacleHit));
        ((ILateTickable)_follower).LateTick();

        AssertPose(_view.CurrentVisualPose, poseBeforeResult);
    }

    [Test]
    public void GameplayStateChanged_ToRunPreparation_AfterVictoryFacing_SnapsBackToTargetPose()
    {
        ((IInitializable)_follower).Initialize();
        _stateService.ChangeTo(_runningStateId);
        _tuning.VisualSnapAngleDegrees = 180f;
        _tuning.VisualHeadingResponseRate = 1f;
        _runResultNotifier.Raise(CreateRunResult(RunEndReason.Finished));
        ((ILateTickable)_follower).LateTick();
        Assert.That(Quaternion.Angle(_view.CurrentVisualPose.Rotation, _targetPoseSource.CurrentPose.Rotation), Is.GreaterThan(0.1f));

        _stateService.ChangeTo(_runPreparationStateId);

        AssertPose(_view.CurrentVisualPose, _targetPoseSource.CurrentPose);
    }

    [Test]
    public void RepeatedInitialize_DoesNotSubscribeTwice()
    {
        ((IInitializable)_follower).Initialize();
        ((IInitializable)_follower).Initialize();
        var appliedPoseCountAfterInitialize = _view.AppliedPoseCount;
        _targetPoseSource.CurrentPose = new CharacterVisualPose(new Vector3(1.2f, 2f, 3f), Quaternion.identity);

        _stateService.ChangeTo(_preLaunchStateId);

        Assert.That(_view.AppliedPoseCount, Is.EqualTo(appliedPoseCountAfterInitialize + 1));
    }

    [Test]
    public void Dispose_UnsubscribesAndStopsLateTicking()
    {
        ((IInitializable)_follower).Initialize();
        ((IDisposable)_follower).Dispose();
        var poseAfterDispose = _view.CurrentVisualPose;
        _targetPoseSource.CurrentPose = new CharacterVisualPose(new Vector3(1.4f, 2f, 3f), Quaternion.Euler(0f, 25f, 0f));

        _stateService.ChangeTo(_preLaunchStateId);
        _launchAppliedNotifier.Apply(CreateLaunchAppliedEvent());
        ((ILateTickable)_follower).LateTick();

        AssertPose(_view.CurrentVisualPose, poseAfterDispose);
    }

    private CharacterVisualFollower CreateFollower()
    {
        return new CharacterVisualFollower(
            _stateService,
            _launchAppliedNotifier,
            _runResultNotifier,
            _runCameraLens,
            _targetPoseSource,
            _view,
            _tuning,
            _smoother,
            _clock,
            _runPreparationStateId,
            _preLaunchStateId);
    }

    private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent()
    {
        var request = new SlingshotLaunchRequest(1f, 1f, 0f, 0f, Vector3.zero, Vector3.forward, Vector3.up);
        return new SlingshotLaunchAppliedEvent(request, Vector3.forward, Vector3.forward, Vector3.up);
    }

    private RunResult CreateRunResult(RunEndReason reason)
    {
        return new RunResult(
            reason,
            elapsedTime: 1f,
            distanceTravelled: 10f,
            finalPosition: Vector3.forward,
            finalSpeed: 2f,
            rewardBreakdown: new RunRewardBreakdown(Array.Empty<RunRewardSourceAmount>()));
    }

    private Quaternion GetFrontFacingCameraRotation(CharacterVisualPose targetPose)
    {
        var up = targetPose.Rotation * Vector3.up;
        var directionToCamera = Vector3.ProjectOnPlane(_runCameraLens.Position - targetPose.Position, up).normalized;
        return Quaternion.LookRotation(directionToCamera, up);
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _objects.Add(stateId);

        return stateId;
    }

    private static void AssertPose(CharacterVisualPose actual, CharacterVisualPose expected)
    {
        Assert.That(actual.Position.x, Is.EqualTo(expected.Position.x).Within(0.0001f));
        Assert.That(actual.Position.y, Is.EqualTo(expected.Position.y).Within(0.0001f));
        Assert.That(actual.Position.z, Is.EqualTo(expected.Position.z).Within(0.0001f));
        Assert.That(Quaternion.Angle(actual.Rotation, expected.Rotation), Is.EqualTo(0f).Within(0.0001f));
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

    private sealed class FakeRunResultNotifier : IRunResultNotifier
    {
        public event Action<RunResult> RunResultAccepted;

        public void Raise(RunResult result)
        {
            RunResultAccepted?.Invoke(result);
        }
    }

    private sealed class FakeRunCameraLens : IRunCameraLens
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    private sealed class FakeCharacterVisualTargetPoseSource : ICharacterVisualTargetPoseSource
    {
        public CharacterVisualPose CurrentPose { get; set; }
    }

    private sealed class FakeCharacterVisualFollowView : ICharacterVisualFollowView
    {
        public CharacterVisualPose CurrentVisualPose { get; set; }
        public int AppliedPoseCount { get; private set; }

        public void ApplyVisualPose(CharacterVisualPose pose)
        {
            CurrentVisualPose = pose;
            AppliedPoseCount += 1;
        }
    }

    private sealed class FakeCharacterVisualFollowTuning : ICharacterVisualFollowTuning
    {
        public float VisualPositionResponseRate { get; set; }
        public float VisualHeadingResponseRate { get; set; }
        public float VisualUpTiltResponseRate { get; set; }
        public float VisualMaxPositionLag { get; set; }
        public float VisualSnapDistance { get; set; }
        public float VisualSnapAngleDegrees { get; set; }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
