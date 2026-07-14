using System.Collections;
using System.Linq;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
public sealed partial class GameplaySceneHighSpeedRunContactSafetyTests
{
    private IEnumerator RunObstacleScenario()
    {
        yield return ReloadGameplaySceneWithIsolatedSaves();
        var context = CreateScenarioContext(SceneManager.GetActiveScene());
        yield return ContinueToPreLaunch(context);

        Assert.That(context.Sphere.radius, Is.EqualTo(_expectedProductionSphereRadius).Within(amount: 0.0001f));
        Assert.That(context.Body.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode.ContinuousDynamic));
        Assert.That(context.Body.useGravity, Is.True);

        var obstacle = CreateThinObstacle(context);
        var observation = new ScenarioObservation();

        try
        {
            var phase = CreateObstaclePhase(context, obstacle);

            AssertObstaclePhasePreconditions(context, obstacle, phase);

            context.ContactNotifier.CollisionEntered += notification =>
            {
                if (notification.OtherCollider != obstacle)
                    return;

                observation.CollisionNotificationCount += 1;
                observation.CollisionContactPointCount += notification.ContactCount;
                observation.CollisionRelativeVelocity = notification.RelativeVelocity;
                observation.CollisionClassified = context.ContactClassifier.TryClassify(notification, out _);

                if (observation.CollisionClassified && !observation.HasClassifiedCollisionSnapshot)
                {
                    observation.HasClassifiedCollisionSnapshot = true;
                    observation.CollisionBodyVelocity = context.Body.linearVelocity;
                    observation.MovementWriteCountAtClassifiedCollision = context.MovementTarget.SuccessfulTargetWriteCountForTests;
                }

                for (var contactIndex = 0; contactIndex < notification.ContactCount; contactIndex += 1)
                {
                    var contact = notification.GetContact(contactIndex);

                    var normalImpactSpeed = Mathf.Abs(
                        Vector3.Dot(
                            notification.RelativeVelocity,
                            contact.Normal.normalized));

                    observation.MaximumNormalImpactSpeed = Mathf.Max(
                        observation.MaximumNormalImpactSpeed,
                        normalImpactSpeed);
                }
            };

            context.ResultNotifier.RunResultAccepted += observation.Results.Add;

            BeginManualLaunch(
                context,
                phase.StartSphereCenter,
                phase.CrossingDirection * phase.SpeedMetersPerSecond);

            for (var fixedFrame = 0; fixedFrame < _fixedFrameObservationLimit; fixedFrame += 1)
            {
                yield return new WaitForFixedUpdate();
            }

            var diagnostic = BuildObstacleDiagnostic(
                context,
                obstacle,
                phase,
                observation);

            TestContext.WriteLine(diagnostic);

            Assert.That(observation.CollisionNotificationCount, Is.GreaterThanOrEqualTo(expected: 1), diagnostic);
            Assert.That(observation.CollisionContactPointCount, Is.GreaterThanOrEqualTo(expected: 1), diagnostic);
            Assert.That(observation.HasClassifiedCollisionSnapshot, Is.True, diagnostic);
            Assert.That(observation.Results, Has.Count.EqualTo(expected: 1), diagnostic);
            Assert.That(observation.Results[index: 0].Reason, Is.EqualTo(RunEndReason.ObstacleHit), diagnostic);

            Assert.That(
                context.MovementTarget.SuccessfulTargetWriteCountForTests,
                Is.EqualTo(observation.MovementWriteCountAtClassifiedCollision),
                diagnostic);

            Assert.That(
                observation.Results[index: 0].FinalSpeed,
                Is.EqualTo(observation.CollisionBodyVelocity.magnitude).Within(amount: 0.001f),
                diagnostic);

            Assert.That(
                context.StateService.CurrentStateId,
                Is.SameAs(context.LifetimeScope.RunEndedStateIdForTests),
                diagnostic);
        }
        finally
        {
            Object.Destroy(obstacle.gameObject);
        }
    }

    private BoxCollider CreateThinObstacle(ScenarioContext context)
    {
        var authoredObstacleLayer = FindComponentsInScene<RunContact>(context.Scene)
            .Where(contact => contact.Category == RunContactCategory.Obstacle)
            .Select(contact => contact.GetComponent<Collider>())
            .Where(collider => collider != null && !collider.isTrigger)
            .Select(collider => collider.gameObject.layer)
            .First();

        var obstacleObject = new GameObject(name: "High-Speed Contact Safety Thin Obstacle")
        {
            layer = authoredObstacleLayer
        };

        obstacleObject.transform.SetPositionAndRotation(
            new Vector3(
                context.Body.position.x + 25f,
                GetHighestRunSurfaceY(context.Scene) + 100f,
                context.Body.position.z + 25f),
            Quaternion.identity);

        var obstacle = obstacleObject.AddComponent<BoxCollider>();
        obstacle.size = new Vector3(x: 0.05f, y: 4f, _thinObstacleThickness);
        obstacle.isTrigger = false;

        var contact = obstacleObject.AddComponent<RunContact>();
        contact.SetCategoryForCourseAuthoring(RunContactCategory.Obstacle);
        Physics.SyncTransforms();

        Assert.That(contact.Category, Is.EqualTo(RunContactCategory.Obstacle));
        Assert.That(obstacle.isTrigger, Is.False);
        return obstacle;
    }

    private ObstaclePhase CreateObstaclePhase(
        ScenarioContext context,
        BoxCollider obstacle)
    {
        var speed = _supportedSpeedMetersPerSecond;
        var fixedDeltaTime = Time.fixedDeltaTime;
        var displacement = speed * fixedDeltaTime;
        var obstacleNormal = GetThinnestWorldAxis(obstacle);
        var crossingDirection = Quaternion.AngleAxis(20f, Vector3.up) * obstacleNormal;
        var bodyRadius = GetSphereWorldRadius(context.Sphere);
        var obstacleThickness = GetProjectedBoxThickness(obstacle, crossingDirection);
        var overlapSpan = bodyRadius * 2f + obstacleThickness;
        var sphereContactOffset = context.Sphere.contactOffset;
        var obstacleContactOffset = obstacle.contactOffset;
        var contactEnvelopeSpan = overlapSpan + 2f * (sphereContactOffset + obstacleContactOffset);
        var minimumPhaseClearance = Mathf.Max(sphereContactOffset, obstacleContactOffset);
        var obstacleCenter = GetBoxWorldCenter(obstacle);
        var startSphereCenter = obstacleCenter - crossingDirection * (displacement * 0.5f);
        var plannedPostStepSphereCenter = startSphereCenter + crossingDirection * displacement;

        return new ObstaclePhase(
            speed,
            fixedDeltaTime,
            displacement,
            crossingDirection,
            bodyRadius,
            obstacleThickness,
            overlapSpan,
            sphereContactOffset,
            obstacleContactOffset,
            contactEnvelopeSpan,
            minimumPhaseClearance,
            obstacleCenter,
            startSphereCenter,
            plannedPostStepSphereCenter);
    }

    private void AssertObstaclePhasePreconditions(
        ScenarioContext context,
        BoxCollider obstacle,
        ObstaclePhase phase)
    {
        var preStepDistance = Mathf.Abs(
            Vector3.Dot(
                phase.StartSphereCenter - phase.ObstacleCenter,
                phase.CrossingDirection));

        var postStepDistance = Mathf.Abs(
            Vector3.Dot(
                phase.PlannedPostStepSphereCenter - phase.ObstacleCenter,
                phase.CrossingDirection));

        Assert.That(phase.Displacement, Is.GreaterThan(phase.OverlapSpan));
        Assert.That(phase.Displacement, Is.GreaterThan(phase.ContactEnvelopeSpan));

        Assert.That(
            preStepDistance - phase.ContactEnvelopeHalfSpan,
            Is.GreaterThan(phase.MinimumPhaseClearance));

        Assert.That(
            postStepDistance - phase.ContactEnvelopeHalfSpan,
            Is.GreaterThan(phase.MinimumPhaseClearance));

        Assert.That(obstacle.isTrigger, Is.False);
        Assert.That(obstacle.GetComponent<RunContact>().Category, Is.EqualTo(RunContactCategory.Obstacle));

        var obstacleNormal = GetThinnestWorldAxis(obstacle);

        Assert.That(
            Vector3.Angle(phase.CrossingDirection, obstacleNormal),
            Is.EqualTo(20f).Within(amount: 0.001f),
            message: "The contact must be oblique so the solver changes body speed before result capture.");

        var normalApproachSpeed = Mathf.Abs(
            Vector3.Dot(
                phase.CrossingDirection * phase.SpeedMetersPerSecond,
                obstacleNormal));

        Assert.That(
            normalApproachSpeed,
            Is.GreaterThan(context.RunEndConfig.ObstacleImpactSpeedThreshold),
            message: "The adversarial safety scenario must preserve Obstacle Impact threshold semantics.");
    }

    private string BuildObstacleDiagnostic(
        ScenarioContext context,
        BoxCollider obstacle,
        ObstaclePhase phase,
        ScenarioObservation observation)
    {
        return "High-speed thin-obstacle proof"
               + $" | speed={phase.SpeedMetersPerSecond:F3}m/s"
               + $" | fixedDeltaTime={phase.FixedDeltaTime:F4}s"
               + $" | displacement={phase.Displacement:F4}m"
               + $" | bodyRadius={phase.BodyRadius:F4}m"
               + $" | obstacleThickness={phase.ObstacleThickness:F4}m"
               + $" | overlapSpan={phase.OverlapSpan:F4}m"
               + $" | sphereContactOffset={phase.SphereContactOffset:F4}m"
               + $" | obstacleContactOffset={phase.ObstacleContactOffset:F4}m"
               + $" | contactEnvelopeSpan={phase.ContactEnvelopeSpan:F4}m"
               + $" | minimumPhaseClearance={phase.MinimumPhaseClearance:F4}m"
               + $" | collisionMode={context.Body.collisionDetectionMode}"
               + $" | useGravity={context.Body.useGravity}"
               + $" | obstacleIsTrigger={obstacle.isTrigger}"
               + $" | startCenter={FormatVector(phase.StartSphereCenter)}"
               + $" | plannedPostStepCenter={FormatVector(phase.PlannedPostStepSphereCenter)}"
               + $" | finalCenter={FormatVector(GetSphereWorldCenter(context.Sphere))}"
               + $" | collisionNotifications={observation.CollisionNotificationCount}"
               + $" | contactPoints={observation.CollisionContactPointCount}"
               + $" | relativeVelocity={FormatVector(observation.CollisionRelativeVelocity)}"
               + $" | collisionBodyVelocity={FormatVector(observation.CollisionBodyVelocity)}"
               + $" | writesAtCollision={observation.MovementWriteCountAtClassifiedCollision}"
               + $" | writesAfterObservation={context.MovementTarget.SuccessfulTargetWriteCountForTests}"
               + $" | maxNormalImpactSpeed={observation.MaximumNormalImpactSpeed:F4}m/s"
               + $" | classifierAccepted={observation.CollisionClassified}"
               + $" | obstacleImpactThreshold={context.RunEndConfig.ObstacleImpactSpeedThreshold:F4}m/s"
               + $" | results={observation.Results.Count}"
               + $" | finalState={GetStateName(context)}";
    }

    private readonly struct ObstaclePhase
    {
        public float SpeedMetersPerSecond { get; }
        public float FixedDeltaTime { get; }
        public float Displacement { get; }
        public Vector3 CrossingDirection { get; }
        public float BodyRadius { get; }
        public float ObstacleThickness { get; }
        public float OverlapSpan { get; }
        public float SphereContactOffset { get; }
        public float ObstacleContactOffset { get; }
        public float ContactEnvelopeSpan { get; }
        public float ContactEnvelopeHalfSpan => ContactEnvelopeSpan * 0.5f;
        public float MinimumPhaseClearance { get; }
        public Vector3 ObstacleCenter { get; }
        public Vector3 StartSphereCenter { get; }
        public Vector3 PlannedPostStepSphereCenter { get; }

        public ObstaclePhase(
            float speedMetersPerSecond,
            float fixedDeltaTime,
            float displacement,
            Vector3 crossingDirection,
            float bodyRadius,
            float obstacleThickness,
            float overlapSpan,
            float sphereContactOffset,
            float obstacleContactOffset,
            float contactEnvelopeSpan,
            float minimumPhaseClearance,
            Vector3 obstacleCenter,
            Vector3 startSphereCenter,
            Vector3 plannedPostStepSphereCenter)
        {
            SpeedMetersPerSecond = speedMetersPerSecond;
            FixedDeltaTime = fixedDeltaTime;
            Displacement = displacement;
            CrossingDirection = crossingDirection;
            BodyRadius = bodyRadius;
            ObstacleThickness = obstacleThickness;
            OverlapSpan = overlapSpan;
            SphereContactOffset = sphereContactOffset;
            ObstacleContactOffset = obstacleContactOffset;
            ContactEnvelopeSpan = contactEnvelopeSpan;
            MinimumPhaseClearance = minimumPhaseClearance;
            ObstacleCenter = obstacleCenter;
            StartSphereCenter = startSphereCenter;
            PlannedPostStepSphereCenter = plannedPostStepSphereCenter;
        }
    }
}
