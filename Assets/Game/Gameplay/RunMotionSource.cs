using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunMotionSource
    {
        Vector3 Position { get; }
        Vector3 LinearVelocity { get; }
    }
}
