using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceSteeringFrameSourceTests
{
    private FakeRunSurfaceContextSource _surfaceContextSource;
    private RunSurfaceSteeringFrameSource _source;

    [SetUp]
    public void OnSetUp()
    {
        _surfaceContextSource = new FakeRunSurfaceContextSource();
        _source = new RunSurfaceSteeringFrameSource(_surfaceContextSource);
    }

    [Test]
    public void GetUpDirection_GroundedValidSurfaceNormal_ReturnsGroundNormal()
    {
        var groundNormal = new Vector3(0f, 2f, 1f).normalized;
        _surfaceContextSource.Current = new RunSurfaceContext(true, groundNormal, 0f);

        var upDirection = _source.GetUpDirection(Vector3.up);

        Assert.That(upDirection, Is.EqualTo(groundNormal));
    }

    [Test]
    public void GetUpDirection_Ungrounded_ReturnsFallbackUpDirection()
    {
        var fallbackUp = new Vector3(1f, 1f, 0f).normalized;
        _surfaceContextSource.Current = new RunSurfaceContext(false, Vector3.up, 0f);

        var upDirection = _source.GetUpDirection(fallbackUp);

        Assert.That(upDirection, Is.EqualTo(fallbackUp));
    }

    [Test]
    public void GetUpDirection_GroundedInvalidSurfaceNormal_ReturnsFallbackUpDirection()
    {
        var fallbackUp = new Vector3(1f, 1f, 0f).normalized;
        _surfaceContextSource.Current = new RunSurfaceContext(true, Vector3.zero, 0f);

        var upDirection = _source.GetUpDirection(fallbackUp);

        Assert.That(upDirection, Is.EqualTo(fallbackUp));
    }

    private sealed class FakeRunSurfaceContextSource : IRunSurfaceContextSource
    {
        public RunSurfaceContext Current { get; set; }
    }
}
