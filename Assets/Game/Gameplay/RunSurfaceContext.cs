using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public readonly struct RunSurfaceContext
    {
        public bool IsGrounded { get; }
        public Vector3 GroundNormal { get; }
        public float ForwardDownhillDegrees { get; }

        public RunSurfaceContext(bool isGrounded, Vector3 groundNormal, float forwardDownhillDegrees)
        {
            IsGrounded = isGrounded;

            if (!isGrounded)
            {
                GroundNormal = Vector3.up;
                ForwardDownhillDegrees = 0f;
                return;
            }

            if (!groundNormal.IsFinite() || groundNormal.sqrMagnitude <= 0.000001f)
            {
                GroundNormal = Vector3.up;
            }
            else
            {
                var normalizedGroundNormal = groundNormal.normalized;
                GroundNormal = normalizedGroundNormal.IsFinite() ? normalizedGroundNormal : Vector3.up;
            }

            ForwardDownhillDegrees = float.IsFinite(forwardDownhillDegrees) ? forwardDownhillDegrees : 0f;
        }
    }
}
