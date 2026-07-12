using System.Linq;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunBodyMovementConfigValidatorTests
{
    private RunBodyMovementConfig _config;
    private RunBodyMovementConfigValidator _validator;

    [SetUp]
    public void OnSetUp()
    {
        _config = ScriptableObject.CreateInstance<RunBodyMovementConfig>();
        _validator = new RunBodyMovementConfigValidator();
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.DestroyImmediate(_config);
    }

    [Test]
    public void Validate_DefaultConfig_ReturnsNoErrors()
    {
        Assert.That(_validator.Validate(_config), Is.Empty);
    }

    [Test]
    public void Config_DefaultConfig_ExposesEveryNarrowView()
    {
        Assert.That(_config, Is.InstanceOf<IRunBodySpeedConfig>());
        Assert.That(_config, Is.InstanceOf<IRunBodyMovementValidityConfig>());
        Assert.That(_config, Is.InstanceOf<IRunLaunchLandingStabilizationConfig>());
        Assert.That(_config, Is.InstanceOf<IRunSteeringConfig>());
        Assert.That(_config, Is.InstanceOf<IRunSurfaceStabilityAuthoringConfig>());
        Assert.That(_config, Is.InstanceOf<IRunSteeringFrameAuthoringConfig>());
    }

    [Test]
    public void Config_InvalidAuthoredValues_ReturnsRawValuesWithoutRepair()
    {
        _config.SetSpeedValuesForTests(
            downhillAcceleration: float.NaN,
            surfaceSlowdown: -1f,
            lowSpeedAssistTargetSpeed: 30f,
            lowSpeedAssistAcceleration: -2f,
            baseSoftMaximumSpeed: 20f,
            aboveMaximumSpeedResistance: float.PositiveInfinity);

        var speed = (IRunBodySpeedConfig)_config;

        Assert.That(speed.DownhillAcceleration, Is.NaN);
        Assert.That(speed.SurfaceSlowdown, Is.EqualTo(-1f));
        Assert.That(speed.LowSpeedAssistTargetSpeed, Is.EqualTo(30f));
        Assert.That(speed.LowSpeedAssistAcceleration, Is.EqualTo(-2f));
        Assert.That(speed.BaseSoftMaximumSpeed, Is.EqualTo(20f));
        Assert.That(speed.AboveMaximumSpeedResistance, Is.EqualTo(float.PositiveInfinity));
    }

    [Test]
    public void Validate_MultipleInvalidValues_ReportsEveryError()
    {
        _config.SetSpeedValuesForTests(
            downhillAcceleration: float.NaN,
            surfaceSlowdown: -1f,
            lowSpeedAssistTargetSpeed: float.PositiveInfinity,
            lowSpeedAssistAcceleration: -2f,
            baseSoftMaximumSpeed: 0f,
            aboveMaximumSpeedResistance: -3f);

        _config.SetMovementValidityValuesForTests(
            maximumSupportedSurfaceNormalLiftSpeed: -1f,
            runBodySpeedSanityGuardMetersPerSecond: 0f);

        _config.SetSteeringInputValuesForTests(
            rangeCentimeters: 0f,
            deadzoneFraction: 1f,
            responsiveness: -1f,
            fallbackDpi: 20f,
            minimumAcceptedDpi: 100f,
            maximumAcceptedDpi: 50f);

        var errors = _validator.Validate(_config).ToArray();

        Assert.That(errors, Has.Some.Contains(nameof(IRunBodySpeedConfig.DownhillAcceleration)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunBodySpeedConfig.SurfaceSlowdown)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunBodySpeedConfig.LowSpeedAssistTargetSpeed)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunBodySpeedConfig.LowSpeedAssistAcceleration)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunBodySpeedConfig.BaseSoftMaximumSpeed)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunBodySpeedConfig.AboveMaximumSpeedResistance)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunBodyMovementValidityConfig.MaximumSupportedSurfaceNormalLiftSpeed)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunBodyMovementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunSteeringConfig.RunSteeringRangeCentimeters)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunSteeringConfig.RunSteeringDeadzoneFraction)));
        Assert.That(errors, Has.Some.Contains(nameof(IRunSteeringConfig.RunSteeringResponsiveness)));
        Assert.That(errors, Has.Some.Contains("minimum accepted DPI"));
        Assert.That(errors, Has.Some.Contains("fallback DPI"));
    }

    [Test]
    public void Validate_BaseEnvelopeAtSanityGuard_ReturnsCrossFieldError()
    {
        _config.SetSpeedValuesForTests(
            downhillAcceleration: 1f,
            surfaceSlowdown: 1f,
            lowSpeedAssistTargetSpeed: 1f,
            lowSpeedAssistAcceleration: 1f,
            baseSoftMaximumSpeed: 250f,
            aboveMaximumSpeedResistance: 1f);

        var errors = _validator.Validate(_config).ToArray();

        Assert.That(errors, Has.Some.Contains("below the Run Body Speed Sanity Guard"));
    }

    [Test]
    public void Validate_AssistTargetAboveBaseEnvelope_ReturnsNoError()
    {
        _config.SetSpeedValuesForTests(
            downhillAcceleration: 1f,
            surfaceSlowdown: 1f,
            lowSpeedAssistTargetSpeed: 30f,
            lowSpeedAssistAcceleration: 1f,
            baseSoftMaximumSpeed: 20f,
            aboveMaximumSpeedResistance: 1f);

        Assert.That(_validator.Validate(_config), Is.Empty);
    }
}
