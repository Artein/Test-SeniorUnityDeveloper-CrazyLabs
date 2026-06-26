using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotGeometrySnapshot
    {
        public Vector3 LeftAnchorPosition { get; }
        public Vector3 RightAnchorPosition { get; }
        public Vector3 RestPoint { get; }
        public Vector3 LaunchFrameRight { get; }
        public Vector3 LaunchFrameForward { get; }
        public Vector3 LaunchFrameUp { get; }

        public SlingshotGeometrySnapshot(
            Vector3 leftAnchorPosition,
            Vector3 rightAnchorPosition,
            Vector3 restPoint,
            Vector3 launchFrameRight,
            Vector3 launchFrameForward,
            Vector3 launchFrameUp)
        {
            var validator = new SlingshotGeometrySnapshotValidator();
            validator.ThrowIfInvalidAxis(launchFrameRight, nameof(launchFrameRight));
            validator.ThrowIfInvalidAxis(launchFrameForward, nameof(launchFrameForward));
            validator.ThrowIfInvalidAxis(launchFrameUp, nameof(launchFrameUp));

            LeftAnchorPosition = leftAnchorPosition;
            RightAnchorPosition = rightAnchorPosition;
            RestPoint = restPoint;
            LaunchFrameRight = launchFrameRight.normalized;
            LaunchFrameForward = launchFrameForward.normalized;
            LaunchFrameUp = launchFrameUp.normalized;
        }

        private sealed class SlingshotGeometrySnapshotValidator
        {
            public void ThrowIfInvalidAxis(Vector3 axis, string parameterName)
            {
                if (axis.sqrMagnitude <= 0.000001f || !math.isfinite(axis.sqrMagnitude))
                    throw new ArgumentException("Slingshot Launch Frame axis must be finite and non-zero.", parameterName);
            }
        }
    }
}
