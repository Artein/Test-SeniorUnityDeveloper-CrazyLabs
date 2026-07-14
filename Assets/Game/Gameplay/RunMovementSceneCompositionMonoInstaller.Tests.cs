#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using System.Linq;

namespace Game.Gameplay
{
    public sealed partial class RunMovementSceneCompositionMonoInstaller
    {
        internal IReadOnlyList<string> GetReferenceValidationErrorsForTests()
        {
            return GetReferenceValidationErrors().ToArray();
        }

        internal void SetReferencesForTests(
            RunBodyMovementConfig config,
            RigidbodyRunBodyMovementTarget movementTarget,
            RigidbodyRunCameraSource runCameraSource,
            RunProgressFrameSource progressFrameSource,
            RigidbodyContactNotifier contactNotifier)
        {
            _config = config;
            _movementTarget = movementTarget;
            _runCameraSource = runCameraSource;
            _progressFrameSource = progressFrameSource;
            _contactNotifier = contactNotifier;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
