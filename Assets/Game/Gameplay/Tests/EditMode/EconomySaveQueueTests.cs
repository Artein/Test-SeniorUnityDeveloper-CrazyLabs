using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class EconomySaveQueueTests
{
    [Test]
    public void EnqueueImportantAsync_MultipleSnapshots_WritesInEnqueueOrder()
    {
        var repository = new RecordingSaveRepository();
        var queue = new EconomySaveQueue(repository);

        var first = new PlayerEconomySnapshot(
            revision: 1,
            new[] { new PlayerCurrencyBalance("currency-coins", 1) },
            Array.Empty<PlayerUpgradeLevel>());

        var second = new PlayerEconomySnapshot(
            revision: 2,
            new[] { new PlayerCurrencyBalance("currency-coins", 2) },
            Array.Empty<PlayerUpgradeLevel>());

        var firstResult = queue.EnqueueImportantAsync(first, "first");
        var secondResult = queue.EnqueueImportantAsync(second, "second");

        Assert.That(firstResult.GetAwaiter().GetResult().IsSuccess, Is.True);
        Assert.That(secondResult.GetAwaiter().GetResult().IsSuccess, Is.True);
        Assert.That(repository.SavedRevisions, Is.EqualTo(new[] { 1L, 2L }));
    }

    [Test]
    public void CommitImportant_FailingRepository_ReturnsFailureAndLogsError()
    {
        var state = new PlayerEconomyState();
        state.SetCurrencyBalance("currency-coins", 5);

        var repository = new RecordingSaveRepository
        {
            NextResult = new EconomyPersistenceResult(
                EconomyPersistenceStatus.Failed,
                "test-commit",
                "disk full",
                new InvalidOperationException("disk full"))
        };
        var committer = new EconomyCommitter(state, new EconomySaveQueue(repository));
        LogAssert.Expect(LogType.Error, new Regex("Economy save commit failed.*disk full"));

        var result = committer.CommitImportant("test-commit");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(repository.SavedRevisions, Is.EqualTo(new[] { 1L }));
    }

    [Test]
    public void CommitImportant_SuccessfulRepository_ReturnsSuccess()
    {
        var state = new PlayerEconomyState();
        state.SetCurrencyBalance("currency-coins", 5);
        var repository = new RecordingSaveRepository();
        var committer = new EconomyCommitter(state, new EconomySaveQueue(repository));

        var result = committer.CommitImportant("test-commit");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(repository.SavedRevisions, Is.EqualTo(new[] { 1L }));
    }

    private sealed class RecordingSaveRepository : IEconomySaveRepository
    {
        private readonly List<long> _savedRevisions = new();

        public IReadOnlyList<long> SavedRevisions => _savedRevisions;

        public EconomyPersistenceResult NextResult { get; set; } =
            new(EconomyPersistenceStatus.Saved, "test-save", "saved", exception: null);

        public EconomyLoadResult Load()
        {
            return new EconomyLoadResult(
                new PlayerEconomySnapshot(0, Array.Empty<PlayerCurrencyBalance>(), Array.Empty<PlayerUpgradeLevel>()),
                new EconomyPersistenceResult(EconomyPersistenceStatus.LoadedDefaults, "test-load", "defaults", exception: null));
        }

        public EconomyPersistenceResult Save(PlayerEconomySnapshot snapshot, string reason)
        {
            _savedRevisions.Add(snapshot.Revision);
            return NextResult;
        }
    }
}
