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

        public RunGameplayStatResolver(IRunModifierSnapshotProvider snapshotProvider)
        {
            _snapshotProvider = snapshotProvider ?? throw new ArgumentNullException(nameof(snapshotProvider));
        }

        public float Resolve(GameplayStatId statId, float baseValue)
        {
            var resolver = new GameplayStatResolver(new IGameplayStatModifierSource[]
            {
                _snapshotProvider.CurrentSnapshot
            });

            return resolver.Resolve(statId, baseValue);
        }
    }
}
