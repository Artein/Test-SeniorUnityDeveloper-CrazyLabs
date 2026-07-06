using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Diagnostics
{
    internal sealed class RunDiagnosticsOverlayMath
    {
        public float CalculateStepSpeed(
            Vector3 currentPosition,
            ref Vector3 previousPosition,
            ref bool hasPreviousPosition,
            float deltaTime,
            out float stepDistance)
        {
            stepDistance = 0f;

            if (!currentPosition.IsFinite())
            {
                hasPreviousPosition = false;
                return 0f;
            }

            var stepSpeed = 0f;

            if (hasPreviousPosition && previousPosition.IsFinite())
            {
                stepDistance = Vector3.Distance(previousPosition, currentPosition);
                stepSpeed = stepDistance / deltaTime;
            }

            previousPosition = currentPosition;
            hasPreviousPosition = true;
            return float.IsFinite(stepSpeed) ? stepSpeed : 0f;
        }

        public float CalculateDirectionDelta(
            Vector3 currentDirection,
            ref Vector3 previousDirection,
            ref bool hasPreviousDirection)
        {
            if (!TryNormalize(currentDirection, out var normalizedDirection))
            {
                hasPreviousDirection = false;
                return 0f;
            }

            var deltaDegrees = 0f;

            if (hasPreviousDirection && TryNormalize(previousDirection, out var normalizedPreviousDirection))
                deltaDegrees = Vector3.Angle(normalizedPreviousDirection, normalizedDirection);

            previousDirection = normalizedDirection;
            hasPreviousDirection = true;
            return float.IsFinite(deltaDegrees) ? deltaDegrees : 0f;
        }

        public float CalculateRotationDelta(
            Quaternion currentRotation,
            ref Quaternion previousRotation,
            ref bool hasPreviousRotation)
        {
            if (!IsFinite(currentRotation))
            {
                hasPreviousRotation = false;
                return 0f;
            }

            var deltaDegrees = 0f;

            if (hasPreviousRotation && IsFinite(previousRotation))
                deltaDegrees = Quaternion.Angle(previousRotation, currentRotation);

            previousRotation = currentRotation;
            hasPreviousRotation = true;
            return float.IsFinite(deltaDegrees) ? deltaDegrees : 0f;
        }

        public float CalculateDistanceCentimeters(Vector3 firstPosition, Vector3 secondPosition)
        {
            if (!firstPosition.IsFinite() || !secondPosition.IsFinite())
                return 0f;

            var centimeters = Vector3.Distance(firstPosition, secondPosition) * 100f;
            return float.IsFinite(centimeters) ? centimeters : 0f;
        }

        private bool TryNormalize(Vector3 value, out Vector3 normalized)
        {
            normalized = Vector3.up;

            if (!value.IsFinite() || value.sqrMagnitude <= 0.000001f)
                return false;

            normalized = value.normalized;
            return normalized.IsFinite();
        }

        private bool IsFinite(Quaternion rotation)
        {
            return float.IsFinite(rotation.x)
                   && float.IsFinite(rotation.y)
                   && float.IsFinite(rotation.z)
                   && float.IsFinite(rotation.w);
        }
    }
}
