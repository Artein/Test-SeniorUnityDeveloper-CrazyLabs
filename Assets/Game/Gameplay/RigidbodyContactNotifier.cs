using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IRigidbodyContactNotifier
    {
        event Action<RigidbodyCollisionNotification> CollisionEntered;
        event Action<RigidbodyTriggerNotification> TriggerEntered;
    }

    public readonly struct RunContactPoint
    {
        public Vector3 Point { get; }
        public Vector3 Normal { get; }

        public RunContactPoint(Vector3 point, Vector3 normal)
        {
            Point = point;
            Normal = normal;
        }
    }

    public sealed class RigidbodyCollisionNotification
    {
        private readonly RunContactPoint[] _contacts;

        public Collider OtherCollider { get; }
        public Vector3 RelativeVelocity { get; }
        public int ContactCount => _contacts.Length;

        public RigidbodyCollisionNotification(
            Collider otherCollider,
            Vector3 relativeVelocity,
            RunContactPoint[] contacts)
        {
            OtherCollider = otherCollider;
            RelativeVelocity = relativeVelocity.IsFinite() ? relativeVelocity : Vector3.zero;
            _contacts = contacts != null ? (RunContactPoint[])contacts.Clone() : Array.Empty<RunContactPoint>();
        }

        public RunContactPoint GetContact(int index)
        {
            return _contacts[index];
        }
    }

    public sealed class RigidbodyTriggerNotification
    {
        public Collider OtherCollider { get; }

        public RigidbodyTriggerNotification(Collider otherCollider)
        {
            OtherCollider = otherCollider;
        }
    }

    public sealed class RigidbodyContactNotifier : MonoBehaviour, IRigidbodyContactNotifier
    {
        public event Action<RigidbodyCollisionNotification> CollisionEntered;
        public event Action<RigidbodyTriggerNotification> TriggerEntered;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            var contacts = new RunContactPoint[collision.contactCount];

            for (var contactIndex = 0; contactIndex < contacts.Length; contactIndex += 1)
            {
                var contact = collision.GetContact(contactIndex);
                contacts[contactIndex] = new RunContactPoint(contact.point, contact.normal);
            }

            CollisionEntered?.Invoke(new RigidbodyCollisionNotification(
                collision.collider,
                collision.relativeVelocity,
                contacts));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
                return;

            TriggerEntered?.Invoke(new RigidbodyTriggerNotification(other));
        }
    }
}
