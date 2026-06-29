using System;

namespace Game.Gameplay.Upgrades
{
    public readonly struct GameplayStatModifier
    {
        public GameplayStatId StatId { get; }
        public GameplayStatModifierOperation Operation { get; }
        public float Value { get; }
        
        public GameplayStatModifier(
            GameplayStatId statId,
            GameplayStatModifierOperation operation,
            float value)
        {
            if (statId == null)
                throw new ArgumentNullException(nameof(statId));

            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentException("Gameplay stat modifier value must be finite.", nameof(value));

            StatId = statId;
            Operation = operation;
            Value = value;
        }
    }
}
