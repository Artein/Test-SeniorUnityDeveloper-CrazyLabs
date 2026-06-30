using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Game.Foundation.Persistence;
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
    public void RunResultAccepted_WithRealCommitter_SavesRewardForNextStateLoad()
    {
        var coins = CreateCurrencyDefinition("Coins", "currency-coins");
        var settings = new EconomySaveSettings();
        var savePath = CreateTemporaryDirectory();
        var contentIndex = new TestContentIndex(new[] { coins.SaveId });

        var repository = new EconomySaveRepository(
            new TestPersistentDataPathProvider(savePath),
            new EconomySaveSerializer(settings, contentIndex),
            settings);
        var state = new PlayerEconomyState();
        ICurrencyStorage storage = new CurrencyStorage(state);
        var saveQueue = new EconomySaveQueue(repository);
        var committer = new EconomyCommitter(state, saveQueue);
        var notifier = new FakeRunResultNotifier();
        var rewardCommitter = new RunRewardCommitter(notifier, storage, committer);
        ((IInitializable)rewardCommitter).Initialize();

        notifier.RaiseRunResultAccepted(CreateRunResult(new RunCurrencySnapshot(new[] { new RunCurrencyAmount(coins, 1) })));

        var nextState = new PlayerEconomyState();
        var loader = new PlayerEconomyStateLoader(nextState, repository);
        ((IInitializable)loader).Initialize();
        ICurrencyStorage nextStorage = new CurrencyStorage(nextState);

        Assert.That(File.Exists(Path.Combine(savePath, settings.PrimaryFileName)), Is.True);
        Assert.That(nextStorage.GetAmount(coins), Is.EqualTo(1));
    }

    [Test]
    public void RunResultAccepted_WithAsyncSaveQueue_DoesNotReadPersistentDataPathOnWorkerThread()
    {
        var coins = CreateCurrencyDefinition("Coins", "currency-coins");
        var settings = new EconomySaveSettings();
        var savePath = CreateTemporaryDirectory();
        var contentIndex = new TestContentIndex(new[] { coins.SaveId });

        var pathProvider = new MainThreadOnlyPersistentDataPathProvider(
            savePath,
            Thread.CurrentThread.ManagedThreadId);

        var repository = new EconomySaveRepository(
            pathProvider,
            new EconomySaveSerializer(settings, contentIndex),
            settings);
        var state = new PlayerEconomyState();
        ICurrencyStorage storage = new CurrencyStorage(state);
        var saveQueue = new EconomySaveQueue(repository);
        var committer = new EconomyCommitter(state, saveQueue);
        var notifier = new FakeRunResultNotifier();
        var rewardCommitter = new RunRewardCommitter(notifier, storage, committer);
        ((IInitializable)rewardCommitter).Initialize();

        notifier.RaiseRunResultAccepted(CreateRunResult(new RunCurrencySnapshot(new[] { new RunCurrencyAmount(coins, 1) })));

        Assert.That(File.Exists(Path.Combine(savePath, settings.PrimaryFileName)), Is.True);
        Assert.That(pathProvider.AccessCount, Is.GreaterThanOrEqualTo(1));
        Assert.That(pathProvider.WorkerThreadAccessCount, Is.Zero);
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

    private string CreateTemporaryDirectory()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _temporaryDirectories.Add(temporaryDirectory);
        return temporaryDirectory;
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

    private sealed class MainThreadOnlyPersistentDataPathProvider : IPersistentDataPathProvider
    {
        private readonly int _allowedThreadId;
        private readonly string _persistentDataPath;

        public int AccessCount { get; private set; }
        public int WorkerThreadAccessCount { get; private set; }

        public MainThreadOnlyPersistentDataPathProvider(string persistentDataPath, int allowedThreadId)
        {
            _persistentDataPath = persistentDataPath;
            _allowedThreadId = allowedThreadId;
        }

        public string PersistentDataPath
        {
            get
            {
                AccessCount++;

                if (Thread.CurrentThread.ManagedThreadId != _allowedThreadId)
                {
                    WorkerThreadAccessCount++;
                    throw new InvalidOperationException("PersistentDataPath accessed from worker thread.");
                }

                return _persistentDataPath;
            }
        }
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

        public TestContentIndex(IEnumerable<string> knownCurrencyIds)
        {
            _knownCurrencyIds = new HashSet<string>(knownCurrencyIds, StringComparer.Ordinal);
        }

        public bool IsKnownCurrencyId(string currencySaveId)
        {
            return !string.IsNullOrWhiteSpace(currencySaveId) && _knownCurrencyIds.Contains(currencySaveId);
        }

        public bool TryGetUpgradeMaxLevel(string upgradeStableId, out int maxLevel)
        {
            maxLevel = 0;
            return false;
        }
    }
}
