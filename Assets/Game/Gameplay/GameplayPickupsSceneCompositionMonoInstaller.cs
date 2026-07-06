using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gameplay.Pickups;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    public sealed partial class GameplayPickupsSceneCompositionMonoInstaller : BaseSceneCompositionMonoInstaller
    {
        [SerializeField] private Pickup[] _levelPickups = Array.Empty<Pickup>();
        [SerializeField] private PickupSensorSource _pickupSensorSource;
        [SerializeField] private string _pickupLayerName = "Pickup";
        [SerializeField] private string _playerBodyPartLayerName = "PlayerBodyPart";

        private ILayerCollisionMatrix _layerCollisionMatrix = new UnityLayerCollisionMatrix();

        internal IReadOnlyList<Pickup> LevelPickups => _levelPickups ?? Array.Empty<Pickup>();

        public override void Install([NotNull] IContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            ThrowIfInvalidReferences();

            var pickupContactSource = LevelPickups.Count > 0
                ? (IPickupContactSource)_pickupSensorSource
                : new EmptyPickupContactSource();

            builder.RegisterInstance<IReadOnlyList<Pickup>>(LevelPickups).Keyed(GameplayState.InjectKey.Pickups.LevelPickups);
            builder.RegisterInstance<ILevelPickupSource>(new ExplicitLevelPickupSource(LevelPickups));
            builder.RegisterInstance<IPickupContactSource>(pickupContactSource);
        }

        internal override IEnumerable<string> GetReferenceValidationErrors()
        {
            var levelPickups = LevelPickups;
            var validator = new PickupSetupValidator();

            foreach (var error in validator.Validate(levelPickups, _pickupLayerName))
            {
                yield return error;
            }

            if (levelPickups.Count <= 0)
                yield break;

            if (_pickupSensorSource == null)
            {
                yield return "GameplayPickupsSceneCompositionMonoInstaller requires a Pickup Sensor Source reference for pickup-bearing levels.";
            }
            else
            {
                foreach (var error in _pickupSensorSource.GetReferenceValidationErrors(_playerBodyPartLayerName, _pickupLayerName))
                {
                    yield return error;
                }
            }

            foreach (var error in GetLayerCollisionMatrixErrors())
            {
                yield return error;
            }
        }

        private IEnumerable<string> GetLayerCollisionMatrixErrors()
        {
            var errors = new List<string>();
            var playerBodyPartLayer = ResolveLayer(_playerBodyPartLayerName, "Player Body Part Layer", errors);
            var pickupLayer = ResolveLayer(_pickupLayerName, "Pickup Layer", errors);

            foreach (var error in errors)
            {
                yield return error;
            }

            if (playerBodyPartLayer < 0 || pickupLayer < 0)
                yield break;

            if (_layerCollisionMatrix.GetIgnoreLayerCollision(playerBodyPartLayer, pickupLayer))
            {
                yield return
                    $"3D Layer Collision Matrix must allow PlayerBodyPart Layer '{_playerBodyPartLayerName}' to overlap Pickup Layer '{_pickupLayerName}'.";
            }
        }

        private int ResolveLayer(string layerName, string label, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                errors.Add($"GameplayPickupsSceneCompositionMonoInstaller requires a configured {label} name.");
                return -1;
            }

            var layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
                errors.Add($"GameplayPickupsSceneCompositionMonoInstaller requires Unity layer '{layerName}' for {label}.");

            return layer;
        }

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToArray();

            if (errors.Length > 0)
                throw new InvalidOperationException(string.Join("\n", errors));
        }

        [UsedImplicitly]
        private sealed class ExplicitLevelPickupSource : ILevelPickupSource
        {
            private readonly IReadOnlyList<Pickup> _pickups;

            public ExplicitLevelPickupSource(IReadOnlyList<Pickup> pickups)
            {
                _pickups = pickups ?? throw new ArgumentNullException(nameof(pickups));
            }

            public IReadOnlyList<Pickup> GetLevelPickups()
            {
                return _pickups;
            }
        }

        private interface ILayerCollisionMatrix
        {
            bool GetIgnoreLayerCollision(int firstLayer, int secondLayer);
        }

        private sealed class UnityLayerCollisionMatrix : ILayerCollisionMatrix
        {
            public bool GetIgnoreLayerCollision(int firstLayer, int secondLayer)
            {
                return Physics.GetIgnoreLayerCollision(firstLayer, secondLayer);
            }
        }

        public static class Serialization
        {
            public const string LevelPickups = nameof(_levelPickups);
        }
    }
}
