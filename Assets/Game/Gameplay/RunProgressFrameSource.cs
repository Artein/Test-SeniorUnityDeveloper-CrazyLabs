using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunProgressFrameSource
    {
        bool TryCreateSnapshot(Vector3 origin, out RunProgressFrameSnapshot snapshot, out string error);
    }

    public sealed class RunProgressFrameSource : MonoBehaviour, IRunProgressFrameSource
    {
        [SerializeField] private Transform _frameTransform;

        bool IRunProgressFrameSource.TryCreateSnapshot(Vector3 origin, out RunProgressFrameSnapshot snapshot, out string error)
        {
            var sourceTransform = _frameTransform != null ? _frameTransform : transform;
            return RunProgressFrameSnapshot.TryCreate(
                origin,
                sourceTransform.forward,
                sourceTransform.up,
                out snapshot,
                out error);
        }
    }
}
