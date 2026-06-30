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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[EconomyDiagnostics] Run reward accepted: "
                      + $"Reason={runResult.Reason}, "
                      + $"CurrencySnapshot={runResult.CurrencySnapshot}");
#endif

            try
            {
                foreach (var amount in runResult.CurrencySnapshot.Amounts)
                {
                    var balanceBefore = _currencyStorage.GetAmount(amount.CurrencyDefinition);
                    _currencyStorage.Grant(amount.CurrencyDefinition, amount.Amount);
                    var balanceAfter = _currencyStorage.GetAmount(amount.CurrencyDefinition);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log("[EconomyDiagnostics] Wallet grant applied: "
                              + $"Currency='{amount.CurrencyDefinition.name}', "
                              + $"SaveId='{amount.CurrencyDefinition.SaveId}', "
                              + $"Amount={amount.Amount}, "
                              + $"BalanceBefore={balanceBefore}, "
                              + $"BalanceAfter={balanceAfter}");
#endif
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Run reward grant failed. " + exception.Message);
                return;
            }

            var result = _economyCommitter.CommitImportant("run-reward");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[EconomyDiagnostics] Run reward save commit completed: "
                      + $"Status={result.Status}, "
                      + $"Success={result.IsSuccess}, "
                      + $"Operation='{result.Operation}', "
                      + $"Message='{result.Message}'");
#endif

            if (!result.IsSuccess)
                Debug.LogError("Run reward save commit failed. " + result.Message);
        }
    }
}
