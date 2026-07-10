using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct RunSteeringContext
    {
        public Vector3 CurrentVelocity { get; }
        public Vector3 SteeringUp { get; }
        public RunSteeringMode SteeringMode { get; }
        public float SmoothedSteer { get; }
        public float MaximumTurnDegreesPerSecond { get; }
        public float MinimumSteerSpeed { get; }
        public float FixedDeltaTime { get; }
        public bool IsGestureActive { get; }

        public RunSteeringContext(
            Vector3 currentVelocity,
            Vector3 steeringUp,
            RunSteeringMode steeringMode,
            float smoothedSteer,
            float maximumTurnDegreesPerSecond,
            float minimumSteerSpeed,
            float fixedDeltaTime,
            bool isGestureActive)
        {
            CurrentVelocity = currentVelocity;
            SteeringUp = steeringUp;
            SteeringMode = steeringMode;
            SmoothedSteer = smoothedSteer;
            MaximumTurnDegreesPerSecond = maximumTurnDegreesPerSecond;
            MinimumSteerSpeed = minimumSteerSpeed;
            FixedDeltaTime = fixedDeltaTime;
            IsGestureActive = isGestureActive;
        }
    }

    internal readonly struct RunSteeringDecision
    {
        public bool ShouldApplySteering { get; }
        public Vector3 SteeringIntentDirection { get; }

        public RunSteeringDecision(bool shouldApplySteering, Vector3 steeringIntentDirection)
        {
            ShouldApplySteering = shouldApplySteering;
            SteeringIntentDirection = steeringIntentDirection;
        }
    }

    internal interface IRunSteeringEvaluator
    {
        RunSteeringDecision Evaluate(RunSteeringContext context);
    }

    internal sealed class DefaultRunSteeringEvaluator : IRunSteeringEvaluator
    {
        private readonly float _minimumDirectionSqrMagnitude = 0.000001f;
        private readonly float _minimumAirSteerMagnitude = 0.0001f;

        public RunSteeringDecision Evaluate(RunSteeringContext context)
        {
            if (!TryNormalize(context.SteeringUp, out var steeringUp))
                return default;

            var planarVelocity = Vector3.ProjectOnPlane(context.CurrentVelocity, steeringUp);
            var planarSpeed = planarVelocity.magnitude;

            if (!float.IsFinite(planarSpeed)
                || planarSpeed < context.MinimumSteerSpeed
                || !TryNormalize(planarVelocity, out var currentDirection))
            {
                return default;
            }

            if (context.SteeringMode == RunSteeringMode.Air
                && (!context.IsGestureActive || Mathf.Abs(context.SmoothedSteer) <= _minimumAirSteerMagnitude))
            {
                return default;
            }

            var turnAngle = context.SmoothedSteer
                            * context.MaximumTurnDegreesPerSecond
                            * Mathf.Max(0f, context.FixedDeltaTime);
            var intent = Quaternion.AngleAxis(turnAngle, steeringUp) * currentDirection;

            return TryNormalize(intent, out var intentDirection)
                ? new RunSteeringDecision(true, intentDirection)
                : default;
        }

        private bool TryNormalize(Vector3 direction, out Vector3 normalizedDirection)
        {
            normalizedDirection = Vector3.zero;
            var sqrMagnitude = direction.sqrMagnitude;

            if (!float.IsFinite(sqrMagnitude) || sqrMagnitude <= _minimumDirectionSqrMagnitude)
                return false;

            var normalized = direction.normalized;

            if (!float.IsFinite(normalized.x)
                || !float.IsFinite(normalized.y)
                || !float.IsFinite(normalized.z))
            {
                return false;
            }

            normalizedDirection = normalized;
            return true;
        }
    }
}
