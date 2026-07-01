#if UNITY_INCLUDE_TESTS

using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunEndedUIView
    {
        internal IReadOnlyList<RunEndedRewardSourceRowUIView> RewardSourceRowsForTests => _rewardSourceRows;
        internal IReadOnlyList<Graphic> EarnedCoinsRevealGraphicsForTests => _earnedCoinsRevealGraphics;
        internal bool IsRevealRunningForTests => _isRevealRunning;
        internal bool CanAcknowledgeForTests => _canAcknowledge;

        internal void SetReferencesForTests(
            GameObject root,
            TMP_Text titleText,
            TMP_Text earnedCoinsText,
            Graphic[] earnedCoinsRevealGraphics,
            TMP_Text reachedDistanceText,
            Transform rewardSourceRowsRoot,
            RunEndedRewardSourceRowUIView rewardSourceRowPrefab,
            GameObject bestImprovementRoot,
            TMP_Text bestImprovementText,
            GameObject tapToContinueRoot,
            Button acknowledgeTouchAreaButton)
        {
            _root = root;
            _titleText = titleText;
            _earnedCoinsText = earnedCoinsText;
            _earnedCoinsRevealGraphics = earnedCoinsRevealGraphics;
            _reachedDistanceText = reachedDistanceText;
            _rewardSourceRowsRoot = rewardSourceRowsRoot;
            _rewardSourceRowPrefab = rewardSourceRowPrefab;
            _bestImprovementRoot = bestImprovementRoot;
            _bestImprovementText = bestImprovementText;
            _tapToContinueRoot = tapToContinueRoot;
            _acknowledgeTouchAreaButton = acknowledgeTouchAreaButton;
        }

        internal void SetRevealTimingsForTests(float labelFadeDuration, float counterDuration, float stepDelay)
        {
            _labelFadeDuration = labelFadeDuration;
            _counterDuration = counterDuration;
            _stepDelay = stepDelay;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
