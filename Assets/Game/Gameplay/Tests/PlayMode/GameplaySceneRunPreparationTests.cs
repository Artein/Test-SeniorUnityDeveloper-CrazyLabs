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
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneRunPreparationTests
{
    private readonly int _gameplaySceneBuildIndex = 0;

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
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

            yield return SendMouse(mouse, pressScreenPosition, true);
            yield return null;

            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
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
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();

            Assert.That(continueCommand.TryContinue(), Is.True);
            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("PreLaunchStateId"));
            Assert.That(playerRigidbody.isKinematic, Is.True);
            Assert.That(pullHint.activeSelf, Is.True);
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
    public IEnumerator given_GameplayScene_when_RunEnds_then_ReturnsToRunPreparation()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Run Preparation Run End Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
            var runEndCandidateReceiver = lifetimeScope.Container.Resolve<IRunEndCandidateReceiver>();
            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();

            Assert.That(continueCommand.TryContinue(), Is.True);
            yield return PullAndReleaseSlingshot(mouse, activeScene);
            yield return WaitUntilStateName(stateService, "RunningStateId", 60);

            LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=OutOfBounds"));
            runEndCandidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.OutOfBounds));
            yield return WaitUntilStateName(stateService, "RunEndedStateId", 60);
            yield return WaitUntilStateName(stateService, "RunPreparationStateId", 120);

            var geometry = slingshotView.CreateGeometrySnapshot();
            Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
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
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;
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

    private T FindSingleInScene<T>(Scene scene, string objectDescription)
        where T : Component
    {
        var results = scene.GetRootGameObjects()
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<T>(true))
            .ToArray();

        Assert.That(results, Has.Length.EqualTo(1), objectDescription);
        return results[0];
    }

    private GameObject FindGameObjectByName(Scene scene, string objectName)
    {
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            var transforms = rootGameObject.GetComponentsInChildren<Transform>(true);

            foreach (var childTransform in transforms)
            {
                if (childTransform.name == objectName)
                    return childTransform.gameObject;
            }
        }

        Assert.Fail($"Expected scene object '{objectName}' to exist.");
        return null;
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
