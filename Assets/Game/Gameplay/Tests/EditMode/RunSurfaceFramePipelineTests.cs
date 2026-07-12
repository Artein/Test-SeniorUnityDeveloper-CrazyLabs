using System;
using Game.Foundation.Time;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay.Tests.EditMode
{
    public sealed class RunSurfaceFramePipelineTests
    {
        private const float FixedDeltaTime = 0.02f;
        private FakeRunMotionSource _motionSource;
        private RunSurfaceFramePipeline _pipeline;

        private FakeRunProgressFrameSource _progressFrameSource;
        private FakeRunSupportProbe _supportProbe;
        private FakeTime _time;

        [SetUp]
        public void SetUp()
        {
            _progressFrameSource = new FakeRunProgressFrameSource();
            _supportProbe = new FakeRunSupportProbe();
            _motionSource = new FakeRunMotionSource();
            _time = new FakeTime { FixedDeltaTime = FixedDeltaTime };

            _pipeline = CreatePipeline(
                _progressFrameSource,
                _supportProbe,
                _motionSource,
                _time);
        }

        [Test]
        public void FixedTick_ValidSupportedObservation_PublishesCompleteSnapshot()
        {
            ((IRunSteeringFrameResetter)_pipeline).Reset(Vector3.forward);
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = Vector3.up;
            _supportProbe.SupportDistance = 0.03f;

            Tick(_pipeline);

            var snapshot = _pipeline.Current;
            Assert.That(_progressFrameSource.CreateSnapshotCount, Is.EqualTo(expected: 1));
            Assert.That(_supportProbe.ObserveCount, Is.EqualTo(expected: 1));
            Assert.That(snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Supported));
            Assert.That(snapshot.ObservedSupport.SupportDistance, Is.EqualTo(expected: 0.03f));
            Assert.That(snapshot.StableSupport.IsGrounded, Is.True);
            Assert.That(snapshot.StableSupport.GroundNormal, Is.EqualTo(Vector3.up));
            Assert.That(snapshot.Transition, Is.EqualTo(RunSurfaceTransition.SupportAcquired));
            Assert.That(snapshot.IsMissingSupportHeld, Is.False);
            Assert.That(snapshot.IsConfirmingDiscontinuity, Is.False);
            Assert.That(snapshot.SteeringFrame.IsValid, Is.True);
            Assert.That(snapshot.SteeringFrame.UpDirection, Is.EqualTo(Vector3.up));
        }

        [Test]
        public void FixedTick_InvalidProgressFrame_PublishesUnavailableHardResetWithoutProbe()
        {
            ((IRunSteeringFrameResetter)_pipeline).Reset(Vector3.forward);
            _progressFrameSource.IsAvailable = false;

            Tick(_pipeline);

            var snapshot = _pipeline.Current;
            Assert.That(_supportProbe.ObserveCount, Is.Zero);
            Assert.That(snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Unavailable));
            Assert.That(snapshot.StableSupport.IsGrounded, Is.False);
            Assert.That(snapshot.Transition, Is.EqualTo(RunSurfaceTransition.HardReset));
            Assert.That(snapshot.SteeringFrame.IsValid, Is.False);
        }

        [Test]
        public void FixedTick_PreviousStableSupport_PassesContinuityNormalToProbe()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = Vector3.up;
            Tick(_pipeline);

            _supportProbe.Normal = Quaternion.AngleAxis(angle: 10f, Vector3.forward) * Vector3.up;
            Tick(_pipeline);

            Assert.That(_supportProbe.LastHasContinuityNormal, Is.True);
            Assert.That(_supportProbe.LastContinuityNormal, Is.EqualTo(Vector3.up));
        }

        [Test]
        public void FixedTick_DuringProbeEvaluation_PreviousCompleteSnapshotRemainsVisible()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = Vector3.up;
            Tick(_pipeline);
            var previousSnapshot = _pipeline.Current;

            var observedPreviousSnapshot = false;

            _supportProbe.OnObserve = () =>
            {
                var visibleSnapshot = _pipeline.Current;

                observedPreviousSnapshot = visibleSnapshot.Transition == previousSnapshot.Transition
                                           && visibleSnapshot.StableSupport.GroundNormal == previousSnapshot.StableSupport.GroundNormal;
            };

            _supportProbe.Normal = Quaternion.AngleAxis(angle: 10f, Vector3.forward) * Vector3.up;

            Tick(_pipeline);

            Assert.That(observedPreviousSnapshot, Is.True);
            Assert.That(_pipeline.Current.Transition, Is.EqualTo(RunSurfaceTransition.ContinuousUpdate));
        }

        [Test]
        public void FixedTick_MissingWithinGrace_PublishesObservedMissAndHeldStableSupportTogether()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            Tick(_pipeline);
            _supportProbe.State = RunSupportObservationState.Missing;

            Tick(_pipeline);

            var snapshot = _pipeline.Current;
            Assert.That(snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Missing));
            Assert.That(snapshot.StableSupport.IsGrounded, Is.True);
            Assert.That(snapshot.IsMissingSupportHeld, Is.True);
            Assert.That(snapshot.Transition, Is.EqualTo(RunSurfaceTransition.None));
        }

        [TestCase(arg: -75f)]
        [TestCase(arg: 75f)]
        public void FixedTick_DetachedFromUSideThenFlatSupport_ReattachesAndSnapsSteering(float bankDegrees)
        {
            ((IRunSteeringFrameResetter)_pipeline).Reset(Vector3.forward);
            var uSideNormal = Quaternion.AngleAxis(bankDegrees, Vector3.forward) * Vector3.up;
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = uSideNormal;
            Tick(_pipeline);

            _motionSource.LinearVelocity = uSideNormal;
            Tick(_pipeline);
            _supportProbe.State = RunSupportObservationState.Missing;
            Tick(_pipeline);

            var detached = _pipeline.Current;
            Assert.That(detached.AttachmentTransition, Is.EqualTo(RunSupportAttachmentTransition.Detached));
            Assert.That(detached.StableSupport.GroundNormal, Is.EqualTo(uSideNormal));

            _motionSource.LinearVelocity = Vector3.down;
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = Vector3.up;
            Tick(_pipeline);
            Tick(_pipeline);

            var reattached = _pipeline.Current;
            Assert.That(reattached.AttachmentTransition, Is.EqualTo(RunSupportAttachmentTransition.Reattached));
            Assert.That(reattached.Transition, Is.EqualTo(RunSurfaceTransition.SupportReattached));
            Assert.That(reattached.StableSupport.GroundNormal, Is.EqualTo(Vector3.up));
            Assert.That(reattached.SteeringFrame.UpDirection, Is.EqualTo(Vector3.up));
            Assert.That(reattached.IsConfirmingDiscontinuity, Is.False);
        }

        [Test]
        public void FixedTick_TwoPipelines_HaveIndependentPolicyState()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            Tick(_pipeline);

            var secondProgressSource = new FakeRunProgressFrameSource();
            var secondProbe = new FakeRunSupportProbe { State = RunSupportObservationState.Missing };

            var secondPipeline = CreatePipeline(
                secondProgressSource,
                secondProbe,
                new FakeRunMotionSource(),
                _time);

            Tick(secondPipeline);

            Assert.That(_pipeline.Current.StableSupport.IsGrounded, Is.True);
            Assert.That(secondPipeline.Current.StableSupport.IsGrounded, Is.False);
        }

        [Test]
        public void FixedTick_AfterWarmup_DoesNotAllocateManagedMemory()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = Vector3.up;

            for (var index = 0; index < 10; index += 1)
            {
                Tick(_pipeline);
            }

            var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();

            for (var index = 0; index < 100; index += 1)
            {
                Tick(_pipeline);
            }

            var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBytesBefore;
            Assert.That(allocatedBytes, Is.Zero);
        }

        private static RunSurfaceFramePipeline CreatePipeline(
            IRunProgressFrameSource progressFrameSource,
            IRunSupportProbe supportProbe,
            IRunMotionSource motionSource,
            ITime time)
        {
            var attachmentPolicy = new RunSupportAttachmentPolicy(
                new RunSupportAttachmentConfig(
                    maximumAttachedSurfaceNormalLiftSpeed: 0.35f,
                    sameSurfaceReattachmentSeparationMeters: 0.08f,
                    minimumReattachmentNormalChangeDegrees: 30f,
                    transitionConfirmationSeconds: 0.04f));

            var stabilityPolicy = new RunSurfaceStabilityPolicy(
                new RunSurfaceStabilityConfig(
                    supportLossConfirmationSeconds: 0.06f,
                    discontinuousNormalThresholdDegrees: 45f,
                    discontinuousNormalConfirmationSeconds: 0.04f,
                    candidateCoherenceDegrees: 8f),
                new RunSurfaceSlopeCalculator());

            var steeringPolicy =
                new RunSteeringFramePolicy(new RunSteeringFrameConfig(normalSlewDegreesPerSecond: 180f, airborneUpRetentionSeconds: 0.08f));

            return new RunSurfaceFramePipeline(
                progressFrameSource,
                supportProbe,
                motionSource,
                attachmentPolicy,
                stabilityPolicy,
                steeringPolicy,
                time);
        }

        private static void Tick(RunSurfaceFramePipeline pipeline)
        {
            ((IFixedTickable)pipeline).FixedTick();
        }

        private sealed class FakeRunProgressFrameSource : IRunProgressFrameSource
        {
            private readonly RunProgressFrameSnapshot _snapshot;
            public int CreateSnapshotCount { get; private set; }

            public bool IsAvailable { get; set; } = true;

            public FakeRunProgressFrameSource()
            {
                Assert.That(
                    RunProgressFrameSnapshot.TryCreate(
                        Vector3.zero,
                        Vector3.forward,
                        Vector3.up,
                        out _snapshot,
                        out var error),
                    Is.True,
                    error);
            }

            public bool TryCreateSnapshot(
                Vector3 origin,
                out RunProgressFrameSnapshot snapshot,
                out string error)
            {
                CreateSnapshotCount += 1;
                snapshot = IsAvailable ? _snapshot : default;
                error = IsAvailable ? string.Empty : "Test progress frame unavailable.";
                return IsAvailable;
            }
        }

        private sealed class FakeRunSupportProbe : IRunSupportProbe
        {
            public Vector3 LastContinuityNormal { get; private set; }
            public bool LastHasContinuityNormal { get; private set; }
            public Vector3 Normal { get; set; } = Vector3.up;
            public int ObserveCount { get; private set; }
            public Action OnObserve { get; set; }
            public Vector3 SampleOrigin { get; set; }
            public RunSupportObservationState State { get; set; } = RunSupportObservationState.Missing;
            public float SupportDistance { get; set; }

            public RunSupportObservation Observe(
                RunProgressFrameSnapshot progressFrame,
                bool hasContinuityNormal,
                Vector3 continuityNormal)
            {
                ObserveCount += 1;
                LastHasContinuityNormal = hasContinuityNormal;
                LastContinuityNormal = continuityNormal;
                OnObserve?.Invoke();

                if (State == RunSupportObservationState.Missing)
                    return new RunSupportObservation(State, progressFrame, surfaceContext: default, supportDistance: 0f);

                var context = new RunSurfaceContext(isGrounded: true, Normal, forwardDownhillDegrees: 0f);

                return new RunSupportObservation(
                    State,
                    progressFrame,
                    context,
                    SupportDistance);
            }
        }

        private sealed class FakeTime : ITime
        {
            public float DeltaTime { get; set; }
            public float FixedDeltaTime { get; set; }
        }

        private sealed class FakeRunMotionSource : IRunMotionSource
        {
            public Vector3 LinearVelocity { get; set; }
            public Vector3 Position { get; set; }
        }
    }
}
