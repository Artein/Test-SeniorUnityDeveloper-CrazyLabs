using System;
using Game.Utils.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    internal sealed class LaunchFrameValidator
    {
        private readonly float _minimumAxisSqrMagnitude = 0.000001f;
        private readonly float _axisDotTolerance = 0.0001f;

        public bool IsValid(Vector3 right, Vector3 forward, Vector3 up)
        {
            return IsValidUnitAxis(right)
                   && IsValidUnitAxis(forward)
                   && IsValidUnitAxis(up)
                   && IsPerpendicular(right, forward, up);
        }

        public void ThrowIfInvalidAndNormalize(
            Vector3 right,
            Vector3 forward,
            Vector3 up,
            out Vector3 normalizedRight,
            out Vector3 normalizedForward,
            out Vector3 normalizedUp)
        {
            if (TryNormalizeAndValidate(right, forward, up, out normalizedRight, out normalizedForward, out normalizedUp))
                return;

            throw new ArgumentException("Slingshot Launch Frame axes must be finite, non-zero, unit, and perpendicular.");
        }

        private bool TryNormalizeAndValidate(
            Vector3 right,
            Vector3 forward,
            Vector3 up,
            out Vector3 normalizedRight,
            out Vector3 normalizedForward,
            out Vector3 normalizedUp)
        {
            normalizedRight = Vector3.zero;
            normalizedForward = Vector3.zero;
            normalizedUp = Vector3.zero;

            if (!CanNormalize(right) || !CanNormalize(forward) || !CanNormalize(up))
                return false;

            normalizedRight = right.normalized;
            normalizedForward = forward.normalized;
            normalizedUp = up.normalized;

            return IsValid(normalizedRight, normalizedForward, normalizedUp);
        }

        private bool IsValidUnitAxis(Vector3 axis)
        {
            return axis.IsFinite()
                   && math.isfinite(axis.sqrMagnitude)
                   && axis.sqrMagnitude > _minimumAxisSqrMagnitude
                   && axis.IsApproximatelyUnit();
        }

        private bool CanNormalize(Vector3 axis)
        {
            return axis.IsFinite()
                   && math.isfinite(axis.sqrMagnitude)
                   && axis.sqrMagnitude > _minimumAxisSqrMagnitude;
        }

        private bool IsPerpendicular(Vector3 right, Vector3 forward, Vector3 up)
        {
            return Mathf.Abs(Vector3.Dot(right, forward)) <= _axisDotTolerance
                   && Mathf.Abs(Vector3.Dot(right, up)) <= _axisDotTolerance
                   && Mathf.Abs(Vector3.Dot(forward, up)) <= _axisDotTolerance;
        }
    }
}
