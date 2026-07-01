#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Foundation.Presentation
{
    public sealed partial class Spinner
    {
        internal float InitialPhaseDegreesForTests => GetInitialPhaseDegrees();

        internal Transform RotationTargetForTests => RotationTarget;

        internal void SetValuesForTests(
            Transform target,
            Vector3 localAxis,
            float degreesPerSecond,
            bool useDeterministicInitialPhase,
            float authoredInitialPhaseOffsetDegrees)
        {
            _target = target;
            _localAxis = localAxis;
            _degreesPerSecond = degreesPerSecond;
            _useDeterministicInitialPhase = useDeterministicInitialPhase;
            _authoredInitialPhaseOffsetDegrees = authoredInitialPhaseOffsetDegrees;
        }

        internal void InitializeForTests()
        {
            ApplyInitialPhase();
        }

        internal void TickForTests(float deltaTime)
        {
            Tick(deltaTime);
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
