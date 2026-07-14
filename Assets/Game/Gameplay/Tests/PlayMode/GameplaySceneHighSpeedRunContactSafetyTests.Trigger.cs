using System.Collections;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
public sealed partial class GameplaySceneHighSpeedRunContactSafetyTests
{
    private IEnumerator RunTriggerScenario(
        string objectName,
        RunContactCategory expectedCategory,
        RunEndReason expectedReason,
        float speedMetersPerSecond,
        string speedTier,
        bool preferBelowCourseDirection)
    {
        yield return ReloadGameplaySceneWithIsolatedSaves();
        var context = CreateScenarioContext(SceneManager.GetActiveScene());
        yield return ContinueToPreLaunch(context);

        Assert.That(context.Sphere.radius, Is.EqualTo(_expectedProductionSphereRadius).Within(amount: 0.0001f));
        Assert.That(context.Body.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode.ContinuousDynamic));
        Assert.That(context.Body.useGravity, Is.True);

        var triggerObject = FindGameObjectByName(context.Scene, objectName);
        var trigger = triggerObject.GetComponent<BoxCollider>();
        var contact = triggerObject.GetComponent<RunContact>();

        Assert.That(trigger, Is.Not.Null, $"{objectName} must retain its authored BoxCollider.");
        Assert.That(contact, Is.Not.Null, $"{objectName} must retain collider-local RunContact metadata.");
        Assert.That(trigger.isTrigger, Is.True, $"{objectName} must remain a trigger.");
        Assert.That(contact.Category, Is.EqualTo(expectedCategory));

        var traversal = CreateTriggerTraversal(
            context,
            trigger,
            speedMetersPerSecond,
            speedTier,
            preferBelowCourseDirection);

        AssertTriggerTraversalPreconditions(traversal);

        var observation = new ScenarioObservation();

        context.ContactNotifier.TriggerEntered += notification =>
        {
            if (notification.OtherCollider == trigger)
                observation.TriggerNotificationCount += 1;
        };

        context.ResultNotifier.RunResultAccepted += observation.Results.Add;

        BeginManualLaunch(
            context,
            traversal.StartSphereCenter,
            traversal.CrossingDirection * speedMetersPerSecond);

        for (var fixedFrame = 0; fixedFrame < _fixedFrameObservationLimit; fixedFrame += 1)
        {
            yield return new WaitForFixedUpdate();
        }

        var diagnostic = BuildTriggerDiagnostic(
            context,
            trigger,
            contact,
            traversal,
            observation);

        TestContext.WriteLine(diagnostic);

        Assert.That(observation.TriggerNotificationCount, Is.GreaterThanOrEqualTo(expected: 1), diagnostic);
        Assert.That(observation.Results, Has.Count.EqualTo(expected: 1), diagnostic);
        Assert.That(observation.Results[index: 0].Reason, Is.EqualTo(expectedReason), diagnostic);

        Assert.That(
            context.StateService.CurrentStateId,
            Is.SameAs(context.LifetimeScope.RunEndedStateIdForTests),
            diagnostic);
    }

    private TriggerTraversal CreateTriggerTraversal(
        ScenarioContext context,
        BoxCollider trigger,
        float speedMetersPerSecond,
        string speedTier,
        bool preferBelowCourseDirection)
    {
        var crossingDirection = GetThinnestWorldAxis(trigger);

        if (preferBelowCourseDirection && Vector3.Dot(crossingDirection, Vector3.down) < 0f)
            crossingDirection = -crossingDirection;

        var fixedDeltaTime = Time.fixedDeltaTime;
        var displacement = speedMetersPerSecond * fixedDeltaTime;
        var bodyDiameter = GetSphereWorldRadius(context.Sphere) * 2f;
        var triggerThickness = GetProjectedBoxThickness(trigger, crossingDirection);
        var overlapSpan = bodyDiameter + triggerThickness;
        var overlapMargin = overlapSpan - displacement;
        var triggerCenter = GetBoxWorldCenter(trigger);
        var preStepGap = Mathf.Min(a: 0.1f, displacement * 0.25f);

        var startSphereCenter =
            triggerCenter - crossingDirection * (overlapSpan * 0.5f + preStepGap);

        var plannedPostStepSphereCenter =
            startSphereCenter + crossingDirection * displacement;

        return new TriggerTraversal(
            speedMetersPerSecond,
            speedTier,
            fixedDeltaTime,
            displacement,
            crossingDirection,
            bodyDiameter,
            triggerThickness,
            overlapSpan,
            overlapMargin,
            triggerCenter,
            preStepGap,
            startSphereCenter,
            plannedPostStepSphereCenter);
    }

    private void AssertTriggerTraversalPreconditions(TriggerTraversal traversal)
    {
        var preStepDistance = Mathf.Abs(
            Vector3.Dot(
                traversal.StartSphereCenter - traversal.TriggerCenter,
                traversal.CrossingDirection));

        var postStepDistance = Mathf.Abs(
            Vector3.Dot(
                traversal.PlannedPostStepSphereCenter - traversal.TriggerCenter,
                traversal.CrossingDirection));

        Assert.That(traversal.CrossingDirection.sqrMagnitude, Is.EqualTo(expected: 1f).Within(amount: 0.0001f));
        Assert.That(traversal.OverlapMargin, Is.GreaterThan(expected: 0f));
        Assert.That(traversal.Displacement, Is.GreaterThan(traversal.PreStepGap));
        Assert.That(preStepDistance, Is.GreaterThan(traversal.OverlapHalfSpan));
        Assert.That(postStepDistance, Is.LessThan(traversal.OverlapHalfSpan));
    }

    private string BuildTriggerDiagnostic(
        ScenarioContext context,
        BoxCollider trigger,
        RunContact contact,
        TriggerTraversal traversal,
        ScenarioObservation observation)
    {
        return "High-speed authored-trigger proof"
               + $" | speedTier={traversal.SpeedTier}"
               + $" | speed={traversal.SpeedMetersPerSecond:F3}m/s"
               + $" | fixedDeltaTime={traversal.FixedDeltaTime:F4}s"
               + $" | displacement={traversal.Displacement:F4}m"
               + $" | projectedBodyDiameter={traversal.BodyDiameter:F4}m"
               + $" | projectedTriggerThickness={traversal.TriggerThickness:F4}m"
               + $" | overlapSpan={traversal.OverlapSpan:F4}m"
               + $" | overlapMargin={traversal.OverlapMargin:F4}m"
               + $" | crossingNormal={FormatVector(traversal.CrossingDirection)}"
               + $" | triggerIsTrigger={trigger.isTrigger}"
               + $" | category={contact.Category}"
               + $" | startCenter={FormatVector(traversal.StartSphereCenter)}"
               + $" | plannedPostStepCenter={FormatVector(traversal.PlannedPostStepSphereCenter)}"
               + $" | finalCenter={FormatVector(GetSphereWorldCenter(context.Sphere))}"
               + $" | triggerNotifications={observation.TriggerNotificationCount}"
               + $" | results={observation.Results.Count}"
               + $" | finalState={GetStateName(context)}"
               + " | detectionContract=sampled-trigger-overlap-not-CCD";
    }

    private readonly struct TriggerTraversal
    {
        public float SpeedMetersPerSecond { get; }
        public string SpeedTier { get; }
        public float FixedDeltaTime { get; }
        public float Displacement { get; }
        public Vector3 CrossingDirection { get; }
        public float BodyDiameter { get; }
        public float TriggerThickness { get; }
        public float OverlapSpan { get; }
        public float OverlapHalfSpan => OverlapSpan * 0.5f;
        public float OverlapMargin { get; }
        public Vector3 TriggerCenter { get; }
        public float PreStepGap { get; }
        public Vector3 StartSphereCenter { get; }
        public Vector3 PlannedPostStepSphereCenter { get; }

        public TriggerTraversal(
            float speedMetersPerSecond,
            string speedTier,
            float fixedDeltaTime,
            float displacement,
            Vector3 crossingDirection,
            float bodyDiameter,
            float triggerThickness,
            float overlapSpan,
            float overlapMargin,
            Vector3 triggerCenter,
            float preStepGap,
            Vector3 startSphereCenter,
            Vector3 plannedPostStepSphereCenter)
        {
            SpeedMetersPerSecond = speedMetersPerSecond;
            SpeedTier = speedTier;
            FixedDeltaTime = fixedDeltaTime;
            Displacement = displacement;
            CrossingDirection = crossingDirection;
            BodyDiameter = bodyDiameter;
            TriggerThickness = triggerThickness;
            OverlapSpan = overlapSpan;
            OverlapMargin = overlapMargin;
            TriggerCenter = triggerCenter;
            PreStepGap = preStepGap;
            StartSphereCenter = startSphereCenter;
            PlannedPostStepSphereCenter = plannedPostStepSphereCenter;
        }
    }
}
