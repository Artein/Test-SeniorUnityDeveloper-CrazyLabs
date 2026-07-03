using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunCameraSource
    {
        Vector3 Position { get; }
        Vector3 LinearVelocity { get; }
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

        public TransformRunCameraLens(Transform transform)
        {
            _transform = transform != null ? transform : throw new System.ArgumentNullException(nameof(transform));
        }

        public Vector3 Position => _transform.position;
        public Quaternion Rotation => _transform.rotation;
    }

    public interface IRunCameraRig
    {
        void ActivateRunPreparationCamera();
        void ActivatePreLaunchCamera();
        void ActivateRunCamera();
    }
}
