using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class TransformRunCameraAnchor : MonoBehaviour, IRunCameraAnchor
    {
        Vector3 IRunCameraAnchor.Position => transform.position;
        Quaternion IRunCameraAnchor.Rotation => transform.rotation;

        void IRunCameraAnchor.SetPose(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
