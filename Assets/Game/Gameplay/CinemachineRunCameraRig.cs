using Unity.Cinemachine;
using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class CinemachineRunCameraRig : MonoBehaviour, IRunCameraRig
    {
        [SerializeField] private CinemachineCamera _preLaunchCamera;
        [SerializeField] private CinemachineCamera _runCamera;

        void IRunCameraRig.SetCameraPriorities(int preLaunchCameraPriority, int runCameraPriority)
        {
            UnityEngine.Assertions.Assert.IsNotNull(_preLaunchCamera, "CinemachineRunCameraRig requires a Pre-Launch Camera reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_runCamera, "CinemachineRunCameraRig requires a Run Camera reference.");

            _preLaunchCamera.Priority.Value = preLaunchCameraPriority;
            _runCamera.Priority.Value = runCameraPriority;
        }

        private void OnValidate()
        {
            if (_preLaunchCamera == null)
                Debug.LogWarning("CinemachineRunCameraRig requires a Pre-Launch Camera reference.", this);

            if (_runCamera == null)
                Debug.LogWarning("CinemachineRunCameraRig requires a Run Camera reference.", this);
        }
    }
}
