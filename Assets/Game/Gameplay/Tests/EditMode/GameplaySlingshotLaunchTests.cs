using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class GameplaySlingshotLaunchTests
{
    private readonly List<Object> _objects = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void Calculate_MinPull_UsesMinimumForwardAndNoUpwardImpulse()
    {
        var config = CreateConfig(minimumForwardImpulse: 8f, maximumForwardImpulse: 35f, upwardImpulse: 3f);
        var calculator = new SlingshotLaunchImpulseCalculator();
        var request = CreateRequest(pullStrength: 0f, normalizedLateralPull: 0f);

        var impulse = calculator.Calculate(request, config, slingshotLaunchPower: 1f);

        Assert.That(impulse.ForwardImpulse, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(impulse.UpwardImpulse, Is.EqualTo(0f).Within(0.0001f));
        AssertVector3(impulse.LaunchDirection, Vector3.forward);
        AssertVector3(impulse.LaunchUpDirection, Vector3.up);
        AssertVector3(impulse.VelocityChange, new Vector3(0f, 0f, 8f));
    }

    [Test]
    public void Calculate_MaxPull_UsesMaximumForwardImpulse()
    {
        var config = CreateConfig(minimumForwardImpulse: 8f, maximumForwardImpulse: 35f, upwardImpulse: 3f);
        var calculator = new SlingshotLaunchImpulseCalculator();
        var request = CreateRequest(pullStrength: 1f, normalizedLateralPull: 0f);

        var impulse = calculator.Calculate(request, config, slingshotLaunchPower: 1f);

        Assert.That(impulse.ForwardImpulse, Is.EqualTo(35f).Within(0.0001f));
        Assert.That(impulse.UpwardImpulse, Is.EqualTo(3f).Within(0.0001f));
        AssertVector3(impulse.VelocityChange, new Vector3(0f, 3f, 35f));
    }

    [Test]
    public void Calculate_MidPull_ScalesForwardAndUpwardImpulseBetweenMinimumAndMaximum()
    {
        var config = CreateConfig(minimumForwardImpulse: 8f, maximumForwardImpulse: 35f, upwardImpulse: 3f);
        var calculator = new SlingshotLaunchImpulseCalculator();
        var request = CreateRequest(pullStrength: 0.5f, normalizedLateralPull: 0f);

        var impulse = calculator.Calculate(request, config, slingshotLaunchPower: 1f);

        Assert.That(impulse.ForwardImpulse, Is.EqualTo(21.5f).Within(0.0001f));
        Assert.That(impulse.UpwardImpulse, Is.EqualTo(1.5f).Within(0.0001f));
        AssertVector3(impulse.VelocityChange, new Vector3(0f, 1.5f, 21.5f));
    }

    [Test]
    public void Calculate_PositiveLateralPull_SteersOppositePullOffset()
    {
        var config = CreateConfig(
            minimumForwardImpulse: 10f,
            maximumForwardImpulse: 10f,
            upwardImpulse: 0f,
            maximumLateralLaunchAngleDegrees: 30f);
        var calculator = new SlingshotLaunchImpulseCalculator();
        var request = CreateRequest(pullStrength: 1f, normalizedLateralPull: 1f);
        var expectedDirection = Quaternion.AngleAxis(-30f, Vector3.up) * Vector3.forward;

        var impulse = calculator.Calculate(request, config, slingshotLaunchPower: 1f);

        Assert.That(impulse.LaunchDirection.x, Is.LessThan(0f));
        AssertVector3(impulse.LaunchDirection, expectedDirection.normalized);
        AssertVector3(impulse.VelocityChange, expectedDirection.normalized * 10f);
    }

    [Test]
    public void Calculate_LaunchPowerMultiplier_ScalesForwardAndUpwardImpulse()
    {
        var config = CreateConfig(minimumForwardImpulse: 8f, maximumForwardImpulse: 35f, upwardImpulse: 3f);
        var calculator = new SlingshotLaunchImpulseCalculator();
        var request = CreateRequest(pullStrength: 1f, normalizedLateralPull: 0f);

        var impulse = calculator.Calculate(request, config, slingshotLaunchPower: 2f);

        Assert.That(impulse.ForwardImpulse, Is.EqualTo(70f).Within(0.0001f));
        Assert.That(impulse.UpwardImpulse, Is.EqualTo(6f).Within(0.0001f));
        AssertVector3(impulse.VelocityChange, new Vector3(0f, 6f, 70f));
    }

    [Test]
    public void Calculate_MaximumTotalImpulseClamp_ClampsVelocityMagnitudeAfterLaunchPower()
    {
        var config = CreateConfig(
            minimumForwardImpulse: 12f,
            maximumForwardImpulse: 12f,
            upwardImpulse: 0f,
            hasMaximumTotalImpulse: true,
            maximumTotalImpulse: 5f);
        var calculator = new SlingshotLaunchImpulseCalculator();
        var request = CreateRequest(pullStrength: 1f, normalizedLateralPull: 0f);

        var impulse = calculator.Calculate(request, config, slingshotLaunchPower: 2f);

        Assert.That(impulse.VelocityChange.magnitude, Is.EqualTo(5f).Within(0.0001f));
        AssertVector3(impulse.LaunchDirection, Vector3.forward);
    }

    [Test]
    public void Calculate_SameInputs_ReturnsDeterministicImpulse()
    {
        var config = CreateConfig(minimumForwardImpulse: 4f, maximumForwardImpulse: 12f, upwardImpulse: 1.5f);
        var calculator = new SlingshotLaunchImpulseCalculator();
        var request = CreateRequest(pullStrength: 0.6f, normalizedLateralPull: -0.5f);

        var first = calculator.Calculate(request, config, slingshotLaunchPower: 1.25f);
        var second = calculator.Calculate(request, config, slingshotLaunchPower: 1.25f);

        Assert.That(second.ForwardImpulse, Is.EqualTo(first.ForwardImpulse).Within(0.0001f));
        Assert.That(second.UpwardImpulse, Is.EqualTo(first.UpwardImpulse).Within(0.0001f));
        AssertVector3(second.LaunchDirection, first.LaunchDirection);
        AssertVector3(second.LaunchUpDirection, first.LaunchUpDirection);
        AssertVector3(second.VelocityChange, first.VelocityChange);
    }

    [Test]
    public void Calculate_SkewedLaunchFrame_ThrowsArgumentException()
    {
        var config = CreateConfig(minimumForwardImpulse: 4f, maximumForwardImpulse: 12f, upwardImpulse: 1.5f);
        var calculator = new SlingshotLaunchImpulseCalculator();

        var request = new SlingshotLaunchRequest(
            1f,
            1f,
            0f,
            0f,
            Vector3.zero,
            new Vector3(0f, 0.2f, 0.98f).normalized,
            Vector3.up);

        Assert.That(
            () => calculator.Calculate(request, config, slingshotLaunchPower: 1f),
            Throws.ArgumentException.With.Message.Contains("perpendicular"));
    }

    [Test]
    public void Launch_ResolvedSlingshotLaunchPowerModifier_AppliesScaledImpulseAndPublishesAppliedEvent()
    {
        var statId = CreateStatId("slingshot_launch_power");
        var config = CreateConfig(minimumForwardImpulse: 5f, maximumForwardImpulse: 5f, upwardImpulse: 1f);
        var applier = new FakeLaunchImpulseApplier();
        var publisher = new FakeSlingshotLaunchAppliedPublisher();

        var snapshot = new RunModifierSnapshot(new[]
        {
            new GameplayStatModifier(statId, GameplayStatModifierOperation.MultiplicativeFactor, 2f)
        });

        var launcher = new GameplaySlingshotLauncher(
            config,
            new SlingshotLaunchImpulseCalculator(),
            applier,
            new RunGameplayStatResolver(new FakeRunModifierSnapshotProvider(snapshot)),
            statId,
            publisher);
        var request = CreateRequest(pullStrength: 1f, normalizedLateralPull: 0f);

        launcher.Launch(request);

        Assert.That(applier.AppliedRequests, Is.EqualTo(new[] { request }));
        Assert.That(applier.AppliedImpulses, Has.Count.EqualTo(1));
        AssertVector3(applier.AppliedImpulses[0].VelocityChange, new Vector3(0f, 2f, 10f));
        Assert.That(publisher.AppliedEvents, Has.Count.EqualTo(1));
        Assert.That(publisher.AppliedEvents[0].Request, Is.EqualTo(request));
        AssertVector3(publisher.AppliedEvents[0].VelocityChange, applier.AppliedImpulses[0].VelocityChange);
        AssertVector3(publisher.AppliedEvents[0].LaunchDirection, Vector3.forward);
        AssertVector3(publisher.AppliedEvents[0].LaunchUpDirection, Vector3.up);
    }

    private GameplaySlingshotLaunchConfig CreateConfig(
        float minimumForwardImpulse,
        float maximumForwardImpulse,
        float upwardImpulse,
        float maximumLateralLaunchAngleDegrees = 35f,
        bool hasMaximumTotalImpulse = false,
        float maximumTotalImpulse = 0f)
    {
        var config = Track(ScriptableObject.CreateInstance<GameplaySlingshotLaunchConfig>());

        config.SetValuesForTests(
            minimumForwardImpulse,
            maximumForwardImpulse,
            AnimationCurve.Linear(0f, 0f, 1f, 1f),
            maximumLateralLaunchAngleDegrees,
            AnimationCurve.Linear(0f, 0f, 1f, 1f),
            upwardImpulse,
            false,
            0f,
            hasMaximumTotalImpulse,
            maximumTotalImpulse);
        return config;
    }

    private SlingshotLaunchRequest CreateRequest(float pullStrength, float normalizedLateralPull)
    {
        return new SlingshotLaunchRequest(
            pullStrength,
            1.25f,
            normalizedLateralPull,
            normalizedLateralPull,
            new Vector3(normalizedLateralPull, 0f, -1.25f),
            Vector3.forward,
            Vector3.up);
    }

    private GameplayStatId CreateStatId(string id)
    {
        var statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        statId.SetValuesForTests(id);
        return statId;
    }

    private T Track<T>(T value)
        where T : Object
    {
        _objects.Add(value);
        return value;
    }

    private void AssertVector3(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
    }

    private sealed class FakeLaunchImpulseApplier : ILaunchImpulseApplier
    {
        public List<SlingshotLaunchRequest> AppliedRequests { get; } = new();
        public List<LaunchImpulse> AppliedImpulses { get; } = new();

        public void Apply(SlingshotLaunchRequest request, LaunchImpulse impulse)
        {
            AppliedRequests.Add(request);
            AppliedImpulses.Add(impulse);
        }
    }

    private sealed class FakeRunModifierSnapshotProvider : IRunModifierSnapshotProvider
    {
        public FakeRunModifierSnapshotProvider(RunModifierSnapshot snapshot)
        {
            CurrentSnapshot = snapshot;
        }

        public RunModifierSnapshot CurrentSnapshot { get; }
    }

    private sealed class FakeSlingshotLaunchAppliedPublisher : ISlingshotLaunchAppliedPublisher
    {
        public List<SlingshotLaunchAppliedEvent> AppliedEvents { get; } = new();

        public void Publish(SlingshotLaunchAppliedEvent appliedEvent)
        {
            AppliedEvents.Add(appliedEvent);
        }
    }
}
