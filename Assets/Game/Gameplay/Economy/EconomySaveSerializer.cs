using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Economy
{
    public interface IPlayerEconomyContentIndex
    {
        bool IsKnownCurrencyId(string currencySaveId);
        bool TryGetUpgradeMaxLevel(string upgradeStableId, out int maxLevel);
    }

    public sealed class EconomySaveSerializer
    {
        private readonly EconomySaveSettings _settings;
        private readonly IPlayerEconomyContentIndex _contentIndex;

        public EconomySaveSerializer(EconomySaveSettings settings, IPlayerEconomyContentIndex contentIndex)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _contentIndex = contentIndex ?? throw new ArgumentNullException(nameof(contentIndex));
        }

        public string Serialize(PlayerEconomySnapshot snapshot)
        {
            if (snapshot is null)
                throw new ArgumentNullException(nameof(snapshot));

            var dto = new EconomySaveDto
            {
                schemaVersion = _settings.CurrentSchemaVersion,
                revision = snapshot.Revision,
                currencyBalances = new List<EconomyCurrencyBalanceDto>(),
                upgradeLevels = new List<EconomyUpgradeLevelDto>()
            };

            foreach (var balance in snapshot.CurrencyBalances)
            {
                dto.currencyBalances.Add(new EconomyCurrencyBalanceDto
                {
                    id = balance.CurrencySaveId,
                    amount = balance.Amount
                });
            }

            foreach (var level in snapshot.UpgradeLevels)
            {
                dto.upgradeLevels.Add(new EconomyUpgradeLevelDto
                {
                    id = level.UpgradeStableId,
                    level = level.Level
                });
            }

            return JsonUtility.ToJson(dto);
        }

        public PlayerEconomySnapshot Deserialize(string json, string sourceName)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Economy save JSON cannot be empty.", nameof(json));

            EnsureJsonObject(json, sourceName);
            var envelope = JsonUtility.FromJson<EconomySaveEnvelopeDto>(json);
            var schemaVersion = envelope?.schemaVersion ?? 0;

            if (schemaVersion > _settings.CurrentSchemaVersion)
            {
                throw new InvalidOperationException(
                    $"Economy save from {sourceName} has future schema version {schemaVersion}.");
            }

            var dto = JsonUtility.FromJson<EconomySaveDto>(json);

            if (dto is null)
                throw new InvalidOperationException($"Economy save from {sourceName} could not be parsed.");

            return CreateSnapshot(dto, sourceName);
        }

        private void EnsureJsonObject(string json, string sourceName)
        {
            var trimmedJson = json.Trim();

            if (trimmedJson.StartsWith("{", StringComparison.Ordinal)
                && trimmedJson.EndsWith("}", StringComparison.Ordinal))
            {
                return;
            }

            throw new InvalidOperationException($"Economy save from {sourceName} is not a complete JSON object.");
        }

        private PlayerEconomySnapshot CreateSnapshot(EconomySaveDto dto, string sourceName)
        {
            var balances = new List<PlayerCurrencyBalance>();
            var levels = new List<PlayerUpgradeLevel>();

            if (dto.currencyBalances != null)
            {
                foreach (var balance in dto.currencyBalances)
                {
                    if (string.IsNullOrWhiteSpace(balance.id))
                        continue;

                    var amount = balance.amount;

                    if (amount < 0)
                    {
                        Debug.LogWarning(
                            $"Economy save {sourceName} has negative currency balance for '{balance.id}'. Clamping to zero.");
                        amount = 0;
                    }

                    if (amount > 0 || _contentIndex.IsKnownCurrencyId(balance.id))
                        balances.Add(new PlayerCurrencyBalance(balance.id, Math.Max(0, amount)));
                }
            }

            if (dto.upgradeLevels != null)
            {
                foreach (var level in dto.upgradeLevels)
                {
                    if (string.IsNullOrWhiteSpace(level.id))
                        continue;

                    var repairedLevel = Math.Max(0, level.level);

                    if (level.level < 0)
                    {
                        Debug.LogWarning(
                            $"Economy save {sourceName} has negative upgrade level for '{level.id}'. Clamping to zero.");
                    }

                    if (_contentIndex.TryGetUpgradeMaxLevel(level.id, out var maxLevel) && repairedLevel > maxLevel)
                    {
                        Debug.LogWarning(
                            $"Economy save {sourceName} upgrade level for '{level.id}' was clamped from {repairedLevel} to {maxLevel}.");
                        repairedLevel = maxLevel;
                    }

                    if (repairedLevel > 0 || _contentIndex.TryGetUpgradeMaxLevel(level.id, out _))
                        levels.Add(new PlayerUpgradeLevel(level.id, repairedLevel));
                }
            }

            return new PlayerEconomySnapshot(dto.revision, balances, levels);
        }

        [Serializable]
        private sealed class EconomySaveEnvelopeDto
        {
            public int schemaVersion;
        }

        [Serializable]
        private sealed class EconomySaveDto
        {
            public int schemaVersion;
            public long revision;
            public List<EconomyCurrencyBalanceDto> currencyBalances;
            public List<EconomyUpgradeLevelDto> upgradeLevels;
        }

        [Serializable]
        private sealed class EconomyCurrencyBalanceDto
        {
            public string id;
            public int amount;
        }

        [Serializable]
        private sealed class EconomyUpgradeLevelDto
        {
            public string id;
            public int level;
        }
    }
}
