using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay;
using Game.Gameplay.Economy;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunRewardCommitterTests
{
    private readonly List<UnityEngine.Object> _objects = new();

    [TearDown]
    public void OnTearDown()
    {
        foreach (var unityObject in _objects)
        {
            UnityEngine.Object.DestroyImmediate(unityObject);
        }

        _objects.Clear();
    }

    [Test]
    public void RunResultAccepted_WithCurrencySnapshot_GrantsWalletAndCommitsSave()
    {
        var coins = CreateCurrencyDefinition("Coins", "currency-coins");
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());
        var committer = new RecordingEconomyCommitter();
        var notifier = new FakeRunResultNotifier();
        var rewardCommitter = new RunRewardCommitter(notifier, storage, committer);
        ((IInitializable)rewardCommitter).Initialize();
        var result = CreateRunResult(new RunCurrencySnapshot(new[] { new RunCurrencyAmount(coins, 12) }));

        notifier.RaiseRunResultAccepted(result);

        Assert.That(storage.GetAmount(coins), Is.EqualTo(12));
        Assert.That(committer.CommitReasons, Is.EqualTo(new[] { "run-reward" }));
    }

    [Test]
    public void RunResultAccepted_SaveCommitFails_LogsErrorAndKeepsInMemoryWallet()
    {
        var coins = CreateCurrencyDefinition("Coins", "currency-coins");
        ICurrencyStorage storage = new CurrencyStorage(new PlayerEconomyState());

        var committer = new RecordingEconomyCommitter
        {
            NextResult = new EconomyPersistenceResult(
                EconomyPersistenceStatus.Failed,
                "run-reward",
                "write failed",
                new InvalidOperationException("write failed"))
        };
        var notifier = new FakeRunResultNotifier();
        var rewardCommitter = new RunRewardCommitter(notifier, storage, committer);
        ((IInitializable)rewardCommitter).Initialize();
        LogAssert.Expect(LogType.Error, new Regex("Run reward save commit failed.*write failed"));

        notifier.RaiseRunResultAccepted(CreateRunResult(new RunCurrencySnapshot(new[] { new RunCurrencyAmount(coins, 8) })));

        Assert.That(storage.GetAmount(coins), Is.EqualTo(8));
        Assert.That(committer.CommitReasons, Is.EqualTo(new[] { "run-reward" }));
    }

    private RunResult CreateRunResult(RunCurrencySnapshot snapshot)
    {
        return new RunResult(
            RunEndReason.Finished,
            elapsedTime: 1f,
            distanceTravelled: 10f,
            finalPosition: Vector3.forward,
            finalSpeed: 2f,
            snapshot);
    }

    private CurrencyDefinition CreateCurrencyDefinition(string objectName, string saveId)
    {
        var definition = Track(ScriptableObject.CreateInstance<CurrencyDefinition>());
        definition.name = objectName;
        definition.SetSaveIdForTests(saveId);
        return definition;
    }

    private T Track<T>(T value)
        where T : UnityEngine.Object
    {
        _objects.Add(value);
        return value;
    }

    private sealed class FakeRunResultNotifier : IRunResultNotifier
    {
        public event Action<RunResult> RunResultAccepted;

        public void RaiseRunResultAccepted(RunResult result)
        {
            RunResultAccepted?.Invoke(result);
        }
    }

    private sealed class RecordingEconomyCommitter : IEconomyCommitter
    {
        private readonly List<string> _commitReasons = new();

        public IReadOnlyList<string> CommitReasons => _commitReasons;
        public bool IsCommitPending { get; private set; }

        public EconomyPersistenceResult NextResult { get; set; } =
            new(EconomyPersistenceStatus.Saved, "run-reward", "saved", exception: null);

        public EconomyPersistenceResult CommitImportant(string reason)
        {
            IsCommitPending = true;
            _commitReasons.Add(reason);
            IsCommitPending = false;
            return NextResult;
        }

        public EconomyPersistenceResult RequestBestEffortFlush(string reason)
        {
            _commitReasons.Add(reason);
            return NextResult;
        }
    }
}
