using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private readonly int _gameplaySceneBuildIndex = 0;
    private readonly string _cameraObstacleLayerName = "CameraObstacle";
    private readonly string _cameraTerrainLayerName = "CameraTerrain";
    private readonly string _courseRootName = "Ladybug Rooftop Half-Tube Run Course";
    private readonly string _pickupLayerName = "Pickup";

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_LadybugCourseRuntimeContractsAreValid()
    {
        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield return null;

        Physics.SyncTransforms();

        var scene = SceneManager.GetActiveScene();
        var courseRoot = FindGameObjectByName(scene, _courseRootName);
        var cameraObstacleLayer = GetRequiredLayer(_cameraObstacleLayerName);
        var cameraTerrainLayer = GetRequiredLayer(_cameraTerrainLayerName);
        var pickupLayer = GetRequiredLayer(_pickupLayerName);

        AssertTerrainSurfaceContracts(scene, courseRoot, cameraTerrainLayer);
        AssertAuthoredMeshSurfaceContracts(scene, courseRoot, cameraTerrainLayer);
        AssertObstacleContracts(scene, courseRoot, cameraObstacleLayer);
        AssertPickupContracts(scene, courseRoot, pickupLayer);
        AssertSingleRunFinishContact(scene, courseRoot);
        AssertCourseRendererMaterialsAreValid(courseRoot);
        AssertNoGeneratedMeshCourseSections(scene, courseRoot);
    }
}
