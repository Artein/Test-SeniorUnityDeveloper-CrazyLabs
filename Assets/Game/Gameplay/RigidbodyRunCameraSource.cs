using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunCameraSource : MonoBehaviour, IRunCameraSource, IRunMotionSource
    {
        [SerializeField] private Rigidbody _rigidbody;

        public Vector3 Position
        {
            get
            {
                UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyRunCameraSource requires a Rigidbody reference.");
                return _rigidbody.transform.position;
            }
        }

        public Vector3 LinearVelocity
        {
            get
            {
                UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyRunCameraSource requires a Rigidbody reference.");
                return _rigidbody.linearVelocity;
            }
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning("RigidbodyRunCameraSource requires a Rigidbody reference.", this);
        }
    }
}
