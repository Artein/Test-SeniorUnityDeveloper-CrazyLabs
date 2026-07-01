using Unity.Cinemachine;
using UnityEngine;

namespace Game.Gameplay
{
    public sealed partial class CinemachineRunCameraRig : MonoBehaviour, IRunCameraRig
    {
        [SerializeField] private CinemachineCamera _runPreparationCamera;
        [SerializeField] private CinemachineCamera _preLaunchCamera;
        [SerializeField] private CinemachineCamera _runCamera;
        [SerializeField] private int _inactiveCameraPriority = 0;
        [SerializeField] private int _runPreparationCameraPriority = 10;
        [SerializeField] private int _preLaunchCameraPriority = 10;
        [SerializeField] private int _runCameraPriority = 20;

        void IRunCameraRig.ActivateRunPreparationCamera()
        {
            SetCameraPriorities(
                _runPreparationCameraPriority,
                _inactiveCameraPriority,
                _inactiveCameraPriority);
        }

        void IRunCameraRig.ActivatePreLaunchCamera()
        {
            SetCameraPriorities(
                _inactiveCameraPriority,
                _preLaunchCameraPriority,
                _inactiveCameraPriority);
        }

        void IRunCameraRig.ActivateRunCamera()
        {
            SetCameraPriorities(
                _inactiveCameraPriority,
                _inactiveCameraPriority,
                _runCameraPriority);
        }

        private void SetCameraPriorities(
            int runPreparationCameraPriority,
            int preLaunchCameraPriority,
            int runCameraPriority)
        {
            AssertReferences();

            _runPreparationCamera.Priority.Value = runPreparationCameraPriority;
            _preLaunchCamera.Priority.Value = preLaunchCameraPriority;
            _runCamera.Priority.Value = runCameraPriority;
        }

        private void AssertReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_runPreparationCamera, "CinemachineRunCameraRig requires a Run Preparation Camera reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_preLaunchCamera, "CinemachineRunCameraRig requires a Pre-Launch Camera reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_runCamera, "CinemachineRunCameraRig requires a Run Camera reference.");
        }

        private void OnValidate()
        {
            if (_runPreparationCamera == null)
                Debug.LogWarning("CinemachineRunCameraRig requires a Run Preparation Camera reference.", this);

            if (_preLaunchCamera == null)
                Debug.LogWarning("CinemachineRunCameraRig requires a Pre-Launch Camera reference.", this);

            if (_runCamera == null)
                Debug.LogWarning("CinemachineRunCameraRig requires a Run Camera reference.", this);
        }
    }
}
