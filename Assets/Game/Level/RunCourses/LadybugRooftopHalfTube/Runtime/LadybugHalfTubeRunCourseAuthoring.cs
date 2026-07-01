using System;
using Game.Gameplay;
using SaintsField;
using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    [DefaultExecutionOrder(-6000)]
    public sealed partial class LadybugHalfTubeRunCourseAuthoring : MonoBehaviour, IGameplayScenePreCompositionStep
    {
        [SerializeField, Layer] private string _cameraTerrainLayerName = "CameraTerrain";

        private bool _isValidated;

        private void Awake()
        {
            PrepareGameplaySceneComposition();
        }

        public void PrepareGameplaySceneComposition()
        {
            if (_isValidated)
                return;

            EnsureAcceptanceProfile();
            ValidateAuthoredCourse();
            _isValidated = true;
        }

        private void ValidateAuthoredCourse()
        {
            var cameraTerrainLayer = ResolveRequiredLayer(_cameraTerrainLayerName, "surfaces");
            var surfaceCount = 0;

            foreach (var terrainCollider in GetComponentsInChildren<TerrainCollider>(true))
            {
                ValidateSurfaceObject(terrainCollider, cameraTerrainLayer);
                surfaceCount += 1;
            }

            if (surfaceCount == 0)
                throw new InvalidOperationException(
                    "Ladybug half-tube course must contain at least one authored TerrainCollider child before entering Play Mode.");
        }

        private static void ValidateSurfaceObject(TerrainCollider terrainCollider, int expectedLayer)
        {
            var surfaceObject = terrainCollider.gameObject;
            var terrain = surfaceObject.GetComponent<Terrain>();
            var runContact = surfaceObject.GetComponent<RunContact>();

            if (surfaceObject.layer != expectedLayer)
                throw new InvalidOperationException(
                    $"Ladybug half-tube terrain section '{surfaceObject.name}' must use the CameraTerrain layer.");

            if (terrain == null)
                throw new InvalidOperationException(
                    $"Ladybug half-tube terrain section '{surfaceObject.name}' requires a Terrain component.");

            if (terrain.terrainData == null)
                throw new InvalidOperationException(
                    $"Ladybug half-tube terrain section '{surfaceObject.name}' requires TerrainData on its Terrain component.");

            if (terrainCollider.terrainData == null)
                throw new InvalidOperationException(
                    $"Ladybug half-tube terrain section '{surfaceObject.name}' requires TerrainData on its TerrainCollider.");

            if (terrainCollider.terrainData != terrain.terrainData)
                throw new InvalidOperationException(
                    $"Ladybug half-tube terrain section '{surfaceObject.name}' must share TerrainData between Terrain and TerrainCollider.");

            if (runContact == null)
                throw new InvalidOperationException(
                    $"Ladybug half-tube terrain section '{surfaceObject.name}' requires a RunContact component.");

            if (runContact.Category != RunContactCategory.Surface)
                throw new InvalidOperationException(
                    $"Ladybug half-tube terrain section '{surfaceObject.name}' RunContact must be Surface.");
        }

        private static int ResolveRequiredLayer(string layerName, string layerPurpose)
        {
            var layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
                throw new InvalidOperationException($"Ladybug half-tube {layerPurpose} require Unity layer '{layerName}'.");

            return layer;
        }
    }
}
