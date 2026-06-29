using System;
using System.Collections.Generic;

namespace Game.Gameplay.Pickups
{
    public interface IResourceStorage
    {
        void Grant(ResourceDefinition resourceDefinition, int amount);
        int GetAmount(ResourceDefinition resourceDefinition);
    }

    public sealed class ResourceStorage : IResourceStorage
    {
        private readonly Dictionary<ResourceDefinition, int> _amountsByResource = new();

        public void Grant(ResourceDefinition resourceDefinition, int amount)
        {
            if (resourceDefinition == null)
                throw new ArgumentNullException(nameof(resourceDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Resource grant amount must be positive.");

            var currentAmount = GetAmount(resourceDefinition);
            _amountsByResource[resourceDefinition] = checked(currentAmount + amount);
        }

        public int GetAmount(ResourceDefinition resourceDefinition)
        {
            return resourceDefinition == null ? 0 : _amountsByResource.GetValueOrDefault(resourceDefinition, 0);
        }
    }
}
