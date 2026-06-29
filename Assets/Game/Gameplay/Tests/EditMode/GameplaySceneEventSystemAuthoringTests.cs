using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneEventSystemAuthoringTests
{
    // TODO - AI Note: Utilize editor asset provider instead of hardcoding paths
    private const string GameplaySceneName = "GameplayScene";
    private const string InputActionsAssetPath = "Assets/InputSystem_Actions.inputactions";

    [Test]
    public void GameplayScene_WhenAuthored_ContainsSingleInputSystemEventSystem()
    {
        var scene = OpenGameplayScene();
        var eventSystems = FindSceneComponents<EventSystem>(scene).ToArray();

        Assert.That(eventSystems, Has.Length.EqualTo(1));
        Assert.That(eventSystems[0].gameObject.name, Is.EqualTo("EventSystem"));

        var eventSystem = eventSystems[0];
        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();

        var legacyModules = eventSystem.GetComponents<BaseInputModule>()
            .Where(module => module is not InputSystemUIInputModule)
            .ToArray();
        var expectedActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsAssetPath);

        Assert.That(inputModule, Is.Not.Null);
        Assert.That(legacyModules, Is.Empty);
        Assert.That(inputModule.actionsAsset, Is.SameAs(expectedActions));
        Assert.That(expectedActions, Is.Not.Null);

        AssertAction(inputModule.point, "Point");
        AssertAction(inputModule.leftClick, "Click");
        AssertAction(inputModule.rightClick, "RightClick");
        AssertAction(inputModule.middleClick, "MiddleClick");
        AssertAction(inputModule.scrollWheel, "ScrollWheel");
        AssertAction(inputModule.move, "Navigate");
        AssertAction(inputModule.submit, "Submit");
        AssertAction(inputModule.cancel, "Cancel");
    }

    [Test]
    public void GameplayScene_WhenAuthored_HasNoMissingScripts()
    {
        var scene = OpenGameplayScene();

        var objectsWithMissingScripts = FindSceneGameObjects(scene)
            .Where(gameObject => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0)
            .Select(gameObject => gameObject.name)
            .ToArray();

        Assert.That(objectsWithMissingScripts, Is.Empty);
    }

    private static Scene OpenGameplayScene()
    {
        var scenePath = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .Single(scenePath => Path.GetFileNameWithoutExtension(scenePath) == GameplaySceneName);

        return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }

    private static IEnumerable<T> FindSceneComponents<T>(Scene scene)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true));
    }

    private static IEnumerable<GameObject> FindSceneGameObjects(Scene scene)
    {
        return scene.GetRootGameObjects().SelectMany(GetSelfAndChildren);
    }

    private static IEnumerable<GameObject> GetSelfAndChildren(GameObject root)
    {
        yield return root;

        foreach (Transform child in root.transform)
        {
            foreach (var descendant in GetSelfAndChildren(child.gameObject))
            {
                yield return descendant;
            }
        }
    }

    private static void AssertAction(InputActionReference actionReference, string actionName)
    {
        Assert.That(actionReference, Is.Not.Null, actionName + " action reference should be assigned.");
        Assert.That(actionReference.action, Is.Not.Null, actionName + " action should resolve.");
        Assert.That(actionReference.action.name, Is.EqualTo(actionName));
    }
}
