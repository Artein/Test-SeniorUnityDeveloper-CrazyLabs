using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Pickups;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneRunPreparationTests : BaseGameplayScenePlayModeFixture
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_StartsInRunPreparationWithSlingshotCaptureDisabled()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Run Preparation Disabled Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var mainCamera = FindSingleInScene<Camera>(activeScene, "Main Camera");
            var runPreparationCamera = FindGameObjectByName(activeScene, "Run Preparation Camera").GetComponent<CinemachineCamera>();
            var preLaunchCamera = FindGameObjectByName(activeScene, "Pre-Launch Camera").GetComponent<CinemachineCamera>();
            var runCamera = FindGameObjectByName(activeScene, "Run Camera").GetComponent<CinemachineCamera>();
            var playerCameraLookTarget = FindGameObjectByName(activeScene, "Player Camera Look Target");
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

            yield return SendMouse(mouse, pressScreenPosition, true);
            yield return null;

            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
            Assert.That(runPreparationCamera.Priority.Value, Is.GreaterThan(preLaunchCamera.Priority.Value));
            Assert.That(runPreparationCamera.Priority.Value, Is.GreaterThan(runCamera.Priority.Value));
            AssertRunPreparationCameraFacesPlayer(runPreparationCamera.transform, launchTarget.transform);
            AssertCameraFramesTarget(mainCamera, playerCameraLookTarget.transform);
            Assert.That(playerRigidbody.isKinematic, Is.True);
            Assert.That(pullHint.activeSelf, Is.False);
            Assert.That(touchIndicator.activeSelf, Is.False);
            Assert.That(bandCenter.transform.position.x, Is.EqualTo(geometry.RestPoint.x).Within(0.05f));
            Assert.That(bandCenter.transform.position.z, Is.EqualTo(geometry.RestPoint.z).Within(0.05f));

            yield return SendMouse(mouse, pressScreenPosition, false);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_ContinueAccepted_then_EntersPreLaunchAndLaunchesOnlyAfterPullRelease()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Run Preparation Continue Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var mainCamera = FindSingleInScene<Camera>(activeScene, "Main Camera");
            var runPreparationCamera = FindGameObjectByName(activeScene, "Run Preparation Camera").GetComponent<CinemachineCamera>();
            var preLaunchCamera = FindGameObjectByName(activeScene, "Pre-Launch Camera").GetComponent<CinemachineCamera>();
            var runCamera = FindGameObjectByName(activeScene, "Run Camera").GetComponent<CinemachineCamera>();
            var playerCameraLookTarget = FindGameObjectByName(activeScene, "Player Camera Look Target");
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();

            yield return WaitUntilCameraHasPositiveSideBias(mainCamera, playerCameraLookTarget.transform, 0.15f, 30);

            Assert.That(continueCommand.TryContinue(), Is.True);
            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("PreLaunchStateId"));
            Assert.That(preLaunchCamera.Priority.Value, Is.GreaterThan(runPreparationCamera.Priority.Value));
            Assert.That(preLaunchCamera.Priority.Value, Is.GreaterThan(runCamera.Priority.Value));

            yield return AssertCameraBlendUsesYUpOrbit(
                mainCamera,
                playerCameraLookTarget.transform,
                runPreparationCamera.transform,
                preLaunchCamera.transform,
                8);
            Assert.That(playerRigidbody.isKinematic, Is.True);
            Assert.That(pullHint.activeSelf, Is.False);
            Assert.That(touchIndicator.activeSelf, Is.False);

            yield return PullAndReleaseSlingshot(mouse, activeScene);
            yield return WaitUntilStateName(stateService, "RunningStateId", 60);
            yield return WaitUntilPlayerLaunches(playerRigidbody);

            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunningStateId"));
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_RunEndAcknowledged_then_ReturnsToRunPreparation()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Run Preparation Run End Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
            var acknowledgeCommand = lifetimeScope.Container.Resolve<IRunResultAcknowledgeCommand>();
            var runEndCandidateReceiver = lifetimeScope.Container.Resolve<IRunEndCandidateReceiver>();
            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var runPreparationCamera = FindGameObjectByName(activeScene, "Run Preparation Camera").GetComponent<CinemachineCamera>();
            var preLaunchCamera = FindGameObjectByName(activeScene, "Pre-Launch Camera").GetComponent<CinemachineCamera>();
            var runCamera = FindGameObjectByName(activeScene, "Run Camera").GetComponent<CinemachineCamera>();
            var runCameraAnchor = FindSingleInScene<TransformRunCameraAnchor>(activeScene, "Run Camera Anchor");
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var runCameraConfig = lifetimeScope.Container.Resolve<IRunCameraConfig>();

            Assert.That(continueCommand.TryContinue(), Is.True);
            yield return PullAndReleaseSlingshot(mouse, activeScene);
            yield return WaitUntilStateName(stateService, "RunningStateId", 60);
            yield return WaitUntilPlayerLaunches(playerRigidbody);

            var geometry = slingshotView.CreateGeometrySnapshot();
            yield return WaitUntilPlayerMovesAwayFrom(playerRigidbody, geometry.RestPoint, 0.5f, 60);
            LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));
            runEndCandidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
            yield return WaitUntilStateName(stateService, "RunEndedStateId", 60);
            var runEndedPlayerPosition = playerRigidbody.position;

            Assert.That(Vector3.Distance(runEndedPlayerPosition, geometry.RestPoint), Is.GreaterThan(0.5f));
            Assert.That(runCamera.Priority.Value, Is.GreaterThan(runPreparationCamera.Priority.Value));
            Assert.That(runCamera.Priority.Value, Is.GreaterThan(preLaunchCamera.Priority.Value));

            Assert.That(
                Vector3.Distance(runCameraAnchor.transform.position, runEndedPlayerPosition + runCameraConfig.AnchorOffset),
                Is.LessThan(1.5f));

            yield return null;
            yield return new WaitForFixedUpdate();

            Assert.That(Vector3.Distance(playerRigidbody.position, runEndedPlayerPosition), Is.LessThan(0.05f));
            Assert.That(acknowledgeCommand.TryAcknowledge(), Is.False);
            yield return AcknowledgeRunEndAfterGuard(acknowledgeCommand, 30);
            yield return WaitUntilStateName(stateService, "RunPreparationStateId", 120);

            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
            Assert.That(runPreparationCamera.Priority.Value, Is.GreaterThan(preLaunchCamera.Priority.Value));
            Assert.That(runPreparationCamera.Priority.Value, Is.GreaterThan(runCamera.Priority.Value));
            Assert.That(playerRigidbody.isKinematic, Is.True);
            Assert.That(playerRigidbody.linearVelocity.sqrMagnitude, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(pullHint.activeSelf, Is.False);
            Assert.That(touchIndicator.activeSelf, Is.False);
            Assert.That(bandCenter.transform.position.x, Is.EqualTo(geometry.RestPoint.x).Within(0.05f));
            Assert.That(bandCenter.transform.position.z, Is.EqualTo(geometry.RestPoint.z).Within(0.05f));
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_UpgradesPurchasedAndRunPlayed_then_SmokePathUsesFrozenRunSnapshot()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Run Preparation Upgrade Smoke Mouse");
        var appliedEvents = new List<SlingshotLaunchAppliedEvent>();

        try
        {
            yield return LoadGameplayScene();

            var activeScene = SceneManager.GetActiveScene();
            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();
            var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();
            var progressStorage = lifetimeScope.Container.Resolve<IUpgradeProgressStorage>();
            var purchaseService = lifetimeScope.Container.Resolve<UpgradePurchaseService>();
            var evaluator = lifetimeScope.Container.Resolve<UpgradeDefinitionEvaluator>();
            var snapshotProvider = lifetimeScope.Container.Resolve<IRunModifierSnapshotProvider>();
            var statResolver = lifetimeScope.Container.Resolve<IRunGameplayStatResolver>();
            var pickupGrantResolver = lifetimeScope.Container.Resolve<IPickupCurrencyGrantResolver>();
            var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
            var acknowledgeCommand = lifetimeScope.Container.Resolve<IRunResultAcknowledgeCommand>();
            var runEndCandidateReceiver = lifetimeScope.Container.Resolve<IRunEndCandidateReceiver>();
            var launchAppliedNotifier = lifetimeScope.Container.Resolve<ISlingshotLaunchAppliedNotifier>();
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var definitions = catalog.UpgradeDefinitions.ToArray();

            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
            Assert.That(definitions, Has.Length.EqualTo(4));
            Assert.That(definitions.Select(definition => definition.StableId), Is.Unique);

            foreach (var definition in definitions)
            {
                Assert.That(definition.Icon, Is.Not.Null, definition.StableId);
                Assert.That(definition.TargetStatId, Is.Not.Null, definition.StableId);
                Assert.That(evaluator.GetEffectValue(definition, 1), Is.GreaterThan(0f), definition.StableId);
            }

            var firstLevelCostTotal = definitions.Sum(definition => evaluator.GetCostValue(definition, 1));
            currencyStorage.Grant(catalog.PurchaseCurrency, firstLevelCostTotal);

            foreach (var definition in definitions)
            {
                var result = purchaseService.TryPurchase(definition);

                Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.Purchased), definition.StableId);
                Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(1), definition.StableId);
            }

            Assert.That(currencyStorage.GetAmount(catalog.PurchaseCurrency), Is.Zero);
            Assert.That(snapshotProvider.CurrentSnapshot.Modifiers, Is.Empty);

            Assert.That(continueCommand.TryContinue(), Is.True);
            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("PreLaunchStateId"));
            Assert.That(snapshotProvider.CurrentSnapshot.Modifiers, Has.Count.EqualTo(definitions.Length));

            foreach (var definition in definitions)
            {
                var effectValue = evaluator.GetEffectValue(definition, 1);
                var expectedValue = ResolveExpectedValue(10f, definition.OperationType, effectValue);
                var resolvedValue = statResolver.Resolve(definition.TargetStatId, 10f);

                Assert.That(resolvedValue, Is.EqualTo(expectedValue).Within(0.001f), definition.StableId);
            }

            launchAppliedNotifier.LaunchApplied += appliedEvents.Add;

            yield return PullAndReleaseSlingshot(mouse, activeScene);
            yield return WaitUntilStateName(stateService, "RunningStateId", 60);
            yield return WaitUntilPlayerLaunches(playerRigidbody);

            Assert.That(appliedEvents, Has.Count.EqualTo(1));
            Assert.That(appliedEvents[0].VelocityChange.sqrMagnitude, Is.GreaterThan(0f));

            Assert.That(Vector3.Dot(playerRigidbody.linearVelocity.normalized, appliedEvents[0].VelocityChange.normalized),
                Is.GreaterThan(0.95f));

            var coinMultiplierDefinition = definitions.Single(definition => definition.StableId == "coin-pickup-multiplier");
            var expectedCoinGrant = Mathf.FloorToInt(10f * evaluator.GetEffectValue(coinMultiplierDefinition, 1));
            var coinResolution = pickupGrantResolver.Resolve(new CurrencyGrant(catalog.PurchaseCurrency, 10));

            Assert.That(coinResolution.BaseAmount, Is.EqualTo(10));
            Assert.That(coinResolution.FinalAmount, Is.EqualTo(expectedCoinGrant));

            LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));
            runEndCandidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
            yield return WaitUntilStateName(stateService, "RunEndedStateId", 60);
            yield return AcknowledgeRunEndAfterGuard(acknowledgeCommand, 30);
            yield return WaitUntilStateName(stateService, "RunPreparationStateId", 120);

            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
            Assert.That(progressStorage.GetLevel(definitions[0]), Is.EqualTo(1));
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator LoadGameplayScene()
    {
        yield return LoadGameplaySceneWithIsolatedSaves();
    }

    private IEnumerator PullAndReleaseSlingshot(Mouse mouse, Scene activeScene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var slingshotInputProjector = lifetimeScope.Container.Resolve<ISlingshotInputProjector>();
        var slingshotConfig = lifetimeScope.Container.Resolve<ISlingshotConfig>();
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
        var pullWorldPosition = geometry.RestPoint - (geometry.LaunchFrameForward * 1.25f);
        var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        Assert.That(
            slingshotInputProjector.TryProjectScreenToPullPlane(releaseScreenPosition, geometry, out _),
            Is.True);
        Assert.That(slingshotConfig.MinimumPullDistance, Is.LessThan(1.25f));

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, releaseScreenPosition, true);
        yield return SendMouse(mouse, releaseScreenPosition, false);
    }

    private IEnumerator WaitUntilStateName(IGameplayStateService stateService, string stateName, int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (stateService.CurrentStateId != null && stateService.CurrentStateId.name == stateName)
                yield break;

            yield return null;
        }

        Assert.Fail($"Expected Gameplay State '{stateName}', but current state is '{stateService.CurrentStateId?.name ?? "<null>"}'.");
    }

    private IEnumerator WaitUntilPlayerLaunches(Rigidbody playerRigidbody)
    {
        for (var frameIndex = 0; frameIndex < 60; frameIndex += 1)
        {
            if (!playerRigidbody.isKinematic && playerRigidbody.linearVelocity.sqrMagnitude > 0.0001f)
                yield break;

            yield return null;
        }

        Assert.Fail("Expected Slingshot pull release to launch the Player.");
    }

    private IEnumerator WaitUntilPlayerMovesAwayFrom(
        Rigidbody playerRigidbody,
        Vector3 origin,
        float minimumDistance,
        int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (Vector3.Distance(playerRigidbody.position, origin) >= minimumDistance)
                yield break;

            yield return null;
        }

        Assert.Fail($"Expected Player to move at least {minimumDistance:0.###} meters away from the origin.");
    }

    private IEnumerator AcknowledgeRunEndAfterGuard(IRunResultAcknowledgeCommand acknowledgeCommand, int fixedFrameLimit)
    {
        for (var frameIndex = 0; frameIndex < fixedFrameLimit; frameIndex += 1)
        {
            if (acknowledgeCommand.TryAcknowledge())
                yield break;

            yield return new WaitForFixedUpdate();
        }

        Assert.Fail("Expected Run End acknowledgement guard to elapse.");
    }

    private void AssertRunPreparationCameraFacesPlayer(Transform runPreparationCamera, Transform player)
    {
        var cameraToPlayer = player.position - runPreparationCamera.position;
        Assert.That(Vector3.Dot(runPreparationCamera.forward.normalized, cameraToPlayer.normalized), Is.GreaterThan(0.85f));
        Assert.That(Vector3.Dot(runPreparationCamera.position - player.position, Vector3.forward), Is.GreaterThan(0.5f));
    }

    private IEnumerator WaitUntilCameraHasPositiveSideBias(Camera camera, Transform target, float minimumSideOffset, int frameLimit)
    {
        var bestSideOffset = float.NegativeInfinity;

        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            yield return null;
            AssertCameraFramesTarget(camera, target);

            var sideOffset = Vector3.Dot(camera.transform.position - target.position, Vector3.right);
            bestSideOffset = Mathf.Max(bestSideOffset, sideOffset);

            if (sideOffset > minimumSideOffset)
                yield break;
        }

        Assert.Fail(
            $"Expected RunPreparation camera to settle on positive X before transition. Best side offset: {bestSideOffset:0.###}m.");
    }

    private IEnumerator AssertCameraBlendUsesYUpOrbit(
        Camera camera,
        Transform target,
        Transform runPreparationCamera,
        Transform preLaunchCamera,
        int frameCount)
    {
        var minimumHeight = Mathf.Min(runPreparationCamera.position.y, preLaunchCamera.position.y) - 0.25f;
        var maximumHeight = Mathf.Max(runPreparationCamera.position.y, preLaunchCamera.position.y) + 0.25f;
        var foundPositiveSideSample = false;

        for (var frameIndex = 0; frameIndex < frameCount; frameIndex += 1)
        {
            yield return null;
            AssertCameraFramesTarget(camera, target);

            Assert.That(camera.transform.position.y, Is.InRange(minimumHeight, maximumHeight));

            var sideOffset = Vector3.Dot(camera.transform.position - target.position, Vector3.right);

            if (sideOffset > 0.15f)
                foundPositiveSideSample = true;
        }

        Assert.That(foundPositiveSideSample, Is.True);
    }

    private void AssertCameraFramesTarget(Camera camera, Transform target)
    {
        var cameraToTarget = target.position - camera.transform.position;
        var forwardAlignment = Vector3.Dot(camera.transform.forward.normalized, cameraToTarget.normalized);
        var viewportPosition = camera.WorldToViewportPoint(target.position);

        Assert.That(forwardAlignment, Is.GreaterThan(0.65f));
        Assert.That(viewportPosition.z, Is.GreaterThan(0f));
        Assert.That(viewportPosition.x, Is.InRange(0.2f, 0.8f));
        Assert.That(viewportPosition.y, Is.InRange(0.2f, 0.85f));
    }

    private Vector2 GetScreenPosition(Camera camera, Vector3 worldPosition)
    {
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        Assert.That(screenPosition.z, Is.GreaterThan(0f));
        return new Vector2(screenPosition.x, screenPosition.y);
    }

    private IEnumerator SendMouse(Mouse mouse, Vector2 screenPosition, bool isPressed)
    {
        mouse.MakeCurrent();

        var mouseState = new MouseState
        {
            position = screenPosition
        }.WithButton(MouseButton.Left, isPressed);

        InputSystem.QueueStateEvent(mouse, mouseState);
        InputSystem.Update();
        yield break;
    }

    private float ResolveExpectedValue(float baseValue, UpgradeOperationType operationType, float effectValue)
    {
        return operationType switch
        {
            UpgradeOperationType.FlatAdd => baseValue + effectValue,
            UpgradeOperationType.AdditivePercent => baseValue * (1f + effectValue),
            UpgradeOperationType.MultiplicativeFactor => baseValue * effectValue,
            UpgradeOperationType.ClampMin => Mathf.Max(baseValue, effectValue),
            UpgradeOperationType.ClampMax => Mathf.Min(baseValue, effectValue),
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };
    }
}
