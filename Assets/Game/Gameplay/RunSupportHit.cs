using UnityEngine;

namespace Game.Gameplay
{
    internal readonly struct RunSupportHit
    {
        public Vector3 Normal { get; }
        public float Distance { get; }
        public Collider Collider { get; }

        public RunSupportHit(Vector3 normal, float distance, Collider collider)
        {
            Normal = normal;
            Distance = distance;
            Collider = collider;
        }
    }
}
