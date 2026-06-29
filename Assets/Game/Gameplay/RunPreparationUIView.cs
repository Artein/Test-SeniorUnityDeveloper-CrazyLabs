using System;
using System.Collections.Generic;
using Game.Gameplay.Upgrades;
using Game.Utils.Invocation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed class RunPreparationUIView : MonoBehaviour, IRunPreparationView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _coinBalanceIcon;
        [SerializeField] private TMP_Text _coinBalanceText;
        [SerializeField] private Button _continueTouchAreaButton;
        [SerializeField] private RunPreparationUpgradeCardView[] _upgradeCards = Array.Empty<RunPreparationUpgradeCardView>();

        public event Action<UpgradeDefinition> BuyRequested;
        public event Action ContinueRequested;

        public void Render(RunPreparationViewState state)
        {
            ValidateSerializedReferences();

            RootObject.SetActive(state.IsVisible);
            _coinBalanceIcon.sprite = state.CurrencyIcon;
            _coinBalanceIcon.enabled = state.CurrencyIcon != null;
            _coinBalanceText.text = state.CoinBalanceText;
            RenderCards(state.Upgrades);
        }

        private void Awake()
        {
            ValidateSerializedReferences();
        }

        private void OnEnable()
        {
            if (_continueTouchAreaButton != null)
                _continueTouchAreaButton.onClick.AddListener(OnContinueTouchAreaClicked);

            foreach (var upgradeCard in _upgradeCards)
            {
                if (upgradeCard != null)
                    upgradeCard.BuyRequested += OnCardBuyRequested;
            }
        }

        private void OnDisable()
        {
            if (_continueTouchAreaButton != null)
                _continueTouchAreaButton.onClick.RemoveListener(OnContinueTouchAreaClicked);

            foreach (var upgradeCard in _upgradeCards)
            {
                if (upgradeCard != null)
                    upgradeCard.BuyRequested -= OnCardBuyRequested;
            }
        }

        private GameObject RootObject => _root != null ? _root : gameObject;

        private void RenderCards(IReadOnlyList<RunPreparationUpgradeViewState> upgrades)
        {
            UnityEngine.Assertions.Assert.IsTrue(
                upgrades.Count <= _upgradeCards.Length,
                $"{nameof(RunPreparationUIView)} has fewer authored upgrade cards than the catalog requires.");

            for (var index = 0; index < _upgradeCards.Length; index += 1)
            {
                var upgradeCard = _upgradeCards[index];
                var isActive = index < upgrades.Count;

                upgradeCard.SetVisible(isActive);

                if (isActive)
                    upgradeCard.Render(upgrades[index]);
            }
        }

        private void ValidateSerializedReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_coinBalanceIcon, $"{nameof(RunPreparationUIView)} requires a coin balance icon reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_coinBalanceText, $"{nameof(RunPreparationUIView)} requires a coin balance text reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_continueTouchAreaButton,
                $"{nameof(RunPreparationUIView)} requires a continue touch area button reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_upgradeCards, $"{nameof(RunPreparationUIView)} requires authored upgrade card views.");

            UnityEngine.Assertions.Assert.IsTrue(_upgradeCards.Length > 0,
                $"{nameof(RunPreparationUIView)} requires at least one authored upgrade card view.");

            foreach (var upgradeCard in _upgradeCards)
            {
                UnityEngine.Assertions.Assert.IsNotNull(upgradeCard, $"{nameof(RunPreparationUIView)} has a missing authored upgrade card view.");
            }
        }

        private void OnCardBuyRequested(UpgradeDefinition definition)
        {
            BuyRequested?.InvokeSafely(definition);
        }

        private void OnContinueTouchAreaClicked()
        {
            ContinueRequested?.InvokeSafely();
        }
    }
}
