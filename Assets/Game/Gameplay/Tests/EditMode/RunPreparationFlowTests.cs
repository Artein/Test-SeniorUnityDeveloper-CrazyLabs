using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunPreparationFlowTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private readonly List<string> _observations = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
        _observations.Clear();
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void TryContinue_CurrentRunPreparation_CreatesSnapshotBeforeEnteringPreLaunch()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var snapshot = new RunModifierSnapshot(Array.Empty<GameplayStatModifier>());

        var snapshotFactory = new FakeRunModifierSnapshotFactory(_observations)
        {
            Snapshot = snapshot
        };
        var snapshotStore = new FakeRunModifierSnapshotStore(_observations);
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(runPreparation, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, snapshotFactory, snapshotStore, runPreparation,
            preLaunch, running);
        var command = (IRunPreparationContinueCommand)controller;

        var accepted = command.TryContinue();

        Assert.That(accepted, Is.True);
        Assert.That(snapshotStore.CurrentSnapshot, Is.SameAs(snapshot));
        Assert.That(stateService.CurrentStateId, Is.SameAs(preLaunch));
        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { preLaunch }));
        Assert.That(capture.EnableCallCount, Is.EqualTo(1));
        Assert.That(_observations, Is.EqualTo(new[] { "snapshot-create", "snapshot-set", "transition", "capture-enable" }));
    }

    [Test]
    public void TryContinue_CurrentStateIsNotRunPreparation_DoesNotCreateSnapshotOrTransition()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var snapshotFactory = new FakeRunModifierSnapshotFactory(_observations);
        var snapshotStore = new FakeRunModifierSnapshotStore(_observations);
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, snapshotFactory, snapshotStore, runPreparation,
            preLaunch, running);
        _observations.Clear();
        var command = (IRunPreparationContinueCommand)controller;

        var accepted = command.TryContinue();

        Assert.That(accepted, Is.False);
        Assert.That(snapshotFactory.CreateCallCount, Is.Zero);
        Assert.That(snapshotStore.SetCallCount, Is.Zero);
        Assert.That(stateService.CurrentStateId, Is.SameAs(preLaunch));
        Assert.That(stateService.RequestedStateIds, Is.Empty);
        Assert.That(_observations, Is.Empty);
    }

    [Test]
    public void TryContinue_SnapshotCreationFails_LeavesStateInRunPreparation()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");

        var snapshotFactory = new FakeRunModifierSnapshotFactory(_observations)
        {
            ThrowOnCreate = true
        };
        var snapshotStore = new FakeRunModifierSnapshotStore(_observations);
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(runPreparation, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, snapshotFactory, snapshotStore, runPreparation,
            preLaunch, running);
        var command = (IRunPreparationContinueCommand)controller;

        Assert.That(
            command.TryContinue,
            Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Snapshot creation failed."));
        Assert.That(stateService.CurrentStateId, Is.SameAs(runPreparation));
        Assert.That(stateService.RequestedStateIds, Is.Empty);
        Assert.That(snapshotStore.SetCallCount, Is.Zero);
        Assert.That(capture.EnableCallCount, Is.Zero);
        Assert.That(_observations, Is.EqualTo(new[] { "snapshot-create" }));
    }

    [Test]
    public void LaunchRequested_CurrentStateIsNotPreLaunch_IgnoresLaunchRequest()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var snapshotFactory = new FakeRunModifierSnapshotFactory(_observations);
        var snapshotStore = new FakeRunModifierSnapshotStore(_observations);
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(runPreparation, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, snapshotFactory, snapshotStore, runPreparation,
            preLaunch, running);
        notifier.RequestLaunch(request);

        Assert.That(stateService.CurrentStateId, Is.SameAs(runPreparation));
        Assert.That(stateService.RequestedStateIds, Is.Empty);
        Assert.That(launcher.LaunchRequests, Is.Empty);
        Assert.That(_observations, Is.Empty);
    }

    private GameplayFlowController CreateInitializedController(
        ISlingshotCapture capture,
        ISlingshotLaunchNotifier notifier,
        IGameplayStateService stateService,
        IGameplaySlingshotLauncher launcher,
        IRunModifierSnapshotFactory snapshotFactory,
        IRunModifierSnapshotStore snapshotStore,
        GameplayStateId runPreparationStateId,
        GameplayStateId preLaunchStateId,
        GameplayStateId runningStateId)
    {
        var controller = new GameplayFlowController(capture, new SilentSlingshotRunPreparationReset(), notifier, stateService, launcher,
            snapshotFactory, snapshotStore, new SilentPreLaunchRigPoseResetter(), runPreparationStateId, preLaunchStateId, runningStateId);
        ((IInitializable)controller).Initialize();
        _observations.Clear();
        return controller;
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
        stateId.name = stateName;
        return stateId;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private SlingshotLaunchRequest CreateLaunchRequest()
    {
        return new SlingshotLaunchRequest(
            0.75f,
            1.5f,
            0.25f,
            0.25f,
            new Vector3(0.25f, 0f, -1.5f),
            Vector3.forward,
            Vector3.up);
    }

    private sealed class FakeRunModifierSnapshotFactory : IRunModifierSnapshotFactory
    {
        private readonly List<string> _observations;

        public bool ThrowOnCreate { get; set; }
        public int CreateCallCount { get; private set; }
        public RunModifierSnapshot Snapshot { get; set; } = new(Array.Empty<GameplayStatModifier>());

        public FakeRunModifierSnapshotFactory(List<string> observations)
        {
            _observations = observations;
        }

        public RunModifierSnapshot CreateSnapshot()
        {
            CreateCallCount += 1;
            _observations.Add("snapshot-create");

            if (ThrowOnCreate)
                throw new InvalidOperationException("Snapshot creation failed.");

            return Snapshot;
        }
    }

    private sealed class FakeRunModifierSnapshotStore : IRunModifierSnapshotStore
    {
        private readonly List<string> _observations;

        public RunModifierSnapshot CurrentSnapshot { get; private set; } = new(Array.Empty<GameplayStatModifier>());
        public int SetCallCount { get; private set; }

        public FakeRunModifierSnapshotStore(List<string> observations)
        {
            _observations = observations;
        }

        public void SetSnapshot(RunModifierSnapshot snapshot)
        {
            SetCallCount += 1;
            CurrentSnapshot = snapshot;
            _observations.Add("snapshot-set");
        }
    }

    private sealed class FakeSlingshotLaunchNotifier : ISlingshotLaunchNotifier
    {
        public event Action<SlingshotLaunchRequest> LaunchRequested;

        public void RequestLaunch(SlingshotLaunchRequest request)
        {
            LaunchRequested?.Invoke(request);
        }
    }

    private sealed class FakeGameplayStateService : IGameplayStateService
    {
        private readonly List<string> _observations;

        public GameplayStateId CurrentStateId { get; private set; }
        public bool ShouldTransitionSucceed { get; set; } = true;
        public List<GameplayStateId> RequestedStateIds { get; } = new();

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        public FakeGameplayStateService(GameplayStateId currentStateId, List<string> observations)
        {
            CurrentStateId = currentStateId;
            _observations = observations;
        }

        public bool IsCurrent(GameplayStateId stateId)
        {
            return ReferenceEquals(CurrentStateId, stateId);
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            RequestedStateIds.Add(nextStateId);
            _observations.Add("transition");

            if (!ShouldTransitionSucceed)
                return false;

            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
            return true;
        }
    }

    private sealed class FakeSlingshotCapture : ISlingshotCapture
    {
        private readonly List<string> _observations;

        public int EnableCallCount { get; private set; }
        public int DisableCallCount { get; private set; }

        public FakeSlingshotCapture(List<string> observations)
        {
            _observations = observations;
        }

        public void EnableCapture()
        {
            EnableCallCount += 1;
            _observations.Add("capture-enable");
        }

        public void DisableCapture()
        {
            DisableCallCount += 1;
            _observations.Add("capture-disable");
        }
    }

    private sealed class SilentSlingshotRunPreparationReset : ISlingshotRunPreparationReset
    {
        public void ResetForRunPreparation()
        {
        }
    }

    private sealed class FakeGameplaySlingshotLauncher : IGameplaySlingshotLauncher
    {
        private readonly List<string> _observations;

        public List<SlingshotLaunchRequest> LaunchRequests { get; } = new();

        public FakeGameplaySlingshotLauncher(List<string> observations)
        {
            _observations = observations;
        }

        public void Launch(SlingshotLaunchRequest request)
        {
            LaunchRequests.Add(request);
            _observations.Add("launch");
        }
    }

    private sealed class SilentPreLaunchRigPoseResetter : IPreLaunchRigPoseResetter
    {
        public void ResetToPreLaunchRigPose()
        {
        }
    }
}
