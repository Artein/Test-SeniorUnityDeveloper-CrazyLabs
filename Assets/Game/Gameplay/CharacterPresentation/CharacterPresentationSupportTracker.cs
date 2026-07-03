using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    internal interface ICharacterPresentationSupportTracker
    {
        CharacterPresentationSupportSample Update(
            RunSurfaceContext rawSurfaceContext,
            Vector3 position,
            Vector3 linearVelocity,
            Vector3 courseUpDirection,
            ICharacterPresentationTuning tuning,
            float deltaTime,
            bool reset);

        void Reset();
    }

    internal sealed class CharacterPresentationSupportTracker : ICharacterPresentationSupportTracker
    {
        private const float MinimumDirectionSqrMagnitude = 0.000001f;

        private float _unsupportedElapsedSeconds;
        private float _supportReacquireElapsedSeconds;
        private bool _hasUnsupportedStartPosition;
        private Vector3 _unsupportedStartPosition;

        CharacterPresentationSupportSample ICharacterPresentationSupportTracker.Update(
            RunSurfaceContext rawSurfaceContext,
            Vector3 position,
            Vector3 linearVelocity,
            Vector3 courseUpDirection,
            ICharacterPresentationTuning tuning,
            float deltaTime,
            bool reset)
        {
            if (tuning is null)
                throw new ArgumentNullException(nameof(tuning));

            var safeDeltaTime = Mathf.Max(0f, deltaTime);
            var safePosition = GetFiniteOrZero(position);
            var safeCourseUpDirection = GetSafeDirection(courseUpDirection, Vector3.up);

            if (reset)
                return ResetToSupported(new RunSurfaceContext(true, safeCourseUpDirection, 0f));

            if (IsSupportedForPresentation(rawSurfaceContext, linearVelocity, safeCourseUpDirection, tuning))
            {
                if (!_hasUnsupportedStartPosition || HasReacquiredSupport(tuning, safeDeltaTime))
                    return ResetToSupported(rawSurfaceContext);
            }
            else
            {
                _supportReacquireElapsedSeconds = 0f;
            }

            if (!_hasUnsupportedStartPosition)
            {
                _hasUnsupportedStartPosition = true;
                _unsupportedStartPosition = safePosition;
                _unsupportedElapsedSeconds = 0f;
            }

            _unsupportedElapsedSeconds += safeDeltaTime;
            var separation = Vector3.Dot(safePosition - _unsupportedStartPosition, safeCourseUpDirection);

            return new CharacterPresentationSupportSample(
                new RunSurfaceContext(false, Vector3.up, 0f),
                _unsupportedElapsedSeconds,
                float.IsFinite(separation) ? separation : 0f);
        }

        void ICharacterPresentationSupportTracker.Reset()
        {
            _unsupportedElapsedSeconds = 0f;
            _supportReacquireElapsedSeconds = 0f;
            _hasUnsupportedStartPosition = false;
            _unsupportedStartPosition = Vector3.zero;
        }

        private bool HasReacquiredSupport(ICharacterPresentationTuning tuning, float deltaTime)
        {
            var reacquireSeconds = Mathf.Max(0f, tuning.PresentationSupportReacquireSeconds);

            if (reacquireSeconds <= 0f)
                return true;

            _supportReacquireElapsedSeconds += deltaTime;
            return _supportReacquireElapsedSeconds >= reacquireSeconds;
        }

        private bool IsSupportedForPresentation(
            RunSurfaceContext rawSurfaceContext,
            Vector3 linearVelocity,
            Vector3 courseUpDirection,
            ICharacterPresentationTuning tuning)
        {
            if (!rawSurfaceContext.IsGrounded)
                return false;

            var groundNormal = GetSafeDirection(rawSurfaceContext.GroundNormal, courseUpDirection);
            var liftSpeed = Vector3.Dot(GetFiniteOrZero(linearVelocity), groundNormal);
            return liftSpeed <= Mathf.Max(0f, tuning.PresentationSupportMaximumSurfaceLiftSpeed);
        }

        private static Vector3 GetSafeDirection(Vector3 direction, Vector3 fallbackDirection)
        {
            if (direction.IsFinite() && direction.sqrMagnitude > MinimumDirectionSqrMagnitude)
                return direction.normalized;

            if (fallbackDirection.IsFinite() && fallbackDirection.sqrMagnitude > MinimumDirectionSqrMagnitude)
                return fallbackDirection.normalized;

            return Vector3.up;
        }

        private static Vector3 GetFiniteOrZero(Vector3 value)
        {
            return value.IsFinite() ? value : Vector3.zero;
        }

        private CharacterPresentationSupportSample ResetToSupported(RunSurfaceContext surfaceContext)
        {
            ((ICharacterPresentationSupportTracker)this).Reset();
            return new CharacterPresentationSupportSample(surfaceContext, 0f, 0f);
        }
    }

    internal readonly struct CharacterPresentationSupportSample
    {
        public RunSurfaceContext SurfaceContext { get; }
        public float UngroundedElapsedSeconds { get; }
        public float UngroundedVerticalSeparation { get; }

        public CharacterPresentationSupportSample(
            RunSurfaceContext surfaceContext,
            float ungroundedElapsedSeconds,
            float ungroundedVerticalSeparation)
        {
            SurfaceContext = surfaceContext;
            UngroundedElapsedSeconds = Mathf.Max(0f, ungroundedElapsedSeconds);
            UngroundedVerticalSeparation = float.IsFinite(ungroundedVerticalSeparation) ? ungroundedVerticalSeparation : 0f;
        }
    }
}
