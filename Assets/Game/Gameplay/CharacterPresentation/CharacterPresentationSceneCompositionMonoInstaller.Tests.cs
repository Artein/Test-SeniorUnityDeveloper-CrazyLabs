#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public sealed partial class CharacterPresentationSceneCompositionMonoInstaller
    {
        internal IReadOnlyList<string> GetReferenceValidationErrorsForTests()
        {
            return GetReferenceValidationErrors().ToArray();
        }

        internal void SetReferencesForTests(
            CharacterPresentationView view,
            Transform visualTarget,
            AnimatedContactSensorPoseSyncView animatedContactSensorPoseSyncView)
        {
            _view = view;
            _visualTarget = visualTarget;
            _animatedContactSensorPoseSyncView = animatedContactSensorPoseSyncView;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
