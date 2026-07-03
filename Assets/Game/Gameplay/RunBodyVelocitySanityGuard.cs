using UnityEngine;

namespace Game.Gameplay
{
    internal interface IRunBodyVelocitySanityGuard
    {
        RunBodyVelocitySanityResult Sanitize(Vector3 velocity, float maximumSpeed);
    }

    internal readonly struct RunBodyVelocitySanityResult
    {
        public RunBodyVelocitySanityResult(Vector3 velocity, bool wasCorrected)
        {
            Velocity = velocity;
            WasCorrected = wasCorrected;
        }

        public Vector3 Velocity { get; }
        public bool WasCorrected { get; }
    }

    internal sealed class RunBodyVelocitySanityGuard : IRunBodyVelocitySanityGuard
    {
        private readonly float _defaultMaximumSpeed = 250f;
        private readonly float _minimumSpeed = 0.000001f;

        public RunBodyVelocitySanityResult Sanitize(Vector3 velocity, float maximumSpeed)
        {
            if (!IsFinite(velocity))
                return new RunBodyVelocitySanityResult(Vector3.zero, wasCorrected: true);

            var resolvedMaximumSpeed = ResolveMaximumSpeed(maximumSpeed);
            var speed = velocity.magnitude;

            if (float.IsFinite(speed) && speed <= resolvedMaximumSpeed)
                return new RunBodyVelocitySanityResult(velocity, wasCorrected: false);

            var direction = GetSafeDirection(velocity);

            if (direction == Vector3.zero)
                return new RunBodyVelocitySanityResult(Vector3.zero, wasCorrected: true);

            return new RunBodyVelocitySanityResult(direction * resolvedMaximumSpeed, wasCorrected: true);
        }

        private float ResolveMaximumSpeed(float maximumSpeed)
        {
            if (!float.IsFinite(maximumSpeed) || maximumSpeed <= 0f)
                return _defaultMaximumSpeed;

            return maximumSpeed;
        }

        private Vector3 GetSafeDirection(Vector3 velocity)
        {
            var maximumComponent = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.y), Mathf.Abs(velocity.z));

            if (!float.IsFinite(maximumComponent) || maximumComponent <= _minimumSpeed)
                return Vector3.zero;

            var scaledVelocity = velocity / maximumComponent;
            var scaledMagnitude = scaledVelocity.magnitude;

            if (!float.IsFinite(scaledMagnitude) || scaledMagnitude <= _minimumSpeed)
                return Vector3.zero;

            return scaledVelocity / scaledMagnitude;
        }

        private bool IsFinite(Vector3 vector)
        {
            return float.IsFinite(vector.x)
                   && float.IsFinite(vector.y)
                   && float.IsFinite(vector.z);
        }
    }
}
