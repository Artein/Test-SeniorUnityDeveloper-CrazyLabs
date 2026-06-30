#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay.Economy
{
    public sealed partial class CurrencyDefinition
    {
        internal void SetIconForTests(Sprite icon)
        {
            _icon = icon;
        }

        internal void SetSaveIdForTests(string saveId)
        {
            _saveId = saveId;
        }

        internal void EnsureSaveIdForTests()
        {
            EnsureSaveId();
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
