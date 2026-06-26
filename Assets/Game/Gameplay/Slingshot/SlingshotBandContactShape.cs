using System;
using System.Collections.Generic;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotBandContactShape
    {
        private readonly Vector3[] _wrapPoints;

        public Vector3 LeftContactPoint { get; }
        public IReadOnlyList<Vector3> WrapPoints => _wrapPoints ?? Array.Empty<Vector3>();
        public Vector3 RightContactPoint { get; }

        public SlingshotBandContactShape(Vector3 leftContactPoint, IReadOnlyList<Vector3> wrapPoints, Vector3 rightContactPoint)
        {
            if (!leftContactPoint.IsFinite())
                throw new ArgumentException("Left contact point must be finite.", nameof(leftContactPoint));

            if (wrapPoints is null)
                throw new ArgumentNullException(nameof(wrapPoints));

            if (!rightContactPoint.IsFinite())
                throw new ArgumentException("Right contact point must be finite.", nameof(rightContactPoint));

            _wrapPoints = new Vector3[wrapPoints.Count];

            for (var i = 0; i < wrapPoints.Count; i += 1)
            {
                var point = wrapPoints[i];

                if (!point.IsFinite())
                    throw new ArgumentException("Wrap points must be finite.", nameof(wrapPoints));

                _wrapPoints[i] = point;
            }

            LeftContactPoint = leftContactPoint;
            RightContactPoint = rightContactPoint;
        }
    }
}
