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

    public interface IRunCameraRig
    {
        void SetCameraPriorities(int preLaunchCameraPriority, int runCameraPriority);
    }
}
