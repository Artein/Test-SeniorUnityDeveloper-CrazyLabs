using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct RunSteeringContext
    {
        public float TangentSpeed { get; }
        public bool HasUsableTangentDirection { get; }
        public RunSteeringMode SteeringMode { get; }
        public float SmoothedSteer { get; }
        public float MaximumTurnDegreesPerSecond { get; }
        public float MinimumSteerSpeed { get; }
        public float FixedDeltaTime { get; }
        public bool IsGestureActive { get; }

        public RunSteeringContext(
            float tangentSpeed,
            bool hasUsableTangentDirection,
            RunSteeringMode steeringMode,
            float smoothedSteer,
            float maximumTurnDegreesPerSecond,
            float minimumSteerSpeed,
            float fixedDeltaTime,
            bool isGestureActive)
        {
            TangentSpeed = tangentSpeed;
            HasUsableTangentDirection = hasUsableTangentDirection;
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
        public bool ShouldTurnVelocity { get; }
        public float SignedTurnDegrees { get; }
        public bool ShouldUpdateFacing { get; }

        public RunSteeringDecision(
            bool shouldTurnVelocity,
            float signedTurnDegrees,
            bool shouldUpdateFacing)
        {
            ShouldTurnVelocity = shouldTurnVelocity;
            SignedTurnDegrees = signedTurnDegrees;
            ShouldUpdateFacing = shouldUpdateFacing;
        }
    }

    internal interface IRunSteeringEvaluator
    {
        RunSteeringDecision Evaluate(RunSteeringContext context);
    }

    internal sealed class DefaultRunSteeringEvaluator : IRunSteeringEvaluator
    {
        private readonly float _minimumAirSteerMagnitude = 0.0001f;

        public RunSteeringDecision Evaluate(RunSteeringContext context)
        {
            if (!context.HasUsableTangentDirection
                || !float.IsFinite(context.TangentSpeed)
                || !float.IsFinite(context.MinimumSteerSpeed)
                || context.TangentSpeed < context.MinimumSteerSpeed
                || !float.IsFinite(context.SmoothedSteer))
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

            return float.IsFinite(turnAngle)
                ? new RunSteeringDecision(turnAngle != 0f, turnAngle, true)
                : default;
        }
    }
}
