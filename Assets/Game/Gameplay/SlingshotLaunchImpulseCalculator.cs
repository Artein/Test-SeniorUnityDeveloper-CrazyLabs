using System;
using System.Linq;
using Game.Gameplay.Slingshot;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public sealed class SlingshotLaunchImpulseCalculator
    {
        private readonly GameplaySlingshotLaunchConfigValidator _configValidator = new();

        public LaunchImpulse Calculate(
            SlingshotLaunchRequest request,
            IGameplaySlingshotLaunchConfig config,
            float slingshotLaunchPower)
        {
            ThrowIfInvalidRequest(request);
            ThrowIfInvalidConfig(config);
            ThrowIfInvalidLaunchPower(slingshotLaunchPower);

            var launchPower = Mathf.Max(0f, slingshotLaunchPower);
            var pullStrength = Mathf.Clamp01(request.PullStrength);
            var pullCurveValue = Mathf.Clamp01(config.PullStrengthCurve.Evaluate(pullStrength));
            var forwardImpulse = Mathf.Lerp(config.MinimumForwardImpulse, config.MaximumForwardImpulse, pullCurveValue) * launchPower;
            var upwardImpulse = config.UpwardImpulse * launchPower;
            var launchDirection = GetLaunchDirection(request, config);
            var velocityChange = (launchDirection * forwardImpulse) + (request.LaunchFrameUp * upwardImpulse);
            velocityChange = ApplyTotalImpulseClamps(velocityChange, request, config);

            return new LaunchImpulse(velocityChange, launchDirection, request.LaunchFrameUp, forwardImpulse, upwardImpulse);
        }

        private Vector3 GetLaunchDirection(SlingshotLaunchRequest request, IGameplaySlingshotLaunchConfig config)
        {
            var normalizedLateralPull = Mathf.Clamp(request.NormalizedLateralPull, -1f, 1f);
            var lateralCurveInput = Mathf.Abs(normalizedLateralPull);
            var lateralCurveValue = Mathf.Clamp01(config.LateralAngleCurve.Evaluate(lateralCurveInput));

            var angleDegrees = -Mathf.Sign(normalizedLateralPull)
                               * config.MaximumLateralLaunchAngleDegrees
                               * lateralCurveValue;

            return (Quaternion.AngleAxis(angleDegrees, request.LaunchFrameUp) * request.LaunchFrameForward).normalized;
        }

        private Vector3 ApplyTotalImpulseClamps(
            Vector3 velocityChange,
            SlingshotLaunchRequest request,
            IGameplaySlingshotLaunchConfig config)
        {
            var magnitude = velocityChange.magnitude;

            if (config.HasMinimumTotalImpulse && magnitude > 0.000001f && magnitude < config.MinimumTotalImpulse)
                return velocityChange.normalized * config.MinimumTotalImpulse;

            if (config.HasMaximumTotalImpulse && magnitude > config.MaximumTotalImpulse)
                return velocityChange.normalized * config.MaximumTotalImpulse;

            if (config.HasMinimumTotalImpulse && magnitude <= 0.000001f && config.MinimumTotalImpulse > 0f)
                return request.LaunchFrameForward * config.MinimumTotalImpulse;

            return velocityChange;
        }

        private void ThrowIfInvalidRequest(SlingshotLaunchRequest request)
        {
            if (IsInvalidFiniteNonNegative(request.PullStrength))
                throw new ArgumentException("Slingshot launch request pull strength must be finite and non-negative.", nameof(request));

            if (IsInvalidFiniteNonNegative(request.PullDistance))
                throw new ArgumentException("Slingshot launch request pull distance must be finite and non-negative.", nameof(request));

            if (IsInvalidFinite(request.PullOffset))
                throw new ArgumentException("Slingshot launch request pull offset must be finite.", nameof(request));

            if (IsInvalidFinite(request.NormalizedLateralPull))
                throw new ArgumentException("Slingshot launch request normalized lateral pull must be finite.", nameof(request));

            if (!request.FinalPullPoint.IsFinite())
                throw new ArgumentException("Slingshot launch request final pull point must be finite.", nameof(request));

            if (!request.LaunchFrameForward.IsFinite() || !request.LaunchFrameForward.IsApproximatelyUnit())
                throw new ArgumentException("Slingshot launch request forward axis must be a finite unit vector.", nameof(request));

            if (!request.LaunchFrameUp.IsFinite() || !request.LaunchFrameUp.IsApproximatelyUnit())
                throw new ArgumentException("Slingshot launch request up axis must be a finite unit vector.", nameof(request));

            if (Mathf.Abs(Vector3.Dot(request.LaunchFrameForward, request.LaunchFrameUp)) > 0.01f)
                throw new ArgumentException("Slingshot launch request forward and up axes must be perpendicular.", nameof(request));
        }

        private void ThrowIfInvalidConfig(IGameplaySlingshotLaunchConfig config)
        {
            var errors = _configValidator.Validate(config).ToList();

            if (errors.Count > 0)
                throw new ArgumentException("Invalid Gameplay Slingshot Launch config: " + string.Join(" ", errors), nameof(config));
        }

        private void ThrowIfInvalidLaunchPower(float slingshotLaunchPower)
        {
            if (IsInvalidFiniteNonNegative(slingshotLaunchPower))
                throw new ArgumentException("Slingshot launch power must be finite and non-negative.", nameof(slingshotLaunchPower));
        }

        private bool IsInvalidFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
        }

        private bool IsInvalidFiniteNonNegative(float value)
        {
            return IsInvalidFinite(value) || value < 0f;
        }
    }
}
