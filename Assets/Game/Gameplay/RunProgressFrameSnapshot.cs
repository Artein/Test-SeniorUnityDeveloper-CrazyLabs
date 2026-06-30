using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public readonly struct RunProgressFrameSnapshot
    {
        private const float MinimumAxisSqrMagnitude = 0.000001f;

        public Vector3 Origin { get; }
        public Vector3 ForwardDirection { get; }
        public Vector3 RightDirection { get; }
        public Vector3 UpDirection { get; }
        public bool IsValid { get; }

        private RunProgressFrameSnapshot(
            Vector3 origin,
            Vector3 forwardDirection,
            Vector3 rightDirection,
            Vector3 upDirection)
        {
            Origin = origin;
            ForwardDirection = forwardDirection;
            RightDirection = rightDirection;
            UpDirection = upDirection;
            IsValid = true;
        }

        public static bool TryCreate(
            Vector3 origin,
            Vector3 forwardDirection,
            Vector3 upDirection,
            out RunProgressFrameSnapshot snapshot,
            out string error)
        {
            snapshot = default;
            error = string.Empty;

            if (!origin.IsFinite())
            {
                error = "Run Progress Frame origin must be finite.";
                return false;
            }

            if (!TryNormalize(upDirection, out var normalizedUp))
            {
                error = "Run Progress Frame up direction must be finite and non-zero.";
                return false;
            }

            if (!TryNormalize(forwardDirection, out var normalizedForward))
            {
                error = "Run Progress Frame forward direction must be finite and non-zero.";
                return false;
            }

            var planarForward = Vector3.ProjectOnPlane(normalizedForward, normalizedUp);

            if (!TryNormalize(planarForward, out var normalizedPlanarForward))
            {
                error = "Run Progress Frame forward direction must not be parallel to up direction.";
                return false;
            }

            var rightDirection = Vector3.Cross(normalizedUp, normalizedPlanarForward);

            if (!TryNormalize(rightDirection, out var normalizedRight))
            {
                error = "Run Progress Frame right direction could not be derived.";
                return false;
            }

            snapshot = new RunProgressFrameSnapshot(origin, normalizedPlanarForward, normalizedRight, normalizedUp);
            return true;
        }

        public float GetForwardProgress(Vector3 position)
        {
            ThrowIfInvalid();
            return !position.IsFinite() ? 0f : Vector3.Dot(position - Origin, ForwardDirection);
        }

        public float GetCoursePlanarSpeed(Vector3 linearVelocity)
        {
            ThrowIfInvalid();
            return !linearVelocity.IsFinite() ? 0f : Vector3.ProjectOnPlane(linearVelocity, UpDirection).magnitude;
        }

        public float GetCourseForwardSpeed(Vector3 linearVelocity)
        {
            ThrowIfInvalid();
            return !linearVelocity.IsFinite() ? 0f : Vector3.Dot(linearVelocity, ForwardDirection);
        }

        private static bool TryNormalize(Vector3 value, out Vector3 normalized)
        {
            normalized = default;

            if (!value.IsFinite() || value.sqrMagnitude <= MinimumAxisSqrMagnitude)
                return false;

            normalized = value.normalized;
            return normalized.IsFinite();
        }

        private void ThrowIfInvalid()
        {
            if (!IsValid)
                throw new InvalidOperationException("Run Progress Frame snapshot is invalid.");
        }
    }
}
