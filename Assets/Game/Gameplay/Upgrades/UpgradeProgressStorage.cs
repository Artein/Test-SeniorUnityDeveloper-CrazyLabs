using System;
using Game.Gameplay.Economy;

namespace Game.Gameplay.Upgrades
{
    public interface IUpgradeProgressStorage
    {
        int GetLevel(UpgradeDefinition definition);
        void SetLevel(UpgradeDefinition definition, int level);
    }

    public sealed class UpgradeProgressStorage : IUpgradeProgressStorage
    {
        private readonly PlayerEconomyState _state;

        public UpgradeProgressStorage(PlayerEconomyState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public int GetLevel(UpgradeDefinition definition)
        {
            return TryGetStableId(definition, out var stableId) ? _state.GetUpgradeLevel(stableId) : 0;
        }

        public void SetLevel(UpgradeDefinition definition, int level)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (!TryGetStableId(definition, out var stableId))
                throw new ArgumentException("Upgrade definition requires a stable id.", nameof(definition));

            if (level < 0 || level > definition.MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Upgrade level must be within definition bounds.");

            _state.SetUpgradeLevel(stableId, level);
        }

        private bool TryGetStableId(UpgradeDefinition definition, out string stableId)
        {
            stableId = definition == null ? null : definition.StableId;
            return !string.IsNullOrWhiteSpace(stableId);
        }
    }
}
