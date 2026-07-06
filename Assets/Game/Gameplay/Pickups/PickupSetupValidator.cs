using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public interface IPickupSetupValidator
    {
        IReadOnlyList<string> Validate(IReadOnlyList<Pickup> pickups, string pickupLayerName);
    }

    public sealed class PickupSetupValidator : IPickupSetupValidator
    {
        public IReadOnlyList<string> Validate(IReadOnlyList<Pickup> pickups, string pickupLayerName)
        {
            var errors = new List<string>();

            if (pickups == null || pickups.Count <= 0)
                return errors;

            var pickupLayer = ResolveLayer(pickupLayerName, "Pickup Layer", errors);
            var uniquePickups = new HashSet<Pickup>();

            for (var pickupIndex = 0; pickupIndex < pickups.Count; pickupIndex += 1)
            {
                var pickup = pickups[pickupIndex];

                if (pickup == null)
                {
                    errors.Add($"GameplayPickupsSceneCompositionMonoInstaller Level Pickup at index {pickupIndex} is missing.");
                    continue;
                }

                if (!uniquePickups.Add(pickup))
                    errors.Add($"GameplayPickupsSceneCompositionMonoInstaller contains duplicate Level Pickup reference '{pickup.name}'.");

                ValidatePickupDefinition(pickup, errors);
                ValidatePickupTriggerCollider(pickup, pickupLayer, pickupLayerName, errors);
            }

            return errors;
        }

        private int ResolveLayer(string layerName, string label, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                errors.Add($"Pickup collection requires a configured {label} name.");
                return -1;
            }

            var layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
                errors.Add($"Pickup collection requires Unity layer '{layerName}' for {label}.");

            return layer;
        }

        private void ValidatePickupDefinition(Pickup pickup, ICollection<string> errors)
        {
            try
            {
                pickup.Validate();
            }
            catch (Exception exception)
            {
                errors.Add($"Invalid Pickup '{pickup.name}': {exception.Message}");
            }
        }

        private void ValidatePickupTriggerCollider(
            Pickup pickup,
            int pickupLayer,
            string pickupLayerName,
            ICollection<string> errors)
        {
            if (pickupLayer < 0)
                return;

            var pickupLayerColliders = pickup.GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.gameObject.layer == pickupLayer)
                .ToArray();

            if (pickupLayerColliders.Length != 1)
            {
                errors.Add(
                    $"Pickup '{pickup.name}' requires exactly one Collider under its hierarchy on Pickup Layer '{pickupLayerName}'.");
                return;
            }

            var collider = pickupLayerColliders[0];

            if (!collider.enabled)
                errors.Add($"Pickup '{pickup.name}' collider '{collider.name}' must be enabled.");

            if (!collider.isTrigger)
                errors.Add($"Pickup '{pickup.name}' collider '{collider.name}' must be marked as Trigger.");
        }
    }
}
