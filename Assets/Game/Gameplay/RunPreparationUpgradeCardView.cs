using System;
using Game.Gameplay.Upgrades;
using Game.Utils.Invocation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunPreparationUpgradeCardView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _effectText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_Text _buyButtonActionLabel;
        [SerializeField] private Image _buyButtonCostIcon;
        [SerializeField] private TMP_Text _buyButtonCostText;

        private UpgradeDefinition _definition;

        private GameObject RootObject => _root != null ? _root : gameObject;

        public event Action<UpgradeDefinition> BuyRequested;

        private void Awake()
        {
            ValidateSerializedReferences();
        }

        private void OnEnable()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(OnBuyClicked);
        }

        private void OnDisable()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(OnBuyClicked);
        }

        public void SetVisible(bool isVisible)
        {
            RootObject.SetActive(isVisible);
        }

        public void Render(RunPreparationUpgradeViewState state)
        {
            ValidateSerializedReferences();

            _definition = state.Definition;
            RootObject.name = "Upgrade Card - " + state.StableId;
            _icon.name = "Upgrade Icon - " + state.StableId;
            _icon.sprite = state.Icon;
            _icon.enabled = state.Icon != null;
            _nameText.name = "Upgrade Name Label";
            _nameText.text = state.CardTitle;
            _levelText.name = "Upgrade Level Label";
            _levelText.text = state.OfferLevelText;
            _effectText.name = "Upgrade Effect Label";
            _effectText.text = state.OfferEffectText;
            RenderButtonCost(state);
            _buyButton.name = "Buy Button - " + state.StableId;
            _buyButton.interactable = state.CanBuy;
            _buyButtonActionLabel.name = "Upgrade Button Action Label";
            _buyButtonActionLabel.text = state.ButtonText;
        }

        private void RenderButtonCost(RunPreparationUpgradeViewState state)
        {
            var hasCost = state.CostCurrencyIcon != null && !string.IsNullOrEmpty(state.NextCostText);

            _buyButtonCostIcon.name = "Upgrade Button Cost Currency Icon";
            _buyButtonCostIcon.sprite = state.CostCurrencyIcon;
            _buyButtonCostIcon.enabled = hasCost;
            _buyButtonCostIcon.gameObject.SetActive(hasCost);

            _buyButtonCostText.name = "Upgrade Button Cost Label";
            _buyButtonCostText.text = hasCost ? state.NextCostText : string.Empty;
            _buyButtonCostText.gameObject.SetActive(hasCost);
        }

        private void ValidateSerializedReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_icon, $"{nameof(RunPreparationUpgradeCardView)} requires an icon reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_nameText, $"{nameof(RunPreparationUpgradeCardView)} requires a name text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_levelText, $"{nameof(RunPreparationUpgradeCardView)} requires a level text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_effectText, $"{nameof(RunPreparationUpgradeCardView)} requires an effect text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_buyButton, $"{nameof(RunPreparationUpgradeCardView)} requires a buy button reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_buyButtonActionLabel,
                $"{nameof(RunPreparationUpgradeCardView)} requires a buy button action label reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_buyButtonCostIcon,
                $"{nameof(RunPreparationUpgradeCardView)} requires a buy button cost icon reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_buyButtonCostText,
                $"{nameof(RunPreparationUpgradeCardView)} requires a buy button cost text reference.");
        }

        private void OnBuyClicked()
        {
            BuyRequested?.InvokeSafely(_definition);
        }
    }
}
