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
    [TearDown]
    public void OnTearDown()
    {
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void Publish_ValidAppliedEvent_RaisesLaunchApplied()
    {
        var controller = new SlingshotLaunchController();
        var notifier = (ISlingshotLaunchAppliedNotifier)controller;
        var publisher = (ISlingshotLaunchAppliedPublisher)controller;
        var appliedEvents = new List<SlingshotLaunchAppliedEvent>();
        var appliedEvent = CreateAppliedEvent();

        notifier.LaunchApplied += appliedEvents.Add;

        publisher.Publish(appliedEvent);

        Assert.That(appliedEvents, Is.EqualTo(new[] { appliedEvent }));
    }

    [Test]
    public void Publish_WhenSubscriberThrows_LogsAndNotifiesRemainingSubscribers()
    {
        var controller = new SlingshotLaunchController();
        var notifier = (ISlingshotLaunchAppliedNotifier)controller;
        var publisher = (ISlingshotLaunchAppliedPublisher)controller;
        var receivedCount = 0;
        var appliedEvent = CreateAppliedEvent();
        notifier.LaunchApplied += _ => throw new InvalidOperationException("subscriber failed");
        notifier.LaunchApplied += _ => receivedCount += 1;
        LogAssert.Expect(LogType.Exception, new Regex("subscriber failed"));

        publisher.Publish(appliedEvent);

        Assert.That(receivedCount, Is.EqualTo(1));
    }

    private SlingshotLaunchAppliedEvent CreateAppliedEvent()
    {
        return new SlingshotLaunchAppliedEvent(
            CreateRequest(),
            new Vector3(0f, 1f, 8f),
            Vector3.forward,
            Vector3.up);
    }

    private SlingshotLaunchRequest CreateRequest()
    {
        return new SlingshotLaunchRequest(1f, 3f, 0f, 0f, new Vector3(0f, 0f, -3f), Vector3.forward, Vector3.up);
    }
}
