using System;
using Game.Foundation.Time;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class SlingshotPresentationContextSourceTests
{
    private FakeActivePullNotifier _activePullNotifier;
    private FakeCaptureLifecycleNotifier _captureLifecycleNotifier;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private FakeGeometrySnapshotSource _geometrySnapshotSource;
    private FakeSlingshotConfig _config;
    private SlingshotPullOffsetNormalizer _pullOffsetNormalizer;
    private FakeTime _clock;
    private SlingshotPresentationContextSource _source;

    [SetUp]
    public void OnSetUp()
    {
        _activePullNotifier = new FakeActivePullNotifier();
        _captureLifecycleNotifier = new FakeCaptureLifecycleNotifier();
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();

        _geometrySnapshotSource = new FakeGeometrySnapshotSource
        {
            CurrentGeometry = new SlingshotGeometrySnapshot(
                leftAnchorPosition: new Vector3(-1f, 0f, 0f),
                rightAnchorPosition: new Vector3(1f, 0f, 0f),
                restPoint: Vector3.zero,
                launchFrameRight: Vector3.right,
                launchFrameForward: Vector3.forward,
                launchFrameUp: Vector3.up)
        };

        _config = new FakeSlingshotConfig
        {
            MinimumPullDistance = 0.25f,
            MaximumPullDistance = 2f,
            MaximumLateralPull = 1.25f,
            BandContactPadding = 0.05f
        };
        _pullOffsetNormalizer = new SlingshotPullOffsetNormalizer(_config);
        _clock = new FakeTime { DeltaTime = 0.1f };
        _source = CreateSource();
        ((IInitializable)_source).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_source).Dispose();
    }

    [Test]
    public void Current_DefaultContext_HasInactiveZeroedChannels()
    {
        Assert.That(_source.Current.HasActivePull, Is.False);
        Assert.That(_source.Current.NormalizedPull, Is.Zero);
        Assert.That(_source.Current.NormalizedPullOffset, Is.Zero);
        Assert.That(_source.Current.HasLaunchPush, Is.False);
        Assert.That(_source.Current.LaunchPushElapsedSeconds, Is.Zero);
        Assert.That(_source.Current.NormalizedLaunchPower, Is.Zero);
        Assert.That(_source.Current.NormalizedLaunchOffset, Is.Zero);
    }

    [Test]
    public void ActivePullChanged_UpdatesPullFieldsWithoutStartingLaunchPush()
    {
        _activePullNotifier.Change(new SlingshotActivePullContext(0.4f, -0.25f));

        Assert.That(_source.Current.HasActivePull, Is.True);
        Assert.That(_source.Current.NormalizedPull, Is.EqualTo(0.4f).Within(0.0001f));
        Assert.That(_source.Current.NormalizedPullOffset, Is.EqualTo(-0.25f).Within(0.0001f));
        Assert.That(_source.Current.HasLaunchPush, Is.False);
        Assert.That(_source.Current.NormalizedLaunchPower, Is.Zero);
        Assert.That(_source.Current.NormalizedLaunchOffset, Is.Zero);
    }

    [Test]
    public void ActivePullCleared_ZerosPullFields()
    {
        _activePullNotifier.Change(new SlingshotActivePullContext(0.4f, -0.25f));

        _activePullNotifier.Clear();

        Assert.That(_source.Current.HasActivePull, Is.False);
        Assert.That(_source.Current.NormalizedPull, Is.Zero);
        Assert.That(_source.Current.NormalizedPullOffset, Is.Zero);
    }

    [Test]
    public void LaunchApplied_StartsLaunchPushAtZeroAndFreezesAcceptedLaunchValues()
    {
        _activePullNotifier.Change(new SlingshotActivePullContext(0.2f, -0.75f));

        var launchRequest = new SlingshotLaunchRequest(
            normalizedPower: 0.8f, 
            FullLateralPullDistance(), 
            pullOffset: 1f, 
            finalPullPoint: new Vector3(1f, 0f, -1f), 
            launchDirection: Vector3.forward, 
            launchSpeed: 10f,
            launchUpDirection: Vector3.up, 
            launchUpSpeed: 1f);

        _launchAppliedNotifier.Apply(launchRequest);

        Assert.That(_source.Current.HasActivePull, Is.False);
        Assert.That(_source.Current.NormalizedPull, Is.Zero);
        Assert.That(_source.Current.NormalizedPullOffset, Is.Zero);
        Assert.That(_source.Current.HasLaunchPush, Is.True);
        Assert.That(_source.Current.LaunchPushElapsedSeconds, Is.Zero);
        Assert.That(_source.Current.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(_source.Current.NormalizedLaunchOffset, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void Tick_WhileLaunchPushActive_AdvancesElapsedWithInjectedTime()
    {
        _launchAppliedNotifier.Apply(new SlingshotLaunchRequest(0.8f, FullLateralPullDistance(), 1f, new Vector3(1f, 0f, -1f), Vector3.forward,
            10f, Vector3.up, 1f));

        ((ITickable)_source).Tick();

        Assert.That(_source.Current.HasLaunchPush, Is.True);
        Assert.That(_source.Current.LaunchPushElapsedSeconds, Is.EqualTo(0.1f).Within(0.0001f));
    }

    [Test]
    public void CaptureDisabled_ClearsPullButKeepsLaunchPush()
    {
        _activePullNotifier.Change(new SlingshotActivePullContext(0.4f, -0.25f));

        _launchAppliedNotifier.Apply(new SlingshotLaunchRequest(0.8f, FullLateralPullDistance(), 1f, new Vector3(1f, 0f, -1f), Vector3.forward,
            10f, Vector3.up, 1f));
        _activePullNotifier.Change(new SlingshotActivePullContext(0.4f, -0.25f));

        _captureLifecycleNotifier.Disable();

        Assert.That(_source.Current.HasActivePull, Is.False);
        Assert.That(_source.Current.NormalizedPull, Is.Zero);
        Assert.That(_source.Current.NormalizedPullOffset, Is.Zero);
        Assert.That(_source.Current.HasLaunchPush, Is.True);
        Assert.That(_source.Current.NormalizedLaunchPower, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(_source.Current.NormalizedLaunchOffset, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void CaptureEnabled_ClearsPullAndLaunchPush()
    {
        _activePullNotifier.Change(new SlingshotActivePullContext(0.4f, -0.25f));

        _launchAppliedNotifier.Apply(new SlingshotLaunchRequest(0.8f, FullLateralPullDistance(), 1f, new Vector3(1f, 0f, -1f), Vector3.forward,
            10f, Vector3.up, 1f));

        _captureLifecycleNotifier.Enable();

        Assert.That(_source.Current.HasActivePull, Is.False);
        Assert.That(_source.Current.NormalizedPull, Is.Zero);
        Assert.That(_source.Current.NormalizedPullOffset, Is.Zero);
        Assert.That(_source.Current.HasLaunchPush, Is.False);
        Assert.That(_source.Current.LaunchPushElapsedSeconds, Is.Zero);
        Assert.That(_source.Current.NormalizedLaunchPower, Is.Zero);
        Assert.That(_source.Current.NormalizedLaunchOffset, Is.Zero);
    }

    private SlingshotPresentationContextSource CreateSource()
    {
        return new SlingshotPresentationContextSource(_activePullNotifier, _captureLifecycleNotifier, _launchAppliedNotifier, _geometrySnapshotSource,
            _pullOffsetNormalizer, _clock);
    }

    private float FullLateralPullDistance()
    {
        return Mathf.Max(0.02f, _config.MinimumPullDistance + (_config.BandContactPadding * 2f)) + (_config.BandContactPadding * 2f);
    }

    private sealed class FakeActivePullNotifier : ISlingshotActivePullNotifier
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

    private sealed class FakeCaptureLifecycleNotifier : ISlingshotCaptureLifecycleNotifier
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

    private sealed class FakeGeometrySnapshotSource : ISlingshotGeometrySnapshotSource
    {
        public SlingshotGeometrySnapshot CurrentGeometry { get; set; }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }
}
