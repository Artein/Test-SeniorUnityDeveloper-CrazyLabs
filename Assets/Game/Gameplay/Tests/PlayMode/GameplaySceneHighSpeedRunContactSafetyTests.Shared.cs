using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed partial class GameplaySceneHighSpeedRunContactSafetyTests
{
    private ScenarioContext CreateScenarioContext(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(
            scene,
            objectDescription: "GameplayLifetimeScope");

        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(
            scene,
            objectDescription: "RigidbodyLaunchTarget");

        var body = launchTarget.GetComponent<Rigidbody>();

        var sphere = FindGameObjectByName(scene, objectName: "RunBodyContactColliderRoot")
            .GetComponent<SphereCollider>();

        Assert.That(body, Is.Not.Null, message: "Production RigidbodyLaunchTarget must own the Run Body Rigidbody.");
        Assert.That(sphere, Is.Not.Null, message: "RunBodyContactColliderRoot must own the authoritative SphereCollider.");
        Assert.That(sphere.transform.IsChildOf(body.transform), Is.True);
        Assert.That(sphere.isTrigger, Is.False);

        return new ScenarioContext(
            scene,
            lifetimeScope,
            lifetimeScope.Container.Resolve<IGameplayStateService>(),
            launchTarget,
            body,
            sphere,
            lifetimeScope.Container.Resolve<IRigidbodyContactNotifier>(),
            lifetimeScope.Container.Resolve<IRunContactClassifier>(),
            lifetimeScope.Container.Resolve<IRunResultNotifier>(),
            lifetimeScope.Container.Resolve<IRunEndConfig>());
    }

    private IEnumerator ContinueToPreLaunch(ScenarioContext context)
    {
        var continueCommand = context.LifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();

        var preLaunchStateId =
            context.LifetimeScope.Container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.PreLaunch);

        Assert.That(continueCommand.TryContinue(), Is.True);
        yield return null;
        Assert.That(context.StateService.CurrentStateId, Is.SameAs(preLaunchStateId));
    }

    private void BeginManualLaunch(
        ScenarioContext context,
        Vector3 desiredSphereCenter,
        Vector3 launchVelocity)
    {
        var preLaunchStateId =
            context.LifetimeScope.Container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.PreLaunch);

        Assert.That(context.StateService.CurrentStateId, Is.SameAs(preLaunchStateId));

        Assert.That(
            context.StateService.TryTransitionTo(context.LifetimeScope.RunningStateIdForTests),
            Is.True);

        SetBodySphereCenter(context, desiredSphereCenter);
        Physics.SyncTransforms();
        ((ILaunchTarget)context.LaunchTarget).Launch(launchVelocity);

        var launchRequest = new SlingshotLaunchRequest(
            pullStrength: 1f,
            pullDistance: 1f,
            pullOffset: 0f,
            normalizedLateralPull: 0f,
            context.LaunchTarget.transform.position,
            launchVelocity.normalized,
            Vector3.up);

        context.LifetimeScope.Container.Resolve<ISlingshotLaunchAppliedPublisher>().Publish(
            new SlingshotLaunchAppliedEvent(
                launchRequest,
                launchVelocity,
                launchVelocity.normalized,
                Vector3.up));
    }

    private void SetBodySphereCenter(ScenarioContext context, Vector3 desiredSphereCenter)
    {
        SetHeldBodyPose(context.Body, Vector3.zero);
        var sphereCenterOffset = GetSphereWorldCenter(context.Sphere) - context.Body.position;
        var bodyPosition = desiredSphereCenter - sphereCenterOffset;
        SetHeldBodyPose(context.Body, bodyPosition);
        Physics.SyncTransforms();

        Assert.That(
            Vector3.Distance(GetSphereWorldCenter(context.Sphere), desiredSphereCenter),
            Is.LessThan(expected: 0.001f),
            message: "Failed to place the production Run Body sphere at the planned pre-step center.");
    }

    private void SetHeldBodyPose(Rigidbody body, Vector3 position)
    {
        body.transform.SetPositionAndRotation(position, Quaternion.identity);
        body.position = position;
        body.rotation = Quaternion.identity;
    }

    private float GetSphereWorldRadius(SphereCollider sphere)
    {
        var scale = sphere.transform.lossyScale;

        var largestAbsoluteScale = Mathf.Max(
            Mathf.Abs(scale.x),
            Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));

        return sphere.radius * largestAbsoluteScale;
    }

    private Vector3 GetSphereWorldCenter(SphereCollider sphere)
    {
        return sphere.transform.TransformPoint(sphere.center);
    }

    private Vector3 GetBoxWorldCenter(BoxCollider box)
    {
        return box.transform.TransformPoint(box.center);
    }

    private Vector3 GetThinnestWorldAxis(BoxCollider box)
    {
        var scale = box.transform.lossyScale;
        var widthX = Mathf.Abs(box.size.x * scale.x);
        var widthY = Mathf.Abs(box.size.y * scale.y);
        var widthZ = Mathf.Abs(box.size.z * scale.z);

        if (widthX <= widthY && widthX <= widthZ)
            return box.transform.right.normalized;

        if (widthY <= widthZ)
            return box.transform.up.normalized;

        return box.transform.forward.normalized;
    }

    private float GetProjectedBoxThickness(BoxCollider box, Vector3 direction)
    {
        var normalizedDirection = direction.normalized;
        var scale = box.transform.lossyScale;
        var widthX = Mathf.Abs(box.size.x * scale.x);
        var widthY = Mathf.Abs(box.size.y * scale.y);
        var widthZ = Mathf.Abs(box.size.z * scale.z);

        return Mathf.Abs(Vector3.Dot(normalizedDirection, box.transform.right.normalized)) * widthX
               + Mathf.Abs(Vector3.Dot(normalizedDirection, box.transform.up.normalized)) * widthY
               + Mathf.Abs(Vector3.Dot(normalizedDirection, box.transform.forward.normalized)) * widthZ;
    }

    private float GetHighestRunSurfaceY(Scene scene)
    {
        var highestSurfaceY = float.NegativeInfinity;

        foreach (var collider in FindComponentsInScene<Collider>(scene))
        {
            if (!collider.TryGetComponent(out RunContact contact)
                || contact.Category != RunContactCategory.Surface)
            {
                continue;
            }

            highestSurfaceY = Mathf.Max(highestSurfaceY, collider.bounds.max.y);
        }

        Assert.That(float.IsFinite(highestSurfaceY), Is.True, message: "Expected an authored Run Surface.");
        return highestSurfaceY;
    }

    private string GetStateName(ScenarioContext context)
    {
        return context.StateService.CurrentStateId != null
            ? context.StateService.CurrentStateId.name
            : "<null>";
    }

    private string FormatVector(Vector3 value)
    {
        return $"({value.x:F4}, {value.y:F4}, {value.z:F4})";
    }

    private sealed class ScenarioContext
    {
        public Rigidbody Body { get; }
        public IRunContactClassifier ContactClassifier { get; }
        public IRigidbodyContactNotifier ContactNotifier { get; }
        public RigidbodyLaunchTarget LaunchTarget { get; }
        public GameplayLifetimeScope LifetimeScope { get; }
        public IRunResultNotifier ResultNotifier { get; }
        public IRunEndConfig RunEndConfig { get; }
        public Scene Scene { get; }
        public SphereCollider Sphere { get; }
        public IGameplayStateService StateService { get; }

        public ScenarioContext(
            Scene scene,
            GameplayLifetimeScope lifetimeScope,
            IGameplayStateService stateService,
            RigidbodyLaunchTarget launchTarget,
            Rigidbody body,
            SphereCollider sphere,
            IRigidbodyContactNotifier contactNotifier,
            IRunContactClassifier contactClassifier,
            IRunResultNotifier resultNotifier,
            IRunEndConfig runEndConfig)
        {
            Scene = scene;
            LifetimeScope = lifetimeScope;
            StateService = stateService;
            LaunchTarget = launchTarget;
            Body = body;
            Sphere = sphere;
            ContactNotifier = contactNotifier;
            ContactClassifier = contactClassifier;
            ResultNotifier = resultNotifier;
            RunEndConfig = runEndConfig;
        }
    }

    private sealed class ScenarioObservation
    {
        public bool CollisionClassified { get; set; }
        public int CollisionContactPointCount { get; set; }
        public int CollisionNotificationCount { get; set; }
        public Vector3 CollisionRelativeVelocity { get; set; }
        public float MaximumNormalImpactSpeed { get; set; }
        public List<RunResult> Results { get; } = new();
        public int TriggerNotificationCount { get; set; }
    }
}
