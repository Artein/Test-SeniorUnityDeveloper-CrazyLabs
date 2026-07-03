using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RunBodyVelocitySanityGuardTests
{
    private RunBodyVelocitySanityGuard _guard;

    [SetUp]
    public void OnSetUp()
    {
        _guard = new RunBodyVelocitySanityGuard();
    }

    [Test]
    public void Sanitize_NormalVelocity_ReturnsUnchanged()
    {
        var velocity = new Vector3(3f, 4f, 12f);

        var result = _guard.Sanitize(velocity, 250f);

        Assert.That(result.WasCorrected, Is.False);
        AssertVectorEqual(result.Velocity, velocity);
    }

    [Test]
    public void Sanitize_MaximumAuthoredLaunchWithUpgrade_ReturnsUnchanged()
    {
        var velocity = new Vector3(0f, 12f, 140f);

        var result = _guard.Sanitize(velocity, 250f);

        Assert.That(result.WasCorrected, Is.False);
        AssertVectorEqual(result.Velocity, velocity);
    }

    [Test]
    public void Sanitize_NonFiniteVelocity_ReturnsZero()
    {
        var result = _guard.Sanitize(new Vector3(float.NaN, 0f, 10f), 250f);

        Assert.That(result.WasCorrected, Is.True);
        AssertVectorEqual(result.Velocity, Vector3.zero);
    }

    [Test]
    public void Sanitize_AbsurdFiniteVelocity_ClampsMagnitude()
    {
        var result = _guard.Sanitize(new Vector3(0f, 300f, 400f), 250f);

        Assert.That(result.WasCorrected, Is.True);
        Assert.That(result.Velocity.magnitude, Is.EqualTo(250f).Within(0.0001f));
        Assert.That(Vector3.Dot(result.Velocity.normalized, new Vector3(0f, 300f, 400f).normalized), Is.GreaterThan(0.9999f));
    }

    [Test]
    public void Sanitize_InvalidMaximumSpeed_UsesDefensiveDefault()
    {
        var result = _guard.Sanitize(Vector3.forward * 300f, float.NaN);

        Assert.That(result.WasCorrected, Is.True);
        Assert.That(result.Velocity.magnitude, Is.EqualTo(250f).Within(0.0001f));
    }

    private void AssertVectorEqual(Vector3 actual, Vector3 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
        Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
    }
}
