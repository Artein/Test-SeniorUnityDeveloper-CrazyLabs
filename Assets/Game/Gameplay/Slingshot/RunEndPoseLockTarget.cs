using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface IRunEndPoseLockTarget
    {
        void HoldRunEndPose(Vector3 position);
        void ReleaseRunEndPose();
    }
}
