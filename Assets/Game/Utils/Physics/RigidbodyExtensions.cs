using System;
using UnityEngine;

namespace Game.Utils.Physics
{
    public static class RigidbodyExtensions
    {
        public static void ClearVelocityIfDynamic(this Rigidbody rigidbody)
        {
            if (rigidbody == null)
                throw new ArgumentNullException(nameof(rigidbody));

            if (rigidbody.isKinematic)
                return;

            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
