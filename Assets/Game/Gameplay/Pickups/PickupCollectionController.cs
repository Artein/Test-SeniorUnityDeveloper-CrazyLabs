using System;
using System.Collections.Generic;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Utils.Invocation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.Pickups
{
    public interface IPickupCollectionNotifier
    {
        event Action<PickupCollectedEventArgs> PickupCollected;
    }

    public interface IPickupCurrencyGrantResolver
    {
        PickupCurrencyGrantResolution Resolve(CurrencyGrant baseCurrencyGrant);
        void Reset();
    }

    public sealed class PickupCollectionController : IInitializable, IDisposable, IPickupCollectionNotifier
    {
        private readonly ILevelPickupSource _pickupSource;
        private readonly HashSet<Pickup> _subscribedPickups = new();
        private readonly ILevelPickupState _levelPickupState;
        private readonly ICurrencyStorage _currencyStorage;
        private readonly IRunCurrencyAccumulator _runCurrencyAccumulator;
        private readonly IPickupCurrencyGrantResolver _pickupCurrencyGrantResolver;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStateId _currencyGrantResolverResetStateId;
        private readonly string _playerTag;

        private bool _isInitialized;
        private bool _isDisposed;

        public event Action<PickupCollectedEventArgs> PickupCollected;

        public PickupCollectionController(
            ILevelPickupSource pickupSource,
            ILevelPickupState levelPickupState,
            ICurrencyStorage currencyStorage,
            IRunCurrencyAccumulator runCurrencyAccumulator,
            IPickupCurrencyGrantResolver pickupCurrencyGrantResolver,
            IGameplayStateService gameplayStateService,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId currencyGrantResolverResetStateId,
            [Key(InjectKey.Tags.Player)] string playerTag)
        {
            _pickupSource = pickupSource ?? throw new ArgumentNullException(nameof(pickupSource));
            _levelPickupState = levelPickupState ?? throw new ArgumentNullException(nameof(levelPickupState));
            _currencyStorage = currencyStorage ?? throw new ArgumentNullException(nameof(currencyStorage));
            _runCurrencyAccumulator = runCurrencyAccumulator ?? throw new ArgumentNullException(nameof(runCurrencyAccumulator));

            _pickupCurrencyGrantResolver = pickupCurrencyGrantResolver
                                           ?? throw new ArgumentNullException(nameof(pickupCurrencyGrantResolver));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));

            _currencyGrantResolverResetStateId = currencyGrantResolverResetStateId != null
                ? currencyGrantResolverResetStateId
                : throw new ArgumentNullException(nameof(currencyGrantResolverResetStateId));

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

            SynchronizePickupSubscriptions();
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_isInitialized)
                return;

            foreach (var pickup in _subscribedPickups)
            {
                if (pickup == null)
                    continue;

                pickup.TriggerEntered -= OnPickupTriggerEntered;
            }

            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
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
            var baseCurrencyGrant = definition.CurrencyGrant;
            var resolution = _pickupCurrencyGrantResolver.Resolve(baseCurrencyGrant);
            var finalCurrencyGrant = resolution.FinalCurrencyGrant;
            var position = pickup.Position;

            _currencyStorage.Grant(finalCurrencyGrant.CurrencyDefinition, finalCurrencyGrant.Amount);
            _runCurrencyAccumulator.Grant(finalCurrencyGrant.CurrencyDefinition, finalCurrencyGrant.Amount);
            pickup.SetAvailable(false);
            PickupCollected?.InvokeSafely(new PickupCollectedEventArgs(baseCurrencyGrant, finalCurrencyGrant, position));
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed)
                return;

            SynchronizePickupSubscriptions();

            if (ReferenceEquals(nextStateId, _currencyGrantResolverResetStateId))
                _pickupCurrencyGrantResolver.Reset();
        }

        private void SynchronizePickupSubscriptions()
        {
            var pickups = _pickupSource.GetLevelPickups();

            if (pickups == null)
                throw new InvalidOperationException("Pickup Collection Controller requires a non-null Pickup list.");

            var uniquePickups = new HashSet<Pickup>(pickups.Count);

            for (var pickupIndex = 0; pickupIndex < pickups.Count; pickupIndex += 1)
            {
                var pickup = pickups[pickupIndex];

                if (pickup == null)
                    throw new InvalidOperationException("Pickup Collection Controller cannot subscribe to a null Pickup reference.");

                if (!uniquePickups.Add(pickup))
                    throw new InvalidOperationException($"Pickup Collection Controller contains duplicate Pickup reference '{pickup.name}'.");

                if (!_subscribedPickups.Add(pickup))
                    continue;

                pickup.TriggerEntered += OnPickupTriggerEntered;
            }
        }
    }
}
