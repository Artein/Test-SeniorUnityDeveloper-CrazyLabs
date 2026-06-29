using System;
using System.Collections.Generic;
using System.Linq;
using Game.Foundation.Input;
using Game.Foundation.Screen;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed partial class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplayStateConfig _gameplayStateConfig;
        [SerializeField] private GameplayStateId _preLaunchStateId;
        [SerializeField] private GameplayStateId _runningStateId;
        [SerializeField] private GameplayStateId _runEndedStateId;
        [SerializeField] private SlingshotConfig _slingshotConfig;
        [SerializeField] private PlayerSteeringConfig _playerSteeringConfig;
        [SerializeField] private RunCameraConfig _runCameraConfig;
        [SerializeField] private RunEndConfig _runEndConfig;
        [SerializeField] private RigidbodyPlayerSteeringTarget _playerSteeringTarget;
        [SerializeField] private RigidbodyRunCameraSource _runCameraSource;
        [SerializeField] private RunProgressFrameSource _runProgressFrameSource;
        [SerializeField] private PhysicsRunSurfaceContextSource _runSurfaceContextSource;
        [SerializeField] private RigidbodyContactNotifier _contactNotifier;
        [SerializeField] private TransformRunCameraAnchor _runCameraAnchor;
        [SerializeField] private CinemachineRunCameraRig _runCameraRig;
        [SerializeField] private Camera _inputCamera;
        [SerializeField] private Transform _slingshotRig;
        [SerializeField] private Transform _preLaunchSlingshotRigPose;
        [SerializeField] private Transform _preLaunchLaunchTargetPose;
        [SerializeField] private SlingshotView _slingshotView;
        [SerializeField] private RigidbodyLaunchTarget _launchTarget;
        [SerializeField] private CharacterPresentationView _characterPresentationView;

        protected override void Configure(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            ThrowIfInvalidReferences();
            InstallGameplay(builder);
        }

        private void InstallGameplay(IContainerBuilder builder)
        {
            new UnityInputInstaller().Install(builder);
            new GameplayStateInstaller(_gameplayStateConfig).Install(builder);

            builder.RegisterInstance<ILaunchTarget, IHeldLaunchTarget, ILaunchTargetSilhouetteSource>(_launchTarget)
                .As<ILaunchTargetPreLaunchReset>();
            builder.RegisterInstance<IPlayerSteeringTarget>(_playerSteeringTarget);
            builder.RegisterInstance<IRunCameraSource, IRunMotionSource>(_runCameraSource);
            builder.RegisterInstance<IRunProgressFrameSource>(_runProgressFrameSource);
            builder.RegisterInstance<IRunSurfaceContextSource>(_runSurfaceContextSource);
            builder.RegisterInstance<IRigidbodyContactNotifier>(_contactNotifier);
            builder.RegisterInstance<IRunCameraAnchor>(_runCameraAnchor);
            builder.RegisterInstance<IRunCameraRig>(_runCameraRig);
            builder.RegisterInstance<ICharacterPresentationView, ICharacterPresentationTuning>(_characterPresentationView);

            builder.RegisterInstance<IPreLaunchRigPoseResetter>(
                new PreLaunchRigPoseResetter(_slingshotRig, _preLaunchSlingshotRigPose, _launchTarget, _preLaunchLaunchTargetPose));

            new SlingshotInstaller(_slingshotConfig, _slingshotView, _inputCamera).Install(builder);
            new GameplayFlowInstaller(_preLaunchStateId, _runningStateId).Install(builder);

            builder.RegisterInstance<IPlayerSteeringConfig>(_playerSteeringConfig);
            builder.RegisterInstance<IRunCameraConfig>(_runCameraConfig);
            builder.RegisterInstance<IRunEndConfig>(_runEndConfig);
            builder.Register<IScreen, UnityScreen>(Lifetime.Singleton);
            builder.Register<IRunContactClassifier, RunContactClassifier>(Lifetime.Singleton);
            builder.Register<ICharacterPresentationModeClassifier, CharacterPresentationModeClassifier>(Lifetime.Singleton);

            builder.RegisterEntryPoint<RunProgressService>();

            builder.RegisterEntryPoint<PlayerSteeringController>()
                .WithParameter(_runningStateId);

            builder.RegisterEntryPoint<RunCameraController>()
                .WithParameter(_runningStateId);

            // TODO - AI Note: Use IID (injection id) instead of argument name
            builder.RegisterEntryPoint<RunEndFlow>()
                .WithParameter("preLaunchStateId", _preLaunchStateId)
                .WithParameter("runningStateId", _runningStateId)
                .WithParameter("runEndedStateId", _runEndedStateId);

            builder.RegisterEntryPoint<CharacterPresentationPresenter>()
                .WithParameter("preLaunchStateId", _preLaunchStateId)
                .WithParameter("runningStateId", _runningStateId);

            builder.RegisterEntryPoint<LostMomentumDetector>()
                .WithParameter(_runningStateId);
        }

        // TODO - AI Note: Move to partial-class "GameplayLifetimeScope.Validation.cs" file
        private void OnValidate()
        {
            LogReferenceValidationWarnings();
        }

        private void LogReferenceValidationWarnings()
        {
            foreach (var error in GetReferenceValidationErrors())
            {
                // TODO - AI Note: We should use Debug.LogXYZFormat() instead
                Debug.LogWarning(error, this);
            }
        }

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToList();

            if (errors.Any())
                throw new InvalidOperationException(string.Join("\n", errors));
        }

        private IEnumerable<string> GetReferenceValidationErrors()
        {
            if (_gameplayStateConfig == null)
                yield return "GameplayLifetimeScope requires a Gameplay State Config reference.";

            if (_preLaunchStateId == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch State Id reference.";

            if (_runningStateId == null)
                yield return "GameplayLifetimeScope requires a Running State Id reference.";

            if (_runEndedStateId == null)
                yield return "GameplayLifetimeScope requires a Run Ended State Id reference.";

            if (_slingshotConfig == null)
                yield return "GameplayLifetimeScope requires a Slingshot Config reference.";

            if (_playerSteeringConfig == null)
                yield return "GameplayLifetimeScope requires a Player Steering Config reference.";

            if (_runCameraConfig == null)
                yield return "GameplayLifetimeScope requires a Run Camera Config reference.";

            if (_runEndConfig == null)
                yield return "GameplayLifetimeScope requires a Run End Config reference.";

            if (_playerSteeringTarget == null)
                yield return "GameplayLifetimeScope requires a Player Steering Target reference.";

            if (_runCameraSource == null)
                yield return "GameplayLifetimeScope requires a Run Camera Source reference.";

            if (_runProgressFrameSource == null)
                yield return "GameplayLifetimeScope requires a Run Progress Frame Source reference.";

            if (_runSurfaceContextSource == null)
                yield return "GameplayLifetimeScope requires a Run Surface Context Source reference.";

            if (_contactNotifier == null)
                yield return "GameplayLifetimeScope requires a Rigidbody Contact Notifier reference.";

            if (_runCameraAnchor == null)
                yield return "GameplayLifetimeScope requires a Run Camera Anchor reference.";

            if (_runCameraRig == null)
                yield return "GameplayLifetimeScope requires a Run Camera Rig reference.";

            if (_inputCamera == null)
                yield return "GameplayLifetimeScope requires an Input Camera reference.";

            if (_slingshotRig == null)
                yield return "GameplayLifetimeScope requires a Slingshot Rig Transform reference.";

            if (_preLaunchSlingshotRigPose == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch Slingshot Rig Pose reference.";

            if (_preLaunchLaunchTargetPose == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch Launch Target Pose reference.";

            if (_slingshotView == null)
                yield return "GameplayLifetimeScope requires a Slingshot View reference.";

            if (_launchTarget == null)
                yield return "GameplayLifetimeScope requires a Launch Target reference.";

            if (_characterPresentationView == null)
                yield return "GameplayLifetimeScope requires a Character Presentation View reference.";
        }
    }
}
