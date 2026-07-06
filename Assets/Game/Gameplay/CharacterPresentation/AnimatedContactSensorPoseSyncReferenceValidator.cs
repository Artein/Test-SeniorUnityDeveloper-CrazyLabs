using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    internal sealed class AnimatedContactSensorPoseSyncReferenceValidator
    {
        public IEnumerable<string> GetReferenceValidationErrors(
            Rigidbody rootRigidbody,
            IReadOnlyList<AnimatedContactSensorPoseBinding> bindings)
        {
            if (rootRigidbody == null)
            {
                yield return "AnimatedContactSensorPoseSyncView requires an Animated Contact Sensor Physics Root Rigidbody reference.";
            }
            else if (!rootRigidbody.isKinematic)
            {
                yield return "AnimatedContactSensorPoseSyncView requires a kinematic Rigidbody on the Animated Contact Sensor Physics Root.";
            }

            var safeBindings = bindings ?? Array.Empty<AnimatedContactSensorPoseBinding>();

            if (safeBindings.Count <= 0)
            {
                yield return "AnimatedContactSensorPoseSyncView requires at least one Animated Contact Sensor Pose Binding.";
                yield break;
            }

            var uniqueTargets = new HashSet<Transform>(safeBindings.Count);

            for (var bindingIndex = 0; bindingIndex < safeBindings.Count; bindingIndex += 1)
            {
                var binding = safeBindings[bindingIndex];

                if (binding.Source == null)
                    yield return $"AnimatedContactSensorPoseSyncView binding at index {bindingIndex} is missing a Source Transform.";

                if (binding.Target == null)
                {
                    yield return $"AnimatedContactSensorPoseSyncView binding at index {bindingIndex} is missing a Target Transform.";
                    continue;
                }

                if (!uniqueTargets.Add(binding.Target))
                    yield return $"AnimatedContactSensorPoseSyncView contains duplicate Target Transform '{binding.Target.name}'.";

                if (rootRigidbody != null && !binding.Target.IsChildOf(rootRigidbody.transform))
                {
                    yield return
                        $"AnimatedContactSensorPoseSyncView Target Transform '{binding.Target.name}' must be inside the Animated Contact Sensor Physics Root.";
                }
            }
        }
    }
}
