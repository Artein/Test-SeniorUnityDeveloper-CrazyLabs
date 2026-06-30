using UnityEngine;

namespace Game.Level.RunCourses.LadybugRooftopHalfTube
{
    internal sealed class LadybugHalfTubeSurfaceMeshBuilder
    {
        public Mesh CreateHalfTubeMesh(LadybugHalfTubeSectionSpec section, float startHeight)
        {
            var endHeight = startHeight - Mathf.Tan(section.PitchDegrees * Mathf.Deg2Rad) * section.Length;
            var lipHeight = Mathf.Tan(section.BankDegrees * Mathf.Deg2Rad) * section.BankWidth;

            var mesh = new Mesh
            {
                name = section.Name
            };

            mesh.vertices = CreateVertices(section, startHeight, endHeight, lipHeight);
            mesh.triangles = CreateTriangles();
            mesh.uv = CreateUvs();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public Mesh CreateRequiredTutorialRampMesh(float startHeight)
        {
            var startProgress = 170f;
            var takeoffEndProgress = 182f;
            var endProgress = 200f;
            var innerHalfWidth = 3f;
            var outerHalfWidth = 5f;
            var lipHeight = Mathf.Tan(30f * Mathf.Deg2Rad) * (outerHalfWidth - innerHalfWidth);
            var takeoffEndHeight = GetRequiredTutorialRampHeight(startHeight, takeoffEndProgress);
            var endHeight = GetRequiredTutorialRampHeight(startHeight, endProgress);

            var mesh = new Mesh
            {
                name = "Band 3 Section 08 Required Tutorial Ramp Surface"
            };

            mesh.vertices = CreateSegmentedVertices(
                new[] { startProgress, takeoffEndProgress, endProgress },
                new[] { startHeight, takeoffEndHeight, endHeight },
                lipHeight,
                innerHalfWidth,
                outerHalfWidth);
            mesh.triangles = CreateSegmentedTriangles(3);
            mesh.uv = CreateSegmentedUvs(3);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public Mesh CreateOptionalBankRampMesh(float bankLineStartHeight)
        {
            var startProgress = 310f;
            var takeoffEndProgress = 326f;
            var endProgress = 350f;
            var innerLateralPosition = 3f;
            var outerLateralPosition = 5f;
            var takeoffEndHeight = GetOptionalBankRampHeight(bankLineStartHeight, takeoffEndProgress);
            var endHeight = GetOptionalBankRampHeight(bankLineStartHeight, endProgress);

            var mesh = new Mesh
            {
                name = "Band 4 Section 13 Optional Bank Ramp Surface"
            };

            mesh.vertices = CreateSideStripVertices(
                new[] { startProgress, takeoffEndProgress, endProgress },
                new[] { GetOptionalBankRampHeight(bankLineStartHeight, startProgress), takeoffEndHeight, endHeight },
                innerLateralPosition,
                outerLateralPosition);
            mesh.triangles = CreateSideStripTriangles(3);
            mesh.uv = CreateSideStripUvs(3);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public float GetRequiredTutorialRampHeight(float startHeight, float progress)
        {
            var takeoffStartProgress = 170f;
            var takeoffEndProgress = 182f;
            var takeoffPitchDegrees = 11f;
            var landingPitchDegrees = 5.5f;
            var clampedProgress = Mathf.Clamp(progress, takeoffStartProgress, 200f);

            var takeoffEndHeight = startHeight -
                                   Mathf.Tan(takeoffPitchDegrees * Mathf.Deg2Rad) *
                                   (takeoffEndProgress - takeoffStartProgress);

            if (clampedProgress <= takeoffEndProgress)
                return startHeight -
                       Mathf.Tan(takeoffPitchDegrees * Mathf.Deg2Rad) *
                       (clampedProgress - takeoffStartProgress);

            return takeoffEndHeight -
                   Mathf.Tan(landingPitchDegrees * Mathf.Deg2Rad) *
                   (clampedProgress - takeoffEndProgress);
        }

        public float GetOptionalBankRampHeight(float bankLineStartHeight, float progress)
        {
            var takeoffStartProgress = 310f;
            var takeoffEndProgress = 326f;
            var takeoffPitchDegrees = 9f;
            var landingPitchDegrees = 5.5f;
            var startLift = 1.25f;
            var clampedProgress = Mathf.Clamp(progress, takeoffStartProgress, 350f);
            var liftedStartHeight = bankLineStartHeight + startLift;

            var takeoffEndHeight = liftedStartHeight -
                                   Mathf.Tan(takeoffPitchDegrees * Mathf.Deg2Rad) *
                                   (takeoffEndProgress - takeoffStartProgress);

            if (clampedProgress <= takeoffEndProgress)
                return liftedStartHeight -
                       Mathf.Tan(takeoffPitchDegrees * Mathf.Deg2Rad) *
                       (clampedProgress - takeoffStartProgress);

            return takeoffEndHeight -
                   Mathf.Tan(landingPitchDegrees * Mathf.Deg2Rad) *
                   (clampedProgress - takeoffEndProgress);
        }

        private Vector3[] CreateVertices(LadybugHalfTubeSectionSpec section, float startHeight, float endHeight, float lipHeight)
        {
            var startProgress = section.StartProgress;
            var endProgress = section.EndProgress;
            var innerHalfWidth = section.InnerHalfWidth;
            var outerHalfWidth = section.OuterHalfWidth;

            return new[]
            {
                new Vector3(-outerHalfWidth, startHeight + lipHeight, startProgress),
                new Vector3(-innerHalfWidth, startHeight, startProgress),
                new Vector3(innerHalfWidth, startHeight, startProgress),
                new Vector3(outerHalfWidth, startHeight + lipHeight, startProgress),
                new Vector3(-outerHalfWidth, endHeight + lipHeight, endProgress),
                new Vector3(-innerHalfWidth, endHeight, endProgress),
                new Vector3(innerHalfWidth, endHeight, endProgress),
                new Vector3(outerHalfWidth, endHeight + lipHeight, endProgress)
            };
        }

        private int[] CreateTriangles()
        {
            return new[]
            {
                0, 4, 1,
                1, 4, 5,
                1, 5, 2,
                2, 5, 6,
                2, 6, 3,
                3, 6, 7
            };
        }

        private Vector2[] CreateUvs()
        {
            return new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0.2f, 0f),
                new Vector2(0.8f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0.2f, 1f),
                new Vector2(0.8f, 1f),
                new Vector2(1f, 1f)
            };
        }

        private Vector3[] CreateSegmentedVertices(
            float[] progresses,
            float[] centerHeights,
            float lipHeight,
            float innerHalfWidth,
            float outerHalfWidth)
        {
            var vertices = new Vector3[progresses.Length * 4];

            for (var stationIndex = 0; stationIndex < progresses.Length; stationIndex += 1)
            {
                var vertexOffset = stationIndex * 4;
                var progress = progresses[stationIndex];
                var centerHeight = centerHeights[stationIndex];

                vertices[vertexOffset] = new Vector3(-outerHalfWidth, centerHeight + lipHeight, progress);
                vertices[vertexOffset + 1] = new Vector3(-innerHalfWidth, centerHeight, progress);
                vertices[vertexOffset + 2] = new Vector3(innerHalfWidth, centerHeight, progress);
                vertices[vertexOffset + 3] = new Vector3(outerHalfWidth, centerHeight + lipHeight, progress);
            }

            return vertices;
        }

        private int[] CreateSegmentedTriangles(int stationCount)
        {
            var triangles = new int[(stationCount - 1) * 18];
            var triangleIndex = 0;

            for (var stationIndex = 0; stationIndex < stationCount - 1; stationIndex += 1)
            {
                var startVertex = stationIndex * 4;
                var nextVertex = (stationIndex + 1) * 4;

                triangles[triangleIndex] = startVertex;
                triangles[triangleIndex + 1] = nextVertex;
                triangles[triangleIndex + 2] = startVertex + 1;
                triangles[triangleIndex + 3] = startVertex + 1;
                triangles[triangleIndex + 4] = nextVertex;
                triangles[triangleIndex + 5] = nextVertex + 1;
                triangles[triangleIndex + 6] = startVertex + 1;
                triangles[triangleIndex + 7] = nextVertex + 1;
                triangles[triangleIndex + 8] = startVertex + 2;
                triangles[triangleIndex + 9] = startVertex + 2;
                triangles[triangleIndex + 10] = nextVertex + 1;
                triangles[triangleIndex + 11] = nextVertex + 2;
                triangles[triangleIndex + 12] = startVertex + 2;
                triangles[triangleIndex + 13] = nextVertex + 2;
                triangles[triangleIndex + 14] = startVertex + 3;
                triangles[triangleIndex + 15] = startVertex + 3;
                triangles[triangleIndex + 16] = nextVertex + 2;
                triangles[triangleIndex + 17] = nextVertex + 3;
                triangleIndex += 18;
            }

            return triangles;
        }

        private Vector2[] CreateSegmentedUvs(int stationCount)
        {
            var uvs = new Vector2[stationCount * 4];

            for (var stationIndex = 0; stationIndex < stationCount; stationIndex += 1)
            {
                var vertexOffset = stationIndex * 4;
                var progressPosition = stationIndex / (stationCount - 1f);

                uvs[vertexOffset] = new Vector2(0f, progressPosition);
                uvs[vertexOffset + 1] = new Vector2(0.2f, progressPosition);
                uvs[vertexOffset + 2] = new Vector2(0.8f, progressPosition);
                uvs[vertexOffset + 3] = new Vector2(1f, progressPosition);
            }

            return uvs;
        }

        private Vector3[] CreateSideStripVertices(
            float[] progresses,
            float[] heights,
            float innerLateralPosition,
            float outerLateralPosition)
        {
            var vertices = new Vector3[progresses.Length * 2];

            for (var stationIndex = 0; stationIndex < progresses.Length; stationIndex += 1)
            {
                var vertexOffset = stationIndex * 2;
                var progress = progresses[stationIndex];
                var height = heights[stationIndex];

                vertices[vertexOffset] = new Vector3(innerLateralPosition, height, progress);
                vertices[vertexOffset + 1] = new Vector3(outerLateralPosition, height, progress);
            }

            return vertices;
        }

        private int[] CreateSideStripTriangles(int stationCount)
        {
            var triangles = new int[(stationCount - 1) * 6];
            var triangleIndex = 0;

            for (var stationIndex = 0; stationIndex < stationCount - 1; stationIndex += 1)
            {
                var startVertex = stationIndex * 2;
                var nextVertex = (stationIndex + 1) * 2;

                triangles[triangleIndex] = startVertex;
                triangles[triangleIndex + 1] = nextVertex;
                triangles[triangleIndex + 2] = startVertex + 1;
                triangles[triangleIndex + 3] = startVertex + 1;
                triangles[triangleIndex + 4] = nextVertex;
                triangles[triangleIndex + 5] = nextVertex + 1;
                triangleIndex += 6;
            }

            return triangles;
        }

        private Vector2[] CreateSideStripUvs(int stationCount)
        {
            var uvs = new Vector2[stationCount * 2];

            for (var stationIndex = 0; stationIndex < stationCount; stationIndex += 1)
            {
                var vertexOffset = stationIndex * 2;
                var progressPosition = stationIndex / (stationCount - 1f);

                uvs[vertexOffset] = new Vector2(0f, progressPosition);
                uvs[vertexOffset + 1] = new Vector2(1f, progressPosition);
            }

            return uvs;
        }
    }

    internal readonly struct LadybugHalfTubeSectionSpec
    {
        public LadybugHalfTubeSectionSpec(string name, float startProgress, float length, float pitchDegrees, float bankDegrees)
        {
            Name = name;
            StartProgress = startProgress;
            Length = length;
            PitchDegrees = pitchDegrees;
            BankDegrees = bankDegrees;
        }

        public string Name { get; }
        public float StartProgress { get; }
        public float Length { get; }
        public float PitchDegrees { get; }
        public float BankDegrees { get; }
        public float EndProgress => StartProgress + Length;
        public float InnerHalfWidth => 3f;
        public float OuterHalfWidth => 5f;
        public float BankWidth => OuterHalfWidth - InnerHalfWidth;
    }
}
