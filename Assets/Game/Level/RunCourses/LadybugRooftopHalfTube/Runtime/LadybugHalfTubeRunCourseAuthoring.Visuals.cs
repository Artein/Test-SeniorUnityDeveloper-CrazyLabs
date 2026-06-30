using System;
using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    public sealed partial class LadybugHalfTubeRunCourseAuthoring
    {
        [SerializeField] private GameObject _rooftopChunk01Prefab;
        [SerializeField] private GameObject _rooftopChunk02Prefab;
        [SerializeField] private GameObject _rooftopChunk03DropPrefab;
        [SerializeField] private GameObject _rooftopChunk05StepPrefab;
        [SerializeField] private GameObject _obstacleAc1Prefab;
        [SerializeField] private GameObject _obstacleAc2Prefab;
        [SerializeField] private GameObject _obstacleSunroofPrefab;
        [SerializeField] private GameObject _obstacleSolarPanelsPrefab;
        [SerializeField] private GameObject _obstacleBillboardPrefab;
        [SerializeField] private GameObject _waterTankPropPrefab;
        [SerializeField] private GameObject _roofExitPropPrefab;
        [SerializeField] private GameObject _satDishPropPrefab;
        [SerializeField] private GameObject _rampPropPrefab;

        private void CreateRooftopVisualDressing(CourseSurfaceContext surfaceContext)
        {
            var dressingRoot = new GameObject("Rooftop Visual Dressing");
            dressingRoot.transform.SetParent(transform, false);

            CreateFittedVisual(
                _rooftopChunk01Prefab,
                "Rooftop_Chunk_01 Edge Visual 030L",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, -8.4f, 30f, -0.35f),
                Quaternion.identity,
                new Vector3(5f, 3.2f, 28f));

            CreateFittedVisual(
                _rooftopChunk02Prefab,
                "Rooftop_Chunk_02 Edge Visual 110R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 8.4f, 110f, -0.35f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(5f, 3.2f, 30f));

            CreateFittedVisual(
                _rooftopChunk03DropPrefab,
                "Rooftop_Chunk_03_Drop Edge Visual 190L",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, -8.5f, 190f, -0.45f),
                Quaternion.identity,
                new Vector3(5.5f, 3.6f, 30f));

            CreateFittedVisual(
                _rooftopChunk05StepPrefab,
                "Rooftop_Chunk_05_Step Edge Visual 330R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 8.5f, 330f, -0.45f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(5.5f, 3.6f, 30f));

            CreateFittedVisual(
                _waterTankPropPrefab,
                "Obstacle_WaterTank Rooftop Prop 300L",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, -7.3f, 300f, 0.8f),
                Quaternion.Euler(0f, 25f, 0f),
                new Vector3(2.4f, 3.2f, 2.4f));

            CreateFittedVisual(
                _satDishPropPrefab,
                "Obstacle_SatDish Rooftop Prop 145R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 7.3f, 145f, 0.6f),
                Quaternion.Euler(0f, -25f, 0f),
                new Vector3(2f, 2f, 2f));

            CreateFittedVisual(
                _roofExitPropPrefab,
                "Obstacle_RoofExit Rooftop Prop 255R",
                dressingRoot.transform,
                CreateRooftopVisualPosition(surfaceContext, 7.9f, 255f, 0.5f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(3f, 2.4f, 3f));

            CreateFittedVisual(
                _rampPropPrefab,
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

        private void FitObstacleVisualToCollider(
            Transform obstacleTransform,
            BoxCollider gameplayCollider,
            ObstacleVisualDefinition definition)
        {
            var visual = FindRequiredDirectChild(obstacleTransform, definition.Name);
            var targetBounds = gameplayCollider.bounds;
            var targetSize = targetBounds.size;

            targetSize.y = Mathf.Max(targetSize.y, 1.4f);
            visual.transform.localPosition = obstacleTransform.InverseTransformPoint(targetBounds.center);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            SetLayerRecursively(visual, 0);
            FitVisualToWorldBounds(visual, new Bounds(targetBounds.center, targetSize));
        }

        private ObstacleVisualDefinition GetObstacleVisualDefinition(ObstacleVisualKind visualKind)
        {
            switch (visualKind)
            {
                case ObstacleVisualKind.Ac1:
                    return new ObstacleVisualDefinition(_obstacleAc1Prefab, "Obstacle_AC1 Visual");
                case ObstacleVisualKind.Ac2:
                    return new ObstacleVisualDefinition(_obstacleAc2Prefab, "Obstacle_AC2 Visual");
                case ObstacleVisualKind.Sunroof:
                    return new ObstacleVisualDefinition(_obstacleSunroofPrefab, "Obstacle_SunRoof Visual");
                case ObstacleVisualKind.SolarPanels:
                    return new ObstacleVisualDefinition(_obstacleSolarPanelsPrefab, "Obstacle_SolarPanels Visual");
                case ObstacleVisualKind.Billboard:
                    return new ObstacleVisualDefinition(_obstacleBillboardPrefab, "Obstacle_Billboard Visual");
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
            var visualRoot = Instantiate(resolvedPrefab, parent, false);

            var fitTarget = TryFindDirectChild(visualRoot.transform, "Visual", out var visualChild)
                ? visualChild.gameObject
                : visualRoot;

            visualRoot.name = wrapperName;
            visualRoot.transform.localPosition = localPosition;
            visualRoot.transform.localRotation = localRotation;
            visualRoot.transform.localScale = Vector3.one;

            SetLayerRecursively(visualRoot, 0);
            FitVisualToWorldBounds(fitTarget, new Bounds(visualRoot.transform.position, targetSize));

            return visualRoot;
        }

        private GameObject ResolveVisualPrefab(GameObject prefab, string wrapperName)
        {
            if (prefab != null)
                return prefab;

            throw new InvalidOperationException($"Ladybug half-tube visual '{wrapperName}' requires an assigned prefab.");
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

        private GameObject FindRequiredDirectChild(Transform parent, string childName)
        {
            if (TryFindDirectChild(parent, childName, out var child))
                return child.gameObject;

            throw new InvalidOperationException($"Ladybug half-tube prefab '{parent.name}' requires a direct child named '{childName}'.");
        }

        private bool TryFindDirectChild(Transform parent, string childName, out Transform child)
        {
            for (var childIndex = 0; childIndex < parent.childCount; childIndex += 1)
            {
                var currentChild = parent.GetChild(childIndex);

                if (string.Equals(currentChild.name, childName, StringComparison.Ordinal))
                {
                    child = currentChild;
                    return true;
                }
            }

            child = null;
            return false;
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
