using System;
using System.Collections;
using System.IO;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Tests.Common;
using Game.Foundation.Persistence;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public abstract class BaseGameplayScenePlayModeFixture : BaseGameplayTestAssetsFixture
{
    protected string IsolatedPersistentDataPathForTests { get; private set; }

    [SetUp]
    public void OnSetUpGameplayScenePlayModeFixture()
    {
        IsolatedPersistentDataPathForTests = Path.Combine(
            Path.GetTempPath(),
            "gameplay-scene-playmode-saves",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(IsolatedPersistentDataPathForTests);
    }

    [TearDown]
    public void OnTearDownGameplayScenePlayModeFixture()
    {
        if (string.IsNullOrWhiteSpace(IsolatedPersistentDataPathForTests) || !Directory.Exists(IsolatedPersistentDataPathForTests))
            return;

        try
        {
            Directory.Delete(IsolatedPersistentDataPathForTests, recursive: true);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to delete PlayMode isolated save directory '{IsolatedPersistentDataPathForTests}'. {exception.Message}");
        }
        finally
        {
            IsolatedPersistentDataPathForTests = null;
        }
    }

    protected IEnumerator LoadGameplaySceneWithIsolatedSaves(Func<Scene, bool> canReuseScene = null)
    {
        var activeScene = SceneManager.GetActiveScene();

        if (CanReuseSceneWithIsolatedSaves(activeScene) && (canReuseScene == null || canReuseScene.Invoke(activeScene)))
            yield break;

        yield return ReloadGameplaySceneWithIsolatedSaves();
    }

    protected IEnumerator ReloadGameplaySceneWithIsolatedSaves()
    {
        var pathProvider = new TestPersistentDataPathProvider(IsolatedPersistentDataPathForTests);

        using (LifetimeScope.Enqueue(builder => builder.RegisterInstance<IPersistentDataPathProvider>(pathProvider)))
        {
            SceneManager.LoadScene(TestAssets.GameplaySceneRef.Path, LoadSceneMode.Single);
            yield return null;
        }

        AssertActiveSceneUsesIsolatedSaves();
    }

    protected IEnumerator LoadGameplaySceneWithIsolatedSavesAndContinueToPreLaunch(Func<Scene, bool> canReuseScene = null)
    {
        yield return LoadGameplaySceneWithIsolatedSaves(canReuseScene);
        yield return ContinueGameplaySceneToPreLaunch(SceneManager.GetActiveScene());
    }

    protected IEnumerator ReloadGameplaySceneWithIsolatedSavesAndContinueToPreLaunch()
    {
        yield return ReloadGameplaySceneWithIsolatedSaves();
        yield return ContinueGameplaySceneToPreLaunch(SceneManager.GetActiveScene());
    }

    protected bool CanReuseSceneWithIsolatedSaves(Scene scene)
    {
        if (!scene.IsValid() || scene.path != TestAssets.GameplaySceneRef.Path)
            return false;

        var lifetimeScopes = FindComponentsInScene<GameplayLifetimeScope>(scene);

        if (lifetimeScopes.Length != 1)
            return false;

        var pathProvider = lifetimeScopes[0].Container.Resolve<IPersistentDataPathProvider>();
        return pathProvider.PersistentDataPath == IsolatedPersistentDataPathForTests;
    }

    protected T FindSingleInScene<T>(Scene scene, string objectDescription)
        where T : Component
    {
        var results = FindComponentsInScene<T>(scene);

        Assert.That(results, Has.Length.EqualTo(1), objectDescription);
        return results[0];
    }

    protected T[] FindComponentsInScene<T>(Scene scene)
        where T : Component
    {
        return scene.GetRootGameObjects()
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<T>(true))
            .ToArray();
    }

    protected GameObject FindGameObjectByName(Scene scene, string objectName)
    {
        if (TryFindGameObjectByName(scene, objectName, out var gameObject))
            return gameObject;

        Assert.Fail($"Expected scene object '{objectName}' to exist.");
        return null;
    }

    protected bool TryFindGameObjectByName(Scene scene, string objectName, out GameObject gameObject)
    {
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            var transforms = rootGameObject.GetComponentsInChildren<Transform>(true);

            foreach (var childTransform in transforms)
            {
                if (childTransform.name == objectName)
                {
                    gameObject = childTransform.gameObject;
                    return true;
                }
            }
        }

        gameObject = null;
        return false;
    }

    private IEnumerator ContinueGameplaySceneToPreLaunch(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
        continueCommand.TryContinue();
        yield return null;
    }

    private void AssertActiveSceneUsesIsolatedSaves()
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(SceneManager.GetActiveScene(), "GameplayLifetimeScope");
        var pathProvider = lifetimeScope.Container.Resolve<IPersistentDataPathProvider>();

        Assert.That(pathProvider.PersistentDataPath, Is.EqualTo(IsolatedPersistentDataPathForTests));
        Assert.That(pathProvider.PersistentDataPath, Is.Not.EqualTo(Application.persistentDataPath));
    }

    private sealed class TestPersistentDataPathProvider : IPersistentDataPathProvider
    {
        public string PersistentDataPath { get; }

        public TestPersistentDataPathProvider(string persistentDataPath)
        {
            PersistentDataPath = persistentDataPath ?? throw new ArgumentNullException(nameof(persistentDataPath));
        }
    }
}
