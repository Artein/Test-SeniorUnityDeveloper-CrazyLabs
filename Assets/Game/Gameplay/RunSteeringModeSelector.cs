using UnityEngine;

namespace Game.Gameplay
{
    internal enum RunSteeringMode
    {
        Grounded,
        Air
    }

    internal interface IRunSteeringModeSelector
    {
        RunSteeringMode Select(RunSurfaceContext surfaceContext, Vector3 velocity, float maximumSurfaceNormalLiftSpeed);
    }

    internal sealed class RunSteeringModeSelector : IRunSteeringModeSelector
    {
        private readonly float _minimumDirectionSqrMagnitude = 0.000001f;

        public RunSteeringMode Select(
            RunSurfaceContext surfaceContext,
            Vector3 velocity,
            float maximumSurfaceNormalLiftSpeed)
        {
            if (!HasValidGroundedSurface(surfaceContext) ||
                HasSurfaceNormalLift(velocity, surfaceContext.GroundNormal, maximumSurfaceNormalLiftSpeed))
            {
                return RunSteeringMode.Air;
            }

            return RunSteeringMode.Grounded;
        }

        private bool HasValidGroundedSurface(RunSurfaceContext surfaceContext)
        {
            return surfaceContext is { IsGrounded: true, HasValidGroundNormal: true }
                   && IsValidDirection(surfaceContext.GroundNormal);
        }

        private bool HasSurfaceNormalLift(
            Vector3 velocity,
            Vector3 groundNormal,
            float maximumSurfaceNormalLiftSpeed)
        {
            if (!IsFinite(velocity) || !IsValidDirection(groundNormal))
                return false;

            var maximumLiftSpeed = Mathf.Max(0f, maximumSurfaceNormalLiftSpeed);
            var liftSpeed = Vector3.Dot(velocity, groundNormal.normalized);

            return float.IsFinite(liftSpeed) && liftSpeed > maximumLiftSpeed;
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
