#if UNITY_INCLUDE_TESTS

namespace Game.Gameplay
{
    public sealed partial class RunBodyMovementConfig
    {
        internal void SetSpeedValuesForTests(
            float downhillAcceleration,
            float surfaceSlowdown,
            float lowSpeedAssistTargetSpeed,
            float lowSpeedAssistAcceleration,
            float baseSoftMaximumSpeed,
            float aboveMaximumSpeedResistance)
        {
            _downhillAcceleration = downhillAcceleration;
            _surfaceSlowdown = surfaceSlowdown;
            _lowSpeedAssistTargetSpeed = lowSpeedAssistTargetSpeed;
            _lowSpeedAssistAcceleration = lowSpeedAssistAcceleration;
            _baseSoftMaximumSpeed = baseSoftMaximumSpeed;
            _aboveMaximumSpeedResistance = aboveMaximumSpeedResistance;
        }

        internal void SetMovementValidityValuesForTests(
            float maximumSupportedSurfaceNormalLiftSpeed,
            float runBodySpeedSanityGuardMetersPerSecond)
        {
            _maximumSupportedSurfaceNormalLiftSpeed = maximumSupportedSurfaceNormalLiftSpeed;
            _runBodySpeedSanityGuardMetersPerSecond = runBodySpeedSanityGuardMetersPerSecond;
        }

        internal void SetSteeringInputValuesForTests(
            float rangeCentimeters,
            float deadzoneFraction,
            float responsiveness,
            float fallbackDpi,
            float minimumAcceptedDpi,
            float maximumAcceptedDpi)
        {
            _runSteeringRangeCentimeters = rangeCentimeters;
            _runSteeringDeadzoneFraction = deadzoneFraction;
            _runSteeringResponsiveness = responsiveness;
            _fallbackDpi = fallbackDpi;
            _minimumAcceptedDpi = minimumAcceptedDpi;
            _maximumAcceptedDpi = maximumAcceptedDpi;
        }

        internal void SetRunSupportAttachmentForTests(
            float maximumAttachedSurfaceNormalLiftSpeed,
            float sameSurfaceReattachmentSeparationMeters,
            float minimumReattachmentNormalChangeDegrees,
            float transitionConfirmationSeconds)
        {
            _runSupportMaximumAttachedSurfaceNormalLiftSpeed = maximumAttachedSurfaceNormalLiftSpeed;
            _runSupportSameSurfaceReattachmentSeparationMeters = sameSurfaceReattachmentSeparationMeters;
            _runSupportMinimumReattachmentNormalChangeDegrees = minimumReattachmentNormalChangeDegrees;
            _runSupportAttachmentTransitionConfirmationSeconds = transitionConfirmationSeconds;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
