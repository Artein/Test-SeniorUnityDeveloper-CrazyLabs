using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunSteeringAffordanceLayoutTests
{
    [Test]
    public void Create_InactiveSnapshot_ReturnsHiddenState()
    {
        var layout = new RunSteeringAffordanceLayout();

        var snapshot = new RunSteeringAffordanceSnapshot(
            isActive: false,
            pointerId: 0,
            originScreenPosition: Vector2.zero,
            currentScreenPosition: Vector2.zero,
            capturedRangePixels: 0f,
            capturedDeadzoneFraction: 0f);

        var state = layout.Create(snapshot);

        Assert.That(state.IsVisible, Is.False);
        Assert.That(state.LeftRangeEndAlphaMultiplier, Is.Zero);
        Assert.That(state.RightRangeEndAlphaMultiplier, Is.Zero);
    }

    [Test]
    public void Create_ActiveSnapshot_PlacesKnobAtOriginAndRangeEndsAroundOrigin()
    {
        var layout = new RunSteeringAffordanceLayout();

        var snapshot = new RunSteeringAffordanceSnapshot(
            isActive: true,
            pointerId: 1,
            originScreenPosition: new Vector2(100f, 200f),
            currentScreenPosition: new Vector2(100f, 200f),
            capturedRangePixels: 80f,
            capturedDeadzoneFraction: 0.25f);

        var state = layout.Create(snapshot);

        Assert.That(state.IsVisible, Is.True);
        Assert.That(state.OriginScreenPosition, Is.EqualTo(new Vector2(100f, 200f)));
        Assert.That(state.KnobScreenPosition, Is.EqualTo(new Vector2(100f, 200f)));
        Assert.That(state.LeftRangeEndScreenPosition, Is.EqualTo(new Vector2(20f, 200f)));
        Assert.That(state.RightRangeEndScreenPosition, Is.EqualTo(new Vector2(180f, 200f)));
        Assert.That(state.DeadzoneDiameterPixels, Is.EqualTo(40f));
        Assert.That(state.LeftRangeEndAlphaMultiplier, Is.Zero);
        Assert.That(state.RightRangeEndAlphaMultiplier, Is.Zero);
    }

    [Test]
    public void Create_RightMovementInsideFadeRamp_FadesOnlyRightRangeEnd()
    {
        var layout = new RunSteeringAffordanceLayout();

        var snapshot = new RunSteeringAffordanceSnapshot(
            isActive: true,
            pointerId: 1,
            originScreenPosition: new Vector2(100f, 200f),
            currentScreenPosition: new Vector2(120f, 200f),
            capturedRangePixels: 80f,
            capturedDeadzoneFraction: 0.25f);

        var state = layout.Create(snapshot);

        Assert.That(state.LeftRangeEndAlphaMultiplier, Is.Zero);
        Assert.That(state.RightRangeEndAlphaMultiplier, Is.GreaterThan(0f).And.LessThan(1f));
    }

    [Test]
    public void Create_FullLeftMovement_ShowsOnlyLeftRangeEndAtFullOpacity()
    {
        var layout = new RunSteeringAffordanceLayout();

        var snapshot = new RunSteeringAffordanceSnapshot(
            isActive: true,
            pointerId: 1,
            originScreenPosition: new Vector2(100f, 200f),
            currentScreenPosition: new Vector2(20f, 200f),
            capturedRangePixels: 80f,
            capturedDeadzoneFraction: 0.25f);

        var state = layout.Create(snapshot);

        Assert.That(state.LeftRangeEndAlphaMultiplier, Is.EqualTo(1f));
        Assert.That(state.RightRangeEndAlphaMultiplier, Is.Zero);
    }

    [Test]
    public void Create_InvalidRange_HidesBothRangeEnds()
    {
        var layout = new RunSteeringAffordanceLayout();

        var snapshot = new RunSteeringAffordanceSnapshot(
            isActive: true,
            pointerId: 1,
            originScreenPosition: new Vector2(100f, 200f),
            currentScreenPosition: new Vector2(20f, 200f),
            capturedRangePixels: -80f,
            capturedDeadzoneFraction: 0.25f);

        var state = layout.Create(snapshot);

        Assert.That(state.LeftRangeEndAlphaMultiplier, Is.Zero);
        Assert.That(state.RightRangeEndAlphaMultiplier, Is.Zero);
    }

    [Test]
    public void Create_ActiveSnapshot_ClampsKnobHorizontallyAndIgnoresVerticalMovement()
    {
        var layout = new RunSteeringAffordanceLayout();

        var snapshot = new RunSteeringAffordanceSnapshot(
            isActive: true,
            pointerId: 1,
            originScreenPosition: new Vector2(100f, 200f),
            currentScreenPosition: new Vector2(260f, 900f),
            capturedRangePixels: 80f,
            capturedDeadzoneFraction: 0f);

        var state = layout.Create(snapshot);

        Assert.That(state.KnobScreenPosition, Is.EqualTo(new Vector2(180f, 200f)));
    }

    [Test]
    public void Create_ActiveSnapshot_PreservesEdgeOriginAndPhysicalRange()
    {
        var layout = new RunSteeringAffordanceLayout();

        var snapshot = new RunSteeringAffordanceSnapshot(
            isActive: true,
            pointerId: 1,
            originScreenPosition: new Vector2(0f, 60f),
            currentScreenPosition: new Vector2(-120f, -400f),
            capturedRangePixels: 100f,
            capturedDeadzoneFraction: 0.1f);

        var state = layout.Create(snapshot);

        Assert.That(state.OriginScreenPosition, Is.EqualTo(new Vector2(0f, 60f)));
        Assert.That(state.KnobScreenPosition, Is.EqualTo(new Vector2(-100f, 60f)));
        Assert.That(state.LeftRangeEndScreenPosition, Is.EqualTo(new Vector2(-100f, 60f)));
        Assert.That(state.RightRangeEndScreenPosition, Is.EqualTo(new Vector2(100f, 60f)));
        Assert.That(state.DeadzoneDiameterPixels, Is.EqualTo(20f));
    }
}
