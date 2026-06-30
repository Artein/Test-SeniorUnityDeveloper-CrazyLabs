using System.Collections.Generic;

namespace Game.Gameplay.Upgrades
{
    public interface IGameplayStatModifierSource
    {
        IReadOnlyList<GameplayStatModifier> Modifiers { get; }
    }
}
