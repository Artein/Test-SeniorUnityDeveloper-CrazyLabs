using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct RunLaunchLandingStabilizationContext
    {
        public Vector3 CurrentVelocity { get; }
        public RunSurfaceContext SurfaceContext { get; }
        public float FixedDeltaTime { get; }

        public RunLaunchLandingStabilizationContext(
            Vector3 currentVelocity,
            RunSurfaceContext surfaceContext,
            float fixedDeltaTime)
        {
            CurrentVelocity = currentVelocity;
            SurfaceContext = surfaceContext;
            FixedDeltaTime = fixedDeltaTime;
        }
    }

    internal interface IRunLaunchLandingStabilizer
    {
        void ArmForLaunch();
        void Reset();
        Vector3 Stabilize(RunLaunchLandingStabilizationContext context);
    }

    internal sealed class RunLaunchLandingStabilizer : IRunLaunchLandingStabilizer
    {
        private readonly IRunLaunchLandingStabilizationConfig _config;

        private bool _isArmed;
        private bool _isActive;
        private bool _hasObservedPostLaunchUngroundedSurface;
        private float _elapsedSeconds;

        public RunLaunchLandingStabilizer(IRunLaunchLandingStabilizationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void ArmForLaunch()
        {
            _isArmed = true;
            _isActive = false;
            _hasObservedPostLaunchUngroundedSurface = false;
            _elapsedSeconds = 0f;
        }

        public void Reset()
        {
            _isArmed = false;
            _isActive = false;
            _hasObservedPostLaunchUngroundedSurface = false;
            _elapsedSeconds = 0f;
        }

        public Vector3 Stabilize(RunLaunchLandingStabilizationContext context)
        {
            if (!_isArmed && !_isActive)
                return context.CurrentVelocity;

            var startedThisTick = false;

            if (_isArmed)
            {
                if (!HasValidGroundedSurface(context.SurfaceContext))
                {
                    _hasObservedPostLaunchUngroundedSurface = true;
                    return context.CurrentVelocity;
                }

                if (!_hasObservedPostLaunchUngroundedSurface)
                    return context.CurrentVelocity;

                _isArmed = false;
                _isActive = true;
                _elapsedSeconds = 0f;
                startedThisTick = true;
            }

            if (!_isActive)
                return context.CurrentVelocity;

            var duration = _config.LaunchLandingStabilizationSeconds;

            if (duration <= 0f)
            {
                Reset();
                return context.CurrentVelocity;
            }

            if (!startedThisTick)
            {
                _elapsedSeconds += Mathf.Max(0f, context.FixedDeltaTime);

                if (_elapsedSeconds > duration)
                {
                    Reset();
                    return context.CurrentVelocity;
                }
            }

            if (!HasValidGroundedSurface(context.SurfaceContext))
                return context.CurrentVelocity;

            var stabilizedVelocity = ClampSurfaceNormalLift(
                context.CurrentVelocity,
                context.SurfaceContext.GroundNormal);

            if (startedThisTick)
                _elapsedSeconds += Mathf.Max(0f, context.FixedDeltaTime);

            if (_elapsedSeconds >= duration)
                Reset();

            return stabilizedVelocity;
        }

        private bool HasValidGroundedSurface(RunSurfaceContext surfaceContext)
        {
            return surfaceContext is { IsGrounded: true, HasValidGroundNormal: true }
                   && IsValidDirection(surfaceContext.GroundNormal);
        }

        private Vector3 ClampSurfaceNormalLift(Vector3 velocity, Vector3 groundNormal)
        {
            var normal = groundNormal.normalized;
            var liftSpeed = Vector3.Dot(velocity, normal);
            var maximumLiftSpeed = _config.LaunchLandingMaximumLiftSpeed;

            if (liftSpeed <= maximumLiftSpeed)
                return velocity;

            return velocity - normal * (liftSpeed - maximumLiftSpeed);
        }

        private bool IsValidDirection(Vector3 direction)
        {
            var sqrMagnitude = direction.sqrMagnitude;
            return float.IsFinite(sqrMagnitude) && sqrMagnitude > 0.000001f;
        }
    }
}
