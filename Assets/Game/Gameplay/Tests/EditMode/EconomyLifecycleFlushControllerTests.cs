using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Foundation.ApplicationLifecycle;
using NUnit.Framework;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class EconomyLifecycleFlushControllerTests
{
    [Test]
    public void FocusLost_Initialized_RequestsBestEffortFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();

        lifecycleNotifier.NotifyFocusChanged(false);

        Assert.That(committer.FlushReasons, Is.EqualTo(new[] { "application-focus-lost" }));
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void FocusGained_Initialized_DoesNotRequestFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();

        lifecycleNotifier.NotifyFocusChanged(true);

        Assert.That(committer.FlushReasons, Is.Empty);
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void ApplicationQuitting_Initialized_RequestsBestEffortFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();

        lifecycleNotifier.NotifyQuitting();

        Assert.That(committer.FlushReasons, Is.EqualTo(new[] { "application-quit" }));
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void FocusLost_AfterDispose_DoesNotRequestFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();
        ((IDisposable)controller).Dispose();

        lifecycleNotifier.NotifyFocusChanged(false);

        Assert.That(committer.FlushReasons, Is.Empty);
    }

    [Test]
    public void ApplicationPaused_Initialized_RequestsBestEffortFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();

        lifecycleNotifier.NotifyPauseChanged(true);

        Assert.That(committer.FlushReasons, Is.EqualTo(new[] { "application-paused" }));
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void ApplicationResumed_Initialized_DoesNotRequestFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();

        lifecycleNotifier.NotifyPauseChanged(false);

        Assert.That(committer.FlushReasons, Is.Empty);
        ((IDisposable)controller).Dispose();
    }

    [Test]
    public void ApplicationPaused_AfterDispose_DoesNotRequestFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();
        ((IDisposable)controller).Dispose();

        lifecycleNotifier.NotifyPauseChanged(true);

        Assert.That(committer.FlushReasons, Is.Empty);
    }

    [Test]
    public void ApplicationQuitting_AfterDispose_DoesNotRequestFlush()
    {
        var committer = new RecordingEconomyCommitter();
        var lifecycleNotifier = new RecordingApplicationLifecycleNotifier();
        var controller = CreateController(committer, lifecycleNotifier);
        ((IInitializable)controller).Initialize();
        ((IDisposable)controller).Dispose();

        lifecycleNotifier.NotifyQuitting();

        Assert.That(committer.FlushReasons, Is.Empty);
    }

    private EconomyLifecycleFlushController CreateController(
        IEconomyCommitter committer,
        RecordingApplicationLifecycleNotifier lifecycleNotifier)
    {
        return new EconomyLifecycleFlushController(
            committer,
            lifecycleNotifier,
            lifecycleNotifier,
            lifecycleNotifier);
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

    private sealed class RecordingApplicationLifecycleNotifier :
        IApplicationPauseNotifier,
        IApplicationFocusChangeNotifier,
        IApplicationQuitNotifier
    {
        public event Action<bool> PauseChanged;
        public event Action<bool> FocusChanged;
        public event Action Quitting;

        public void NotifyPauseChanged(bool isPaused)
        {
            PauseChanged?.Invoke(isPaused);
        }

        public void NotifyFocusChanged(bool hasFocus)
        {
            FocusChanged?.Invoke(hasFocus);
        }

        public void NotifyQuitting()
        {
            Quitting?.Invoke();
        }
    }
}
