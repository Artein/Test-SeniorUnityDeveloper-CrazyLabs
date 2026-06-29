using System;
using Game.Gameplay.Upgrades;

namespace Game.Gameplay
{
    public interface IRunGameplayStatResolver
    {
        float Resolve(GameplayStatId statId, float baseValue);
    }

    public sealed class RunGameplayStatResolver : IRunGameplayStatResolver
    {
        private readonly IRunModifierSnapshotProvider _snapshotProvider;
        private readonly IGameplayStatModifierSource[] _modifierSourceBuffer = new IGameplayStatModifierSource[1];

        private RunModifierSnapshot _cachedSnapshot;
        private GameplayStatResolver _cachedResolver;

        public RunGameplayStatResolver(IRunModifierSnapshotProvider snapshotProvider)
        {
            _snapshotProvider = snapshotProvider ?? throw new ArgumentNullException(nameof(snapshotProvider));
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            var snapshot = _snapshotProvider.CurrentSnapshot;
            var resolver = GetResolver(snapshot);

            return resolver.Resolve(statId, baseValue);
        }

        private GameplayStatResolver GetResolver(RunModifierSnapshot snapshot)
        {
            if (ReferenceEquals(_cachedSnapshot, snapshot) && _cachedResolver != null)
                return _cachedResolver;

            _modifierSourceBuffer[0] = snapshot;
            _cachedResolver = new GameplayStatResolver(_modifierSourceBuffer);
            _cachedSnapshot = snapshot;

            return _cachedResolver;
        }
    }
}
