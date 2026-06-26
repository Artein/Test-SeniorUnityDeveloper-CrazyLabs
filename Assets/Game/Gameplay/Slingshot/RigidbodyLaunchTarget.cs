using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ILaunchTarget
    {
        void Hold();
        void Launch(Vector3 velocity);
    }

    public sealed partial class RigidbodyLaunchTarget : MonoBehaviour, ILaunchTarget
    {
        [SerializeField] private Rigidbody _rigidbody;

        private bool _isHeld;
        private bool _previousIsKinematic;
        private RigidbodyConstraints _previousConstraints;

        void ILaunchTarget.Hold()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyLaunchTarget requires a Rigidbody reference.");

            if (!_isHeld)
            {
                _previousIsKinematic = _rigidbody.isKinematic;
                _previousConstraints = _rigidbody.constraints;
                _isHeld = true;
            }

            _rigidbody.isKinematic = true;
            ClearVelocity();
        }

        void ILaunchTarget.Launch(Vector3 velocity)
        {
            UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyLaunchTarget requires a Rigidbody reference.");

            if (_isHeld)
            {
                _rigidbody.isKinematic = _previousIsKinematic;
                _rigidbody.constraints = _previousConstraints;
                _isHeld = false;
            }

            ClearVelocity();
            _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning("RigidbodyLaunchTarget requires a Rigidbody reference.", this);
        }

        private void ClearVelocity()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
