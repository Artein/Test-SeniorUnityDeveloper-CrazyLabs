#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay.Upgrades
{
    public sealed partial class UpgradeDefinition
    {
        internal void SetValuesForTests(
            string stableId,
            string displayName,
            string description,
            Sprite icon,
            GameplayStatId targetStatId,
            int maxLevel,
            UpgradeProgression costProgression,
            UpgradeProgression effectProgression,
            UpgradeOperationType operationType,
            UpgradeValueFormat valueFormat,
            int displayDecimalPlaces,
            string shortDisplayName = null)
        {
            _stableId = stableId;
            _displayName = displayName;
            _shortDisplayName = shortDisplayName;
            _description = description;
            _icon = icon;
            _targetStatId = targetStatId;
            _maxLevel = maxLevel;
            _costProgression = costProgression;
            _effectProgression = effectProgression;
            _operationType = operationType;
            _valueFormat = valueFormat;
            _displayDecimalPlaces = displayDecimalPlaces;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
