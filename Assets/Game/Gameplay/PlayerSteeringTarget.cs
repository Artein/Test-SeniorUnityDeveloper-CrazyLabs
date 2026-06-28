using UnityEngine;

namespace Game.Gameplay
{
    public interface IPlayerSteeringTarget
    {
        Vector3 LinearVelocity { get; }
        void ApplySteering(Vector3 linearVelocity, Quaternion rotation);
    }
}
