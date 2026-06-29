#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class GameplaySlingshotLaunchConfig
    {
        internal void SetValuesForTests(
            float minimumForwardImpulse,
            float maximumForwardImpulse,
            AnimationCurve pullStrengthCurve,
            float maximumLateralLaunchAngleDegrees,
            AnimationCurve lateralAngleCurve,
            float upwardImpulse,
            bool hasMinimumTotalImpulse,
            float minimumTotalImpulse,
            bool hasMaximumTotalImpulse,
            float maximumTotalImpulse)
        {
            _minimumForwardImpulse = minimumForwardImpulse;
            _maximumForwardImpulse = maximumForwardImpulse;
            _pullStrengthCurve = pullStrengthCurve;
            _maximumLateralLaunchAngleDegrees = maximumLateralLaunchAngleDegrees;
            _lateralAngleCurve = lateralAngleCurve;
            _upwardImpulse = upwardImpulse;
            _hasMinimumTotalImpulse = hasMinimumTotalImpulse;
            _minimumTotalImpulse = minimumTotalImpulse;
            _hasMaximumTotalImpulse = hasMaximumTotalImpulse;
            _maximumTotalImpulse = maximumTotalImpulse;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
