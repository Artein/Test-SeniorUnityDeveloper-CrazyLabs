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
        private readonly IRunProgressFrameSource _progressFrameSource;
        private readonly IRunSupportProbe _supportProbe;
        private readonly RunSurfaceStabilityPolicy _stabilityPolicy;
        private readonly RunSteeringFramePolicy _steeringPolicy;
        private readonly ITime _time;

        public RunSurfaceFrameSnapshot Current { get; private set; }

        public RunSurfaceFramePipeline(
            IRunProgressFrameSource progressFrameSource,
            IRunSupportProbe supportProbe,
            RunSurfaceStabilityPolicy stabilityPolicy,
            RunSteeringFramePolicy steeringPolicy,
            ITime time)
        {
            _progressFrameSource = progressFrameSource ?? throw new ArgumentNullException(nameof(progressFrameSource));
            _supportProbe = supportProbe ?? throw new ArgumentNullException(nameof(supportProbe));
            _stabilityPolicy = stabilityPolicy ?? throw new ArgumentNullException(nameof(stabilityPolicy));
            _steeringPolicy = steeringPolicy ?? throw new ArgumentNullException(nameof(steeringPolicy));
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            return _steeringPolicy.GetUpDirection(fallbackUpDirection);
        }

        void IRunSteeringFrameResetter.Reset(Vector3 launchUpDirection)
        {
            _steeringPolicy.Reset(launchUpDirection);
        }

        void IRunSteeringFrameResetter.Clear()
        {
            _steeringPolicy.Clear();
        }

        void IFixedTickable.FixedTick()
        {
            var previousStableSupport = Current.StableSupport;

            var hasContinuityNormal = previousStableSupport.IsGrounded
                                      && previousStableSupport.HasValidGroundNormal;

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
                    default,
                    default,
                    0f);
            }

            var fixedDeltaTime = _time.FixedDeltaTime;
            var stability = _stabilityPolicy.Evaluate(observation, fixedDeltaTime);
            var steeringFrame = _steeringPolicy.Evaluate(stability, fixedDeltaTime);

            var next = new RunSurfaceFrameSnapshot(
                observation,
                stability.StableSupport,
                stability.Transition,
                stability.IsMissingSupportHeld,
                stability.IsConfirmingDiscontinuity,
                steeringFrame);

            Current = next;
        }
    }
}
