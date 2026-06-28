using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Foundation.Input;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
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
                .And.Message.Contains("Pre-Launch State Id")
                .And.Message.Contains("Running State Id")
                .And.Message.Contains("Run Ended State Id")
                .And.Message.Contains("Slingshot Config")
                .And.Message.Contains("Player Steering Config")
                .And.Message.Contains("Run Camera Config")
                .And.Message.Contains("Run End Config")
                .And.Message.Contains("Player Steering Target")
                .And.Message.Contains("Run Camera Source")
                .And.Message.Contains("Run Progress Frame Source")
                .And.Message.Contains("Rigidbody Contact Notifier")
                .And.Message.Contains("Run Camera Anchor")
                .And.Message.Contains("Run Camera Rig")
                .And.Message.Contains("Input Camera")
                .And.Message.Contains("Slingshot View")
                .And.Message.Contains("Launch Target"));
    }

    [Test]
    public void ValidateRequiredReferencesForTests_AllReferencesAssigned_DoesNotThrow()
    {
        var fixture = CreateValidScopeFixture();

        Assert.That(fixture.Scope.ValidateRequiredReferencesForTests, Throws.Nothing);
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
        var slingshotLauncher = container.Resolve<ISlingshotLauncher>();
        var initializables = container.Resolve<ContainerLocal<IReadOnlyList<IInitializable>>>().Value;
        var fixedTickables = container.Resolve<ContainerLocal<IReadOnlyList<IFixedTickable>>>().Value;
        var lateTickables = container.Resolve<ContainerLocal<IReadOnlyList<ILateTickable>>>().Value;
        var launchTarget = container.Resolve<ILaunchTarget>();
        var heldLaunchTarget = container.Resolve<IHeldLaunchTarget>();
        var silhouetteSource = container.Resolve<ILaunchTargetSilhouetteSource>();
        var steeringTarget = container.Resolve<IPlayerSteeringTarget>();
        var steeringConfig = container.Resolve<IPlayerSteeringConfig>();
        var runCameraConfig = container.Resolve<IRunCameraConfig>();
        var runEndConfig = container.Resolve<IRunEndConfig>();
        var runCameraSource = container.Resolve<IRunCameraSource>();
        var runMotionSource = container.Resolve<IRunMotionSource>();
        var runProgressService = container.Resolve<IRunProgressService>();
        var runProgressFrameSource = container.Resolve<IRunProgressFrameSource>();
        var contactNotifier = container.Resolve<IRigidbodyContactNotifier>();
        var contactClassifier = container.Resolve<IRunContactClassifier>();
        var runEndCandidateReceiver = container.Resolve<IRunEndCandidateReceiver>();
        var runCameraAnchor = container.Resolve<IRunCameraAnchor>();
        var runCameraRig = container.Resolve<IRunCameraRig>();
        var bandShapeProvider = container.Resolve<ISlingshotBandShapeProvider>();

        Assert.That(unityInput, Is.Not.Null);
        Assert.That(gameplayStateService.CurrentStateId, Is.SameAs(fixture.PreLaunchStateId));
        Assert.That(slingshotNotifier, Is.Not.Null);
        Assert.That(slingshotLauncher, Is.Not.Null);
        Assert.That(initializables.Count, Is.EqualTo(7));
        Assert.That(fixedTickables.Count, Is.EqualTo(4));
        Assert.That(lateTickables.Count, Is.EqualTo(1));
        Assert.That(launchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(heldLaunchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(silhouetteSource, Is.SameAs(fixture.LaunchTarget));
        Assert.That(steeringTarget, Is.SameAs(fixture.PlayerSteeringTarget));
        Assert.That(steeringTarget, Is.Not.SameAs(fixture.LaunchTarget));
        Assert.That(steeringConfig, Is.Not.Null);
        Assert.That(runCameraConfig, Is.SameAs(fixture.RunCameraConfig));
        Assert.That(runEndConfig, Is.SameAs(fixture.RunEndConfig));
        Assert.That(runCameraSource, Is.SameAs(fixture.RunCameraSource));
        Assert.That(runMotionSource, Is.SameAs(fixture.RunCameraSource));
        Assert.That(runProgressService, Is.Not.Null);
        Assert.That(runProgressFrameSource, Is.SameAs(fixture.RunProgressFrameSource));
        Assert.That(contactNotifier, Is.SameAs(fixture.ContactNotifier));
        Assert.That(contactClassifier, Is.Not.Null);
        Assert.That(runEndCandidateReceiver, Is.Not.Null);
        Assert.That(runCameraAnchor, Is.SameAs(fixture.RunCameraAnchor));
        Assert.That(runCameraRig, Is.SameAs(fixture.RunCameraRig));
        Assert.That(bandShapeProvider, Is.Not.Null);
    }

    private ValidScopeFixture CreateValidScopeFixture()
    {
        var scope = CreateGameObject("Gameplay Lifetime Scope Test").AddComponent<GameplayLifetimeScope>();
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var runEnded = CreateStateId("Run Ended");
        var preLaunchToRunning = CreateTransition(preLaunch, running);
        var runningToRunEnded = CreateTransition(running, runEnded);
        var runEndedToPreLaunch = CreateTransition(runEnded, preLaunch);
        var gameplayStateConfig = CreateGameplayStateConfig(
            preLaunch,
            preLaunchToRunning,
            runningToRunEnded,
            runEndedToPreLaunch);
        var slingshotConfig = Track(ScriptableObject.CreateInstance<SlingshotConfig>());
        var playerSteeringConfig = Track(ScriptableObject.CreateInstance<PlayerSteeringConfig>());
        var runCameraConfig = Track(ScriptableObject.CreateInstance<RunCameraConfig>());
        var runEndConfig = Track(ScriptableObject.CreateInstance<RunEndConfig>());
        var camera = CreateGameObject("Gameplay Camera").AddComponent<Camera>();
        var slingshotView = CreateSlingshotView(slingshotConfig);
        var launchTarget = CreateLaunchTarget(out var playerSteeringTarget, out var runCameraSource, out var contactNotifier);
        var runProgressFrameSource = CreateGameObject("Run Progress Frame Source").AddComponent<RunProgressFrameSource>();
        var runCameraAnchor = CreateGameObject("Run Camera Anchor").AddComponent<TransformRunCameraAnchor>();
        var runCameraRig = CreateGameObject("Run Camera Rig").AddComponent<CinemachineRunCameraRig>();
        var preLaunchCamera = CreateGameObject("Pre-Launch Camera").AddComponent<CinemachineCamera>();
        var runCamera = CreateGameObject("Run Camera").AddComponent<CinemachineCamera>();
        runCameraRig.SetReferencesForTests(preLaunchCamera, runCamera);

        scope.SetReferencesForTests(
            gameplayStateConfig,
            preLaunch,
            running,
            runEnded,
            slingshotConfig,
            playerSteeringConfig,
            runCameraConfig,
            runEndConfig,
            playerSteeringTarget,
            runCameraSource,
            runProgressFrameSource,
            contactNotifier,
            runCameraAnchor,
            runCameraRig,
            camera,
            slingshotView,
            launchTarget);

        return new ValidScopeFixture(
            scope,
            preLaunch,
            launchTarget,
            playerSteeringTarget,
            runCameraConfig,
            runEndConfig,
            runCameraSource,
            runProgressFrameSource,
            contactNotifier,
            runCameraAnchor,
            runCameraRig);
    }

    private SlingshotView CreateSlingshotView(SlingshotConfig config)
    {
        var view = CreateGameObject("Slingshot View").AddComponent<SlingshotView>();
        var leftAnchor = CreateGameObject("Left Anchor").transform;
        var rightAnchor = CreateGameObject("Right Anchor").transform;
        var restPoint = CreateGameObject("Rest Point").transform;
        var launchFrame = CreateGameObject("Launch Frame").transform;
        var bandLineRenderer = CreateGameObject("Band").AddComponent<LineRenderer>();
        var pullHintObject = CreateGameObject("Pull Hint");
        var touchIndicatorObject = CreateGameObject("Touch Indicator");

        leftAnchor.position = new Vector3(-1f, 1f, 0f);
        rightAnchor.position = new Vector3(1f, 1f, 0f);
        restPoint.position = new Vector3(0f, 1f, 0f);
        launchFrame.position = restPoint.position;
        launchFrame.rotation = Quaternion.identity;

        view.SetReferencesForTests(leftAnchor, rightAnchor, restPoint, launchFrame, bandLineRenderer, pullHintObject, touchIndicatorObject, config);

        return view;
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

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
        stateId.name = stateName;

        return stateId;
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

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);

        return value;
    }

    private readonly struct ValidScopeFixture
    {
        public GameplayLifetimeScope Scope { get; }
        public GameplayStateId PreLaunchStateId { get; }
        public RigidbodyLaunchTarget LaunchTarget { get; }
        public RigidbodyPlayerSteeringTarget PlayerSteeringTarget { get; }
        public RunCameraConfig RunCameraConfig { get; }
        public RunEndConfig RunEndConfig { get; }
        public RigidbodyRunCameraSource RunCameraSource { get; }
        public RunProgressFrameSource RunProgressFrameSource { get; }
        public RigidbodyContactNotifier ContactNotifier { get; }
        public TransformRunCameraAnchor RunCameraAnchor { get; }
        public CinemachineRunCameraRig RunCameraRig { get; }

        public ValidScopeFixture(
            GameplayLifetimeScope scope,
            GameplayStateId preLaunchStateId,
            RigidbodyLaunchTarget launchTarget,
            RigidbodyPlayerSteeringTarget playerSteeringTarget,
            RunCameraConfig runCameraConfig,
            RunEndConfig runEndConfig,
            RigidbodyRunCameraSource runCameraSource,
            RunProgressFrameSource runProgressFrameSource,
            RigidbodyContactNotifier contactNotifier,
            TransformRunCameraAnchor runCameraAnchor,
            CinemachineRunCameraRig runCameraRig)
        {
            Scope = scope;
            PreLaunchStateId = preLaunchStateId;
            LaunchTarget = launchTarget;
            PlayerSteeringTarget = playerSteeringTarget;
            RunCameraConfig = runCameraConfig;
            RunEndConfig = runEndConfig;
            RunCameraSource = runCameraSource;
            RunProgressFrameSource = runProgressFrameSource;
            ContactNotifier = contactNotifier;
            RunCameraAnchor = runCameraAnchor;
            RunCameraRig = runCameraRig;
        }
    }
}
