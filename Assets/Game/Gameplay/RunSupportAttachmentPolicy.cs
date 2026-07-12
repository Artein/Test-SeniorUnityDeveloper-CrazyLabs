using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    internal enum RunSupportAttachmentState
    {
        Unknown = 0,
        Attached = 1,
        Detached = 2
    }

    internal readonly struct RunSupportAttachmentResult
    {
        public RunSupportAttachmentState State { get; }
        public RunSupportAttachmentTransition Transition { get; }

        public RunSupportAttachmentResult(
            RunSupportAttachmentState state,
            RunSupportAttachmentTransition transition)
        {
            State = state;
            Transition = transition;
        }
    }

    internal sealed class RunSupportAttachmentPolicy
    {
        private readonly RunSupportAttachmentConfig _config;
        private Vector3 _candidateDetachmentNormal = Vector3.up;
        private Vector3 _candidateDetachmentPosition;
        private float _candidateElapsedSeconds;
        private CandidateKind _candidateKind;
        private Vector3 _detachmentNormal = Vector3.up;
        private Vector3 _detachmentPosition;
        private bool _hasExceededSameSurfaceReattachmentSeparation;
        private bool _hasLastSupportNormal;
        private Vector3 _lastSupportNormal = Vector3.up;
        private RunSupportAttachmentState _state;

        public RunSupportAttachmentPolicy(RunSupportAttachmentConfig config)
        {
            _config = config;
        }

        public RunSupportAttachmentResult Evaluate(
            RunSupportObservation observation,
            Vector3 position,
            Vector3 linearVelocity,
            float fixedDeltaTime)
        {
            if (observation.State == RunSupportObservationState.Unavailable)
            {
                Reset();
                return CreateResult(RunSupportAttachmentTransition.None);
            }

            if (!position.IsFinite() || !linearVelocity.IsFinite())
            {
                ClearCandidate();
                return CreateResult(RunSupportAttachmentTransition.None);
            }

            var safeFixedDeltaTime = float.IsFinite(fixedDeltaTime)
                ? Mathf.Max(a: 0f, fixedDeltaTime)
                : 0f;

            if (observation.State == RunSupportObservationState.Supported)
            {
                _lastSupportNormal = observation.SurfaceContext.GroundNormal;
                _hasLastSupportNormal = true;
            }

            if (_state == RunSupportAttachmentState.Unknown)
            {
                if (observation.State != RunSupportObservationState.Supported)
                    return CreateResult(RunSupportAttachmentTransition.None);

                _state = RunSupportAttachmentState.Attached;
            }

            return _state == RunSupportAttachmentState.Detached
                ? EvaluateDetached(observation, position, linearVelocity, safeFixedDeltaTime)
                : EvaluateAttached(observation, position, linearVelocity, safeFixedDeltaTime);
        }

        public void Reset()
        {
            _state = RunSupportAttachmentState.Unknown;
            _lastSupportNormal = Vector3.up;
            _hasLastSupportNormal = false;
            _detachmentNormal = Vector3.up;
            _detachmentPosition = Vector3.zero;
            _hasExceededSameSurfaceReattachmentSeparation = false;
            ClearCandidate();
        }

        private RunSupportAttachmentResult EvaluateAttached(
            RunSupportObservation observation,
            Vector3 position,
            Vector3 linearVelocity,
            float fixedDeltaTime)
        {
            if (!TryGetSeparationNormal(observation, out var separationNormal)
                || Vector3.Dot(linearVelocity, separationNormal)
                <= _config.MaximumAttachedSurfaceNormalLiftSpeed)
            {
                ClearCandidate();
                return CreateResult(RunSupportAttachmentTransition.None);
            }

            if (_candidateKind != CandidateKind.Detachment)
            {
                _candidateKind = CandidateKind.Detachment;
                _candidateDetachmentNormal = separationNormal;
                _candidateDetachmentPosition = position;
                _candidateElapsedSeconds = fixedDeltaTime;
            }
            else
            {
                _candidateElapsedSeconds += fixedDeltaTime;
            }

            if (!HasCandidateReachedConfirmation())
                return CreateResult(RunSupportAttachmentTransition.None);

            _state = RunSupportAttachmentState.Detached;
            _detachmentNormal = _candidateDetachmentNormal;
            _detachmentPosition = _candidateDetachmentPosition;
            _hasExceededSameSurfaceReattachmentSeparation = false;
            ClearCandidate();
            UpdateSeparationEpisode(position);
            return CreateResult(RunSupportAttachmentTransition.Detached);
        }

        private RunSupportAttachmentResult EvaluateDetached(
            RunSupportObservation observation,
            Vector3 position,
            Vector3 linearVelocity,
            float fixedDeltaTime)
        {
            UpdateSeparationEpisode(position);

            if (observation.State != RunSupportObservationState.Supported
                || Vector3.Dot(linearVelocity, observation.SurfaceContext.GroundNormal)
                > _config.MaximumAttachedSurfaceNormalLiftSpeed
                || !CanReattach(observation, position))
            {
                ClearCandidate();
                return CreateResult(RunSupportAttachmentTransition.None);
            }

            if (_candidateKind != CandidateKind.Reattachment)
            {
                _candidateKind = CandidateKind.Reattachment;
                _candidateElapsedSeconds = fixedDeltaTime;
            }
            else
            {
                _candidateElapsedSeconds += fixedDeltaTime;
            }

            if (!HasCandidateReachedConfirmation())
                return CreateResult(RunSupportAttachmentTransition.None);

            _state = RunSupportAttachmentState.Attached;
            ClearDetachmentEpisode();
            ClearCandidate();
            return CreateResult(RunSupportAttachmentTransition.Reattached);
        }

        private bool CanReattach(
            RunSupportObservation observation,
            Vector3 position)
        {
            var landingNormal = observation.SurfaceContext.GroundNormal;
            var normalDeltaDegrees = Vector3.Angle(_detachmentNormal, landingNormal);

            if (normalDeltaDegrees >= _config.MinimumReattachmentNormalChangeDegrees)
                return true;

            if (!_hasExceededSameSurfaceReattachmentSeparation)
                return false;

            var separation = Vector3.Dot(position - _detachmentPosition, _detachmentNormal);
            return separation <= _config.SameSurfaceReattachmentSeparationMeters;
        }

        private void UpdateSeparationEpisode(Vector3 position)
        {
            var separation = Vector3.Dot(position - _detachmentPosition, _detachmentNormal);

            if (separation > _config.SameSurfaceReattachmentSeparationMeters)
                _hasExceededSameSurfaceReattachmentSeparation = true;
        }

        private bool TryGetSeparationNormal(
            RunSupportObservation observation,
            out Vector3 separationNormal)
        {
            if (observation.State == RunSupportObservationState.Supported)
            {
                separationNormal = observation.SurfaceContext.GroundNormal;
                return true;
            }

            separationNormal = _lastSupportNormal;
            return observation.State == RunSupportObservationState.Missing && _hasLastSupportNormal;
        }

        private bool HasCandidateReachedConfirmation()
        {
            return _config.TransitionConfirmationSeconds <= 0f
                   || _candidateElapsedSeconds >= _config.TransitionConfirmationSeconds
                   || Mathf.Approximately(
                       _candidateElapsedSeconds,
                       _config.TransitionConfirmationSeconds);
        }

        private void ClearDetachmentEpisode()
        {
            _detachmentNormal = Vector3.up;
            _detachmentPosition = Vector3.zero;
            _hasExceededSameSurfaceReattachmentSeparation = false;
        }

        private void ClearCandidate()
        {
            _candidateKind = CandidateKind.None;
            _candidateDetachmentNormal = Vector3.up;
            _candidateDetachmentPosition = Vector3.zero;
            _candidateElapsedSeconds = 0f;
        }

        private RunSupportAttachmentResult CreateResult(RunSupportAttachmentTransition transition)
        {
            return new RunSupportAttachmentResult(_state, transition);
        }

        private enum CandidateKind
        {
            None = 0,
            Detachment = 1,
            Reattachment = 2
        }
    }
}
