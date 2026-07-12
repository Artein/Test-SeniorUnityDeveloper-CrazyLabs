using System;
using Game.Foundation.Time;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    public interface IRunSurfaceFrameSource
    {
        RunSurfaceFrameSnapshot Current { get; }
    }

    public interface IRunSteeringFrameSource
    {
        Vector3 GetUpDirection(Vector3 fallbackUpDirection);
    }

    internal interface IRunSteeringFrameResetter
    {
        void Reset(Vector3 launchUpDirection);
        void Clear();
    }

    internal interface IRunSupportProbe
    {
        Vector3 SampleOrigin { get; }

        RunSupportObservation Observe(
            RunProgressFrameSnapshot progressFrame,
            bool hasContinuityNormal,
            Vector3 continuityNormal);
    }

    internal sealed class RunSurfaceFramePipeline :
        IRunSurfaceFrameSource,
        IRunSteeringFrameSource,
        IRunSteeringFrameResetter,
        IFixedTickable
    {
        private readonly RunSupportAttachmentPolicy _attachmentPolicy;
        private readonly IRunMotionSource _motionSource;
        private readonly IRunProgressFrameSource _progressFrameSource;
        private readonly RunSurfaceStabilityPolicy _stabilityPolicy;
        private readonly RunSteeringFramePolicy _steeringPolicy;
        private readonly IRunSupportProbe _supportProbe;
        private readonly ITime _time;

        public RunSurfaceFrameSnapshot Current { get; private set; }

        public RunSurfaceFramePipeline(
            IRunProgressFrameSource progressFrameSource,
            IRunSupportProbe supportProbe,
            IRunMotionSource motionSource,
            RunSupportAttachmentPolicy attachmentPolicy,
            RunSurfaceStabilityPolicy stabilityPolicy,
            RunSteeringFramePolicy steeringPolicy,
            ITime time)
        {
            _progressFrameSource = progressFrameSource ?? throw new ArgumentNullException(nameof(progressFrameSource));
            _supportProbe = supportProbe ?? throw new ArgumentNullException(nameof(supportProbe));
            _motionSource = motionSource ?? throw new ArgumentNullException(nameof(motionSource));
            _attachmentPolicy = attachmentPolicy ?? throw new ArgumentNullException(nameof(attachmentPolicy));
            _stabilityPolicy = stabilityPolicy ?? throw new ArgumentNullException(nameof(stabilityPolicy));
            _steeringPolicy = steeringPolicy ?? throw new ArgumentNullException(nameof(steeringPolicy));
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        void IFixedTickable.FixedTick()
        {
            var previousStableSupport = Current.StableSupport;

            var hasContinuityNormal = previousStableSupport is { IsGrounded: true, HasValidGroundNormal: true };

            var continuityNormal = hasContinuityNormal
                ? previousStableSupport.GroundNormal
                : Vector3.up;

            RunSupportObservation observation;

            if (_progressFrameSource.TryCreateSnapshot(_supportProbe.SampleOrigin, out var progressFrame, out _))
            {
                observation = _supportProbe.Observe(
                    progressFrame,
                    hasContinuityNormal,
                    continuityNormal);
            }
            else
            {
                observation = new RunSupportObservation(
                    RunSupportObservationState.Unavailable,
                    progressFrame: default,
                    surfaceContext: default,
                    supportDistance: 0f);
            }

            var fixedDeltaTime = _time.FixedDeltaTime;

            var attachment = _attachmentPolicy.Evaluate(
                observation,
                _motionSource.Position,
                _motionSource.LinearVelocity,
                fixedDeltaTime);

            var stability = _stabilityPolicy.Evaluate(observation, attachment.Transition, fixedDeltaTime);
            var steeringFrame = _steeringPolicy.Evaluate(stability, fixedDeltaTime);

            var next = new RunSurfaceFrameSnapshot(
                observation,
                stability.StableSupport,
                stability.Transition,
                stability.IsMissingSupportHeld,
                stability.IsConfirmingDiscontinuity,
                steeringFrame,
                attachment.Transition);

            Current = next;
        }

        void IRunSteeringFrameResetter.Reset(Vector3 launchUpDirection)
        {
            _attachmentPolicy.Reset();
            _steeringPolicy.Reset(launchUpDirection);
        }

        void IRunSteeringFrameResetter.Clear()
        {
            _attachmentPolicy.Reset();
            _steeringPolicy.Clear();
        }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            return _steeringPolicy.GetUpDirection(fallbackUpDirection);
        }
    }
}
