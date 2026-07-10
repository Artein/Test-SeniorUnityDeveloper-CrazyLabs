using System;
using Game.Gameplay;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunBodySpeedEnvelopeValidatorTests
{
    private RunBodySpeedEnvelopeValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new RunBodySpeedEnvelopeValidator(
            new FakeRunBodyMovementValidityConfig
            {
                RunBodySpeedSanityGuardMetersPerSecond = 250f,
            });
    }

    [TestCase(0.001f)]
    [TestCase(20f)]
    [TestCase(249.999f)]
    public void ValidateOrThrow_EnvelopeInsideGameplayRange_DoesNotThrow(float resolvedSoftMaximumSpeed)
    {
        Assert.That(
            () => _validator.ValidateOrThrow(resolvedSoftMaximumSpeed),
            Throws.Nothing);
    }

    [Test]
    public void ValidateOrThrow_InvalidEnvelopes_ThrowInvalidOperationException()
    {
        var invalidValues = new[]
        {
            float.NaN,
            float.PositiveInfinity,
            float.NegativeInfinity,
            -1f,
            0f,
            250f,
            251f,
        };

        foreach (var invalidValue in invalidValues)
        {
            Assert.That(
                () => _validator.ValidateOrThrow(invalidValue),
                Throws.TypeOf<InvalidOperationException>(),
                $"Expected {invalidValue} to be rejected.");
        }
    }

    private sealed class FakeRunBodyMovementValidityConfig : IRunBodyMovementValidityConfig
    {
        public float MaximumSupportedSurfaceNormalLiftSpeed { get; set; }
        public float RunBodySpeedSanityGuardMetersPerSecond { get; set; }
    }
}
