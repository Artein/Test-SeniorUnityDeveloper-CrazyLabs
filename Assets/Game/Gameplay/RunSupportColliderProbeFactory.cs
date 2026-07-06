using System;
using UnityEngine;

namespace Game.Gameplay
{
    internal interface IRunSupportColliderProbeFactory
    {
        IRunSupportColliderProbe Create(Collider collider);
    }

    internal sealed class RunSupportColliderProbeFactory : IRunSupportColliderProbeFactory
    {
        public IRunSupportColliderProbe Create(Collider collider)
        {
            if (collider == null)
                throw new ArgumentNullException(nameof(collider));

            return collider switch
            {
                CapsuleCollider capsule => new CapsuleRunSupportColliderProbe(capsule),
                SphereCollider sphere => new SphereRunSupportColliderProbe(sphere),
                BoxCollider box => new BoxRunSupportColliderProbe(box),
                _ => new BoundsRunSupportColliderProbe(collider)
            };
        }
    }
}
