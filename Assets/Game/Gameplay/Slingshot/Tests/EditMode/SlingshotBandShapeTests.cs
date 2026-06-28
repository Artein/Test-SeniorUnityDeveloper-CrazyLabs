using System;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class SlingshotBandShapeTests
{
    [Test]
    public void Constructor_VariablePointPolyline_CopiesOrderedPoints()
    {
        var sourcePoints = new[]
        {
            new Vector3(-1f, 0f, 0f),
            new Vector3(-0.5f, 0f, -0.5f),
            new Vector3(0.5f, 0f, -0.5f),
            new Vector3(1f, 0f, 0f)
        };

        var shape = new SlingshotBandShape(sourcePoints);
        sourcePoints[1] = Vector3.zero;

        Assert.That(shape.Points.Count, Is.EqualTo(4));
        Assert.That(shape.Points[0], Is.EqualTo(new Vector3(-1f, 0f, 0f)));
        Assert.That(shape.Points[1], Is.EqualTo(new Vector3(-0.5f, 0f, -0.5f)));
        Assert.That(shape.Points[2], Is.EqualTo(new Vector3(0.5f, 0f, -0.5f)));
        Assert.That(shape.Points[3], Is.EqualTo(new Vector3(1f, 0f, 0f)));
    }

    [Test]
    public void Constructor_OnePoint_ThrowsArgumentException()
    {
        Assert.That(
            () => new SlingshotBandShape(Vector3.zero),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_NonFinitePoint_ThrowsArgumentException()
    {
        Assert.That(
            () => new SlingshotBandShape(Vector3.zero, new Vector3(float.NaN, 0f, 0f)),
            Throws.TypeOf<ArgumentException>());
    }
}
