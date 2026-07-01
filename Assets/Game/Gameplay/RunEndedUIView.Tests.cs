#if UNITY_INCLUDE_TESTS

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunEndedUIView
    {
        internal void SetReferencesForTests(
            GameObject root,
            TMP_Text titleText,
            TMP_Text earnedCoinsText,
            TMP_Text reachedDistanceText,
            GameObject bestImprovementRoot,
            TMP_Text bestImprovementText,
            Button acknowledgeTouchAreaButton)
        {
            _root = root;
            _titleText = titleText;
            _earnedCoinsText = earnedCoinsText;
            _reachedDistanceText = reachedDistanceText;
            _bestImprovementRoot = bestImprovementRoot;
            _bestImprovementText = bestImprovementText;
            _acknowledgeTouchAreaButton = acknowledgeTouchAreaButton;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
