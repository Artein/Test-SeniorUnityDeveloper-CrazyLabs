#if UNITY_INCLUDE_TESTS

using Unity.Cinemachine;

namespace Game.Gameplay
{
    public sealed partial class CinemachineRunCameraRig
    {
        internal void SetReferencesForTests(CinemachineCamera preLaunchCamera, CinemachineCamera runCamera)
        {
            _preLaunchCamera = preLaunchCamera;
            _runCamera = runCamera;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
