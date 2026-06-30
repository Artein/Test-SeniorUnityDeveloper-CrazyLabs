using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class PullHintPresenterTests
{
    private TestPullHintView _view;
    private TestPullHintTuning _tuning;
    private TestTime _clock;
    private TestCaptureLifecycleNotifier _captureLifecycleNotifier;
    private TestActivePullNotifier _activePullNotifier;
    private TestGeometrySnapshotSource _geometrySnapshotSource;
    private TestSlingshotInputProjector _inputProjector;
    private PullHintPresenter _presenter;

    [SetUp]
    public void OnSetUp()
    {
        _view = new TestPullHintView();

        _tuning = new TestPullHintTuning
        {
            InitialIdleDelaySeconds = 2f,
            PlaybackDurationSeconds = 1.25f,
            RepeatCooldownSeconds = 4f
        };
        _clock = new TestTime();
        _captureLifecycleNotifier = new TestCaptureLifecycleNotifier();
        _activePullNotifier = new TestActivePullNotifier();

        _geometrySnapshotSource = new TestGeometrySnapshotSource
        {
            CurrentGeometry = new SlingshotGeometrySnapshot(
                new Vector3(-1f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0.25f, 0.5f, 0.75f),
                Vector3.right,
                Vector3.forward,
                Vector3.up)
        };

        _inputProjector = new TestSlingshotInputProjector
        {
            ScreenPosition = new Vector2(120f, 240f)
        };
        _presenter = CreateInitializedPresenter();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_presenter).Dispose();
    }

    [Test]
    public void Tick_CaptureIdleDelayElapsed_ShowsAndPlaysAtProjectedRestPoint()
    {
        _captureLifecycleNotifier.Enable();

        Tick(_tuning.InitialIdleDelaySeconds - 0.1f);

        Assert.That(_view.ShowCount, Is.Zero);
        Assert.That(_view.PlayCount, Is.Zero);

        Tick(0.1f);

        Assert.That(_view.ShowCount, Is.EqualTo(1));
        Assert.That(_view.PlayCount, Is.EqualTo(1));
        Assert.That(_view.IsVisible, Is.True);
        Assert.That(_view.LastScreenPosition, Is.EqualTo(new Vector2(120f, 240f)));
        Assert.That(_inputProjector.ProjectedWorldPoints, Is.EqualTo(new[] { _geometrySnapshotSource.CurrentGeometry.RestPoint }));
    }

    [Test]
    public void Tick_PlaybackElapsed_HidesAndRepeatsAfterCooldown()
    {
        StartFirstPlayback();

        Tick(_tuning.PlaybackDurationSeconds);

        Assert.That(_view.IsVisible, Is.False);
        Assert.That(_view.ShowCount, Is.EqualTo(1));
        Assert.That(_view.PlayCount, Is.EqualTo(1));

        Tick(_tuning.RepeatCooldownSeconds - 0.1f);

        Assert.That(_view.ShowCount, Is.EqualTo(1));
        Assert.That(_view.PlayCount, Is.EqualTo(1));

        Tick(0.1f);

        Assert.That(_view.ShowCount, Is.EqualTo(2));
        Assert.That(_view.PlayCount, Is.EqualTo(2));
        Assert.That(_view.IsVisible, Is.True);
    }

    [Test]
    public void ActivePullChanged_WhenHintIsPlaying_HidesAndSuppressesUntilNextCapture()
    {
        StartFirstPlayback();

        _activePullNotifier.Change(new SlingshotActivePullContext(0.5f, 0f));

        Assert.That(_view.IsVisible, Is.False);

        Tick(100f);

        Assert.That(_view.ShowCount, Is.EqualTo(1));
        Assert.That(_view.PlayCount, Is.EqualTo(1));

        _captureLifecycleNotifier.Disable();
        _captureLifecycleNotifier.Enable();
        Tick(_tuning.InitialIdleDelaySeconds);

        Assert.That(_view.ShowCount, Is.EqualTo(2));
        Assert.That(_view.PlayCount, Is.EqualTo(2));
        Assert.That(_view.IsVisible, Is.True);
    }

    [Test]
    public void Tick_WhenRestPointProjectionFails_DoesNotPlayAndRetriesNextTick()
    {
        _inputProjector.CanProjectWorldToScreen = false;
        _captureLifecycleNotifier.Enable();

        Tick(_tuning.InitialIdleDelaySeconds);

        Assert.That(_view.ShowCount, Is.Zero);
        Assert.That(_view.PlayCount, Is.Zero);

        _inputProjector.CanProjectWorldToScreen = true;
        Tick(0f);

        Assert.That(_view.ShowCount, Is.EqualTo(1));
        Assert.That(_view.PlayCount, Is.EqualTo(1));
    }

    [Test]
    public void CaptureDisabled_WhenHintIsPlaying_HidesAndStopsRepeating()
    {
        StartFirstPlayback();

        _captureLifecycleNotifier.Disable();
        Tick(100f);

        Assert.That(_view.IsVisible, Is.False);
        Assert.That(_view.ShowCount, Is.EqualTo(1));
        Assert.That(_view.PlayCount, Is.EqualTo(1));
    }

    private PullHintPresenter CreateInitializedPresenter()
    {
        var presenter = new PullHintPresenter(
            _view,
            _tuning,
            _clock,
            _captureLifecycleNotifier,
            _activePullNotifier,
            _geometrySnapshotSource,
            _inputProjector);

        ((IInitializable)presenter).Initialize();
        return presenter;
    }

    private void StartFirstPlayback()
    {
        _captureLifecycleNotifier.Enable();
        Tick(_tuning.InitialIdleDelaySeconds);
    }

    private void Tick(float deltaTime)
    {
        _clock.DeltaTime = deltaTime;
        ((ITickable)_presenter).Tick();
    }

    private sealed class TestPullHintView : IPullHintView
    {
        public int ShowCount { get; private set; }
        public int PlayCount { get; private set; }
        public int HideCount { get; private set; }
        public bool IsVisible { get; private set; }
        public Vector2 LastScreenPosition { get; private set; }

        public void ShowAt(Vector2 screenPosition)
        {
            ShowCount += 1;
            LastScreenPosition = screenPosition;
            IsVisible = true;
        }

        public void Play()
        {
            PlayCount += 1;
        }

        public void Hide()
        {
            HideCount += 1;
            IsVisible = false;
        }
    }

    private sealed class TestPullHintTuning : IPullHintTuning
    {
        public float InitialIdleDelaySeconds { get; set; }
        public float PlaybackDurationSeconds { get; set; }
        public float RepeatCooldownSeconds { get; set; }
    }

    private sealed class TestTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }

    private sealed class TestCaptureLifecycleNotifier : ISlingshotCaptureLifecycleNotifier
    {
        public event Action CaptureEnabled;
        public event Action CaptureDisabled;

        public void Enable()
        {
            CaptureEnabled?.Invoke();
        }

        public void Disable()
        {
            CaptureDisabled?.Invoke();
        }
    }

    private sealed class TestActivePullNotifier : ISlingshotActivePullNotifier
    {
        public event Action<SlingshotActivePullContext> ActivePullChanged;
        public event Action ActivePullCleared;

        public void Change(SlingshotActivePullContext context)
        {
            ActivePullChanged?.Invoke(context);
        }

        public void Clear()
        {
            ActivePullCleared?.Invoke();
        }
    }

    private sealed class TestGeometrySnapshotSource : ISlingshotGeometrySnapshotSource
    {
        public SlingshotGeometrySnapshot CurrentGeometry { get; set; }
    }

    private sealed class TestSlingshotInputProjector : ISlingshotInputProjector
    {
        public bool CanProjectWorldToScreen { get; set; } = true;
        public Vector2 ScreenPosition { get; set; }
        public List<Vector3> ProjectedWorldPoints { get; } = new();

        public bool TryProjectScreenToPullPlane(Vector2 screenPosition, SlingshotGeometrySnapshot geometry, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            return false;
        }

        public bool TryProjectWorldToScreen(Vector3 worldPosition, out Vector2 screenPosition)
        {
            ProjectedWorldPoints.Add(worldPosition);

            if (!CanProjectWorldToScreen)
            {
                screenPosition = Vector2.zero;
                return false;
            }

            screenPosition = ScreenPosition;
            return true;
        }
    }
}
