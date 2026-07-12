using System;
using Game.Gameplay.Diagnostics;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunDiagnosticsOverlayBufferTests
{
    [Test]
    public void Constructor_NonPositiveCapacity_Throws()
    {
        Assert.That(() => new RunDiagnosticsOverlayBuffer(capacity: 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Add_UnderCapacity_StoresSamplesChronologically()
    {
        var buffer = new RunDiagnosticsOverlayBuffer(capacity: 3);

        buffer.Add(CreateSample(speed: 1f));
        buffer.Add(CreateSample(speed: 2f));

        Assert.That(buffer.Count, Is.EqualTo(expected: 2));
        Assert.That(buffer.GetChronological(index: 0).SpeedMetersPerSecond, Is.EqualTo(expected: 1f));
        Assert.That(buffer.GetChronological(index: 1).SpeedMetersPerSecond, Is.EqualTo(expected: 2f));
        Assert.That(buffer.Latest.SpeedMetersPerSecond, Is.EqualTo(expected: 2f));
    }

    [Test]
    public void Add_OverCapacity_KeepsNewestSamplesChronologically()
    {
        var buffer = new RunDiagnosticsOverlayBuffer(capacity: 3);

        buffer.Add(CreateSample(speed: 1f));
        buffer.Add(CreateSample(speed: 2f));
        buffer.Add(CreateSample(speed: 3f));
        buffer.Add(CreateSample(speed: 4f));

        Assert.That(buffer.Count, Is.EqualTo(expected: 3));
        Assert.That(buffer.GetChronological(index: 0).SpeedMetersPerSecond, Is.EqualTo(expected: 2f));
        Assert.That(buffer.GetChronological(index: 1).SpeedMetersPerSecond, Is.EqualTo(expected: 3f));
        Assert.That(buffer.GetChronological(index: 2).SpeedMetersPerSecond, Is.EqualTo(expected: 4f));
        Assert.That(buffer.Latest.SpeedMetersPerSecond, Is.EqualTo(expected: 4f));
    }

    [Test]
    public void Clear_WithBufferedSamples_EmptiesBuffer()
    {
        var buffer = new RunDiagnosticsOverlayBuffer(capacity: 2);
        buffer.Add(CreateSample(speed: 3f));

        buffer.Clear();

        Assert.That(buffer.Count, Is.Zero);
        Assert.That(() => buffer.GetChronological(index: 0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    private RunDiagnosticsOverlaySample CreateSample(float speed)
    {
        return new RunDiagnosticsOverlaySample(
            speed,
            speed + 1f,
            speed + 2f,
            speed + 3f,
            speed + 3f,
            speed + 4f,
            speed + 5f,
            speed + 6f,
            speed + 7f,
            speed + 8f,
            speed + 8f,
            speed + 9f,
            RunDiagnosticsOverlaySnapReason.None,
            fixedStepsThisFrame: 1,
            surfaceFrame: default,
            speedDiagnostics: default);
    }
}
