using System;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class SlingshotBandShapeProviderTests
{
    [Test]
    public void TryCreateBandShape_ValidQuery_WritesWorldShapeIntoCallerOwnedBuffer()
    {
        var config = new FakeSlingshotConfig
        {
            TouchTargetRadiusPixels = 30f,
            MinimumPullDistance = 0.25f,
            MaximumPullDistance = 2f,
            MaximumLateralPull = 1.25f,
            MaximumLaunchAngleDegrees = 35f,
            MinimumLaunchSpeed = 4f,
            MaximumLaunchSpeed = 12f,
            LaunchSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            LaunchUpSpeed = 1.5f,
            BandContactPadding = 0f,
            BandSilhouetteSampleCount = 8,
            BandWrapSampleCount = 5,
            BandRecoilDuration = 0.2f,
            BandRecoilCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
        };

        var source = new FakeLaunchTargetSilhouetteSource(
            new Vector3(-1f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(1f, 0f, -2f),
            new Vector3(-1f, 0f, -2f));
        var provider = new SlingshotBandShapeProvider(source, config);
        var output = new Vector3[provider.BandShapePointCount];

        var solved = provider.TryCreateBandShape(
            new SlingshotBandShapeQuery(
                new Vector3(-3f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                Vector3.zero,
                new Vector3(0f, 0f, -1f),
                Vector3.right,
                Vector3.forward,
                Vector3.up),
            output,
            out var pointCount);

        Assert.That(solved, Is.True);
        Assert.That(pointCount, Is.EqualTo(9));
        Assert.That(output[0], Is.EqualTo(new Vector3(-3f, 0f, 0f)));
        Assert.That(output[8], Is.EqualTo(new Vector3(3f, 0f, 0f)));
        AssertVector3(output[4], new Vector3(0f, 0f, -2f));
        Assert.That(source.Queries, Is.EqualTo(1));
    }

    [Test]
    public void TryCreateRenderedBandShape_RenderedBandRadius_InflatesContactPadding()
    {
        var config = CreateValidConfig();
        config.BandContactPadding = 0.05f;
        var source = new FakeLaunchTargetSilhouetteSource(CreateSquareSamples());
        ISlingshotRenderedBandShapeProvider provider = new SlingshotBandShapeProvider(source, config);
        var output = new Vector3[config.BandWrapSampleCount + 4];

        var solved = provider.TryCreateRenderedBandShape(CreateQuery(), 0.1f, output, out var pointCount);

        Assert.That(solved, Is.True);
        Assert.That(pointCount, Is.EqualTo(9));
        AssertVector3(output[4], new Vector3(0f, 0f, -2.15f));
    }

    [Test]
    public void TryCreateBandShape_SourceFailure_ReturnsFalseWithoutThrowing()
    {
        var config = CreateValidConfig();
        var source = new FakeLaunchTargetSilhouetteSource(Array.Empty<Vector3>()) { ShouldFail = true };
        var provider = new SlingshotBandShapeProvider(source, config);
        var output = new Vector3[provider.BandShapePointCount];

        var solved = provider.TryCreateBandShape(CreateQuery(), output, out var pointCount);

        Assert.That(solved, Is.False);
        Assert.That(pointCount, Is.Zero);
    }

    [Test]
    public void TryCreateBandShape_OutputBufferTooSmall_ThrowsArgumentException()
    {
        var provider = new SlingshotBandShapeProvider(new FakeLaunchTargetSilhouetteSource(CreateSquareSamples()), CreateValidConfig());

        Assert.That(
            () => provider.TryCreateBandShape(CreateQuery(), new Vector3[provider.BandShapePointCount - 1], out _),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void TryCreateBandShape_SkewedLaunchFrame_ThrowsArgumentException()
    {
        var provider = new SlingshotBandShapeProvider(new FakeLaunchTargetSilhouetteSource(CreateSquareSamples()), CreateValidConfig());
        var output = new Vector3[provider.BandShapePointCount];

        var query = new SlingshotBandShapeQuery(
            new Vector3(-3f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            Vector3.zero,
            new Vector3(0f, 0f, -1f),
            Vector3.right,
            CreateSkewedUnitForward(),
            Vector3.up);

        Assert.That(
            () => provider.TryCreateBandShape(query, output, out _),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void TryCheckBandShapeClearance_ValidClearShape_ReturnsTrueAndSamplesSource()
    {
        var source = new FakeLaunchTargetSilhouetteSource(CreateSquareSamples());
        var provider = new SlingshotBandShapeProvider(source, CreateValidConfig());

        var checkedClearance = provider.TryCheckBandShapeClearance(
            CreateQuery(),
            new[] { new Vector3(-3f, 0f, 1f), new Vector3(3f, 0f, 1f) },
            0.1f,
            out var isClear);

        Assert.That(checkedClearance, Is.True);
        Assert.That(isClear, Is.True);
        Assert.That(source.Queries, Is.EqualTo(1));
    }

    [Test]
    public void TryCheckBandShapeClearance_ShapeCrossesSilhouette_ReturnsBlocked()
    {
        var provider = new SlingshotBandShapeProvider(new FakeLaunchTargetSilhouetteSource(CreateSquareSamples()), CreateValidConfig());

        var checkedClearance = provider.TryCheckBandShapeClearance(
            CreateQuery(),
            new[] { new Vector3(-3f, 0f, -1f), new Vector3(3f, 0f, -1f) },
            0f,
            out var isClear);

        Assert.That(checkedClearance, Is.True);
        Assert.That(isClear, Is.False);
    }

    [Test]
    public void TryCheckBandShapeClearance_SourceFailure_ReturnsFalseWithoutThrowing()
    {
        var source = new FakeLaunchTargetSilhouetteSource(Array.Empty<Vector3>()) { ShouldFail = true };
        var provider = new SlingshotBandShapeProvider(source, CreateValidConfig());

        var checkedClearance = provider.TryCheckBandShapeClearance(
            CreateQuery(),
            new[] { new Vector3(-3f, 0f, 1f), new Vector3(3f, 0f, 1f) },
            0.1f,
            out var isClear);

        Assert.That(checkedClearance, Is.False);
        Assert.That(isClear, Is.False);
    }

    [Test]
    public void TryGetSilhouetteDepthSpan_ValidSamples_ReturnsPullPlaneDepthRange()
    {
        var source = new FakeLaunchTargetSilhouetteSource(
            new Vector3(-1f, 0f, 1f),
            new Vector3(1f, 0f, 1f),
            new Vector3(1f, 0f, -2f),
            new Vector3(-1f, 0f, -2f));
        ISlingshotBandShapeDepthProvider provider = new SlingshotBandShapeProvider(source, CreateValidConfig());

        var gotDepthSpan = provider.TryGetSilhouetteDepthSpan(CreateQuery(), out var minimumDepth, out var maximumDepth);

        Assert.That(gotDepthSpan, Is.True);
        Assert.That(minimumDepth, Is.EqualTo(-1f).Within(0.0001f));
        Assert.That(maximumDepth, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(source.Queries, Is.EqualTo(1));
    }

    [Test]
    public void TryGetSilhouetteOffsetSpan_ValidSamples_ReturnsPullPlaneOffsetRange()
    {
        var source = new FakeLaunchTargetSilhouetteSource(
            new Vector3(-1.2f, 0f, 1f),
            new Vector3(0.8f, 0f, 1f),
            new Vector3(0.8f, 0f, -2f),
            new Vector3(-1.2f, 0f, -2f));
        ISlingshotBandShapeOffsetProvider provider = new SlingshotBandShapeProvider(source, CreateValidConfig());

        var gotOffsetSpan = provider.TryGetSilhouetteOffsetSpan(CreateQuery(), out var minimumOffset, out var maximumOffset);

        Assert.That(gotOffsetSpan, Is.True);
        Assert.That(minimumOffset, Is.EqualTo(-1.2f).Within(0.0001f));
        Assert.That(maximumOffset, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(source.Queries, Is.EqualTo(1));
    }

    private FakeSlingshotConfig CreateValidConfig()
    {
        return new FakeSlingshotConfig
        {
            TouchTargetRadiusPixels = 30f,
            MinimumPullDistance = 0.25f,
            MaximumPullDistance = 2f,
            MaximumLateralPull = 1.25f,
            MaximumLaunchAngleDegrees = 35f,
            MinimumLaunchSpeed = 4f,
            MaximumLaunchSpeed = 12f,
            LaunchSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            LaunchUpSpeed = 1.5f,
            BandContactPadding = 0f,
            BandSilhouetteSampleCount = 8,
            BandWrapSampleCount = 5,
            BandRecoilDuration = 0.2f,
            BandRecoilCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
        };
    }

    private SlingshotBandShapeQuery CreateQuery()
    {
        return new SlingshotBandShapeQuery(
            new Vector3(-3f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            Vector3.zero,
            new Vector3(0f, 0f, -1f),
            Vector3.right,
            Vector3.forward,
            Vector3.up);
    }

    private Vector3[] CreateSquareSamples()
    {
        return new[]
        {
            new Vector3(-1f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(1f, 0f, -2f),
            new Vector3(-1f, 0f, -2f)
        };
    }

    private Vector3 CreateSkewedUnitForward()
    {
        return new Vector3(0.0002f, 0f, Mathf.Sqrt(1f - (0.0002f * 0.0002f)));
    }

    private void AssertVector3(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
    }

    private sealed class FakeLaunchTargetSilhouetteSource : ILaunchTargetSilhouetteSource
    {
        private readonly Vector3[] _samples;

        public int Queries { get; private set; }
        public bool ShouldFail { get; set; }

        public FakeLaunchTargetSilhouetteSource(params Vector3[] samples)
        {
            _samples = samples;
        }

        public bool TryWriteSilhouetteSamples(LaunchTargetSilhouetteQuery query, Vector3[] outputSamples, out int sampleCount)
        {
            Queries += 1;

            if (ShouldFail)
            {
                sampleCount = 0;
                return false;
            }

            if (outputSamples.Length < _samples.Length)
                throw new ArgumentException("Output buffer is too small.", nameof(outputSamples));

            for (var i = 0; i < _samples.Length; i += 1)
            {
                outputSamples[i] = _samples[i];
            }

            sampleCount = _samples.Length;
            return true;
        }
    }
}
