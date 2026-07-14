using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class RigidbodyRunCameraSource : MonoBehaviour, IRunCameraSource, IRunMotionSource
    {
        [SerializeField] private Rigidbody _rigidbody;

        Vector3 IRunCameraSource.LinearVelocity => RequiredRigidbody.linearVelocity;
        Vector3 IRunCameraSource.Position => RequiredRigidbody.transform.position;
        Vector3 IRunMotionSource.LinearVelocity => RequiredRigidbody.linearVelocity;
        Vector3 IRunMotionSource.Position => RequiredRigidbody.position;

        private Rigidbody RequiredRigidbody
        {
            get
            {
                UnityEngine.Assertions.Assert.IsNotNull(
                    _rigidbody,
                    message: "RigidbodyRunCameraSource requires a Rigidbody reference.");

                return _rigidbody;
            }
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                Debug.LogWarning(message: "RigidbodyRunCameraSource requires a Rigidbody reference.", this);
        }
    }
}
