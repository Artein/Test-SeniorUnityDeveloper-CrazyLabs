using System;

namespace Game.Gameplay.Economy
{
    public interface ICurrencyStorage
    {
        void Grant(CurrencyDefinition currencyDefinition, int amount);
        bool TrySpend(CurrencyDefinition currencyDefinition, int amount);
        int GetAmount(CurrencyDefinition currencyDefinition);
    }

    public sealed class CurrencyStorage : ICurrencyStorage
    {
        private readonly PlayerEconomyState _state;

        public CurrencyStorage(PlayerEconomyState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        void ICurrencyStorage.Grant(CurrencyDefinition currencyDefinition, int amount)
        {
            if (currencyDefinition == null)
                throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency grant amount must be positive.");

            _state.GrantCurrency(GetRequiredSaveId(currencyDefinition), amount);
        }

        bool ICurrencyStorage.TrySpend(CurrencyDefinition currencyDefinition, int amount)
        {
            if (currencyDefinition == null)
                throw new ArgumentNullException(nameof(currencyDefinition));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency spend amount must be positive.");

            return _state.TrySpendCurrency(GetRequiredSaveId(currencyDefinition), amount);
        }

        int ICurrencyStorage.GetAmount(CurrencyDefinition currencyDefinition)
        {
            return currencyDefinition == null || string.IsNullOrWhiteSpace(currencyDefinition.SaveId)
                ? 0
                : _state.GetCurrencyBalance(currencyDefinition.SaveId);
        }

        private string GetRequiredSaveId(CurrencyDefinition currencyDefinition)
        {
            if (!string.IsNullOrWhiteSpace(currencyDefinition.SaveId))
                return currencyDefinition.SaveId;

            throw new ArgumentException("Currency definition requires a stable save id.", nameof(currencyDefinition));
        }
    }
}
