using System;
using System.Collections.Generic;

namespace Game.Gameplay.Pickups
{
    public readonly struct RunResourceAmount
    {
        public ResourceDefinition ResourceDefinition { get; }
        public int Amount { get; }

        public RunResourceAmount(ResourceDefinition resourceDefinition, int amount)
        {
            ResourceDefinition = resourceDefinition != null ? resourceDefinition : throw new ArgumentNullException(nameof(resourceDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Run resource snapshot amount must be positive.");

            Amount = amount;
        }
    }

    public sealed class RunResourceSnapshot
    {
        private readonly Dictionary<ResourceDefinition, int> _amountsByResource;
        private readonly RunResourceAmount[] _amounts;

        public IReadOnlyList<RunResourceAmount> Amounts => _amounts;

        public RunResourceSnapshot(IEnumerable<RunResourceAmount> amounts)
        {
            if (amounts is null)
                throw new ArgumentNullException(nameof(amounts));

            _amountsByResource = new Dictionary<ResourceDefinition, int>();

            foreach (var amount in amounts)
            {
                if (_amountsByResource.ContainsKey(amount.ResourceDefinition))
                    throw new ArgumentException("Run resource snapshot cannot contain duplicate Resource Definition entries.", nameof(amounts));

                _amountsByResource.Add(amount.ResourceDefinition, amount.Amount);
            }

            _amounts = new RunResourceAmount[_amountsByResource.Count];
            var index = 0;

            foreach (var pair in _amountsByResource)
            {
                _amounts[index] = new RunResourceAmount(pair.Key, pair.Value);
                index += 1;
            }
        }

        public int GetAmount(ResourceDefinition resourceDefinition)
        {
            return resourceDefinition == null ? 0 : _amountsByResource.GetValueOrDefault(resourceDefinition, 0);
        }
    }
}
