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

        private FakeRunProgressFrameSource _progressFrameSource;
        private FakeRunSupportProbe _supportProbe;
        private FakeTime _time;
        private RunSurfaceFramePipeline _pipeline;

        [SetUp]
        public void SetUp()
        {
            _progressFrameSource = new FakeRunProgressFrameSource();
            _supportProbe = new FakeRunSupportProbe();
            _time = new FakeTime { FixedDeltaTime = FixedDeltaTime };
            _pipeline = CreatePipeline(_progressFrameSource, _supportProbe, _time);
        }

        [Test]
        public void FixedTick_ValidSupportedObservation_PublishesCompleteSnapshot()
        {
            ((IRunSteeringFrameResetter)_pipeline).Reset(Vector3.forward);
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = Vector3.up;
            _supportProbe.SupportDistance = 0.03f;

            Tick(_pipeline);

            var snapshot = ((IRunSurfaceFrameSource)_pipeline).Current;
            Assert.That(_progressFrameSource.CreateSnapshotCount, Is.EqualTo(1));
            Assert.That(_supportProbe.ObserveCount, Is.EqualTo(1));
            Assert.That(snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Supported));
            Assert.That(snapshot.ObservedSupport.SupportDistance, Is.EqualTo(0.03f));
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

            var snapshot = ((IRunSurfaceFrameSource)_pipeline).Current;
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

            _supportProbe.Normal = Quaternion.AngleAxis(10f, Vector3.forward) * Vector3.up;
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
            var previousSnapshot = ((IRunSurfaceFrameSource)_pipeline).Current;

            var observedPreviousSnapshot = false;

            _supportProbe.OnObserve = () =>
            {
                var visibleSnapshot = ((IRunSurfaceFrameSource)_pipeline).Current;

                observedPreviousSnapshot = visibleSnapshot.Transition == previousSnapshot.Transition
                                           && visibleSnapshot.StableSupport.GroundNormal == previousSnapshot.StableSupport.GroundNormal;
            };
            _supportProbe.Normal = Quaternion.AngleAxis(10f, Vector3.forward) * Vector3.up;

            Tick(_pipeline);

            Assert.That(observedPreviousSnapshot, Is.True);
            Assert.That(((IRunSurfaceFrameSource)_pipeline).Current.Transition, Is.EqualTo(RunSurfaceTransition.ContinuousUpdate));
        }

        [Test]
        public void FixedTick_MissingWithinGrace_PublishesObservedMissAndHeldStableSupportTogether()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            Tick(_pipeline);
            _supportProbe.State = RunSupportObservationState.Missing;

            Tick(_pipeline);

            var snapshot = ((IRunSurfaceFrameSource)_pipeline).Current;
            Assert.That(snapshot.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Missing));
            Assert.That(snapshot.StableSupport.IsGrounded, Is.True);
            Assert.That(snapshot.IsMissingSupportHeld, Is.True);
            Assert.That(snapshot.Transition, Is.EqualTo(RunSurfaceTransition.None));
        }

        [Test]
        public void FixedTick_TwoPipelines_HaveIndependentPolicyState()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            Tick(_pipeline);

            var secondProgressSource = new FakeRunProgressFrameSource();
            var secondProbe = new FakeRunSupportProbe { State = RunSupportObservationState.Missing };
            var secondPipeline = CreatePipeline(secondProgressSource, secondProbe, _time);
            Tick(secondPipeline);

            Assert.That(((IRunSurfaceFrameSource)_pipeline).Current.StableSupport.IsGrounded, Is.True);
            Assert.That(((IRunSurfaceFrameSource)secondPipeline).Current.StableSupport.IsGrounded, Is.False);
        }

        [Test]
        public void FixedTick_AfterWarmup_DoesNotAllocateManagedMemory()
        {
            _supportProbe.State = RunSupportObservationState.Supported;
            _supportProbe.Normal = Vector3.up;

            for (var index = 0; index < 10; index += 1)
                Tick(_pipeline);

            var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();

            for (var index = 0; index < 100; index += 1)
                Tick(_pipeline);

            var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBytesBefore;
            Assert.That(allocatedBytes, Is.Zero);
        }

        private static RunSurfaceFramePipeline CreatePipeline(
            IRunProgressFrameSource progressFrameSource,
            IRunSupportProbe supportProbe,
            ITime time)
        {
            var stabilityPolicy = new RunSurfaceStabilityPolicy(
                new RunSurfaceStabilityConfig(0.06f, 45f, 0.04f, 8f),
                new RunSurfaceSlopeCalculator());
            var steeringPolicy = new RunSteeringFramePolicy(new RunSteeringFrameConfig(180f, 0.08f));
            return new RunSurfaceFramePipeline(progressFrameSource, supportProbe, stabilityPolicy, steeringPolicy, time);
        }

        private static void Tick(RunSurfaceFramePipeline pipeline)
        {
            ((IFixedTickable)pipeline).FixedTick();
        }

        private sealed class FakeRunProgressFrameSource : IRunProgressFrameSource
        {
            private readonly RunProgressFrameSnapshot _snapshot;

            public bool IsAvailable { get; set; } = true;
            public int CreateSnapshotCount { get; private set; }

            public FakeRunProgressFrameSource()
            {
                Assert.That(
                    RunProgressFrameSnapshot.TryCreate(Vector3.zero, Vector3.forward, Vector3.up, out _snapshot, out var error),
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
            public Vector3 SampleOrigin { get; set; }
            public RunSupportObservationState State { get; set; } = RunSupportObservationState.Missing;
            public Vector3 Normal { get; set; } = Vector3.up;
            public float SupportDistance { get; set; }
            public int ObserveCount { get; private set; }
            public bool LastHasContinuityNormal { get; private set; }
            public Vector3 LastContinuityNormal { get; private set; }
            public Action OnObserve { get; set; }

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
                    return new RunSupportObservation(State, progressFrame, default, 0f);

                var context = new RunSurfaceContext(true, Normal, 0f);
                return new RunSupportObservation(State, progressFrame, context, SupportDistance);
            }
        }

        private sealed class FakeTime : ITime
        {
            public float DeltaTime { get; set; }
            public float FixedDeltaTime { get; set; }
        }
    }
}
