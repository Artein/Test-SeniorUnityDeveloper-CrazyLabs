using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Economy;
using NUnit.Framework;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class EconomyLifecycleFlushControllerTests
{
    [Test]
    public void FocusLost_Initialized_RequestsBestEffortFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var controller = new EconomyLifecycleFlushController(committer);
        ((IInitializable)controller).Initialize();

        controller.NotifyApplicationFocusChangedForTests(false);

        Assert.That(committer.FlushReasons, Is.EqualTo(new[] { "application-focus-lost" }));
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void FocusGained_Initialized_DoesNotRequestFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var controller = new EconomyLifecycleFlushController(committer);
        ((IInitializable)controller).Initialize();

        controller.NotifyApplicationFocusChangedForTests(true);

        Assert.That(committer.FlushReasons, Is.Empty);
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void ApplicationQuitting_Initialized_RequestsBestEffortFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var controller = new EconomyLifecycleFlushController(committer);
        ((IInitializable)controller).Initialize();

        controller.NotifyApplicationQuittingForTests();

        Assert.That(committer.FlushReasons, Is.EqualTo(new[] { "application-quit" }));
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void FocusLost_AfterDispose_DoesNotRequestFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var controller = new EconomyLifecycleFlushController(committer);
        ((IInitializable)controller).Initialize();
        ((IDisposable)controller).Dispose();

        controller.NotifyApplicationFocusChangedForTests(false);

        Assert.That(committer.FlushReasons, Is.Empty);
    }

    private sealed class RecordingEconomyCommitter : IEconomyCommitter
    {
        private readonly List<string> _flushReasons = new();

        public IReadOnlyList<string> FlushReasons => _flushReasons;
        public bool IsCommitPending => false;

        public EconomyPersistenceResult CommitImportant(string reason)
        {
            throw new InvalidOperationException("Lifecycle flush controller must not request important commits.");
        }

        public EconomyPersistenceResult RequestBestEffortFlush(string reason)
        {
            _flushReasons.Add(reason);
            return new EconomyPersistenceResult(EconomyPersistenceStatus.Saved, reason, "saved", exception: null);
        }
    }
}
