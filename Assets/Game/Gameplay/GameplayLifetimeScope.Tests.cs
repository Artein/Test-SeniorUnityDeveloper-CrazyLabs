#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    public sealed partial class GameplayLifetimeScope
    {
        internal GameplayStateId RunPreparationStateIdForTests => _runPreparationStateId;
        internal GameplayStateId RunningStateIdForTests => _runningStateId;
        internal GameplayStateId RunEndedStateIdForTests => _runEndedStateId;
        internal PlayerSteeringConfig PlayerSteeringConfigForTests => _playerSteeringConfig;
        internal RunCameraConfig RunCameraConfigForTests => _runCameraConfig;
        internal RunEndConfig RunEndConfigForTests => _runEndConfig;
        internal RunProgressFrameSource RunProgressFrameSourceForTests => _runProgressFrameSource;
        internal IReadOnlyList<string> PickupSetupValidationErrorsForTests => GetPickupSetupValidationErrors().ToArray();

        internal void SetReferencesForTests(
            GameplayStateConfig gameplayStateConfig,
            GameplayStateId runPreparationStateId,
            GameplayStateId preLaunchStateId,
            GameplayStateId runningStateId,
            GameplayStateId runEndedStateId,
            UpgradeCatalog upgradeCatalog,
            GameplayStatId slingshotLaunchPowerStatId,
            GameplayStatId playerMaxSpeedStatId,
            GameplayStatId playerSteeringResponsivenessStatId,
            CurrencyDefinition coinCurrencyDefinition,
            GameplayStatId coinPickupMultiplierStatId,
            SlingshotConfig slingshotConfig,
            GameplaySlingshotLaunchConfig gameplaySlingshotLaunchConfig,
            PlayerSteeringConfig playerSteeringConfig,
            RunCameraConfig runCameraConfig,
            RunEndConfig runEndConfig,
            RigidbodyPlayerSteeringTarget playerSteeringTarget,
            RigidbodyRunCameraSource runCameraSource,
            RunProgressFrameSource runProgressFrameSource,
            BaseSceneCompositionMonoInstaller[] sceneCompositionInstallers,
            RigidbodyContactNotifier contactNotifier,
            TransformRunCameraAnchor runCameraAnchor,
            CinemachineRunCameraRig runCameraRig,
            Camera inputCamera,
            Transform slingshotRig,
            Transform preLaunchSlingshotRigPose,
            Transform preLaunchLaunchTargetPose,
            SlingshotView slingshotView,
            PullHintView pullHintView,
            RunPreparationUIView runPreparationView,
            RunEndedUIView runEndedView,
            RigidbodyLaunchTarget launchTarget,
            CharacterPresentationView characterPresentationView,
            AnimatedContactSensorPoseSyncView animatedContactSensorPoseSyncView,
            FinishPresentationView finishPresentationView)
        {
            _gameplayStateConfig = gameplayStateConfig;
            _runPreparationStateId = runPreparationStateId;
            _preLaunchStateId = preLaunchStateId;
            _runningStateId = runningStateId;
            _runEndedStateId = runEndedStateId;
            _upgradeCatalog = upgradeCatalog;
            _slingshotLaunchPowerStatId = slingshotLaunchPowerStatId;
            _playerMaxSpeedStatId = playerMaxSpeedStatId;
            _playerSteeringResponsivenessStatId = playerSteeringResponsivenessStatId;
            _coinCurrencyDefinition = coinCurrencyDefinition;
            _coinPickupMultiplierStatId = coinPickupMultiplierStatId;
            _slingshotConfig = slingshotConfig;
            _gameplaySlingshotLaunchConfig = gameplaySlingshotLaunchConfig;
            _playerSteeringConfig = playerSteeringConfig;
            _runCameraConfig = runCameraConfig;
            _runEndConfig = runEndConfig;
            _playerSteeringTarget = playerSteeringTarget;
            _runCameraSource = runCameraSource;
            _runProgressFrameSource = runProgressFrameSource;
            _sceneCompositionInstallers = sceneCompositionInstallers;
            _contactNotifier = contactNotifier;
            _runCameraAnchor = runCameraAnchor;
            _runCameraRig = runCameraRig;
            _inputCamera = inputCamera;
            _slingshotRig = slingshotRig;
            _preLaunchSlingshotRigPose = preLaunchSlingshotRigPose;
            _preLaunchLaunchTargetPose = preLaunchLaunchTargetPose;
            _slingshotView = slingshotView;
            _pullHintView = pullHintView;
            _runPreparationView = runPreparationView;
            _runEndedView = runEndedView;
            _launchTarget = launchTarget;
            _characterPresentationView = characterPresentationView;
            _animatedContactSensorPoseSyncView = animatedContactSensorPoseSyncView;
            _finishPresentationView = finishPresentationView;
        }

        internal void SetSceneCompositionInstallersForTests(BaseSceneCompositionMonoInstaller[] sceneCompositionInstallers)
        {
            _sceneCompositionInstallers = sceneCompositionInstallers;
        }

        internal void SetRunDiagnosticsOverlayEnabledForTests(bool enabled)
        {
            _runDiagnosticsOverlayEnabled = enabled;
        }

        internal void ValidateRequiredReferencesForTests()
        {
            ThrowIfInvalidReferences();
        }

        internal void ConfigureForTests(IContainerBuilder builder)
        {
            Configure(builder);
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
