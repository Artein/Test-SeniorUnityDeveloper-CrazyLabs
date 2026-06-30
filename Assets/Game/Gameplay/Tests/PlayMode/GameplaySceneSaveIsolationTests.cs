using System.Collections;
using System.IO;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.Upgrades;
using Game.Foundation.Persistence;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneSaveIsolationTests : BaseGameplayScenePlayModeFixture
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_LoadedThroughPlayModeFixture_then_PersistentDataPathProviderUsesIsolatedTempPath()
    {
        yield return LoadGameplaySceneWithIsolatedSaves();

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(SceneManager.GetActiveScene(), "GameplayLifetimeScope");
        var pathProvider = lifetimeScope.Container.Resolve<IPersistentDataPathProvider>();

        Assert.That(pathProvider.PersistentDataPath, Is.EqualTo(IsolatedPersistentDataPathForTests));
        Assert.That(pathProvider.PersistentDataPath, Is.Not.EqualTo(Application.persistentDataPath));
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_UpgradePurchased_then_EconomySaveIsWrittenUnderIsolatedTempPath()
    {
        yield return LoadGameplaySceneWithIsolatedSaves();

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(SceneManager.GetActiveScene(), "GameplayLifetimeScope");
        var catalog = lifetimeScope.Container.Resolve<IUpgradeCatalog>();
        var currencyStorage = lifetimeScope.Container.Resolve<ICurrencyStorage>();
        var evaluator = lifetimeScope.Container.Resolve<UpgradeDefinitionEvaluator>();
        var purchaseService = lifetimeScope.Container.Resolve<UpgradePurchaseService>();
        var saveSettings = lifetimeScope.Container.Resolve<EconomySaveSettings>();
        var definition = catalog.UpgradeDefinitions.First();
        var firstLevelCost = evaluator.GetCostValue(definition, 1);

        currencyStorage.Grant(catalog.PurchaseCurrency, firstLevelCost);
        var result = purchaseService.TryPurchase(definition);
        var savePath = Path.Combine(IsolatedPersistentDataPathForTests, saveSettings.PrimaryFileName);

        Assert.That(result.Status, Is.EqualTo(UpgradePurchaseStatus.Purchased));
        Assert.That(File.Exists(savePath), Is.True);
        Assert.That(Path.GetFullPath(savePath), Does.StartWith(Path.GetFullPath(IsolatedPersistentDataPathForTests)));
    }
}
