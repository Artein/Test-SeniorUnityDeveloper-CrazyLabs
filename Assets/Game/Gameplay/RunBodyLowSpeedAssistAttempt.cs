using UnityEngine;

namespace Game.Gameplay
{
    internal enum RunBodyLowSpeedAssistAttemptState
    {
        Unavailable = 0,
        Eligible = 1,
        Active = 2,
        Paused = 3,
        Exhausted = 4,
        Rearmed = 5,
    }

    internal readonly struct RunBodyLowSpeedAssistAttemptContext
    {
        public float SampledTangentSpeed { get; }
        public float NaturallyIntegratedTangentSpeed { get; }
        public bool HasValidGroundedRunSurface { get; }
        public bool HasUsableTangentDirection { get; }
        public float EffectiveTargetSpeed { get; }
        public float EffectiveAcceleration { get; }
        public float FixedDeltaTime { get; }

        public RunBodyLowSpeedAssistAttemptContext(
            float sampledTangentSpeed,
            float naturallyIntegratedTangentSpeed,
            bool hasValidGroundedRunSurface,
            bool hasUsableTangentDirection,
            float effectiveTargetSpeed,
            float effectiveAcceleration,
            float fixedDeltaTime)
        {
            SampledTangentSpeed = sampledTangentSpeed;
            NaturallyIntegratedTangentSpeed = naturallyIntegratedTangentSpeed;
            HasValidGroundedRunSurface = hasValidGroundedRunSurface;
            HasUsableTangentDirection = hasUsableTangentDirection;
            EffectiveTargetSpeed = effectiveTargetSpeed;
            EffectiveAcceleration = effectiveAcceleration;
            FixedDeltaTime = fixedDeltaTime;
        }
    }

    internal readonly struct RunBodyLowSpeedAssistAttemptSnapshot
    {
        public RunBodyLowSpeedAssistAttemptState State { get; }
        public bool IsEligible { get; }
        public float EffectiveTargetSpeed { get; }
        public float RemainingRequestedVelocityBudget { get; }

        public RunBodyLowSpeedAssistAttemptSnapshot(
            RunBodyLowSpeedAssistAttemptState state,
            bool isEligible,
            float effectiveTargetSpeed,
            float remainingRequestedVelocityBudget)
        {
            State = state;
            IsEligible = isEligible;
            EffectiveTargetSpeed = effectiveTargetSpeed;
            RemainingRequestedVelocityBudget = remainingRequestedVelocityBudget;
        }
    }

    internal sealed class RunBodyLowSpeedAssistAttempt
    {
        private readonly float _numericEpsilon = 0.000001f;
        private readonly float _rearmSpeedTolerance = 0.001f;

        private bool _isArmed;
        private bool _hasAttempt;
        private float _attemptTargetSpeed;
        private float _remainingRequestedVelocityBudget;

        public RunBodyLowSpeedAssistAttemptSnapshot Snapshot { get; private set; }

        public void RearmForNewRun()
        {
            Rearm();
        }

        public void Clear()
        {
            _isArmed = false;
            _hasAttempt = false;
            _attemptTargetSpeed = 0f;
            _remainingRequestedVelocityBudget = 0f;

            Snapshot = new RunBodyLowSpeedAssistAttemptSnapshot(
                RunBodyLowSpeedAssistAttemptState.Unavailable,
                false,
                0f,
                0f);
        }

        public float Advance(RunBodyLowSpeedAssistAttemptContext context)
        {
            if (_hasAttempt
                && context.HasValidGroundedRunSurface
                && float.IsFinite(context.SampledTangentSpeed))
            {
                if (context.SampledTangentSpeed > _attemptTargetSpeed + _rearmSpeedTolerance)
                {
                    Rearm();
                    return 0f;
                }

                if (context.SampledTangentSpeed >= _attemptTargetSpeed)
                {
                    ExhaustAttempt(false);
                    return 0f;
                }
            }

            var isEligible = IsEligible(context);

            if (!isEligible)
            {
                SetIneligibleSnapshot();
                return 0f;
            }

            if (!_hasAttempt)
            {
                if (!_isArmed)
                {
                    SetSnapshot(
                        RunBodyLowSpeedAssistAttemptState.Unavailable,
                        true,
                        context.EffectiveTargetSpeed,
                        0f);
                    return 0f;
                }

                if (context.EffectiveAcceleration <= _numericEpsilon
                    || context.FixedDeltaTime <= _numericEpsilon)
                {
                    SetSnapshot(
                        RunBodyLowSpeedAssistAttemptState.Eligible,
                        true,
                        context.EffectiveTargetSpeed,
                        0f);
                    return 0f;
                }

                _isArmed = false;
                _hasAttempt = true;
                _attemptTargetSpeed = context.EffectiveTargetSpeed;

                _remainingRequestedVelocityBudget = Mathf.Max(
                    0f,
                    _attemptTargetSpeed - context.SampledTangentSpeed);
            }

            if (context.EffectiveAcceleration <= _numericEpsilon
                || context.FixedDeltaTime <= _numericEpsilon)
            {
                SetSnapshot(
                    RunBodyLowSpeedAssistAttemptState.Paused,
                    true,
                    _attemptTargetSpeed,
                    _remainingRequestedVelocityBudget);
                return 0f;
            }

            var targetDeficitAfterNaturalIntegration = Mathf.Max(
                0f,
                _attemptTargetSpeed - context.NaturallyIntegratedTangentSpeed);

            if (targetDeficitAfterNaturalIntegration <= _numericEpsilon
                || _remainingRequestedVelocityBudget <= _numericEpsilon)
            {
                ExhaustAttempt(true);
                return 0f;
            }

            var requestedVelocityDelta = Mathf.Min(
                Mathf.Min(
                    context.EffectiveAcceleration * context.FixedDeltaTime,
                    targetDeficitAfterNaturalIntegration),
                _remainingRequestedVelocityBudget);

            if (!float.IsFinite(requestedVelocityDelta) || requestedVelocityDelta <= _numericEpsilon)
            {
                SetSnapshot(
                    RunBodyLowSpeedAssistAttemptState.Paused,
                    true,
                    _attemptTargetSpeed,
                    _remainingRequestedVelocityBudget);
                return 0f;
            }

            _remainingRequestedVelocityBudget = Mathf.Max(
                0f,
                _remainingRequestedVelocityBudget - requestedVelocityDelta);

            var reachesTarget = requestedVelocityDelta
                                >= targetDeficitAfterNaturalIntegration - _numericEpsilon;
            var spendsBudget = _remainingRequestedVelocityBudget <= _numericEpsilon;

            if (reachesTarget || spendsBudget)
            {
                ExhaustAttempt(true);
            }
            else
            {
                SetSnapshot(
                    RunBodyLowSpeedAssistAttemptState.Active,
                    true,
                    _attemptTargetSpeed,
                    _remainingRequestedVelocityBudget);
            }

            return requestedVelocityDelta;
        }

        private bool IsEligible(RunBodyLowSpeedAssistAttemptContext context)
        {
            return context.HasValidGroundedRunSurface
                   && context.HasUsableTangentDirection
                   && float.IsFinite(context.SampledTangentSpeed)
                   && float.IsFinite(context.NaturallyIntegratedTangentSpeed)
                   && float.IsFinite(context.EffectiveTargetSpeed)
                   && float.IsFinite(context.EffectiveAcceleration)
                   && float.IsFinite(context.FixedDeltaTime)
                   && context.SampledTangentSpeed >= 0f
                   && context.NaturallyIntegratedTangentSpeed >= 0f
                   && context.EffectiveTargetSpeed > _numericEpsilon
                   && context.EffectiveAcceleration >= 0f
                   && context.FixedDeltaTime >= 0f
                   && context.SampledTangentSpeed < context.EffectiveTargetSpeed;
        }

        private void SetIneligibleSnapshot()
        {
            if (_hasAttempt)
            {
                var state = Snapshot.State == RunBodyLowSpeedAssistAttemptState.Exhausted
                    ? RunBodyLowSpeedAssistAttemptState.Exhausted
                    : RunBodyLowSpeedAssistAttemptState.Paused;

                SetSnapshot(
                    state,
                    false,
                    _attemptTargetSpeed,
                    _remainingRequestedVelocityBudget);
                return;
            }

            SetSnapshot(
                RunBodyLowSpeedAssistAttemptState.Unavailable,
                false,
                0f,
                0f);
        }

        private void ExhaustAttempt(bool isEligible)
        {
            _remainingRequestedVelocityBudget = 0f;

            SetSnapshot(
                RunBodyLowSpeedAssistAttemptState.Exhausted,
                isEligible,
                _attemptTargetSpeed,
                0f);
        }

        private void Rearm()
        {
            _isArmed = true;
            _hasAttempt = false;
            _attemptTargetSpeed = 0f;
            _remainingRequestedVelocityBudget = 0f;

            Snapshot = new RunBodyLowSpeedAssistAttemptSnapshot(
                RunBodyLowSpeedAssistAttemptState.Rearmed,
                false,
                0f,
                0f);
        }

        private void SetSnapshot(
            RunBodyLowSpeedAssistAttemptState state,
            bool isEligible,
            float effectiveTargetSpeed,
            float remainingRequestedVelocityBudget)
        {
            Snapshot = new RunBodyLowSpeedAssistAttemptSnapshot(
                state,
                isEligible,
                effectiveTargetSpeed,
                remainingRequestedVelocityBudget);
        }
    }
}
