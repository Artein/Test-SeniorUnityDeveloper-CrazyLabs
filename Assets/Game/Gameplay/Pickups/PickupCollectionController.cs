using System;
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
        private readonly IPickupContactSource _pickupContactSource;
        private readonly ILevelPickupState _levelPickupState;
        private readonly IRunCurrencyAccumulator _runCurrencyAccumulator;
        private readonly RunRewardSourceCatalog _runRewardSourceCatalog;
        private readonly IPickupCurrencyGrantResolver _pickupCurrencyGrantResolver;
        private readonly IGameplayStateService _gameplayStateService;
        private readonly GameplayStateId _runningStateId;
        private readonly GameplayStateId _currencyGrantResolverResetStateId;

        private bool _isInitialized;
        private bool _isDisposed;

        public event Action<PickupCollectedEventArgs> PickupCollected;

        public PickupCollectionController(
            IPickupContactSource pickupContactSource,
            ILevelPickupState levelPickupState,
            IRunCurrencyAccumulator runCurrencyAccumulator,
            RunRewardSourceCatalog runRewardSourceCatalog,
            IPickupCurrencyGrantResolver pickupCurrencyGrantResolver,
            IGameplayStateService gameplayStateService,
            [Key(InjectKey.GameplayStateId.Running)]
            GameplayStateId runningStateId,
            [Key(InjectKey.GameplayStateId.RunPreparation)]
            GameplayStateId currencyGrantResolverResetStateId)
        {
            _pickupContactSource = pickupContactSource ?? throw new ArgumentNullException(nameof(pickupContactSource));
            _levelPickupState = levelPickupState ?? throw new ArgumentNullException(nameof(levelPickupState));
            _runCurrencyAccumulator = runCurrencyAccumulator ?? throw new ArgumentNullException(nameof(runCurrencyAccumulator));
            _runRewardSourceCatalog = runRewardSourceCatalog ?? throw new ArgumentNullException(nameof(runRewardSourceCatalog));

            _pickupCurrencyGrantResolver = pickupCurrencyGrantResolver
                                           ?? throw new ArgumentNullException(nameof(pickupCurrencyGrantResolver));
            _gameplayStateService = gameplayStateService ?? throw new ArgumentNullException(nameof(gameplayStateService));
            _runningStateId = runningStateId != null ? runningStateId : throw new ArgumentNullException(nameof(runningStateId));

            _currencyGrantResolverResetStateId = currencyGrantResolverResetStateId != null
                ? currencyGrantResolverResetStateId
                : throw new ArgumentNullException(nameof(currencyGrantResolverResetStateId));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PickupCollectionController));

            if (_isInitialized)
                return;

            _pickupContactSource.PickupContacted += OnPickupContacted;
            _gameplayStateService.GameplayStateChanged += OnGameplayStateChanged;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _pickupContactSource.PickupContacted -= OnPickupContacted;
            _gameplayStateService.GameplayStateChanged -= OnGameplayStateChanged;
        }

        private void OnPickupContacted(PickupContact contact)
        {
            var pickup = contact.Pickup;

            if (_isDisposed
                || pickup == null
                || !_gameplayStateService.IsCurrent(_runningStateId))
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

            _runCurrencyAccumulator.Grant(
                _runRewardSourceCatalog.PickedUpCoins,
                finalCurrencyGrant.CurrencyDefinition,
                finalCurrencyGrant.Amount);

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
            if (_isDisposed)
                return;

            if (nextStateId == _currencyGrantResolverResetStateId)
                _pickupCurrencyGrantResolver.Reset();
        }
    }
}
