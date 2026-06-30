using System;

namespace Game.Gameplay.Upgrades
{
    public interface IRunModifierSnapshotProvider
    {
        RunModifierSnapshot CurrentSnapshot { get; }
    }

    public interface IRunModifierSnapshotStore : IRunModifierSnapshotProvider
    {
        void SetSnapshot(RunModifierSnapshot snapshot);
    }

    public sealed class RunModifierSnapshotHolder : IRunModifierSnapshotStore
    {
        public RunModifierSnapshot CurrentSnapshot { get; private set; } = new(Array.Empty<GameplayStatModifier>());

        public void SetSnapshot(RunModifierSnapshot snapshot)
        {
            CurrentSnapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        }
    }
}
