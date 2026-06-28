using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Pickups
{
    public interface IPickupSetupValidator
    {
        IReadOnlyList<string> Validate(
            IReadOnlyList<Pickup> pickups,
            IReadOnlyList<Collider> playerPickupContactColliders,
            string playerTag,
            string playerLayerName,
            string pickupLayerName);
    }

    public sealed class PickupSetupValidator : IPickupSetupValidator
    {
        public IReadOnlyList<string> Validate(
            IReadOnlyList<Pickup> pickups,
            IReadOnlyList<Collider> playerPickupContactColliders,
            string playerTag,
            string playerLayerName,
            string pickupLayerName)
        {
            var errors = new List<string>();
            var playerLayer = ResolveLayer(playerLayerName, "Player Layer", errors);
            var pickupLayer = ResolveLayer(pickupLayerName, "Pickup Layer", errors);

            if (string.IsNullOrWhiteSpace(playerTag))
                errors.Add("Pickup collection requires a configured Player Tag.");

            ValidatePickupReferences(pickups, pickupLayer, pickupLayerName, errors);
            ValidatePlayerContactColliders(playerPickupContactColliders, playerLayer, playerLayerName, playerTag, errors);
            ValidateLayerCollisionMatrix(playerLayer, pickupLayer, playerLayerName, pickupLayerName, errors);

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

        private void ValidatePickupReferences(
            IReadOnlyList<Pickup> pickups,
            int pickupLayer,
            string pickupLayerName,
            ICollection<string> errors)
        {
            if (pickups == null || pickups.Count <= 0)
            {
                errors.Add("GameplayLifetimeScope requires at least one Level Pickup reference.");
                return;
            }

            var uniquePickups = new HashSet<Pickup>();

            for (var pickupIndex = 0; pickupIndex < pickups.Count; pickupIndex += 1)
            {
                var pickup = pickups[pickupIndex];

                if (pickup == null)
                {
                    errors.Add($"GameplayLifetimeScope Level Pickup at index {pickupIndex} is missing.");
                    continue;
                }

                if (!uniquePickups.Add(pickup))
                    errors.Add($"GameplayLifetimeScope contains duplicate Level Pickup reference '{pickup.name}'.");

                ValidatePickupDefinition(pickup, errors);
                ValidatePickupColliders(pickup, pickupLayer, pickupLayerName, errors);
            }
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

        private void ValidatePickupColliders(
            Pickup pickup,
            int pickupLayer,
            string pickupLayerName,
            ICollection<string> errors)
        {
            var colliders = pickup.GetComponentsInChildren<Collider>(true);

            if (colliders.Length <= 0)
            {
                errors.Add($"Pickup '{pickup.name}' requires at least one trigger Collider.");
                return;
            }

            foreach (var collider in colliders)
            {
                if (!collider.isTrigger)
                    errors.Add($"Pickup '{pickup.name}' collider '{collider.name}' must be marked as Trigger.");

                if (pickupLayer >= 0 && collider.gameObject.layer != pickupLayer)
                {
                    errors.Add(
                        $"Pickup '{pickup.name}' collider '{collider.name}' must be on Pickup Layer '{pickupLayerName}'.");
                }
            }
        }

        private void ValidatePlayerContactColliders(
            IReadOnlyList<Collider> playerPickupContactColliders,
            int playerLayer,
            string playerLayerName,
            string playerTag,
            ICollection<string> errors)
        {
            if (playerPickupContactColliders == null || playerPickupContactColliders.Count <= 0)
            {
                errors.Add("GameplayLifetimeScope requires at least one Player Pickup Contact Collider reference.");
                return;
            }

            var uniqueColliders = new HashSet<Collider>();

            for (var colliderIndex = 0; colliderIndex < playerPickupContactColliders.Count; colliderIndex += 1)
            {
                var collider = playerPickupContactColliders[colliderIndex];

                if (collider == null)
                {
                    errors.Add($"Player Pickup Contact Collider at index {colliderIndex} is missing.");
                    continue;
                }

                if (!uniqueColliders.Add(collider))
                {
                    errors.Add(
                        $"GameplayLifetimeScope contains duplicate Player Pickup Contact Collider reference '{collider.name}'.");
                }

                ValidatePlayerContactCollider(collider, playerLayer, playerLayerName, playerTag, errors);
            }
        }

        private void ValidatePlayerContactCollider(
            Collider collider,
            int playerLayer,
            string playerLayerName,
            string playerTag,
            ICollection<string> errors)
        {
            if (playerLayer >= 0 && collider.gameObject.layer != playerLayer)
            {
                errors.Add(
                    $"Player Pickup Contact Collider '{collider.name}' must be on Player Layer '{playerLayerName}'.");
            }

            if (string.IsNullOrWhiteSpace(playerTag))
                return;

            try
            {
                if (!collider.gameObject.CompareTag(playerTag))
                {
                    errors.Add(
                        $"Player Pickup Contact Collider '{collider.name}' GameObject must have Player Tag '{playerTag}'.");
                }
            }
            catch (UnityException exception)
            {
                errors.Add($"Invalid Player Tag '{playerTag}' for pickup collection: {exception.Message}");
            }
        }

        private void ValidateLayerCollisionMatrix(
            int playerLayer,
            int pickupLayer,
            string playerLayerName,
            string pickupLayerName,
            ICollection<string> errors)
        {
            if (playerLayer < 0 || pickupLayer < 0)
                return;

            if (Physics.GetIgnoreLayerCollision(playerLayer, pickupLayer))
            {
                errors.Add(
                    $"3D Layer Collision Matrix must allow Player Layer '{playerLayerName}' to overlap Pickup Layer '{pickupLayerName}'.");
            }
        }
    }
}
