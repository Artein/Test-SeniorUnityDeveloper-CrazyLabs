using System;
using System.Collections.Generic;
using Game.Foundation;
using Game.Foundation.Input;
using Game.Foundation.Screen;
using Game.Gameplay.CharacterPresentation;
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
        [SerializeField] private PhysicsRunSurfaceContextSource _runSurfaceContextSource;
        [SerializeField] private RigidbodyContactNotifier _contactNotifier;
        [SerializeField] private TransformRunCameraAnchor _runCameraAnchor;
        [SerializeField] private CinemachineRunCameraRig _runCameraRig;
        [SerializeField] private Camera _inputCamera;
        [SerializeField] private Transform _slingshotRig;
        [SerializeField] private Transform _preLaunchSlingshotRigPose;
        [SerializeField] private Transform _preLaunchLaunchTargetPose;
        [SerializeField] private SlingshotView _slingshotView;
        [SerializeField] private PullHintView _pullHintView;
        [SerializeField] private RunPreparationUIView _runPreparationView;
        [SerializeField] private RigidbodyLaunchTarget _launchTarget;
        [SerializeField] private CharacterPresentationView _characterPresentationView;
        [SerializeField] [TagSelector] private string _playerTag = "Player";
        [SerializeField] private string _playerLayerName = "Player";
        [SerializeField] private string _pickupLayerName = "Pickup";
        [SerializeField] private Pickup[] _levelPickups = Array.Empty<Pickup>();
        [SerializeField] private Collider[] _playerPickupContactColliders = Array.Empty<Collider>();

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

            builder.RegisterInstance<IUpgradeCatalog>(_upgradeCatalog);
            builder.RegisterInstance<IGameplaySlingshotLaunchConfig>(_gameplaySlingshotLaunchConfig);

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
            builder.RegisterInstance<IPullHintView, IPullHintTuning>(_pullHintView);
            builder.RegisterInstance<IRunPreparationView>(_runPreparationView);

            builder.RegisterInstance<IPreLaunchRigPoseResetter>(
                new PreLaunchRigPoseResetter(_slingshotRig, _preLaunchSlingshotRigPose, _launchTarget, _preLaunchLaunchTargetPose));

            new SlingshotInstaller(_slingshotConfig, _slingshotView, _inputCamera).Install(builder);
            new GameplayFlowInstaller(_runPreparationStateId, _preLaunchStateId, _runningStateId, _runEndedStateId).Install(builder);

            builder.RegisterInstance<IPlayerSteeringConfig>(_playerSteeringConfig);
            builder.RegisterInstance<IRunCameraConfig>(_runCameraConfig);
            builder.RegisterInstance<IRunEndConfig>(_runEndConfig);

            builder.RegisterInstance(_coinCurrencyDefinition).Keyed(InjectKey.CurrencyDefinition.Coin);

            builder.RegisterInstance(_coinPickupMultiplierStatId).Keyed(InjectKey.GameplayStatId.CoinPickupMultiplier);
            builder.RegisterInstance(_slingshotLaunchPowerStatId).Keyed(InjectKey.GameplayStatId.SlingshotLaunchPower);
            builder.RegisterInstance(_playerMaxSpeedStatId).Keyed(InjectKey.GameplayStatId.PlayerMaxSpeed);
            builder.RegisterInstance(_playerSteeringResponsivenessStatId).Keyed(InjectKey.GameplayStatId.PlayerSteeringResponsiveness);

            var levelPickups = GetLevelPickups();
            builder.RegisterInstance<IReadOnlyList<Pickup>>(levelPickups).Keyed(InjectKey.Pickups.LevelPickups);
            builder.RegisterInstance(_playerTag).Keyed(InjectKey.Tags.Player);

            builder.Register<IScreen, UnityScreen>(Lifetime.Singleton);
            builder.Register<IRunContactClassifier, RunContactClassifier>(Lifetime.Singleton);
            builder.Register<ICharacterPresentationModeClassifier, CharacterPresentationModeClassifier>(Lifetime.Singleton);
            builder.Register<PlayerEconomyState>(Lifetime.Singleton);
            builder.Register<EconomySaveSettings>(Lifetime.Singleton);
            builder.Register<IPersistentDataPathProvider, UnityPersistentDataPathProvider>(Lifetime.Singleton);

            builder.Register<IPlayerEconomyContentIndex, GameplayEconomyContentIndex>(Lifetime.Singleton)
                .WithParameter("coinCurrencyDefinition", _coinCurrencyDefinition);

            builder.Register<EconomySaveSerializer>(Lifetime.Singleton);
            builder.Register<IEconomySaveRepository, EconomySaveRepository>(Lifetime.Singleton);
            builder.Register<EconomySaveQueue>(Lifetime.Singleton);
            builder.Register<IEconomyCommitter, EconomyCommitter>(Lifetime.Singleton);
            builder.Register<ICurrencyStorage, CurrencyStorage>(Lifetime.Singleton);
            builder.Register<IRunCurrencyAccumulator, RunCurrencyAccumulator>(Lifetime.Singleton);
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
            builder.RegisterEntryPoint<RunEndFlow>();
            builder.RegisterEntryPoint<RunRewardCommitter>();
            builder.RegisterEntryPoint<EconomyLifecycleFlushController>();
            builder.RegisterEntryPoint<CharacterPresentationPresenter>();
            builder.RegisterEntryPoint<PickupCollectionController>();
            builder.RegisterEntryPoint<LostMomentumDetector>();
            builder.RegisterEntryPoint<RunPreparationPresenter>();
        }

        private IReadOnlyList<Pickup> GetLevelPickups()
        {
            return _levelPickups ?? Array.Empty<Pickup>();
        }

        private IReadOnlyList<Collider> GetPlayerPickupContactColliders()
        {
            return _playerPickupContactColliders ?? Array.Empty<Collider>();
        }

        public static class Serialization
        {
            public const string LevelPickups = nameof(_levelPickups);
        }
    }
}
