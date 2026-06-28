using System;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class SlingshotGeometrySnapshotTests
{
    [Test]
    public void SlingshotGeometrySnapshot_SkewedLaunchFrame_ThrowsArgumentException()
    {
        var skewedForward = new Vector3(0.0002f, 0f, Mathf.Sqrt(1f - (0.0002f * 0.0002f)));

        Assert.That(
            () => new SlingshotGeometrySnapshot(
                new Vector3(-1f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                Vector3.zero,
                Vector3.right,
                skewedForward,
                Vector3.up),
            Throws.TypeOf<ArgumentException>());
    }
}
