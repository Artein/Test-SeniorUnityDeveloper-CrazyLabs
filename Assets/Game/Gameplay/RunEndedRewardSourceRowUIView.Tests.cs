#if UNITY_INCLUDE_TESTS

using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunEndedRewardSourceRowUIView
    {
        internal TMP_Text LabelTextForTests => _labelText;
        internal TMP_Text AmountTextForTests => _amountText;
        internal IReadOnlyList<Graphic> RevealGraphicsForTests => _revealGraphics;

        internal void SetReferencesForTests(TMP_Text labelText, TMP_Text amountText, params Graphic[] revealGraphics)
        {
            _labelText = labelText;
            _amountText = amountText;
            _revealGraphics = revealGraphics;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
