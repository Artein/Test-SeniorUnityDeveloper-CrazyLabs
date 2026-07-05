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

        Vector3 GetSupportProbeOrigin(Vector3 upDirection, float skinWidth);
    }
}
