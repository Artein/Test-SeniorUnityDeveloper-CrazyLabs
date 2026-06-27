using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Input.UnityInput;
using NUnit.Framework;
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
                .And.Message.Contains("Slingshot Config")
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
        var launchTarget = container.Resolve<ILaunchTarget>();
        var heldLaunchTarget = container.Resolve<IHeldLaunchTarget>();
        var silhouetteSource = container.Resolve<ILaunchTargetSilhouetteSource>();
        var bandShapeProvider = container.Resolve<ISlingshotBandShapeProvider>();

        Assert.That(unityInput, Is.Not.Null);
        Assert.That(gameplayStateService.CurrentStateId, Is.SameAs(fixture.PreLaunchStateId));
        Assert.That(slingshotNotifier, Is.Not.Null);
        Assert.That(slingshotLauncher, Is.Not.Null);
        Assert.That(initializables.Count, Is.EqualTo(3));
        Assert.That(launchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(heldLaunchTarget, Is.SameAs(fixture.LaunchTarget));
        Assert.That(silhouetteSource, Is.SameAs(fixture.LaunchTarget));
        Assert.That(bandShapeProvider, Is.Not.Null);
    }

    private ValidScopeFixture CreateValidScopeFixture()
    {
        var scope = CreateGameObject("Gameplay Lifetime Scope Test").AddComponent<GameplayLifetimeScope>();
        var preLaunch = CreateStateId("Pre-Launch");
        var running = CreateStateId("Running");
        var transition = CreateTransition(preLaunch, running);
        var gameplayStateConfig = CreateGameplayStateConfig(preLaunch, transition);
        var slingshotConfig = Track(ScriptableObject.CreateInstance<SlingshotConfig>());
        var camera = CreateGameObject("Gameplay Camera").AddComponent<Camera>();
        var slingshotView = CreateSlingshotView(slingshotConfig);
        var launchTarget = CreateLaunchTarget();

        scope.SetReferencesForTests(gameplayStateConfig, preLaunch, running, slingshotConfig, camera, slingshotView, launchTarget);

        return new ValidScopeFixture(scope, preLaunch, launchTarget);
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

    private RigidbodyLaunchTarget CreateLaunchTarget()
    {
        var rigidbody = CreateGameObject("Player").AddComponent<Rigidbody>();
        var collider = rigidbody.gameObject.AddComponent<SphereCollider>();
        var launchTarget = rigidbody.gameObject.AddComponent<RigidbodyLaunchTarget>();
        launchTarget.SetReferencesForTests(rigidbody, collider);

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

    private GameplayStateConfig CreateGameplayStateConfig(GameplayStateId initialStateId, GameplayStateTransition transition)
    {
        var config = Track(ScriptableObject.CreateInstance<GameplayStateConfig>());
        config.SetValuesForTests(initialStateId, new[] { transition });

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

        public ValidScopeFixture(GameplayLifetimeScope scope, GameplayStateId preLaunchStateId, RigidbodyLaunchTarget launchTarget)
        {
            Scope = scope;
            PreLaunchStateId = preLaunchStateId;
            LaunchTarget = launchTarget;
        }
    }
}
