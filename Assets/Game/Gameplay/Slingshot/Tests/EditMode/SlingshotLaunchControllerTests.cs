using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class SlingshotLaunchControllerTests
{
    private GameplayStateId _preLaunchStateId;
    private GameplayStateId _runningStateId;
    private GameplayStateId _runEndedStateId;
    private FakeGameplayStateService _stateService;
    private FakeLaunchTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _preLaunchStateId = CreateStateId("PreLaunch");
        _runningStateId = CreateStateId("Running");
        _runEndedStateId = CreateStateId("RunEnded");
        _stateService = new FakeGameplayStateService(_runningStateId);
        _target = new FakeLaunchTarget();
    }

    [TearDown]
    public void OnTearDown()
    {
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void Initialize_CurrentPreLaunch_HoldsTargetOnce()
    {
        _stateService.CurrentStateId = _preLaunchStateId;
        var controller = CreateController();

        ((IInitializable)controller).Initialize();

        Assert.That(_target.HoldCallCount, Is.EqualTo(1));
    }

    [Test]
    public void GameplayStateChanged_ReEnteringPreLaunch_HoldsTargetEveryTime()
    {
        var controller = CreateController();
        ((IInitializable)controller).Initialize();

        _stateService.ChangeTo(_preLaunchStateId);
        _stateService.ChangeTo(_runningStateId);
        _stateService.ChangeTo(_preLaunchStateId);

        Assert.That(_target.HoldCallCount, Is.EqualTo(2));
    }

    [Test]
    public void Initialize_NotPreLaunch_DoesNotHoldTarget()
    {
        var controller = CreateController();

        ((IInitializable)controller).Initialize();

        Assert.That(_target.HoldCallCount, Is.Zero);
    }

    [Test]
    public void GameplayStateChanged_UnrelatedTransition_DoesNotHoldTarget()
    {
        var controller = CreateController();
        ((IInitializable)controller).Initialize();

        _stateService.ChangeTo(_runEndedStateId);

        Assert.That(_target.HoldCallCount, Is.Zero);
    }

    [Test]
    public void Dispose_AfterInitialize_UnsubscribesFromGameplayState()
    {
        var controller = CreateController();
        ((IInitializable)controller).Initialize();
        ((IDisposable)controller).Dispose();

        _stateService.ChangeTo(_preLaunchStateId);

        Assert.That(_target.HoldCallCount, Is.Zero);
    }

    [Test]
    public void Launch_ValidRequest_CallsTargetWithCombinedVelocity()
    {
        var controller = CreateController();
        var request = CreateRequest(Vector3.forward, 8f, Vector3.up, 2f);

        controller.Launch(request);

        Assert.That(_target.LaunchVelocities, Has.Count.EqualTo(1));
        Assert.That(_target.LaunchVelocities[0], Is.EqualTo(new Vector3(0f, 2f, 8f)));
    }

    [Test]
    public void Launch_InvalidDirection_WarnsAndSkipsTargetLaunch()
    {
        var controller = CreateController();
        var request = CreateRequest(new Vector3(2f, 0f, 0f), 8f, Vector3.up, 2f);
        LogAssert.Expect(LogType.Warning, new Regex("Invalid Slingshot launch request"));

        controller.Launch(request);

        Assert.That(_target.LaunchVelocities, Is.Empty);
    }

    [Test]
    public void Launch_InvalidSpeed_WarnsAndSkipsTargetLaunch()
    {
        var controller = CreateController();
        var request = CreateRequest(Vector3.forward, -1f, Vector3.up, 2f);
        LogAssert.Expect(LogType.Warning, new Regex("Invalid Slingshot launch request"));

        controller.Launch(request);

        Assert.That(_target.LaunchVelocities, Is.Empty);
    }

    [Test]
    public void Launch_ZeroFinalVelocity_WarnsAndSkipsTargetLaunch()
    {
        var controller = CreateController();
        var request = CreateRequest(Vector3.forward, 0f, Vector3.up, 0f);
        LogAssert.Expect(LogType.Warning, new Regex("Invalid Slingshot final velocity"));

        controller.Launch(request);

        Assert.That(_target.LaunchVelocities, Is.Empty);
    }

    [Test]
    public void Launch_ValidRequest_DoesNotRequestGameplayTransition()
    {
        var controller = CreateController();
        var request = CreateRequest(Vector3.forward, 8f, Vector3.up, 2f);

        controller.Launch(request);

        Assert.That(_stateService.TryTransitionCallCount, Is.Zero);
    }

    [Test]
    public void Launch_TargetThrows_PropagatesException()
    {
        var controller = CreateController();
        var request = CreateRequest(Vector3.forward, 8f, Vector3.up, 2f);
        _target.ThrowOnLaunch = true;

        Assert.That(
            () => controller.Launch(request),
            Throws.TypeOf<InvalidOperationException>());
    }

    private SlingshotLaunchController CreateController()
    {
        return new SlingshotLaunchController(_target, _stateService, _preLaunchStateId);
    }

    private SlingshotLaunchRequest CreateRequest(Vector3 launchDirection, float launchSpeed, Vector3 launchUpDirection, float launchUpSpeed)
    {
        return new SlingshotLaunchRequest(1f, 3f, 0f, launchDirection, launchSpeed, launchUpDirection, launchUpSpeed);
    }

    private GameplayStateId CreateStateId(string name)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = name;
        return stateId;
    }

    private sealed class FakeLaunchTarget : ILaunchTarget
    {
        public int HoldCallCount { get; private set; }
        public bool ThrowOnLaunch { get; set; }
        public List<Vector3> LaunchVelocities { get; } = new();

        public void Hold()
        {
            HoldCallCount += 1;
        }

        public void Launch(Vector3 velocity)
        {
            if (ThrowOnLaunch)
                throw new InvalidOperationException("target failed");

            LaunchVelocities.Add(velocity);
        }
    }
}
