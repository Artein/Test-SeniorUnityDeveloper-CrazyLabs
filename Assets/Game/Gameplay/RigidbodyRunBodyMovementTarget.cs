using System;
using System.Diagnostics;
using Game.Utils.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunBodyMovementTarget : MonoBehaviour, IRunBodyMovementTarget
    {
        [SerializeField] private Rigidbody _rigidbody;

        Vector3 IRunBodyMovementTarget.LinearVelocity
        {
            get
            {
                Assert.IsNotNull(_rigidbody, message: "RigidbodyRunBodyMovementTarget requires a Rigidbody reference.");
                return _rigidbody.linearVelocity;
            }
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning(message: "RigidbodyRunBodyMovementTarget requires a Rigidbody reference.", this);
        }

        void IRunBodyMovementTarget.ApplyTargetState(RunBodyMovementTargetState targetState)
        {
            Assert.IsNotNull(_rigidbody, message: "RigidbodyRunBodyMovementTarget requires a Rigidbody reference.");

            if (!targetState.LinearVelocity.IsFinite())
                throw new ArgumentException(message: "Run Body movement velocity must be finite.", nameof(targetState));

            _rigidbody.linearVelocity = targetState.LinearVelocity;

            if (targetState.HasRotation)
                _rigidbody.MoveRotation(targetState.Rotation);

            RecordSuccessfulTargetWriteForTests();
        }

        [Conditional(conditionString: "UNITY_INCLUDE_TESTS")]
        partial void RecordSuccessfulTargetWriteForTests();
    }
}
