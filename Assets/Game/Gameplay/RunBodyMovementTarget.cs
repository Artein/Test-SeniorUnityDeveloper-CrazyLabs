using UnityEngine;

namespace Game.Gameplay
{
    public interface IRunBodyMovementTarget
    {
        Vector3 LinearVelocity { get; }
        void ApplyTargetState(RunBodyMovementTargetState targetState);
    }
}
