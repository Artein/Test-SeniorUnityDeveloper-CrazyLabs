using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class RunBodySpeedResolver
    {
        private readonly RunBodyLowSpeedAssistAttempt _lowSpeedAssistAttempt;

        public RunBodySpeedResolver(RunBodyLowSpeedAssistAttempt lowSpeedAssistAttempt)
        {
            _lowSpeedAssistAttempt = lowSpeedAssistAttempt
                                     ?? throw new ArgumentNullException(nameof(lowSpeedAssistAttempt));
        }

        public RunBodySpeedResolution Resolve(
            float currentTangentSpeed,
            bool hasValidGroundedRunSurface,
            bool hasUsableTangentDirection,
            RunBodySpeedDecision speedDecision,
            float fixedDeltaTime)
        {
            if (!float.IsFinite(currentTangentSpeed))
            {
                return new RunBodySpeedResolution(
                    0f,
                    speedDecision.Contributors,
                    0f,
                    0f,
                    0f,
                    _lowSpeedAssistAttempt.Snapshot);
            }

            var acceleratedSpeed = currentTangentSpeed;
            var accelerationDelta = Mathf.Max(0f, speedDecision.TangentAcceleration) * fixedDeltaTime;

            if (acceleratedSpeed < speedDecision.SoftMaximumSpeed)
            {
                acceleratedSpeed = Mathf.Min(
                    acceleratedSpeed + accelerationDelta,
                    speedDecision.SoftMaximumSpeed);
            }

            var integratedAccelerationDelta = acceleratedSpeed - currentTangentSpeed;
            var dragDelta = Mathf.Max(0f, speedDecision.TangentDrag) * fixedDeltaTime;
            var naturallyIntegratedSpeed = Mathf.Max(0f, acceleratedSpeed - dragDelta);
            var integratedDragDelta = acceleratedSpeed - naturallyIntegratedSpeed;

            var requestedAssistDelta = _lowSpeedAssistAttempt.Advance(
                new RunBodyLowSpeedAssistAttemptContext(
                    currentTangentSpeed,
                    naturallyIntegratedSpeed,
                    hasValidGroundedRunSurface,
                    hasUsableTangentDirection,
                    speedDecision.LowSpeedAssistTargetSpeed,
                    speedDecision.LowSpeedAssistAcceleration,
                    fixedDeltaTime));

            return new RunBodySpeedResolution(
                naturallyIntegratedSpeed + requestedAssistDelta,
                speedDecision.Contributors,
                integratedAccelerationDelta,
                integratedDragDelta,
                requestedAssistDelta,
                _lowSpeedAssistAttempt.Snapshot);
        }
    }
}
