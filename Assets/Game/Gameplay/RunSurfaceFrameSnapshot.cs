using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public enum RunSupportObservationState
    {
        Unavailable = 0,
        Missing = 1,
        Supported = 2
    }

    public enum RunSurfaceTransition
    {
        None = 0,
        ContinuousUpdate = 1,
        SupportAcquired = 2,
        SupportLost = 3,
        ConfirmedDiscontinuity = 4,
        HardReset = 5,
        SupportReattached = 6
    }

    public enum RunSupportAttachmentTransition
    {
        None = 0,
        Detached = 1,
        Reattached = 2
    }

    public readonly struct RunSupportObservation
    {
        public RunSupportObservationState State { get; }
        public RunProgressFrameSnapshot ProgressFrame { get; }
        public RunSurfaceContext SurfaceContext { get; }
        public float SupportDistance { get; }

        public RunSupportObservation(
            RunSupportObservationState state,
            RunProgressFrameSnapshot progressFrame,
            RunSurfaceContext surfaceContext,
            float supportDistance)
        {
            if (state is < RunSupportObservationState.Unavailable or > RunSupportObservationState.Supported)
                throw new ArgumentOutOfRangeException(nameof(state));

            if (state != RunSupportObservationState.Unavailable && !progressFrame.IsValid)
                throw new ArgumentException("Missing and Supported observations require a valid Run Progress Frame.", nameof(progressFrame));

            if (state == RunSupportObservationState.Supported
                && (!surfaceContext.IsGrounded || !surfaceContext.HasValidGroundNormal))
            {
                throw new ArgumentException("Supported observations require a valid grounded surface context.", nameof(surfaceContext));
            }

            if (!float.IsFinite(supportDistance) || supportDistance < 0f)
                throw new ArgumentOutOfRangeException(nameof(supportDistance));

            State = state;
            ProgressFrame = state == RunSupportObservationState.Unavailable ? default : progressFrame;

            SurfaceContext = state == RunSupportObservationState.Supported
                ? surfaceContext
                : new RunSurfaceContext(false, Vector3.up, 0f);
            SupportDistance = state == RunSupportObservationState.Supported ? supportDistance : 0f;
        }
    }

    public readonly struct RunSteeringFrameSnapshot
    {
        public bool IsValid { get; }
        public Vector3 UpDirection { get; }

        public RunSteeringFrameSnapshot(bool isValid, Vector3 upDirection)
        {
            IsValid = isValid && upDirection.IsFinite() && upDirection.sqrMagnitude > 0.000001f;
            UpDirection = IsValid ? upDirection.normalized : Vector3.up;
        }
    }

    public readonly struct RunSurfaceFrameSnapshot
    {
        public RunSupportObservation ObservedSupport { get; }
        public RunSurfaceContext StableSupport { get; }
        public RunSurfaceTransition Transition { get; }
        public bool IsMissingSupportHeld { get; }
        public bool IsConfirmingDiscontinuity { get; }
        public RunSteeringFrameSnapshot SteeringFrame { get; }
        public RunSupportAttachmentTransition AttachmentTransition { get; }

        public RunSurfaceFrameSnapshot(
            RunSupportObservation observedSupport,
            RunSurfaceContext stableSupport,
            RunSurfaceTransition transition,
            bool isMissingSupportHeld,
            bool isConfirmingDiscontinuity,
            RunSteeringFrameSnapshot steeringFrame,
            RunSupportAttachmentTransition attachmentTransition = RunSupportAttachmentTransition.None)
        {
            ObservedSupport = observedSupport;
            StableSupport = stableSupport;
            Transition = transition;
            IsMissingSupportHeld = isMissingSupportHeld;
            IsConfirmingDiscontinuity = isConfirmingDiscontinuity;
            SteeringFrame = steeringFrame;
            AttachmentTransition = attachmentTransition;
        }
    }

    internal readonly struct RunSurfaceStabilityResult
    {
        public RunSurfaceContext StableSupport { get; }
        public RunSurfaceTransition Transition { get; }
        public bool IsMissingSupportHeld { get; }
        public bool IsConfirmingDiscontinuity { get; }

        public RunSurfaceStabilityResult(
            RunSurfaceContext stableSupport,
            RunSurfaceTransition transition,
            bool isMissingSupportHeld,
            bool isConfirmingDiscontinuity)
        {
            StableSupport = stableSupport;
            Transition = transition;
            IsMissingSupportHeld = isMissingSupportHeld;
            IsConfirmingDiscontinuity = isConfirmingDiscontinuity;
        }
    }
}
