using System;
using System.Collections.Generic;
using System.Linq;
using Game.Foundation;
using Game.Foundation.Input;
using Game.Foundation.Screen;
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
        [SerializeField] private RigidbodyContactNotifier _contactNotifier;
        [SerializeField] private TransformRunCameraAnchor _runCameraAnchor;
        [SerializeField] private CinemachineRunCameraRig _runCameraRig;
        [SerializeField] private Camera _inputCamera;
        [SerializeField] private SlingshotView _slingshotView;
        [SerializeField] private RunPreparationUIView _runPreparationView;
        [SerializeField] private RigidbodyLaunchTarget _launchTarget;
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
            builder.RegisterInstance<ILaunchTarget, IHeldLaunchTarget, ILaunchTargetSilhouetteSource>(_launchTarget);
            builder.RegisterInstance<IPlayerSteeringTarget>(_playerSteeringTarget);
            builder.RegisterInstance<IRunCameraSource, IRunMotionSource>(_runCameraSource);
            builder.RegisterInstance<IRunProgressFrameSource>(_runProgressFrameSource);
            builder.RegisterInstance<IRigidbodyContactNotifier>(_contactNotifier);
            builder.RegisterInstance<IRunCameraAnchor>(_runCameraAnchor);
            builder.RegisterInstance<IRunCameraRig>(_runCameraRig);
            builder.RegisterInstance<IRunPreparationView>(_runPreparationView);

            new SlingshotInstaller(_slingshotConfig, _slingshotView, _inputCamera).Install(builder);
            new GameplayFlowInstaller(_runPreparationStateId, _preLaunchStateId, _runningStateId).Install(builder);

            builder.RegisterInstance<IPlayerSteeringConfig>(_playerSteeringConfig);
            builder.RegisterInstance<IRunCameraConfig>(_runCameraConfig);
            builder.RegisterInstance<IRunEndConfig>(_runEndConfig);
            builder.Register<IScreen, UnityScreen>(Lifetime.Singleton);
            builder.Register<IRunContactClassifier, RunContactClassifier>(Lifetime.Singleton);
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
            builder.Register<IPickupCurrencyGrantResolver, CoinPickupCurrencyGrantResolver>(Lifetime.Singleton)
                .WithParameter("coinCurrencyDefinition", _coinCurrencyDefinition)
                .WithParameter("coinPickupMultiplierStatId", _coinPickupMultiplierStatId);
            
            builder.Register<SlingshotLaunchImpulseCalculator>(Lifetime.Singleton);
            builder.Register<ILaunchImpulseApplier, SlingshotLaunchImpulseApplier>(Lifetime.Singleton);

            builder.Register<IGameplaySlingshotLauncher, GameplaySlingshotLauncher>(Lifetime.Singleton)
                .WithParameter("slingshotLaunchPowerStatId", _slingshotLaunchPowerStatId);

            builder.Register<IRunModifierSnapshotProvider, IRunModifierSnapshotStore, RunModifierSnapshotHolder>(Lifetime.Singleton);

            builder.Register<ILevelPickupState, LevelPickupState>(Lifetime.Singleton)
                .WithParameter("pickups", GetLevelPickups());

            builder.RegisterEntryPoint<RunProgressService>();

            builder.RegisterEntryPoint<PlayerSteeringController>()
                .WithParameter(_runningStateId)
                .WithParameter("playerMaxSpeedStatId", _playerMaxSpeedStatId)
                .WithParameter("playerSteeringResponsivenessStatId", _playerSteeringResponsivenessStatId);

            builder.RegisterEntryPoint<RunCameraController>()
                .WithParameter(_runningStateId);

            // TODO - AI Note: Use IID (injection id) instead of argument name
            builder.RegisterEntryPoint<RunEndFlow>()
                .WithParameter("restartStateId", _runPreparationStateId)
                .WithParameter("runningStateId", _runningStateId)
                .WithParameter("runEndedStateId", _runEndedStateId);

            builder.RegisterEntryPoint<PickupCollectionController>()
                .WithParameter("pickups", GetLevelPickups())
                .WithParameter("runningStateId", _runningStateId)
                .WithParameter("currencyGrantResolverResetStateId", _runPreparationStateId)
                .WithParameter("playerTag", _playerTag);

            builder.RegisterEntryPoint<LostMomentumDetector>()
                .WithParameter(_runningStateId);

            builder.RegisterEntryPoint<RunPreparationPresenter>()
                .WithParameter("runPreparationStateId", _runPreparationStateId);
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

            if (_runPreparationStateId == null)
                yield return "GameplayLifetimeScope requires a Run Preparation State Id reference.";

            if (_preLaunchStateId == null)
                yield return "GameplayLifetimeScope requires a Pre-Launch State Id reference.";

            if (_runningStateId == null)
                yield return "GameplayLifetimeScope requires a Running State Id reference.";

            if (_runEndedStateId == null)
                yield return "GameplayLifetimeScope requires a Run Ended State Id reference.";

            if (_upgradeCatalog == null)
                yield return "GameplayLifetimeScope requires an Upgrade Catalog reference.";

            if (_slingshotLaunchPowerStatId == null)
                yield return "GameplayLifetimeScope requires a Slingshot Launch Power Stat Id reference.";

            if (_playerMaxSpeedStatId == null)
                yield return "GameplayLifetimeScope requires a Player Max Speed Stat Id reference.";

            if (_playerSteeringResponsivenessStatId == null)
                yield return "GameplayLifetimeScope requires a Player Steering Responsiveness Stat Id reference.";

            if (_coinCurrencyDefinition == null)
                yield return "GameplayLifetimeScope requires a Coin Currency Definition reference.";

            if (_coinPickupMultiplierStatId == null)
                yield return "GameplayLifetimeScope requires a Coin Pickup Multiplier Stat Id reference.";

            if (_slingshotConfig == null)
                yield return "GameplayLifetimeScope requires a Slingshot Config reference.";

            if (_gameplaySlingshotLaunchConfig == null)
                yield return "GameplayLifetimeScope requires a Gameplay Slingshot Launch Config reference.";

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

            if (_contactNotifier == null)
                yield return "GameplayLifetimeScope requires a Rigidbody Contact Notifier reference.";

            if (_runCameraAnchor == null)
                yield return "GameplayLifetimeScope requires a Run Camera Anchor reference.";

            if (_runCameraRig == null)
                yield return "GameplayLifetimeScope requires a Run Camera Rig reference.";

            if (_inputCamera == null)
                yield return "GameplayLifetimeScope requires an Input Camera reference.";

            if (_slingshotView == null)
                yield return "GameplayLifetimeScope requires a Slingshot View reference.";

            if (_runPreparationView == null)
                yield return "GameplayLifetimeScope requires a Run Preparation View reference.";

            if (_launchTarget == null)
                yield return "GameplayLifetimeScope requires a Launch Target reference.";

            foreach (var error in GetPickupSetupValidationErrors())
                yield return error;
        }

        private IReadOnlyList<Pickup> GetLevelPickups()
        {
            return _levelPickups ?? Array.Empty<Pickup>();
        }

        private IReadOnlyList<Collider> GetPlayerPickupContactColliders()
        {
            return _playerPickupContactColliders ?? Array.Empty<Collider>();
        }

        private IEnumerable<string> GetPickupSetupValidationErrors()
        {
            var validator = new PickupSetupValidator();
            return validator.Validate(GetLevelPickups(), GetPlayerPickupContactColliders(), _playerTag, _playerLayerName, _pickupLayerName);
        }
    }
}
