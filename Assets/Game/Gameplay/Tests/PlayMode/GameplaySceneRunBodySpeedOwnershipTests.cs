using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneRunBodySpeedOwnershipTests : BaseGameplayScenePlayModeFixture
{
    [UnityTest]
    public IEnumerator given_ProductionGameplayScene_when_UnsupportedHighSpeedLaunchHitsObstacle_then_OverspeedIsVisibleAndRunEnds()
    {
        yield return LoadGameplaySceneWithIsolatedSaves();
        var scene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(scene, "RigidbodyLaunchTarget");
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var obstacle = FindGameObjectByName(scene, "Run Obstacle");
        var obstacleCollider = obstacle.GetComponent<Collider>();
        var contactNotifier = lifetimeScope.Container.Resolve<IRigidbodyContactNotifier>();
        var resultNotifier = lifetimeScope.Container.Resolve<IRunResultNotifier>();
        var diagnostics = lifetimeScope.Container.Resolve<IRunBodySpeedDiagnosticsSource>();
        var results = new List<RunResult>();
        RigidbodyCollisionNotification obstacleCollision = null;

        contactNotifier.CollisionEntered += notification =>
        {
            if (notification.OtherCollider == obstacleCollider)
                obstacleCollision = notification;
        };
        resultNotifier.RunResultAccepted += results.Add;

        yield return ContinueToPreLaunch(lifetimeScope, stateService);

        var launchPosition = new Vector3(0f, GetHighestRunSurfaceY(scene) + 100f, 0f);
        var launchVelocity = Vector3.forward * 80f;
        obstacle.transform.SetPositionAndRotation(launchPosition + Vector3.forward * 16f, Quaternion.identity);

        BeginManualLaunch(lifetimeScope, stateService, launchTarget, launchPosition, launchVelocity);
        yield return WaitUntilUnsupportedSpeedDiagnosticsIsActive(diagnostics, 4);

        var firstPass = diagnostics.Current;
        var initialForwardSpeed = Vector3.Dot(playerRigidbody.linearVelocity, Vector3.forward);

        Assert.That(firstPass.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Active));
        Assert.That(firstPass.HasValidGroundedRunSurface, Is.False);
        Assert.That(firstPass.EffectiveSoftMaximumSpeed, Is.GreaterThan(0f));
        Assert.That(initialForwardSpeed, Is.EqualTo(launchVelocity.z).Within(1f));
        Assert.That(initialForwardSpeed, Is.GreaterThan(firstPass.EffectiveSoftMaximumSpeed * 3f));

        yield return WaitUntilRunResult(results, 30);

        Assert.That(obstacleCollision, Is.Not.Null, "Expected the production Rigidbody contact notifier to observe the obstacle collision.");
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].Reason, Is.EqualTo(RunEndReason.ObstacleHit));
        Assert.That(stateService.CurrentStateId, Is.SameAs(lifetimeScope.RunEndedStateIdForTests));
    }

    [UnityTest]
    public IEnumerator given_ProductionGameplayScene_when_RunEndsAndUpgradedRunLaunches_then_MovementStateIsCleanAndEnvelopeRefreshes()
    {
        yield return LoadGameplaySceneWithIsolatedSaves();
        var scene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
        var acknowledgeCommand = lifetimeScope.Container.Resolve<IRunResultAcknowledgeCommand>();
        var runEndCandidateReceiver = lifetimeScope.Container.Resolve<IRunEndCandidateReceiver>();
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(scene, "RigidbodyLaunchTarget");
        var diagnostics = lifetimeScope.Container.Resolve<IRunBodySpeedDiagnosticsSource>();
        var speedConfig = lifetimeScope.Container.Resolve<IRunBodySpeedConfig>();
        var statResolver = lifetimeScope.Container.Resolve<IRunGameplayStatResolver>();

        var playerMaxSpeedStatId =
            lifetimeScope.Container.Resolve<GameplayStatId>(InjectKey.GameplayStatId.PlayerMaxSpeed);

        var baselineEnvelope = speedConfig.BaseSoftMaximumSpeed;
        var support = CreateRunSurface("Run Body Speed Ownership Test Surface", new Vector3(0f, 99.5f, 0f));

        try
        {
            Assert.That(continueCommand.TryContinue(), Is.True);
            Assert.That(statResolver.Resolve(playerMaxSpeedStatId, baselineEnvelope), Is.EqualTo(baselineEnvelope).Within(0.0001f));

            BeginManualLaunch(
                lifetimeScope,
                stateService,
                launchTarget,
                new Vector3(0f, 100f, 0f),
                Vector3.forward * 2f);
            yield return WaitUntilAssistIsActive(diagnostics, 8);

            var firstRunSnapshot = diagnostics.Current;
            Assert.That(firstRunSnapshot.IsRunSurfaceGrounded, Is.True);
            Assert.That(firstRunSnapshot.HasValidGroundedRunSurface, Is.True);
            Assert.That(float.IsFinite(firstRunSnapshot.SampledTangentSpeed), Is.True);
            Assert.That(firstRunSnapshot.EffectiveSoftMaximumSpeed, Is.EqualTo(baselineEnvelope).Within(0.0001f));
            Assert.That(firstRunSnapshot.RemainingRequestedLowSpeedAssistVelocityBudget, Is.GreaterThan(0f));

            runEndCandidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
            yield return WaitUntilState(stateService, lifetimeScope.RunEndedStateIdForTests, 8);

            Assert.That(diagnostics.Current.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Inactive));
            Assert.That(diagnostics.Current.RemainingRequestedLowSpeedAssistVelocityBudget, Is.Zero);

            yield return AcknowledgeRunEnd(acknowledgeCommand, stateService, lifetimeScope.RunPreparationStateIdForTests, 30);
            PurchasePlayerMaxSpeedUpgrade(lifetimeScope, playerMaxSpeedStatId);

            Assert.That(continueCommand.TryContinue(), Is.True);
            var upgradedEnvelope = statResolver.Resolve(playerMaxSpeedStatId, baselineEnvelope);
            Assert.That(upgradedEnvelope, Is.GreaterThan(baselineEnvelope));

            BeginManualLaunch(
                lifetimeScope,
                stateService,
                launchTarget,
                new Vector3(30f, 100f, 0f),
                Vector3.forward * 10f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var upgradedRunSnapshot = diagnostics.Current;
            Assert.That(upgradedRunSnapshot.State, Is.EqualTo(RunBodySpeedDiagnosticsState.Active));
            Assert.That(upgradedRunSnapshot.HasValidGroundedRunSurface, Is.False);
            Assert.That(upgradedRunSnapshot.EffectiveSoftMaximumSpeed, Is.EqualTo(upgradedEnvelope).Within(0.0001f));
            Assert.That(upgradedRunSnapshot.RemainingRequestedLowSpeedAssistVelocityBudget, Is.Zero);
        }
        finally
        {
            Object.Destroy(support);
        }
    }

    private IEnumerator ContinueToPreLaunch(GameplayLifetimeScope lifetimeScope, IGameplayStateService stateService)
    {
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
        var preLaunchStateId = lifetimeScope.Container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.PreLaunch);

        Assert.That(continueCommand.TryContinue(), Is.True);
        yield return null;
        Assert.That(stateService.CurrentStateId, Is.SameAs(preLaunchStateId));
    }

    private void BeginManualLaunch(
        GameplayLifetimeScope lifetimeScope,
        IGameplayStateService stateService,
        RigidbodyLaunchTarget launchTarget,
        Vector3 launchPosition,
        Vector3 launchVelocity)
    {
        var preLaunchStateId = lifetimeScope.Container.Resolve<GameplayStateId>(InjectKey.GameplayStateId.PreLaunch);

        Assert.That(stateService.CurrentStateId, Is.SameAs(preLaunchStateId));
        Assert.That(stateService.TryTransitionTo(lifetimeScope.RunningStateIdForTests), Is.True);

        SetHeldBodyPose(launchTarget.GetComponent<Rigidbody>(), launchPosition);
        Physics.SyncTransforms();
        ((ILaunchTarget)launchTarget).Launch(launchVelocity);

        var launchRequest = new SlingshotLaunchRequest(
            pullStrength: 1f,
            pullDistance: 1f,
            pullOffset: 0f,
            normalizedLateralPull: 0f,
            finalPullPoint: launchTarget.transform.position,
            launchFrameForward: launchVelocity.normalized,
            launchFrameUp: Vector3.up);

        lifetimeScope.Container.Resolve<ISlingshotLaunchAppliedPublisher>().Publish(
            new SlingshotLaunchAppliedEvent(
                launchRequest,
                launchVelocity,
                launchVelocity.normalized,
                Vector3.up));
    }

    private void SetHeldBodyPose(Rigidbody rigidbody, Vector3 position)
    {
        rigidbody.transform.SetPositionAndRotation(position, Quaternion.identity);
        rigidbody.position = position;
        rigidbody.rotation = Quaternion.identity;
    }

    private GameObject CreateRunSurface(string objectName, Vector3 position)
    {
        var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = objectName;
        surface.layer = GetSingleLayer(TestAssets.RunSurfaceLayerMask);
        surface.transform.SetPositionAndRotation(position, Quaternion.identity);
        surface.transform.localScale = new Vector3(20f, 1f, 20f);
        surface.AddComponent<RunContact>().SetCategoryForCourseAuthoring(RunContactCategory.Surface);
        return surface;
    }

    private float GetHighestRunSurfaceY(Scene scene)
    {
        var surfaceColliders = FindComponentsInScene<Collider>(scene)
            .Where(collider => collider.TryGetComponent(out RunContact contact)
                               && contact.Category == RunContactCategory.Surface)
            .ToArray();

        Assert.That(surfaceColliders, Is.Not.Empty, "Expected the production scene to contain authored Run Surface colliders.");
        return surfaceColliders.Max(collider => collider.bounds.max.y);
    }

    private int GetSingleLayer(LayerMask layerMask)
    {
        var value = layerMask.value;
        Assert.That(value, Is.GreaterThan(0));
        Assert.That(value & (value - 1), Is.Zero, "Expected a LayerMask containing exactly one layer.");

        var layer = 0;

        while ((value >>= 1) > 0)
        {
            layer += 1;
        }

        return layer;
    }

    private IEnumerator WaitUntilAssistIsActive(IRunBodySpeedDiagnosticsSource diagnostics, int fixedFrameLimit)
    {
        for (var frameIndex = 0; frameIndex < fixedFrameLimit; frameIndex += 1)
        {
            yield return new WaitForFixedUpdate();

            var snapshot = diagnostics.Current;

            if (snapshot.State == RunBodySpeedDiagnosticsState.Active
                && snapshot.RequestedContributors.HasFlag(RunBodySpeedDecisionContributors.LowSpeedAssist)
                && snapshot.RequestedLowSpeedAssistVelocityDelta > 0f
                && snapshot.RemainingRequestedLowSpeedAssistVelocityBudget > 0f)
            {
                yield break;
            }
        }

        Assert.Fail("Expected the first production-scene run to activate bounded low-speed assistance.");
    }

    private IEnumerator WaitUntilUnsupportedSpeedDiagnosticsIsActive(
        IRunBodySpeedDiagnosticsSource diagnostics,
        int fixedFrameLimit)
    {
        for (var frameIndex = 0; frameIndex < fixedFrameLimit; frameIndex += 1)
        {
            yield return new WaitForFixedUpdate();

            var snapshot = diagnostics.Current;

            if (snapshot.State == RunBodySpeedDiagnosticsState.Active && !snapshot.HasValidGroundedRunSurface)
                yield break;
        }

        Assert.Fail("Expected unsupported Run Body speed diagnostics to become active.");
    }

    private IEnumerator WaitUntilRunResult(IReadOnlyCollection<RunResult> results, int fixedFrameLimit)
    {
        for (var frameIndex = 0; frameIndex < fixedFrameLimit; frameIndex += 1)
        {
            yield return new WaitForFixedUpdate();

            if (results.Count > 0)
                yield break;
        }

        Assert.Fail("Expected the production-scene run to accept a Run Result.");
    }

    private IEnumerator WaitUntilState(
        IGameplayStateService stateService,
        GameplayStateId expectedState,
        int fixedFrameLimit)
    {
        for (var frameIndex = 0; frameIndex < fixedFrameLimit; frameIndex += 1)
        {
            yield return new WaitForFixedUpdate();

            if (stateService.CurrentStateId == expectedState)
                yield break;
        }

        Assert.Fail($"Expected Gameplay State '{expectedState.name}'.");
    }

    private IEnumerator AcknowledgeRunEnd(
        IRunResultAcknowledgeCommand acknowledgeCommand,
        IGameplayStateService stateService,
        GameplayStateId runPreparationState,
        int fixedFrameLimit)
    {
        for (var frameIndex = 0; frameIndex < fixedFrameLimit; frameIndex += 1)
        {
            if (acknowledgeCommand.TryAcknowledge())
            {
                Assert.That(stateService.CurrentStateId, Is.SameAs(runPreparationState));
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }

        Assert.Fail("Expected Run End acknowledgement guard to elapse.");
    }

    private void PurchasePlayerMaxSpeedUpgrade(GameplayLifetimeScope lifetimeScope, GameplayStatId playerMaxSpeedStatId)
    {
        var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();

        var definition = catalog.UpgradeDefinitions.Single(candidate => ReferenceEquals(candidate.TargetStatId, playerMaxSpeedStatId));

        var evaluator = lifetimeScope.Container.Resolve<UpgradeDefinitionEvaluator>();
        var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();
        var purchaseService = lifetimeScope.Container.Resolve<UpgradePurchaseService>();
        var nextLevelCost = evaluator.GetCostValue(definition, 1);

        currencyStorage.Grant(catalog.PurchaseCurrency, nextLevelCost);

        var purchaseResult = purchaseService.TryPurchase(definition);
        Assert.That(purchaseResult.Status, Is.EqualTo(UpgradePurchaseStatus.Purchased));
    }
}
