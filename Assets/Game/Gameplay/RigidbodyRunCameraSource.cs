using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunCameraSource : MonoBehaviour, IRunCameraSource
    {
        [SerializeField] private Rigidbody _rigidbody;

        Vector3 IRunCameraSource.Position
        {
            get
            {
                UnityEngine.Assertions.Assert.IsNotNull(_rigidbody, "RigidbodyRunCameraSource requires a Rigidbody reference.");
                return _rigidbody.position;
            }
        }

        Vector3 IRunCameraSource.LinearVelocity
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
