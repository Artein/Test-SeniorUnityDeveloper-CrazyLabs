#if UNITY_INCLUDE_TESTS

using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class PullHintView
    {
        internal void SetReferencesForTests(
            RectTransform root,
            CanvasGroup canvasGroup,
            RectTransform fingerRoot,
            Image fingerImage,
            Animator animator,
            float initialIdleDelaySeconds,
            float playbackDurationSeconds,
            float repeatCooldownSeconds,
            Vector2 screenOffset,
            string playTriggerName)
        {
            _root = root;
            _canvasGroup = canvasGroup;
            _fingerRoot = fingerRoot;
            _fingerImage = fingerImage;
            _animator = animator;
            _initialIdleDelaySeconds = initialIdleDelaySeconds;
            _playbackDurationSeconds = playbackDurationSeconds;
            _repeatCooldownSeconds = repeatCooldownSeconds;
            _screenOffset = screenOffset;
            _playTriggerName = playTriggerName;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
