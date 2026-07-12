using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunBodyMovementConfigTests
{
    private RunBodyMovementConfig _configObject;
    private IRunSteeringConfig _steeringConfig;
    private IRunSteeringInputMetricsResolver _metricsResolver;

    [SetUp]
    public void OnSetUp()
    {
        _configObject = ScriptableObject.CreateInstance<RunBodyMovementConfig>();
        _steeringConfig = _configObject;
        _metricsResolver = new DefaultRunSteeringInputMetricsResolver(_steeringConfig);
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.DestroyImmediate(_configObject);
    }

    [Test]
    public void DefaultValues_ExposeMovementBaseline()
    {
        var speedConfig = (IRunBodySpeedConfig)_configObject;
        var validityConfig = (IRunBodyMovementValidityConfig)_configObject;
        var landingConfig = (IRunLaunchLandingStabilizationConfig)_configObject;
        var stabilityConfig = (IRunSurfaceStabilityAuthoringConfig)_configObject;
        var frameConfig = (IRunSteeringFrameAuthoringConfig)_configObject;

        Assert.That(speedConfig.DownhillAcceleration, Is.EqualTo(8f));
        Assert.That(speedConfig.SurfaceSlowdown, Is.EqualTo(0.5f));
        Assert.That(speedConfig.LowSpeedAssistTargetSpeed, Is.EqualTo(5f));
        Assert.That(speedConfig.LowSpeedAssistAcceleration, Is.EqualTo(8f));
        Assert.That(speedConfig.BaseSoftMaximumSpeed, Is.EqualTo(20f));
        Assert.That(speedConfig.AboveMaximumSpeedResistance, Is.EqualTo(12f));
        Assert.That(validityConfig.MaximumSupportedSurfaceNormalLiftSpeed, Is.EqualTo(0f));
        Assert.That(validityConfig.RunBodySpeedSanityGuardMetersPerSecond, Is.EqualTo(250f));
        Assert.That(landingConfig.LaunchLandingStabilizationSeconds, Is.EqualTo(0.3f));
        Assert.That(landingConfig.LaunchLandingMaximumLiftSpeed, Is.EqualTo(0f));
        Assert.That(_steeringConfig.RunSteeringRangeCentimeters, Is.EqualTo(1.5f));
        Assert.That(_steeringConfig.RunSteeringDeadzoneFraction, Is.EqualTo(0.15f));
        Assert.That(_steeringConfig.RunSteeringResponsiveness, Is.EqualTo(8f));
        Assert.That(_steeringConfig.FallbackDpi, Is.EqualTo(326f));
        Assert.That(_steeringConfig.MinimumAcceptedDpi, Is.EqualTo(1f));
        Assert.That(_steeringConfig.MaximumAcceptedDpi, Is.EqualTo(1000f));
        Assert.That(_steeringConfig.MaximumTurnDegreesPerSecond, Is.EqualTo(120f));
        Assert.That(_steeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond, Is.EqualTo(45f));
        Assert.That(_steeringConfig.MinimumSteerSpeed, Is.EqualTo(0.25f));
        Assert.That(stabilityConfig.SupportLossConfirmationSeconds, Is.EqualTo(0.08f));
        Assert.That(stabilityConfig.DiscontinuousNormalThresholdDegrees, Is.EqualTo(60f));
        Assert.That(stabilityConfig.DiscontinuousNormalConfirmationSeconds, Is.EqualTo(0.04f));
        Assert.That(stabilityConfig.CandidateCoherenceDegrees, Is.EqualTo(1f));
        Assert.That(frameConfig.NormalSlewDegreesPerSecond, Is.EqualTo(180f));
        Assert.That(frameConfig.AirborneUpRetentionSeconds, Is.EqualTo(0.12f));
    }

    [TestCase(1f)]
    [TestCase(96f)]
    [TestCase(326f)]
    [TestCase(1000f)]
    public void Resolve_ValidRawDpi_UsesRawValue(float rawDpi)
    {
        Assert.That(_metricsResolver.Resolve(rawDpi).ResolvedDpi, Is.EqualTo(rawDpi));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(1000.1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void Resolve_InvalidRawDpi_UsesFallback(float rawDpi)
    {
        Assert.That(_metricsResolver.Resolve(rawDpi).ResolvedDpi, Is.EqualTo(326f));
    }

    [Test]
    public void Resolve_ValidDpi_ConvertsCentimetersAndPreservesDeadzone()
    {
        var metrics = _metricsResolver.Resolve(96f);

        Assert.That(metrics.RangePixels, Is.EqualTo(1.5f / 2.54f * 96f).Within(0.0001f));
        Assert.That(metrics.DeadzoneFraction, Is.EqualTo(0.15f));
    }
}
