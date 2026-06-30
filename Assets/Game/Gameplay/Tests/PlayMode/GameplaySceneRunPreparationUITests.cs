using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Tests.Common;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneRunPreparationUITests : BaseGameplayTestAssetsFixture
{
    private const float MinimumCoinIconScreenHeight = 40f;
    private const float MinimumBuyButtonScreenHeight = 64f;
    private const float MinimumUpgradeIconScreenHeight = 39.5f;
    private const float MinimumUpgradeCardAutoSizeFloor = 18f;
    private const float MinimumCoinBalanceFontSize = 36f;
    private const float MinimumTapToPlayFontSize = 44f;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_RunPreparationUIRendersCatalogUpgrades()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();
        var evaluator = lifetimeScope.Container.Resolve<UpgradeDefinitionEvaluator>();
        var definitions = catalog.UpgradeDefinitions.ToArray();

        yield return WaitUntilUpgradeCards(activeScene, definitions.Length);

        var panel = FindGameObjectByName(activeScene, "Run Preparation Panel");
        var coinBalanceIcon = FindGameObjectByName(activeScene, "Coin Balance Icon").GetComponent<Image>();
        var coinBalance = FindGameObjectByName(activeScene, "Coin Balance Label").GetComponent<TMP_Text>();
        var continueButton = FindGameObjectByName(activeScene, "Run Preparation Continue Touch Area").GetComponent<Button>();
        var upgradeCardsRoot = FindGameObjectByName(activeScene, "Run Preparation Upgrade Cards").GetComponent<RectTransform>();
        var cards = GetUpgradeCards(activeScene);

        Assert.That(panel.activeInHierarchy, Is.True);
        Assert.That(coinBalanceIcon.enabled, Is.True);
        Assert.That(coinBalanceIcon.sprite, Is.SameAs(catalog.PurchaseCurrency.Icon));
        Assert.That(coinBalance.text, Is.EqualTo("0"));
        Assert.That(FindChildComponent<TMP_Text>(continueButton.transform, "Tap To Play Label").text, Is.EqualTo("TAP TO PLAY"));
        AssertMobileHeaderSizing(coinBalanceIcon, coinBalance, continueButton);
        Assert.That(definitions, Has.Length.EqualTo(4));
        Assert.That(cards, Has.Length.EqualTo(4));
        Assert.That(cards.Select(card => card.name), Is.EqualTo(definitions.Select(definition => "Upgrade Card - " + definition.StableId)));
        AssertMobileUpgradeStripSizing(upgradeCardsRoot, cards);

        for (var i = 0; i < definitions.Length; i += 1)
        {
            var definition = definitions[i];
            var card = cards[i];
            var headerRow = FindChildTransform(card, "Upgrade Header Row");
            var cardBackground = card.GetComponent<Image>();
            var icon = FindChildComponent<Image>(card, "Upgrade Icon - " + definition.StableId);
            var nameLabel = FindChildComponent<TMP_Text>(card, "Upgrade Name Label");
            var levelLabel = FindChildComponent<TMP_Text>(card, "Upgrade Level Label");
            var effectLabel = FindChildComponent<TMP_Text>(card, "Upgrade Effect Label");
            var buyButton = FindChildComponent<Button>(card, "Buy Button - " + definition.StableId);
            var buyButtonBackground = buyButton.GetComponent<Image>();
            var costIcon = FindChildComponent<Image>(buyButton.transform, "Upgrade Button Cost Currency Icon");
            var costLabel = FindChildComponent<TMP_Text>(buyButton.transform, "Upgrade Button Cost Label");
            var firstCost = evaluator.GetCostValue(definition, 1);
            var nextEffectText = FormatEffect(definition, evaluator.GetEffectValue(definition, 1));

            AssertGeneratedUpgradeBackgrounds(cardBackground, buyButtonBackground, definition.StableId);
            Assert.That(icon.transform.parent, Is.SameAs(headerRow), $"{definition.StableId} icon should be in the header row.");
            Assert.That(nameLabel.transform.parent, Is.SameAs(headerRow), $"{definition.StableId} title should be in the header row.");
            Assert.That(levelLabel.transform.parent, Is.SameAs(headerRow), $"{definition.StableId} offer level should be in the header row.");
            AssertHeaderRowOrder(icon.rectTransform, nameLabel.rectTransform, levelLabel.rectTransform);
            AssertStandaloneCostRowInactiveOrAbsent(card);
            Assert.That(icon.enabled, Is.True, $"{definition.StableId} icon should be enabled.");
            Assert.That(icon.sprite, Is.SameAs(definition.Icon), $"{definition.StableId} icon should match the catalog.");
            Assert.That(nameLabel.text, Is.EqualTo(definition.ShortDisplayName), $"{definition.StableId} short name should match.");
            Assert.That(levelLabel.text, Is.EqualTo("1"), $"{definition.StableId} offer level should match.");

            Assert.That(effectLabel.text, Is.EqualTo(nextEffectText),
                $"{definition.StableId} effect preview should show the next offered effect.");

            Assert.That(effectLabel.text, Does.Not.Contain(">"),
                $"{definition.StableId} effect preview should not show current-to-next comparison.");

            Assert.That(costIcon.enabled, Is.True, $"{definition.StableId} cost icon should be enabled.");
            Assert.That(costIcon.sprite, Is.SameAs(catalog.PurchaseCurrency.Icon), $"{definition.StableId} cost icon should match currency.");

            Assert.That(costLabel.text, Is.EqualTo(firstCost.ToString(CultureInfo.InvariantCulture)), $"{definition.StableId} cost should render.");
            Assert.That(buyButton.interactable, Is.False, $"{definition.StableId} should start unaffordable.");

            Assert.That(GetButtonActionLabel(buyButton), Is.EqualTo("UPGRADE"), $"{definition.StableId} should show upgrade CTA.");

            AssertMobileUpgradeCardSizing(card, icon, nameLabel, levelLabel, effectLabel, buyButton, costIcon, costLabel, definition.StableId);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_RunPreparationViewIsAuthoredOnPanel()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");

        yield return WaitUntilUpgradeCards(activeScene, lifetimeScope.Container.Resolve<IUpgradeCatalog>().UpgradeDefinitions.Count);

        var view = FindSingleInScene<RunPreparationUIView>(activeScene, "RunPreparationUIView");
        var panel = FindGameObjectByName(activeScene, "Run Preparation Panel");
        var gameplayUi = FindGameObjectByName(activeScene, "Gameplay UI");

        Assert.That(view.gameObject, Is.SameAs(panel),
            "RunPreparationUIView should be an authored panel view, not a canvas-level runtime UI builder.");
        Assert.That(view.transform.parent.gameObject, Is.SameAs(gameplayUi));
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_BuyClickedWithSessionCoins_then_UpdatesVisibleBalanceLevelCostAndEffect()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();
        var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();
        var progressStorage = lifetimeScope.Container.Resolve<IUpgradeProgressStorage>();
        var evaluator = lifetimeScope.Container.Resolve<UpgradeDefinitionEvaluator>();
        var definition = catalog.UpgradeDefinitions[0];
        var firstCost = evaluator.GetCostValue(definition, 1);
        var secondCost = evaluator.GetCostValue(definition, 2);
        var startingBalance = firstCost + secondCost;

        yield return ReturnToRunPreparationWithBalance(activeScene, startingBalance);
        yield return WaitUntilUpgradeCards(activeScene, catalog.UpgradeDefinitions.Count);

        var card = FindUpgradeCard(activeScene, definition);
        var buyButton = FindChildComponent<Button>(card, "Buy Button - " + definition.StableId);
        var previousEffectText = FindChildComponent<TMP_Text>(card, "Upgrade Effect Label").text;

        Assert.That(buyButton.interactable, Is.True);

        buyButton.onClick.Invoke();
        yield return null;

        card = FindUpgradeCard(activeScene, definition);
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(1));
        Assert.That(currencyStorage.GetAmount(catalog.PurchaseCurrency), Is.EqualTo(startingBalance - firstCost));

        Assert.That(FindGameObjectByName(activeScene, "Coin Balance Label").GetComponent<TMP_Text>().text,
            Is.EqualTo((startingBalance - firstCost).ToString(CultureInfo.InvariantCulture)));
        Assert.That(FindChildComponent<TMP_Text>(card, "Upgrade Level Label").text, Is.EqualTo("2"));
        var secondEffectText = FormatEffect(definition, evaluator.GetEffectValue(definition, 2));
        var effectLabel = FindChildComponent<TMP_Text>(card, "Upgrade Effect Label");

        Assert.That(effectLabel.text, Is.Not.EqualTo(previousEffectText));
        Assert.That(effectLabel.text, Is.EqualTo(secondEffectText));
        Assert.That(effectLabel.text, Does.Not.Contain(">"));

        buyButton = FindChildComponent<Button>(card, "Buy Button - " + definition.StableId);

        Assert.That(FindChildComponent<TMP_Text>(buyButton.transform, "Upgrade Button Cost Label").text,
            Is.EqualTo(secondCost.ToString(CultureInfo.InvariantCulture)));
        Assert.That(GetButtonActionLabel(buyButton), Is.EqualTo("UPGRADE"));
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_UnaffordableOrMaxed_then_BuyButtonDoesNotMutateProgress()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();
        var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();
        var progressStorage = lifetimeScope.Container.Resolve<IUpgradeProgressStorage>();
        var evaluator = lifetimeScope.Container.Resolve<UpgradeDefinitionEvaluator>();
        var definition = catalog.UpgradeDefinitions[0];

        yield return WaitUntilUpgradeCards(activeScene, catalog.UpgradeDefinitions.Count);

        var unaffordableButton = FindChildComponent<Button>(
            FindUpgradeCard(activeScene, definition),
            "Buy Button - " + definition.StableId);

        Assert.That(unaffordableButton.interactable, Is.False);
        unaffordableButton.onClick.Invoke();
        yield return null;
        Assert.That(progressStorage.GetLevel(definition), Is.Zero);
        Assert.That(currencyStorage.GetAmount(catalog.PurchaseCurrency), Is.Zero);

        var fullUpgradeCost = 0;

        for (var level = 1; level <= definition.MaxLevel; level += 1)
        {
            fullUpgradeCost += evaluator.GetCostValue(definition, level);
        }

        yield return ReturnToRunPreparationWithBalance(activeScene, fullUpgradeCost);

        for (var purchaseIndex = 0; purchaseIndex < definition.MaxLevel; purchaseIndex += 1)
        {
            var buyButton = FindChildComponent<Button>(
                FindUpgradeCard(activeScene, definition),
                "Buy Button - " + definition.StableId);

            Assert.That(buyButton.interactable, Is.True, $"Purchase {purchaseIndex + 1}");
            buyButton.onClick.Invoke();
            yield return null;
        }

        var balanceAtMax = currencyStorage.GetAmount(catalog.PurchaseCurrency);
        var maxedCard = FindUpgradeCard(activeScene, definition);
        var maxedButton = FindChildComponent<Button>(maxedCard, "Buy Button - " + definition.StableId);

        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(definition.MaxLevel));
        Assert.That(maxedButton.interactable, Is.False);
        Assert.That(GetButtonActionLabel(maxedButton), Is.EqualTo("MAX"));
        Assert.That(FindChildComponent<TMP_Text>(maxedCard, "Upgrade Level Label").text, Is.EqualTo("MAX"));
        Assert.That(FindChildComponent<Image>(maxedButton.transform, "Upgrade Button Cost Currency Icon").gameObject.activeSelf, Is.False);
        Assert.That(FindChildComponent<TMP_Text>(maxedButton.transform, "Upgrade Button Cost Label").gameObject.activeSelf, Is.False);

        maxedButton.onClick.Invoke();
        yield return null;
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(definition.MaxLevel));
        Assert.That(currencyStorage.GetAmount(catalog.PurchaseCurrency), Is.EqualTo(balanceAtMax));
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_ContinueClicked_then_TransitionsToPreLaunch()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();

        yield return WaitUntilUpgradeCards(activeScene, lifetimeScope.Container.Resolve<IUpgradeCatalog>().UpgradeDefinitions.Count);

        var panel = FindGameObjectByName(activeScene, "Run Preparation Panel");
        var continueButton = FindGameObjectByName(activeScene, "Run Preparation Continue Touch Area").GetComponent<Button>();
        Assert.That(panel.activeInHierarchy, Is.True);

        continueButton.onClick.Invoke();
        yield return null;

        Assert.That(stateService.CurrentStateId.name, Is.EqualTo("PreLaunchStateId"));
        Assert.That(panel.activeInHierarchy, Is.False);
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_ContinueClickedThroughEventSystem_then_TransitionsToPreLaunch()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();

        yield return WaitUntilUpgradeCards(activeScene, lifetimeScope.Container.Resolve<IUpgradeCatalog>().UpgradeDefinitions.Count);

        var panel = FindGameObjectByName(activeScene, "Run Preparation Panel");
        var continueButton = FindGameObjectByName(activeScene, "Run Preparation Continue Touch Area").GetComponent<Button>();
        Assert.That(panel.activeInHierarchy, Is.True);
        Assert.That(continueButton.interactable, Is.True);
        Assert.That(EventSystem.current, Is.Not.Null, "Gameplay UI must have an EventSystem for player clicks.");
        Assert.That(EventSystem.current.currentInputModule, Is.TypeOf<InputSystemUIInputModule>());

        var upperAreaScreenPosition = GetUpperAreaScreenPosition(continueButton.GetComponent<RectTransform>());

        AssertRaycastHits(continueButton.gameObject, upperAreaScreenPosition);
        yield return ClickThroughEventSystem(upperAreaScreenPosition);
        yield return WaitUntilStateName(stateService, "PreLaunchStateId", 30);

        Assert.That(panel.activeInHierarchy, Is.False);
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_UpgradeButtonClickedThroughEventSystem_then_PurchasesWithoutPreLaunchTransition()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();
        var progressStorage = lifetimeScope.Container.Resolve<IUpgradeProgressStorage>();
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
        var evaluator = lifetimeScope.Container.Resolve<UpgradeDefinitionEvaluator>();
        var definition = catalog.UpgradeDefinitions[0];
        var firstCost = evaluator.GetCostValue(definition, 1);

        yield return ReturnToRunPreparationWithBalance(activeScene, firstCost);
        yield return WaitUntilUpgradeCards(activeScene, catalog.UpgradeDefinitions.Count);

        var buyButton = FindChildComponent<Button>(
            FindUpgradeCard(activeScene, definition),
            "Buy Button - " + definition.StableId);

        Assert.That(buyButton.interactable, Is.True);
        AssertRaycastHits(buyButton);
        yield return ClickButtonThroughEventSystem(buyButton);

        Assert.That(stateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(1));
    }

    private IEnumerator LoadGameplayScene()
    {
        SceneManager.LoadScene(TestAssets.GameplaySceneRef.Path, LoadSceneMode.Single);
        yield return null;
    }

    private IEnumerator ReturnToRunPreparationWithBalance(Scene scene, int balance)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
        var stateConfig = lifetimeScope.Container.Resolve<IGameplayStateConfig>();
        var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();
        var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();

        Assert.That(continueCommand.TryContinue(), Is.True);
        currencyStorage.Grant(catalog.PurchaseCurrency, balance);
        Assert.That(stateService.TryTransitionTo(FindStateId(stateConfig, "RunningStateId")), Is.True);
        Assert.That(stateService.TryTransitionTo(FindStateId(stateConfig, "RunEndedStateId")), Is.True);
        Assert.That(stateService.TryTransitionTo(FindStateId(stateConfig, "RunPreparationStateId")), Is.True);
        yield return null;
    }

    private GameplayStateId FindStateId(IGameplayStateConfig stateConfig, string stateName)
    {
        if (stateConfig.InitialStateId != null && stateConfig.InitialStateId.name == stateName)
            return stateConfig.InitialStateId;

        foreach (var transition in stateConfig.AllowedTransitions)
        {
            if (transition == null)
                continue;

            if (transition.FromStateId != null && transition.FromStateId.name == stateName)
                return transition.FromStateId;

            if (transition.ToStateId != null && transition.ToStateId.name == stateName)
                return transition.ToStateId;
        }

        Assert.Fail($"Expected Gameplay State '{stateName}' to exist in the Gameplay State config.");
        return null;
    }

    private IEnumerator WaitUntilUpgradeCards(Scene scene, int expectedCount)
    {
        for (var frameIndex = 0; frameIndex < 30; frameIndex += 1)
        {
            if (TryFindGameObjectByName(scene, "Run Preparation Upgrade Cards", out var cardsRoot))
            {
                var cards = GetUpgradeCards(cardsRoot.transform);

                if (cards.Length == expectedCount)
                    yield break;
            }

            yield return null;
        }

        Assert.Fail($"Expected Run Preparation UI to render {expectedCount} upgrade cards.");
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

    private IEnumerator ClickButtonThroughEventSystem(Button button)
    {
        var screenPosition = GetScreenCenter(button.GetComponent<RectTransform>());

        yield return ClickThroughEventSystem(screenPosition);
    }

    private IEnumerator ClickThroughEventSystem(Vector2 screenPosition)
    {
        var pointerEventData = CreatePointerEventData(screenPosition, out var clickHandler, out _);

        Assert.That(clickHandler, Is.Not.Null, $"Expected EventSystem raycast at {screenPosition} to find a click handler.");

        ExecuteEvents.Execute(clickHandler, pointerEventData, ExecuteEvents.pointerDownHandler);
        yield return null;

        ExecuteEvents.Execute(clickHandler, pointerEventData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(clickHandler, pointerEventData, ExecuteEvents.pointerClickHandler);
        yield return null;
    }

    private Vector2 GetScreenCenter(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        var center = (corners[0] + corners[2]) * 0.5f;
        return RectTransformUtility.WorldToScreenPoint(null, center);
    }

    private Vector2 GetUpperAreaScreenPosition(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        var x = (corners[0].x + corners[2].x) * 0.5f;
        var y = Mathf.Lerp(corners[0].y, corners[1].y, 0.62f);

        return RectTransformUtility.WorldToScreenPoint(null, new Vector3(x, y, 0f));
    }

    private void AssertRaycastHits(Button button)
    {
        var screenPosition = GetScreenCenter(button.GetComponent<RectTransform>());

        AssertRaycastHits(button.gameObject, screenPosition);
    }

    private void AssertRaycastHits(GameObject expectedClickHandler, Vector2 screenPosition)
    {
        var pointerEventData = CreatePointerEventData(screenPosition, out var clickHandler, out var results);

        Assert.That(clickHandler, Is.SameAs(expectedClickHandler),
            $"Expected EventSystem raycast at {pointerEventData.position} to hit '{expectedClickHandler.name}', but hit [{string.Join(", ", results.Select(result => result.gameObject.name))}].");
    }

    private PointerEventData CreatePointerEventData(Vector2 screenPosition, out GameObject clickHandler, out List<RaycastResult> results)
    {
        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            button = PointerEventData.InputButton.Left,
            clickCount = 1,
            eligibleForClick = true,
            position = screenPosition,
            useDragThreshold = true
        };
        results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerEventData, results);

        clickHandler = results.Count > 0
            ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(results[0].gameObject)
            : null;

        if (results.Count > 0)
        {
            pointerEventData.pointerCurrentRaycast = results[0];
            pointerEventData.pointerPressRaycast = results[0];
        }

        pointerEventData.pointerPress = clickHandler;
        pointerEventData.rawPointerPress = clickHandler;

        return pointerEventData;
    }

    private Transform FindUpgradeCard(Scene scene, UpgradeDefinition definition)
    {
        return FindGameObjectByName(scene, "Upgrade Card - " + definition.StableId).transform;
    }

    private Transform[] GetUpgradeCards(Scene scene)
    {
        return GetUpgradeCards(FindGameObjectByName(scene, "Run Preparation Upgrade Cards").transform);
    }

    private Transform[] GetUpgradeCards(Transform cardsRoot)
    {
        return cardsRoot.Cast<Transform>()
            .Where(child => child.name.StartsWith("Upgrade Card - "))
            .ToArray();
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

    private T FindChildComponent<T>(Transform root, string objectName)
        where T : Component
    {
        var results = root.GetComponentsInChildren<T>(true)
            .Where(component => component.name == objectName)
            .ToArray();

        Assert.That(results, Has.Length.EqualTo(1), objectName);
        return results[0];
    }

    private Transform FindChildTransform(Transform root, string objectName)
    {
        var results = root.GetComponentsInChildren<Transform>(true)
            .Where(component => component.name == objectName)
            .ToArray();

        Assert.That(results, Has.Length.EqualTo(1), objectName);
        return results[0];
    }

    private void AssertStandaloneCostRowInactiveOrAbsent(Transform card)
    {
        var costRows = card.GetComponentsInChildren<Transform>(true)
            .Where(child => child.name == "Upgrade Cost Row")
            .ToArray();

        Assert.That(costRows.All(row => !row.gameObject.activeSelf), Is.True,
            "Cost icon and amount should live inside the upgrade button, not in a standalone card row.");
    }

    private void AssertGeneratedUpgradeBackgrounds(Image cardBackground, Image buyButtonBackground, string stableId)
    {
        Assert.That(cardBackground.sprite, Is.Not.Null, $"{stableId} card should have a generated background sprite.");
        Assert.That(cardBackground.sprite.name, Is.EqualTo("UpgradeCardBackground"), $"{stableId} card should use the generated panel asset.");
        Assert.That(cardBackground.type, Is.EqualTo(Image.Type.Sliced), $"{stableId} card background should be sliced.");
        AssertSpriteBorder(cardBackground.sprite.border, new Vector4(8f, 8f, 8f, 8f), $"{stableId} card");

        Assert.That(buyButtonBackground.sprite, Is.Not.Null, $"{stableId} buy button should have a generated background sprite.");

        Assert.That(buyButtonBackground.sprite.name, Is.EqualTo("UpgradeButtonBackground"),
            $"{stableId} buy button should use the generated button asset.");

        Assert.That(buyButtonBackground.type, Is.EqualTo(Image.Type.Sliced), $"{stableId} buy button background should be sliced.");
        AssertSpriteBorder(buyButtonBackground.sprite.border, new Vector4(8f, 8f, 8f, 8f), $"{stableId} buy button");
    }

    private void AssertSpriteBorder(Vector4 actual, Vector4 expected, string context)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.01f), $"{context} left sprite border should stay compact.");
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.01f), $"{context} bottom sprite border should stay compact.");
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.01f), $"{context} right sprite border should stay compact.");
        Assert.That(actual.w, Is.EqualTo(expected.w).Within(0.01f), $"{context} top sprite border should stay compact.");
    }

    private void AssertHeaderRowOrder(RectTransform icon, RectTransform nameLabel, RectTransform levelLabel)
    {
        Assert.That(icon.position.x, Is.LessThan(nameLabel.position.x), "Upgrade icon should be left of the name.");
        Assert.That(nameLabel.position.x, Is.LessThan(levelLabel.position.x), "Upgrade name should be left of the offer level.");

        Assert.That(Mathf.Abs(icon.position.y - nameLabel.position.y), Is.LessThan(2f),
            "Upgrade icon and name should be on the same row.");

        Assert.That(Mathf.Abs(nameLabel.position.y - levelLabel.position.y), Is.LessThan(2f),
            "Upgrade name and offer level should be on the same row.");
    }

    private void AssertMobileHeaderSizing(Image coinBalanceIcon, TMP_Text coinBalance, Button continueButton)
    {
        var tapToPlayLabel = FindChildComponent<TMP_Text>(continueButton.transform, "Tap To Play Label");

        AssertScreenHeightAtLeast(
            coinBalanceIcon.rectTransform,
            MinimumCoinIconScreenHeight,
            "Coin balance icon should stay readable on mobile.");

        Assert.That(coinBalance.fontSize, Is.GreaterThanOrEqualTo(MinimumCoinBalanceFontSize),
            "Coin balance text should use the mobile HUD font size.");

        Assert.That(tapToPlayLabel.fontSize, Is.GreaterThanOrEqualTo(MinimumTapToPlayFontSize),
            "Tap To Play label should use the mobile prompt font size.");
    }

    private void AssertMobileUpgradeStripSizing(RectTransform upgradeCardsRoot, Transform[] cards)
    {
        var stripRect = GetScreenRect(upgradeCardsRoot);

        Assert.That(stripRect.height, Is.GreaterThan(0f),
            "Upgrade strip should render with positive screen height.");

        AssertCardsShareRowAndDoNotOverlap(cards);

        foreach (var card in cards)
        {
            var cardRect = GetScreenRect(card.GetComponent<RectTransform>());

            Assert.That(cardRect.yMin, Is.GreaterThanOrEqualTo(stripRect.yMin - 2f),
                $"{card.name} should stay inside the upgrade strip bottom.");

            Assert.That(cardRect.yMax, Is.LessThanOrEqualTo(stripRect.yMax + 2f),
                $"{card.name} should stay inside the upgrade strip top.");
        }
    }

    private void AssertMobileUpgradeCardSizing(
        Transform card,
        Image icon,
        TMP_Text nameLabel,
        TMP_Text levelLabel,
        TMP_Text effectLabel,
        Button buyButton,
        Image costIcon,
        TMP_Text costLabel,
        string stableId)
    {
        Assert.That(GetScreenRect(card.GetComponent<RectTransform>()).height, Is.GreaterThan(0f),
            $"{stableId} card should have rendered screen height.");

        AssertScreenHeightAtLeast(
            icon.rectTransform,
            MinimumUpgradeIconScreenHeight,
            $"{stableId} upgrade icon should stay readable on mobile.");

        AssertScreenHeightAtLeast(
            buyButton.GetComponent<RectTransform>(),
            MinimumBuyButtonScreenHeight,
            $"{stableId} buy button should stay comfortably tappable on mobile.");

        Assert.That(nameLabel.fontSize, Is.GreaterThanOrEqualTo(MinimumUpgradeCardAutoSizeFloor),
            $"{stableId} card title should stay above the authored auto-size floor.");

        Assert.That(levelLabel.fontSize, Is.GreaterThanOrEqualTo(MinimumUpgradeCardAutoSizeFloor),
            $"{stableId} offer level should stay above the authored auto-size floor.");

        Assert.That(effectLabel.fontSize, Is.GreaterThanOrEqualTo(MinimumUpgradeCardAutoSizeFloor),
            $"{stableId} effect preview should stay above the authored auto-size floor.");

        Assert.That(GetButtonActionLabel(buyButton), Is.Not.Empty, $"{stableId} button action label should render.");

        Assert.That(FindChildComponent<TMP_Text>(buyButton.transform, "Upgrade Button Action Label").fontSize,
            Is.GreaterThanOrEqualTo(MinimumUpgradeCardAutoSizeFloor),
            $"{stableId} button action should stay above the authored auto-size floor.");

        if (costIcon.gameObject.activeSelf)
        {
            Assert.That(costLabel.fontSize, Is.GreaterThanOrEqualTo(MinimumUpgradeCardAutoSizeFloor),
                $"{stableId} button cost should stay above the authored auto-size floor.");
        }
    }

    private void AssertCardsShareRowAndDoNotOverlap(Transform[] cards)
    {
        var cardRects = cards
            .Select(card => new
            {
                Card = card,
                Rect = GetScreenRect(card.GetComponent<RectTransform>())
            })
            .OrderBy(item => item.Rect.xMin)
            .ToArray();

        var rowCenterY = cardRects[0].Rect.center.y;

        foreach (var item in cardRects)
        {
            Assert.That(Mathf.Abs(item.Rect.center.y - rowCenterY), Is.LessThan(20f),
                $"{item.Card.name} should stay on the same row as the other upgrade cards.");
        }

        for (var index = 1; index < cardRects.Length; index += 1)
        {
            Assert.That(cardRects[index].Rect.xMin, Is.GreaterThanOrEqualTo(cardRects[index - 1].Rect.xMax - 2f),
                $"{cardRects[index].Card.name} should not overlap {cardRects[index - 1].Card.name}.");
        }
    }

    private void AssertScreenHeightAtLeast(RectTransform rectTransform, float minimumHeight, string message)
    {
        Assert.That(GetScreenRect(rectTransform).height, Is.GreaterThanOrEqualTo(minimumHeight), message);
    }

    private Rect GetScreenRect(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        var bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        var topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
    }

    private GameObject FindGameObjectByName(Scene scene, string objectName)
    {
        if (TryFindGameObjectByName(scene, objectName, out var gameObject))
            return gameObject;

        Assert.Fail($"Expected scene object '{objectName}' to exist.");
        return null;
    }

    private bool TryFindGameObjectByName(Scene scene, string objectName, out GameObject result)
    {
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            var transforms = rootGameObject.GetComponentsInChildren<Transform>(true);

            foreach (var childTransform in transforms)
            {
                if (childTransform.name == objectName)
                {
                    result = childTransform.gameObject;
                    return true;
                }
            }
        }

        result = null;
        return false;
    }

    private string GetButtonActionLabel(Button button)
    {
        return FindChildComponent<TMP_Text>(button.transform, "Upgrade Button Action Label").text;
    }

    private string FormatEffect(UpgradeDefinition definition, float value)
    {
        var decimalPlaces = Math.Max(0, definition.DisplayDecimalPlaces);
        var decimalPlacesText = decimalPlaces.ToString(CultureInfo.InvariantCulture);
        var fixedPointFormat = "F" + decimalPlacesText;

        return definition.ValueFormat switch
        {
            UpgradeValueFormat.Multiplier => "x" + value.ToString(fixedPointFormat, CultureInfo.InvariantCulture),
            UpgradeValueFormat.Percent => value.ToString("P" + decimalPlacesText, CultureInfo.InvariantCulture),
            _ => value.ToString(fixedPointFormat, CultureInfo.InvariantCulture)
        };
    }
}
