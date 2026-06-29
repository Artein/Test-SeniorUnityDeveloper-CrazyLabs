#if UNITY_INCLUDE_TESTS

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunPreparationUpgradeCardView
    {
        internal void SetReferencesForTests(
            GameObject root,
            Image icon,
            TMP_Text nameText,
            TMP_Text levelText,
            TMP_Text effectText,
            Button buyButton,
            TMP_Text buyButtonActionLabel,
            Image buyButtonCostIcon,
            TMP_Text buyButtonCostText)
        {
            _root = root;
            _icon = icon;
            _nameText = nameText;
            _levelText = levelText;
            _effectText = effectText;
            _buyButton = buyButton;
            _buyButtonActionLabel = buyButtonActionLabel;
            _buyButtonCostIcon = buyButtonCostIcon;
            _buyButtonCostText = buyButtonCostText;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
