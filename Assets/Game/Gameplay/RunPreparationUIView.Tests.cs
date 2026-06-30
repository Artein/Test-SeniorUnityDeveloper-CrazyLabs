#if UNITY_INCLUDE_TESTS

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunPreparationUIView
    {
        internal void SetReferencesForTests(
            GameObject root,
            Image coinBalanceIcon,
            TMP_Text coinBalanceText,
            Button continueTouchAreaButton,
            RunPreparationUpgradeCardView[] upgradeCards)
        {
            _root = root;
            _coinBalanceIcon = coinBalanceIcon;
            _coinBalanceText = coinBalanceText;
            _continueTouchAreaButton = continueTouchAreaButton;
            _upgradeCards = upgradeCards ?? Array.Empty<RunPreparationUpgradeCardView>();
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
