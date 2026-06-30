using System;
using System.Collections.Generic;

namespace Game.Gameplay.Economy
{
    public readonly struct PlayerCurrencyBalance
    {
        public string CurrencySaveId { get; }
        public int Amount { get; }

        public PlayerCurrencyBalance(string currencySaveId, int amount)
        {
            if (string.IsNullOrWhiteSpace(currencySaveId))
                throw new ArgumentException("Currency balance requires a non-empty save id.", nameof(currencySaveId));

            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency balance must not be negative.");

            CurrencySaveId = currencySaveId;
            Amount = amount;
        }
    }

    public readonly struct PlayerUpgradeLevel
    {
        public string UpgradeStableId { get; }
        public int Level { get; }

        public PlayerUpgradeLevel(string upgradeStableId, int level)
        {
            if (string.IsNullOrWhiteSpace(upgradeStableId))
                throw new ArgumentException("Upgrade level requires a non-empty stable id.", nameof(upgradeStableId));

            if (level < 0)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Upgrade level must not be negative.");

            UpgradeStableId = upgradeStableId;
            Level = level;
        }
    }

    public sealed class PlayerEconomySnapshot
    {
        private readonly Dictionary<string, int> _currencyBalancesById;
        private readonly Dictionary<string, int> _upgradeLevelsById;
        private readonly PlayerCurrencyBalance[] _currencyBalances;
        private readonly PlayerUpgradeLevel[] _upgradeLevels;

        public long Revision { get; }
        public IReadOnlyList<PlayerCurrencyBalance> CurrencyBalances => _currencyBalances;
        public IReadOnlyList<PlayerUpgradeLevel> UpgradeLevels => _upgradeLevels;

        public PlayerEconomySnapshot(
            long revision,
            IEnumerable<PlayerCurrencyBalance> currencyBalances,
            IEnumerable<PlayerUpgradeLevel> upgradeLevels)
        {
            if (currencyBalances is null)
                throw new ArgumentNullException(nameof(currencyBalances));

            if (upgradeLevels is null)
                throw new ArgumentNullException(nameof(upgradeLevels));

            Revision = Math.Max(0, revision);
            _currencyBalancesById = new Dictionary<string, int>(StringComparer.Ordinal);
            _upgradeLevelsById = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var balance in currencyBalances)
            {
                _currencyBalancesById[balance.CurrencySaveId] = balance.Amount;
            }

            foreach (var level in upgradeLevels)
            {
                _upgradeLevelsById[level.UpgradeStableId] = level.Level;
            }

            _currencyBalances = new PlayerCurrencyBalance[_currencyBalancesById.Count];
            var currencyIndex = 0;

            foreach (var pair in _currencyBalancesById)
            {
                _currencyBalances[currencyIndex] = new PlayerCurrencyBalance(pair.Key, pair.Value);
                currencyIndex += 1;
            }

            _upgradeLevels = new PlayerUpgradeLevel[_upgradeLevelsById.Count];
            var upgradeIndex = 0;

            foreach (var pair in _upgradeLevelsById)
            {
                _upgradeLevels[upgradeIndex] = new PlayerUpgradeLevel(pair.Key, pair.Value);
                upgradeIndex += 1;
            }
        }

        public int GetCurrencyBalance(string currencySaveId)
        {
            return string.IsNullOrWhiteSpace(currencySaveId) ? 0 : _currencyBalancesById.GetValueOrDefault(currencySaveId, 0);
        }

        public int GetUpgradeLevel(string upgradeStableId)
        {
            return string.IsNullOrWhiteSpace(upgradeStableId) ? 0 : _upgradeLevelsById.GetValueOrDefault(upgradeStableId, 0);
        }
    }

    public sealed class PlayerEconomyState
    {
        private readonly Dictionary<string, int> _currencyBalancesById = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _upgradeLevelsById = new(StringComparer.Ordinal);

        public long Revision { get; private set; }

        public int GetCurrencyBalance(string currencySaveId)
        {
            return string.IsNullOrWhiteSpace(currencySaveId) ? 0 : _currencyBalancesById.GetValueOrDefault(currencySaveId, 0);
        }

        public void GrantCurrency(string currencySaveId, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency grant amount must be positive.");

            SetCurrencyBalance(currencySaveId, checked(GetCurrencyBalance(currencySaveId) + amount));
        }

        public bool TrySpendCurrency(string currencySaveId, int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency spend amount must be positive.");

            var currentAmount = GetCurrencyBalance(currencySaveId);

            if (currentAmount < amount)
                return false;

            SetCurrencyBalance(currencySaveId, currentAmount - amount);
            return true;
        }

        public void SetCurrencyBalance(string currencySaveId, int amount)
        {
            ThrowIfBlankId(currencySaveId, nameof(currencySaveId));

            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Currency balance must not be negative.");

            if (amount == 0)
                _currencyBalancesById.Remove(currencySaveId);
            else
                _currencyBalancesById[currencySaveId] = amount;

            IncrementRevision();
        }

        public int GetUpgradeLevel(string upgradeStableId)
        {
            return string.IsNullOrWhiteSpace(upgradeStableId) ? 0 : _upgradeLevelsById.GetValueOrDefault(upgradeStableId, 0);
        }

        public void SetUpgradeLevel(string upgradeStableId, int level)
        {
            ThrowIfBlankId(upgradeStableId, nameof(upgradeStableId));

            if (level < 0)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Upgrade level must not be negative.");

            if (level == 0)
                _upgradeLevelsById.Remove(upgradeStableId);
            else
                _upgradeLevelsById[upgradeStableId] = level;

            IncrementRevision();
        }

        public PlayerEconomySnapshot CreateSnapshot()
        {
            var balances = new PlayerCurrencyBalance[_currencyBalancesById.Count];
            var balanceIndex = 0;

            foreach (var pair in _currencyBalancesById)
            {
                balances[balanceIndex] = new PlayerCurrencyBalance(pair.Key, pair.Value);
                balanceIndex += 1;
            }

            var levels = new PlayerUpgradeLevel[_upgradeLevelsById.Count];
            var levelIndex = 0;

            foreach (var pair in _upgradeLevelsById)
            {
                levels[levelIndex] = new PlayerUpgradeLevel(pair.Key, pair.Value);
                levelIndex += 1;
            }

            return new PlayerEconomySnapshot(Revision, balances, levels);
        }

        public void ReplaceWith(PlayerEconomySnapshot snapshot)
        {
            if (snapshot is null)
                throw new ArgumentNullException(nameof(snapshot));

            _currencyBalancesById.Clear();
            _upgradeLevelsById.Clear();

            foreach (var balance in snapshot.CurrencyBalances)
            {
                if (balance.Amount > 0)
                    _currencyBalancesById[balance.CurrencySaveId] = balance.Amount;
            }

            foreach (var level in snapshot.UpgradeLevels)
            {
                if (level.Level > 0)
                    _upgradeLevelsById[level.UpgradeStableId] = level.Level;
            }

            Revision = snapshot.Revision;
        }

        private void IncrementRevision()
        {
            Revision = checked(Revision + 1);
        }

        private void ThrowIfBlankId(string id, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Economy state id must be non-empty.", parameterName);
        }
    }
}
