using System;
using Game.Gameplay.Diagnostics;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
public sealed class RunDiagnosticsOverlayBufferTests
{
    [Test]
    public void Constructor_NonPositiveCapacity_Throws()
    {
        Assert.That(() => new RunDiagnosticsOverlayBuffer(0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Add_UnderCapacity_StoresSamplesChronologically()
    {
        var buffer = new RunDiagnosticsOverlayBuffer(3);

        buffer.Add(CreateSample(1f));
        buffer.Add(CreateSample(2f));

        Assert.That(buffer.Count, Is.EqualTo(2));
        Assert.That(buffer.GetChronological(0).SpeedMetersPerSecond, Is.EqualTo(1f));
        Assert.That(buffer.GetChronological(1).SpeedMetersPerSecond, Is.EqualTo(2f));
        Assert.That(buffer.Latest.SpeedMetersPerSecond, Is.EqualTo(2f));
    }

    [Test]
    public void Add_OverCapacity_KeepsNewestSamplesChronologically()
    {
        var buffer = new RunDiagnosticsOverlayBuffer(3);

        buffer.Add(CreateSample(1f));
        buffer.Add(CreateSample(2f));
        buffer.Add(CreateSample(3f));
        buffer.Add(CreateSample(4f));

        Assert.That(buffer.Count, Is.EqualTo(3));
        Assert.That(buffer.GetChronological(0).SpeedMetersPerSecond, Is.EqualTo(2f));
        Assert.That(buffer.GetChronological(1).SpeedMetersPerSecond, Is.EqualTo(3f));
        Assert.That(buffer.GetChronological(2).SpeedMetersPerSecond, Is.EqualTo(4f));
        Assert.That(buffer.Latest.SpeedMetersPerSecond, Is.EqualTo(4f));
    }

    [Test]
    public void Clear_WithBufferedSamples_EmptiesBuffer()
    {
        var buffer = new RunDiagnosticsOverlayBuffer(2);
        buffer.Add(CreateSample(3f));

        buffer.Clear();

        Assert.That(buffer.Count, Is.Zero);
        Assert.That(() => buffer.GetChronological(0), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    private RunDiagnosticsOverlaySample CreateSample(float speed)
    {
        return new RunDiagnosticsOverlaySample(
            speed,
            motionStepMetersPerSecond: speed + 1f,
            visualTargetStepMetersPerSecond: speed + 2f,
            visualTargetStepMeters: speed + 3f,
            rawGroundNormalDeltaDegrees: speed + 3f,
            steeringUpDeltaDegrees: speed + 4f,
            visualLagCentimeters: speed + 5f,
            cameraStepMetersPerSecond: speed + 6f,
            targetToMotionCentimeters: speed + 7f,
            visualTargetRotationDeltaDegrees: speed + 8f,
            visualRotationDeltaDegrees: speed + 8f,
            cameraRotationDeltaDegrees: speed + 9f,
            estimatedVisualSnapReason: RunDiagnosticsOverlaySnapReason.None,
            fixedStepsThisFrame: 1,
            isGrounded: true);
    }
}
