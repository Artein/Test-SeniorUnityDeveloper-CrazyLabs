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
