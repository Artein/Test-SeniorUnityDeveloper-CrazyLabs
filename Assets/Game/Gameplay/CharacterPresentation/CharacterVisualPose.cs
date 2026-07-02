using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    internal readonly struct CharacterVisualPose
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public CharacterVisualPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
