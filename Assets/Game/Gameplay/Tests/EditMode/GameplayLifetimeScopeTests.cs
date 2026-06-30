using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Economy;
using Game.Gameplay.Pickups;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using Game.Foundation.Input;
using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using TMPro;
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

    [TearDown]
    public void OnTearDown()
    {
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
                .And.Message.Contains("Player Steering Config")
                .And.Message.Contains("Run Camera Config")
                .And.Message.Contains("Run End Config")
                .And.Message.Contains("Player Steering Target")
                .And.Message.Contains("Run Camera Source")
                .And.Message.Contains("Run Progress Frame Source")
                .And.Message.Contains("Run Surface Context Source")
                .And.Message.Contains("Rigidbody Contact Notifier")
                .And.Message.Contains("Run Camera Anchor")
                .And.Message.Contains("Run Camera Rig")
                .And.Message.Contains("Input Camera")
                .And.Message.Contains("Slingshot Rig Transform")
                .And.Message.Contains("Pre-Launch Slingshot Rig Pose")
                .And.Message.Contains("Pre-Launch Launch Target Pose")
                .And.Message.Contains("Slingshot View")
                .And.Message.Contains("Pull Hint View")
                .And.Message.Contains("Run Preparation View")
                .And.Message.Contains("Launch Target")
                .And.Message.Contains("Character Presentation View")
                .And.Message.Contains("Level Pickup")
                .And.Message.Contains("Player Pickup Contact Collider"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_AllReferencesAssigned_DoesNotThrow()
    {
        var fixture = CreateValidScopeFixture();

        Assert.That(fixture.Scope.ValidateRequiredReferencesForTests, Throws.Nothing);
    }

    [Test]
    public void ValidateRequiredReferencesForTests_DuplicatePickupReference_ThrowsWithDuplicatePickupMessage()
    {
        var fixture = CreateValidScopeFixture();

        fixture.Scope.SetPickupReferencesForTests(
            new[] { fixture.LevelPickup, fixture.LevelPickup },
            new[] { fixture.PlayerPickupContactCollider },
            "Player",
            "Player",
            "Pickup");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("duplicate Level Pickup"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_EmptyPlayerTag_ThrowsWithPlayerTagMessage()
    {
        var fixture = CreateValidScopeFixture();

        fixture.Scope.SetPickupReferencesForTests(
            new[] { fixture.LevelPickup },
            new[] { fixture.PlayerPickupContactCollider },
            string.Empty,
            "Player",
            "Pickup");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Player Tag"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_MissingPickupDefinition_ThrowsWithPickupDefinitionMessage()
    {
        var fixture = CreateValidScopeFixture();
        var invalidPickup = CreatePickup("Invalid Pickup", null);

        fixture.Scope.SetPickupReferencesForTests(
            new[] { invalidPickup },
            new[] { fixture.PlayerPickupContactCollider },
            "Player",
            "Player",
            "Pickup");

        Assert.That(
            fixture.Scope.ValidateRequiredReferencesForTests,
            Throws.TypeOf<InvalidOperationException>().With.Message.Contains("Pickup Definition"));
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
        var preLaunchRigPoseResetter = container.Resolve<IPreLaunchRigPoseResetter>();
        var steeringTarget = container.Resolve<IPlayerSteeringTarget>();
        var steeringConfig = container.Resolve<IPlayerSteeringConfig>();
        var runCameraConfig = container.Resolve<IRunCameraConfig>();
        var runEndConfig = container.Resolve<IRunEndConfig>();
        var runCameraSource = container.Resolve<IRunCameraSource>();
        var runMotionSource = container.Resolve<IRunMotionSource>();
        var runProgressService = container.Resolve<IRunProgressService>();
        var runProgressFrameSource = container.Resolve<IRunProgressFrameSource>();
        var runSurfaceContextSource = container.Resolve<IRunSurfaceContextSource>();
        var contactNotifier = container.Resolve<IRigidbodyContactNotifier>();
        var contactClassifier = container.Resolve<IRunContactClassifier>();
        var runEndCandidateReceiver = container.Resolve<IRunEndCandidateReceiver>();
        var runResultNotifier = container.Resolve<IRunResultNotifier>();
        var playerEconomyState = container.Resolve<PlayerEconomyState>();
        var currencyStorage = container.Resolve<ICurrencyStorage>();
        var economyContentIndex = container.Resolve<IPlayerEconomyContentIndex>();
        var economySaveRepository = container.Resolve<IEconomySaveRepository>();
        var economyCommitter = container.Resolve<IEconomyCommitter>();
        var runCurrencyAccumulator = container.Resolve<IRunCurrencyAccumulator>();
        var upgradeCatalog = container.Resolve<IUpgradeCatalog>();
        var upgradeProgressStorage = container.Resolve<IUpgradeProgressStorage>();
        var runModifierSnapshotFactory = container.Resolve<IRunModifierSnapshotFactory>();
        var runModifierSnapshotProvider = container.Resolve<IRunModifierSnapshotProvider>();
        var runModifierSnapshotStore = container.Resolve<IRunModifierSnapshotStore>();
        var runGameplayStatResolver = container.Resolve<IRunGameplayStatResolver>();
        var pickupCurrencyGrantResolver = container.Resolve<IPickupCurrencyGrantResolver>();
        var runPreparationView = container.Resolve<IRunPreparationView>();
        var pullHintView = container.Resolve<IPullHintView>();
        var pullHintTuning = container.Resolve<IPullHintTuning>();
        var upgradePreviewBuilder = container.Resolve<UpgradePreviewBuilder>();
        var upgradePreviewService = container.Resolve<UpgradePreviewService>();
        var upgradePurchaseService = container.Resolve<UpgradePurchaseService>();
        var levelPickupStateService = container.Resolve<ILevelPickupState>();
        var levelPickupState = container.Resolve<ILevelPickupState>();
        var pickupCollectionNotifier = container.Resolve<IPickupCollectionNotifier>();
        var runCameraAnchor = container.Resolve<IRunCameraAnchor>();
        var runCameraRig = container.Resolve<IRunCameraRig>();
        var bandShapeProvider = container.Resolve<ISlingshotBandShapeProvider>();
        var presentationView = container.Resolve<ICharacterPresentationView>();
        var presentationTuning = container.Resolve<ICharacterPresentationTuning>();
        var presentationClassifier = container.Resolve<ICharacterPresentationModeClassifier>();
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
        var resolvedPlayerTag = container.Resolve<string>(InjectKey.Tags.Player);

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
        Assert.That(initializables.Count, Is.EqualTo(15));
        Assert.That(tickables.Count, Is.EqualTo(4));
        Assert.That(fixedTickables.Count, Is.EqualTo(4));
        Assert.That(lateTickables.Count, Is.EqualTo(1));
        Assert.That(launchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(heldLaunchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(silhouetteSource, Is.SameAs(fixture.LaunchTarget));
        Assert.That(launchTargetPreLaunchReset, Is.SameAs(fixture.LaunchTarget));
        Assert.That(preLaunchRigPoseResetter, Is.Not.Null);
        Assert.That(steeringTarget, Is.SameAs(fixture.PlayerSteeringTarget));
        Assert.That(steeringTarget, Is.Not.SameAs(fixture.LaunchTarget));
        Assert.That(steeringConfig, Is.Not.Null);
        Assert.That(runCameraConfig, Is.SameAs(fixture.RunCameraConfig));
        Assert.That(runEndConfig, Is.SameAs(fixture.RunEndConfig));
        Assert.That(runCameraSource, Is.SameAs(fixture.RunCameraSource));
        Assert.That(runMotionSource, Is.SameAs(fixture.RunCameraSource));
        Assert.That(runProgressService, Is.Not.Null);
        Assert.That(runProgressFrameSource, Is.SameAs(fixture.RunProgressFrameSource));
        Assert.That(runSurfaceContextSource, Is.SameAs(fixture.RunSurfaceContextSource));
        Assert.That(contactNotifier, Is.SameAs(fixture.ContactNotifier));
        Assert.That(contactClassifier, Is.Not.Null);
        Assert.That(runEndCandidateReceiver, Is.Not.Null);
        Assert.That(runResultNotifier, Is.Not.Null);
        Assert.That(playerEconomyState, Is.Not.Null);
        Assert.That(currencyStorage, Is.Not.Null);
        Assert.That(economyContentIndex.IsKnownCurrencyId(fixture.CoinCurrencyDefinition.SaveId), Is.True);
        Assert.That(economySaveRepository, Is.Not.Null);
        Assert.That(economyCommitter, Is.Not.Null);
        Assert.That(runCurrencyAccumulator, Is.Not.Null);
        Assert.That(upgradeCatalog, Is.SameAs(fixture.UpgradeCatalog));
        Assert.That(upgradeProgressStorage, Is.Not.Null);
        Assert.That(runModifierSnapshotFactory, Is.Not.Null);
        Assert.That(runModifierSnapshotProvider, Is.SameAs(runModifierSnapshotStore));
        Assert.That(runGameplayStatResolver, Is.Not.Null);
        Assert.That(pickupCurrencyGrantResolver, Is.Not.Null);
        Assert.That(runPreparationView, Is.Not.Null);
        Assert.That(pullHintView, Is.SameAs(fixture.PullHintView));
        Assert.That(pullHintTuning, Is.SameAs(fixture.PullHintView));
        Assert.That(upgradePreviewBuilder, Is.Not.Null);
        Assert.That(upgradePreviewService, Is.Not.Null);
        Assert.That(upgradePurchaseService, Is.Not.Null);
        Assert.That(levelPickupStateService, Is.SameAs(levelPickupState));
        Assert.That(levelPickupState.IsAvailable(fixture.LevelPickup), Is.True);
        Assert.That(pickupCollectionNotifier, Is.Not.Null);
        Assert.That(runCameraAnchor, Is.SameAs(fixture.RunCameraAnchor));
        Assert.That(runCameraRig, Is.SameAs(fixture.RunCameraRig));
        Assert.That(bandShapeProvider, Is.Not.Null);
        Assert.That(presentationView, Is.SameAs(fixture.CharacterPresentationView));
        Assert.That(presentationTuning, Is.SameAs(fixture.CharacterPresentationView));
        Assert.That(presentationClassifier, Is.Not.Null);
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
        Assert.That(resolvedPlayerTag, Is.EqualTo(fixture.PlayerTag));
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
        var playerSteeringConfig = Track(ScriptableObject.CreateInstance<PlayerSteeringConfig>());
        var runCameraConfig = Track(ScriptableObject.CreateInstance<RunCameraConfig>());
        var runEndConfig = Track(ScriptableObject.CreateInstance<RunEndConfig>());
        var camera = CreateGameObject("Gameplay Camera").AddComponent<Camera>();
        var slingshotView = CreateSlingshotView(slingshotConfig);
        var pullHintView = CreateGameObject("Pull Hint View").AddComponent<PullHintView>();
        var runPreparationView = CreateRunPreparationView();
        var launchTarget = CreateLaunchTarget(out var playerSteeringTarget, out var runCameraSource, out var contactNotifier);
        var slingshotRig = CreateGameObject("Slingshot Rig").transform;
        var preLaunchSlingshotRigPose = CreateGameObject("Pre-Launch Slingshot Rig Pose").transform;
        var preLaunchLaunchTargetPose = CreateGameObject("Pre-Launch Launch Target Pose").transform;
        var runProgressFrameSource = CreateGameObject("Run Progress Frame Source").AddComponent<RunProgressFrameSource>();
        var runCameraAnchor = CreateGameObject("Run Camera Anchor").AddComponent<TransformRunCameraAnchor>();
        var runCameraRig = CreateGameObject("Run Camera Rig").AddComponent<CinemachineRunCameraRig>();
        var preLaunchCamera = CreateGameObject("Pre-Launch Camera").AddComponent<CinemachineCamera>();
        var runCamera = CreateGameObject("Run Camera").AddComponent<CinemachineCamera>();
        runCameraRig.SetReferencesForTests(preLaunchCamera, runCamera);
        var runSurfaceContextSource = CreateGameObject("Run Surface Context Source").AddComponent<PhysicsRunSurfaceContextSource>();
        var characterPresentationView = CreateGameObject("Character Presentation View").AddComponent<CharacterPresentationView>();
        var playerPickupContactCollider = launchTarget.GetComponent<Collider>();
        playerPickupContactCollider.gameObject.layer = GetRequiredLayer("Player");
        playerPickupContactCollider.gameObject.tag = "Player";
        var currencyDefinition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        currencyDefinition.name = "Coins";
        currencyDefinition.SetSaveIdForTests("currency-coins");
        var upgradeCatalog = Track(ScriptableObject.CreateInstance<UpgradeCatalog>());
        upgradeCatalog.SetValuesForTests(currencyDefinition, Array.Empty<UpgradeDefinition>());
        var pickupDefinition = CreatePickupDefinition(currencyDefinition, 1);
        var levelPickup = CreatePickup("Level Pickup", pickupDefinition);
        var levelPickups = new[] { levelPickup };
        var playerPickupContactColliders = new[] { playerPickupContactCollider };
        var playerTag = "Player";

        scope.SetReferencesForTests(
            gameplayStateConfig,
            runPreparation,
            preLaunch,
            running,
            runEnded,
            upgradeCatalog,
            slingshotLaunchPowerStatId,
            playerMaxSpeedStatId,
            playerSteeringResponsivenessStatId,
            currencyDefinition,
            coinPickupMultiplierStatId,
            slingshotConfig,
            gameplaySlingshotLaunchConfig,
            playerSteeringConfig,
            runCameraConfig,
            runEndConfig,
            playerSteeringTarget,
            runCameraSource,
            runProgressFrameSource,
            runSurfaceContextSource,
            contactNotifier,
            runCameraAnchor,
            runCameraRig,
            camera,
            slingshotRig,
            preLaunchSlingshotRigPose,
            preLaunchLaunchTargetPose,
            slingshotView,
            pullHintView,
            runPreparationView,
            launchTarget,
            characterPresentationView,
            levelPickups,
            playerPickupContactColliders,
            playerTag,
            "Player",
            "Pickup");

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
            LaunchTarget = launchTarget,
            LevelPickup = levelPickup,
            LevelPickups = levelPickups,
            PlayerPickupContactCollider = playerPickupContactCollider,
            PlayerSteeringTarget = playerSteeringTarget,
            RunCameraConfig = runCameraConfig,
            RunEndConfig = runEndConfig,
            RunCameraSource = runCameraSource,
            RunProgressFrameSource = runProgressFrameSource,
            RunSurfaceContextSource = runSurfaceContextSource,
            ContactNotifier = contactNotifier,
            RunCameraAnchor = runCameraAnchor,
            RunCameraRig = runCameraRig,
            PullHintView = pullHintView,
            CharacterPresentationView = characterPresentationView,
            PlayerTag = playerTag
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
        pickup.SetDefinitionForTests(definition);
        var collider = pickup.gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.gameObject.layer = GetRequiredLayer("Pickup");
        return pickup;
    }

    private RigidbodyLaunchTarget CreateLaunchTarget(
        out RigidbodyPlayerSteeringTarget playerSteeringTarget,
        out RigidbodyRunCameraSource runCameraSource,
        out RigidbodyContactNotifier contactNotifier)
    {
        var rigidbody = CreateGameObject("Player").AddComponent<Rigidbody>();
        var collider = rigidbody.gameObject.AddComponent<SphereCollider>();
        var bandCenter = CreateGameObject("Band Center").transform;
        bandCenter.SetParent(rigidbody.transform, false);
        var launchTarget = rigidbody.gameObject.AddComponent<RigidbodyLaunchTarget>();
        launchTarget.SetReferencesForTests(rigidbody, collider, bandCenter);
        playerSteeringTarget = rigidbody.gameObject.AddComponent<RigidbodyPlayerSteeringTarget>();
        playerSteeringTarget.SetRigidbodyForTests(rigidbody);
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
        public RigidbodyLaunchTarget LaunchTarget { get; set; }
        public Pickup LevelPickup { get; set; }
        public IReadOnlyList<Pickup> LevelPickups { get; set; }
        public Collider PlayerPickupContactCollider { get; set; }
        public RigidbodyPlayerSteeringTarget PlayerSteeringTarget { get; set; }
        public RunCameraConfig RunCameraConfig { get; set; }
        public RunEndConfig RunEndConfig { get; set; }
        public RigidbodyRunCameraSource RunCameraSource { get; set; }
        public RunProgressFrameSource RunProgressFrameSource { get; set; }
        public PhysicsRunSurfaceContextSource RunSurfaceContextSource { get; set; }
        public RigidbodyContactNotifier ContactNotifier { get; set; }
        public TransformRunCameraAnchor RunCameraAnchor { get; set; }
        public CinemachineRunCameraRig RunCameraRig { get; set; }
        public PullHintView PullHintView { get; set; }
        public CharacterPresentationView CharacterPresentationView { get; set; }
        public string PlayerTag { get; set; }
    }
}
