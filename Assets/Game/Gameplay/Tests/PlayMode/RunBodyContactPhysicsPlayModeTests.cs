using System.Collections;
using Game.Gameplay;
using Game.Gameplay.Tests.Common;
using Game.Gameplay.Tests.PlayMode;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class RunBodyContactPhysicsPlayModeTests : BaseGameplayTestAssetsFixture
{
    private readonly Vector3 _testOrigin = new(3000f, 0f, 3000f);

    [UnityTest]
    public IEnumerator given_LaunchLandingBounce_when_FirstSupportedPassRuns_then_LiftIsBoundedAndTangentIsPreserved()
    {
        var config = new RunBodyContactPhysicsConfig
        {
            BodyBounciness = 0.75f,
            SupportProbeDistance = 0.8f,
            LaunchLandingStabilizationSeconds = 0.4f,
            LaunchLandingMaximumLiftSpeed = 0f,
        };

        using var scenario = new RunBodyContactPhysicsScenario(
            TestAssets.RunSurfaceLayerMask,
            _testOrigin,
            config);

        var surface = scenario.CreateRunSurface(
            "Landing Surface",
            _testOrigin,
            Quaternion.identity,
            new Vector3(30f, 0.5f, 30f),
            0.75f);

        scenario.SetBodyPose(
            _testOrigin + Vector3.up * 1.5f,
            Vector3.forward * 6f + Vector3.down * 5f);
        scenario.SynchronizeTransforms();
        scenario.ActivateRun();

        var observedUnsupportedSample = false;
        var observedStabilizedBounce = false;
        var stabilizedStep = default(RunBodyContactPhysicsStep);

        for (var stepIndex = 0; stepIndex < 60; stepIndex += 1)
        {
            yield return scenario.Step();
            var step = scenario.CurrentStep;
            observedUnsupportedSample |= !step.SurfaceContext.IsGrounded;

            Assert.That(step.MovementWriteCount, Is.EqualTo(1));

            if (!scenario.HasCollisionWith(surface) || !step.SurfaceContext.IsGrounded)
                continue;

            var groundNormal = step.SurfaceContext.GroundNormal;
            var sampledLiftSpeed = Vector3.Dot(step.SampledVelocity, groundNormal);
            var writtenLiftSpeed = Vector3.Dot(step.WrittenVelocity, groundNormal);

            if (sampledLiftSpeed <= config.LaunchLandingMaximumLiftSpeed + 0.1f
                || writtenLiftSpeed > config.LaunchLandingMaximumLiftSpeed + 0.02f)
            {
                continue;
            }

            observedStabilizedBounce = true;
            stabilizedStep = step;
            break;
        }

        Assert.That(observedUnsupportedSample, Is.True, "Expected the armed stabilizer to observe launch flight.");
        Assert.That(scenario.HasCollisionWith(surface), Is.True, "Expected a real Rigidbody collision with the Run Surface.");
        Assert.That(observedStabilizedBounce, Is.True, "Expected a real bounce followed by a supported stabilization pass.");

        var normal = stabilizedStep.SurfaceContext.GroundNormal;
        var sampledTangent = Vector3.ProjectOnPlane(stabilizedStep.SampledVelocity, normal);
        var writtenTangent = Vector3.ProjectOnPlane(stabilizedStep.WrittenVelocity, normal);
        var sampledLift = Vector3.Dot(stabilizedStep.SampledVelocity, normal);
        var writtenLift = Vector3.Dot(stabilizedStep.WrittenVelocity, normal);

        Assert.That(sampledLift, Is.GreaterThan(config.LaunchLandingMaximumLiftSpeed + 0.1f));
        Assert.That(writtenLift, Is.LessThanOrEqualTo(config.LaunchLandingMaximumLiftSpeed + 0.02f));
        Assert.That(Vector3.Distance(writtenTangent, sampledTangent), Is.LessThanOrEqualTo(0.02f));
        Assert.That(stabilizedStep.Diagnostics.HasValidGroundedRunSurface, Is.True);
        AssertFinite(stabilizedStep.PostSolverVelocity);
    }

    [UnityTest]
    public IEnumerator given_GroundedFlatMotion_when_SlowdownEvaluatesAroundRealSupport_then_OnlyTangentSpeedDecreases()
    {
        var config = new RunBodyContactPhysicsConfig
        {
            SurfaceSlowdown = 10f,
        };

        using var scenario = new RunBodyContactPhysicsScenario(
            TestAssets.RunSurfaceLayerMask,
            _testOrigin,
            config);

        var surface = scenario.CreateRunSurface(
            "Flat Slowdown Surface",
            _testOrigin,
            Quaternion.identity,
            new Vector3(20f, 0.5f, 20f));

        scenario.SetBodyPose(_testOrigin + Vector3.up * 0.52f, Vector3.zero);
        scenario.SynchronizeTransforms();
        yield return WaitUntilCollision(scenario, surface, 12);

        scenario.SetBodyVelocity(Vector3.forward * 5f);
        scenario.ActivateRun();
        yield return scenario.Step();

        var step = scenario.CurrentStep;
        var normal = step.SurfaceContext.GroundNormal;
        var sampledTangentSpeed = TangentSpeed(step.SampledVelocity, normal);
        var writtenTangentSpeed = TangentSpeed(step.WrittenVelocity, normal);
        var sampledNormalSpeed = Vector3.Dot(step.SampledVelocity, normal);
        var writtenNormalSpeed = Vector3.Dot(step.WrittenVelocity, normal);

        Assert.That(step.MovementWriteCount, Is.EqualTo(1));
        Assert.That(step.SurfaceContext.IsGrounded, Is.True);
        Assert.That(step.Diagnostics.HasValidGroundedRunSurface, Is.True);

        Assert.That(
            step.Diagnostics.PolicyContributors,
            Is.EqualTo(RunBodySpeedDecisionContributors.SurfaceSlowdown));
        Assert.That(writtenTangentSpeed, Is.LessThan(sampledTangentSpeed - 0.05f));
        Assert.That(writtenNormalSpeed, Is.EqualTo(sampledNormalSpeed).Within(0.01f));
        AssertFinite(step.PostSolverVelocity);
    }

    [UnityTest]
    public IEnumerator given_EquivalentGroundedSlope_when_DownhillAccelerationEnabled_then_WrittenSpeedExceedsControl()
    {
        var rotation = Quaternion.AngleAxis(25f, Vector3.right);
        var normal = rotation * Vector3.up;
        var downhillDirection = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
        var initialVelocity = downhillDirection * 8f;
        var controlWrittenSpeed = 0f;

        using (var controlScenario = new RunBodyContactPhysicsScenario(
                   TestAssets.RunSurfaceLayerMask,
                   _testOrigin,
                   new RunBodyContactPhysicsConfig()))
        {
            var controlSurface = controlScenario.CreateRunSurface(
                "Downhill Control Surface",
                _testOrigin,
                rotation,
                new Vector3(20f, 0.5f, 30f));

            controlScenario.SetBodyPose(_testOrigin + normal * 0.52f, Vector3.zero);
            controlScenario.SynchronizeTransforms();
            yield return WaitUntilCollision(controlScenario, controlSurface, 12);

            controlScenario.SetBodyVelocity(initialVelocity);
            controlScenario.ActivateRun();
            yield return controlScenario.Step();

            var controlStep = controlScenario.CurrentStep;
            Assert.That(controlStep.MovementWriteCount, Is.EqualTo(1));
            Assert.That(controlStep.Diagnostics.HasValidGroundedRunSurface, Is.True);
            controlWrittenSpeed = TangentSpeed(controlStep.WrittenVelocity, controlStep.SurfaceContext.GroundNormal);
        }

        var acceleratedOrigin = _testOrigin + Vector3.right * 50f;

        var acceleratedConfig = new RunBodyContactPhysicsConfig
        {
            DownhillAcceleration = 20f,
        };

        using var acceleratedScenario = new RunBodyContactPhysicsScenario(
            TestAssets.RunSurfaceLayerMask,
            acceleratedOrigin,
            acceleratedConfig);

        var acceleratedSurface = acceleratedScenario.CreateRunSurface(
            "Downhill Accelerated Surface",
            acceleratedOrigin,
            rotation,
            new Vector3(20f, 0.5f, 30f));

        acceleratedScenario.SetBodyPose(acceleratedOrigin + normal * 0.52f, Vector3.zero);
        acceleratedScenario.SynchronizeTransforms();
        yield return WaitUntilCollision(acceleratedScenario, acceleratedSurface, 12);

        acceleratedScenario.SetBodyVelocity(initialVelocity);
        acceleratedScenario.ActivateRun();
        yield return acceleratedScenario.Step();

        var acceleratedStep = acceleratedScenario.CurrentStep;

        var acceleratedWrittenSpeed = TangentSpeed(
            acceleratedStep.WrittenVelocity,
            acceleratedStep.SurfaceContext.GroundNormal);

        Assert.That(acceleratedStep.MovementWriteCount, Is.EqualTo(1));
        Assert.That(acceleratedStep.Diagnostics.HasValidGroundedRunSurface, Is.True);
        Assert.That(acceleratedStep.Diagnostics.ForwardDownhillDegrees, Is.GreaterThan(20f));

        Assert.That(
            acceleratedStep.Diagnostics.PolicyContributors & RunBodySpeedDecisionContributors.DownhillAcceleration,
            Is.EqualTo(RunBodySpeedDecisionContributors.DownhillAcceleration));
        Assert.That(acceleratedWrittenSpeed, Is.GreaterThan(controlWrittenSpeed + 0.05f));
        Assert.That(acceleratedWrittenSpeed, Is.LessThan(acceleratedConfig.BaseSoftMaximumSpeed));
        AssertFinite(acceleratedStep.PostSolverVelocity);
    }

    [UnityTest]
    public IEnumerator given_LowSpeedAssistHitsWall_when_SolverCancelsMotion_then_RequestBudgetRemainsBoundedAndPropulsionStops()
    {
        var config = new RunBodyContactPhysicsConfig
        {
            LowSpeedAssistTargetSpeed = 5f,
            LowSpeedAssistAcceleration = 12f,
        };

        using var scenario = new RunBodyContactPhysicsScenario(
            TestAssets.RunSurfaceLayerMask,
            _testOrigin,
            config);

        var surface = scenario.CreateRunSurface(
            "Wall Assist Surface",
            _testOrigin,
            Quaternion.identity,
            new Vector3(20f, 0.5f, 20f));

        var wall = scenario.CreateWall(
            "Assist Blocking Wall",
            _testOrigin + Vector3.forward * 1f + Vector3.up,
            new Vector3(4f, 2f, 0.2f));

        const float initialSpeed = 2f;
        var initialBudget = config.LowSpeedAssistTargetSpeed - initialSpeed;

        scenario.SetBodyPose(_testOrigin + Vector3.up * 0.52f, Vector3.zero);
        scenario.SynchronizeTransforms();
        yield return WaitUntilCollision(scenario, surface, 12);

        scenario.SetBodyVelocity(Vector3.forward * initialSpeed);
        scenario.ActivateRun();

        var cumulativeRequestedAssist = 0f;
        var observedSolverCancellation = false;
        var observedTerminalAttempt = false;

        for (var stepIndex = 0; stepIndex < 60; stepIndex += 1)
        {
            yield return scenario.Step();
            var step = scenario.CurrentStep;
            cumulativeRequestedAssist += step.Diagnostics.RequestedLowSpeedAssistVelocityDelta;

            Assert.That(step.MovementWriteCount, Is.EqualTo(1));

            var writtenForwardSpeed = Vector3.Dot(step.WrittenVelocity, Vector3.forward);
            var postSolverForwardSpeed = Vector3.Dot(step.PostSolverVelocity, Vector3.forward);

            if (scenario.HasCollisionWith(wall)
                && postSolverForwardSpeed < writtenForwardSpeed - 0.25f)
            {
                observedSolverCancellation = true;
            }

            var attemptState = step.Diagnostics.LowSpeedAssistAttemptState;

            if (scenario.HasCollisionWith(wall)
                && step.Diagnostics.RequestedLowSpeedAssistVelocityDelta <= 0.0001f
                && (attemptState == RunBodyLowSpeedAssistAttemptState.Paused
                    || attemptState == RunBodyLowSpeedAssistAttemptState.Exhausted))
            {
                observedTerminalAttempt = true;
                break;
            }
        }

        var postTerminalRequestedAssist = 0f;

        for (var stepIndex = 0; stepIndex < 8; stepIndex += 1)
        {
            yield return scenario.Step();
            var step = scenario.CurrentStep;
            postTerminalRequestedAssist += step.Diagnostics.RequestedLowSpeedAssistVelocityDelta;
            cumulativeRequestedAssist += step.Diagnostics.RequestedLowSpeedAssistVelocityDelta;
            Assert.That(step.MovementWriteCount, Is.EqualTo(1));
        }

        Assert.That(scenario.HasCollisionWith(wall), Is.True, "Expected a real Rigidbody collision with the wall.");
        Assert.That(observedSolverCancellation, Is.True, "Expected PhysX to resolve below the controller-written target.");
        Assert.That(observedTerminalAttempt, Is.True, "Expected the bounded attempt to pause or exhaust after blocking.");
        Assert.That(cumulativeRequestedAssist, Is.LessThanOrEqualTo(initialBudget + 0.01f));
        Assert.That(postTerminalRequestedAssist, Is.LessThanOrEqualTo(0.001f));

        Assert.That(
            scenario.Body.position.z,
            Is.LessThanOrEqualTo(wall.bounds.min.z - scenario.BodyRadius + 0.1f),
            "The blocked Run Body must not tunnel through the wall.");
        AssertFinite(scenario.CurrentStep.PostSolverVelocity);
    }

    [UnityTest]
    public IEnumerator given_CollisionCreatesGroundedOverspeed_when_NextMovementPassRuns_then_ResistanceIsSoft()
    {
        var config = new RunBodyContactPhysicsConfig
        {
            AboveMaximumSpeedResistance = 20f,
            BaseSoftMaximumSpeed = 20f,
        };

        using var scenario = new RunBodyContactPhysicsScenario(
            TestAssets.RunSurfaceLayerMask,
            _testOrigin,
            config);

        var surface = scenario.CreateRunSurface(
            "Collision Overspeed Surface",
            _testOrigin,
            Quaternion.identity,
            new Vector3(30f, 0.5f, 40f));

        scenario.SetBodyPose(_testOrigin + Vector3.up * 0.52f, Vector3.zero);
        scenario.SynchronizeTransforms();
        yield return WaitUntilCollision(scenario, surface, 12);

        scenario.SetBodyVelocity(Vector3.forward * 5f);
        scenario.ActivateRun();

        scenario.CreateProjectile(
            "Overspeed Projectile",
            _testOrigin + Vector3.up * 0.52f - Vector3.forward * 1.6f,
            Vector3.forward * 40f,
            0.35f,
            20f,
            out var projectileCollider);
        scenario.SynchronizeTransforms();

        var observedCollisionOverspeed = false;

        for (var stepIndex = 0; stepIndex < 20; stepIndex += 1)
        {
            yield return scenario.Step();
            var step = scenario.CurrentStep;
            Assert.That(step.MovementWriteCount, Is.EqualTo(1));

            if (scenario.HasCollisionWith(projectileCollider)
                && TangentSpeed(step.PostSolverVelocity, Vector3.up) > config.BaseSoftMaximumSpeed)
            {
                observedCollisionOverspeed = true;
                break;
            }
        }

        Assert.That(scenario.HasCollisionWith(projectileCollider), Is.True, "Expected a real dynamic-body collision.");
        Assert.That(observedCollisionOverspeed, Is.True, "Expected the solver to create speed above the soft envelope.");

        yield return scenario.Step();
        var resistanceStep = scenario.CurrentStep;
        var normal = resistanceStep.SurfaceContext.GroundNormal;
        var sampledSpeed = TangentSpeed(resistanceStep.SampledVelocity, normal);
        var writtenSpeed = TangentSpeed(resistanceStep.WrittenVelocity, normal);

        Assert.That(resistanceStep.MovementWriteCount, Is.EqualTo(1));
        Assert.That(resistanceStep.Diagnostics.HasValidGroundedRunSurface, Is.True);
        Assert.That(sampledSpeed, Is.GreaterThan(config.BaseSoftMaximumSpeed));

        Assert.That(
            resistanceStep.Diagnostics.PolicyContributors & RunBodySpeedDecisionContributors.AboveEnvelopeResistance,
            Is.EqualTo(RunBodySpeedDecisionContributors.AboveEnvelopeResistance));
        Assert.That(writtenSpeed, Is.LessThan(sampledSpeed - 0.05f));
        Assert.That(writtenSpeed, Is.GreaterThan(config.BaseSoftMaximumSpeed));
        AssertFinite(resistanceStep.PostSolverVelocity);
    }

    private IEnumerator WaitUntilCollision(
        RunBodyContactPhysicsScenario scenario,
        Collider expectedCollider,
        int maximumFixedSteps)
    {
        for (var stepIndex = 0; stepIndex < maximumFixedSteps; stepIndex += 1)
        {
            if (scenario.HasCollisionWith(expectedCollider))
                yield break;

            yield return new WaitForFixedUpdate();
        }

        Assert.That(
            scenario.HasCollisionWith(expectedCollider),
            Is.True,
            "Expected a real Rigidbody collision before the fixed-step ceiling.");
    }

    private float TangentSpeed(Vector3 velocity, Vector3 normal)
    {
        return Vector3.ProjectOnPlane(velocity, normal).magnitude;
    }

    private void AssertFinite(Vector3 value)
    {
        Assert.That(float.IsFinite(value.x), Is.True);
        Assert.That(float.IsFinite(value.y), Is.True);
        Assert.That(float.IsFinite(value.z), Is.True);
    }
}
