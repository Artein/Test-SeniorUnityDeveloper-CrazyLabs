using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Game.Foundation.Persistence;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class EconomySaveRepositoryTests
{
    private readonly List<string> _temporaryDirectories = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var temporaryDirectory in _temporaryDirectories)
        {
            if (Directory.Exists(temporaryDirectory))
                Directory.Delete(temporaryDirectory, recursive: true);
        }

        _temporaryDirectories.Clear();
    }

    [Test]
    public void Load_MissingSave_ReturnsDefaultSnapshot()
    {
        var repository = CreateRepository(out _);

        var loadResult = repository.Load();

        Assert.That(loadResult.Result.Status, Is.EqualTo(EconomyPersistenceStatus.LoadedDefaults));
        Assert.That(loadResult.Snapshot.CurrencyBalances, Is.Empty);
        Assert.That(loadResult.Snapshot.UpgradeLevels, Is.Empty);
    }

    [Test]
    public void SaveThenLoad_RestoresCurrencyBalancesAndUpgradeLevels()
    {
        var repository = CreateRepository(out _);

        var snapshot = new PlayerEconomySnapshot(
            revision: 3,
            new[] { new PlayerCurrencyBalance("currency-coins", 125) },
            new[] { new PlayerUpgradeLevel("launch-power", 2) });

        var saveResult = repository.Save(snapshot, "test-save");
        var loadResult = repository.Load();

        Assert.That(saveResult.IsSuccess, Is.True);
        Assert.That(loadResult.Result.IsSuccess, Is.True);
        Assert.That(loadResult.Snapshot.Revision, Is.EqualTo(3));
        Assert.That(loadResult.Snapshot.GetCurrencyBalance("currency-coins"), Is.EqualTo(125));
        Assert.That(loadResult.Snapshot.GetUpgradeLevel("launch-power"), Is.EqualTo(2));
    }

    [Test]
    public void Load_CorruptPrimaryWithValidBackup_LoadsBackup()
    {
        var repository = CreateRepository(out var paths);
        var serializer = new EconomySaveSerializer(new EconomySaveSettings(), CreateContentIndex());

        var backupSnapshot = new PlayerEconomySnapshot(
            revision: 7,
            new[] { new PlayerCurrencyBalance("currency-coins", 44) },
            Array.Empty<PlayerUpgradeLevel>());
        Directory.CreateDirectory(paths.PersistentDataPath);
        File.WriteAllText(paths.PrimaryPath, "{ broken json", System.Text.Encoding.UTF8);
        File.WriteAllText(paths.BackupPath, serializer.Serialize(backupSnapshot), System.Text.Encoding.UTF8);
        LogAssert.Expect(LogType.Error, new Regex("Economy save load failed.*primary"));

        var loadResult = repository.Load();

        Assert.That(loadResult.Result.Status, Is.EqualTo(EconomyPersistenceStatus.LoadedBackup));
        Assert.That(loadResult.Snapshot.GetCurrencyBalance("currency-coins"), Is.EqualTo(44));
    }

    [Test]
    public void Load_UnknownFutureSchema_ReturnsDefaultsAndLogsError()
    {
        var repository = CreateRepository(out var paths);
        Directory.CreateDirectory(paths.PersistentDataPath);
        File.WriteAllText(paths.PrimaryPath, "{\"schemaVersion\":999,\"revision\":1}", System.Text.Encoding.UTF8);
        LogAssert.Expect(LogType.Error, new Regex("future schema|schema version"));

        var loadResult = repository.Load();

        Assert.That(loadResult.Result.IsSuccess, Is.False);
        Assert.That(loadResult.Snapshot.CurrencyBalances, Is.Empty);
        Assert.That(loadResult.Snapshot.UpgradeLevels, Is.Empty);
    }

    [Test]
    public void Load_NegativeKnownCurrencyBalance_RepairsToZeroAndLogsWarning()
    {
        var repository = CreateRepository(out var paths);
        Directory.CreateDirectory(paths.PersistentDataPath);

        File.WriteAllText(
            paths.PrimaryPath,
            "{\"schemaVersion\":1,\"revision\":4,\"currencyBalances\":[{\"id\":\"currency-coins\",\"amount\":-5}],\"upgradeLevels\":[]}",
            System.Text.Encoding.UTF8);
        LogAssert.Expect(LogType.Warning, new Regex("negative currency balance.*currency-coins"));

        var loadResult = repository.Load();

        Assert.That(loadResult.Result.IsSuccess, Is.True);
        Assert.That(loadResult.Snapshot.GetCurrencyBalance("currency-coins"), Is.Zero);
    }

    [Test]
    public void Load_KnownUpgradeAboveMax_ClampsToMaxAndLogsWarning()
    {
        var repository = CreateRepository(out var paths);
        Directory.CreateDirectory(paths.PersistentDataPath);

        File.WriteAllText(
            paths.PrimaryPath,
            "{\"schemaVersion\":1,\"revision\":4,\"currencyBalances\":[],\"upgradeLevels\":[{\"id\":\"launch-power\",\"level\":5}]}",
            System.Text.Encoding.UTF8);
        LogAssert.Expect(LogType.Warning, new Regex("upgrade level.*launch-power.*clamped"));

        var loadResult = repository.Load();

        Assert.That(loadResult.Result.IsSuccess, Is.True);
        Assert.That(loadResult.Snapshot.GetUpgradeLevel("launch-power"), Is.EqualTo(2));
    }

    [Test]
    public void Load_OldSchema_MigratesToCurrentSnapshot()
    {
        var repository = CreateRepository(out var paths);
        Directory.CreateDirectory(paths.PersistentDataPath);

        File.WriteAllText(
            paths.PrimaryPath,
            "{\"currencyBalances\":[{\"id\":\"currency-coins\",\"amount\":7}],\"upgradeLevels\":[{\"id\":\"launch-power\",\"level\":1}]}",
            System.Text.Encoding.UTF8);

        var loadResult = repository.Load();

        Assert.That(loadResult.Result.IsSuccess, Is.True);
        Assert.That(loadResult.Snapshot.GetCurrencyBalance("currency-coins"), Is.EqualTo(7));
        Assert.That(loadResult.Snapshot.GetUpgradeLevel("launch-power"), Is.EqualTo(1));
    }

    [Test]
    public void SaveThenLoad_UnknownIds_PreservesEntries()
    {
        var repository = CreateRepository(out _);

        var snapshot = new PlayerEconomySnapshot(
            revision: 10,
            new[] { new PlayerCurrencyBalance("legacy-currency", 9) },
            new[] { new PlayerUpgradeLevel("legacy-upgrade", 3) });

        repository.Save(snapshot, "unknown-roundtrip");
        var loadResult = repository.Load();

        Assert.That(loadResult.Snapshot.GetCurrencyBalance("legacy-currency"), Is.EqualTo(9));
        Assert.That(loadResult.Snapshot.GetUpgradeLevel("legacy-upgrade"), Is.EqualTo(3));
    }

    private EconomySaveRepository CreateRepository(out TestSavePaths paths)
    {
        var persistentDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _temporaryDirectories.Add(persistentDataPath);

        var settings = new EconomySaveSettings();

        paths = new TestSavePaths(
            persistentDataPath,
            Path.Combine(persistentDataPath, settings.PrimaryFileName),
            Path.Combine(persistentDataPath, settings.BackupFileName));

        return new EconomySaveRepository(
            new TestPersistentDataPathProvider(persistentDataPath),
            new EconomySaveSerializer(settings, CreateContentIndex()),
            settings);
    }

    private IPlayerEconomyContentIndex CreateContentIndex()
    {
        return new TestContentIndex(
            new[] { "currency-coins" },
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["launch-power"] = 2
            });
    }

    private sealed class TestPersistentDataPathProvider : IPersistentDataPathProvider
    {
        public string PersistentDataPath { get; }

        public TestPersistentDataPathProvider(string persistentDataPath)
        {
            PersistentDataPath = persistentDataPath;
        }
    }

    private sealed class TestContentIndex : IPlayerEconomyContentIndex
    {
        private readonly HashSet<string> _knownCurrencyIds;
        private readonly Dictionary<string, int> _upgradeMaxLevels;

        public TestContentIndex(IEnumerable<string> knownCurrencyIds, Dictionary<string, int> upgradeMaxLevels)
        {
            _knownCurrencyIds = new HashSet<string>(knownCurrencyIds, StringComparer.Ordinal);
            _upgradeMaxLevels = upgradeMaxLevels;
        }

        public bool IsKnownCurrencyId(string currencySaveId)
        {
            return !string.IsNullOrWhiteSpace(currencySaveId) && _knownCurrencyIds.Contains(currencySaveId);
        }

        public bool TryGetUpgradeMaxLevel(string upgradeStableId, out int maxLevel)
        {
            return _upgradeMaxLevels.TryGetValue(upgradeStableId, out maxLevel);
        }
    }

    private readonly struct TestSavePaths
    {
        public string PersistentDataPath { get; }
        public string PrimaryPath { get; }
        public string BackupPath { get; }

        public TestSavePaths(string persistentDataPath, string primaryPath, string backupPath)
        {
            PersistentDataPath = persistentDataPath;
            PrimaryPath = primaryPath;
            BackupPath = backupPath;
        }
    }
}
