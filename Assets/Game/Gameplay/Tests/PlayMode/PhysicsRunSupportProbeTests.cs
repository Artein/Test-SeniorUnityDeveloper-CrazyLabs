using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Tests.Common;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public sealed class PhysicsRunSupportProbeTests : BaseGameplayTestAssetsFixture
{
    private static readonly Vector3 TestOrigin = new(x: 1000f, y: 0f, z: 1000f);

    private readonly List<GameObject> _createdObjects = new();

    [TearDown]
    public void OnTearDown()
    {
        for (var i = 0; i < _createdObjects.Count; i += 1)
        {
            if (_createdObjects[i] != null)
                Object.DestroyImmediate(_createdObjects[i]);
        }

        _createdObjects.Clear();
    }

    [Test]
    public void given_SurfaceBelowSupportButOutsideProbeDistance_when_Sampled_then_ContextIsUngrounded()
    {
        var source = CreateSource(supportCenterY: 1.25f, supportProbeDistance: 0.08f);
        CreateSurface(topY: 0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    [Test]
    public void given_SurfaceWithinSupportProbeDistance_when_Sampled_then_ContextIsGrounded()
    {
        var source = CreateSource(supportCenterY: 1.1f, supportProbeDistance: 0.08f);
        CreateSurface(topY: 0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.GroundNormal, Is.EqualTo(Vector3.up));
        Assert.That(source.Current.ForwardDownhillDegrees, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
    }

    [Test]
    public void given_SurfaceExactlyTouchingSupport_when_Sampled_then_ContextIsGrounded()
    {
        var source = CreateSource(supportCenterY: 1.05f, supportProbeDistance: 0.08f);
        CreateSurface(topY: 0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.GroundNormal, Is.EqualTo(Vector3.up));
    }

    [Test]
    public void given_SurfaceSlightlyPenetratingSupport_when_Sampled_then_ContextIsGrounded()
    {
        var source = CreateSource(supportCenterY: 1.03f, supportProbeDistance: 0.08f);
        CreateSurface(topY: 0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.GroundNormal, Is.EqualTo(Vector3.up));
    }

    [Test]
    public void given_GroundedForwardDownhillSurface_when_Sampled_then_ContextUsesSupportSlope()
    {
        var source = CreateSource(supportCenterY: 1.1f, supportProbeDistance: 0.2f);
        CreateSurface(topY: 0.05f, Quaternion.AngleAxis(angle: 12f, Vector3.right));
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(source.Current.ForwardDownhillDegrees, Is.EqualTo(expected: 12f).Within(amount: 0.75f));
    }

    [Test]
    public void given_ColliderWithoutRunContactWithinProbeDistance_when_Sampled_then_ContextIsUngrounded()
    {
        var source = CreateSource(supportCenterY: 1.1f, supportProbeDistance: 0.08f);
        CreateSurface(topY: 0.05f, Quaternion.identity, RunContactCategory.Surface, addRunContact: false);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    [TestCase(RunContactCategory.Obstacle)]
    [TestCase(RunContactCategory.SafetyNet)]
    [TestCase(RunContactCategory.Finish)]
    public void given_NonSurfaceRunContactWithinProbeDistance_when_Sampled_then_ContextIsUngrounded(RunContactCategory category)
    {
        var source = CreateSource(supportCenterY: 1.1f, supportProbeDistance: 0.08f);
        CreateSurface(topY: 0.05f, Quaternion.identity, category);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    [Test]
    public void given_CloserSteepSurfaceAndFartherFlatSurface_when_Sampled_then_ContextUsesMostSupportiveSurfaceNormal()
    {
        var source = CreateSource(supportCenterY: 1.35f, supportProbeDistance: 0.35f);
        CreateSurface(topY: 0.22f, Quaternion.AngleAxis(angle: 55f, Vector3.right));
        CreateSurface(topY: 0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        Assert.That(Vector3.Dot(source.Current.GroundNormal, Vector3.up), Is.GreaterThan(expected: 0.98f));
        Assert.That(source.Current.ForwardDownhillDegrees, Is.EqualTo(expected: 0f).Within(amount: 0.75f));
    }

    [Test]
    public void given_SupportFootprintStraddlesFlatAndBankedSurface_when_SampledRepeatedly_then_ContextKeepsDominantSupportNormal()
    {
        var bankRotation = Quaternion.AngleAxis(angle: 35f, Vector3.forward);
        var bankNormal = bankRotation * Vector3.up;
        var source = CreateSphereSource(new Vector3(x: 0f, y: 0.62f, z: 0f), supportRadius: 0.4f, supportProbeDistance: 0.4f, out _);

        CreateSurfacePatch(
            name: "Bank Surface",
            Vector3.zero,
            new Vector3(x: 3f, y: 0.1f, z: 3f),
            topY: 0.05f,
            bankRotation);

        CreateSurfacePatch(
            name: "Flat Center Patch",
            Vector3.zero,
            new Vector3(x: 0.16f, y: 0.1f, z: 0.16f),
            topY: 0.08f,
            Quaternion.identity);

        Physics.SyncTransforms();

        source.SampleForTests();
        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        AssertGroundNormalNear(source.Current.GroundNormal, bankNormal, toleranceDegrees: 2f);
    }

    [Test]
    public void given_BankedSurfaceHasMajorityFootprintSupport_when_Sampled_then_ContextUsesBankNormal()
    {
        var bankRotation = Quaternion.AngleAxis(angle: 35f, Vector3.forward);
        var bankNormal = bankRotation * Vector3.up;
        var source = CreateSphereSource(new Vector3(x: 0f, y: 0.62f, z: 0f), supportRadius: 0.4f, supportProbeDistance: 0.4f, out _);

        CreateSurfacePatch(
            name: "Flat Center Patch",
            Vector3.zero,
            new Vector3(x: 0.16f, y: 0.1f, z: 0.16f),
            topY: 0.08f,
            Quaternion.identity);

        CreateSurfacePatch(
            name: "Bank Surface",
            Vector3.zero,
            new Vector3(x: 3f, y: 0.1f, z: 3f),
            topY: 0.05f,
            bankRotation);

        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        AssertGroundNormalNear(source.Current.GroundNormal, bankNormal, toleranceDegrees: 2f);
    }

    [Test]
    public void given_FootprintSamplesHitNonSurfaceContacts_when_Sampled_then_IgnoresThem()
    {
        var obstacleRotation = Quaternion.AngleAxis(angle: 35f, Vector3.forward);
        var source = CreateSphereSource(new Vector3(x: 0f, y: 0.62f, z: 0f), supportRadius: 0.4f, supportProbeDistance: 0.4f, out _);

        CreateSurfacePatch(
            name: "Obstacle Surface",
            Vector3.zero,
            new Vector3(x: 3f, y: 0.1f, z: 3f),
            topY: 0.05f,
            obstacleRotation,
            RunContactCategory.Obstacle);

        CreateSurfacePatch(
            name: "Flat Center Patch",
            Vector3.zero,
            new Vector3(x: 0.16f, y: 0.1f, z: 0.16f),
            topY: 0.08f,
            Quaternion.identity);

        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.True);
        AssertGroundNormalNear(source.Current.GroundNormal, Vector3.up, toleranceDegrees: 0.1f);
    }

    [Test]
    public void given_CurvedFootprintSideRayHitsLedgeBeyondShapeClearance_when_Sampled_then_ContextIsUngrounded()
    {
        var source = CreateSphereSource(new Vector3(x: 0f, y: 0.5f, z: 0f), supportRadius: 0.4f, supportProbeDistance: 0.04f, out _);

        CreateSurfacePatch(
            name: "Right Side Ledge",
            Vector3.right * 0.24f,
            new Vector3(x: 0.08f, y: 0.1f, z: 0.08f),
            topY: 0.07f,
            Quaternion.identity);

        Physics.SyncTransforms();

        source.SampleForTests();

        Assert.That(source.Current.IsGrounded, Is.False);
    }

    [Test]
    public void given_MissingSupportCollider_when_Constructed_then_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new PhysicsRunSupportProbe(
                supportCollider: null,
                new RunSupportColliderProbeFactory(),
                CreateProbeConfig(supportProbeDistance: 0.08f),
                new RunSurfaceSlopeCalculator()),
            Throws.ArgumentNullException.With.Property(name: "ParamName").EqualTo(expected: "supportCollider"));
    }

    [Test]
    public void given_MissingRunSupportColliderProbeFactory_when_Constructed_then_ThrowsArgumentNullException()
    {
        var sourceObject = CreateGameObject(name: "Run Surface Context Source");
        var supportCollider = CreateSupportCollider(sourceObject.transform, supportCenterY: 1.1f);

        Assert.That(
            () => new PhysicsRunSupportProbe(
                supportCollider,
                runSupportColliderProbeFactory: null,
                CreateProbeConfig(supportProbeDistance: 0.08f),
                new RunSurfaceSlopeCalculator()),
            Throws.ArgumentNullException.With.Property(name: "ParamName").EqualTo(expected: "runSupportColliderProbeFactory"));
    }

    [Test]
    public void given_ValidFrameAndSurface_when_Observed_then_ProbeReturnsSameTickSupportedObservation()
    {
        var sourceObject = CreateGameObject(name: "Run Support Probe");
        sourceObject.transform.position = TestOrigin;
        var supportCollider = CreateSupportCollider(sourceObject.transform, supportCenterY: 1.1f);
        CreateSurface(topY: 0.05f, Quaternion.identity);
        Physics.SyncTransforms();

        var probe = new PhysicsRunSupportProbe(
            supportCollider,
            new RunSupportColliderProbeFactory(),
            new RunSurfaceProbeConfig(
                distance: 0.08f,
                skinWidth: 0.02f,
                TestAssets.RunSurfaceLayerMask,
                minimumSupportNormalDot: 0.17f,
                footprintSampleOffsetScale: 0.6f,
                footprintNormalClusterAngleDegrees: 8f),
            new RunSurfaceSlopeCalculator());

        Assert.That(
            RunProgressFrameSnapshot.TryCreate(
                TestOrigin,
                Vector3.forward,
                Vector3.up,
                out var frame,
                out _),
            Is.True);

        var observation = probe.Observe(frame, hasContinuityNormal: false, Vector3.up);

        Assert.That(observation.State, Is.EqualTo(RunSupportObservationState.Supported));
        Assert.That(observation.SurfaceContext.IsGrounded, Is.True);
        Assert.That(observation.SurfaceContext.GroundNormal, Is.EqualTo(Vector3.up));
    }

    private ProbeHarness CreateSource(float supportCenterY, float supportProbeDistance)
    {
        var sourceObject = CreateGameObject(name: "Run Surface Context Source");
        sourceObject.transform.position = TestOrigin;
        var runProgressFrameSource = sourceObject.AddComponent<RunProgressFrameSource>();

        var supportCollider = CreateSupportCollider(sourceObject.transform, supportCenterY);

        return new ProbeHarness(
            new PhysicsRunSupportProbe(
                supportCollider,
                new RunSupportColliderProbeFactory(),
                CreateProbeConfig(supportProbeDistance),
                new RunSurfaceSlopeCalculator()),
            runProgressFrameSource);
    }

    private ProbeHarness CreateSphereSource(
        Vector3 supportCenterOffset,
        float supportRadius,
        float supportProbeDistance,
        out FakeRunProgressFrameSource frameSource)
    {
        var sourceObject = CreateGameObject(name: "Run Surface Context Source");
        sourceObject.transform.position = TestOrigin;
        frameSource = new FakeRunProgressFrameSource(sourceObject.transform.position);

        var supportObject = CreateGameObject(name: "Support Collider");
        supportObject.transform.SetParent(sourceObject.transform, worldPositionStays: false);
        supportObject.transform.localPosition = supportCenterOffset;

        var supportCollider = supportObject.AddComponent<SphereCollider>();
        supportCollider.radius = supportRadius;

        return new ProbeHarness(
            new PhysicsRunSupportProbe(
                supportCollider,
                new RunSupportColliderProbeFactory(),
                CreateProbeConfig(supportProbeDistance),
                new RunSurfaceSlopeCalculator()),
            frameSource);
    }

    private RunSurfaceProbeConfig CreateProbeConfig(float supportProbeDistance)
    {
        return new RunSurfaceProbeConfig(
            supportProbeDistance,
            skinWidth: 0.02f,
            TestAssets.RunSurfaceLayerMask,
            minimumSupportNormalDot: 0.17f,
            footprintSampleOffsetScale: 0.6f,
            footprintNormalClusterAngleDegrees: 8f);
    }

    private Collider CreateSupportCollider(Transform parent, float supportCenterY)
    {
        var supportObject = CreateGameObject(name: "Support Collider");
        supportObject.transform.SetParent(parent, worldPositionStays: false);
        supportObject.transform.localPosition = new Vector3(x: 0f, supportCenterY, z: 0f);

        var supportCollider = supportObject.AddComponent<CapsuleCollider>();
        supportCollider.direction = 1;
        supportCollider.radius = 0.15f;
        supportCollider.height = 2f;
        return supportCollider;
    }

    private GameObject CreateSurfacePatch(
        string name,
        Vector3 centerOffset,
        Vector3 size,
        float topY,
        Quaternion rotation,
        RunContactCategory category = RunContactCategory.Surface)
    {
        var surface = CreateGameObject(name);
        surface.layer = GetSingleLayer(TestAssets.RunSurfaceLayerMask, description: "Run Surface");

        surface.transform.SetPositionAndRotation(
            TestOrigin + centerOffset + Vector3.up * topY - rotation * Vector3.up * (size.y * 0.5f),
            rotation);

        surface.transform.localScale = size;
        surface.AddComponent<BoxCollider>();
        surface.AddComponent<RunContact>().SetCategoryForTests(category);
        return surface;
    }

    private GameObject CreateSurface(
        float topY,
        Quaternion rotation,
        RunContactCategory category = RunContactCategory.Surface,
        bool addRunContact = true)
    {
        var surface = CreateGameObject(name: "Surface");
        surface.layer = GetSingleLayer(TestAssets.RunSurfaceLayerMask, description: "Run Surface");

        surface.transform.SetPositionAndRotation(
            TestOrigin + new Vector3(x: 0f, topY, z: 0f) - rotation * Vector3.up * 0.05f,
            rotation);

        surface.transform.localScale = new Vector3(x: 8f, y: 0.1f, z: 8f);
        surface.AddComponent<BoxCollider>();

        if (addRunContact)
            surface.AddComponent<RunContact>().SetCategoryForTests(category);

        return surface;
    }

    private int GetSingleLayer(LayerMask layerMask, string description)
    {
        Assert.That(layerMask.value, Is.Not.EqualTo(expected: 0), description);
        Assert.That(layerMask.value & (layerMask.value - 1), Is.Zero, description);
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, p: 2f));
    }

    private static void AssertGroundNormalNear(Vector3 actual, Vector3 expected, float toleranceDegrees)
    {
        Assert.That(Vector3.Angle(actual, expected), Is.LessThanOrEqualTo(toleranceDegrees));
    }

    private GameObject CreateGameObject(string name)
    {
        var gameObject = new GameObject(name);
        _createdObjects.Add(gameObject);
        return gameObject;
    }

    private sealed class ProbeHarness
    {
        private readonly PhysicsRunSupportProbe _probe;
        private readonly IRunProgressFrameSource _progressFrameSource;

        public RunSurfaceContext Current { get; private set; } = new(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f);

        public ProbeHarness(
            PhysicsRunSupportProbe probe,
            IRunProgressFrameSource progressFrameSource)
        {
            _probe = probe;
            _progressFrameSource = progressFrameSource;
        }

        public void SampleForTests()
        {
            if (!_progressFrameSource.TryCreateSnapshot(_probe.SampleOrigin, out var frame, out _))
            {
                Current = new RunSurfaceContext(isGrounded: false, Vector3.up, forwardDownhillDegrees: 0f);
                return;
            }

            var observation = _probe.Observe(
                frame,
                Current.IsGrounded,
                Current.HasValidGroundNormal ? Current.GroundNormal : Vector3.up);

            Current = observation.SurfaceContext;
        }
    }

    private sealed class FakeRunProgressFrameSource : IRunProgressFrameSource
    {
        private readonly Vector3 _origin;

        public Vector3 UpDirection { get; } = Vector3.up;

        public FakeRunProgressFrameSource(Vector3 origin)
        {
            _origin = origin;
        }

        public bool TryCreateSnapshot(Vector3 origin, out RunProgressFrameSnapshot snapshot, out string error)
        {
            return RunProgressFrameSnapshot.TryCreate(
                _origin,
                Vector3.forward,
                UpDirection,
                out snapshot,
                out error);
        }
    }
}
