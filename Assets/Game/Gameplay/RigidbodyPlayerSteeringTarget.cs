using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyPlayerSteeringTarget : MonoBehaviour, IPlayerSteeringTarget
    {
        [SerializeField] private Rigidbody _rigidbody;

        Vector3 IPlayerSteeringTarget.LinearVelocity
        {
            get
            {
                UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyPlayerSteeringTarget requires a Rigidbody reference.");
                return _rigidbody.linearVelocity;
            }
        }

        void IPlayerSteeringTarget.ApplySteering(Vector3 linearVelocity, Quaternion rotation)
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyPlayerSteeringTarget requires a Rigidbody reference.");

            ApplyLinearVelocity(linearVelocity);
            _rigidbody.MoveRotation(rotation);
        }

        void IPlayerSteeringTarget.ApplyVelocity(Vector3 linearVelocity)
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyPlayerSteeringTarget requires a Rigidbody reference.");

            ApplyLinearVelocity(linearVelocity);
        }

        private void ApplyLinearVelocity(Vector3 linearVelocity)
        {
            if (!linearVelocity.IsFinite())
                throw new ArgumentException("Steering velocity must be finite.", nameof(linearVelocity));

            _rigidbody.linearVelocity = linearVelocity;
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning("RigidbodyPlayerSteeringTarget requires a Rigidbody reference.", this);
        }
    }
}
