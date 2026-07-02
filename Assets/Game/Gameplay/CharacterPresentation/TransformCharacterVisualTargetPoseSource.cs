using System;
using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    internal sealed class TransformCharacterVisualTargetPoseSource : ICharacterVisualTargetPoseSource
    {
        private readonly Transform _targetTransform;

        public CharacterVisualPose CurrentPose => new(_targetTransform.position, _targetTransform.rotation);

        public TransformCharacterVisualTargetPoseSource(Transform targetTransform)
        {
            _targetTransform = targetTransform != null ? targetTransform : throw new ArgumentNullException(nameof(targetTransform));
        }
    }
}
