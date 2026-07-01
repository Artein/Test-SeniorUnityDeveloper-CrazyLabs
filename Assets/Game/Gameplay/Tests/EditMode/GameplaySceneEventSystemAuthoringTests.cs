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
    public void GameplayScene_WhenAuthored_UsesSerializedTerrainSectionsForLadybugRunCourse()
    {
        var scene = OpenGameplayScene();
        var authoring = FindLadybugHalfTubeAuthoring(scene);
        var courseRoot = GetAuthoringFeatureRoot(authoring);
        var cameraTerrainLayer = LayerMask.NameToLayer("CameraTerrain");

        Assert.That(cameraTerrainLayer, Is.GreaterThanOrEqualTo(0));

        var terrainColliders = authoring.GetComponentsInChildren<TerrainCollider>(true);

        Assert.That(
            terrainColliders,
            Has.Length.GreaterThan(0),
            "Ladybug course must contain at least one authored terrain surface.");

        foreach (var terrainCollider in terrainColliders)
        {
            Assert.That(terrainCollider.transform.IsChildOf(authoring.transform), Is.True, terrainCollider.name);
            AssertTerrainSurfaceContract(terrainCollider.gameObject, courseRoot, cameraTerrainLayer);
        }
    }

    private static void AssertTerrainSurfaceContract(GameObject surfaceObject, string courseRoot, int cameraTerrainLayer)
    {
        var surfaceName = surfaceObject.name;
        var terrain = surfaceObject.GetComponent<Terrain>();
        var terrainCollider = surfaceObject.GetComponent<TerrainCollider>();
        var runContact = surfaceObject.GetComponent<RunContact>();

        Assert.That(surfaceObject.layer, Is.EqualTo(cameraTerrainLayer), surfaceName);
        Assert.That(surfaceObject.GetComponent<MeshFilter>(), Is.Null, surfaceName);
        Assert.That(surfaceObject.GetComponent<MeshRenderer>(), Is.Null, surfaceName);
        Assert.That(surfaceObject.GetComponent<MeshCollider>(), Is.Null, surfaceName);
        Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(surfaceObject), Is.EqualTo(0), surfaceName);
        Assert.That(terrain, Is.Not.Null, surfaceName);
        Assert.That(terrainCollider, Is.Not.Null, surfaceName);
        Assert.That(terrainCollider.enabled, Is.True, surfaceName);
        Assert.That(runContact, Is.Not.Null, surfaceName);
        Assert.That(runContact.Category, Is.EqualTo(RunContactCategory.Surface), surfaceName);
        Assert.That(terrain.terrainData, Is.Not.Null, surfaceName);
        Assert.That(terrainCollider.terrainData, Is.SameAs(terrain.terrainData), surfaceName);

        AssertCourseOwnedAsset(terrain.terrainData, courseRoot, surfaceName);
        AssertCourseOwnedAsset(terrainCollider.sharedMaterial, courseRoot, surfaceName);
    }

    private static void AssertCourseOwnedAsset(UnityEngine.Object asset, string courseRoot, string description)
    {
        Assert.That(asset, Is.Not.Null, description);

        var assetPath = AssetDatabase.GetAssetPath(asset);

        Assert.That(assetPath, Does.StartWith(courseRoot), description);
        Assert.That(assetPath, Does.Not.StartWith("Assets/Plugins/"), description);
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
