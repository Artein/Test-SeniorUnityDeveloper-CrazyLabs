using System;
using System.Linq;
using Game.Utils.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotBandShapeProvider
    {
        int BandShapePointCount { get; }
        bool TryCreateBandShape(SlingshotBandShapeQuery query, Vector3[] outputPoints, out int pointCount);
    }

    public sealed class SlingshotBandShapeProvider : ISlingshotBandShapeProvider
    {
        private readonly ILaunchTargetSilhouetteSource _silhouetteSource;
        private readonly ISlingshotConfig _config;
        private readonly PullPlaneTautBandSolver _solver;
        private readonly Vector3[] _worldSilhouetteSamples;
        private readonly float2[] _solverSilhouetteSamples;
        private readonly float2[] _solverOutputPoints;

        public int BandShapePointCount { get; }

        public SlingshotBandShapeProvider(ILaunchTargetSilhouetteSource silhouetteSource, ISlingshotConfig config)
        {
            _silhouetteSource = silhouetteSource ?? throw new ArgumentNullException(nameof(silhouetteSource));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ValidateConfig(config);

            BandShapePointCount = config.BandWrapSampleCount + 4;
            _worldSilhouetteSamples = new Vector3[config.BandSilhouetteSampleCount];
            _solverSilhouetteSamples = new float2[config.BandSilhouetteSampleCount];
            _solverOutputPoints = new float2[BandShapePointCount];
            _solver = new PullPlaneTautBandSolver(config.BandSilhouetteSampleCount, config.BandWrapSampleCount);
        }

        public bool TryCreateBandShape(SlingshotBandShapeQuery query, Vector3[] outputPoints, out int pointCount)
        {
            pointCount = 0;

            if (outputPoints is null)
                throw new ArgumentNullException(nameof(outputPoints));

            if (outputPoints.Length < BandShapePointCount)
                throw new ArgumentException("Output buffer is too small for the Slingshot Band Shape.", nameof(outputPoints));

            if (!IsValidQuery(query))
                throw new ArgumentException("Invalid Slingshot Band Shape query.", nameof(query));

            var silhouetteQuery = new LaunchTargetSilhouetteQuery(
                query.RestPoint,
                query.LaunchFrameRight,
                query.LaunchFrameForward,
                query.LaunchFrameUp,
                _config.BandSilhouetteSampleCount);

            if (!_silhouetteSource.TryWriteSilhouetteSamples(silhouetteQuery, _worldSilhouetteSamples, out var silhouetteSampleCount))
                return false;

            if (silhouetteSampleCount < 3 || silhouetteSampleCount > _worldSilhouetteSamples.Length)
                return false;

            for (var sampleIndex = 0; sampleIndex < silhouetteSampleCount; sampleIndex += 1)
            {
                var sample = _worldSilhouetteSamples[sampleIndex];

                if (!sample.IsFinite())
                    return false;

                _solverSilhouetteSamples[sampleIndex] = ToPullPlane(sample, query);
            }

            var solved = _solver.TrySolve(
                ToPullPlane(query.LeftAnchorPosition, query),
                ToPullPlane(query.RightAnchorPosition, query),
                ToPullPlane(query.PullPoint, query),
                _solverSilhouetteSamples,
                silhouetteSampleCount,
                _config.BandContactPadding,
                _config.BandWrapSampleCount,
                _solverOutputPoints,
                out var solverPointCount);

            if (!solved || solverPointCount != BandShapePointCount)
                return false;

            for (var pointIndex = 0; pointIndex < solverPointCount; pointIndex += 1)
            {
                outputPoints[pointIndex] = ToWorld(_solverOutputPoints[pointIndex], query);
            }

            pointCount = solverPointCount;
            return true;
        }

        private void ValidateConfig(ISlingshotConfig config)
        {
            var validator = new SlingshotConfigValidator();
            var errors = validator.Validate(config).ToList();

            if (errors.Count <= 0)
                return;

            throw new ArgumentException("Invalid Slingshot config: " + string.Join(" ", errors), nameof(config));
        }

        private bool IsValidQuery(SlingshotBandShapeQuery query)
        {
            return query.LeftAnchorPosition.IsFinite()
                   && query.RightAnchorPosition.IsFinite()
                   && query.RestPoint.IsFinite()
                   && query.PullPoint.IsFinite()
                   && query.LaunchFrameRight.IsFinite()
                   && query.LaunchFrameForward.IsFinite()
                   && query.LaunchFrameUp.IsFinite()
                   && query.LaunchFrameRight.IsApproximatelyUnit()
                   && query.LaunchFrameForward.IsApproximatelyUnit()
                   && query.LaunchFrameUp.IsApproximatelyUnit();
        }

        private float2 ToPullPlane(Vector3 point, SlingshotBandShapeQuery query)
        {
            var delta = point - query.RestPoint;

            return new float2(
                Vector3.Dot(delta, query.LaunchFrameRight),
                Vector3.Dot(delta, -query.LaunchFrameForward));
        }

        private Vector3 ToWorld(float2 point, SlingshotBandShapeQuery query)
        {
            return query.RestPoint
                   + (query.LaunchFrameRight * point.x)
                   - (query.LaunchFrameForward * point.y);
        }
    }
}
