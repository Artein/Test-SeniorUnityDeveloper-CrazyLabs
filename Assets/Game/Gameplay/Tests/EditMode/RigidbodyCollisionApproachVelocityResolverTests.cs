using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class RigidbodyCollisionApproachVelocityResolverTests
{
    private RigidbodyCollisionApproachVelocityResolver _resolver;

    [SetUp]
    public void OnSetUp()
    {
        _resolver = new RigidbodyCollisionApproachVelocityResolver();
    }

    [Test]
    public void TryResolve_FiniteReportedVelocity_ReturnsReportedVelocity()
    {
        var reportedVelocity = new Vector3(x: 1f, y: 2f, z: 3f);

        var resolved = _resolver.TryResolve(
            reportedVelocity,
            Vector3.zero,
            Vector3.zero,
            bodyMass: 0f,
            otherBodyAttached: true,
            out var resolvedVelocity);

        Assert.That(resolved, Is.True);
        Assert.That(resolvedVelocity, Is.EqualTo(reportedVelocity));
    }

    [Test]
    public void TryResolve_ZeroReportedVelocityForStaticObstacle_ReconstructsPreSolveVelocity()
    {
        var postSolveVelocity = new Vector3(x: 3f, y: -0.2f, z: 0f);
        var impulse = new Vector3(x: 0f, y: 0f, z: -40f);

        var resolved = _resolver.TryResolve(
            Vector3.zero,
            postSolveVelocity,
            impulse,
            bodyMass: 2f,
            otherBodyAttached: false,
            out var resolvedVelocity);

        Assert.That(resolved, Is.True);
        Assert.That(resolvedVelocity, Is.EqualTo(new Vector3(x: 3f, y: -0.2f, z: 20f)));
    }

    [Test]
    public void TryResolve_ZeroReportedVelocityForAttachedBody_ReturnsNoFallback()
    {
        var resolved = _resolver.TryResolve(
            Vector3.zero,
            Vector3.zero,
            new Vector3(x: 0f, y: 0f, z: -40f),
            bodyMass: 1f,
            otherBodyAttached: true,
            out var resolvedVelocity);

        Assert.That(resolved, Is.False);
        Assert.That(resolvedVelocity, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void TryResolve_NonFiniteReportedVelocity_ReturnsNoFallback()
    {
        var resolved = _resolver.TryResolve(
            new Vector3(float.NaN, y: 0f, z: 0f),
            Vector3.zero,
            new Vector3(x: 0f, y: 0f, z: -40f),
            bodyMass: 1f,
            otherBodyAttached: false,
            out var resolvedVelocity);

        Assert.That(resolved, Is.False);
        Assert.That(resolvedVelocity, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void TryResolve_NonFinitePostSolveVelocity_ReturnsNoFallback()
    {
        var resolved = _resolver.TryResolve(
            Vector3.zero,
            new Vector3(x: 0f, float.PositiveInfinity, z: 0f),
            new Vector3(x: 0f, y: 0f, z: -40f),
            bodyMass: 1f,
            otherBodyAttached: false,
            out var resolvedVelocity);

        Assert.That(resolved, Is.False);
        Assert.That(resolvedVelocity, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void TryResolve_NonFiniteImpulse_ReturnsNoFallback()
    {
        var resolved = _resolver.TryResolve(
            Vector3.zero,
            Vector3.zero,
            new Vector3(x: 0f, y: 0f, float.NaN),
            bodyMass: 1f,
            otherBodyAttached: false,
            out var resolvedVelocity);

        Assert.That(resolved, Is.False);
        Assert.That(resolvedVelocity, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void TryResolve_InvalidMassReturnsNoFallback()
    {
        var resolved = _resolver.TryResolve(
            Vector3.zero,
            Vector3.zero,
            new Vector3(x: 0f, y: 0f, z: -40f),
            bodyMass: 0f,
            otherBodyAttached: false,
            out var resolvedVelocity);

        Assert.That(resolved, Is.False);
        Assert.That(resolvedVelocity, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void TryResolve_ZeroImpulse_ReturnsNoFallback()
    {
        var resolved = _resolver.TryResolve(
            Vector3.zero,
            new Vector3(x: 0f, y: 0f, z: 40f),
            Vector3.zero,
            bodyMass: 1f,
            otherBodyAttached: false,
            out var resolvedVelocity);

        Assert.That(resolved, Is.False);
        Assert.That(resolvedVelocity, Is.EqualTo(Vector3.zero));
    }
}
