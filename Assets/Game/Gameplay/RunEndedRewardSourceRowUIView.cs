using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunEndedRewardSourceRowUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Graphic[] _revealGraphics;

        private RunEndedRewardSourceRowViewState _state;

        public void Apply(RunEndedRewardSourceRowViewState state)
        {
            ValidateSerializedReferences();
            _state = state;
            _labelText.text = state.LabelText;
            _amountText.text = state.AmountText;
            SetRevealAlpha(1f);
        }

        public void ApplyRevealFrame(float alpha, int displayedAmount)
        {
            ValidateSerializedReferences();
            var clampedAmount = Mathf.Clamp(displayedAmount, 0, _state.Amount);
            _labelText.text = _state.LabelText;

            _amountText.text = clampedAmount >= _state.Amount
                ? _state.AmountText
                : clampedAmount.ToString(CultureInfo.InvariantCulture);
            SetRevealAlpha(alpha);
        }

        private void Awake()
        {
            ValidateSerializedReferences();
        }

        private void ValidateSerializedReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_labelText, $"{nameof(RunEndedRewardSourceRowUIView)} requires a label text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_amountText, $"{nameof(RunEndedRewardSourceRowUIView)} requires an amount text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_revealGraphics, $"{nameof(RunEndedRewardSourceRowUIView)} requires reveal graphic references.");

            UnityEngine.Assertions.Assert.IsTrue(_revealGraphics.Length > 0,
                $"{nameof(RunEndedRewardSourceRowUIView)} requires at least one reveal graphic reference.");

            foreach (var revealGraphic in _revealGraphics)
            {
                UnityEngine.Assertions.Assert.IsNotNull(revealGraphic,
                    $"{nameof(RunEndedRewardSourceRowUIView)} has a missing reveal graphic reference.");
            }
        }

        private void SetRevealAlpha(float alpha)
        {
            foreach (var revealGraphic in _revealGraphics)
            {
                SetGraphicAlpha(revealGraphic, alpha);
            }
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            var color = graphic.color;
            color.a = Mathf.Clamp01(alpha);
            graphic.color = color;
        }
    }
}
