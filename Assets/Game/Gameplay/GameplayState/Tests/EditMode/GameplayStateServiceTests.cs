using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay.GameplayState.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class GameplayStateServiceTests : GameplayStateTestFixture
{
    [Test]
    public void Constructor_ValidConfig_InitializesCurrentState()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var config = CreateConfig(preLaunch, CreateTransition(preLaunch, running));

        var service = CreateService(config);

        Assert.That(service.CurrentStateId, Is.SameAs(preLaunch));
        Assert.That(service.IsCurrent(preLaunch), Is.True);
        Assert.That(service.IsCurrent(running), Is.False);
    }

    [Test]
    public void TryTransitionTo_AllowedTransition_RaisesChangingThenChangesThenRaisesChanged()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var config = CreateConfig(preLaunch, CreateTransition(preLaunch, running));
        var service = CreateService(config);
        var observations = new List<string>();

        service.GameplayStateChanging += (nextStateId, previousStateId) =>
        {
            observations.Add($"changing:{previousStateId.name}->{nextStateId.name}:current={service.CurrentStateId.name}");
        };

        service.GameplayStateChanged += (nextStateId, previousStateId) =>
        {
            observations.Add($"changed:{previousStateId.name}->{nextStateId.name}:current={service.CurrentStateId.name}");
        };

        var result = service.TryTransitionTo(running);

        Assert.That(result, Is.True);
        Assert.That(service.CurrentStateId, Is.SameAs(running));

        Assert.That(
            observations,
            Is.EqualTo(new[]
            {
                "changing:Pre-Launch->Running:current=Pre-Launch",
                "changed:Pre-Launch->Running:current=Running"
            }));
    }

    [Test]
    public void TryTransitionTo_SameState_ReturnsFalseWithoutEventsOrWarning()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var config = CreateConfig(preLaunch, CreateTransition(preLaunch, running));
        var service = CreateService(config);
        var eventCount = 0;

        service.GameplayStateChanging += (_, _) => eventCount += 1;
        service.GameplayStateChanged += (_, _) => eventCount += 1;

        var result = service.TryTransitionTo(preLaunch);

        Assert.That(result, Is.False);
        Assert.That(eventCount, Is.Zero);
        Assert.That(service.CurrentStateId, Is.SameAs(preLaunch));
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void TryTransitionTo_InvalidTransition_ReturnsFalseWarnsAndPreservesCurrentState()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var runEnded = CreateStateId("Run Ended");
        var config = CreateConfig(preLaunch, CreateTransition(preLaunch, running));
        var service = CreateService(config);

        LogAssert.Expect(LogType.Warning, new Regex("Invalid Gameplay State transition"));

        var result = service.TryTransitionTo(runEnded);

        Assert.That(result, Is.False);
        Assert.That(service.CurrentStateId, Is.SameAs(preLaunch));
    }

    [Test]
    public void TryTransitionTo_ChangingSubscriberThrows_LogsAndCompletesTransition()
    {
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var config = CreateConfig(preLaunch, CreateTransition(preLaunch, running));
        var service = CreateService(config);

        service.GameplayStateChanging += (_, _) => throw new InvalidOperationException("Changing failed");
        LogAssert.Expect(LogType.Exception, new Regex("Changing failed"));

        var result = service.TryTransitionTo(running);

        Assert.That(result, Is.True);
        Assert.That(service.CurrentStateId, Is.SameAs(running));
    }
}
