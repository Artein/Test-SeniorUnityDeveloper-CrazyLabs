using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Tests.Common;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneEventSystemAuthoringTests : BaseGameplayTestAssetsFixture
{
    private const string LadybugHalfTubeAuthoringTypeName =
        "Game.Level.RunCourses.LadybugRooftopHalfTube.LadybugHalfTubeRunCourseAuthoring";

    private static readonly string[] LadybugObstaclePrefabProperties =
    {
        "_obstacleAc1Prefab",
        "_obstacleAc2Prefab",
        "_obstacleSunroofPrefab",
        "_obstacleSolarPanelsPrefab",
        "_obstacleBillboardPrefab"
    };

    private static readonly string[] LadybugPropPrefabProperties =
    {
        "_rooftopChunk01Prefab",
        "_rooftopChunk02Prefab",
        "_rooftopChunk03DropPrefab",
        "_rooftopChunk05StepPrefab",
        "_waterTankPropPrefab",
        "_roofExitPropPrefab",
        "_satDishPropPrefab",
        "_rampPropPrefab"
    };

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

        Assert.That(inputModule, Is.Not.Null);
        Assert.That(legacyModules, Is.Empty);
        Assert.That(inputModule.actionsAsset, Is.SameAs(TestAssets.InputActionsAsset));
        Assert.That(TestAssets.InputActionsAsset, Is.Not.Null);

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

    [Test]
    public void GameplayScene_WhenAuthored_UsesCourseOwnedLadybugLevelVisualPrefabs()
    {
        var scene = OpenGameplayScene();
        var authoring = FindLadybugHalfTubeAuthoring(scene);
        var serializedObject = new SerializedObject(authoring);
        var courseRoot = GetAuthoringFeatureRoot(authoring);

        for (var propertyIndex = 0; propertyIndex < LadybugObstaclePrefabProperties.Length; propertyIndex += 1)
        {
            var propertyName = LadybugObstaclePrefabProperties[propertyIndex];
            var prefab = AssertCourseOwnedPrefabVariantReference(serializedObject, propertyName, courseRoot);

            AssertObstaclePrefabContract(prefab, propertyName);
        }

        for (var propertyIndex = 0; propertyIndex < LadybugPropPrefabProperties.Length; propertyIndex += 1)
        {
            var propertyName = LadybugPropPrefabProperties[propertyIndex];
            var prefab = AssertCourseOwnedPrefabVariantReference(serializedObject, propertyName, courseRoot);

            AssertPropPrefabContract(prefab, propertyName);
        }
    }

    private static GameObject AssertCourseOwnedPrefabVariantReference(SerializedObject serializedObject, string propertyName, string courseRoot)
    {
        var property = serializedObject.FindProperty(propertyName);

        Assert.That(property, Is.Not.Null, propertyName);
        Assert.That(property.objectReferenceValue, Is.Not.Null, propertyName);

        var prefabPath = AssetDatabase.GetAssetPath(property.objectReferenceValue);
        var prefab = property.objectReferenceValue as GameObject;

        Assert.That(prefabPath, Does.EndWith(".prefab").IgnoreCase, propertyName);
        Assert.That(prefabPath, Does.StartWith(courseRoot), propertyName);
        Assert.That(prefab, Is.Not.Null, propertyName);
        Assert.That(PrefabUtility.GetPrefabAssetType(prefab), Is.EqualTo(PrefabAssetType.Variant), propertyName);

        return prefab;
    }

    private static void AssertObstaclePrefabContract(GameObject prefab, string description)
    {
        var collider = prefab.GetComponent<BoxCollider>();
        var runContact = prefab.GetComponent<RunContact>();

        Assert.That(collider, Is.Not.Null, description);
        Assert.That(collider.enabled, Is.True, description);
        Assert.That(collider.isTrigger, Is.False, description);
        Assert.That(runContact, Is.Not.Null, description);
        Assert.That(runContact.Category, Is.EqualTo(RunContactCategory.Obstacle), description);
    }

    private static void AssertPropPrefabContract(GameObject prefab, string description)
    {
        var activeColliders = prefab.GetComponentsInChildren<Collider>(true)
            .Where(collider => collider.enabled)
            .ToArray();
        var runContacts = prefab.GetComponentsInChildren<RunContact>(true);

        Assert.That(FindDirectChild(prefab.transform, "Visual"), Is.Not.Null, description);
        Assert.That(activeColliders, Is.Empty, description);
        Assert.That(runContacts, Is.Empty, description);
    }

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private Scene OpenGameplayScene()
    {
        return EditorSceneManager.OpenScene(TestAssets.GameplaySceneRef.Path, OpenSceneMode.Single);
    }

    private static MonoBehaviour FindLadybugHalfTubeAuthoring(Scene scene)
    {
        var authoring = FindSceneComponents<MonoBehaviour>(scene)
            .SingleOrDefault(component => component != null && component.GetType().FullName == LadybugHalfTubeAuthoringTypeName);

        Assert.That(authoring, Is.Not.Null);
        return authoring;
    }

    private static string GetAuthoringFeatureRoot(MonoBehaviour authoring)
    {
        var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(authoring));
        var runtimeFolderIndex = scriptPath.IndexOf("/Runtime/", StringComparison.Ordinal);

        Assert.That(runtimeFolderIndex, Is.GreaterThan(0), scriptPath);

        return scriptPath[..(runtimeFolderIndex + 1)];
    }

    private static IEnumerable<T> FindSceneComponents<T>(Scene scene)
    {
        return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
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
