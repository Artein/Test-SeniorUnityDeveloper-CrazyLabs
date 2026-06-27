using System;
using System.Collections.Generic;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotBandShape
    {
        private readonly Vector3[] _points;

        public IReadOnlyList<Vector3> Points => _points ?? Array.Empty<Vector3>();

        internal SlingshotBandShape(Vector3[] points, bool useExistingBuffer)
        {
            if (points is null)
                throw new ArgumentNullException(nameof(points));

            if (points.Length < 2)
                throw new ArgumentException("Slingshot Band Shape requires at least two points.", nameof(points));

            if (useExistingBuffer)
            {
                for (var i = 0; i < points.Length; i += 1)
                {
                    if (!points[i].IsFinite())
                        throw new ArgumentException("Slingshot Band Shape points must be finite.", nameof(points));
                }

                _points = points;
                return;
            }

            _points = new Vector3[points.Length];

            for (var i = 0; i < points.Length; i += 1)
            {
                var point = points[i];

                if (!point.IsFinite())
                    throw new ArgumentException("Slingshot Band Shape points must be finite.", nameof(points));

                _points[i] = point;
            }
        }

        public SlingshotBandShape(params Vector3[] points)
            : this((IReadOnlyList<Vector3>)points)
        {
        }

        public SlingshotBandShape(IReadOnlyList<Vector3> points)
        {
            if (points is null)
                throw new ArgumentNullException(nameof(points));

            if (points.Count < 2)
                throw new ArgumentException("Slingshot Band Shape requires at least two points.", nameof(points));

            _points = new Vector3[points.Count];

            for (var i = 0; i < points.Count; i += 1)
            {
                var point = points[i];

                if (!point.IsFinite())
                    throw new ArgumentException("Slingshot Band Shape points must be finite.", nameof(points));

                _points[i] = point;
            }
        }
    }
}
