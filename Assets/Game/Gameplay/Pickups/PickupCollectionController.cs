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
        private readonly IReadOnlyList<Pickup> _pickups;
        private readonly ILevelPickupState _levelPickupState;
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
            [Key(InjectKey.Pickups.LevelPickups)] IReadOnlyList<Pickup> pickups,
            ILevelPickupState levelPickupState,
            IRunCurrencyAccumulator runCurrencyAccumulator,
            IPickupCurrencyGrantResolver pickupCurrencyGrantResolver,
            IGameplayStateService gameplayStateService,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId currencyGrantResolverResetStateId,
            [Key(InjectKey.Tags.Player)] string playerTag)
        {
            _pickups = pickups ?? throw new ArgumentNullException(nameof(pickups));
            _levelPickupState = levelPickupState ?? throw new ArgumentNullException(nameof(levelPickupState));
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

            foreach (var pickup in _pickups)
            {
                if (pickup == null)
                    throw new InvalidOperationException("Pickup Collection Controller cannot subscribe to a null Pickup reference.");

                pickup.TriggerEntered += OnPickupTriggerEntered;
            }

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

            foreach (var pickup in _pickups)
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

            _runCurrencyAccumulator.Grant(finalCurrencyGrant.CurrencyDefinition, finalCurrencyGrant.Amount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[EconomyDiagnostics] Pickup collected into run accumulator: "
                      + $"Pickup='{pickup.name}', "
                      + $"Currency='{finalCurrencyGrant.CurrencyDefinition.name}', "
                      + $"SaveId='{finalCurrencyGrant.CurrencyDefinition.SaveId}', "
                      + $"BaseAmount={baseCurrencyGrant.Amount}, "
                      + $"FinalAmount={finalCurrencyGrant.Amount}, "
                      + $"Position={position}");
#endif
            pickup.SetAvailable(false);
            PickupCollected?.InvokeSafely(new PickupCollectedEventArgs(baseCurrencyGrant, finalCurrencyGrant, position));
        }

        private void OnGameplayStateChanged(GameplayStateId nextStateId, GameplayStateId previousStateId)
        {
            if (_isDisposed || !ReferenceEquals(nextStateId, _currencyGrantResolverResetStateId))
                return;

            _pickupCurrencyGrantResolver.Reset();
        }
    }
}
