using System;
using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    public sealed partial class LadybugHalfTubeRunCourseAuthoring
    {
        [SerializeField] private GameObject _rooftopChunk01VisualPrefab;
        [SerializeField] private GameObject _rooftopChunk02VisualPrefab;
        [SerializeField] private GameObject _rooftopChunk03DropVisualPrefab;
        [SerializeField] private GameObject _rooftopChunk05StepVisualPrefab;
        [SerializeField] private GameObject _obstacleAc1VisualPrefab;
        [SerializeField] private GameObject _obstacleAc2VisualPrefab;
        [SerializeField] private GameObject _obstacleSunroofVisualPrefab;
        [SerializeField] private GameObject _obstacleSolarPanelsVisualPrefab;
        [SerializeField] private GameObject _obstacleBillboardVisualPrefab;
        [SerializeField] private GameObject _obstacleWaterTankVisualPrefab;
        [SerializeField] private GameObject _obstacleRoofExitVisualPrefab;
        [SerializeField] private GameObject _obstacleSatDishVisualPrefab;
        [SerializeField] private GameObject _rampVisualPrefab;

        private void CreateRooftopVisualDressing(CourseSurfaceContext surfaceContext)
        {
            var dressingRoot = new GameObject("Rooftop Visual Dressing");
            dressingRoot.transform.SetParent(transform, false);

            CreateFittedVisual(
                _rooftopChunk01VisualPrefab,
                "Rooftop_Chunk_01 Edge Visual 030L",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, -8.4f, 30f, -0.35f),
                Quaternion.identity,
                new Vector3(5f, 3.2f, 28f));

            CreateFittedVisual(
                _rooftopChunk02VisualPrefab,
                "Rooftop_Chunk_02 Edge Visual 110R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 8.4f, 110f, -0.35f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(5f, 3.2f, 30f));

            CreateFittedVisual(
                _rooftopChunk03DropVisualPrefab,
                "Rooftop_Chunk_03_Drop Edge Visual 190L",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, -8.5f, 190f, -0.45f),
                Quaternion.identity,
                new Vector3(5.5f, 3.6f, 30f));

            CreateFittedVisual(
                _rooftopChunk05StepVisualPrefab,
                "Rooftop_Chunk_05_Step Edge Visual 330R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 8.5f, 330f, -0.45f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(5.5f, 3.6f, 30f));

            CreateFittedVisual(
                _obstacleWaterTankVisualPrefab,
                "Obstacle_WaterTank Rooftop Prop 300L",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, -7.3f, 300f, 0.8f),
                Quaternion.Euler(0f, 25f, 0f),
                new Vector3(2.4f, 3.2f, 2.4f));

            CreateFittedVisual(
                _obstacleSatDishVisualPrefab,
                "Obstacle_SatDish Rooftop Prop 145R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 7.3f, 145f, 0.6f),
                Quaternion.Euler(0f, -25f, 0f),
                new Vector3(2f, 2f, 2f));

            CreateFittedVisual(
                _obstacleRoofExitVisualPrefab,
                "Obstacle_RoofExit Rooftop Prop 255R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 7.9f, 255f, 0.5f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(3f, 2.4f, 3f));

            CreateFittedVisual(
                _rampVisualPrefab,
                "Ramp Rooftop Prop 050L",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, -7.6f, 50f, 0.2f),
                Quaternion.identity,
                new Vector3(3f, 1.6f, 4.5f));
        }

        private Vector3 CreateRooftopVisualPosition(CourseSurfaceContext surfaceContext, float lateralPosition, float progress, float verticalOffset)
        {
            var lipLateralPosition = lateralPosition < 0f ? -5f : 5f;
            var surfaceHeight = GetSurfaceHeightAtPosition(surfaceContext, lipLateralPosition, progress);

            return new Vector3(lateralPosition, surfaceHeight + verticalOffset, progress);
        }

        private void CreateObstacleVisual(Transform obstacleTransform, BoxCollider gameplayCollider, ObstacleVisualKind visualKind)
        {
            var definition = GetObstacleVisualDefinition(visualKind);
            var targetBounds = gameplayCollider.bounds;
            var targetSize = targetBounds.size;

            targetSize.y = Mathf.Max(targetSize.y, 1.4f);

            CreateFittedVisual(
                definition.Prefab,
                definition.Name,
                obstacleTransform,
                obstacleTransform.InverseTransformPoint(targetBounds.center),
                Quaternion.identity,
                targetSize);
        }

        private ObstacleVisualDefinition GetObstacleVisualDefinition(ObstacleVisualKind visualKind)
        {
            switch (visualKind)
            {
                case ObstacleVisualKind.Ac1:
                    return new ObstacleVisualDefinition(_obstacleAc1VisualPrefab, "Obstacle_AC1 Visual");
                case ObstacleVisualKind.Ac2:
                    return new ObstacleVisualDefinition(_obstacleAc2VisualPrefab, "Obstacle_AC2 Visual");
                case ObstacleVisualKind.Sunroof:
                    return new ObstacleVisualDefinition(_obstacleSunroofVisualPrefab, "Obstacle_SunRoof Visual");
                case ObstacleVisualKind.SolarPanels:
                    return new ObstacleVisualDefinition(_obstacleSolarPanelsVisualPrefab, "Obstacle_SolarPanels Visual");
                case ObstacleVisualKind.Billboard:
                    return new ObstacleVisualDefinition(_obstacleBillboardVisualPrefab, "Obstacle_Billboard Visual");
                default:
                    throw new ArgumentOutOfRangeException(nameof(visualKind), visualKind, "Unsupported Ladybug half-tube obstacle visual kind.");
            }
        }

        private void CreateRunFinishVisualMarker(CourseSurfaceContext surfaceContext)
        {
            var markerRoot = new GameObject("Band 5 Run Finish Visual Marker");
            var surfaceHeight = GetSurfaceHeightAtPosition(surfaceContext, 0f, 416f);

            markerRoot.transform.SetParent(transform, false);

            CreateVisualBox(
                markerRoot.transform,
                "Finish Marker Left Post",
                new Vector3(-3.7f, surfaceHeight + 1.55f, 416f),
                new Vector3(0.28f, 3.1f, 0.35f));

            CreateVisualBox(
                markerRoot.transform,
                "Finish Marker Right Post",
                new Vector3(3.7f, surfaceHeight + 1.55f, 416f),
                new Vector3(0.28f, 3.1f, 0.35f));

            CreateVisualBox(
                markerRoot.transform,
                "Finish Marker Top Bar",
                new Vector3(0f, surfaceHeight + 3f, 416f),
                new Vector3(8.2f, 0.28f, 0.35f));
        }

        private void CreateVisualBox(Transform parent, string boxName, Vector3 localPosition, Vector3 localScale)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var collider = box.GetComponent<Collider>();

            box.name = boxName;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localScale = localScale;
            box.layer = 0;

            if (collider != null)
                collider.enabled = false;
        }

        private GameObject CreateFittedVisual(
            GameObject prefab,
            string wrapperName,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 targetSize)
        {
            var resolvedPrefab = ResolveVisualPrefab(prefab, wrapperName);
            var wrapper = new GameObject(wrapperName);

            wrapper.transform.SetParent(parent, false);
            wrapper.transform.localPosition = localPosition;
            wrapper.transform.localRotation = Quaternion.identity;
            wrapper.transform.localScale = Vector3.one;

            var visual = Instantiate(resolvedPrefab, wrapper.transform, false);

            visual.name = $"{wrapperName} Mesh";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = localRotation;
            visual.transform.localScale = Vector3.one;

            SetLayerRecursively(wrapper, 0);
            DisableVisualPhysics(wrapper);
            FitVisualToWorldBounds(visual, new Bounds(wrapper.transform.position, targetSize));

            return wrapper;
        }

        private GameObject ResolveVisualPrefab(GameObject prefab, string wrapperName)
        {
            if (prefab != null)
                return prefab;

            throw new InvalidOperationException($"Ladybug half-tube visual '{wrapperName}' requires an assigned prefab.");
        }

        private void DisableVisualPhysics(GameObject root)
        {
            var colliders = root.GetComponentsInChildren<Collider>(true);

            for (var i = 0; i < colliders.Length; i += 1)
            {
                colliders[i].enabled = false;
            }
        }

        private void FitVisualToWorldBounds(GameObject visual, Bounds targetBounds)
        {
            if (!TryGetRendererBounds(visual, out var currentBounds))
                return;

            var scaleMultiplier = CalculateScaleMultiplier(currentBounds.size, targetBounds.size);

            visual.transform.localScale = new Vector3(
                visual.transform.localScale.x * scaleMultiplier.x,
                visual.transform.localScale.y * scaleMultiplier.y,
                visual.transform.localScale.z * scaleMultiplier.z);

            if (!TryGetRendererBounds(visual, out currentBounds))
                return;

            visual.transform.position += targetBounds.center - currentBounds.center;
        }

        private Vector3 CalculateScaleMultiplier(Vector3 currentSize, Vector3 targetSize)
        {
            return new Vector3(
                CalculateScaleMultiplier(currentSize.x, targetSize.x),
                CalculateScaleMultiplier(currentSize.y, targetSize.y),
                CalculateScaleMultiplier(currentSize.z, targetSize.z));
        }

        private float CalculateScaleMultiplier(float currentSize, float targetSize)
        {
            if (currentSize <= 0.001f || targetSize <= 0.001f)
                return 1f;

            return Mathf.Clamp(targetSize / currentSize, 0.01f, 100f);
        }

        private bool TryGetRendererBounds(GameObject root, out Bounds bounds)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var hasBounds = false;

            bounds = default;

            for (var i = 0; i < renderers.Length; i += 1)
            {
                var rend = renderers[i];

                if (!rend.enabled)
                    continue;

                if (!hasBounds)
                {
                    bounds = rend.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(rend.bounds);
            }

            return hasBounds;
        }

        private readonly struct ObstacleVisualDefinition
        {
            public GameObject Prefab { get; }
            public string Name { get; }

            public ObstacleVisualDefinition(GameObject prefab, string name)
            {
                Prefab = prefab;
                Name = name;
            }
        }
    }
}
