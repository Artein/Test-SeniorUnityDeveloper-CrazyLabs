using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    internal sealed class CharacterVisualPoseSmoother : ICharacterVisualPoseSmoother
    {
        private readonly float _minimumDirectionSqrMagnitude = 0.000001f;
        private readonly float _minimumSnapDistance = 0.0001f;
        private readonly float _maximumSnapAngle = 180f;

        private CharacterVisualPose _smoothedPose;
        private bool _hasSmoothedPose;

        public void Reset()
        {
            _hasSmoothedPose = false;
            _smoothedPose = new CharacterVisualPose(Vector3.zero, Quaternion.identity);
        }

        public CharacterVisualPose Update(
            CharacterVisualPose currentVisualPose,
            CharacterVisualPose targetPose,
            ICharacterVisualFollowTuning tuning,
            float deltaTime,
            bool snap)
        {
            if (tuning is null)
                throw new ArgumentNullException(nameof(tuning));

            var safeCurrentPose = SanitizePose(currentVisualPose,
                _hasSmoothedPose ? _smoothedPose : new CharacterVisualPose(Vector3.zero, Quaternion.identity));
            var safeTargetPose = SanitizePose(targetPose, _hasSmoothedPose ? _smoothedPose : safeCurrentPose);

            if (!_hasSmoothedPose)
                return SnapTo(safeTargetPose);

            if (ShouldSnap(safeTargetPose, tuning, snap))
                return SnapTo(safeTargetPose);

            var safeDeltaTime = Mathf.Max(0f, deltaTime);
            var position = GetNextPosition(safeTargetPose.Position, tuning, safeDeltaTime);
            var rotation = GetNextRotation(safeTargetPose.Rotation, tuning, safeDeltaTime);

            _smoothedPose = SanitizePose(new CharacterVisualPose(position, rotation), safeTargetPose);
            _hasSmoothedPose = true;
            return _smoothedPose;
        }

        private CharacterVisualPose SnapTo(CharacterVisualPose targetPose)
        {
            _smoothedPose = targetPose;
            _hasSmoothedPose = true;
            return _smoothedPose;
        }

        private bool ShouldSnap(CharacterVisualPose targetPose, ICharacterVisualFollowTuning tuning, bool snap)
        {
            if (snap)
                return true;

            var snapDistance = Mathf.Max(_minimumSnapDistance, tuning.VisualSnapDistance);

            if (Vector3.Distance(_smoothedPose.Position, targetPose.Position) >= snapDistance)
                return true;

            var snapAngle = Mathf.Clamp(tuning.VisualSnapAngleDegrees, 0f, _maximumSnapAngle);
            return Quaternion.Angle(_smoothedPose.Rotation, targetPose.Rotation) >= snapAngle;
        }

        private Vector3 GetNextPosition(Vector3 targetPosition, ICharacterVisualFollowTuning tuning, float deltaTime)
        {
            var response = Mathf.Max(0f, tuning.VisualPositionResponseRate);
            var t = GetResponseT(response, deltaTime);
            var nextPosition = Vector3.Lerp(_smoothedPose.Position, targetPosition, t);
            var maxLag = Mathf.Max(0f, tuning.VisualMaxPositionLag);

            if (maxLag <= 0f)
                return targetPosition;

            var lag = targetPosition - nextPosition;

            if (!lag.IsFinite() || lag.sqrMagnitude <= maxLag * maxLag)
                return nextPosition;

            return targetPosition - (lag.normalized * maxLag);
        }

        private Quaternion GetNextRotation(Quaternion targetRotation, ICharacterVisualFollowTuning tuning, float deltaTime)
        {
            var previousForward = _smoothedPose.Rotation * Vector3.forward;
            var previousUp = _smoothedPose.Rotation * Vector3.up;
            var targetForward = targetRotation * Vector3.forward;
            var targetUp = targetRotation * Vector3.up;
            var nextUp = GetNextDirection(previousUp, targetUp, tuning.VisualUpTiltResponseRate, deltaTime);
            var previousHeading = GetProjectedDirection(previousForward, nextUp, targetForward);
            var targetHeading = GetProjectedDirection(targetForward, nextUp, previousHeading);
            var nextForward = GetNextDirection(previousHeading, targetHeading, tuning.VisualHeadingResponseRate, deltaTime);

            return CreateRotation(nextForward, nextUp, targetRotation);
        }

        private Vector3 GetNextDirection(Vector3 previousDirection, Vector3 targetDirection, float responseRate, float deltaTime)
        {
            var safePreviousDirection = GetSafeDirection(previousDirection, Vector3.forward);
            var safeTargetDirection = GetSafeDirection(targetDirection, safePreviousDirection);
            var response = Mathf.Max(0f, responseRate);
            var t = GetResponseT(response, deltaTime);
            var nextDirection = Vector3.Slerp(safePreviousDirection, safeTargetDirection, t);

            return GetSafeDirection(nextDirection, safeTargetDirection);
        }

        private float GetResponseT(float responseRate, float deltaTime)
        {
            if (responseRate <= 0f)
                return 1f;

            if (deltaTime <= 0f)
                return 0f;

            return Mathf.Clamp01(1f - Mathf.Exp(-responseRate * deltaTime));
        }

        private CharacterVisualPose SanitizePose(CharacterVisualPose pose, CharacterVisualPose fallbackPose)
        {
            var position = pose.Position.IsFinite()
                ? pose.Position
                : fallbackPose.Position.IsFinite()
                    ? fallbackPose.Position
                    : Vector3.zero;

            var fallbackRotation = IsValidRotation(fallbackPose.Rotation) ? fallbackPose.Rotation : Quaternion.identity;

            var rotation = IsValidRotation(pose.Rotation)
                ? CreateRotation(pose.Rotation * Vector3.forward, pose.Rotation * Vector3.up, fallbackRotation)
                : fallbackRotation;

            return new CharacterVisualPose(position, rotation);
        }

        private Quaternion CreateRotation(Vector3 forward, Vector3 up, Quaternion fallbackRotation)
        {
            var safeUp = GetSafeDirection(up, fallbackRotation * Vector3.up);
            var projectedForward = GetProjectedDirection(forward, safeUp, fallbackRotation * Vector3.forward);
            return Quaternion.LookRotation(projectedForward, safeUp);
        }

        private Vector3 GetProjectedDirection(Vector3 direction, Vector3 normal, Vector3 fallbackDirection)
        {
            var safeNormal = GetSafeDirection(normal, Vector3.up);
            var projectedDirection = Vector3.ProjectOnPlane(direction, safeNormal);

            if (IsValidDirection(projectedDirection))
                return projectedDirection.normalized;

            var projectedFallback = Vector3.ProjectOnPlane(fallbackDirection, safeNormal);

            if (IsValidDirection(projectedFallback))
                return projectedFallback.normalized;

            var cross = Vector3.Cross(safeNormal, Vector3.right);

            if (IsValidDirection(cross))
                return cross.normalized;

            return Vector3.Cross(safeNormal, Vector3.forward).normalized;
        }

        private Vector3 GetSafeDirection(Vector3 direction, Vector3 fallbackDirection)
        {
            if (IsValidDirection(direction))
                return direction.normalized;

            if (IsValidDirection(fallbackDirection))
                return fallbackDirection.normalized;

            return Vector3.forward;
        }

        private bool IsValidDirection(Vector3 direction)
        {
            return direction.IsFinite() && direction.sqrMagnitude > _minimumDirectionSqrMagnitude;
        }

        private bool IsValidRotation(Quaternion rotation)
        {
            return float.IsFinite(rotation.x)
                   && float.IsFinite(rotation.y)
                   && float.IsFinite(rotation.z)
                   && float.IsFinite(rotation.w)
                   && Quaternion.Dot(rotation, rotation) > _minimumDirectionSqrMagnitude;
        }
    }
}
