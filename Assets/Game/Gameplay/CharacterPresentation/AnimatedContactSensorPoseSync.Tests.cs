#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;

namespace Game.Gameplay.CharacterPresentation
{
    public sealed partial class AnimatedContactSensorPoseSyncView
    {
        internal void SetReferencesForTests(
            UnityEngine.Rigidbody rootRigidbody,
            IReadOnlyList<AnimatedContactSensorPoseBinding> bindings)
        {
            _rootRigidbody = rootRigidbody;
            _bindings = bindings == null ? null : new AnimatedContactSensorPoseBinding[bindings.Count];

            if (bindings == null)
                return;

            for (var bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex += 1)
            {
                _bindings[bindingIndex] = bindings[bindingIndex];
            }
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
