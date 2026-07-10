using System;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunBodyMovementTarget : MonoBehaviour, IRunBodyMovementTarget
    {
        [SerializeField] private Rigidbody _rigidbody;

        Vector3 IRunBodyMovementTarget.LinearVelocity
        {
            get
            {
                UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyRunBodyMovementTarget requires a Rigidbody reference.");
                return _rigidbody.linearVelocity;
            }
        }

        void IRunBodyMovementTarget.ApplyTargetState(RunBodyMovementTargetState targetState)
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyRunBodyMovementTarget requires a Rigidbody reference.");

            if (!targetState.LinearVelocity.IsFinite())
                throw new ArgumentException("Run Body movement velocity must be finite.", nameof(targetState));

            _rigidbody.linearVelocity = targetState.LinearVelocity;

            if (targetState.HasRotation)
                _rigidbody.MoveRotation(targetState.Rotation);
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning("RigidbodyRunBodyMovementTarget requires a Rigidbody reference.", this);
        }
    }
}
