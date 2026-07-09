using System;
using System.Collections.Generic;
using System.Linq;
using Game.Foundation.ApplicationLifecycle;
using Game.Foundation.Input;
using Game.Foundation.Persistence;
using Game.Foundation.Screen;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Diagnostics;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Pickups;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed partial class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplayStateConfig _gameplayStateConfig;
        [SerializeField] private GameplayStateId _runPreparationStateId;
        [SerializeField] private GameplayStateId _preLaunchStateId;
        [SerializeField] private GameplayStateId _runningStateId;
        [SerializeField] private GameplayStateId _runEndedStateId;
        [SerializeField] private UpgradeCatalog _upgradeCatalog;
        [SerializeField] private GameplayStatId _slingshotLaunchPowerStatId;
        [SerializeField] private GameplayStatId _playerMaxSpeedStatId;
        [SerializeField] private GameplayStatId _playerSteeringResponsivenessStatId;
        [SerializeField] private CurrencyDefinition _coinCurrencyDefinition;
        [SerializeField] private GameplayStatId _coinPickupMultiplierStatId;
        [SerializeField] private SlingshotConfig _slingshotConfig;
        [SerializeField] private GameplaySlingshotLaunchConfig _gameplaySlingshotLaunchConfig;
        [SerializeField] private PlayerSteeringConfig _playerSteeringConfig;
        [SerializeField] private RunCameraConfig _runCameraConfig;
        [SerializeField] private RunEndConfig _runEndConfig;
        [SerializeField] private RigidbodyPlayerSteeringTarget _playerSteeringTarget;
        [SerializeField] private RigidbodyRunCameraSource _runCameraSource;
        [SerializeField] private RunProgressFrameSource _runProgressFrameSource;
        [SerializeField] private BaseSceneCompositionMonoInstaller[] _sceneCompositionInstallers;
        [SerializeField] private RigidbodyContactNotifier _contactNotifier;
        [SerializeField] private TransformRunCameraAnchor _runCameraAnchor;
        [SerializeField] private CinemachineRunCameraRig _runCameraRig;
        [SerializeField] private Camera _inputCamera;
        [SerializeField] private Transform _slingshotRig;
        [SerializeField] private Transform _preLaunchSlingshotRigPose;
        [SerializeField] private Transform _preLaunchLaunchTargetPose;
        [SerializeField] private SlingshotView _slingshotView;
        [SerializeField] private PullHintView _pullHintView;
        [SerializeField] private RunSteeringAffordanceView _runSteeringAffordanceView;
        [SerializeField] private RunPreparationUIView _runPreparationView;
        [SerializeField] private RunEndedUIView _runEndedView;
        [SerializeField] private RigidbodyLaunchTarget _launchTarget;
        [SerializeField] private CharacterPresentationView _characterPresentationView;
        [SerializeField] private AnimatedContactSensorPoseSyncView _animatedContactSensorPoseSyncView;
        [SerializeField] private FinishPresentationView _finishPresentationView;
        
        [Header("Diagnostics")]
        [SerializeField] private bool _runDiagnosticsOverlayEnabled;

        protected override void Configure(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            PrepareGameplaySceneComposition();
            ThrowIfInvalidReferences();
            InstallGameplay(builder);
        }

        private void PrepareGameplaySceneComposition()
        {
            var scene = gameObject.scene;

            if (!scene.IsValid() || !scene.isLoaded)
                return;

            var preCompositionSteps = scene.GetRootGameObjects()
                .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<MonoBehaviour>(true))
                .OfType<IGameplayScenePreCompositionStep>()
                .ToArray();

            for (var stepIndex = 0; stepIndex < preCompositionSteps.Length; stepIndex += 1)
            {
                preCompositionSteps[stepIndex].PrepareGameplaySceneComposition();
            }
        }

        private void InstallGameplay(IContainerBuilder builder)
        {
            new UnityInputInstaller().Install(builder);
            new GameplayStateInstaller(_gameplayStateConfig).Install(builder);

            builder.RegisterInstance<IUpgradeCatalog>(_upgradeCatalog);
            builder.RegisterInstance<IGameplaySlingshotLaunchConfig>(_gameplaySlingshotLaunchConfig);

            builder.RegisterInstance<ILaunchTarget, IHeldLaunchTarget, ILaunchTargetSilhouetteSource>(_launchTarget)
                .As<ILaunchTargetPreLaunchReset, IRunEndPoseLockTarget>();
            builder.RegisterInstance<IPlayerSteeringTarget>(_playerSteeringTarget);
            builder.RegisterInstance<IRunCameraSource, IRunMotionSource>(_runCameraSource);
            builder.RegisterInstance<IRunProgressFrameSource>(_runProgressFrameSource);
            builder.Register<IRunSupportColliderProbeFactory, RunSupportColliderProbeFactory>(Lifetime.Singleton);
            InstallSceneComposition(builder);
            builder.RegisterInstance<IRigidbodyContactNotifier>(_contactNotifier);
            builder.RegisterInstance<IRunCameraAnchor>(_runCameraAnchor);
            builder.RegisterInstance<IRunCameraLens>(new TransformRunCameraLens(_inputCamera.transform));
            builder.RegisterInstance<IRunCameraRig>(_runCameraRig);
            builder.RegisterInstance<ICharacterPresentationView>(_characterPresentationView);
            builder.RegisterInstance<ICharacterPresentationTuning>(_characterPresentationView);
            builder.RegisterInstance<ICharacterVisualFollowView>(_characterPresentationView);
            builder.RegisterInstance<ICharacterVisualFollowTuning>(_characterPresentationView);
            builder.RegisterInstance<ICharacterVisualTargetPoseSource>(new TransformCharacterVisualTargetPoseSource(_launchTarget.transform));
            builder.RegisterInstance<IAnimatedContactSensorPoseSyncView>(_animatedContactSensorPoseSyncView);
            builder.RegisterInstance<IFinishPresentationView>(_finishPresentationView);
            builder.RegisterInstance<IPullHintView, IPullHintTuning>(_pullHintView);
            builder.RegisterInstance<IRunSteeringAffordanceView>(
                _runSteeringAffordanceView != null ? _runSteeringAffordanceView : new NullRunSteeringAffordanceView());
            builder.RegisterInstance<IRunPreparationView>(_runPreparationView);
            builder.RegisterInstance<IRunEndedView>(_runEndedView);

            builder.RegisterInstance<IPreLaunchRigPoseResetter>(
                new PreLaunchRigPoseResetter(_slingshotRig, _preLaunchSlingshotRigPose, _launchTarget, _preLaunchLaunchTargetPose));

            new SlingshotInstaller(_slingshotConfig, _slingshotView, _inputCamera).Install(builder);
            new GameplayFlowInstaller(_runPreparationStateId, _preLaunchStateId, _runningStateId, _runEndedStateId).Install(builder);

            builder.RegisterInstance<IPlayerSteeringConfig>(_playerSteeringConfig);
            builder.RegisterInstance<IRunCameraConfig>(_runCameraConfig);
            builder.RegisterInstance<IRunEndConfig>(_runEndConfig);
            builder.RegisterInstance<IRunRewardConfig>(_runEndConfig);

            builder.RegisterInstance(_coinCurrencyDefinition).Keyed(InjectKey.CurrencyDefinition.Coin);

            builder.RegisterInstance(_coinPickupMultiplierStatId).Keyed(InjectKey.GameplayStatId.CoinPickupMultiplier);
            builder.RegisterInstance(_slingshotLaunchPowerStatId).Keyed(InjectKey.GameplayStatId.SlingshotLaunchPower);
            builder.RegisterInstance(_playerMaxSpeedStatId).Keyed(InjectKey.GameplayStatId.PlayerMaxSpeed);
            builder.RegisterInstance(_playerSteeringResponsivenessStatId).Keyed(InjectKey.GameplayStatId.PlayerSteeringResponsiveness);

            builder.Register<IScreen, UnityScreen>(Lifetime.Singleton);
            builder.Register<IRunSteeringGesture, RunSteeringGesture>(Lifetime.Transient);
            builder.Register<IRunSteeringPointerPressGuard, UnityEventSystemRunSteeringPointerPressGuard>(Lifetime.Singleton);
            builder.Register<IRunContactClassifier, RunContactClassifier>(Lifetime.Singleton);

            builder.Register<IRunSteeringFrameSource, IRunSteeringFrameResetter, IFixedTickable, RunSurfaceSteeringFrameSource>(Lifetime.Singleton);
            builder.Register<ICharacterPresentationModeClassifier, CharacterPresentationModeClassifier>(Lifetime.Singleton);
            builder.Register<ICharacterPresentationSupportTracker, CharacterPresentationSupportTracker>(Lifetime.Singleton);
            builder.Register<ICharacterVisualPoseSmoother, CharacterVisualPoseSmoother>(Lifetime.Transient);
            builder.Register<RunSessionBestDistanceTracker>(Lifetime.Singleton);
            builder.Register<RunEndedResultStatsBuilder>(Lifetime.Singleton);
            builder.Register<RunRewardSourceCatalog>(Lifetime.Singleton);
            builder.Register<IRunRewardContributor, AccumulatedRunRewardContributor>(Lifetime.Singleton);
            builder.Register<IRunRewardContributor, DistanceBonusRunRewardContributor>(Lifetime.Singleton);
            builder.Register<IRunRewardContributor, AirTimeBonusRunRewardContributor>(Lifetime.Singleton);
            builder.Register<RunRewardBreakdownBuilder>(Lifetime.Singleton);
            builder.Register<PlayerEconomyState>(Lifetime.Singleton);
            builder.Register<EconomySaveSettings>(Lifetime.Singleton);
            builder.Register<IPersistentDataPathProvider, UnityPersistentDataPathProvider>(Lifetime.Singleton);

            builder.RegisterComponentOnNewGameObject<UnityApplicationLifecycleNotifier>(Lifetime.Singleton, "ApplicationLifecycleNotifier")
                .As<IApplicationPauseNotifier, IApplicationFocusChangeNotifier, IApplicationQuitNotifier>();

            builder.Register<IPlayerEconomyContentIndex, GameplayEconomyContentIndex>(Lifetime.Singleton);
            builder.Register<EconomySaveSerializer>(Lifetime.Singleton);
            builder.Register<IEconomySaveRepository, EconomySaveRepository>(Lifetime.Singleton);
            builder.Register<EconomySaveQueue>(Lifetime.Singleton);
            builder.Register<IEconomyCommitter, EconomyCommitter>(Lifetime.Singleton);
            builder.Register<ICurrencyStorage, CurrencyStorage>(Lifetime.Singleton);
            builder.Register<RunCurrencyAccumulator>(Lifetime.Singleton).As<IRunCurrencyAccumulator, IRunRewardSourceLedger>();
            builder.Register<IUpgradeProgressStorage, UpgradeProgressStorage>(Lifetime.Singleton);
            builder.Register<UpgradeDefinitionValidator>(Lifetime.Singleton);
            builder.Register<UpgradeCatalogValidator>(Lifetime.Singleton);
            builder.Register<UpgradeDefinitionEvaluator>(Lifetime.Singleton);
            builder.Register<UpgradePreviewBuilder>(Lifetime.Singleton);
            builder.Register<UpgradePreviewService>(Lifetime.Singleton);
            builder.Register<UpgradePurchaseService>(Lifetime.Singleton);
            builder.Register<IRunModifierSnapshotFactory, RunModifierSnapshotFactory>(Lifetime.Singleton);
            builder.Register<IRunGameplayStatResolver, RunGameplayStatResolver>(Lifetime.Singleton);
            builder.Register<IPickupCurrencyGrantResolver, CoinPickupCurrencyGrantResolver>(Lifetime.Singleton);
            builder.Register<SlingshotLaunchImpulseCalculator>(Lifetime.Singleton);
            builder.Register<ILaunchImpulseApplier, SlingshotLaunchImpulseApplier>(Lifetime.Singleton);
            builder.Register<IGameplaySlingshotLauncher, GameplaySlingshotLauncher>(Lifetime.Singleton);
            builder.Register<IRunModifierSnapshotProvider, IRunModifierSnapshotStore, RunModifierSnapshotHolder>(Lifetime.Singleton);
            builder.Register<ILevelPickupState, LevelPickupState>(Lifetime.Singleton);

            builder.RegisterEntryPoint<PlayerEconomyStateLoader>();
            builder.RegisterEntryPoint<RunProgressService>();
            builder.RegisterEntryPoint<PlayerSteeringController>();
            builder.RegisterEntryPoint<RunCameraController>();
            builder.RegisterEntryPoint<CharacterVisualFollower>();
            builder.RegisterEntryPoint<AnimatedContactSensorPoseSync>();
            builder.RegisterEntryPoint<RunAirTimeTracker>();
            builder.RegisterEntryPoint<RunEndFlow>();
            builder.RegisterEntryPoint<RunEndPoseLockController>();
            builder.RegisterEntryPoint<RunRewardCommitter>();
            builder.RegisterEntryPoint<EconomyLifecycleFlushController>();
            builder.RegisterEntryPoint<CharacterPresenter>();
            builder.RegisterEntryPoint<PickupCollectionController>();
            builder.RegisterEntryPoint<LostMomentumDetector>();
            builder.RegisterEntryPoint<RunPreparationPresenter>();
            builder.RegisterEntryPoint<RunEndedPresenter>();
            builder.RegisterEntryPoint<FinishCelebrationPresenter>();
            
            if (_runDiagnosticsOverlayEnabled)
            {
                builder.RegisterComponentOnNewGameObject<RunDiagnosticsOverlay>(Lifetime.Singleton, "RunDiagnosticsOverlay");
                builder.RegisterBuildCallback(container => container.Resolve<RunDiagnosticsOverlay>());
            }
        }

        private void InstallSceneComposition(IContainerBuilder builder)
        {
            var installers = _sceneCompositionInstallers ?? Array.Empty<BaseSceneCompositionMonoInstaller>();

            for (var installerIndex = 0; installerIndex < installers.Length; installerIndex += 1)
            {
                var installer = installers[installerIndex];

                if (installer == null)
                {
                    throw new InvalidOperationException(
                        $"GameplayLifetimeScope Scene Composition Installer at index {installerIndex} is missing.");
                }

                installer.Install(builder);
            }
        }

        private IReadOnlyList<GameplayPickupsSceneCompositionMonoInstaller> GetPickupSceneCompositionInstallers()
        {
            return (_sceneCompositionInstallers ?? Array.Empty<BaseSceneCompositionMonoInstaller>())
                .OfType<GameplayPickupsSceneCompositionMonoInstaller>()
                .ToArray();
        }
    }
}
