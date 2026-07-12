using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal sealed class RunSurfaceStabilityPolicy
    {
        private readonly RunSurfaceStabilityConfig _config;
        private readonly IRunSurfaceSlopeCalculator _slopeCalculator;

        private RunSurfaceContext _stableSupport = new(false, Vector3.up, 0f);
        private Vector3 _candidateNormalSum;
        private int _candidateNormalCount;
        private float _missingElapsedSeconds;
        private float _candidateElapsedSeconds;

        public RunSurfaceStabilityPolicy(
            RunSurfaceStabilityConfig config,
            IRunSurfaceSlopeCalculator slopeCalculator)
        {
            _config = config;
            _slopeCalculator = slopeCalculator ?? throw new ArgumentNullException(nameof(slopeCalculator));
        }

        public RunSurfaceStabilityResult Evaluate(
            RunSupportObservation observation,
            float fixedDeltaTime)
        {
            var safeFixedDeltaTime = float.IsFinite(fixedDeltaTime) ? Mathf.Max(0f, fixedDeltaTime) : 0f;

            switch (observation.State)
            {
                case RunSupportObservationState.Unavailable:
                    return HardReset();
                case RunSupportObservationState.Missing:
                    return EvaluateMissing(safeFixedDeltaTime);
                case RunSupportObservationState.Supported:
                    return EvaluateSupported(observation, safeFixedDeltaTime);
                default:
                    throw new ArgumentOutOfRangeException(nameof(observation));
            }
        }

        private RunSurfaceStabilityResult EvaluateMissing(float fixedDeltaTime)
        {
            ClearCandidate();

            if (!_stableSupport.IsGrounded)
                return CreateResult(RunSurfaceTransition.None, false, false);

            _missingElapsedSeconds += fixedDeltaTime;

            if (_config.SupportLossConfirmationSeconds > 0f
                && _missingElapsedSeconds < _config.SupportLossConfirmationSeconds
                && !Mathf.Approximately(_missingElapsedSeconds, _config.SupportLossConfirmationSeconds))
            {
                return CreateResult(RunSurfaceTransition.None, true, false);
            }

            _stableSupport = new RunSurfaceContext(false, Vector3.up, 0f);
            _missingElapsedSeconds = 0f;
            return CreateResult(RunSurfaceTransition.SupportLost, false, false);
        }

        private RunSurfaceStabilityResult EvaluateSupported(
            RunSupportObservation observation,
            float fixedDeltaTime)
        {
            _missingElapsedSeconds = 0f;

            if (!_stableSupport.IsGrounded || !_stableSupport.HasValidGroundNormal)
            {
                AcceptObservedSupport(observation);
                return CreateResult(RunSurfaceTransition.SupportAcquired, false, false);
            }

            var sampledNormal = observation.SurfaceContext.GroundNormal;
            var normalDeltaDegrees = Vector3.Angle(_stableSupport.GroundNormal, sampledNormal);

            if (normalDeltaDegrees <= _config.DiscontinuousNormalThresholdDegrees)
            {
                var transition = normalDeltaDegrees > 0.0001f
                    ? RunSurfaceTransition.ContinuousUpdate
                    : RunSurfaceTransition.None;

                AcceptObservedSupport(observation);
                return CreateResult(transition, false, false);
            }

            return EvaluateDiscontinuousCandidate(observation, fixedDeltaTime);
        }

        private RunSurfaceStabilityResult EvaluateDiscontinuousCandidate(
            RunSupportObservation observation,
            float fixedDeltaTime)
        {
            var sampledNormal = observation.SurfaceContext.GroundNormal;

            if (_candidateNormalCount <= 0
                || Vector3.Angle(GetCandidateRepresentative(), sampledNormal) > _config.CandidateCoherenceDegrees)
            {
                _candidateNormalSum = sampledNormal;
                _candidateNormalCount = 1;
                _candidateElapsedSeconds = fixedDeltaTime;
            }
            else
            {
                _candidateNormalSum += sampledNormal;
                _candidateNormalCount += 1;
                _candidateElapsedSeconds += fixedDeltaTime;
            }

            if (_config.DiscontinuousNormalConfirmationSeconds > 0f
                && _candidateElapsedSeconds < _config.DiscontinuousNormalConfirmationSeconds
                && !Mathf.Approximately(
                    _candidateElapsedSeconds,
                    _config.DiscontinuousNormalConfirmationSeconds))
            {
                return CreateResult(RunSurfaceTransition.None, false, true);
            }

            var representativeNormal = GetCandidateRepresentative();

            var downhillDegrees = _slopeCalculator.CalculateForwardDownhillDegrees(
                representativeNormal,
                observation.ProgressFrame);
            _stableSupport = new RunSurfaceContext(true, representativeNormal, downhillDegrees);
            ClearCandidate();
            return CreateResult(RunSurfaceTransition.ConfirmedDiscontinuity, false, false);
        }

        private RunSurfaceStabilityResult HardReset()
        {
            _stableSupport = new RunSurfaceContext(false, Vector3.up, 0f);
            _missingElapsedSeconds = 0f;
            ClearCandidate();
            return CreateResult(RunSurfaceTransition.HardReset, false, false);
        }

        private void AcceptObservedSupport(RunSupportObservation observation)
        {
            _stableSupport = observation.SurfaceContext;
            ClearCandidate();
        }

        private Vector3 GetCandidateRepresentative()
        {
            return _candidateNormalCount > 0 && _candidateNormalSum.sqrMagnitude > 0.000001f
                ? _candidateNormalSum.normalized
                : Vector3.up;
        }

        private void ClearCandidate()
        {
            _candidateNormalSum = Vector3.zero;
            _candidateNormalCount = 0;
            _candidateElapsedSeconds = 0f;
        }

        private RunSurfaceStabilityResult CreateResult(
            RunSurfaceTransition transition,
            bool isMissingSupportHeld,
            bool isConfirmingDiscontinuity)
        {
            return new RunSurfaceStabilityResult(
                _stableSupport,
                transition,
                isMissingSupportHeld,
                isConfirmingDiscontinuity);
        }
    }
}
