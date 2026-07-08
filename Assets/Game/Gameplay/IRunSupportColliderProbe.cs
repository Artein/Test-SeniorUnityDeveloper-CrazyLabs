using UnityEngine;

namespace Game.Gameplay
{
    internal interface IRunSupportColliderProbe
    {
        Collider Collider { get; }

        int Cast(
            Vector3 castOffset,
            Vector3 direction,
            float distance,
            RaycastHit[] results,
            LayerMask surfaceMask);

        int Overlap(Collider[] results, LayerMask surfaceMask);

        float GetProjectedFootprintExtent(Vector3 direction);

        bool TryGetSupportSampleOrigin(
            Vector3 upDirection,
            Vector3 lateralOffset,
            float skinWidth,
            out Vector3 origin);
    }
}
