using System;
using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public interface IUpgradeProgressStorage
    {
        int GetLevel(UpgradeDefinition definition);
        void SetLevel(UpgradeDefinition definition, int level);
    }

    public sealed class UpgradeProgressStorage : IUpgradeProgressStorage
    {
        private readonly Dictionary<string, int> _levelsByUpgradeId = new(StringComparer.Ordinal);

        public int GetLevel(UpgradeDefinition definition)
        {
            return TryGetStableId(definition, out var stableId)
                ? _levelsByUpgradeId.GetValueOrDefault(stableId, 0)
                : 0;
        }

        public void SetLevel(UpgradeDefinition definition, int level)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (!TryGetStableId(definition, out var stableId))
                throw new ArgumentException("Upgrade definition requires a stable id.", nameof(definition));

            if (level < 0 || level > definition.MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Upgrade level must be within definition bounds.");

            if (level == 0)
            {
                _levelsByUpgradeId.Remove(stableId);
                return;
            }

            _levelsByUpgradeId[stableId] = level;
        }

        private bool TryGetStableId(UpgradeDefinition definition, out string stableId)
        {
            stableId = definition == null ? null : definition.StableId;
            return !string.IsNullOrWhiteSpace(stableId);
        }
    }
}
