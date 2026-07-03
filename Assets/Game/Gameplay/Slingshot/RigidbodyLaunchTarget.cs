using System;
using System.Collections.Generic;
using Game.Utils.Mathematics;
using Game.Utils.Physics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ILaunchTarget
    {
        void Hold();
        void Launch(Vector3 velocity);
    }

    public interface IHeldLaunchTarget
    {
        // Aligns the authored Band Center marker to the requested held position.
        void SetHeldPosition(Vector3 heldPosition);
    }

    public interface ILaunchTargetPreLaunchReset
    {
        void ResetToPreLaunchPose(Vector3 position, Quaternion rotation);
    }

    internal interface ILaunchTargetBandOcclusionSource
    {
        bool IsBandPointHiddenFrom(Ray ray, float maxDistance);
    }

    public sealed partial class RigidbodyLaunchTarget : MonoBehaviour,
        ILaunchTarget,
        IHeldLaunchTarget,
        ILaunchTargetPreLaunchReset,
        IRunEndPoseLockTarget,
        ILaunchTargetSilhouetteSource,
        ILaunchTargetBandShapeClearanceSource,
        ILaunchTargetBandOcclusionSource
    {
        private const int MinimumBandShapeClearanceSegmentSampleCount = 64;
        private const int MaximumBandShapeClearanceSegmentSampleCount = 2048;
        private const int BandShapeClearanceHitBufferSize = 16;
        private const float MinimumBandShapeClearanceSampleSpacing = 0.0005f;
        private const float MaximumBandShapeClearanceSampleSpacing = 0.0025f;
        private const float MinimumBandShapeClearanceSegmentLength = 0.0001f;

        private const RigidbodyConstraints PostLaunchStabilizationConstraints =
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _bandContactCollider;
        [SerializeField] private Transform _bandCenter;

        private readonly LaunchFrameValidator _launchFrameValidator = new();
        private readonly RaycastHit[] _bandShapeClearanceHits = new RaycastHit[BandShapeClearanceHitBufferSize];
        private bool _isHeld;
        private bool _hasPreviousState;
        private bool _previousIsKinematic;
        private RigidbodyConstraints _previousConstraints;

        void ILaunchTarget.Hold()
        {
            ThrowIfMissingRigidbody();

            if (!_isHeld)
            {
                CapturePreviousStateIfNeeded();
                _isHeld = true;
            }

            _rigidbody.ClearVelocityIfDynamic();
            _rigidbody.isKinematic = true;
        }

        void ILaunchTarget.Launch(Vector3 velocity)
        {
            ThrowIfMissingRigidbody();

            var launchBaseConstraints = _rigidbody.constraints;

            if (_isHeld)
            {
                _rigidbody.isKinematic = _previousIsKinematic;
                launchBaseConstraints = _previousConstraints;
                _isHeld = false;
            }

            _rigidbody.constraints = launchBaseConstraints | PostLaunchStabilizationConstraints;
            _rigidbody.ClearVelocityIfDynamic();
            _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        void IHeldLaunchTarget.SetHeldPosition(Vector3 heldPosition)
        {
            ThrowIfInvalidReferences();

            if (!_isHeld)
                throw new InvalidOperationException("Launch Target must be held before assigning a held position.");

            if (!heldPosition.IsFinite())
                throw new ArgumentException("Held position must be finite.", nameof(heldPosition));

            var targetRotation = _rigidbody.rotation;
            var positionDelta = heldPosition - GetBandCenterPositionFromRigidbodyPose();
            var targetPosition = _rigidbody.position + positionDelta;

            _rigidbody.transform.SetPositionAndRotation(targetPosition, targetRotation);
            _rigidbody.position = targetPosition;
            _rigidbody.rotation = targetRotation;
            Physics.SyncTransforms();
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

        bool ILaunchTargetBandShapeClearanceSource.TryCheckBandShapeClearance(
            IReadOnlyList<Vector3> bandShapePoints,
            float clearanceRadius,
            out bool isClear)
        {
            isClear = false;
            ThrowIfInvalidReferences();

            if (bandShapePoints is null)
                throw new ArgumentNullException(nameof(bandShapePoints));

            if (!math.isfinite(clearanceRadius) || clearanceRadius < 0f)
                throw new ArgumentException("Clearance radius must be finite and non-negative.", nameof(clearanceRadius));

            if (!_bandContactCollider.enabled || bandShapePoints.Count < 2)
                return false;

            Physics.SyncTransforms();

            for (var pointIndex = 0; pointIndex < bandShapePoints.Count; pointIndex += 1)
            {
                if (!bandShapePoints[pointIndex].IsFinite())
                    return false;
            }

            for (var pointIndex = 0; pointIndex < bandShapePoints.Count - 1; pointIndex += 1)
            {
                var start = bandShapePoints[pointIndex];
                var end = bandShapePoints[pointIndex + 1];

                if (IsBandSegmentTooCloseToContactCollider(start, end, clearanceRadius))
                    return true;
            }

            isClear = true;
            return true;
        }

        private bool IsBandSegmentTooCloseToContactCollider(Vector3 start, Vector3 end, float clearanceRadius)
        {
            if (IsBandPointTooCloseToContactCollider(start, clearanceRadius)
                || IsBandPointTooCloseToContactCollider(end, clearanceRadius))
            {
                return true;
            }

            var segment = end - start;
            var segmentLength = segment.magnitude;

            if (segmentLength <= MinimumBandShapeClearanceSegmentLength)
                return false;

            if (DoesBandSegmentSweepHitContactCollider(start, segment / segmentLength, segmentLength, clearanceRadius))
                return true;

            var segmentSampleCount = GetBandShapeClearanceSegmentSampleCount(start, end, clearanceRadius);

            for (var sampleIndex = 1; sampleIndex < segmentSampleCount; sampleIndex += 1)
            {
                var t = (float)sampleIndex / segmentSampleCount;
                var samplePoint = Vector3.Lerp(start, end, t);

                if (IsBandPointTooCloseToContactCollider(samplePoint, clearanceRadius))
                    return true;
            }

            return false;
        }

        private bool DoesBandSegmentSweepHitContactCollider(
            Vector3 start,
            Vector3 direction,
            float distance,
            float clearanceRadius)
        {
            var layerMask = 1 << _bandContactCollider.gameObject.layer;

            var hitCount = Physics.SphereCastNonAlloc(
                start,
                clearanceRadius,
                direction,
                _bandShapeClearanceHits,
                distance,
                layerMask,
                QueryTriggerInteraction.Collide);

            for (var hitIndex = 0; hitIndex < hitCount; hitIndex += 1)
            {
                if (_bandShapeClearanceHits[hitIndex].collider == _bandContactCollider)
                    return true;
            }

            return false;
        }

        private bool IsBandPointTooCloseToContactCollider(Vector3 point, float clearanceRadius)
        {
            var closestPoint = _bandContactCollider.ClosestPoint(point);
            return Vector3.Distance(point, closestPoint) <= clearanceRadius;
        }

        private int GetBandShapeClearanceSegmentSampleCount(Vector3 start, Vector3 end, float clearanceRadius)
        {
            var segmentLength = Vector3.Distance(start, end);
            var radiusBasedSpacing = Mathf.Max(MinimumBandShapeClearanceSampleSpacing, clearanceRadius * 0.05f);
            var sampleSpacing = Mathf.Min(MaximumBandShapeClearanceSampleSpacing, radiusBasedSpacing);
            var sampleCount = Mathf.CeilToInt(segmentLength / sampleSpacing);

            return Mathf.Clamp(
                sampleCount,
                MinimumBandShapeClearanceSegmentSampleCount,
                MaximumBandShapeClearanceSegmentSampleCount);
        }

        bool ILaunchTargetBandOcclusionSource.IsBandPointHiddenFrom(Ray ray, float maxDistance)
        {
            ThrowIfInvalidReferences();

            if (!_bandContactCollider.enabled || maxDistance <= 0.0001f)
                return false;

            return _bandContactCollider.Raycast(ray, out _, maxDistance);
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Rigidbody reference.", this);

            if (_bandContactCollider == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Launch Target Silhouette Collider reference.", this);

            if (_bandCenter == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Band Center reference.", this);

            if (_rigidbody != null
                && _bandCenter != null
                && _bandCenter != _rigidbody.transform
                && !_bandCenter.IsChildOf(_rigidbody.transform))
            {
                Debug.LogWarning("RigidbodyLaunchTarget Band Center must be assigned under the Rigidbody transform hierarchy.", this);
            }
        }

        void ILaunchTargetPreLaunchReset.ResetToPreLaunchPose(Vector3 position, Quaternion rotation)
        {
            ThrowIfInvalidReferences();

            if (!position.IsFinite())
                throw new ArgumentException("Pre-Launch position must be finite.", nameof(position));

            if (!IsValidRotation(rotation))
                throw new ArgumentException("Pre-Launch rotation must be finite and non-zero.", nameof(rotation));

            CapturePreviousStateIfNeeded();
            _rigidbody.ClearVelocityIfDynamic();
            _isHeld = true;
            _rigidbody.isKinematic = true;
            _rigidbody.constraints = _previousConstraints;
            _rigidbody.transform.SetPositionAndRotation(position, rotation);
            _rigidbody.position = position;
            _rigidbody.rotation = rotation;
            Physics.SyncTransforms();
        }

        void IRunEndPoseLockTarget.HoldRunEndPose(Vector3 position)
        {
            ThrowIfMissingRigidbody();

            if (!position.IsFinite())
                throw new ArgumentException("Run End position must be finite.", nameof(position));

            CapturePreviousStateIfNeeded();
            _rigidbody.ClearVelocityIfDynamic();
            _isHeld = true;
            _rigidbody.isKinematic = true;
            _rigidbody.transform.SetPositionAndRotation(position, _rigidbody.rotation);
            _rigidbody.position = position;
            Physics.SyncTransforms();
        }

        void IRunEndPoseLockTarget.ReleaseRunEndPose()
        {
            ThrowIfMissingRigidbody();
            _rigidbody.ClearVelocityIfDynamic();
        }

        private void CapturePreviousStateIfNeeded()
        {
            if (_hasPreviousState)
                return;

            _previousIsKinematic = _rigidbody.isKinematic;
            _previousConstraints = _rigidbody.constraints;
            _hasPreviousState = true;
        }

        private void ThrowIfInvalidReferences()
        {
            ThrowIfMissingRigidbody();

            if (_bandContactCollider == null)
                throw new InvalidOperationException("RigidbodyLaunchTarget requires a Launch Target Silhouette Collider reference.");

            if (_bandCenter == null)
                throw new InvalidOperationException("RigidbodyLaunchTarget requires a Band Center reference.");

            if (_bandCenter != _rigidbody.transform
                && !_bandCenter.IsChildOf(_rigidbody.transform))
            {
                throw new InvalidOperationException("RigidbodyLaunchTarget Band Center must be assigned under the Rigidbody transform hierarchy.");
            }
        }

        private void ThrowIfMissingRigidbody()
        {
            if (_rigidbody == null)
                throw new InvalidOperationException("RigidbodyLaunchTarget requires a Rigidbody reference.");
        }

        private bool IsValidRotation(Quaternion rotation)
        {
            return math.isfinite(rotation.x)
                   && math.isfinite(rotation.y)
                   && math.isfinite(rotation.z)
                   && math.isfinite(rotation.w)
                   && Quaternion.Dot(rotation, rotation) > 0.000001f;
        }

        private void ThrowIfInvalidQuery(LaunchTargetSilhouetteQuery query, Vector3[] outputSamples)
        {
            if (outputSamples is null)
                throw new ArgumentNullException(nameof(outputSamples));

            if (!query.PlaneOrigin.IsFinite()
                || !_launchFrameValidator.IsValid(
                    query.LaunchFrameRight,
                    query.LaunchFrameForward,
                    query.LaunchFrameUp)
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

        private Vector3 GetBandCenterPositionFromRigidbodyPose()
        {
            var localBandCenterPosition = GetBandCenterLocalPositionInRigidbodySpace();
            var scaledLocalBandCenterPosition = Vector3.Scale(localBandCenterPosition, _rigidbody.transform.lossyScale);
            return _rigidbody.position + (_rigidbody.rotation * scaledLocalBandCenterPosition);
        }

        private Vector3 GetBandCenterLocalPositionInRigidbodySpace()
        {
            var localToRigidbody = Matrix4x4.identity;

            for (var current = _bandCenter; current != _rigidbody.transform; current = current.parent)
            {
                localToRigidbody = Matrix4x4.TRS(current.localPosition, current.localRotation, current.localScale) * localToRigidbody;
            }

            return localToRigidbody.MultiplyPoint3x4(Vector3.zero);
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

        public static class Serialization
        {
            public const string BandContactCollider = nameof(_bandContactCollider);
            public const string BandCenter = nameof(_bandCenter);
        }
    }
}
