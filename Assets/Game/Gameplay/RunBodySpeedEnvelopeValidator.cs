using System;

namespace Game.Gameplay
{
    public sealed class RunBodySpeedEnvelopeValidator
    {
        private readonly IRunBodyMovementValidityConfig _movementValidityConfig;

        public RunBodySpeedEnvelopeValidator(IRunBodyMovementValidityConfig movementValidityConfig)
        {
            _movementValidityConfig = movementValidityConfig
                                      ?? throw new ArgumentNullException(nameof(movementValidityConfig));
        }

        public void ValidateOrThrow(float resolvedSoftMaximumSpeed)
        {
            var sanityGuard = _movementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond;

            if (!float.IsFinite(sanityGuard) || sanityGuard <= 0f)
            {
                throw new InvalidOperationException(
                    "Run Body Speed Sanity Guard must be finite and positive before validating a gameplay speed envelope.");
            }

            if (float.IsFinite(resolvedSoftMaximumSpeed)
                && resolvedSoftMaximumSpeed > 0f
                && resolvedSoftMaximumSpeed < sanityGuard)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Resolved Run Body Speed Envelope must be finite, positive, and below the " +
                $"Run Body Speed Sanity Guard ({sanityGuard}), but was {resolvedSoftMaximumSpeed}.");
        }
    }
}
