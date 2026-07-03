using UnityEngine;

namespace Game.Gameplay
{
    public interface IPlayerSteeringTarget
    {
        Vector3 LinearVelocity { get; }
        void ApplyVelocity(Vector3 linearVelocity);
        void ApplySteering(Vector3 linearVelocity, Quaternion rotation);
    }
}
