using System;
using System.Collections.Generic;
using Game.Utils.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ILaunchTarget
    {
        // TODO - AI Note: If Slingshot feel should rotate the held target toward launch direction later,
        // add an explicit orientation contract instead of hiding rotation inside hold or launch.
        void Hold();
        void Launch(Vector3 velocity);
    }

    public interface IHeldLaunchTarget
    {
        void SetHeldPosition(Vector3 heldPosition);
    }

    public interface ISlingshotBandContactProvider
    {
        SlingshotBandContactShape CreateBandContactShape(SlingshotBandContactQuery query);
    }

    public sealed partial class RigidbodyLaunchTarget : MonoBehaviour, ILaunchTarget, IHeldLaunchTarget, ISlingshotBandContactProvider
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _bandContactCollider;

        private bool _isHeld;
        private bool _previousIsKinematic;
        private RigidbodyConstraints _previousConstraints;

        void ILaunchTarget.Hold()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyLaunchTarget requires a Rigidbody reference.");

            if (!_isHeld)
            {
                _previousIsKinematic = _rigidbody.isKinematic;
                _previousConstraints = _rigidbody.constraints;
                _isHeld = true;
            }

            _rigidbody.isKinematic = true;
            ClearVelocity();
        }

        void ILaunchTarget.Launch(Vector3 velocity)
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyLaunchTarget requires a Rigidbody reference.");

            if (_isHeld)
            {
                _rigidbody.isKinematic = _previousIsKinematic;
                _rigidbody.constraints = _previousConstraints;
                _isHeld = false;
            }

            ClearVelocity();
            _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        void IHeldLaunchTarget.SetHeldPosition(Vector3 heldPosition)
        {
            ThrowIfInvalidReferences();

            if (!_isHeld)
                throw new InvalidOperationException("Launch Target must be held before assigning a held position.");

            if (!heldPosition.IsFinite())
                throw new ArgumentException("Held position must be finite.", nameof(heldPosition));

            // TODO - AI Note: If held visuals need facing, lean, or spin, add an explicit orientation contract instead of rotating here.
            var colliderCenter = _bandContactCollider.bounds.center;
            var positionDelta = heldPosition - colliderCenter;
            var targetPosition = _rigidbody.transform.position + positionDelta;

            _rigidbody.transform.position = targetPosition;
            _rigidbody.position = targetPosition;
            ClearVelocity();
        }

        public SlingshotBandContactShape CreateBandContactShape(SlingshotBandContactQuery query)
        {
            ThrowIfInvalidReferences();
            ThrowIfInvalidQuery(query);

            var leftContactPoint = CreateSurfacePoint(query.LeftAnchorPosition, query);
            var rightContactPoint = CreateSurfacePoint(query.RightAnchorPosition, query);
            var wrapPoints = CreateWrapPoints(leftContactPoint, rightContactPoint, query);

            return new SlingshotBandContactShape(leftContactPoint, wrapPoints, rightContactPoint);
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Rigidbody reference.", this);

            if (_bandContactCollider == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Band Contact Collider reference.", this);
        }

        private void ClearVelocity()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        private void ThrowIfInvalidReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyLaunchTarget requires a Rigidbody reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_bandContactCollider, "RigidbodyLaunchTarget requires a Band Contact Collider reference.");
        }

        private void ThrowIfInvalidQuery(SlingshotBandContactQuery query)
        {
            if (!query.LeftAnchorPosition.IsFinite()
                || !query.RightAnchorPosition.IsFinite()
                || !query.PullPoint.IsFinite()
                || !query.LaunchFrameRight.IsFinite()
                || !query.LaunchFrameForward.IsFinite()
                || !query.LaunchFrameUp.IsFinite()
                || !query.LaunchFrameRight.IsApproximatelyUnit()
                || !query.LaunchFrameForward.IsApproximatelyUnit()
                || !query.LaunchFrameUp.IsApproximatelyUnit()
                || !math.isfinite(query.ContactPadding)
                || query.ContactPadding < 0f
                || query.WrapSampleCount < 2)
            {
                throw new ArgumentException("Invalid Slingshot Band contact query.", nameof(query));
            }
        }

        private IReadOnlyList<Vector3> CreateWrapPoints(Vector3 leftContactPoint, Vector3 rightContactPoint, SlingshotBandContactQuery query)
        {
            var sampleCount = Mathf.Clamp(query.WrapSampleCount, 2, 24);
            var wrapPoints = new Vector3[sampleCount];
            var center = GetCurrentBandCenter(query);
            var radius = GetWrapOriginRadius(center, query);
            var leftAngle = GetBackwardPlaneAngle(leftContactPoint, center, query);
            var rightAngle = GetBackwardPlaneAngle(rightContactPoint, center, query);

            if (leftAngle > 0f)
                leftAngle -= Mathf.PI * 2f;

            if (rightAngle < 0f)
                rightAngle += Mathf.PI * 2f;

            for (var pointIndex = 0; pointIndex < sampleCount; pointIndex += 1)
            {
                var progress = (pointIndex + 1f) / (sampleCount + 1f);
                var angle = Mathf.Lerp(leftAngle, rightAngle, progress);

                var originDirection = (query.LaunchFrameRight * Mathf.Sin(angle))
                                      - (query.LaunchFrameForward * Mathf.Cos(angle));
                var queryOrigin = center + (originDirection * radius);
                wrapPoints[pointIndex] = CreateSurfacePoint(queryOrigin, query);
            }

            return wrapPoints;
        }

        private Vector3 CreateSurfacePoint(Vector3 queryOrigin, SlingshotBandContactQuery query)
        {
            var closestPoint = _bandContactCollider.ClosestPoint(queryOrigin);
            var paddingDirection = ProjectVectorOntoPlane(queryOrigin - closestPoint, query.LaunchFrameUp);

            if (paddingDirection.sqrMagnitude <= 0.000001f)
            {
                var center = GetCurrentBandCenter(query);
                paddingDirection = ProjectVectorOntoPlane(closestPoint - center, query.LaunchFrameUp);
            }

            if (paddingDirection.sqrMagnitude <= 0.000001f)
                paddingDirection = -query.LaunchFrameForward;

            if (paddingDirection.sqrMagnitude > 0.000001f)
                closestPoint += paddingDirection.normalized * query.ContactPadding;

            return ProjectPointOntoPlane(closestPoint, query.PullPoint, query.LaunchFrameUp);
        }

        private Vector3 GetCurrentBandCenter(SlingshotBandContactQuery query)
        {
            return ProjectPointOntoPlane(_bandContactCollider.bounds.center, query.PullPoint, query.LaunchFrameUp);
        }

        private float GetWrapOriginRadius(Vector3 center, SlingshotBandContactQuery query)
        {
            var bounds = _bandContactCollider.bounds;
            var extents = bounds.extents;
            var maxRadius = 0.05f;

            for (var xSign = -1; xSign <= 1; xSign += 2)
            {
                for (var ySign = -1; ySign <= 1; ySign += 2)
                {
                    for (var zSign = -1; zSign <= 1; zSign += 2)
                    {
                        var corner = bounds.center + new Vector3(extents.x * xSign, extents.y * ySign, extents.z * zSign);
                        var projectedCorner = ProjectPointOntoPlane(corner, center, query.LaunchFrameUp);
                        var radius = (projectedCorner - center).magnitude;
                        maxRadius = Mathf.Max(maxRadius, radius);
                    }
                }
            }

            return maxRadius + query.ContactPadding + 0.1f;
        }

        private float GetBackwardPlaneAngle(Vector3 point, Vector3 center, SlingshotBandContactQuery query)
        {
            var direction = ProjectVectorOntoPlane(point - center, query.LaunchFrameUp);

            if (direction.sqrMagnitude <= 0.000001f)
                direction = -query.LaunchFrameForward;

            direction.Normalize();

            return Mathf.Atan2(
                Vector3.Dot(direction, query.LaunchFrameRight),
                Vector3.Dot(direction, -query.LaunchFrameForward));
        }

        private Vector3 ProjectPointOntoPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return point - (planeNormal * Vector3.Dot(point - planePoint, planeNormal));
        }

        private Vector3 ProjectVectorOntoPlane(Vector3 vector, Vector3 planeNormal)
        {
            return vector - (planeNormal * Vector3.Dot(vector, planeNormal));
        }
    }
}
