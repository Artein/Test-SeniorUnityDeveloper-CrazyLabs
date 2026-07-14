using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunBodyMovementConfigTests
{
    private RunBodyMovementConfig _configObject;
    private IRunSteeringInputMetricsResolver _metricsResolver;
    private IRunSteeringConfig _steeringConfig;

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
        var attachmentConfig = (IRunSupportAttachmentAuthoringConfig)_configObject;
        var frameConfig = (IRunSteeringFrameAuthoringConfig)_configObject;

        Assert.That(speedConfig.DownhillAcceleration, Is.EqualTo(expected: 8f));
        Assert.That(speedConfig.SurfaceSlowdown, Is.EqualTo(expected: 0.5f));
        Assert.That(speedConfig.LowSpeedAssistTargetSpeed, Is.EqualTo(expected: 5f));
        Assert.That(speedConfig.LowSpeedAssistAcceleration, Is.EqualTo(expected: 8f));
        Assert.That(speedConfig.BaseSoftMaximumSpeed, Is.EqualTo(expected: 20f));
        Assert.That(speedConfig.AboveMaximumSpeedResistance, Is.EqualTo(expected: 12f));
        Assert.That(validityConfig.MaximumSupportedSurfaceNormalLiftSpeed, Is.EqualTo(expected: 0f));
        Assert.That(validityConfig.RunBodySpeedSanityGuardMetersPerSecond, Is.EqualTo(expected: 250f));
        Assert.That(landingConfig.LaunchLandingStabilizationSeconds, Is.EqualTo(expected: 0.3f));
        Assert.That(landingConfig.LaunchLandingMaximumLiftSpeed, Is.EqualTo(expected: 0f));
        Assert.That(_steeringConfig.RunSteeringRangeCentimeters, Is.EqualTo(expected: 1.5f));
        Assert.That(_steeringConfig.RunSteeringDeadzoneFraction, Is.EqualTo(expected: 0.15f));
        Assert.That(_steeringConfig.RunSteeringResponsiveness, Is.EqualTo(expected: 8f));
        Assert.That(_steeringConfig.FallbackDpi, Is.EqualTo(expected: 326f));
        Assert.That(_steeringConfig.MinimumAcceptedDpi, Is.EqualTo(expected: 1f));
        Assert.That(_steeringConfig.MaximumAcceptedDpi, Is.EqualTo(expected: 1000f));
        Assert.That(_steeringConfig.MaximumTurnDegreesPerSecond, Is.EqualTo(expected: 120f));
        Assert.That(_steeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond, Is.EqualTo(expected: 45f));
        Assert.That(_steeringConfig.MinimumSteerSpeed, Is.EqualTo(expected: 0.25f));
        Assert.That(stabilityConfig.SupportLossConfirmationSeconds, Is.EqualTo(expected: 0.08f));
        Assert.That(stabilityConfig.DiscontinuousNormalThresholdDegrees, Is.EqualTo(expected: 60f));
        Assert.That(stabilityConfig.DiscontinuousNormalConfirmationSeconds, Is.EqualTo(expected: 0.04f));
        Assert.That(stabilityConfig.CandidateCoherenceDegrees, Is.EqualTo(expected: 1f));
        Assert.That(attachmentConfig.MaximumAttachedSurfaceNormalLiftSpeed, Is.EqualTo(expected: 0.35f));
        Assert.That(attachmentConfig.SameSurfaceReattachmentSeparationMeters, Is.EqualTo(expected: 0.08f));
        Assert.That(attachmentConfig.MinimumReattachmentNormalChangeDegrees, Is.EqualTo(expected: 30f));
        Assert.That(attachmentConfig.TransitionConfirmationSeconds, Is.EqualTo(expected: 0.04f));
        Assert.That(frameConfig.NormalSlewDegreesPerSecond, Is.EqualTo(expected: 180f));
        Assert.That(frameConfig.AirborneUpRetentionSeconds, Is.EqualTo(expected: 0.12f));
    }

    [TestCase(arg: 1f)]
    [TestCase(arg: 96f)]
    [TestCase(arg: 326f)]
    [TestCase(arg: 1000f)]
    public void Resolve_ValidRawDpi_UsesRawValue(float rawDpi)
    {
        Assert.That(_metricsResolver.Resolve(rawDpi).ResolvedDpi, Is.EqualTo(rawDpi));
    }

    [TestCase(arg: 0f)]
    [TestCase(arg: -1f)]
    [TestCase(arg: 1000.1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void Resolve_InvalidRawDpi_UsesFallback(float rawDpi)
    {
        Assert.That(_metricsResolver.Resolve(rawDpi).ResolvedDpi, Is.EqualTo(expected: 326f));
    }

    [Test]
    public void Resolve_ValidDpi_ConvertsCentimetersAndPreservesDeadzone()
    {
        var metrics = _metricsResolver.Resolve(rawDpi: 96f);

        Assert.That(metrics.RangePixels, Is.EqualTo(1.5f / 2.54f * 96f).Within(amount: 0.0001f));
        Assert.That(metrics.DeadzoneFraction, Is.EqualTo(expected: 0.15f));
    }
}
