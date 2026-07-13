using System;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Utils.Mathematics;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    internal interface IRunCameraLateStep
    {
        void UpdateCamera();
    }

    internal sealed class RunCameraController : IRunCameraLateStep, IInitializable, IDisposable
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRunCameraSource _source;
        private readonly IRunCameraAnchor _anchor;
        private readonly IRunCameraRig _rig;
        private readonly IRunCameraConfig _config;
        private readonly ITime _clock;
        private readonly GameplayStateId _runPreparationStateId;
        private readonly GameplayStateId _preLaunchStateId;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStateId _runEndedStateId;

        private Vector3 _cameraUp = Vector3.up;
        private Quaternion _lastValidYaw = Quaternion.identity;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _hasLaunchApplied;
        private bool _isRunCameraActive;
        private bool _hasAnchorPose;

        public RunCameraController(
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IRunCameraSource source,
            IRunCameraAnchor anchor,
            IRunCameraRig rig,
            IRunCameraConfig config,
            ITime clock,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId runPreparationStateId,
            [Key(InjectKey.GameplayStateId.PreLaunch)]
            GameplayStateId preLaunchStateId,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStateId.RunEnded)]
            GameplayStateId runEndedStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            _rig = rig ?? throw new ArgumentNullException(nameof(rig));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));

            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));
            _runEndedStateId = runEndedStateId != null ? runEndedStateId : throw new ArgumentNullException(nameof(runEndedStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunCameraController));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnSlingshotLaunchApplied;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;
            ApplyCameraForState(_gameplayStateService.CurrentStateId);
            UpdateAnchorPose(0f, true);
        }

        void IRunCameraLateStep.UpdateCamera()
        {
            if (_isDisposed || !_isRunCameraActive)
                return;

            UpdateAnchorPose(Mathf.Max(0f, _clock.DeltaTime), false);
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_isInitialized)
            {
                _launchAppliedNotifier.LaunchApplied -= OnSlingshotLaunchApplied;
                _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
            }

            _hasLaunchApplied = false;
            _isRunCameraActive = false;
        }

        private void OnSlingshotLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            _hasLaunchApplied = true;
            _cameraUp = GetValidUpDirection(launchApplied.LaunchUpDirection);
            PrimeYawFromLaunch(launchApplied);

            if (_gameplayStateService.IsCurrent(_runningStateId))
                ActivateRunCamera();
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            ApplyCameraForState(nextStateId);
        }

        private void ApplyCameraForState(GameplayStateId nextStateId)
        {
            if (nextStateId == _runPreparationStateId)
            {
                ResetRunCameraForNeutralState();
                _rig.ActivateRunPreparationCamera();
                return;
            }

            if (nextStateId == _preLaunchStateId)
            {
                ResetRunCameraForNeutralState();
                _rig.ActivatePreLaunchCamera();
                return;
            }

            if (nextStateId == _runningStateId)
            {
                if (_hasLaunchApplied)
                    ActivateRunCamera();

                return;
            }

            if (nextStateId == _runEndedStateId)
            {
                if (_hasLaunchApplied)
                    ActivateRunCamera();

                return;
            }

            ResetRunCameraForNeutralState();
            _rig.ActivateRunPreparationCamera();
        }

        private void ActivateRunCamera()
        {
            if (_isRunCameraActive)
                return;

            UpdateAnchorPose(0f, true, false);
            _rig.ActivateRunCamera();
            _isRunCameraActive = true;
        }

        private void ResetRunCameraForNeutralState()
        {
            _hasLaunchApplied = false;
            _isRunCameraActive = false;
            _hasAnchorPose = false;
            _cameraUp = Vector3.up;
            _lastValidYaw = Quaternion.identity;
            UpdateAnchorPose(0f, true, false);
        }

        private void UpdateAnchorPose(float deltaTime, bool snap)
        {
            UpdateAnchorPose(deltaTime, snap, true);
        }

        private void UpdateAnchorPose(float deltaTime, bool snap, bool acceptVelocityYaw)
        {
            var sourcePosition = GetSourcePosition();
            var sourceVelocity = _source.LinearVelocity;
            var targetPosition = sourcePosition + _config.AnchorOffset;
            var yawDecision = GetTargetYawDecision(sourceVelocity, acceptVelocityYaw);
            var targetYaw = yawDecision.TargetYaw;
            var nextPosition = GetNextPosition(targetPosition, deltaTime, snap);
            var nextYaw = GetNextYaw(targetYaw, deltaTime, snap);

            _anchor.SetPose(nextPosition, nextYaw);
            _hasAnchorPose = true;
        }

        private Vector3 GetSourcePosition()
        {
            var sourcePosition = _source.Position;

            if (!sourcePosition.IsFinite())
                sourcePosition = _anchor.Position.IsFinite() ? _anchor.Position : Vector3.zero;

            return sourcePosition;
        }

        private RunCameraYawDecision GetTargetYawDecision(Vector3 sourceVelocity, bool acceptVelocityYaw)
        {
            var planarVelocity = Vector3.ProjectOnPlane(sourceVelocity, _cameraUp);
            var planarSpeed = planarVelocity.IsFinite() ? planarVelocity.magnitude : 0f;
            var acceptedVelocityYaw = acceptVelocityYaw && planarVelocity.IsFinite() && planarSpeed >= _config.MinimumYawSpeed;

            if (acceptedVelocityYaw)
                _lastValidYaw = Quaternion.LookRotation(planarVelocity.normalized, _cameraUp);

            return new RunCameraYawDecision(
                planarVelocity,
                planarSpeed,
                acceptedVelocityYaw,
                _lastValidYaw);
        }

        private Vector3 GetNextPosition(Vector3 targetPosition, float deltaTime, bool snap)
        {
            if (snap || !_hasAnchorPose || _config.PositionResponseRate <= 0f)
                return targetPosition;

            var t = Mathf.Clamp01(_config.PositionResponseRate * deltaTime);
            return Vector3.Lerp(_anchor.Position, targetPosition, t);
        }

        private Quaternion GetNextYaw(Quaternion targetYaw, float deltaTime, bool snap)
        {
            if (snap || !_hasAnchorPose || _config.YawResponseRate <= 0f)
                return targetYaw;

            var t = Mathf.Clamp01(_config.YawResponseRate * deltaTime);
            return Quaternion.Slerp(_anchor.Rotation, targetYaw, t);
        }

        private void PrimeYawFromLaunch(SlingshotLaunchAppliedEvent launchApplied)
        {
            var planarLaunchDirection = Vector3.ProjectOnPlane(launchApplied.LaunchDirection, _cameraUp);

            if (planarLaunchDirection.IsFinite() && planarLaunchDirection.sqrMagnitude > 0.000001f)
                _lastValidYaw = Quaternion.LookRotation(planarLaunchDirection.normalized, _cameraUp);
        }

        private Vector3 GetValidUpDirection(Vector3 upDirection)
        {
            var sqrMagnitude = upDirection.sqrMagnitude;

            if (sqrMagnitude <= 0.000001f || float.IsNaN(sqrMagnitude) || float.IsInfinity(sqrMagnitude))
                return Vector3.up;

            return upDirection.normalized;
        }

        private readonly struct RunCameraYawDecision
        {
            public Vector3 PlanarVelocity { get; }
            public float PlanarSpeed { get; }
            public bool AcceptedVelocityYaw { get; }
            public Quaternion TargetYaw { get; }

            public RunCameraYawDecision(
                Vector3 planarVelocity,
                float planarSpeed,
                bool acceptedVelocityYaw,
                Quaternion targetYaw)
            {
                PlanarVelocity = planarVelocity;
                PlanarSpeed = planarSpeed;
                AcceptedVelocityYaw = acceptedVelocityYaw;
                TargetYaw = targetYaw;
            }
        }
    }
}
