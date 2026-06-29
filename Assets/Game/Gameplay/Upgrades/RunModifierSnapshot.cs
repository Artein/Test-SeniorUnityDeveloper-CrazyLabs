using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Game.Gameplay.Upgrades
{
    public sealed class RunModifierSnapshot : IGameplayStatModifierSource
    {
        private readonly ReadOnlyCollection<GameplayStatModifier> _modifiers;

        public RunModifierSnapshot(IReadOnlyList<GameplayStatModifier> modifiers)
        {
            if (modifiers is null)
                throw new ArgumentNullException(nameof(modifiers));

            var modifiersCopy = new GameplayStatModifier[modifiers.Count];

            for (var index = 0; index < modifiers.Count; index++)
            {
                modifiersCopy[index] = modifiers[index];
            }

            _modifiers = new ReadOnlyCollection<GameplayStatModifier>(modifiersCopy);
        }

        public IReadOnlyList<GameplayStatModifier> Modifiers => _modifiers;
    }
}
