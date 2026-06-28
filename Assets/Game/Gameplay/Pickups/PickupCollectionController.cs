using System;
using System.Collections.Generic;
using Game.Gameplay.GameplayState;
using Game.Utils.Invocation;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Pickups
{
    public interface IPickupCollectionNotifier
    {
        event Action<PickupCollectedEventArgs> PickupCollected;
    }

    public sealed class PickupCollectionController : IInitializable, IDisposable, IPickupCollectionNotifier
    {
        private readonly IReadOnlyList<Pickup> _pickups;
        private readonly ILevelPickupState _levelPickupState;
        private readonly IResourceStorage _resourceStorage;
        private readonly IRunResourceAccumulator _runResourceAccumulator;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly GameplayStateId _runningStateId;
        private readonly string _playerTag;

        private bool _isInitialized;
        private bool _isDisposed;

        public event Action<PickupCollectedEventArgs> PickupCollected;

        public PickupCollectionController(
            IReadOnlyList<Pickup> pickups,
            ILevelPickupState levelPickupState,
            IResourceStorage resourceStorage,
            IRunResourceAccumulator runResourceAccumulator,
            IGameplayStateService gameplayStateService,
            GameplayStateId runningStateId,
            string playerTag)
        {
            _pickups = pickups ?? throw new ArgumentNullException(nameof(pickups));
            _levelPickupState = levelPickupState ?? throw new ArgumentNullException(nameof(levelPickupState));
            _resourceStorage = resourceStorage ?? throw new ArgumentNullException(nameof(resourceStorage));
            _runResourceAccumulator = runResourceAccumulator ?? throw new ArgumentNullException(nameof(runResourceAccumulator));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));

            if (string.IsNullOrWhiteSpace(playerTag))
                throw new ArgumentException("Pickup collection requires a non-empty Player Tag.", nameof(playerTag));

            _playerTag = playerTag;
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PickupCollectionController));

            if (_isInitialized)
                return;

            foreach (var pickup in _pickups)
            {
                if (pickup == null)
                    throw new InvalidOperationException("Pickup Collection Controller cannot subscribe to a null Pickup reference.");

                pickup.TriggerEntered += OnPickupTriggerEntered;
            }

            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_isInitialized)
                return;

            foreach (var pickup in _pickups)
            {
                if (pickup == null)
                    continue;

                pickup.TriggerEntered -= OnPickupTriggerEntered;
            }
        }

        private void OnPickupTriggerEntered(Pickup pickup, Collider other)
        {
            if (_isDisposed
                || pickup == null
                || other == null
                || !_gameplayStateService.IsCurrent(_runningStateId)
                || !other.gameObject.CompareTag(_playerTag))
            {
                return;
            }

            if (!_levelPickupState.TryConsume(pickup))
                return;

            var definition = pickup.Definition;
            var resourceDefinition = definition.ResourceDefinition;
            var amount = definition.Amount;
            var position = pickup.Position;

            _resourceStorage.Grant(resourceDefinition, amount);
            _runResourceAccumulator.Grant(resourceDefinition, amount);
            pickup.SetAvailable(false);
            PickupCollected?.InvokeSafely(new PickupCollectedEventArgs(resourceDefinition, amount, position));
        }
    }
}
