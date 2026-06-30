using System;
using Game.Gameplay.Economy;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class RunRewardCommitter : IInitializable, IDisposable
    {
        private readonly IRunResultNotifier _runResultNotifier;
        private readonly ICurrencyStorage _currencyStorage;
        private readonly IEconomyCommitter _economyCommitter;

        private bool _isInitialized;
        private bool _isDisposed;

        public RunRewardCommitter(
            IRunResultNotifier runResultNotifier,
            ICurrencyStorage currencyStorage,
            IEconomyCommitter economyCommitter)
        {
            _runResultNotifier = runResultNotifier ?? throw new ArgumentNullException(nameof(runResultNotifier));
            _currencyStorage = currencyStorage ?? throw new ArgumentNullException(nameof(currencyStorage));
            _economyCommitter = economyCommitter ?? throw new ArgumentNullException(nameof(economyCommitter));
        }

        void IInitializable.Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(RunRewardCommitter));

            if (_isInitialized)
                return;

            _runResultNotifier.RunResultAccepted += OnRunResultAccepted;
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (!_isInitialized)
                return;

            _runResultNotifier.RunResultAccepted -= OnRunResultAccepted;
        }

        private void OnRunResultAccepted(RunResult runResult)
        {
            if (_isDisposed)
                return;

            try
            {
                foreach (var amount in runResult.CurrencySnapshot.Amounts)
                {
                    _currencyStorage.Grant(amount.CurrencyDefinition, amount.Amount);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Run reward grant failed. " + exception.Message);
                return;
            }

            var result = _economyCommitter.CommitImportant("run-reward");

            if (!result.IsSuccess)
                Debug.LogError("Run reward save commit failed. " + result.Message);
        }
    }
}
