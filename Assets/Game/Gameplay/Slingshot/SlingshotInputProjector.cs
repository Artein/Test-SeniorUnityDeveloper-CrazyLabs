using System;
using Game.Utils.Mathematics;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotInputProjector
    {
        bool TryProjectScreenToPullPlane(Vector2 screenPosition, SlingshotGeometrySnapshot geometry, out Vector3 worldPosition);
        bool TryProjectWorldToScreen(Vector3 worldPosition, out Vector2 screenPosition);
    }

    internal interface ISlingshotBandVisibilityRayProvider
    {
        bool TryCreateRayToWorldPoint(Vector3 worldPoint, out Ray ray, out float distance);
    }

    [UsedImplicitly]
    public sealed class SlingshotInputProjector : ISlingshotInputProjector, ISlingshotBandVisibilityRayProvider
    {
        private readonly Camera _camera;

        public SlingshotInputProjector(Camera camera)
        {
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
        }

        bool ISlingshotInputProjector.TryProjectScreenToPullPlane(Vector2 screenPosition, SlingshotGeometrySnapshot geometry, out Vector3 worldPosition)
        {
            var plane = new Plane(geometry.LaunchFrameUp, geometry.RestPoint);
            var ray = _camera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0f));

            if (!plane.Raycast(ray, out var enter) || enter < 0f)
            {
                worldPosition = Vector3.zero;
                return false;
            }

            worldPosition = ray.GetPoint(enter);
            return worldPosition.IsFinite();
        }

        bool ISlingshotInputProjector.TryProjectWorldToScreen(Vector3 worldPosition, out Vector2 screenPosition)
        {
            var projectedPosition = _camera.WorldToScreenPoint(worldPosition);

            if (projectedPosition.z < 0f || !projectedPosition.IsFinite())
            {
                screenPosition = Vector2.zero;
                return false;
            }

            screenPosition = new Vector2(projectedPosition.x, projectedPosition.y);
            return true;
        }

        bool ISlingshotBandVisibilityRayProvider.TryCreateRayToWorldPoint(Vector3 worldPoint, out Ray ray, out float distance)
        {
            var rayDirection = worldPoint - _camera.transform.position;
            distance = rayDirection.magnitude;

            if (distance <= 0.0001f || !rayDirection.IsFinite())
            {
                ray = default;
                return false;
            }

            ray = new Ray(_camera.transform.position, rayDirection / distance);
            return true;
        }
    }
}
