using System;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunCameraSource
    {
        Vector3 LinearVelocity { get; }

        /// <summary>
        ///     Gets the presentation pose position, which may include Rigidbody interpolation.
        /// </summary>
        Vector3 Position { get; }
    }

    public interface IRunCameraAnchor
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        void SetPose(Vector3 position, Quaternion rotation);
    }

    public interface IRunCameraLens
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
    }

    internal sealed class TransformRunCameraLens : IRunCameraLens
    {
        private readonly Transform _transform;

        public Vector3 Position => _transform.position;
        public Quaternion Rotation => _transform.rotation;

        public TransformRunCameraLens(Transform transform)
        {
            _transform = transform != null ? transform : throw new ArgumentNullException(nameof(transform));
        }
    }

    public interface IRunCameraRig
    {
        void ActivateRunPreparationCamera();
        void ActivatePreLaunchCamera();
        void ActivateRunCamera();
    }
}
