using System;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.CharacterPresentation
{
    internal interface ICharacterVisualLateStep
    {
        void UpdateVisual();
    }

    internal sealed class CharacterVisualFollower : ICharacterVisualLateStep, IInitializable, IDisposable
    {
        private readonly IGameplayStateService _gameplayStateService;
        private readonly ISlingshotLaunchAppliedNotifier _launchAppliedNotifier;
        private readonly IRunResultNotifier _runResultNotifier;
        private readonly IRunCameraLens _runCameraLens;
        private readonly ICharacterVisualTargetPoseSource _targetPoseSource;
        private readonly ICharacterVisualFollowView _view;
        private readonly ICharacterVisualFollowTuning _tuning;
        private readonly ICharacterVisualPoseSmoother _smoother;
        private readonly ITime _clock;
        private readonly GameplayStateId _runPreparationStateId;
        private readonly GameplayStateId _preLaunchStateId;

        private bool _isInitialized;
        private bool _isDisposed;
        private bool _shouldFaceRunCamera;

        public CharacterVisualFollower(
            IGameplayStateService gameplayStateService,
            ISlingshotLaunchAppliedNotifier launchAppliedNotifier,
            IRunResultNotifier runResultNotifier,
            IRunCameraLens runCameraLens,
            ICharacterVisualTargetPoseSource targetPoseSource,
            ICharacterVisualFollowView view,
            ICharacterVisualFollowTuning tuning,
            ICharacterVisualPoseSmoother smoother,
            ITime clock,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId runPreparationStateId,
            [Key(InjectKey.GameplayStateId.PreLaunch)]
            GameplayStateId preLaunchStateId)
        {
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _launchAppliedNotifier = launchAppliedNotifier ?? throw new ArgumentNullException(nameof(launchAppliedNotifier));
            _runResultNotifier = runResultNotifier ?? throw new ArgumentNullException(nameof(runResultNotifier));
            _runCameraLens = runCameraLens ?? throw new ArgumentNullException(nameof(runCameraLens));
            _targetPoseSource = targetPoseSource ?? throw new ArgumentNullException(nameof(targetPoseSource));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _smoother = smoother ?? throw new ArgumentNullException(nameof(smoother));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _runPreparationStateId = runPreparationStateId != null
                ? runPreparationStateId
                : throw new ArgumentNullException(nameof(runPreparationStateId));

            _preLaunchStateId = preLaunchStateId != null ? preLaunchStateId : throw new ArgumentNullException(nameof(preLaunchStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CharacterVisualFollower));

            if (_isInitialized)
                return;

            _launchAppliedNotifier.LaunchApplied += OnLaunchApplied;
            _runResultNotifier.RunResultAccepted += OnRunResultAccepted;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;
            SnapToTargetPose();
        }

        void ICharacterVisualLateStep.UpdateVisual()
        {
            if (_isDisposed || !_isInitialized)
                return;

            ApplyTargetPose(Mathf.Max(0f, _clock.DeltaTime), false);
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _launchAppliedNotifier.LaunchApplied -= OnLaunchApplied;
            _runResultNotifier.RunResultAccepted -= OnRunResultAccepted;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
        }

        private void OnLaunchApplied(SlingshotLaunchAppliedEvent launchApplied)
        {
            if (_isDisposed)
                return;

            _shouldFaceRunCamera = false;
            SnapToTargetPose();
        }

        private void OnRunResultAccepted(RunResult result)
        {
            if (_isDisposed)
                return;

            _shouldFaceRunCamera = result.IsSuccess;
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            if (ReferenceEquals(nextStateId, _runPreparationStateId)
                || ReferenceEquals(nextStateId, _preLaunchStateId))
            {
                _shouldFaceRunCamera = false;
                SnapToTargetPose();
            }
        }

        private void SnapToTargetPose()
        {
            _smoother.Reset();
            ApplyTargetPose(0f, true);
        }

        private void ApplyTargetPose(float deltaTime, bool snap)
        {
            var targetPose = _shouldFaceRunCamera
                ? CreateCameraFacingPose(_targetPoseSource.CurrentPose)
                : _targetPoseSource.CurrentPose;

            var nextPose = _smoother.Update(_view.CurrentVisualPose, targetPose, _tuning, deltaTime, snap);
            _view.ApplyVisualPose(nextPose);
        }

        private CharacterVisualPose CreateCameraFacingPose(CharacterVisualPose targetPose)
        {
            var up = targetPose.Rotation * Vector3.up;
            var directionToCamera = Vector3.ProjectOnPlane(_runCameraLens.Position - targetPose.Position, up);

            if (directionToCamera.sqrMagnitude <= Mathf.Epsilon)
                return targetPose;

            return new CharacterVisualPose(
                targetPose.Position,
                Quaternion.LookRotation(directionToCamera.normalized, up));
        }
    }
}
