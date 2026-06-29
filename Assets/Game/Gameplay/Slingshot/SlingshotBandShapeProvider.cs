using System;
using System.Collections.Generic;
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

        bool TryCheckBandShapeClearance(
            SlingshotBandShapeQuery query,
            IReadOnlyList<Vector3> bandShapePoints,
            float clearanceRadius,
            out bool isClear);
    }

    internal interface ISlingshotBandShapeDepthProvider
    {
        bool TryGetSilhouetteDepthSpan(
            SlingshotBandShapeQuery query,
            out float minimumDepth,
            out float maximumDepth);
    }

    internal interface ISlingshotBandShapeOffsetProvider
    {
        bool TryGetSilhouetteOffsetSpan(
            SlingshotBandShapeQuery query,
            out float minimumOffset,
            out float maximumOffset);
    }

    internal interface ISlingshotRenderedBandShapeProvider
    {
        bool TryCreateRenderedBandShape(
            SlingshotBandShapeQuery query,
            float renderedBandRadius,
            Vector3[] outputPoints,
            out int pointCount);
    }

    public sealed class SlingshotBandShapeProvider :
        ISlingshotBandShapeProvider,
        ISlingshotBandShapeDepthProvider,
        ISlingshotBandShapeOffsetProvider,
        ISlingshotRenderedBandShapeProvider
    {
        private readonly ILaunchTargetSilhouetteSource _silhouetteSource;
        private readonly ISlingshotConfig _config;
        private readonly PullPlaneTautBandSolver _solver;
        private readonly PullPlaneBandShapeClearance _clearance;
        private readonly LaunchFrameValidator _launchFrameValidator;
        private readonly Vector3[] _worldSilhouetteSamples;
        private readonly float2[] _solverSilhouetteSamples;
        private readonly float2[] _solverOutputPoints;
        private readonly float2[] _clearanceBandShapePoints;

        public int BandShapePointCount { get; }

        public SlingshotBandShapeProvider(ILaunchTargetSilhouetteSource silhouetteSource, ISlingshotConfig config)
        {
            _silhouetteSource = silhouetteSource ?? throw new ArgumentNullException(nameof(silhouetteSource));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            ValidateConfig(config);

            _launchFrameValidator = new LaunchFrameValidator();
            BandShapePointCount = config.BandWrapSampleCount + 4;
            _worldSilhouetteSamples = new Vector3[config.BandSilhouetteSampleCount];
            _solverSilhouetteSamples = new float2[config.BandSilhouetteSampleCount];
            _solverOutputPoints = new float2[BandShapePointCount];
            _clearanceBandShapePoints = new float2[BandShapePointCount];
            _solver = new PullPlaneTautBandSolver(config.BandSilhouetteSampleCount, config.BandWrapSampleCount);
            _clearance = new PullPlaneBandShapeClearance(config.BandSilhouetteSampleCount);
        }

        public bool TryCreateBandShape(SlingshotBandShapeQuery query, Vector3[] outputPoints, out int pointCount)
        {
            return TryCreateBandShape(query, 0f, outputPoints, out pointCount);
        }

        bool ISlingshotRenderedBandShapeProvider.TryCreateRenderedBandShape(
            SlingshotBandShapeQuery query,
            float renderedBandRadius,
            Vector3[] outputPoints,
            out int pointCount)
        {
            return TryCreateBandShape(query, renderedBandRadius, outputPoints, out pointCount);
        }

        private bool TryCreateBandShape(
            SlingshotBandShapeQuery query,
            float renderedBandRadius,
            Vector3[] outputPoints,
            out int pointCount)
        {
            pointCount = 0;

            if (outputPoints is null)
                throw new ArgumentNullException(nameof(outputPoints));

            if (outputPoints.Length < BandShapePointCount)
                throw new ArgumentException("Output buffer is too small for the Slingshot Band Shape.", nameof(outputPoints));

            if (!IsValidQuery(query))
                throw new ArgumentException("Invalid Slingshot Band Shape query.", nameof(query));

            if (!math.isfinite(renderedBandRadius) || renderedBandRadius < 0f)
                throw new ArgumentException("Rendered Band radius must be finite and non-negative.", nameof(renderedBandRadius));

            if (!TryWriteSilhouetteSamplesToPullPlane(query, out var silhouetteSampleCount))
                return false;

            var solved = _solver.TrySolve(
                ToPullPlane(query.LeftAnchorPosition, query),
                ToPullPlane(query.RightAnchorPosition, query),
                ToPullPlane(query.PullPoint, query),
                _solverSilhouetteSamples,
                silhouetteSampleCount,
                _config.BandContactPadding + renderedBandRadius,
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

        public bool TryCheckBandShapeClearance(
            SlingshotBandShapeQuery query,
            IReadOnlyList<Vector3> bandShapePoints,
            float clearanceRadius,
            out bool isClear)
        {
            isClear = false;

            if (bandShapePoints is null)
                throw new ArgumentNullException(nameof(bandShapePoints));

            if (!IsValidQuery(query))
                throw new ArgumentException("Invalid Slingshot Band Shape query.", nameof(query));

            if (!math.isfinite(clearanceRadius) || clearanceRadius < 0f)
                throw new ArgumentException("Clearance radius must be finite and non-negative.", nameof(clearanceRadius));

            if (bandShapePoints.Count < 2 || bandShapePoints.Count > _clearanceBandShapePoints.Length)
                return false;

            if (!TryWriteSilhouetteSamplesToPullPlane(query, out var silhouetteSampleCount))
                return false;

            for (var pointIndex = 0; pointIndex < bandShapePoints.Count; pointIndex += 1)
            {
                var point = bandShapePoints[pointIndex];

                if (!point.IsFinite())
                    return false;

                _clearanceBandShapePoints[pointIndex] = ToPullPlane(point, query);
            }

            isClear = _clearance.IsClear(_clearanceBandShapePoints, bandShapePoints.Count, _solverSilhouetteSamples, silhouetteSampleCount,
                clearanceRadius);
            return true;
        }

        bool ISlingshotBandShapeDepthProvider.TryGetSilhouetteDepthSpan(
            SlingshotBandShapeQuery query,
            out float minimumDepth,
            out float maximumDepth)
        {
            minimumDepth = 0f;
            maximumDepth = 0f;

            if (!IsValidQuery(query))
                throw new ArgumentException("Invalid Slingshot Band Shape query.", nameof(query));

            if (!TryWriteSilhouetteSamplesToPullPlane(query, out var silhouetteSampleCount))
                return false;

            minimumDepth = float.PositiveInfinity;
            maximumDepth = float.NegativeInfinity;

            for (var sampleIndex = 0; sampleIndex < silhouetteSampleCount; sampleIndex += 1)
            {
                var depth = _solverSilhouetteSamples[sampleIndex].y;
                minimumDepth = Mathf.Min(minimumDepth, depth);
                maximumDepth = Mathf.Max(maximumDepth, depth);
            }

            return true;
        }

        bool ISlingshotBandShapeOffsetProvider.TryGetSilhouetteOffsetSpan(
            SlingshotBandShapeQuery query,
            out float minimumOffset,
            out float maximumOffset)
        {
            minimumOffset = 0f;
            maximumOffset = 0f;

            if (!IsValidQuery(query))
                throw new ArgumentException("Invalid Slingshot Band Shape query.", nameof(query));

            if (!TryWriteSilhouetteSamplesToPullPlane(query, out var silhouetteSampleCount))
                return false;

            minimumOffset = float.PositiveInfinity;
            maximumOffset = float.NegativeInfinity;

            for (var sampleIndex = 0; sampleIndex < silhouetteSampleCount; sampleIndex += 1)
            {
                var offset = _solverSilhouetteSamples[sampleIndex].x;
                minimumOffset = Mathf.Min(minimumOffset, offset);
                maximumOffset = Mathf.Max(maximumOffset, offset);
            }

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
                   && _launchFrameValidator.IsValid(
                       query.LaunchFrameRight,
                       query.LaunchFrameForward,
                       query.LaunchFrameUp);
        }

        private bool TryWriteSilhouetteSamplesToPullPlane(SlingshotBandShapeQuery query, out int silhouetteSampleCount)
        {
            silhouetteSampleCount = 0;

            var silhouetteQuery = new LaunchTargetSilhouetteQuery(
                query.RestPoint,
                query.LaunchFrameRight,
                query.LaunchFrameForward,
                query.LaunchFrameUp,
                _config.BandSilhouetteSampleCount);

            if (!_silhouetteSource.TryWriteSilhouetteSamples(silhouetteQuery, _worldSilhouetteSamples, out silhouetteSampleCount))
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

            return true;
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
