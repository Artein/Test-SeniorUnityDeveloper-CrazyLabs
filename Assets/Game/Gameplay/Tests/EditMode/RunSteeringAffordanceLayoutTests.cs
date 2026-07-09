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
            false,
            0,
            Vector2.zero,
            Vector2.zero,
            0f,
            0f);

        var state = layout.Create(snapshot);

        Assert.That(state.IsVisible, Is.False);
    }

    [Test]
    public void Create_ActiveSnapshot_PlacesKnobAtOriginAndRangeEndsAroundOrigin()
    {
        var layout = new RunSteeringAffordanceLayout();
        var snapshot = new RunSteeringAffordanceSnapshot(
            true,
            1,
            new Vector2(100f, 200f),
            new Vector2(100f, 200f),
            80f,
            0.25f);

        var state = layout.Create(snapshot);

        Assert.That(state.IsVisible, Is.True);
        Assert.That(state.OriginScreenPosition, Is.EqualTo(new Vector2(100f, 200f)));
        Assert.That(state.KnobScreenPosition, Is.EqualTo(new Vector2(100f, 200f)));
        Assert.That(state.LeftRangeEndScreenPosition, Is.EqualTo(new Vector2(20f, 200f)));
        Assert.That(state.RightRangeEndScreenPosition, Is.EqualTo(new Vector2(180f, 200f)));
        Assert.That(state.DeadzoneDiameterPixels, Is.EqualTo(40f));
    }

    [Test]
    public void Create_ActiveSnapshot_ClampsKnobHorizontallyAndIgnoresVerticalMovement()
    {
        var layout = new RunSteeringAffordanceLayout();
        var snapshot = new RunSteeringAffordanceSnapshot(
            true,
            1,
            new Vector2(100f, 200f),
            new Vector2(260f, 900f),
            80f,
            0f);

        var state = layout.Create(snapshot);

        Assert.That(state.KnobScreenPosition, Is.EqualTo(new Vector2(180f, 200f)));
    }

    [Test]
    public void Create_ActiveSnapshot_PreservesEdgeOriginAndPhysicalRange()
    {
        var layout = new RunSteeringAffordanceLayout();
        var snapshot = new RunSteeringAffordanceSnapshot(
            true,
            1,
            new Vector2(0f, 60f),
            new Vector2(-120f, -400f),
            100f,
            0.1f);

        var state = layout.Create(snapshot);

        Assert.That(state.OriginScreenPosition, Is.EqualTo(new Vector2(0f, 60f)));
        Assert.That(state.KnobScreenPosition, Is.EqualTo(new Vector2(-100f, 60f)));
        Assert.That(state.LeftRangeEndScreenPosition, Is.EqualTo(new Vector2(-100f, 60f)));
        Assert.That(state.RightRangeEndScreenPosition, Is.EqualTo(new Vector2(100f, 60f)));
        Assert.That(state.DeadzoneDiameterPixels, Is.EqualTo(20f));
    }
}
