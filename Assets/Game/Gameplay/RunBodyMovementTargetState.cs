using UnityEngine;

namespace Game.Gameplay
{
    public readonly struct RunBodyMovementTargetState
    {
        public Vector3 LinearVelocity { get; }
        public bool HasRotation { get; }
        public Quaternion Rotation { get; }

        public RunBodyMovementTargetState(
            Vector3 linearVelocity,
            bool hasRotation,
            Quaternion rotation)
        {
            LinearVelocity = linearVelocity;
            HasRotation = hasRotation;
            Rotation = rotation;
        }
    }
}
