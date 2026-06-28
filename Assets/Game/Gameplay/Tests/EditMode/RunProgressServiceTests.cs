using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunProgressServiceTests
{
    private FakeRunProgressFrameSource _frameSource;
    private FakeRunMotionSource _motionSource;
    private FakeSlingshotLaunchAppliedNotifier _launchAppliedNotifier;
    private RunProgressService _service;

    [SetUp]
    public void OnSetUp()
    {
        _frameSource = new FakeRunProgressFrameSource
        {
            ForwardDirection = new Vector3(0f, 0f, 5f),
            UpDirection = new Vector3(0f, 2f, 0f)
        };
        _motionSource = new FakeRunMotionSource();
        _launchAppliedNotifier = new FakeSlingshotLaunchAppliedNotifier();
        _service = new RunProgressService(_frameSource, _motionSource, _launchAppliedNotifier);
        ((IInitializable)_service).Initialize();
    }

    [TearDown]
    public void OnTearDown()
    {
        ((IDisposable)_service).Dispose();
        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void LaunchApplied_SnapshotsNormalizedFrameWithMotionOrigin()
    {
        _motionSource.Position = new Vector3(10f, 3f, -4f);

        _launchAppliedNotifier.Apply(CreateLaunchRequest(new Vector3(1f, 0f, 0f)));

        Assert.That(_service.HasValidSnapshot, Is.True);
        Assert.That(_service.Snapshot.Origin, Is.EqualTo(_motionSource.Position));
        Assert.That(_service.Snapshot.ForwardDirection, Is.EqualTo(Vector3.forward));
        Assert.That(_service.Snapshot.RightDirection, Is.EqualTo(Vector3.right));
        Assert.That(_service.Snapshot.UpDirection, Is.EqualTo(Vector3.up));
    }

    [Test]
    public void SamplePosition_MaxProgressIgnoresSidewaysVerticalAndBackwardMovement()
    {
        Assert.That(_service.TryBeginRun(Vector3.zero, out _), Is.True);

        _service.SamplePosition(new Vector3(5f, 10f, 0f));
        _service.SamplePosition(new Vector3(5f, 3f, 7f));
        _service.SamplePosition(new Vector3(5f, 3f, 2f));

        Assert.That(_service.CurrentForwardProgress, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(_service.MaximumForwardProgress, Is.EqualTo(7f).Within(0.0001f));
    }

    [Test]
    public void TryBeginRun_InvalidFrame_ReturnsFalseAndLogs()
    {
        _frameSource.ForwardDirection = Vector3.up;
        _frameSource.UpDirection = Vector3.up;

        LogAssert.Expect(LogType.Error, "Invalid Run Progress Frame. Run Progress Frame forward direction must not be parallel to up direction.");

        var began = _service.TryBeginRun(Vector3.zero, out var error);

        Assert.That(began, Is.False);
        Assert.That(_service.HasValidSnapshot, Is.False);
        Assert.That(error, Does.Contain("parallel"));
    }

    [Test]
    public void LaunchRequestDirection_DoesNotDefineProgressAxis()
    {
        _motionSource.Position = Vector3.zero;
        _launchAppliedNotifier.Apply(CreateLaunchRequest(Vector3.right));

        _service.SamplePosition(new Vector3(5f, 0f, 8f));

        Assert.That(_service.MaximumForwardProgress, Is.EqualTo(8f).Within(0.0001f));
    }

    private SlingshotLaunchRequest CreateLaunchRequest(Vector3 launchDirection)
    {
        return new SlingshotLaunchRequest(
            1f,
            1f,
            0f,
            Vector3.zero,
            launchDirection.normalized,
            10f,
            Vector3.up,
            1f);
    }

    private sealed class FakeRunProgressFrameSource : IRunProgressFrameSource
    {
        public Vector3 ForwardDirection { get; set; }
        public Vector3 UpDirection { get; set; }

        public bool TryCreateSnapshot(Vector3 origin, out RunProgressFrameSnapshot snapshot, out string error)
        {
            return RunProgressFrameSnapshot.TryCreate(origin, ForwardDirection, UpDirection, out snapshot, out error);
        }
    }

    private sealed class FakeRunMotionSource : IRunMotionSource
    {
        public Vector3 Position { get; set; }
        public Vector3 LinearVelocity { get; set; }
    }

    private sealed class FakeSlingshotLaunchAppliedNotifier : ISlingshotLaunchAppliedNotifier
    {
        public event Action<SlingshotLaunchRequest> LaunchApplied;

        public void Apply(SlingshotLaunchRequest request)
        {
            LaunchApplied?.Invoke(request);
        }
    }
}
