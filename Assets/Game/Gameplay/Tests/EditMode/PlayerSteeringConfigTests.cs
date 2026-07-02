using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PlayerSteeringConfigTests
{
    private PlayerSteeringConfig _configObject;
    private IPlayerSteeringConfig _config;

    [SetUp]
    public void OnSetUp()
    {
        _configObject = ScriptableObject.CreateInstance<PlayerSteeringConfig>();
        _config = _configObject;
    }

    [TearDown]
    public void OnTearDown()
    {
        Object.DestroyImmediate(_configObject);
    }

    [Test]
    public void DefaultValues_ExposeRunSteeringBaseline()
    {
        Assert.That(_config.RunSteeringRangeCentimeters, Is.EqualTo(1.5f));
        Assert.That(_config.RunSteeringDeadzoneFraction, Is.EqualTo(0.15f));
        Assert.That(_config.RunSteeringResponsiveness, Is.EqualTo(8f));
        Assert.That(_config.FallbackDpi, Is.EqualTo(326f));
        Assert.That(_config.MinimumAcceptedDpi, Is.EqualTo(1f));
        Assert.That(_config.MaximumAcceptedDpi, Is.EqualTo(1000f));
        Assert.That(_config.LaunchBurstPlanarSpeedGraceSeconds, Is.EqualTo(0.35f));
        Assert.That(_config.LaunchBurstPlanarSpeedRecoverySeconds, Is.EqualTo(0.65f));
        Assert.That(_config.LaunchBurstMaximumPlanarSpeedMultiplier, Is.EqualTo(3f));
        Assert.That(_config.RunSteeringFrameNormalSlewDegreesPerSecond, Is.EqualTo(180f));
        Assert.That(_config.RunSteeringFrameSnapDegrees, Is.EqualTo(60f));
        Assert.That(_config.RunSteeringFrameUngroundedGraceSeconds, Is.EqualTo(0.08f));
        Assert.That(_config.RunSteeringFrameSuspectNormalConfirmationSeconds, Is.EqualTo(0.04f));
    }

    [Test]
    public void RunSteeringFrameStabilityValues_InvalidAuthoredValues_ResolveDefensively()
    {
        _configObject.SetRunSteeringFrameStabilityForTests(
            normalSlewDegreesPerSecond: float.NaN,
            snapDegrees: 240f,
            ungroundedGraceSeconds: -1f,
            suspectNormalConfirmationSeconds: float.PositiveInfinity);

        Assert.That(_config.RunSteeringFrameNormalSlewDegreesPerSecond, Is.EqualTo(180f));
        Assert.That(_config.RunSteeringFrameSnapDegrees, Is.EqualTo(180f));
        Assert.That(_config.RunSteeringFrameUngroundedGraceSeconds, Is.EqualTo(0.08f));
        Assert.That(_config.RunSteeringFrameSuspectNormalConfirmationSeconds, Is.EqualTo(0.04f));
    }

    [Test]
    public void LaunchBurstValues_InvalidAuthoredValues_ResolveDefensively()
    {
        _configObject.SetLaunchBurstForTests(
            graceSeconds: float.NaN,
            recoverySeconds: -1f,
            maximumPlanarSpeedMultiplier: 0.5f);

        Assert.That(_config.LaunchBurstPlanarSpeedGraceSeconds, Is.EqualTo(0.35f));
        Assert.That(_config.LaunchBurstPlanarSpeedRecoverySeconds, Is.EqualTo(0.65f));
        Assert.That(_config.LaunchBurstMaximumPlanarSpeedMultiplier, Is.EqualTo(1f));
    }

    [TestCase(1f)]
    [TestCase(96f)]
    [TestCase(326f)]
    [TestCase(1000f)]
    public void ResolveRunSteeringDpi_ValidRawDpi_UsesRawValue(float rawDpi)
    {
        Assert.That(_config.ResolveRunSteeringDpi(rawDpi), Is.EqualTo(rawDpi));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(1000.1f)]
    public void ResolveRunSteeringDpi_InvalidRawDpi_UsesFallback(float rawDpi)
    {
        Assert.That(_config.ResolveRunSteeringDpi(rawDpi), Is.EqualTo(326f));
    }

    [Test]
    public void ResolveRunSteeringDpi_NaN_UsesFallback()
    {
        Assert.That(_config.ResolveRunSteeringDpi(float.NaN), Is.EqualTo(326f));
    }

    [Test]
    public void ResolveRunSteeringDpi_Infinity_UsesFallback()
    {
        Assert.That(_config.ResolveRunSteeringDpi(float.PositiveInfinity), Is.EqualTo(326f));
    }

    [Test]
    public void ResolveRunSteeringRangePixels_ValidDpi_ConvertsCentimetersToPixels()
    {
        Assert.That(_config.ResolveRunSteeringRangePixels(96f), Is.EqualTo(1.5f / 2.54f * 96f).Within(0.0001f));
    }

    [Test]
    public void ResolveRunSteeringRangePixels_InvalidDpi_ConvertsWithFallbackDpi()
    {
        Assert.That(_config.ResolveRunSteeringRangePixels(0f), Is.EqualTo(1.5f / 2.54f * 326f).Within(0.0001f));
    }
}
