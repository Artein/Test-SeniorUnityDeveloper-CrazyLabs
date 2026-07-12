using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class RunSteeringFramePolicy
    {
        private readonly RunSteeringFrameConfig _config;

        private RunSteeringFrameSnapshot _current = new(false, Vector3.up);
        private bool _isActive;
        private bool _hasGroundedSupport;
        private float _airborneElapsedSeconds;

        public RunSteeringFrameSnapshot Current => _current;

        public RunSteeringFramePolicy(RunSteeringFrameConfig config)
        {
            _config = config;
        }

        public Vector3 GetUpDirection(Vector3 fallbackUpDirection)
        {
            if (_current.IsValid)
                return _current.UpDirection;

            return TryNormalize(fallbackUpDirection, out var fallbackUp)
                ? fallbackUp
                : Vector3.up;
        }

        public void Reset(Vector3 launchUpDirection)
        {
            _current = new RunSteeringFrameSnapshot(
                true,
                TryNormalize(launchUpDirection, out var launchUp) ? launchUp : Vector3.up);
            _isActive = true;
            _hasGroundedSupport = false;
            _airborneElapsedSeconds = 0f;
        }

        public void Clear()
        {
            _current = new RunSteeringFrameSnapshot(false, Vector3.up);
            _isActive = false;
            _hasGroundedSupport = false;
            _airborneElapsedSeconds = 0f;
        }

        public RunSteeringFrameSnapshot Evaluate(
            RunSurfaceStabilityResult stability,
            float fixedDeltaTime)
        {
            if (!_isActive)
                return _current;

            var safeFixedDeltaTime = float.IsFinite(fixedDeltaTime) ? Mathf.Max(0f, fixedDeltaTime) : 0f;

            if (stability.Transition == RunSurfaceTransition.HardReset)
            {
                ResetRuntimeState();
                return _current;
            }

            if (stability.StableSupport is { IsGrounded: true, HasValidGroundNormal: true })
            {
                EvaluateSupported(stability, safeFixedDeltaTime);
                return _current;
            }

            EvaluateUnsupported(safeFixedDeltaTime);
            return _current;
        }

        private void EvaluateSupported(
            RunSurfaceStabilityResult stability,
            float fixedDeltaTime)
        {
            _airborneElapsedSeconds = 0f;
            var stableUp = stability.StableSupport.GroundNormal;

            if (!_hasGroundedSupport
                || !_current.IsValid
                || stability.Transition == RunSurfaceTransition.SupportAcquired
                || stability.Transition == RunSurfaceTransition.SupportReattached
                || stability.Transition == RunSurfaceTransition.ConfirmedDiscontinuity)
            {
                _current = new RunSteeringFrameSnapshot(true, stableUp);
                _hasGroundedSupport = true;
                return;
            }

            _hasGroundedSupport = true;

            if (stability.Transition != RunSurfaceTransition.ContinuousUpdate)
                return;

            var maximumRadiansDelta = _config.NormalSlewDegreesPerSecond * Mathf.Deg2Rad * fixedDeltaTime;
            var slewedUp = Vector3.RotateTowards(_current.UpDirection, stableUp, maximumRadiansDelta, 0f);
            _current = new RunSteeringFrameSnapshot(true, slewedUp);
        }

        private void EvaluateUnsupported(float fixedDeltaTime)
        {
            _hasGroundedSupport = false;

            if (!_current.IsValid)
                return;

            _airborneElapsedSeconds += fixedDeltaTime;

            if (_config.AirborneUpRetentionSeconds > 0f
                && _airborneElapsedSeconds < _config.AirborneUpRetentionSeconds)
            {
                return;
            }

            _current = new RunSteeringFrameSnapshot(false, Vector3.up);
            _airborneElapsedSeconds = 0f;
        }

        private void ResetRuntimeState()
        {
            _current = new RunSteeringFrameSnapshot(false, Vector3.up);
            _hasGroundedSupport = false;
            _airborneElapsedSeconds = 0f;
        }

        private bool TryNormalize(Vector3 direction, out Vector3 normalized)
        {
            normalized = Vector3.up;

            if (!direction.IsFinite() || direction.sqrMagnitude <= 0.000001f)
                return false;

            normalized = direction.normalized;
            return normalized.IsFinite();
        }
    }
}
