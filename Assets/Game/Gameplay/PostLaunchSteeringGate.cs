using UnityEngine;

namespace Game.Gameplay
{
    internal interface IPostLaunchSteeringGate
    {
        void Arm();
        void Clear();
        bool ShouldBlockSteering(RunSurfaceContext surfaceContext, Vector3 velocity, float maximumSurfaceNormalLiftSpeed);
    }

    internal sealed class PostLaunchSteeringGate : IPostLaunchSteeringGate
    {
        private readonly float _minimumDirectionSqrMagnitude = 0.000001f;
        private readonly float _liftSpeedTolerance = 0.0001f;

        private bool _isArmed;
        private bool _hasObservedUnsupportedSurface;

        public void Arm()
        {
            _isArmed = true;
            _hasObservedUnsupportedSurface = false;
        }

        public void Clear()
        {
            _isArmed = false;
            _hasObservedUnsupportedSurface = false;
        }

        public bool ShouldBlockSteering(
            RunSurfaceContext surfaceContext,
            Vector3 velocity,
            float maximumSurfaceNormalLiftSpeed)
        {
            if (!_isArmed)
                return false;

            if (!HasValidGroundedSurface(surfaceContext))
            {
                _hasObservedUnsupportedSurface = true;
                return true;
            }

            if (_hasObservedUnsupportedSurface)
            {
                Clear();
                return false;
            }

            if (HasSurfaceNormalLift(velocity, surfaceContext.GroundNormal, maximumSurfaceNormalLiftSpeed))
                return true;

            Clear();
            return false;
        }

        private bool HasValidGroundedSurface(RunSurfaceContext surfaceContext)
        {
            return surfaceContext.IsGrounded
                   && surfaceContext.HasValidGroundNormal
                   && IsValidDirection(surfaceContext.GroundNormal);
        }

        private bool HasSurfaceNormalLift(
            Vector3 velocity,
            Vector3 groundNormal,
            float maximumSurfaceNormalLiftSpeed)
        {
            if (!IsValidDirection(groundNormal) || !IsFinite(velocity))
                return false;

            var maximumLiftSpeed = Mathf.Max(0f, maximumSurfaceNormalLiftSpeed);
            var liftSpeed = Vector3.Dot(velocity, groundNormal.normalized);

            return float.IsFinite(liftSpeed) && liftSpeed > maximumLiftSpeed + _liftSpeedTolerance;
        }

        private bool IsValidDirection(Vector3 direction)
        {
            var sqrMagnitude = direction.sqrMagnitude;

            return sqrMagnitude > _minimumDirectionSqrMagnitude
                   && !float.IsNaN(sqrMagnitude)
                   && !float.IsInfinity(sqrMagnitude);
        }

        private bool IsFinite(Vector3 vector)
        {
            return float.IsFinite(vector.x)
                   && float.IsFinite(vector.y)
                   && float.IsFinite(vector.z);
        }
    }
}
