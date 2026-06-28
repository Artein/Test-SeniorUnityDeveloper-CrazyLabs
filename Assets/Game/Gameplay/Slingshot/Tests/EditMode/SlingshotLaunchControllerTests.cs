using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class SlingshotLaunchControllerTests
{
    private FakeLaunchTarget _target;

    [SetUp]
    public void OnSetUp()
    {
        _target = new FakeLaunchTarget();
    }

    [TearDown]
    public void OnTearDown()
    {
        LogAssert.NoUnexpectedReceived();
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
    public void Launch_ValidRequest_ReappliesFinalPullPointBeforeTargetLaunch()
    {
        var controller = CreateController();
        var finalPullPoint = new Vector3(0.5f, 1f, -1.25f);
        var request = CreateRequest(Vector3.forward, 8f, Vector3.up, 2f, finalPullPoint);

        controller.Launch(request);

        Assert.That(_target.HeldPositions, Is.EqualTo(new[] { finalPullPoint }));
        Assert.That(_target.Observations, Is.EqualTo(new[] { "set-held-position", "launch" }));
    }

    [Test]
    public void Launch_ValidRequest_RaisesLaunchAppliedAfterTargetLaunch()
    {
        var controller = CreateController();
        var notifier = (ISlingshotLaunchAppliedNotifier)controller;
        var appliedRequests = new List<SlingshotLaunchRequest>();

        notifier.LaunchApplied += request =>
        {
            appliedRequests.Add(request);
            _target.Observations.Add("launch-applied");
        };
        var request = CreateRequest(Vector3.forward, 8f, Vector3.up, 2f);

        controller.Launch(request);

        Assert.That(appliedRequests, Is.EqualTo(new[] { request }));
        Assert.That(_target.Observations, Is.EqualTo(new[] { "set-held-position", "launch", "launch-applied" }));
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
    public void Launch_InvalidFinalPullPoint_WarnsAndSkipsHeldPositionAndTargetLaunch()
    {
        var controller = CreateController();
        var request = CreateRequest(Vector3.forward, 8f, Vector3.up, 2f, new Vector3(float.NaN, 0f, 0f));
        LogAssert.Expect(LogType.Warning, new Regex("Invalid Slingshot launch request"));

        controller.Launch(request);

        Assert.That(_target.HeldPositions, Is.Empty);
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
    public void Launch_ValidRequest_DoesNotHoldTarget()
    {
        var controller = CreateController();
        var request = CreateRequest(Vector3.forward, 8f, Vector3.up, 2f);

        controller.Launch(request);

        Assert.That(_target.HoldCallCount, Is.Zero);
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
        return new SlingshotLaunchController(_target, _target);
    }

    private SlingshotLaunchRequest CreateRequest(Vector3 launchDirection, float launchSpeed, Vector3 launchUpDirection, float launchUpSpeed)
    {
        return CreateRequest(launchDirection, launchSpeed, launchUpDirection, launchUpSpeed, new Vector3(0f, 0f, -3f));
    }

    private SlingshotLaunchRequest CreateRequest(Vector3 launchDirection, float launchSpeed, Vector3 launchUpDirection, float launchUpSpeed,
        Vector3 finalPullPoint)
    {
        return new SlingshotLaunchRequest(1f, 3f, 0f, finalPullPoint, launchDirection, launchSpeed, launchUpDirection, launchUpSpeed);
    }

    private sealed class FakeLaunchTarget : ILaunchTarget, IHeldLaunchTarget
    {
        public int HoldCallCount { get; private set; }
        public bool ThrowOnLaunch { get; set; }
        public List<Vector3> LaunchVelocities { get; } = new();
        public List<Vector3> HeldPositions { get; } = new();
        public List<string> Observations { get; } = new();

        public void Hold()
        {
            HoldCallCount += 1;
        }

        public void Launch(Vector3 velocity)
        {
            Observations.Add("launch");

            if (ThrowOnLaunch)
                throw new InvalidOperationException("target failed");

            LaunchVelocities.Add(velocity);
        }

        public void SetHeldPosition(Vector3 heldPosition)
        {
            Observations.Add("set-held-position");
            HeldPositions.Add(heldPosition);
        }
    }
}
