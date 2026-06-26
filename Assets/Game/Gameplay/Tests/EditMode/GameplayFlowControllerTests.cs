using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
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
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = true
        };
        var launcher = new FakeSlingshotLauncher(_observations);
        using var controller = CreateInitializedController(notifier, stateService, launcher, running);

        notifier.RequestLaunch(request);

        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { running }));
        Assert.That(launcher.LaunchRequests, Has.Count.EqualTo(1));
        Assert.That(launcher.LaunchRequests[0].LaunchSpeed, Is.EqualTo(request.LaunchSpeed));
        Assert.That(_observations, Is.EqualTo(new[] { "transition", "launch" }));
    }

    [Test]
    public void LaunchRequested_WhenTransitionFails_DoesNotLaunch()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = false
        };
        var launcher = new FakeSlingshotLauncher(_observations);
        using var controller = CreateInitializedController(notifier, stateService, launcher, running);

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
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = true
        };

        var launcher = new FakeSlingshotLauncher(_observations)
        {
            ThrowOnLaunch = true
        };
        using var controller = CreateInitializedController(notifier, stateService, launcher, running);

        Assert.That(
            () => notifier.RequestLaunch(request),
            Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Launch failed"));
        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { running }));
        Assert.That(_observations, Is.EqualTo(new[] { "transition", "launch" }));
    }

    [Test]
    public void LaunchRequested_WhenLauncherThrows_DoesNotRollbackAcceptedTransition()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var request = CreateLaunchRequest();
        var notifier = new FakeSlingshotLaunchNotifier();

        var stateService = new FakeGameplayStateService(preLaunch, _observations)
        {
            ShouldTransitionSucceed = true
        };

        var launcher = new FakeSlingshotLauncher(_observations)
        {
            ThrowOnLaunch = true
        };
        using var controller = CreateInitializedController(notifier, stateService, launcher, running);

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
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeSlingshotLauncher(_observations);
        using var controller = CreateInitializedController(notifier, stateService, launcher, running);

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
        var notifier = new FakeSlingshotLaunchNotifier();
        var stateService = new FakeGameplayStateService(preLaunch, _observations);
        var launcher = new FakeSlingshotLauncher(_observations);
        using var controller = CreateInitializedController(notifier, stateService, launcher, anotherRunning);

        notifier.RequestLaunch(request);

        Assert.That(stateService.RequestedStateIds, Is.EqualTo(new[] { anotherRunning }));
        Assert.That(stateService.RequestedStateIds, Has.None.SameAs(running));
    }

    [Test]
    public void Install_ContainerBuilt_ResolvesGameplayFlowController()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new FakeSlingshotLaunchNotifier()).As<ISlingshotLaunchNotifier>();
        builder.RegisterInstance(new FakeGameplayStateService(preLaunch, _observations)).As<IGameplayStateService>();
        builder.RegisterInstance(new FakeSlingshotLauncher(_observations)).As<ISlingshotLauncher>();
        var installer = new GameplayFlowInstaller(running);

        installer.Install(builder);

        using var container = builder.Build();
        var controller = container.Resolve<GameplayFlowController>();

        Assert.That(controller, Is.Not.Null);
    }

    private GameplayFlowController CreateInitializedController(
        ISlingshotLaunchNotifier notifier,
        IGameplayStateService stateService,
        ISlingshotLauncher launcher,
        GameplayStateId runningStateId)
    {
        var controller = new GameplayFlowController(notifier, stateService, launcher, runningStateId);
        ((IInitializable)controller).Initialize();

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
            Vector3.forward,
            9f,
            Vector3.up,
            1.25f);
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
    }

    private sealed class FakeSlingshotLauncher : ISlingshotLauncher
    {
        private readonly List<string> _observations;

        public bool ThrowOnLaunch { get; set; }
        public List<SlingshotLaunchRequest> LaunchRequests { get; } = new();

        public FakeSlingshotLauncher(List<string> observations)
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
