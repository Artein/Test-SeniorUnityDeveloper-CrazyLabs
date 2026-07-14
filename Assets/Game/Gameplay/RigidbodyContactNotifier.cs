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
        public int ContactCount => _contacts.Length;

        public Collider OtherCollider { get; }
        public Vector3 RelativeVelocity { get; }

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
        private readonly RigidbodyCollisionApproachVelocityResolver _approachVelocityResolver = new();
        private Rigidbody _rigidbody;

        public event Action<RigidbodyCollisionNotification> CollisionEntered;
        public event Action<RigidbodyTriggerNotification> TriggerEntered;

        private void Awake()
        {
            _rigidbody = gameObject.GetComponent<Rigidbody>();
        }

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

            CollisionEntered?.Invoke(
                new RigidbodyCollisionNotification(
                    collision.collider,
                    ResolveRelativeVelocity(collision),
                    contacts));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null)
                return;

            TriggerEntered?.Invoke(new RigidbodyTriggerNotification(other));
        }

        private Vector3 ResolveRelativeVelocity(Collision collision)
        {
            var reportedRelativeVelocity = collision.relativeVelocity;
            var otherCollider = collision.collider;

            if (_rigidbody == null || otherCollider == null)
                return reportedRelativeVelocity;

            var otherBodyAttached = otherCollider.attachedRigidbody != null
                                    || otherCollider.attachedArticulationBody != null;

            return _approachVelocityResolver.TryResolve(
                reportedRelativeVelocity,
                _rigidbody.linearVelocity,
                collision.impulse,
                _rigidbody.mass,
                otherBodyAttached,
                out var resolvedRelativeVelocity)
                ? resolvedRelativeVelocity
                : reportedRelativeVelocity;
        }
    }

    internal sealed class RigidbodyCollisionApproachVelocityResolver
    {
        public bool TryResolve(
            Vector3 reportedRelativeVelocity,
            Vector3 postSolveBodyVelocity,
            Vector3 collisionImpulse,
            float bodyMass,
            bool otherBodyAttached,
            out Vector3 resolvedRelativeVelocity)
        {
            resolvedRelativeVelocity = Vector3.zero;

            if (!reportedRelativeVelocity.IsFinite())
                return false;

            if (reportedRelativeVelocity.sqrMagnitude > 0.000001f)
            {
                resolvedRelativeVelocity = reportedRelativeVelocity;
                return true;
            }

            if (otherBodyAttached
                || !postSolveBodyVelocity.IsFinite()
                || !collisionImpulse.IsFinite()
                || collisionImpulse.sqrMagnitude <= 0.000001f
                || !float.IsFinite(bodyMass)
                || bodyMass <= 0f)
            {
                return false;
            }

            var reconstructedVelocity = postSolveBodyVelocity - collisionImpulse / bodyMass;

            if (!reconstructedVelocity.IsFinite() || reconstructedVelocity.sqrMagnitude <= 0.000001f)
                return false;

            resolvedRelativeVelocity = reconstructedVelocity;
            return true;
        }
    }
}
