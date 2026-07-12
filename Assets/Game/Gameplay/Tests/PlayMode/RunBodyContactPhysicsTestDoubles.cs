using System;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Upgrades;
using UnityEngine;

namespace Game.Gameplay.Tests.PlayMode
{
    internal sealed class RunBodyContactPhysicsConfig :
        IRunBodySpeedConfig,
        IRunBodyMovementValidityConfig,
        IRunLaunchLandingStabilizationConfig,
        IRunSteeringConfig
    {
        public float DownhillAcceleration { get; set; }
        public float SurfaceSlowdown { get; set; }
        public float LowSpeedAssistTargetSpeed { get; set; }
        public float LowSpeedAssistAcceleration { get; set; }
        public float BaseSoftMaximumSpeed { get; set; } = 20f;
        public float AboveMaximumSpeedResistance { get; set; }
        public float MaximumSupportedSurfaceNormalLiftSpeed { get; set; } = 0.05f;
        public float RunBodySpeedSanityGuardMetersPerSecond { get; set; } = 250f;
        public float LaunchLandingStabilizationSeconds { get; set; } = 0.4f;
        public float LaunchLandingMaximumLiftSpeed { get; set; }
        public float RunSteeringRangeCentimeters { get; set; } = 2.54f;
        public float RunSteeringDeadzoneFraction { get; set; }
        public float RunSteeringResponsiveness { get; set; } = 100f;
        public float FallbackDpi { get; set; } = 100f;
        public float MinimumAcceptedDpi { get; set; } = 1f;
        public float MaximumAcceptedDpi { get; set; } = 1000f;
        public float MaximumTurnDegreesPerSecond { get; set; } = 90f;
        public float RunAirSteeringMaximumTurnDegreesPerSecond { get; set; } = 30f;
        public float MinimumSteerSpeed { get; set; } = 0.25f;
        public float RunSteeringFrameNormalSlewDegreesPerSecond { get; set; } = 360f;
        public float RunSurfaceDiscontinuousNormalThresholdDegrees { get; set; } = 60f;
        public float RunSurfaceSupportLossConfirmationSeconds { get; set; } = 0.08f;
        public float RunSurfaceDiscontinuousNormalConfirmationSeconds { get; set; } = 0.04f;
        public float RunSurfaceCandidateCoherenceDegrees { get; set; } = 1f;
        public float RunSteeringFrameAirborneUpRetentionSeconds { get; set; } = 0.12f;
        public float SupportProbeDistance { get; set; } = 0.3f;
        public float BodyBounciness { get; set; }
    }

    internal sealed class RunBodyContactPhysicsStateService : IGameplayStateService
    {
        public GameplayStateId CurrentStateId { get; private set; }

        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        public RunBodyContactPhysicsStateService(GameplayStateId runningStateId)
        {
            CurrentStateId = runningStateId;
        }

        public bool IsCurrent(GameplayStateId stateId)
        {
            return ReferenceEquals(CurrentStateId, stateId);
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            if (ReferenceEquals(CurrentStateId, nextStateId))
                return false;

            var previousStateId = CurrentStateId;
            GameplayStateChanging?.Invoke(nextStateId, previousStateId);
            CurrentStateId = nextStateId;
            GameplayStateChanged?.Invoke(nextStateId, previousStateId);
            return true;
        }
    }

    internal sealed class NeutralRunSteeringInputSource : IRunSteeringInputSource
    {
        public RunSteeringInputState AdvanceAndRead(float fixedDeltaTime)
        {
            return default;
        }
    }

    internal sealed class FixedRunGameplayStatResolver : IRunGameplayStatResolver
    {
        private readonly float _resolvedSoftMaximumSpeed;

        public FixedRunGameplayStatResolver(float resolvedSoftMaximumSpeed)
        {
            _resolvedSoftMaximumSpeed = resolvedSoftMaximumSpeed;
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            return _resolvedSoftMaximumSpeed;
        }
    }

    internal sealed class FixedRunProgressContext : IRunProgressFrameSource, IRunProgressService
    {
        private readonly Vector3 _forwardDirection;
        private readonly Vector3 _upDirection;

        public bool HasValidSnapshot { get; private set; }
        public string SnapshotError { get; private set; } = string.Empty;
        public RunProgressFrameSnapshot Snapshot { get; private set; }
        public float CurrentForwardProgress { get; private set; }
        public float MaximumForwardProgress { get; private set; }
        public RunProgressSample CurrentSample => default;

        public FixedRunProgressContext(Vector3 origin, Vector3 forwardDirection, Vector3 upDirection)
        {
            _forwardDirection = forwardDirection;
            _upDirection = upDirection;

            if (!TryBeginRun(origin, out var error))
                throw new ArgumentException(error, nameof(forwardDirection));
        }

        public bool TryCreateSnapshot(
            Vector3 origin,
            out RunProgressFrameSnapshot snapshot,
            out string error)
        {
            return RunProgressFrameSnapshot.TryCreate(
                origin,
                _forwardDirection,
                _upDirection,
                out snapshot,
                out error);
        }

        public bool TryBeginRun(Vector3 origin, out string error)
        {
            HasValidSnapshot = RunProgressFrameSnapshot.TryCreate(
                origin,
                _forwardDirection,
                _upDirection,
                out var snapshot,
                out error);
            Snapshot = snapshot;
            SnapshotError = error;
            CurrentForwardProgress = 0f;
            MaximumForwardProgress = 0f;
            return HasValidSnapshot;
        }

        public void SamplePosition(Vector3 position)
        {
            if (!HasValidSnapshot)
                return;

            CurrentForwardProgress = Snapshot.GetForwardProgress(position);
            MaximumForwardProgress = Mathf.Max(MaximumForwardProgress, CurrentForwardProgress);
        }

        public void Reset()
        {
            HasValidSnapshot = false;
            SnapshotError = string.Empty;
            Snapshot = default;
            CurrentForwardProgress = 0f;
            MaximumForwardProgress = 0f;
        }
    }

    internal sealed class RecordingRunBodyMovementTarget : IRunBodyMovementTarget
    {
        private readonly IRunBodyMovementTarget _inner;

        public Vector3 LinearVelocity => _inner.LinearVelocity;
        public int StepWriteCount { get; private set; }
        public int TotalWriteCount { get; private set; }
        public bool HasLastTargetState { get; private set; }
        public RunBodyMovementTargetState LastTargetState { get; private set; }

        public RecordingRunBodyMovementTarget(IRunBodyMovementTarget inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public void BeginStep()
        {
            StepWriteCount = 0;
            HasLastTargetState = false;
            LastTargetState = default;
        }

        public void ApplyTargetState(RunBodyMovementTargetState targetState)
        {
            StepWriteCount += 1;
            TotalWriteCount += 1;
            HasLastTargetState = true;
            LastTargetState = targetState;
            _inner.ApplyTargetState(targetState);
        }
    }

    internal sealed class TestRunMotionSource : IRunMotionSource
    {
        private readonly Transform _transform;
        private readonly Rigidbody _rigidbody;

        public Vector3 Position => _transform.position;
        public Vector3 LinearVelocity => _rigidbody != null ? _rigidbody.linearVelocity : Vector3.zero;

        public TestRunMotionSource(Transform transform, Rigidbody rigidbody = null)
        {
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
            _rigidbody = rigidbody;
        }
    }
}
