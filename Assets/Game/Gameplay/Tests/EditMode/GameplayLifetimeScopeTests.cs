using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using Game.Foundation.ApplicationLifecycle;
using Game.Foundation.Input;
using Game.Foundation.Physics;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Diagnostics;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Internal;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class GameplayLifetimeScopeTests
{
    private readonly List<UnityEngine.Object> _objects = new();
    private readonly List<string> _assetDirectories = new();

    [TearDown]
    public void OnTearDown()
    {
        DestroyGeneratedGameObjects<RunDiagnosticsOverlay>();
        DestroyGeneratedGameObjects<UnityApplicationLifecycleNotifier>();

        foreach (var assetDirectory in _assetDirectories)
        {
            AssetDatabase.DeleteAsset(assetDirectory);
        }

        _assetDirectories.Clear();
        AssetDatabase.Refresh();

        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void AddComponent_GameplayLifetimeScope_ComponentIsCreated()
    {
        var gameObject = CreateGameObject("Gameplay Lifetime Scope Test");

        var lifetimeScope = gameObject.AddComponent<GameplayLifetimeScope>();

        Assert.That(lifetimeScope, Is.Not.Null);
    }

    [Test]
    public void ValidateRequiredReferencesForTests_MissingReferences_ThrowsWithRequiredReferenceNames()
    {
        var scope = CreateGameObject("Gameplay Lifetime Scope Test").AddComponent<GameplayLifetimeScope>();

        Assert.That(
            scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains("Gameplay State Config")
                .And.Message.Contains("Run Preparation State Id")
                .And.Message.Contains("Pre-Launch State Id")
                .And.Message.Contains("Running State Id")
                .And.Message.Contains("Run Ended State Id")
                .And.Message.Contains("Upgrade Catalog")
                .And.Message.Contains("Slingshot Launch Power Stat Id")
                .And.Message.Contains("Player Max Speed Stat Id")
                .And.Message.Contains("Player Steering Responsiveness Stat Id")
                .And.Message.Contains("Coin Currency Definition")
                .And.Message.Contains("Coin Pickup Multiplier Stat Id")
                .And.Message.Contains("Slingshot Config")
                .And.Message.Contains("Gameplay Slingshot Launch Config")
                .And.Message.Contains("Run Body Movement Tuning")
                .And.Message.Contains("Run Camera Config")
                .And.Message.Contains("Run End Config")
                .And.Message.Contains("Run Body Movement Target")
                .And.Message.Contains("Run Camera Source")
                .And.Message.Contains("Run Progress Frame Source")
                .And.Message.Contains("Scene Composition Installer")
                .And.Message.Contains("Rigidbody Contact Notifier")
                .And.Message.Contains("Run Camera Anchor")
                .And.Message.Contains("Run Camera Rig")
                .And.Message.Contains("Input Camera")
                .And.Message.Contains("Slingshot Rig Transform")
                .And.Message.Contains("Pre-Launch Slingshot Rig Pose")
                .And.Message.Contains("Pre-Launch Launch Target Pose")
                .And.Message.Contains("Slingshot View")
                .And.Message.Contains("Pull Hint View")
                .And.Message.Contains("Run Steering Affordance View")
                .And.Message.Contains("Run Preparation View")
                .And.Message.Contains("Run Ended View")
                .And.Message.Contains("Launch Target")
                .And.Message.Contains("Character Presentation View")
                .And.Message.Contains("Animated Contact Sensor Pose Sync View")
                .And.Message.Contains("Finish Presentation View")
                .And.Message.Contains("Gameplay Pickups Scene Composition Installer"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_AllReferencesAssigned_DoesNotThrow()
    {
        var fixture = CreateValidScopeFixture();

        Assert.That(fixture.Scope.ValidateRequiredReferencesForTests, Throws.Nothing);
    }

    [Test]
    public void ValidateRequiredReferencesForTests_InvalidMovementConfig_ThrowsWithEveryConfigError()
    {
        var fixture = CreateValidScopeFixture();

        fixture.RunBodyMovementConfig.SetSpeedValuesForTests(
            downhillAcceleration: float.NaN,
            surfaceSlowdown: -1f,
            lowSpeedAssistTargetSpeed: 5f,
            lowSpeedAssistAcceleration: 8f,
            baseSoftMaximumSpeed: 20f,
            aboveMaximumSpeedResistance: 12f);

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains(nameof(IRunBodySpeedConfig.DownhillAcceleration))
                .And.Message.Contains(nameof(IRunBodySpeedConfig.SurfaceSlowdown)));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_MissingRunSteeringAffordanceView_ThrowsWithRequiredReferenceName()
    {
        var fixture = CreateValidScopeFixture();

        fixture.Scope.SetRunSteeringAffordanceViewForTests(null);

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Run Steering Affordance View"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_InvalidRunSteeringAffordanceView_ThrowsWithSetupMessage()
    {
        var fixture = CreateValidScopeFixture();
        var invalidView = CreateGameObject("Invalid Run Steering Affordance").AddComponent<RunSteeringAffordanceView>();

        fixture.Scope.SetRunSteeringAffordanceViewForTests(invalidView);

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("RunSteeringAffordanceView requires"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_EmptySceneCompositionInstallers_ThrowsWithSceneCompositionInstallerMessage()
    {
        var fixture = CreateValidScopeFixture();

        fixture.Scope.SetSceneCompositionInstallersForTests(Array.Empty<BaseSceneCompositionMonoInstaller>());

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("at least one Scene Composition Installer"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_NullSceneCompositionInstallerEntry_ThrowsWithInstallerIndexMessage()
    {
        var fixture = CreateValidScopeFixture();

        fixture.Scope.SetSceneCompositionInstallersForTests(new BaseSceneCompositionMonoInstaller[] { null });

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Scene Composition Installer at index 0"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_DuplicateSceneCompositionInstaller_ThrowsWithDuplicateInstallerMessage()
    {
        var fixture = CreateValidScopeFixture();

        fixture.Scope.SetSceneCompositionInstallersForTests(new BaseSceneCompositionMonoInstaller[]
        {
            fixture.RunSurfaceInstaller,
            fixture.RunSurfaceInstaller
        });

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("duplicate Scene Composition Installer"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_MissingPhysicsSceneInstallerReferences_ThrowsWithReferenceNames()
    {
        var fixture = CreateValidScopeFixture();

        var invalidInstaller = CreateGameObject("Invalid Run Surface Installer")
            .AddComponent<GameplayPhysicsSceneCompositionMonoInstaller>();

        fixture.Scope.SetSceneCompositionInstallersForTests(new BaseSceneCompositionMonoInstaller[] { invalidInstaller });

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.Contains("Support Collider"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_SameCurrencyReferencedByCoinCatalogAndPickup_DoesNotThrow()
    {
        var fixture = CreateValidScopeFixture();

        Assert.That(fixture.Scope.ValidateRequiredReferencesForTests, Throws.Nothing);
    }

    [Test]
    public void ValidateRequiredReferencesForTests_DistinctReachableCurrencyWithDuplicateSaveId_ThrowsWithDuplicateSaveIdMessage()
    {
        var fixture = CreateValidScopeFixture();
        var duplicateCurrencyDefinition = CreateCurrencyDefinition("Duplicate Coins", fixture.CoinCurrencyDefinition.SaveId);
        var duplicatePickupDefinition = CreatePickupDefinition(duplicateCurrencyDefinition, 1);
        var duplicatePickup = CreatePickup("Duplicate Currency Pickup", duplicatePickupDefinition);

        fixture.PickupsInstaller.SetReferencesForTests(
            levelPickups: new[] { fixture.LevelPickup, duplicatePickup },
            fixture.PickupSensorSource,
            pickupLayerName: "Pickup",
            playerBodyPartLayerName: "PlayerBodyPart");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("duplicate save id"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_BlankReachablePickupCurrencySaveId_ThrowsWithStableSaveIdMessage()
    {
        var fixture = CreateValidScopeFixture();
        var blankCurrencyDefinition = CreateCurrencyDefinition("Blank Pickup Currency", string.Empty);
        var pickupDefinition = CreatePickupDefinition(blankCurrencyDefinition, 1);
        var pickup = CreatePickup("Blank Currency Pickup", pickupDefinition);

        fixture.PickupsInstaller.SetReferencesForTests(
            levelPickups: new[] { pickup },
            fixture.PickupSensorSource,
            pickupLayerName: "Pickup",
            playerBodyPartLayerName: "PlayerBodyPart");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("requires a stable save id"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_MismatchedReachablePickupCurrencySaveId_ThrowsWithAssetGuidMessage()
    {
        var fixture = CreateValidScopeFixture();
        var mismatchedCurrencyDefinition = CreateCurrencyDefinitionAsset("Mismatched Pickup Currency", "currency-copied");
        var pickupDefinition = CreatePickupDefinition(mismatchedCurrencyDefinition, 1);
        var pickup = CreatePickup("Mismatched Currency Pickup", pickupDefinition);

        fixture.PickupsInstaller.SetReferencesForTests(
            levelPickups: new[] { pickup },
            fixture.PickupSensorSource,
            pickupLayerName: "Pickup",
            playerBodyPartLayerName: "PlayerBodyPart");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("does not match its asset GUID"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_DuplicatePickupReference_ThrowsWithDuplicatePickupMessage()
    {
        var fixture = CreateValidScopeFixture();

        fixture.PickupsInstaller.SetReferencesForTests(
            levelPickups: new[] { fixture.LevelPickup, fixture.LevelPickup },
            fixture.PickupSensorSource,
            pickupLayerName: "Pickup",
            playerBodyPartLayerName: "PlayerBodyPart");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("duplicate Level Pickup"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_MissingPickupSensorSource_ThrowsWithSensorSourceMessage()
    {
        var fixture = CreateValidScopeFixture();

        fixture.PickupsInstaller.SetReferencesForTests(
            levelPickups: new[] { fixture.LevelPickup },
            pickupSensorSource: null,
            pickupLayerName: "Pickup",
            playerBodyPartLayerName: "PlayerBodyPart");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Pickup Sensor Source"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_MissingPickupDefinition_ThrowsWithPickupDefinitionMessage()
    {
        var fixture = CreateValidScopeFixture();
        var invalidPickup = CreatePickup("Invalid Pickup", null);

        fixture.PickupsInstaller.SetReferencesForTests(
            levelPickups: new[] { invalidPickup },
            fixture.PickupSensorSource,
            pickupLayerName: "Pickup",
            playerBodyPartLayerName: "PlayerBodyPart");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Pickup Definition"));
    }

    [Test]
    public void ConfigureForTests_RunDiagnosticsDisabled_DoesNotCreateRunDiagnosticsOverlayOnBuild()
    {
        var fixture = CreateValidScopeFixture();
        var builder = new ContainerBuilder();

        fixture.Scope.ConfigureForTests(builder);

        using var container = builder.Build();

        var overlays = UnityEngine.Object.FindObjectsByType<RunDiagnosticsOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var diagnosticsSource = container.Resolve<IRunBodySpeedDiagnosticsSource>();
        var diagnosticsSink = container.Resolve<IRunBodySpeedDiagnosticsSink>();

        Assert.That(overlays, Is.Empty);
        Assert.That(() => container.Resolve<RunDiagnosticsOverlay>(), Throws.TypeOf<VContainerException>());
        Assert.That(diagnosticsSource, Is.SameAs(diagnosticsSink));
        Assert.That(diagnosticsSource.Current.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Inactive));
    }

    [Test]
    public void ConfigureForTests_RunDiagnosticsEnabled_CreatesRunDiagnosticsOverlayOnBuild()
    {
        var fixture = CreateValidScopeFixture();
        var builder = new ContainerBuilder();
        fixture.Scope.SetRunDiagnosticsOverlayEnabledForTests(true);

        fixture.Scope.ConfigureForTests(builder);

        using var container = builder.Build();

        var overlays = UnityEngine.Object.FindObjectsByType<RunDiagnosticsOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var diagnosticsSource = container.Resolve<IRunBodySpeedDiagnosticsSource>();
        var diagnosticsSink = container.Resolve<IRunBodySpeedDiagnosticsSink>();

        Assert.That(overlays, Has.Length.EqualTo(1));
        Assert.That(overlays[0].gameObject.name, Is.EqualTo("RunDiagnosticsOverlay"));
        Assert.That(container.Resolve<RunDiagnosticsOverlay>(), Is.SameAs(overlays[0]));
        Assert.That(diagnosticsSource, Is.SameAs(diagnosticsSink));
        Assert.That(diagnosticsSource.Current.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Inactive));
    }

    [Test]
    public void ConfigureForTests_ValidReferences_RegistersGameplayComposition()
    {
        var fixture = CreateValidScopeFixture();
        var builder = new ContainerBuilder();

        fixture.Scope.ConfigureForTests(builder);

        using var container = builder.Build();
        var unityInput = container.Resolve<IUnityInput>();
        var gameplayStateService = container.Resolve<IGameplayStateService>();
        var slingshotNotifier = container.Resolve<ISlingshotLaunchNotifier>();
        var slingshotActivePullNotifier = container.Resolve<ISlingshotActivePullNotifier>();
        var slingshotCaptureLifecycleNotifier = container.Resolve<ISlingshotCaptureLifecycleNotifier>();
        var slingshotPresentationContextSource = container.Resolve<ISlingshotPresentationContextSource>();
        var slingshotPullOffsetNormalizer = container.Resolve<ISlingshotPullOffsetNormalizer>();
        var slingshotRunPreparationReset = container.Resolve<ISlingshotRunPreparationReset>();
        var slingshotLauncher = container.Resolve<IGameplaySlingshotLauncher>();
        var gameplaySlingshotLaunchConfig = container.Resolve<IGameplaySlingshotLaunchConfig>();
        var launchImpulseCalculator = container.Resolve<SlingshotLaunchImpulseCalculator>();
        var launchImpulseApplier = container.Resolve<ILaunchImpulseApplier>();
        var launchAppliedNotifier = container.Resolve<ISlingshotLaunchAppliedNotifier>();
        var launchAppliedPublisher = container.Resolve<ISlingshotLaunchAppliedPublisher>();
        var continueCommand = container.Resolve<IRunPreparationContinueCommand>();
        var initializables = container.Resolve<ContainerLocal<IReadOnlyList<IInitializable>>>().Value;
        var tickables = container.Resolve<ContainerLocal<IReadOnlyList<ITickable>>>().Value;
        var fixedTickables = container.Resolve<ContainerLocal<IReadOnlyList<IFixedTickable>>>().Value;
        var lateTickables = container.Resolve<ContainerLocal<IReadOnlyList<ILateTickable>>>().Value;
        var launchTarget = container.Resolve<ILaunchTarget>();
        var heldLaunchTarget = container.Resolve<IHeldLaunchTarget>();
        var silhouetteSource = container.Resolve<ILaunchTargetSilhouetteSource>();
        var launchTargetPreLaunchReset = container.Resolve<ILaunchTargetPreLaunchReset>();
        var runEndPoseLockTarget = container.Resolve<IRunEndPoseLockTarget>();
        var preLaunchRigPoseResetter = container.Resolve<IPreLaunchRigPoseResetter>();
        var movementTarget = container.Resolve<IRunBodyMovementTarget>();
        var speedConfig = container.Resolve<IRunBodySpeedConfig>();
        var movementValidityConfig = container.Resolve<IRunBodyMovementValidityConfig>();
        var landingStabilizationConfig = container.Resolve<IRunLaunchLandingStabilizationConfig>();
        var steeringConfig = container.Resolve<IRunSteeringConfig>();
        var steeringFrameConfig = container.Resolve<IRunSteeringFrameConfig>();
        var steeringInputMetricsResolver = container.Resolve<IRunSteeringInputMetricsResolver>();
        var steeringInputSource = container.Resolve<IRunSteeringInputSource>();
        var speedEvaluator = container.Resolve<IRunBodySpeedEvaluator>();
        var speedDiagnosticsSource = container.Resolve<IRunBodySpeedDiagnosticsSource>();
        var speedDiagnosticsSink = container.Resolve<IRunBodySpeedDiagnosticsSink>();
        var steeringEvaluator = container.Resolve<IRunSteeringEvaluator>();
        var launchLandingStabilizer = container.Resolve<IRunLaunchLandingStabilizer>();
        var runSteeringGesture = container.Resolve<IRunSteeringGesture>();
        var runSteeringAffordanceLayout = container.Resolve<IRunSteeringAffordanceLayout>();
        var runSteeringAffordancePresenter = container.Resolve<IRunSteeringAffordancePresenter>();
        var runSteeringAffordanceView = container.Resolve<IRunSteeringAffordanceView>();
        var runSteeringAffordanceTuning = container.Resolve<IRunSteeringAffordanceTuning>();
        var runSteeringPointerPressGuard = container.Resolve<IRunSteeringPointerPressGuard>();
        var runSteeringFrameSource = container.Resolve<IRunSteeringFrameSource>();
        var runSteeringFrameResetter = container.Resolve<IRunSteeringFrameResetter>();
        var runCameraConfig = container.Resolve<IRunCameraConfig>();
        var runEndConfig = container.Resolve<IRunEndConfig>();
        var runRewardConfig = container.Resolve<IRunRewardConfig>();
        var runCameraSource = container.Resolve<IRunCameraSource>();
        var runMotionSource = container.Resolve<IRunMotionSource>();
        var runProgressService = container.Resolve<IRunProgressService>();
        var runProgressFrameSource = container.Resolve<IRunProgressFrameSource>();
        var runSupportColliderProbeFactory = container.Resolve<IRunSupportColliderProbeFactory>();
        var runSurfaceContextSource = container.Resolve<IRunSurfaceContextSource>();
        var contactNotifier = container.Resolve<IRigidbodyContactNotifier>();
        var contactClassifier = container.Resolve<IRunContactClassifier>();
        var runEndCandidateReceiver = container.Resolve<IRunEndCandidateReceiver>();
        var runResultNotifier = container.Resolve<IRunResultNotifier>();
        var runResultAcknowledgeCommand = container.Resolve<IRunResultAcknowledgeCommand>();
        var playerEconomyState = container.Resolve<PlayerEconomyState>();
        var applicationPauseNotifier = container.Resolve<IApplicationPauseNotifier>();
        var applicationFocusChangeNotifier = container.Resolve<IApplicationFocusChangeNotifier>();
        var applicationQuitNotifier = container.Resolve<IApplicationQuitNotifier>();
        var currencyStorage = container.Resolve<ICurrencyStorage>();
        var economyContentIndex = container.Resolve<IPlayerEconomyContentIndex>();
        var economySaveRepository = container.Resolve<IEconomySaveRepository>();
        var economyCommitter = container.Resolve<IEconomyCommitter>();
        var runCurrencyAccumulator = container.Resolve<IRunCurrencyAccumulator>();
        var runRewardSourceLedger = container.Resolve<IRunRewardSourceLedger>();
        var runRewardSourceCatalog = container.Resolve<RunRewardSourceCatalog>();
        var runRewardBreakdownBuilder = container.Resolve<RunRewardBreakdownBuilder>();
        var runAirTimeSource = container.Resolve<IRunAirTimeSource>();
        var upgradeCatalog = container.Resolve<IUpgradeCatalog>();
        var upgradeProgressStorage = container.Resolve<IUpgradeProgressStorage>();
        var runModifierSnapshotFactory = container.Resolve<IRunModifierSnapshotFactory>();
        var runModifierSnapshotProvider = container.Resolve<IRunModifierSnapshotProvider>();
        var runModifierSnapshotStore = container.Resolve<IRunModifierSnapshotStore>();
        var runGameplayStatResolver = container.Resolve<IRunGameplayStatResolver>();
        var pickupCurrencyGrantResolver = container.Resolve<IPickupCurrencyGrantResolver>();
        var runPreparationView = container.Resolve<IRunPreparationView>();
        var runEndedView = container.Resolve<IRunEndedView>();
        var runSessionBestDistanceTracker = container.Resolve<RunSessionBestDistanceTracker>();
        var runEndedResultStatsBuilder = container.Resolve<RunEndedResultStatsBuilder>();
        var pullHintView = container.Resolve<IPullHintView>();
        var pullHintTuning = container.Resolve<IPullHintTuning>();
        var upgradePreviewBuilder = container.Resolve<UpgradePreviewBuilder>();
        var upgradePreviewService = container.Resolve<UpgradePreviewService>();
        var upgradePurchaseService = container.Resolve<UpgradePurchaseService>();
        var levelPickupStateService = container.Resolve<ILevelPickupState>();
        var levelPickupState = container.Resolve<ILevelPickupState>();
        var pickupCollectionNotifier = container.Resolve<IPickupCollectionNotifier>();
        var runCameraAnchor = container.Resolve<IRunCameraAnchor>();
        var runCameraLens = container.Resolve<IRunCameraLens>();
        var runCameraRig = container.Resolve<IRunCameraRig>();
        var bandShapeProvider = container.Resolve<ISlingshotBandShapeProvider>();
        var presentationView = container.Resolve<ICharacterPresentationView>();
        var presentationTuning = container.Resolve<ICharacterPresentationTuning>();
        var characterVisualTargetPoseSource = container.Resolve<ICharacterVisualTargetPoseSource>();
        var characterVisualFollowView = container.Resolve<ICharacterVisualFollowView>();
        var characterVisualFollowTuning = container.Resolve<ICharacterVisualFollowTuning>();
        var characterVisualPoseSmoother = container.Resolve<ICharacterVisualPoseSmoother>();
        var presentationClassifier = container.Resolve<ICharacterPresentationModeClassifier>();
        var finishPresentationView = container.Resolve<IFinishPresentationView>();
        var presentationSupportTracker = container.Resolve<ICharacterPresentationSupportTracker>();
        var animatedContactSensorPoseSyncView = container.Resolve<IAnimatedContactSensorPoseSyncView>();
        var pickupContactSource = container.Resolve<IPickupContactSource>();
        var resolvedRunPreparationState = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.RunPreparation);
        var resolvedPreLaunchState = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.PreLaunch);
        var resolvedRunningState = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.Running);
        var resolvedRunEndedState = container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.RunEnded);
        var resolvedCoinCurrencyDefinition = container.Resolve<CurrencyDefinition>(InjectKey.CurrencyDefinition.Coin);
        var resolvedCoinPickupMultiplierStat = container.Resolve<GameplayStatId>(InjectKey.GameplayStatId.CoinPickupMultiplier);
        var resolvedSlingshotLaunchPowerStat = container.Resolve<GameplayStatId>(InjectKey.GameplayStatId.SlingshotLaunchPower);
        var resolvedPlayerMaxSpeedStat = container.Resolve<GameplayStatId>(InjectKey.GameplayStatId.PlayerMaxSpeed);
        var resolvedPlayerSteeringResponsivenessStat = container.Resolve<GameplayStatId>(InjectKey.GameplayStatId.PlayerSteeringResponsiveness);
        var resolvedLevelPickups = container.Resolve<IReadOnlyList<Pickup>>(InjectKey.Pickups.LevelPickups);

        var runSteeringFrameFixedTickableIndex =
            GetFixedTickableIndex(fixedTickables, fixedTickable => ReferenceEquals(fixedTickable, runSteeringFrameSource));

        var runSurfaceContextSourceFixedTickableIndex =
            GetFixedTickableIndex(fixedTickables, fixedTickable => ReferenceEquals(fixedTickable, runSurfaceContextSource));

        var runBodyMovementControllerFixedTickableIndex =
            GetFixedTickableIndex(fixedTickables, fixedTickable => fixedTickable is RunBodyMovementController);
        var characterVisualFollowerLateTickableIndex = GetLateTickableIndex(lateTickables, lateTickable => lateTickable is CharacterVisualFollower);

        var animatedContactSensorPoseSyncLateTickableIndex =
            GetLateTickableIndex(lateTickables, lateTickable => lateTickable is AnimatedContactSensorPoseSync);

        Assert.That(unityInput, Is.Not.Null);
        Assert.That(gameplayStateService.CurrentStateId, Is.SameAs(fixture.RunPreparationStateId));
        Assert.That(slingshotNotifier, Is.Not.Null);
        Assert.That(slingshotRunPreparationReset, Is.Not.Null);
        Assert.That(slingshotLauncher, Is.Not.Null);
        Assert.That(slingshotActivePullNotifier, Is.Not.Null);
        Assert.That(slingshotCaptureLifecycleNotifier, Is.Not.Null);
        Assert.That(slingshotPresentationContextSource, Is.Not.Null);
        Assert.That(slingshotPullOffsetNormalizer, Is.Not.Null);
        Assert.That(gameplaySlingshotLaunchConfig, Is.SameAs(fixture.GameplaySlingshotLaunchConfig));
        Assert.That(launchImpulseCalculator, Is.Not.Null);
        Assert.That(launchImpulseApplier, Is.Not.Null);
        Assert.That(launchAppliedNotifier, Is.Not.Null);
        Assert.That(launchAppliedPublisher, Is.Not.Null);
        Assert.That(continueCommand, Is.Not.Null);
        Assert.That(initializables.Count, Is.EqualTo(22));
        Assert.That(tickables.Count, Is.EqualTo(5));
        Assert.That(fixedTickables.Count, Is.EqualTo(7));
        Assert.That(lateTickables.Count, Is.EqualTo(3));
        Assert.That(launchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(heldLaunchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(silhouetteSource, Is.SameAs(fixture.LaunchTarget));
        Assert.That(launchTargetPreLaunchReset, Is.SameAs(fixture.LaunchTarget));
        Assert.That(runEndPoseLockTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(preLaunchRigPoseResetter, Is.Not.Null);
        Assert.That(movementTarget, Is.SameAs(fixture.RunBodyMovementTarget));
        Assert.That(movementTarget, Is.Not.SameAs(fixture.LaunchTarget));
        Assert.That(speedConfig, Is.SameAs(fixture.RunBodyMovementConfig));
        Assert.That(movementValidityConfig, Is.SameAs(fixture.RunBodyMovementConfig));
        Assert.That(landingStabilizationConfig, Is.SameAs(fixture.RunBodyMovementConfig));
        Assert.That(steeringConfig, Is.SameAs(fixture.RunBodyMovementConfig));
        Assert.That(steeringFrameConfig, Is.SameAs(fixture.RunBodyMovementConfig));
        Assert.That(steeringInputMetricsResolver, Is.TypeOf<DefaultRunSteeringInputMetricsResolver>());
        Assert.That(steeringInputSource, Is.TypeOf<RunSteeringInputController>());
        Assert.That(speedEvaluator, Is.TypeOf<DefaultRunBodySpeedEvaluator>());
        Assert.That(speedDiagnosticsSource, Is.SameAs(speedDiagnosticsSink));
        Assert.That(steeringEvaluator, Is.TypeOf<DefaultRunSteeringEvaluator>());
        Assert.That(launchLandingStabilizer, Is.TypeOf<RunLaunchLandingStabilizer>());
        Assert.That(runSteeringGesture, Is.Not.Null);
        Assert.That(runSteeringAffordanceLayout, Is.TypeOf<RunSteeringAffordanceLayout>());
        Assert.That(runSteeringAffordancePresenter, Is.Not.InstanceOf<MonoBehaviour>());
        Assert.That(runSteeringAffordancePresenter, Is.InstanceOf<ITickable>());
        Assert.That(tickables, Has.Some.SameAs(runSteeringAffordancePresenter));
        Assert.That(runSteeringAffordanceView, Is.SameAs(fixture.RunSteeringAffordanceView));
        Assert.That(runSteeringAffordanceTuning, Is.SameAs(fixture.RunSteeringAffordanceView));
        Assert.That(runSteeringPointerPressGuard, Is.TypeOf<UnityEventSystemRunSteeringPointerPressGuard>());
        Assert.That(runSteeringFrameSource, Is.Not.Null);
        Assert.That(runSteeringFrameResetter, Is.SameAs(runSteeringFrameSource));
        Assert.That(runSurfaceContextSourceFixedTickableIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(runSteeringFrameFixedTickableIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(runBodyMovementControllerFixedTickableIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(fixedTickables.Count(fixedTickable => fixedTickable is RunBodyMovementController), Is.EqualTo(1));
        Assert.That(runSurfaceContextSourceFixedTickableIndex, Is.LessThan(runSteeringFrameFixedTickableIndex));
        Assert.That(runSteeringFrameFixedTickableIndex, Is.LessThan(runBodyMovementControllerFixedTickableIndex));
        Assert.That(runCameraConfig, Is.SameAs(fixture.RunCameraConfig));
        Assert.That(runEndConfig, Is.SameAs(fixture.RunEndConfig));
        Assert.That(runRewardConfig, Is.SameAs(fixture.RunEndConfig));
        Assert.That(runCameraSource, Is.SameAs(fixture.RunCameraSource));
        Assert.That(runMotionSource, Is.SameAs(fixture.RunCameraSource));
        Assert.That(runProgressService, Is.Not.Null);
        Assert.That(runProgressFrameSource, Is.SameAs(fixture.RunProgressFrameSource));
        Assert.That(runSupportColliderProbeFactory, Is.TypeOf<RunSupportColliderProbeFactory>());
        Assert.That(runSurfaceContextSource, Is.TypeOf<PhysicsRunSurfaceContextSource>());
        Assert.That(runSurfaceContextSource, Is.Not.SameAs(fixture.RunSurfaceInstaller));
        Assert.That(contactNotifier, Is.SameAs(fixture.ContactNotifier));
        Assert.That(contactClassifier, Is.Not.Null);
        Assert.That(runEndCandidateReceiver, Is.Not.Null);
        Assert.That(runResultNotifier, Is.Not.Null);
        Assert.That(runResultNotifier, Is.SameAs(runEndCandidateReceiver));
        Assert.That(runResultAcknowledgeCommand, Is.SameAs(runEndCandidateReceiver));
        Assert.That(playerEconomyState, Is.Not.Null);
        Assert.That(applicationPauseNotifier, Is.Not.Null);
        Assert.That(applicationFocusChangeNotifier, Is.SameAs(applicationPauseNotifier));
        Assert.That(applicationQuitNotifier, Is.SameAs(applicationPauseNotifier));
        Assert.That(currencyStorage, Is.Not.Null);
        Assert.That(economyContentIndex.IsKnownCurrencyId(fixture.CoinCurrencyDefinition.SaveId), Is.True);
        Assert.That(economySaveRepository, Is.Not.Null);
        Assert.That(economyCommitter, Is.Not.Null);
        Assert.That(runCurrencyAccumulator, Is.Not.Null);
        Assert.That(runRewardSourceLedger, Is.SameAs(runCurrencyAccumulator));
        Assert.That(runRewardSourceCatalog, Is.Not.Null);
        Assert.That(runRewardBreakdownBuilder, Is.Not.Null);
        Assert.That(runAirTimeSource, Is.Not.Null);
        Assert.That(upgradeCatalog, Is.SameAs(fixture.UpgradeCatalog));
        Assert.That(upgradeProgressStorage, Is.Not.Null);
        Assert.That(runModifierSnapshotFactory, Is.Not.Null);
        Assert.That(runModifierSnapshotProvider, Is.SameAs(runModifierSnapshotStore));
        Assert.That(runGameplayStatResolver, Is.Not.Null);
        Assert.That(pickupCurrencyGrantResolver, Is.Not.Null);
        Assert.That(runPreparationView, Is.Not.Null);
        Assert.That(runEndedView, Is.SameAs(fixture.RunEndedView));
        Assert.That(runSessionBestDistanceTracker, Is.Not.Null);
        Assert.That(runEndedResultStatsBuilder, Is.Not.Null);
        Assert.That(pullHintView, Is.SameAs(fixture.PullHintView));
        Assert.That(pullHintTuning, Is.SameAs(fixture.PullHintView));
        Assert.That(upgradePreviewBuilder, Is.Not.Null);
        Assert.That(upgradePreviewService, Is.Not.Null);
        Assert.That(upgradePurchaseService, Is.Not.Null);
        Assert.That(levelPickupStateService, Is.SameAs(levelPickupState));
        Assert.That(levelPickupState.IsAvailable(fixture.LevelPickup), Is.True);
        Assert.That(pickupCollectionNotifier, Is.Not.Null);
        Assert.That(runCameraAnchor, Is.SameAs(fixture.RunCameraAnchor));
        Assert.That(runCameraLens, Is.Not.Null);
        Assert.That(runCameraLens.Position, Is.EqualTo(fixture.InputCamera.transform.position));
        Assert.That(runCameraRig, Is.SameAs(fixture.RunCameraRig));
        Assert.That(bandShapeProvider, Is.Not.Null);
        Assert.That(presentationView, Is.SameAs(fixture.CharacterPresentationView));
        Assert.That(presentationTuning, Is.SameAs(fixture.CharacterPresentationView));
        Assert.That(characterVisualTargetPoseSource, Is.Not.Null);
        Assert.That(characterVisualTargetPoseSource.CurrentPose.Position, Is.EqualTo(fixture.LaunchTarget.transform.position));

        Assert.That(Quaternion.Angle(characterVisualTargetPoseSource.CurrentPose.Rotation, fixture.LaunchTarget.transform.rotation),
            Is.EqualTo(0f).Within(0.0001f));
        Assert.That(characterVisualFollowView, Is.SameAs(fixture.CharacterPresentationView));
        Assert.That(characterVisualFollowTuning, Is.SameAs(fixture.CharacterPresentationView));
        Assert.That(characterVisualFollowTuning.VisualPositionResponseRate, Is.EqualTo(60f).Within(0.0001f));
        Assert.That(characterVisualFollowTuning.VisualHeadingResponseRate, Is.EqualTo(45f).Within(0.0001f));
        Assert.That(characterVisualFollowTuning.VisualUpTiltResponseRate, Is.EqualTo(18f).Within(0.0001f));
        Assert.That(characterVisualFollowTuning.VisualMaxPositionLag, Is.EqualTo(0.06f).Within(0.0001f));
        Assert.That(characterVisualFollowTuning.VisualSnapDistance, Is.EqualTo(0.75f).Within(0.0001f));
        Assert.That(characterVisualFollowTuning.VisualSnapAngleDegrees, Is.EqualTo(45f).Within(0.0001f));
        Assert.That(characterVisualPoseSmoother, Is.Not.Null);
        Assert.That(presentationClassifier, Is.Not.Null);
        Assert.That(finishPresentationView, Is.SameAs(fixture.FinishPresentationView));
        Assert.That(presentationSupportTracker, Is.Not.Null);
        Assert.That(animatedContactSensorPoseSyncView, Is.SameAs(fixture.AnimatedContactSensorPoseSyncView));
        Assert.That(pickupContactSource, Is.SameAs(fixture.PickupSensorSource));
        Assert.That(characterVisualFollowerLateTickableIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(animatedContactSensorPoseSyncLateTickableIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(characterVisualFollowerLateTickableIndex, Is.LessThan(animatedContactSensorPoseSyncLateTickableIndex));
        Assert.That(resolvedRunPreparationState, Is.SameAs(fixture.RunPreparationStateId));
        Assert.That(resolvedPreLaunchState, Is.SameAs(fixture.PreLaunchStateId));
        Assert.That(resolvedRunningState, Is.SameAs(fixture.RunningStateId));
        Assert.That(resolvedRunEndedState, Is.SameAs(fixture.RunEndedStateId));
        Assert.That(resolvedCoinCurrencyDefinition, Is.SameAs(fixture.CoinCurrencyDefinition));
        Assert.That(resolvedCoinPickupMultiplierStat, Is.SameAs(fixture.CoinPickupMultiplierStatId));
        Assert.That(resolvedSlingshotLaunchPowerStat, Is.SameAs(fixture.SlingshotLaunchPowerStatId));
        Assert.That(resolvedPlayerMaxSpeedStat, Is.SameAs(fixture.PlayerMaxSpeedStatId));
        Assert.That(resolvedPlayerSteeringResponsivenessStat, Is.SameAs(fixture.PlayerSteeringResponsivenessStatId));
        Assert.That(resolvedLevelPickups, Is.SameAs(fixture.LevelPickups));
        Assert.That(((LevelPickupState)levelPickupState).PickupsForTests, Is.EquivalentTo(fixture.LevelPickups));
    }

    private ValidScopeFixture CreateValidScopeFixture()
    {
        var scope = CreateGameObject("Gameplay Lifetime Scope Test").AddComponent<GameplayLifetimeScope>();
        var runPreparation = CreateStateId("Run Preparation");
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var runEnded = CreateStateId("Run Ended");
        var runPreparationToPreLaunch = CreateTransition(runPreparation, preLaunch);
        var preLaunchToRunning = CreateTransition(preLaunch, running);
        var runningToRunEnded = CreateTransition(running, runEnded);
        var runEndedToRunPreparation = CreateTransition(runEnded, runPreparation);

        var gameplayStateConfig = CreateGameplayStateConfig(runPreparation, runPreparationToPreLaunch, preLaunchToRunning, runningToRunEnded,
            runEndedToRunPreparation);
        var slingshotLaunchPowerStatId = CreateStatId("slingshot_launch_power");
        var playerMaxSpeedStatId = CreateStatId("player_max_speed");
        var playerSteeringResponsivenessStatId = CreateStatId("player_steering_responsiveness");
        var coinPickupMultiplierStatId = CreateStatId("coin_pickup_multiplier");
        var slingshotConfig = Track(ScriptableObject.CreateInstance<SlingshotConfig>());
        var gameplaySlingshotLaunchConfig = Track(ScriptableObject.CreateInstance<GameplaySlingshotLaunchConfig>());
        var runBodyMovementConfig = Track(ScriptableObject.CreateInstance<RunBodyMovementConfig>());
        var runCameraConfig = Track(ScriptableObject.CreateInstance<RunCameraConfig>());
        var runEndConfig = Track(ScriptableObject.CreateInstance<RunEndConfig>());
        var camera = CreateGameObject("Gameplay Camera").AddComponent<Camera>();
        var slingshotView = CreateSlingshotView(slingshotConfig);
        var pullHintView = CreateGameObject("Pull Hint View").AddComponent<PullHintView>();
        var runSteeringAffordanceView = CreateRunSteeringAffordanceView();
        var runPreparationView = CreateRunPreparationView();
        var runEndedView = CreateRunEndedView();
        var launchTarget = CreateLaunchTarget(out var runBodyMovementTarget, out var runCameraSource, out var contactNotifier);
        var slingshotRig = CreateGameObject("Slingshot Rig").transform;
        var preLaunchSlingshotRigPose = CreateGameObject("Pre-Launch Slingshot Rig Pose").transform;
        var preLaunchLaunchTargetPose = CreateGameObject("Pre-Launch Launch Target Pose").transform;
        var runProgressFrameSource = CreateGameObject("Run Progress Frame Source").AddComponent<RunProgressFrameSource>();
        var runCameraAnchor = CreateGameObject("Run Camera Anchor").AddComponent<TransformRunCameraAnchor>();
        var runCameraRig = CreateGameObject("Run Camera Rig").AddComponent<CinemachineRunCameraRig>();
        var runPreparationCamera = CreateGameObject("Run Preparation Camera").AddComponent<CinemachineCamera>();
        var preLaunchCamera = CreateGameObject("Pre-Launch Camera").AddComponent<CinemachineCamera>();
        var runCamera = CreateGameObject("Run Camera").AddComponent<CinemachineCamera>();
        runCameraRig.SetReferencesForTests(runPreparationCamera, preLaunchCamera, runCamera);
        var characterPresentationView = CreateGameObject("Character Presentation View").AddComponent<CharacterPresentationView>();
        var animatedContactSensorPoseSyncView = CreateAnimatedContactSensorPoseSyncView(characterPresentationView.transform);
        var finishPresentationView = CreateGameObject("Finish Presentation View").AddComponent<FinishPresentationView>();
        var runBodyContactCollider = launchTarget.GetComponent<Collider>();
        runBodyContactCollider.gameObject.layer = GetRequiredLayer("Player");
        runBodyContactCollider.gameObject.tag = "Player";
        var runSurfaceInstaller = CreateGameObject("Run Surface Installer").AddComponent<GameplayPhysicsSceneCompositionMonoInstaller>();
        var runSurfaceMask = new LayerMask { value = Physics.DefaultRaycastLayers };
        runSurfaceInstaller.SetReferencesForTests(runBodyContactCollider, supportProbeDistance: 0.08f, runSurfaceMask);
        var currencyDefinition = CreateCurrencyDefinition("Coins", "currency-coins");
        var upgradeCatalog = Track(ScriptableObject.CreateInstance<UpgradeCatalog>());
        upgradeCatalog.SetValuesForTests(currencyDefinition, Array.Empty<UpgradeDefinition>());
        var pickupDefinition = CreatePickupDefinition(currencyDefinition, 1);
        var levelPickup = CreatePickup("Level Pickup", pickupDefinition);
        var levelPickups = new[] { levelPickup };
        var pickupSensorSource = CreatePickupSensorSource();
        var pickupsInstaller = CreateGameObject("Pickups Installer").AddComponent<GameplayPickupsSceneCompositionMonoInstaller>();
        pickupsInstaller.SetReferencesForTests(levelPickups, pickupSensorSource, "Pickup", "PlayerBodyPart");

        scope.SetReferencesForTests(gameplayStateConfig, runPreparation, preLaunch, running, runEnded, upgradeCatalog, slingshotLaunchPowerStatId,
            playerMaxSpeedStatId, playerSteeringResponsivenessStatId, currencyDefinition, coinPickupMultiplierStatId, slingshotConfig,
            gameplaySlingshotLaunchConfig, runBodyMovementConfig, runCameraConfig, runEndConfig, runBodyMovementTarget, runCameraSource,
            runProgressFrameSource, sceneCompositionInstallers: new BaseSceneCompositionMonoInstaller[] { runSurfaceInstaller, pickupsInstaller },
            contactNotifier, runCameraAnchor, runCameraRig, camera, slingshotRig, preLaunchSlingshotRigPose, preLaunchLaunchTargetPose,
            slingshotView, pullHintView, runSteeringAffordanceView, runPreparationView, runEndedView, launchTarget, characterPresentationView,
            animatedContactSensorPoseSyncView, finishPresentationView);

        return new ValidScopeFixture
        {
            Scope = scope,
            RunPreparationStateId = runPreparation,
            PreLaunchStateId = preLaunch,
            RunningStateId = running,
            RunEndedStateId = runEnded,
            SlingshotLaunchPowerStatId = slingshotLaunchPowerStatId,
            PlayerMaxSpeedStatId = playerMaxSpeedStatId,
            PlayerSteeringResponsivenessStatId = playerSteeringResponsivenessStatId,
            CoinCurrencyDefinition = currencyDefinition,
            CoinPickupMultiplierStatId = coinPickupMultiplierStatId,
            UpgradeCatalog = upgradeCatalog,
            GameplaySlingshotLaunchConfig = gameplaySlingshotLaunchConfig,
            RunBodyMovementConfig = runBodyMovementConfig,
            LaunchTarget = launchTarget,
            LevelPickup = levelPickup,
            LevelPickups = levelPickups,
            PickupsInstaller = pickupsInstaller,
            PickupSensorSource = pickupSensorSource,
            RunBodyMovementTarget = runBodyMovementTarget,
            RunCameraConfig = runCameraConfig,
            RunEndConfig = runEndConfig,
            RunCameraSource = runCameraSource,
            RunProgressFrameSource = runProgressFrameSource,
            RunSurfaceInstaller = runSurfaceInstaller,
            ContactNotifier = contactNotifier,
            RunCameraAnchor = runCameraAnchor,
            InputCamera = camera,
            RunCameraRig = runCameraRig,
            PullHintView = pullHintView,
            RunSteeringAffordanceView = runSteeringAffordanceView,
            RunEndedView = runEndedView,
            CharacterPresentationView = characterPresentationView,
            AnimatedContactSensorPoseSyncView = animatedContactSensorPoseSyncView,
            FinishPresentationView = finishPresentationView
        };
    }

    private SlingshotView CreateSlingshotView(SlingshotConfig config)
    {
        var view = CreateGameObject("Slingshot View").AddComponent<SlingshotView>();
        var leftAnchor = CreateGameObject("Left Anchor").transform;
        var rightAnchor = CreateGameObject("Right Anchor").transform;
        var restPoint = CreateGameObject("Rest Point").transform;
        var launchFrame = CreateGameObject("Launch Frame").transform;
        var bandLineRenderer = CreateGameObject("Band").AddComponent<LineRenderer>();
        var touchIndicatorObject = CreateGameObject("Touch Indicator");

        leftAnchor.position = new Vector3(-1f, 1f, 0f);
        rightAnchor.position = new Vector3(1f, 1f, 0f);
        restPoint.position = new Vector3(0f, 1f, 0f);
        launchFrame.position = restPoint.position;
        launchFrame.rotation = Quaternion.identity;

        view.SetReferencesForTests(leftAnchor, rightAnchor, restPoint, launchFrame, bandLineRenderer, touchIndicatorObject, config);
        return view;
    }

    private RunSteeringAffordanceView CreateRunSteeringAffordanceView()
    {
        var canvasObject = Track(new GameObject("Run Steering Affordance Canvas", typeof(RectTransform), typeof(Canvas)));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var rootObject = Track(new GameObject("Run Steering Affordance", typeof(RectTransform)));
        rootObject.SetActive(false);
        rootObject.transform.SetParent(canvasObject.transform, false);
        var root = rootObject.GetComponent<RectTransform>();
        var canvasGroup = rootObject.AddComponent<CanvasGroup>();
        var deadzoneRoot = CreateChildImage(root, "Deadzone Hint");
        var leftRangeEndRoot = CreateChildImage(root, "Left Range End Hint");
        var rightRangeEndRoot = CreateChildImage(root, "Right Range End Hint");
        var knobRoot = CreateChildImage(root, "Knob");
        var view = rootObject.AddComponent<RunSteeringAffordanceView>();

        view.SetReferencesForTests(root, canvasGroup, knobRoot.rectTransform, knobImage: knobRoot, leftRangeEndRoot.rectTransform,
            leftRangeEndRoot, rightRangeEndRoot.rectTransform, rightRangeEndRoot, deadzoneRoot.rectTransform, deadzoneImage: deadzoneRoot);

        rootObject.SetActive(true);
        return view;
    }

    private AnimatedContactSensorPoseSyncView CreateAnimatedContactSensorPoseSyncView(Transform sourceTransform)
    {
        var rootRigidbody = CreateGameObject("Animated Contact Sensor Physics Root").AddComponent<Rigidbody>();
        rootRigidbody.isKinematic = true;
        rootRigidbody.useGravity = false;

        var target = CreateGameObject("Body Sensor").transform;
        target.SetParent(rootRigidbody.transform, false);

        var view = rootRigidbody.gameObject.AddComponent<AnimatedContactSensorPoseSyncView>();
        view.SetReferencesForTests(rootRigidbody, new[] { new AnimatedContactSensorPoseBinding(sourceTransform, target) });

        return view;
    }

    private PickupSensorSource CreatePickupSensorSource()
    {
        var source = CreateGameObject("Pickup Sensor Source").AddComponent<PickupSensorSource>();
        var sensorObject = CreateGameObject("Body Sensor");
        sensorObject.transform.SetParent(source.transform, false);
        sensorObject.layer = GetRequiredLayer("PlayerBodyPart");
        var sensorCollider = sensorObject.AddComponent<SphereCollider>();
        sensorCollider.isTrigger = true;
        var sensor = sensorObject.AddComponent<TriggerNotifier>();
        source.SetSensorEntriesForTests(sensor);

        return source;
    }

    private static int GetFixedTickableIndex(IReadOnlyList<IFixedTickable> fixedTickables, Predicate<IFixedTickable> predicate)
    {
        for (var fixedTickableIndex = 0; fixedTickableIndex < fixedTickables.Count; fixedTickableIndex += 1)
        {
            if (predicate(fixedTickables[fixedTickableIndex]))
                return fixedTickableIndex;
        }

        return -1;
    }

    private static int GetLateTickableIndex(IReadOnlyList<ILateTickable> lateTickables, Predicate<ILateTickable> predicate)
    {
        for (var lateTickableIndex = 0; lateTickableIndex < lateTickables.Count; lateTickableIndex += 1)
        {
            if (predicate(lateTickables[lateTickableIndex]))
                return lateTickableIndex;
        }

        return -1;
    }

    private RunPreparationUIView CreateRunPreparationView()
    {
        var viewObject = CreateGameObject("Run Preparation View");
        viewObject.SetActive(false);
        var view = viewObject.AddComponent<RunPreparationUIView>();
        var coinBalanceIcon = CreateChildImage(viewObject.transform, "Coin Balance Icon");
        var coinBalanceText = CreateChildText(viewObject.transform, "Coin Balance Label");
        var continueTouchAreaButton = CreateChildButton(viewObject.transform, "Run Preparation Continue Touch Area");
        var upgradeCard = CreateRunPreparationUpgradeCard(viewObject.transform);
        view.SetReferencesForTests(viewObject, coinBalanceIcon, coinBalanceText, continueTouchAreaButton, new[] { upgradeCard });
        viewObject.SetActive(true);
        return view;
    }

    private RunEndedUIView CreateRunEndedView()
    {
        var viewObject = CreateGameObject("Run Ended View");
        viewObject.SetActive(false);
        var view = viewObject.AddComponent<RunEndedUIView>();
        var titleText = CreateChildText(viewObject.transform, "Run Ended Title");
        var earnedCoinsIcon = CreateChildImage(viewObject.transform, "Icon");
        var earnedCoinsText = CreateChildText(viewObject.transform, "RunTotalLabel");
        var reachedDistanceText = CreateChildText(viewObject.transform, "ReachedDistanceLabel");
        var rewardSourceRowsRoot = CreateChildGameObject(viewObject.transform, "RewardSourceContainer");
        var rewardSourceRowPrefab = CreateRunEndedRewardSourceRowTemplate(rewardSourceRowsRoot.transform);
        var bestImprovementRoot = CreateChildGameObject(viewObject.transform, "Run Ended Best Improvement");
        var bestImprovementText = CreateChildText(bestImprovementRoot.transform, "BestImprovementLabel");
        var tapToContinueRoot = CreateChildGameObject(viewObject.transform, "Run Ended Continue Label");
        var acknowledgeTouchAreaButton = CreateChildButton(viewObject.transform, "Run Ended Continue Touch Area");

        view.SetReferencesForTests(root: viewObject, titleText, earnedCoinsText,
            earnedCoinsRevealGraphics: new Graphic[] { earnedCoinsIcon, earnedCoinsText }, reachedDistanceText, rewardSourceRowsRoot.transform,
            rewardSourceRowPrefab, bestImprovementRoot, bestImprovementText, tapToContinueRoot, acknowledgeTouchAreaButton);
        viewObject.SetActive(true);
        return view;
    }

    private RunEndedRewardSourceRowUIView CreateRunEndedRewardSourceRowTemplate(Transform parent)
    {
        var rowObject = CreateChildGameObject(parent, "RowTemplate");
        var row = rowObject.AddComponent<RunEndedRewardSourceRowUIView>();
        var labelText = CreateChildText(rowObject.transform, "Label");
        var currencyIcon = CreateChildImage(rowObject.transform, "CurrencyIcon");
        var amountText = CreateChildText(rowObject.transform, "Amount");
        amountText.color = Color.white;
        row.SetReferencesForTests(labelText, amountText, labelText, currencyIcon, amountText);
        rowObject.SetActive(false);
        return row;
    }

    private RunPreparationUpgradeCardView CreateRunPreparationUpgradeCard(Transform parent)
    {
        var cardObject = CreateChildGameObject(parent, "Run Preparation Upgrade Card");
        var card = cardObject.AddComponent<RunPreparationUpgradeCardView>();
        var icon = CreateChildImage(cardObject.transform, "Upgrade Icon");
        var nameText = CreateChildText(cardObject.transform, "Upgrade Name Label");
        var levelText = CreateChildText(cardObject.transform, "Upgrade Level Label");
        var effectText = CreateChildText(cardObject.transform, "Upgrade Effect Label");
        var buyButton = CreateChildButton(cardObject.transform, "Buy Button");
        var buyButtonActionLabel = CreateChildText(buyButton.transform, "Upgrade Button Action Label");
        var buyButtonCostIcon = CreateChildImage(buyButton.transform, "Upgrade Button Cost Currency Icon");
        var buyButtonCostText = CreateChildText(buyButton.transform, "Upgrade Button Cost Label");

        card.SetReferencesForTests(cardObject, icon, nameText, levelText, effectText, buyButton, buyButtonActionLabel, buyButtonCostIcon,
            buyButtonCostText);

        return card;
    }

    private PickupDefinition CreatePickupDefinition(CurrencyDefinition currencyDefinition, int amount)
    {
        var definition = Track(ScriptableObject.CreateInstance<PickupDefinition>());
        definition.SetValuesForTests(currencyDefinition, amount);
        return definition;
    }

    private Pickup CreatePickup(string objectName, PickupDefinition definition)
    {
        var pickup = CreateGameObject(objectName).AddComponent<Pickup>();
        pickup.gameObject.layer = GetRequiredLayer("Pickup");
        pickup.SetDefinitionForTests(definition);
        var triggerObject = CreateGameObject($"{objectName} Trigger");
        triggerObject.transform.SetParent(pickup.transform, false);
        triggerObject.layer = GetRequiredLayer("Pickup");
        var notifier = triggerObject.AddComponent<TriggerNotifier>();
        var collider = triggerObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        pickup.SetTriggerNotifierForTests(notifier);
        return pickup;
    }

    private CurrencyDefinition CreateCurrencyDefinition(string definitionName, string saveId)
    {
        var definition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        definition.name = definitionName;
        definition.SetSaveIdForTests(saveId);
        return definition;
    }

    private CurrencyDefinition CreateCurrencyDefinitionAsset(string definitionName, string saveId)
    {
        var assetDirectory = "Assets/__GameplayLifetimeScopeCurrencyTests_" + Guid.NewGuid().ToString("N");
        _assetDirectories.Add(assetDirectory);
        Directory.CreateDirectory(assetDirectory);
        AssetDatabase.Refresh();

        var definition = ScriptableObject.CreateInstance<CurrencyDefinition>();
        definition.name = definitionName;
        var assetPath = Path.Combine(assetDirectory, definitionName + ".asset").Replace('\\', '/');
        AssetDatabase.CreateAsset(definition, assetPath);
        AssetDatabase.Refresh();

        var loadedDefinition = (CurrencyDefinition)AssetDatabase.LoadAssetAtPath(assetPath, typeof(CurrencyDefinition));
        loadedDefinition.SetSaveIdForTests(saveId);
        EditorUtility.SetDirty(loadedDefinition);
        return loadedDefinition;
    }

    private RigidbodyLaunchTarget CreateLaunchTarget(
        out RigidbodyRunBodyMovementTarget runBodyMovementTarget,
        out RigidbodyRunCameraSource runCameraSource,
        out RigidbodyContactNotifier contactNotifier)
    {
        var rigidbody = CreateGameObject("Player").AddComponent<Rigidbody>();
        var collider = rigidbody.gameObject.AddComponent<SphereCollider>();
        var bandCenter = CreateGameObject("Band Center").transform;
        bandCenter.SetParent(rigidbody.transform, false);
        var launchTarget = rigidbody.gameObject.AddComponent<RigidbodyLaunchTarget>();
        launchTarget.SetReferencesForTests(rigidbody, collider, bandCenter);
        runBodyMovementTarget = rigidbody.gameObject.AddComponent<RigidbodyRunBodyMovementTarget>();
        runBodyMovementTarget.SetRigidbodyForTests(rigidbody);
        runCameraSource = rigidbody.gameObject.AddComponent<RigidbodyRunCameraSource>();
        runCameraSource.SetRigidbodyForTests(rigidbody);
        contactNotifier = rigidbody.gameObject.AddComponent<RigidbodyContactNotifier>();
        return launchTarget;
    }

    private int GetRequiredLayer(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);
        Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Unity layer '{layerName}' must exist for gameplay tests.");
        return layer;
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
        stateId.name = stateName;
        return stateId;
    }

    private GameplayStatId CreateStatId(string id)
    {
        var statId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
        statId.SetValuesForTests(id);
        return statId;
    }

    private GameplayStateTransition CreateTransition(GameplayStateId fromStateId, GameplayStateId toStateId)
    {
        var transition = Track(ScriptableObject.CreateInstance<GameplayStateTransition>());
        transition.SetStateIdsForTests(fromStateId, toStateId);
        return transition;
    }

    private GameplayStateConfig CreateGameplayStateConfig(
        GameplayStateId initialStateId,
        params GameplayStateTransition[] transitions)
    {
        var config = Track(ScriptableObject.CreateInstance<GameplayStateConfig>());
        config.SetValuesForTests(initialStateId, transitions);
        return config;
    }

    private GameObject CreateGameObject(string objectName)
    {
        return Track(new GameObject(objectName));
    }

    private GameObject CreateChildGameObject(Transform parent, string objectName)
    {
        var gameObject = Track(new GameObject(objectName, typeof(RectTransform)));
        gameObject.transform.SetParent(parent, false);

        return gameObject;
    }

    private Image CreateChildImage(Transform parent, string objectName)
    {
        return CreateChildGameObject(parent, objectName).AddComponent<Image>();
    }

    private TMP_Text CreateChildText(Transform parent, string objectName)
    {
        return CreateChildGameObject(parent, objectName).AddComponent<TextMeshProUGUI>();
    }

    private Button CreateChildButton(Transform parent, string objectName)
    {
        var buttonObject = CreateChildGameObject(parent, objectName);
        var targetGraphic = buttonObject.AddComponent<Image>();
        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = targetGraphic;

        return button;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private void DestroyGeneratedGameObjects<T>()
        where T : Component
    {
        var components = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var component in components)
        {
            if (component != null)
                UnityEngine.Object.DestroyImmediate(component.gameObject);
        }
    }

    private sealed class ValidScopeFixture
    {
        public GameplayLifetimeScope Scope { get; set; }
        public GameplayStateId RunPreparationStateId { get; set; }
        public GameplayStateId PreLaunchStateId { get; set; }
        public GameplayStateId RunningStateId { get; set; }
        public GameplayStateId RunEndedStateId { get; set; }
        public GameplayStatId SlingshotLaunchPowerStatId { get; set; }
        public GameplayStatId PlayerMaxSpeedStatId { get; set; }
        public GameplayStatId PlayerSteeringResponsivenessStatId { get; set; }
        public CurrencyDefinition CoinCurrencyDefinition { get; set; }
        public GameplayStatId CoinPickupMultiplierStatId { get; set; }
        public UpgradeCatalog UpgradeCatalog { get; set; }
        public GameplaySlingshotLaunchConfig GameplaySlingshotLaunchConfig { get; set; }
        public RunBodyMovementConfig RunBodyMovementConfig { get; set; }
        public RigidbodyLaunchTarget LaunchTarget { get; set; }
        public Pickup LevelPickup { get; set; }
        public IReadOnlyList<Pickup> LevelPickups { get; set; }
        public GameplayPickupsSceneCompositionMonoInstaller PickupsInstaller { get; set; }
        public PickupSensorSource PickupSensorSource { get; set; }
        public RigidbodyRunBodyMovementTarget RunBodyMovementTarget { get; set; }
        public RunCameraConfig RunCameraConfig { get; set; }
        public RunEndConfig RunEndConfig { get; set; }
        public RigidbodyRunCameraSource RunCameraSource { get; set; }
        public RunProgressFrameSource RunProgressFrameSource { get; set; }
        public GameplayPhysicsSceneCompositionMonoInstaller RunSurfaceInstaller { get; set; }
        public RigidbodyContactNotifier ContactNotifier { get; set; }
        public TransformRunCameraAnchor RunCameraAnchor { get; set; }
        public Camera InputCamera { get; set; }
        public CinemachineRunCameraRig RunCameraRig { get; set; }
        public PullHintView PullHintView { get; set; }
        public RunSteeringAffordanceView RunSteeringAffordanceView { get; set; }
        public RunEndedUIView RunEndedView { get; set; }
        public CharacterPresentationView CharacterPresentationView { get; set; }
        public AnimatedContactSensorPoseSyncView AnimatedContactSensorPoseSyncView { get; set; }
        public FinishPresentationView FinishPresentationView { get; set; }
    }
}
