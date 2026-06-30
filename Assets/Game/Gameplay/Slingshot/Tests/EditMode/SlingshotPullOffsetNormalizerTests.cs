using Game.Gameplay.Slingshot;
using Game.Gameplay.Slingshot.Tests.EditMode;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class SlingshotPullOffsetNormalizerTests
{
    private FakeSlingshotConfig _config;
    private SlingshotGeometrySnapshot _geometry;
    private SlingshotPullOffsetNormalizer _normalizer;

    [SetUp]
    public void OnSetUp()
    {
        _config = new FakeSlingshotConfig
        {
            MinimumPullDistance = 0.25f,
            MaximumPullDistance = 2f,
            MaximumLateralPull = 1.25f,
            BandContactPadding = 0.05f
        };

        _geometry = CreateGeometry(leftAnchorOffset: -1f, rightAnchorOffset: 1f);
        _normalizer = new SlingshotPullOffsetNormalizer(_config);
    }

    [Test]
    public void Normalize_CenteredPull_ReturnsZero()
    {
        var normalized = _normalizer.Normalize(_geometry, FullLateralPullDistance(), 0f);

        Assert.That(normalized, Is.Zero);
    }

    [Test]
    public void Normalize_FullEffectiveLeftPull_ReturnsMinusOne()
    {
        var normalized = _normalizer.Normalize(_geometry, FullLateralPullDistance(), -1f);

        Assert.That(normalized, Is.EqualTo(-1f).Within(0.0001f));
    }

    [Test]
    public void Normalize_FullEffectiveRightPull_ReturnsOne()
    {
        var normalized = _normalizer.Normalize(_geometry, FullLateralPullDistance(), 1f);

        Assert.That(normalized, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void Normalize_AsymmetricAnchorLimits_UsesSideSpecificRange()
    {
        _geometry = CreateGeometry(-0.5f, 2f);

        var left = _normalizer.Normalize(_geometry, FullLateralPullDistance(), -0.25f);
        var right = _normalizer.Normalize(_geometry, FullLateralPullDistance(), 0.625f);

        Assert.That(left, Is.EqualTo(-0.5f).Within(0.0001f));
        Assert.That(right, Is.EqualTo(0.5f).Within(0.0001f));
    }

    [Test]
    public void Normalize_ShallowPullDepth_UsesLateralRampLimit()
    {
        var halfRampDistance = FullLateralPullDistance() * 0.5f;

        var normalized = _normalizer.Normalize(_geometry, halfRampDistance, 0.5f);

        Assert.That(normalized, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void Normalize_OffsetBeyondEffectiveRange_ClampsToSignedRange()
    {
        var normalized = _normalizer.Normalize(_geometry, FullLateralPullDistance(), 5f);

        Assert.That(normalized, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void Normalize_ZeroEffectiveRange_ReturnsZero()
    {
        _config.MaximumLateralPull = 0f;
        _geometry = CreateGeometry(0f, 0f);
        _normalizer = new SlingshotPullOffsetNormalizer(_config);

        var normalized = _normalizer.Normalize(_geometry, FullLateralPullDistance(), 1f);

        Assert.That(normalized, Is.Zero);
    }

    private float FullLateralPullDistance()
    {
        return Mathf.Max(0.02f, _config.MinimumPullDistance + (_config.BandContactPadding * 2f))
               + (_config.BandContactPadding * 2f);
    }

    private SlingshotGeometrySnapshot CreateGeometry(float leftAnchorOffset, float rightAnchorOffset)
    {
        return new SlingshotGeometrySnapshot(
            leftAnchorPosition: new Vector3(leftAnchorOffset, 0f, 0f),
            rightAnchorPosition: new Vector3(rightAnchorOffset, 0f, 0f),
            restPoint: Vector3.zero,
            launchFrameRight: Vector3.right,
            launchFrameForward: Vector3.forward,
            launchFrameUp: Vector3.up);
    }
}
