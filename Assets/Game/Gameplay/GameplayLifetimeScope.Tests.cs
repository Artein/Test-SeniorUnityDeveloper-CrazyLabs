#if UNITY_INCLUDE_TESTS

using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    public sealed partial class GameplayLifetimeScope
    {
        internal PlayerSteeringConfig PlayerSteeringConfigForTests => _playerSteeringConfig;
        internal RunCameraConfig RunCameraConfigForTests => _runCameraConfig;
        internal RunEndConfig RunEndConfigForTests => _runEndConfig;
        internal RunProgressFrameSource RunProgressFrameSourceForTests => _runProgressFrameSource;

        internal void SetReferencesForTests(
            GameplayStateConfig gameplayStateConfig,
            GameplayStateId preLaunchStateId,
            GameplayStateId runningStateId,
            GameplayStateId runEndedStateId,
            SlingshotConfig slingshotConfig,
            PlayerSteeringConfig playerSteeringConfig,
            RunCameraConfig runCameraConfig,
            RunEndConfig runEndConfig,
            RigidbodyPlayerSteeringTarget playerSteeringTarget,
            RigidbodyRunCameraSource runCameraSource,
            RunProgressFrameSource runProgressFrameSource,
            RigidbodyContactNotifier contactNotifier,
            TransformRunCameraAnchor runCameraAnchor,
            CinemachineRunCameraRig runCameraRig,
            Camera inputCamera,
            SlingshotView slingshotView,
            RigidbodyLaunchTarget launchTarget)
        {
            _gameplayStateConfig = gameplayStateConfig;
            _preLaunchStateId = preLaunchStateId;
            _runningStateId = runningStateId;
            _runEndedStateId = runEndedStateId;
            _slingshotConfig = slingshotConfig;
            _playerSteeringConfig = playerSteeringConfig;
            _runCameraConfig = runCameraConfig;
            _runEndConfig = runEndConfig;
            _playerSteeringTarget = playerSteeringTarget;
            _runCameraSource = runCameraSource;
            _runProgressFrameSource = runProgressFrameSource;
            _contactNotifier = contactNotifier;
            _runCameraAnchor = runCameraAnchor;
            _runCameraRig = runCameraRig;
            _inputCamera = inputCamera;
            _slingshotView = slingshotView;
            _launchTarget = launchTarget;
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
