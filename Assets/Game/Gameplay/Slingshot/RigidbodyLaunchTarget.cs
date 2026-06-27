using System;
using Game.Utils.Mathematics;
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

    public sealed partial class RigidbodyLaunchTarget : MonoBehaviour, ILaunchTarget, IHeldLaunchTarget, ILaunchTargetSilhouetteSource
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

        bool ILaunchTargetSilhouetteSource.TryWriteSilhouetteSamples(LaunchTargetSilhouetteQuery query, Vector3[] outputSamples, out int sampleCount)
        {
            sampleCount = 0;
            ThrowIfInvalidReferences();
            ThrowIfInvalidQuery(query, outputSamples);

            if (!_bandContactCollider.enabled)
                return false;

            var center = GetCurrentBandCenter(query);
            var radius = GetSilhouetteSampleRadius(center, query);
            var backward = -query.LaunchFrameForward;

            for (var sampleIndex = 0; sampleIndex < query.SampleCount; sampleIndex += 1)
            {
                var normalizedAngle = (float)sampleIndex / query.SampleCount;
                var angle = normalizedAngle * Mathf.PI * 2f;
                var direction = (query.LaunchFrameRight * Mathf.Cos(angle)) + (backward * Mathf.Sin(angle));
                var queryOrigin = center + (direction * radius);
                var closestPoint = _bandContactCollider.ClosestPoint(queryOrigin);
                outputSamples[sampleIndex] = ProjectPointOntoPlane(closestPoint, query.PlaneOrigin, query.LaunchFrameUp);
            }

            sampleCount = query.SampleCount;
            return true;
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Rigidbody reference.", this);

            if (_bandContactCollider == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Launch Target Silhouette Collider reference.", this);
        }

        private void ClearVelocity()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        private void ThrowIfInvalidReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyLaunchTarget requires a Rigidbody reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_bandContactCollider,
                "RigidbodyLaunchTarget requires a Launch Target Silhouette Collider reference.");
        }

        private void ThrowIfInvalidQuery(LaunchTargetSilhouetteQuery query, Vector3[] outputSamples)
        {
            if (outputSamples is null)
                throw new ArgumentNullException(nameof(outputSamples));

            if (!query.PlaneOrigin.IsFinite()
                || !query.LaunchFrameRight.IsFinite()
                || !query.LaunchFrameForward.IsFinite()
                || !query.LaunchFrameUp.IsFinite()
                || !query.LaunchFrameRight.IsApproximatelyUnit()
                || !query.LaunchFrameForward.IsApproximatelyUnit()
                || !query.LaunchFrameUp.IsApproximatelyUnit()
                || query.SampleCount < 3)
            {
                throw new ArgumentException("Invalid Launch Target silhouette query.", nameof(query));
            }

            if (outputSamples.Length < query.SampleCount)
                throw new ArgumentException("Output samples buffer is too small for the Launch Target silhouette query.", nameof(outputSamples));
        }

        private Vector3 GetCurrentBandCenter(LaunchTargetSilhouetteQuery query)
        {
            return ProjectPointOntoPlane(_bandContactCollider.bounds.center, query.PlaneOrigin, query.LaunchFrameUp);
        }

        private float GetSilhouetteSampleRadius(Vector3 center, LaunchTargetSilhouetteQuery query)
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

            return maxRadius + 0.1f;
        }

        private Vector3 ProjectPointOntoPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
        {
            return point - (planeNormal * Vector3.Dot(point - planePoint, planeNormal));
        }
    }
}
