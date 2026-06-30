using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Internal;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class GameplayFlowControllerTests
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
    public void LaunchRequested_WhenTransitionSucceeds_TransitionsToRunningBeforeLaunching()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = true
        };
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        notifier.RequestLaunch(request);

        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { running }));
        Assert.That(launcher.LaunchRequests, Has.Count.EqualTo(1));
        Assert.That(launcher.LaunchRequests[0], Is.EqualTo(request));
        Assert.That(_observations, Is.EqualTo(new[] { "transition", "capture-disable", "launch" }));
    }

    [Test]
    public void LaunchRequested_WhenTransitionFails_DoesNotLaunch()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = false
        };
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        notifier.RequestLaunch(request);

        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { running }));
        Assert.That(launcher.LaunchRequests, Is.Empty);
        Assert.That(_observations, Is.EqualTo(new[] { "transition" }));
    }

    [Test]
    public void LaunchRequested_WhenLauncherThrows_PropagatesAfterSuccessfulTransition()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = true
        };

        var launcher = new FakeGameplaySlingshotLauncher(_observations)
        {
            ThrowOnLaunch = true
        };
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        Assert.That(
            () => notifier.RequestLaunch(request),
            Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Launch failed"));
        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { running }));
        Assert.That(_observations, Is.EqualTo(new[] { "transition", "capture-disable", "launch" }));
    }

    [Test]
    public void LaunchRequested_WhenLauncherThrows_DoesNotRollbackAcceptedTransition()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = true
        };

        var launcher = new FakeGameplaySlingshotLauncher(_observations)
        {
            ThrowOnLaunch = true
        };
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        Assert.That(
            () => notifier.RequestLaunch(request),
            Throws.TypeOf<InvalidOperationException>());

        Assert.That(stateService.CurrentStateId, Is.SameAs(running));
        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { running }));
    }

    [Test]
    public void Dispose_WhenLaunchRequestedAfterDispose_DoesNotTransitionOrLaunch()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        ((IDisposable)controller).Dispose();
        notifier.RequestLaunch(request);

        Assert.That(stateService.RequestedStateIds, Is.Empty);
        Assert.That(launcher.LaunchRequests, Is.Empty);
        Assert.That(stateService.CurrentStateId, Is.SameAs(preLaunch));
    }

    [Test]
    public void LaunchRequested_UsesConfiguredRunningStateId()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var anotherRunning = CreateStateId("Another Running");
        var request = CreateLaunchRequest();
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, anotherRunning);

        notifier.RequestLaunch(request);

        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { anotherRunning }));
        Assert.That(stateService.RequestedStateIds, Has.None.SameAs(running));
    }

    [Test]
    public void Initialize_CurrentPreLaunch_EnablesSlingshotCapture()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running, false);

        Assert.That(capture.EnableCallCount, Is.EqualTo(1));
        Assert.That(capture.DisableCallCount, Is.Zero);
        Assert.That(_observations, Is.EqualTo(new[] { "prelaunch-reset", "capture-enable" }));
    }

    [Test]
    public void Initialize_CurrentPreLaunch_ResetsPreLaunchRigBeforeCapture()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        var resetter = new FakePreLaunchRigPoseResetter(_observations);

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running,
            clearObservationsAfterInitialize: false, resetter);

        Assert.That(resetter.ResetCallCount, Is.EqualTo(1));
        Assert.That(_observations, Is.EqualTo(new[] { "prelaunch-reset", "capture-enable" }));
    }

    [Test]
    public void Initialize_CurrentRunPreparation_ResetsSceneAndLeavesCaptureDisabled()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(runPreparation, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        var resetter = new FakePreLaunchRigPoseResetter(_observations);
        var slingshotReset = new FakeSlingshotRunPreparationReset(_observations);

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running,
            clearObservationsAfterInitialize: false, resetter, runPreparation, slingshotReset);

        Assert.That(resetter.ResetCallCount, Is.EqualTo(1));
        Assert.That(slingshotReset.ResetCallCount, Is.EqualTo(1));
        Assert.That(capture.EnableCallCount, Is.Zero);
        Assert.That(capture.DisableCallCount, Is.Zero);
        Assert.That(_observations, Is.EqualTo(new[] { "prelaunch-reset", "slingshot-run-prep-reset" }));
    }

    [Test]
    public void Initialize_CurrentStateIsNotPreLaunch_LeavesSlingshotCaptureDisabled()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(running, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running, false);

        Assert.That(capture.EnableCallCount, Is.Zero);
        Assert.That(capture.DisableCallCount, Is.Zero);
        Assert.That(_observations, Is.Empty);
    }

    [Test]
    public void GameplayStateChanged_EnteringPreLaunch_EnablesSlingshotCapture()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(running, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        stateService.ChangeTo(preLaunch);

        Assert.That(capture.EnableCallCount, Is.EqualTo(1));
        Assert.That(capture.DisableCallCount, Is.Zero);
        Assert.That(_observations, Is.EqualTo(new[] { "prelaunch-reset", "capture-enable" }));
    }

    [Test]
    public void GameplayStateChanged_ExternalObserverSubscribedBeforeController_ObserverRunsAfterPreLaunchReset()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(running, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        var resetter = new FakePreLaunchRigPoseResetter(_observations);

        stateService.GameplayStateChanged += (_, _) =>
            _observations.Add(resetter.ResetCallCount > 0 ? "changed-after-reset" : "changed-before-reset");

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running,
            clearObservationsAfterInitialize: true, resetter);

        stateService.ChangeTo(preLaunch);

        Assert.That(_observations, Is.EqualTo(new[] { "prelaunch-reset", "changed-after-reset", "capture-enable" }));
    }

    [Test]
    public void GameplayStateChanging_EnteringRunPreparation_ResetsBeforeChangedObservers()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        var resetter = new FakePreLaunchRigPoseResetter(_observations);
        var slingshotReset = new FakeSlingshotRunPreparationReset(_observations);

        stateService.GameplayStateChanged += (_, _) =>
            _observations.Add(slingshotReset.ResetCallCount > 0 ? "changed-after-runprep-reset" : "changed-before-runprep-reset");

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running,
            clearObservationsAfterInitialize: true, resetter, runPreparation, slingshotReset);

        stateService.ChangeTo(runPreparation);

        Assert.That(_observations, Is.EqualTo(new[]
        {
            "prelaunch-reset",
            "slingshot-run-prep-reset",
            "changed-after-runprep-reset"
        }));
    }

    [Test]
    public void TryContinue_RunPreparationToPreLaunch_EnablesCaptureWithoutSecondReset()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(runPreparation, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        var resetter = new FakePreLaunchRigPoseResetter(_observations);
        var slingshotReset = new FakeSlingshotRunPreparationReset(_observations);

        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running,
            clearObservationsAfterInitialize: true, resetter, runPreparation, slingshotReset);
        var command = (IRunPreparationContinueCommand)controller;

        var accepted = command.TryContinue();

        Assert.That(accepted, Is.True);
        Assert.That(capture.EnableCallCount, Is.EqualTo(1));
        Assert.That(capture.DisableCallCount, Is.Zero);
        Assert.That(resetter.ResetCallCount, Is.EqualTo(1));
        Assert.That(slingshotReset.ResetCallCount, Is.EqualTo(1));
        Assert.That(_observations, Is.EqualTo(new[] { "transition", "capture-enable" }));
    }

    [Test]
    public void GameplayStateChanged_LeavingPreLaunch_DisablesSlingshotCapture()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        stateService.ChangeTo(running);

        Assert.That(capture.EnableCallCount, Is.EqualTo(1));
        Assert.That(capture.DisableCallCount, Is.EqualTo(1));
        Assert.That(_observations, Is.EqualTo(new[] { "capture-disable" }));
    }

    [Test]
    public void Dispose_WhenGameplayStateChangesAfterDispose_DoesNotToggleCapture()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var capture = new FakeSlingshotCapture(_observations);
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(running, _observations);
        var launcher = new FakeGameplaySlingshotLauncher(_observations);
        using var controller = CreateInitializedController(capture, notifier, stateService, launcher, preLaunch, running);

        ((IDisposable)controller).Dispose();
        stateService.ChangeTo(preLaunch);

        Assert.That(capture.EnableCallCount, Is.Zero);
        Assert.That(capture.DisableCallCount, Is.Zero);
        Assert.That(_observations, Is.Empty);
    }

    [Test]
    public void Install_ContainerBuilt_RegistersGameplayFlowEntryPoint()
    {
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var runEnded = CreateStateId("Run Ended");
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new FakeSlingshotCapture(_observations)).As<ISlingshotCapture>();
        builder.RegisterInstance(new FakeSlingshotRunPreparationReset(_observations)).As<ISlingshotRunPreparationReset>();
        builder.RegisterInstance(new FakeSlingshotLaunchNotifier()).As<ISlingshotLaunchNotifier>();
        builder.RegisterInstance(new FakeGameplayStateService(preLaunch, _observations)).As<IGameplayStateService>();
        builder.RegisterInstance(new FakeGameplaySlingshotLauncher(_observations)).As<IGameplaySlingshotLauncher>();
        builder.RegisterInstance(new FakeRunModifierSnapshotFactory()).As<IRunModifierSnapshotFactory>();
        builder.RegisterInstance(new FakeRunModifierSnapshotStore()).As<IRunModifierSnapshotStore>();
        builder.RegisterInstance(new FakePreLaunchRigPoseResetter(_observations)).As<IPreLaunchRigPoseResetter>();
        var installer = new GameplayFlowInstaller(runPreparation, preLaunch, running, runEnded);

        installer.Install(builder);

        using var container = builder.Build();
        var initializables = container.Resolve<ContainerLocal<IReadOnlyList<IInitializable>>>().Value;
        var continueCommand = container.Resolve<IRunPreparationContinueCommand>();
        var resolvedRunPreparation = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.RunPreparation);
        var resolvedPreLaunch = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.PreLaunch);
        var resolvedRunning = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.Running);
        var resolvedRunEnded = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.RunEnded);

        Assert.That(initializables.Count, Is.EqualTo(1));
        Assert.That(continueCommand, Is.Not.Null);
        Assert.That(resolvedRunPreparation, Is.SameAs(runPreparation));
        Assert.That(resolvedPreLaunch, Is.SameAs(preLaunch));
        Assert.That(resolvedRunning, Is.SameAs(running));
        Assert.That(resolvedRunEnded, Is.SameAs(runEnded));
    }

    private GameplayFlowController CreateInitializedController(
        ISlingshotCapture capture,
        ISlingshotLaunchNotifier notifier,
        IGameplayStateService stateService,
        IGameplaySlingshotLauncher launcher,
        GameplayStateId preLaunchStateId,
        GameplayStateId runningStateId,
        bool clearObservationsAfterInitialize = true,
        FakePreLaunchRigPoseResetter preLaunchRigPoseResetter = null,
        GameplayStateId runPreparationStateId = null,
        FakeSlingshotRunPreparationReset slingshotRunPreparationReset = null)
    {
        preLaunchRigPoseResetter ??= new FakePreLaunchRigPoseResetter(_observations);
        slingshotRunPreparationReset ??= new FakeSlingshotRunPreparationReset(_observations);
        runPreparationStateId ??= CreateStateId("Run Preparation");

        var controller = new GameplayFlowController(capture, slingshotRunPreparationReset, notifier, stateService, launcher,
            new FakeRunModifierSnapshotFactory(), new FakeRunModifierSnapshotStore(), preLaunchRigPoseResetter,
            runPreparationStateId, preLaunchStateId, runningStateId);
        ((IInitializable)controller).Initialize();

        if (clearObservationsAfterInitialize)
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
            pullStrength: 0.75f,
            pullDistance: 1.5f,
            pullOffset: 0.25f,
            normalizedLateralPull: 0.25f,
            finalPullPoint: new Vector3(0.25f, 0f, -1.5f),
            launchFrameForward: Vector3.forward,
            launchFrameUp: Vector3.up);
    }

    private sealed class FakeSlingshotLaunchNotifier : ISlingshotLaunchNotifier
    {
        public event Action<SlingshotLaunchRequest> LaunchRequested;

        public void RequestLaunch(SlingshotLaunchRequest request)
        {
            LaunchRequested?.Invoke(request);
        }
    }

    private sealed class FakeRunModifierSnapshotFactory : IRunModifierSnapshotFactory
    {
        public RunModifierSnapshot CreateSnapshot()
        {
            return new RunModifierSnapshot(Array.Empty<GameplayStatModifier>());
        }
    }

    private sealed class FakeRunModifierSnapshotStore : IRunModifierSnapshotStore
    {
        public RunModifierSnapshot CurrentSnapshot { get; private set; } = new(Array.Empty<GameplayStatModifier>());

        public void SetSnapshot(RunModifierSnapshot snapshot)
        {
            CurrentSnapshot = snapshot;
        }
    }

    private sealed class FakeGameplayStateService : IGameplayStateService
    {
        private readonly List<string> _observations;

        public GameplayStateId CurrentStateId { get; private set; }
        public bool ShouldTransitionSucceed { get; set; }
        public List<GameplayStateId> RequestedStateIds { get; } = new();

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        public FakeGameplayStateService(GameplayStateId currentStateId, List<string> observations)
        {
            CurrentStateId = currentStateId;
            _observations = observations;
            ShouldTransitionSucceed = true;
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

        public void ChangeTo(GameplayStateId nextStateId)
        {
            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
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

    private sealed class FakeSlingshotRunPreparationReset : ISlingshotRunPreparationReset
    {
        private readonly List<string> _observations;

        public int ResetCallCount { get; private set; }

        public FakeSlingshotRunPreparationReset(List<string> observations)
        {
            _observations = observations;
        }

        public void ResetForRunPreparation()
        {
            ResetCallCount += 1;
            _observations.Add("slingshot-run-prep-reset");
        }
    }

    private sealed class FakePreLaunchRigPoseResetter : IPreLaunchRigPoseResetter
    {
        private readonly List<string> _observations;

        public int ResetCallCount { get; private set; }

        public FakePreLaunchRigPoseResetter(List<string> observations)
        {
            _observations = observations;
        }

        public void ResetToPreLaunchRigPose()
        {
            ResetCallCount += 1;
            _observations.Add("prelaunch-reset");
        }
    }

    private sealed class FakeGameplaySlingshotLauncher : IGameplaySlingshotLauncher
    {
        private readonly List<string> _observations;

        public bool ThrowOnLaunch { get; set; }
        public List<SlingshotLaunchRequest> LaunchRequests { get; } = new();

        public FakeGameplaySlingshotLauncher(List<string> observations)
        {
            _observations = observations;
        }

        public void Launch(SlingshotLaunchRequest request)
        {
            LaunchRequests.Add(request);
            _observations.Add("launch");

            if (ThrowOnLaunch)
                throw new InvalidOperationException("Launch failed");
        }
    }
}
