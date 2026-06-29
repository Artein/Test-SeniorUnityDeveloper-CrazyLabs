using System;
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
        // Aligns the authored Band Center marker to the requested held position.
        void SetHeldPosition(Vector3 heldPosition);
    }

    public interface ILaunchTargetPreLaunchReset
    {
        void ResetToPreLaunchPose(Vector3 position, Quaternion rotation);
    }

    public sealed partial class RigidbodyLaunchTarget : MonoBehaviour,
        ILaunchTarget,
        IHeldLaunchTarget,
        ILaunchTargetPreLaunchReset,
        ILaunchTargetSilhouetteSource
    {
        private const RigidbodyConstraints PostLaunchStabilizationConstraints =
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _bandContactCollider;
        [SerializeField] private Transform _bandCenter;

        private readonly LaunchFrameValidator _launchFrameValidator = new LaunchFrameValidator();
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

            _rigidbody.isKinematic = true;
            ClearVelocity();
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
            var targetRotation = _rigidbody.rotation;
            var positionDelta = heldPosition - GetBandCenterPositionFromRigidbodyPose();
            var targetPosition = _rigidbody.position + positionDelta;

            _rigidbody.transform.SetPositionAndRotation(targetPosition, targetRotation);
            _rigidbody.position = targetPosition;
            _rigidbody.rotation = targetRotation;
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

        private void ClearVelocity()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        void ILaunchTargetPreLaunchReset.ResetToPreLaunchPose(Vector3 position, Quaternion rotation)
        {
            ThrowIfInvalidReferences();

            if (!position.IsFinite())
                throw new ArgumentException("Pre-Launch position must be finite.", nameof(position));

            if (!IsValidRotation(rotation))
                throw new ArgumentException("Pre-Launch rotation must be finite and non-zero.", nameof(rotation));

            CapturePreviousStateIfNeeded();
            _isHeld = true;
            _rigidbody.isKinematic = true;
            _rigidbody.constraints = _previousConstraints;
            _rigidbody.transform.SetPositionAndRotation(position, rotation);
            _rigidbody.position = position;
            _rigidbody.rotation = rotation;
            ClearVelocity();
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
    }
}
