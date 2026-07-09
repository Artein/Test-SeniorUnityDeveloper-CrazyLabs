#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunSteeringAffordanceView
    {
        internal void SetReferencesForTests(
            RectTransform root,
            CanvasGroup canvasGroup,
            RectTransform knobRoot,
            Image knobImage,
            RectTransform leftRangeEndRoot,
            Image leftRangeEndImage,
            RectTransform rightRangeEndRoot,
            Image rightRangeEndImage,
            RectTransform deadzoneRoot,
            Image deadzoneImage)
        {
            _root = root;
            _canvasGroup = canvasGroup;
            _knobRoot = knobRoot;
            _knobImage = knobImage;
            _leftRangeEndRoot = leftRangeEndRoot;
            _leftRangeEndImage = leftRangeEndImage;
            _rightRangeEndRoot = rightRangeEndRoot;
            _rightRangeEndImage = rightRangeEndImage;
            _deadzoneRoot = deadzoneRoot;
            _deadzoneImage = deadzoneImage;
            ApplyNonInteractiveSettings();
        }

        internal void SetAnimationDurationsForTests(float showSeconds, float hideSeconds)
        {
            _showSeconds = showSeconds;
            _hideSeconds = hideSeconds;
        }

        internal void TickAnimationForTests(float deltaSeconds)
        {
            TickAnimation(deltaSeconds);
        }

        internal IReadOnlyList<string> GetReferenceValidationErrorsForTests()
        {
            return GetReferenceValidationErrors().ToArray();
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
