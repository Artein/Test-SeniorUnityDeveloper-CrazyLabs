using System;
using System.Collections.Generic;

namespace Game.Gameplay.Pickups
{
    public interface IRunResourceAccumulator
    {
        void Grant(ResourceDefinition resourceDefinition, int amount);
        void Reset();
        RunResourceSnapshot CreateSnapshot();
    }

    public sealed class RunResourceAccumulator : IRunResourceAccumulator
    {
        private readonly Dictionary<ResourceDefinition, int> _amountsByResource = new();

        void IRunResourceAccumulator.Grant(ResourceDefinition resourceDefinition, int amount)
        {
            if (resourceDefinition == null)
                throw new ArgumentNullException(nameof(resourceDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Run resource grant amount must be positive.");

            var currentAmount = _amountsByResource.GetValueOrDefault(resourceDefinition, 0);
            _amountsByResource[resourceDefinition] = checked(currentAmount + amount);
        }

        void IRunResourceAccumulator.Reset()
        {
            _amountsByResource.Clear();
        }

        RunResourceSnapshot IRunResourceAccumulator.CreateSnapshot()
        {
            var amounts = new RunResourceAmount[_amountsByResource.Count];
            var index = 0;

            foreach (var pair in _amountsByResource)
            {
                amounts[index] = new RunResourceAmount(pair.Key, pair.Value);
                index += 1;
            }

            return new RunResourceSnapshot(amounts);
        }
    }
}
