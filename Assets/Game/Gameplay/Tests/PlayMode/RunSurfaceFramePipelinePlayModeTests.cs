using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.Diagnostics;
using Game.Gameplay.Tests.Common;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceFramePipelinePlayModeTests : BaseGameplayTestAssetsFixture
{
    private static readonly Vector3 TestOrigin = new(1100f, 0f, 1100f);

    private readonly List<GameObject> _createdObjects = new();

    [TearDown]
    public void OnTearDown()
    {
        for (var index = 0; index < _createdObjects.Count; index += 1)
        {
            if (_createdObjects[index] != null)
                Object.DestroyImmediate(_createdObjects[index]);
        }

        _createdObjects.Clear();
    }

    [TestCase(0.01f, 6, 4)]
    [TestCase(0.02f, 3, 2)]
    public void given_CanonicalGroundSeamAndGap_when_PipelineTicks_then_AtomicTraceUsesSeconds(
        float fixedDeltaTime,
        int supportLossTick,
        int discontinuityTick)
    {
        var fixture = CreateFixture(fixedDeltaTime);
        fixture.ShowFlat();

        var acquired = fixture.Tick("flat acquire");
        AssertSupported(acquired, RunSurfaceTransition.SupportAcquired, Vector3.up);
        Assert.That(acquired.IsCoreLocomotionGrounded, Is.True, acquired.FailureContext);
        Assert.That(acquired.IsPresentationGrounded, Is.True, acquired.FailureContext);
        Assert.That(acquired.DiagnosticsText, Does.Contain("observed:Supported normal:"), acquired.FailureContext);
        Assert.That(acquired.DiagnosticsText, Does.Contain("stable:grounded normal:"), acquired.FailureContext);
        Assert.That(acquired.DiagnosticsText, Does.Contain("transition:SupportAcquired"), acquired.FailureContext);
        Assert.That(acquired.DiagnosticsText, Does.Contain("steering:valid up:"), acquired.FailureContext);

        fixture.ShowNone();
        var briefGap = fixture.Tick("brief gap");
        Assert.That(briefGap.Snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Missing), briefGap.FailureContext);
        Assert.That(briefGap.Snapshot.IsMissingSupportHeld, Is.True, briefGap.FailureContext);
        Assert.That(briefGap.IsCoreLocomotionGrounded, Is.True, briefGap.FailureContext);
        Assert.That(briefGap.IsPresentationGrounded, Is.False, briefGap.FailureContext);
        Assert.That(briefGap.Snapshot.SteeringFrame.IsValid, Is.True, briefGap.FailureContext);
        Assert.That(briefGap.DiagnosticsText, Does.Contain("observed:Missing normal:n/a"), briefGap.FailureContext);
        Assert.That(briefGap.DiagnosticsText, Does.Contain("stable:grounded normal:"), briefGap.FailureContext);
        Assert.That(briefGap.DiagnosticsText, Does.Contain("held:yes"), briefGap.FailureContext);

        fixture.ShowFlat();
        var recoveredGap = fixture.Tick("brief gap recovery");
        AssertSupported(recoveredGap, RunSurfaceTransition.None, Vector3.up);

        fixture.ShowNone();

        for (var missTick = 1; missTick < supportLossTick; missTick += 1)
        {
            var held = fixture.Tick($"sustained gap miss {missTick}");
            Assert.That(held.Snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Missing), held.FailureContext);
            Assert.That(held.Snapshot.StableSupport.IsGrounded, Is.True, held.FailureContext);
            Assert.That(held.Snapshot.IsMissingSupportHeld, Is.True, held.FailureContext);
            Assert.That(held.Snapshot.Transition, Is.EqualTo(RunSurfaceTransition.None), held.FailureContext);
        }

        var lost = fixture.Tick($"sustained gap miss {supportLossTick}");
        Assert.That(lost.Snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Missing), lost.FailureContext);
        Assert.That(lost.Snapshot.StableSupport.IsGrounded, Is.False, lost.FailureContext);
        Assert.That(lost.Snapshot.Transition, Is.EqualTo(RunSurfaceTransition.SupportLost), lost.FailureContext);
        Assert.That(lost.IsCoreLocomotionGrounded, Is.False, lost.FailureContext);
        Assert.That(lost.DiagnosticsText, Does.Contain("stable:unsupported normal:n/a"), lost.FailureContext);
        Assert.That(lost.DiagnosticsText, Does.Contain("transition:SupportLost"), lost.FailureContext);

        fixture.ShowFlat();
        var reacquired = fixture.Tick("gap reacquire");
        AssertSupported(reacquired, RunSurfaceTransition.SupportAcquired, Vector3.up);

        fixture.ShowBank();

        for (var seamTick = 1; seamTick < discontinuityTick; seamTick += 1)
        {
            var confirming = fixture.Tick($"coherent seam candidate {seamTick}");

            Assert.That(confirming.Snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Supported),
                confirming.FailureContext);
            Assert.That(confirming.Snapshot.IsConfirmingDiscontinuity, Is.True, confirming.FailureContext);
            Assert.That(confirming.Snapshot.StableSupport.GroundNormal, Is.EqualTo(Vector3.up), confirming.FailureContext);
            Assert.That(confirming.Snapshot.Transition, Is.EqualTo(RunSurfaceTransition.None), confirming.FailureContext);
            Assert.That(confirming.DiagnosticsText, Does.Contain("confirming:yes"), confirming.FailureContext);
        }

        var confirmed = fixture.Tick($"coherent seam candidate {discontinuityTick}");
        AssertSupported(confirmed, RunSurfaceTransition.ConfirmedDiscontinuity, fixture.BankNormal, 2f);
        Assert.That(confirmed.Snapshot.IsConfirmingDiscontinuity, Is.False, confirmed.FailureContext);
        Assert.That(confirmed.DiagnosticsText, Does.Contain("transition:ConfirmedDiscontinuity"), confirmed.FailureContext);
        Assert.That(confirmed.DiagnosticsText, Does.Contain("confirming:no"), confirmed.FailureContext);

        Assert.That(Vector3.Angle(confirmed.Snapshot.SteeringFrame.UpDirection, fixture.BankNormal), Is.LessThanOrEqualTo(2f),
            confirmed.FailureContext);
    }

    [TestCase(0.01f, 4)]
    [TestCase(0.02f, 2)]
    public void given_AlternatingTroughNormals_when_PipelineTicks_then_OnlyCoherentNormalsConfirm(
        float fixedDeltaTime,
        int discontinuityTick)
    {
        var fixture = CreateFixture(fixedDeltaTime);
        fixture.ShowFlat();
        AssertSupported(fixture.Tick("flat acquire"), RunSurfaceTransition.SupportAcquired, Vector3.up);

        for (var alternatingTick = 1; alternatingTick <= 6; alternatingTick += 1)
        {
            if (alternatingTick % 2 == 1)
                fixture.ShowBank();
            else
                fixture.ShowOppositeBank();

            var sample = fixture.Tick($"alternating trough candidate {alternatingTick}");

            Assert.That(sample.Snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Supported),
                sample.FailureContext);
            Assert.That(sample.Snapshot.IsConfirmingDiscontinuity, Is.True, sample.FailureContext);
            Assert.That(sample.Snapshot.StableSupport.GroundNormal, Is.EqualTo(Vector3.up), sample.FailureContext);
            Assert.That(sample.Snapshot.Transition, Is.EqualTo(RunSurfaceTransition.None), sample.FailureContext);
        }

        fixture.ShowBank();

        for (var coherentTick = 1; coherentTick < discontinuityTick; coherentTick += 1)
        {
            var sample = fixture.Tick($"coherent trough exit {coherentTick}");
            Assert.That(sample.Snapshot.IsConfirmingDiscontinuity, Is.True, sample.FailureContext);
            Assert.That(sample.Snapshot.Transition, Is.EqualTo(RunSurfaceTransition.None), sample.FailureContext);
        }

        var confirmed = fixture.Tick($"coherent trough exit {discontinuityTick}");
        AssertSupported(confirmed, RunSurfaceTransition.ConfirmedDiscontinuity, fixture.BankNormal, 2f);
    }

    [Test]
    public void given_ContinuousSlopeAndUnavailableFrame_when_PipelineTicks_then_UpdatesAndHardResetsExactlyOnce()
    {
        var fixture = CreateFixture(0.02f);
        fixture.ShowFlat();
        AssertSupported(fixture.Tick("flat acquire"), RunSurfaceTransition.SupportAcquired, Vector3.up);

        fixture.ShowGentleSlope();
        var continuous = fixture.Tick("continuous slope");
        AssertSupported(continuous, RunSurfaceTransition.ContinuousUpdate, fixture.GentleSlopeNormal, 2f);
        Assert.That(continuous.Snapshot.SteeringFrame.IsValid, Is.True, continuous.FailureContext);

        var steeringUp = continuous.Snapshot.SteeringFrame.UpDirection;

        Assert.That(Vector3.Angle(Vector3.up, steeringUp), Is.GreaterThan(0f).And.LessThanOrEqualTo(3.7f),
            continuous.FailureContext);

        Assert.That(Vector3.Angle(steeringUp, fixture.GentleSlopeNormal), Is.LessThan(20f),
            continuous.FailureContext);

        fixture.IsProgressFrameAvailable = false;
        var hardReset = fixture.Tick("unavailable frame");

        Assert.That(hardReset.Snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Unavailable),
            hardReset.FailureContext);
        Assert.That(hardReset.Snapshot.Transition, Is.EqualTo(RunSurfaceTransition.HardReset), hardReset.FailureContext);
        Assert.That(hardReset.Snapshot.StableSupport.IsGrounded, Is.False, hardReset.FailureContext);
        Assert.That(hardReset.Snapshot.SteeringFrame.IsValid, Is.False, hardReset.FailureContext);
        Assert.That(hardReset.DiagnosticsText, Does.Contain("observed:Unavailable normal:n/a"), hardReset.FailureContext);
        Assert.That(hardReset.DiagnosticsText, Does.Contain("transition:HardReset"), hardReset.FailureContext);
        Assert.That(hardReset.DiagnosticsText, Does.Contain("steering:unavailable up:n/a"), hardReset.FailureContext);

        var repeatedUnavailable = fixture.Tick("repeated unavailable frame");

        Assert.That(repeatedUnavailable.Snapshot.Transition, Is.EqualTo(RunSurfaceTransition.HardReset),
            repeatedUnavailable.FailureContext);
        Assert.That(repeatedUnavailable.Snapshot.SteeringFrame.IsValid, Is.False, repeatedUnavailable.FailureContext);
    }

    private SurfaceFixture CreateFixture(float fixedDeltaTime)
    {
        var body = CreateGameObject("Run Surface Trace Body");
        body.transform.position = TestOrigin + (Vector3.up * 0.4f);
        var supportCollider = body.AddComponent<SphereCollider>();
        supportCollider.radius = 0.05f;

        var flat = CreateSurface("Flat", Quaternion.identity);
        var gentleSlope = CreateSurface("Gentle Slope", Quaternion.AngleAxis(20f, Vector3.forward));
        var bank = CreateSurface("Bank", Quaternion.AngleAxis(60f, Vector3.forward));
        var oppositeBank = CreateSurface("Opposite Bank", Quaternion.AngleAxis(-60f, Vector3.forward));

        var frameSource = new FakeRunProgressFrameSource();
        var time = new FakeTime { FixedDeltaTime = fixedDeltaTime };
        var slopeCalculator = new RunSurfaceSlopeCalculator();

        var probe = new PhysicsRunSupportProbe(
            supportCollider,
            new RunSupportColliderProbeFactory(),
            new RunSurfaceProbeConfig(0.4f, 0.02f, TestAssets.RunSurfaceLayerMask, 0.17f, 0.6f, 8f),
            slopeCalculator);

        var pipeline = new RunSurfaceFramePipeline(
            frameSource,
            probe,
            new SurfaceRunMotionSource(supportCollider.transform),
            new RunSupportAttachmentPolicy(
                new RunSupportAttachmentConfig(0.35f, 0.08f, 30f, 0.04f)),
            new RunSurfaceStabilityPolicy(
                new RunSurfaceStabilityConfig(0.06f, 45f, 0.04f, 8f),
                slopeCalculator),
            new RunSteeringFramePolicy(new RunSteeringFrameConfig(180f, 0.08f)),
            time);
        ((IRunSteeringFrameResetter)pipeline).Reset(Vector3.up);

        return new SurfaceFixture(pipeline, frameSource, flat, gentleSlope, bank, oppositeBank);
    }

    private GameObject CreateSurface(string name, Quaternion rotation)
    {
        var surface = CreateGameObject(name);
        surface.layer = GetSingleLayer(TestAssets.RunSurfaceLayerMask, "Run Surface");
        var size = new Vector3(4f, 0.1f, 4f);

        surface.transform.SetPositionAndRotation(
            TestOrigin + (Vector3.up * 0.05f) - (rotation * Vector3.up * (size.y * 0.5f)),
            rotation);
        surface.transform.localScale = size;
        surface.AddComponent<BoxCollider>();
        surface.AddComponent<RunContact>().SetCategoryForTests(RunContactCategory.Surface);
        surface.SetActive(false);
        return surface;
    }

    private int GetSingleLayer(LayerMask layerMask, string description)
    {
        Assert.That(layerMask.value, Is.Not.EqualTo(0), description);
        Assert.That(layerMask.value & (layerMask.value - 1), Is.Zero, description);
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, 2f));
    }

    private GameObject CreateGameObject(string name)
    {
        var gameObject = new GameObject(name);
        _createdObjects.Add(gameObject);
        return gameObject;
    }

    private static void AssertSupported(
        RunSurfaceTraceSample sample,
        RunSurfaceTransition expectedTransition,
        Vector3 expectedNormal,
        float toleranceDegrees = 0.1f)
    {
        Assert.That(sample.Snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Supported),
            sample.FailureContext);
        Assert.That(sample.Snapshot.ObservedSupport.SupportDistance, Is.GreaterThan(0f), sample.FailureContext);
        Assert.That(sample.Snapshot.StableSupport.IsGrounded, Is.True, sample.FailureContext);

        Assert.That(Vector3.Angle(sample.Snapshot.StableSupport.GroundNormal, expectedNormal),
            Is.LessThanOrEqualTo(toleranceDegrees), sample.FailureContext);
        Assert.That(sample.Snapshot.Transition, Is.EqualTo(expectedTransition), sample.FailureContext);
    }

    private sealed class SurfaceFixture
    {
        private readonly RunSurfaceFramePipeline _pipeline;
        private readonly FakeRunProgressFrameSource _frameSource;
        private readonly GameObject _flat;
        private readonly GameObject _gentleSlope;
        private readonly GameObject _bank;
        private readonly GameObject _oppositeBank;
        private int _tick;

        public Vector3 GentleSlopeNormal => _gentleSlope.transform.up;
        public Vector3 BankNormal => _bank.transform.up;

        public bool IsProgressFrameAvailable
        {
            set => _frameSource.IsAvailable = value;
        }

        public SurfaceFixture(
            RunSurfaceFramePipeline pipeline,
            FakeRunProgressFrameSource frameSource,
            GameObject flat,
            GameObject gentleSlope,
            GameObject bank,
            GameObject oppositeBank)
        {
            _pipeline = pipeline;
            _frameSource = frameSource;
            _flat = flat;
            _gentleSlope = gentleSlope;
            _bank = bank;
            _oppositeBank = oppositeBank;
        }

        public void ShowFlat()
        {
            Show(_flat);
        }

        public void ShowGentleSlope()
        {
            Show(_gentleSlope);
        }

        public void ShowBank()
        {
            Show(_bank);
        }

        public void ShowOppositeBank()
        {
            Show(_oppositeBank);
        }

        public void ShowNone()
        {
            Show(null);
        }

        public RunSurfaceTraceSample Tick(string label)
        {
            _tick += 1;
            Physics.SyncTransforms();
            ((IFixedTickable)_pipeline).FixedTick();
            return new RunSurfaceTraceSample(_tick, label, ((IRunSurfaceFrameSource)_pipeline).Current);
        }

        private void Show(GameObject activeSurface)
        {
            _flat.SetActive(activeSurface == _flat);
            _gentleSlope.SetActive(activeSurface == _gentleSlope);
            _bank.SetActive(activeSurface == _bank);
            _oppositeBank.SetActive(activeSurface == _oppositeBank);
        }
    }

    private readonly struct RunSurfaceTraceSample
    {
        public RunSurfaceFrameSnapshot Snapshot { get; }
        public bool IsCoreLocomotionGrounded => Snapshot.StableSupport.IsGrounded;
        public bool IsPresentationGrounded => Snapshot.ObservedSupport.State == RunSupportObservationState.Supported;
        public string DiagnosticsText { get; }

        public string FailureContext { get; }

        public RunSurfaceTraceSample(int tick, string label, RunSurfaceFrameSnapshot snapshot)
        {
            Snapshot = snapshot;

            var diagnosticsSample = new RunDiagnosticsOverlaySample(
                speedMetersPerSecond: 0f,
                motionStepMetersPerSecond: 0f,
                visualTargetStepMetersPerSecond: 0f,
                visualTargetStepMeters: 0f,
                observedGroundNormalDeltaDegrees: 0f,
                steeringUpDeltaDegrees: 0f,
                visualLagCentimeters: 0f,
                cameraStepMetersPerSecond: 0f,
                targetToMotionCentimeters: 0f,
                visualTargetRotationDeltaDegrees: 0f,
                visualRotationDeltaDegrees: 0f,
                cameraRotationDeltaDegrees: 0f,
                estimatedVisualSnapReason: RunDiagnosticsOverlaySnapReason.None,
                fixedStepsThisFrame: 1,
                snapshot,
                speedDiagnostics: default);

            DiagnosticsText = new RunDiagnosticsOverlayTextFormatter().FormatMotionSummary(diagnosticsSample);

            FailureContext =
                $"trace tick {tick} ({label}): observed={snapshot.ObservedSupport.State}, transition={snapshot.Transition}";
        }
    }

    private sealed class FakeRunProgressFrameSource : IRunProgressFrameSource
    {
        public bool IsAvailable { get; set; } = true;

        public bool TryCreateSnapshot(
            Vector3 origin,
            out RunProgressFrameSnapshot snapshot,
            out string error)
        {
            if (!IsAvailable)
            {
                snapshot = default;
                error = "Canonical trace progress frame unavailable.";
                return false;
            }

            return RunProgressFrameSnapshot.TryCreate(origin, Vector3.forward, Vector3.up, out snapshot, out error);
        }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }

    private sealed class SurfaceRunMotionSource : IRunMotionSource
    {
        private readonly Transform _transform;

        public Vector3 Position => _transform.position;
        public Vector3 LinearVelocity => Vector3.zero;

        public SurfaceRunMotionSource(Transform transform)
        {
            _transform = transform;
        }
    }
}
