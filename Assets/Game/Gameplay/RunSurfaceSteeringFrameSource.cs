using System;
using Game.Foundation.Time;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    public interface IRunSteeringFrameSource
    {
        Vector3 GetUpDirection(Vector3 fallbackUpDirection);
    }

    internal interface IRunSteeringFrameResetter
    {
        void Reset(Vector3 launchUpDirection);
        void Clear();
    }

    internal sealed class RunSurfaceSteeringFrameSource : IRunSteeringFrameSource, IRunSteeringFrameResetter, IFixedTickable
    {
        private const float MinimumUpSqrMagnitude = 0.000001f;
        private const float SameDirectionDot = 0.9999f;

        private readonly IRunSurfaceContextSource _surfaceContextSource;
        private readonly IRunSteeringFrameConfig _config;
        private readonly ITime _clock;

        private Vector3 _stableUpDirection = Vector3.up;
        private Vector3 _suspectUpDirection = Vector3.up;
        private bool _isActive;
        private bool _hasStableUpDirection;
        private bool _hasGroundingContinuity;
        private bool _hasSuspectUpDirection;
        private float _ungroundedSeconds;
        private float _suspectSeconds;

        public RunSurfaceSteeringFrameSource(
            IRunSurfaceContextSource surfaceContextSource,
            IRunSteeringFrameConfig config,
            ITime clock)
        {
            _surfaceContextSource = surfaceContextSource ?? throw new ArgumentNullException(nameof(surfaceContextSource));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            var fallbackUp = GetValidUpDirection(fallbackUpDirection, Vector3.up);

            if (!_isActive || !_hasStableUpDirection)
                return fallbackUp;

            return GetValidUpDirection(_stableUpDirection, fallbackUp);
        }

        void IRunSteeringFrameResetter.Reset(Vector3 launchUpDirection)
        {
            _stableUpDirection = GetValidUpDirection(launchUpDirection, Vector3.up);
            _suspectUpDirection = Vector3.up;
            _isActive = true;
            _hasStableUpDirection = true;
            _hasGroundingContinuity = false;
            _hasSuspectUpDirection = false;
            _ungroundedSeconds = 0f;
            _suspectSeconds = 0f;
        }

        void IRunSteeringFrameResetter.Clear()
        {
            _stableUpDirection = Vector3.up;
            _suspectUpDirection = Vector3.up;
            _isActive = false;
            _hasStableUpDirection = false;
            _hasGroundingContinuity = false;
            _hasSuspectUpDirection = false;
            _ungroundedSeconds = 0f;
            _suspectSeconds = 0f;
        }

        void IFixedTickable.FixedTick()
        {
            if (!_isActive)
                return;

            var fixedDeltaTime = Mathf.Max(0f, _clock.FixedDeltaTime);
            var surfaceContext = _surfaceContextSource.Current;

            if (!surfaceContext.IsGrounded
                || !surfaceContext.HasValidGroundNormal
                || !TryGetValidUpDirection(surfaceContext.GroundNormal, out var sampledUpDirection))
            {
                ProcessMissingSupport(fixedDeltaTime);
                return;
            }

            ProcessGroundedSupport(sampledUpDirection, fixedDeltaTime);
        }

        private Vector3 GetValidUpDirection(Vector3 upDirection, Vector3 fallbackUpDirection)
        {
            return TryGetValidUpDirection(upDirection, out var normalizedUpDirection)
                ? normalizedUpDirection
                : fallbackUpDirection;
        }

        private bool TryGetValidUpDirection(Vector3 upDirection, out Vector3 normalizedUpDirection)
        {
            normalizedUpDirection = Vector3.up;
            var sqrMagnitude = upDirection.sqrMagnitude;

            if (sqrMagnitude <= MinimumUpSqrMagnitude || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return false;

            var normalized = upDirection.normalized;
            var normalizedSqrMagnitude = normalized.sqrMagnitude;

            if (normalizedSqrMagnitude <= MinimumUpSqrMagnitude
                || float.IsNaN(normalizedSqrMagnitude)
                || float.IsInfinity(normalizedSqrMagnitude))
            {
                return false;
            }

            normalizedUpDirection = normalized;
            return true;
        }

        private void ProcessMissingSupport(float fixedDeltaTime)
        {
            ClearSuspect();

            if (!_hasStableUpDirection)
            {
                _hasGroundingContinuity = false;
                _ungroundedSeconds = 0f;
                return;
            }

            _ungroundedSeconds += fixedDeltaTime;

            if (ResolveUngroundedGraceSeconds() > 0f && _ungroundedSeconds <= ResolveUngroundedGraceSeconds())
                return;

            _hasStableUpDirection = false;
            _hasGroundingContinuity = false;
            _ungroundedSeconds = 0f;
        }

        private void ProcessGroundedSupport(Vector3 sampledUpDirection, float fixedDeltaTime)
        {
            _ungroundedSeconds = 0f;

            if (!_hasStableUpDirection || !_hasGroundingContinuity)
            {
                AcceptStableUpDirection(sampledUpDirection);
                return;
            }

            var angleToSample = Vector3.Angle(_stableUpDirection, sampledUpDirection);

            if (angleToSample <= 0.0001f)
            {
                AcceptStableUpDirection(sampledUpDirection);
                return;
            }

            if (angleToSample > ResolveSnapDegrees())
            {
                ProcessSuspectUpDirection(sampledUpDirection, fixedDeltaTime);
                return;
            }

            ClearSuspect();
            var maxRadiansDelta = ResolveNormalSlewDegreesPerSecond() * Mathf.Deg2Rad * fixedDeltaTime;
            AcceptStableUpDirection(Vector3.RotateTowards(_stableUpDirection, sampledUpDirection, maxRadiansDelta, 0f));
        }

        private void ProcessSuspectUpDirection(Vector3 sampledUpDirection, float fixedDeltaTime)
        {
            _hasGroundingContinuity = true;

            if (!_hasSuspectUpDirection || !AreSameDirection(_suspectUpDirection, sampledUpDirection))
            {
                _suspectUpDirection = sampledUpDirection;
                _suspectSeconds = fixedDeltaTime;
                _hasSuspectUpDirection = true;
            }
            else
            {
                _suspectSeconds += fixedDeltaTime;
            }

            if (ResolveSuspectNormalConfirmationSeconds() > 0f &&
                _suspectSeconds < ResolveSuspectNormalConfirmationSeconds())
            {
                return;
            }

            AcceptStableUpDirection(sampledUpDirection);
        }

        private void AcceptStableUpDirection(Vector3 upDirection)
        {
            _stableUpDirection = GetValidUpDirection(upDirection, Vector3.up);
            _hasStableUpDirection = true;
            _hasGroundingContinuity = true;
            _ungroundedSeconds = 0f;
            ClearSuspect();
        }

        private void ClearSuspect()
        {
            _hasSuspectUpDirection = false;
            _suspectSeconds = 0f;
        }

        private bool AreSameDirection(Vector3 firstDirection, Vector3 secondDirection)
        {
            return Vector3.Dot(firstDirection, secondDirection) >= SameDirectionDot;
        }

        private float ResolveNormalSlewDegreesPerSecond()
        {
            return _config.RunSteeringFrameNormalSlewDegreesPerSecond;
        }

        private float ResolveSnapDegrees()
        {
            return _config.RunSteeringFrameSnapDegrees;
        }

        private float ResolveUngroundedGraceSeconds()
        {
            return _config.RunSteeringFrameUngroundedGraceSeconds;
        }

        private float ResolveSuspectNormalConfirmationSeconds()
        {
            return _config.RunSteeringFrameSuspectNormalConfirmationSeconds;
        }
    }
}
