using System;
using System.Collections.Generic;
using Game.Gameplay;
using SaintsField;
using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    [DefaultExecutionOrder(-6000)]
    public sealed partial class LadybugHalfTubeRunCourseAuthoring : MonoBehaviour, IGameplayScenePreCompositionStep
    {
        [SerializeField] private Material _surfaceMaterial;
        [SerializeField] private bool _buildOnAwake = true;
        [SerializeField, Layer] private string _cameraTerrainLayerName = "CameraTerrain";
        [SerializeField, Layer] private string _cameraObstacleLayerName = "CameraObstacle";

        private const float EarlyReachPressureSurfaceFriction = 0.138f;
        private const float CompletionGlideSurfaceFriction = 0.09f;

        private readonly List<Mesh> _generatedMeshes = new();
        private readonly LadybugHalfTubeSurfaceMeshBuilder _meshBuilder = new();
        private PhysicsMaterial _earlyReachPressurePhysicsMaterial;
        private PhysicsMaterial _completionGlidePhysicsMaterial;
        private bool _isBuilt;

        private void Awake()
        {
            if (_buildOnAwake)
                PrepareGameplaySceneComposition();
        }

        public void PrepareGameplaySceneComposition()
        {
            if (_isBuilt)
                return;

            BuildCourse();
            _isBuilt = true;
        }

        private void OnDestroy()
        {
            for (var meshIndex = 0; meshIndex < _generatedMeshes.Count; meshIndex += 1)
            {
                if (_generatedMeshes[meshIndex] != null)
                    Destroy(_generatedMeshes[meshIndex]);
            }

            _generatedMeshes.Clear();

            if (_earlyReachPressurePhysicsMaterial != null)
                Destroy(_earlyReachPressurePhysicsMaterial);

            if (_completionGlidePhysicsMaterial != null)
                Destroy(_completionGlidePhysicsMaterial);

            _earlyReachPressurePhysicsMaterial = null;
            _completionGlidePhysicsMaterial = null;
            _isBuilt = false;
        }

        private void BuildCourse()
        {
            EnsureAcceptanceProfile();

            var cameraTerrainLayer = ResolveRequiredLayer(_cameraTerrainLayerName, "surfaces");
            var cameraObstacleLayer = ResolveRequiredLayer(_cameraObstacleLayerName, "obstacles");
            var preRampSections = CreatePreRampSections();
            var postRampSections = CreatePostRampSections();

            var rampStartHeight = CreateHalfTubeSections(
                cameraTerrainLayer,
                preRampSections,
                0f,
                CourseSurfaceFrictionProfile.EarlyReachPressure);
            var rampMesh = _meshBuilder.CreateRequiredTutorialRampMesh(rampStartHeight);

            var rampObject = CreateSectionObject(
                "Band 3 Section 08 Required Tutorial Ramp Surface",
                cameraTerrainLayer,
                rampMesh,
                CourseSurfaceFrictionProfile.CompletionGlide);
            var postRampStartHeight = _meshBuilder.GetRequiredTutorialRampHeight(rampStartHeight, 200f);
            var surfaceContext = new CourseSurfaceContext(preRampSections, postRampSections, rampStartHeight, postRampStartHeight);

            _generatedMeshes.Add(rampMesh);
            rampObject.transform.SetParent(transform, false);

            CreateHalfTubeSections(
                cameraTerrainLayer,
                postRampSections,
                postRampStartHeight,
                CourseSurfaceFrictionProfile.CompletionGlide);
            CreateOptionalBankRamp(cameraTerrainLayer, surfaceContext);
            CreateCoursePickupLines(surfaceContext);
            CreateBandTwoObstacles(surfaceContext, cameraObstacleLayer);
            CreateBandThreeObstacles(surfaceContext, cameraObstacleLayer);
            CreateBandFourObstacles(surfaceContext, cameraObstacleLayer);
            CreateBandFiveObstacles(surfaceContext, cameraObstacleLayer);
            CreateRunFinish(surfaceContext);
            CreateRunFinishVisualMarker(surfaceContext);
            CreateRooftopVisualDressing(surfaceContext);
        }

        private int ResolveRequiredLayer(string layerName, string layerPurpose)
        {
            var layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
                throw new InvalidOperationException($"Ladybug half-tube {layerPurpose} require Unity layer '{layerName}'.");

            return layer;
        }

        private float CreateHalfTubeSections(
            int cameraTerrainLayer,
            LadybugHalfTubeSectionSpec[] sections,
            float startHeight,
            CourseSurfaceFrictionProfile frictionProfile)
        {
            var currentHeight = startHeight;

            for (var sectionIndex = 0; sectionIndex < sections.Length; sectionIndex += 1)
            {
                var section = sections[sectionIndex];
                var mesh = _meshBuilder.CreateHalfTubeMesh(section, currentHeight);
                var sectionObject = CreateSectionObject(section.Name, cameraTerrainLayer, mesh, frictionProfile);

                _generatedMeshes.Add(mesh);
                sectionObject.transform.SetParent(transform, false);
                currentHeight -= Mathf.Tan(section.PitchDegrees * Mathf.Deg2Rad) * section.Length;
            }

            return currentHeight;
        }

        private LadybugHalfTubeSectionSpec[] CreatePreRampSections()
        {
            return new[]
            {
                new LadybugHalfTubeSectionSpec("Band 1 Section 01 Shallow Settle Surface", 0f, 20f, 4f, 30f),
                new LadybugHalfTubeSectionSpec("Band 1 Section 02 Gentle S Surface", 20f, 25f, 6f, 30f),
                new LadybugHalfTubeSectionSpec("Band 1 Section 03 Low Bank Touch Surface", 45f, 25f, 6f, 30f),
                new LadybugHalfTubeSectionSpec("Band 2 Section 04 First Choice Surface", 70f, 25f, 6.5f, 30f),
                new LadybugHalfTubeSectionSpec("Band 2 Section 05 Recovery Trough Surface", 95f, 25f, 5.5f, 30f),
                new LadybugHalfTubeSectionSpec("Band 2 Section 06 Offset Choice Surface", 120f, 30f, 7.5f, 30f),
                new LadybugHalfTubeSectionSpec("Band 3 Section 07 Required Ramp Setup Surface", 150f, 20f, 9f, 30f)
            };
        }

        private LadybugHalfTubeSectionSpec[] CreatePostRampSections()
        {
            return new[]
            {
                new LadybugHalfTubeSectionSpec("Band 3 Section 09 Landing Recovery Surface", 200f, 25f, 6.5f, 30f),
                new LadybugHalfTubeSectionSpec("Band 3 Section 10 Bank Pressure Surface", 225f, 25f, 7f, 30f),
                new LadybugHalfTubeSectionSpec("Band 4 Section 11 Transfer Lines Surface", 250f, 30f, 6.5f, 30f),
                new LadybugHalfTubeSectionSpec("Band 4 Section 12 Reach Pressure Glide Surface", 280f, 30f, 4f, 30f),
                new LadybugHalfTubeSectionSpec("Band 4 Section 13 Center Bypass Surface", 310f, 40f, 5.5f, 30f),
                new LadybugHalfTubeSectionSpec("Band 5 Section 14 Finish Approach Surface", 350f, 40f, 6.5f, 30f),
                new LadybugHalfTubeSectionSpec("Band 5 Section 15 Final Funnel Surface", 390f, 30f, 5.5f, 30f)
            };
        }

        private void CreateOptionalBankRamp(int cameraTerrainLayer, CourseSurfaceContext surfaceContext)
        {
            var bankLineStartHeight = GetSurfaceHeightAtPosition(surfaceContext, 4f, 310f);
            var optionalRampMesh = _meshBuilder.CreateOptionalBankRampMesh(bankLineStartHeight);

            var optionalRampObject = CreateSectionObject(
                "Band 4 Section 13 Optional Bank Ramp Surface",
                cameraTerrainLayer,
                optionalRampMesh,
                CourseSurfaceFrictionProfile.CompletionGlide);

            _generatedMeshes.Add(optionalRampMesh);
            optionalRampObject.transform.SetParent(transform, false);
        }

        private void CreateBandTwoObstacles(CourseSurfaceContext surfaceContext, int cameraObstacleLayer)
        {
            var obstacleRoot = new GameObject("Band 2 Obstacles");
            var obstacles = CreateBandTwoObstacleSpecs();

            obstacleRoot.transform.SetParent(transform, false);

            for (var obstacleIndex = 0; obstacleIndex < obstacles.Length; obstacleIndex += 1)
            {
                var obstacleObject = CreateObstacleObject(obstacles[obstacleIndex], surfaceContext, cameraObstacleLayer);

                obstacleObject.transform.SetParent(obstacleRoot.transform, false);
            }
        }

        private ObstacleSpec[] CreateBandTwoObstacleSpecs()
        {
            return new[]
            {
                new ObstacleSpec(
                    "Band 2 Obstacle 01 Center AC Blocker",
                    0f,
                    82.5f,
                    2.5f,
                    1.6f,
                    3f,
                    ObstacleVisualKind.Ac1),
                new ObstacleSpec(
                    "Band 2 Obstacle 02 Left Offset Sunroof Blocker",
                    -2f,
                    135f,
                    2.4f,
                    1.2f,
                    4f,
                    ObstacleVisualKind.Sunroof)
            };
        }

        private void CreateBandThreeObstacles(CourseSurfaceContext surfaceContext, int cameraObstacleLayer)
        {
            var obstacleRoot = new GameObject("Band 3 Obstacles");
            var obstacles = CreateBandThreeObstacleSpecs();

            obstacleRoot.transform.SetParent(transform, false);

            for (var obstacleIndex = 0; obstacleIndex < obstacles.Length; obstacleIndex += 1)
            {
                var obstacleObject = CreateObstacleObject(obstacles[obstacleIndex], surfaceContext, cameraObstacleLayer);

                obstacleObject.transform.SetParent(obstacleRoot.transform, false);
            }
        }

        private ObstacleSpec[] CreateBandThreeObstacleSpecs()
        {
            return new[]
            {
                new ObstacleSpec(
                    "Band 3 Obstacle 03 Right Low Solar Blocker",
                    2.6f,
                    216f,
                    1.8f,
                    1f,
                    3.5f,
                    ObstacleVisualKind.SolarPanels),
                new ObstacleSpec(
                    "Band 3 Obstacle 04 Left Bank AC Blocker",
                    -3.3f,
                    238f,
                    1.5f,
                    1.1f,
                    4f,
                    ObstacleVisualKind.Ac2)
            };
        }

        private void CreateBandFourObstacles(CourseSurfaceContext surfaceContext, int cameraObstacleLayer)
        {
            var obstacleRoot = new GameObject("Band 4 Obstacles");
            var obstacles = CreateBandFourObstacleSpecs();

            obstacleRoot.transform.SetParent(transform, false);

            for (var obstacleIndex = 0; obstacleIndex < obstacles.Length; obstacleIndex += 1)
            {
                var obstacleObject = CreateObstacleObject(obstacles[obstacleIndex], surfaceContext, cameraObstacleLayer);

                obstacleObject.transform.SetParent(obstacleRoot.transform, false);
            }
        }

        private ObstacleSpec[] CreateBandFourObstacleSpecs()
        {
            return new[]
            {
                new ObstacleSpec(
                    "Band 4 Obstacle 05 Right Bank Billboard Blocker",
                    3.45f,
                    266f,
                    1.35f,
                    1.1f,
                    4f,
                    ObstacleVisualKind.Billboard)
            };
        }

        private void CreateBandFiveObstacles(CourseSurfaceContext surfaceContext, int cameraObstacleLayer)
        {
            var obstacleRoot = new GameObject("Band 5 Obstacles");
            var obstacles = CreateBandFiveObstacleSpecs();

            obstacleRoot.transform.SetParent(transform, false);

            for (var obstacleIndex = 0; obstacleIndex < obstacles.Length; obstacleIndex += 1)
            {
                var obstacleObject = CreateObstacleObject(obstacles[obstacleIndex], surfaceContext, cameraObstacleLayer);

                obstacleObject.transform.SetParent(obstacleRoot.transform, false);
            }
        }

        private ObstacleSpec[] CreateBandFiveObstacleSpecs()
        {
            return new[]
            {
                new ObstacleSpec(
                    "Band 5 Obstacle 06 Left Finish Approach Solar Blocker",
                    -3.4f,
                    366f,
                    1.35f,
                    1.1f,
                    4f,
                    ObstacleVisualKind.SolarPanels)
            };
        }

        private GameObject CreateObstacleObject(ObstacleSpec obstacle, CourseSurfaceContext surfaceContext, int cameraObstacleLayer)
        {
            var obstacleObject = new GameObject(obstacle.Name)
            {
                layer = cameraObstacleLayer
            };
            var collider = obstacleObject.AddComponent<BoxCollider>();
            var runContact = obstacleObject.AddComponent<RunContact>();
            var surfaceHeight = GetSurfaceHeightAtPosition(surfaceContext, obstacle.LateralPosition, obstacle.Progress);

            obstacleObject.transform.localPosition = new Vector3(
                obstacle.LateralPosition,
                surfaceHeight,
                obstacle.Progress);
            collider.size = new Vector3(obstacle.Width, obstacle.Height, obstacle.Depth);
            collider.center = new Vector3(0f, obstacle.Height * 0.5f, 0f);
            runContact.SetCategoryForCourseAuthoring(RunContactCategory.Obstacle);
            CreateObstacleVisual(obstacleObject.transform, collider, obstacle.VisualKind);

            return obstacleObject;
        }

        private float GetSurfaceHeightAtProgress(CourseSurfaceContext surfaceContext, float progress)
        {
            if (progress < 170f)
                return GetSectionSurfaceHeight(surfaceContext.PreRampSections, 0f, progress);

            if (progress <= 200f)
                return _meshBuilder.GetRequiredTutorialRampHeight(surfaceContext.RampStartHeight, progress);

            return GetSectionSurfaceHeight(surfaceContext.PostRampSections, surfaceContext.PostRampStartHeight, progress);
        }

        private float GetSurfaceHeightAtPosition(CourseSurfaceContext surfaceContext, float lateralPosition, float progress)
        {
            var centerHeight = GetSurfaceHeightAtProgress(surfaceContext, progress);
            var bankOffset = Mathf.Clamp(Mathf.Abs(lateralPosition) - 3f, 0f, 2f);

            return centerHeight + Mathf.Tan(30f * Mathf.Deg2Rad) * bankOffset;
        }

        private float GetSectionSurfaceHeight(LadybugHalfTubeSectionSpec[] sections, float firstSectionStartHeight, float progress)
        {
            var startHeight = firstSectionStartHeight;

            for (var sectionIndex = 0; sectionIndex < sections.Length; sectionIndex += 1)
            {
                var section = sections[sectionIndex];

                if (progress >= section.StartProgress && progress <= section.EndProgress)
                    return startHeight - Mathf.Tan(section.PitchDegrees * Mathf.Deg2Rad) * (progress - section.StartProgress);

                startHeight -= Mathf.Tan(section.PitchDegrees * Mathf.Deg2Rad) * section.Length;
            }

            return startHeight;
        }

        private GameObject CreateSectionObject(
            string sectionName,
            int terrainLayer,
            Mesh mesh,
            CourseSurfaceFrictionProfile frictionProfile)
        {
            var sectionObject = new GameObject(sectionName)
            {
                layer = terrainLayer
            };

            var meshFilter = sectionObject.AddComponent<MeshFilter>();
            var meshRenderer = sectionObject.AddComponent<MeshRenderer>();
            var meshCollider = sectionObject.AddComponent<MeshCollider>();
            var runContact = sectionObject.AddComponent<RunContact>();

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = _surfaceMaterial;
            meshCollider.sharedMesh = mesh;
            meshCollider.sharedMaterial = GetOrCreateCourseSurfacePhysicsMaterial(frictionProfile);
            runContact.SetCategoryForCourseAuthoring(RunContactCategory.Surface);

            return sectionObject;
        }

        private PhysicsMaterial GetOrCreateCourseSurfacePhysicsMaterial(CourseSurfaceFrictionProfile frictionProfile)
        {
            return frictionProfile == CourseSurfaceFrictionProfile.EarlyReachPressure
                ? GetOrCreateEarlyReachPressurePhysicsMaterial()
                : GetOrCreateCompletionGlidePhysicsMaterial();
        }

        private PhysicsMaterial GetOrCreateEarlyReachPressurePhysicsMaterial()
        {
            if (_earlyReachPressurePhysicsMaterial == null)
                _earlyReachPressurePhysicsMaterial = CreateCourseSurfacePhysicsMaterial(
                    "Ladybug Half-Tube Early Reach Pressure Surface",
                    EarlyReachPressureSurfaceFriction);

            return _earlyReachPressurePhysicsMaterial;
        }

        private PhysicsMaterial GetOrCreateCompletionGlidePhysicsMaterial()
        {
            if (_completionGlidePhysicsMaterial == null)
                _completionGlidePhysicsMaterial = CreateCourseSurfacePhysicsMaterial(
                    "Ladybug Half-Tube Completion Glide Surface",
                    CompletionGlideSurfaceFriction);

            return _completionGlidePhysicsMaterial;
        }

        private PhysicsMaterial CreateCourseSurfacePhysicsMaterial(string materialName, float friction)
        {
            return new PhysicsMaterial(materialName)
            {
                staticFriction = friction,
                dynamicFriction = friction,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
        }

        private void CreateRunFinish(CourseSurfaceContext surfaceContext)
        {
            var finishObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var collider = finishObject.GetComponent<BoxCollider>();
            var meshRenderer = finishObject.GetComponent<MeshRenderer>();
            var runContact = finishObject.AddComponent<RunContact>();
            var surfaceHeight = GetSurfaceHeightAtPosition(surfaceContext, 0f, 416f);

            finishObject.name = "Band 5 Run Finish";
            finishObject.transform.SetParent(transform, false);
            finishObject.transform.localPosition = new Vector3(0f, surfaceHeight + 1.2f, 416f);
            finishObject.transform.localScale = new Vector3(7f, 2.4f, 1.5f);
            collider.isTrigger = true;
            runContact.SetCategoryForCourseAuthoring(RunContactCategory.Finish);

            if (_surfaceMaterial != null)
                meshRenderer.sharedMaterial = _surfaceMaterial;
        }
    }
}
