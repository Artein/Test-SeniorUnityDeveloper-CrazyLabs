using NUnit.Framework;
using UnityEngine;

namespace Game.Gameplay.Tests.EditMode
{
    public sealed class RunSupportAttachmentPolicyTests
    {
        private const float FixedDeltaTime = 0.02f;

        private RunSupportAttachmentPolicy _policy;
        private RunProgressFrameSnapshot _frame;

        [SetUp]
        public void SetUp()
        {
            _policy = new RunSupportAttachmentPolicy(new RunSupportAttachmentConfig(0.35f, 0.08f, 30f, 0.04f));

            Assert.That(
                RunProgressFrameSnapshot.TryCreate(
                    Vector3.zero,
                    Vector3.forward,
                    Vector3.up,
                    out _frame,
                    out var error),
                Is.True,
                error);
        }

        [Test]
        public void Evaluate_SeparatingSupportThenBriefMiss_ConfirmsDetachmentUsingLastSupportNormal()
        {
            var uSideNormal = Quaternion.AngleAxis(75f, Vector3.forward) * Vector3.up;
            _policy.Evaluate(Supported(uSideNormal), Vector3.zero, Vector3.zero, FixedDeltaTime);

            var candidate = _policy.Evaluate(
                Supported(uSideNormal),
                Vector3.zero,
                uSideNormal,
                FixedDeltaTime);

            var detached = _policy.Evaluate(
                Missing(),
                uSideNormal * 0.1f,
                uSideNormal,
                FixedDeltaTime);

            Assert.That(candidate.Transition, Is.EqualTo(RunSupportAttachmentTransition.None));
            Assert.That(detached.State, Is.EqualTo(RunSupportAttachmentState.Detached));
            Assert.That(detached.Transition, Is.EqualTo(RunSupportAttachmentTransition.Detached));
        }

        [TestCase(-75f)]
        [TestCase(75f)]
        public void Evaluate_DetachedThenCoherentFlatSupport_ConfirmsSymmetricReattachment(float bankDegrees)
        {
            var uSideNormal = Quaternion.AngleAxis(bankDegrees, Vector3.forward) * Vector3.up;
            _policy.Evaluate(Supported(uSideNormal), Vector3.zero, Vector3.zero, FixedDeltaTime);
            _policy.Evaluate(Supported(uSideNormal), Vector3.zero, uSideNormal, FixedDeltaTime);
            _policy.Evaluate(Supported(uSideNormal), uSideNormal * 0.1f, uSideNormal, FixedDeltaTime);

            var candidate = _policy.Evaluate(Supported(Vector3.up), uSideNormal * 0.1f, Vector3.down, FixedDeltaTime);
            var reattached = _policy.Evaluate(Supported(Vector3.up), uSideNormal * 0.1f, Vector3.down, FixedDeltaTime);

            Assert.That(candidate.State, Is.EqualTo(RunSupportAttachmentState.Detached));
            Assert.That(candidate.Transition, Is.EqualTo(RunSupportAttachmentTransition.None));
            Assert.That(reattached.State, Is.EqualTo(RunSupportAttachmentState.Attached));
            Assert.That(reattached.Transition, Is.EqualTo(RunSupportAttachmentTransition.Reattached));
        }

        [Test]
        public void Evaluate_BriefSeamWithoutSeparatingLift_RemainsAttached()
        {
            _policy.Evaluate(Supported(Vector3.up), Vector3.zero, Vector3.forward, FixedDeltaTime);

            var missing = _policy.Evaluate(Missing(), Vector3.zero, Vector3.forward, FixedDeltaTime);

            var recovered = _policy.Evaluate(
                Supported(Quaternion.AngleAxis(75f, Vector3.forward) * Vector3.up),
                Vector3.zero,
                Vector3.forward,
                FixedDeltaTime);

            Assert.That(missing.State, Is.EqualTo(RunSupportAttachmentState.Attached));
            Assert.That(missing.Transition, Is.EqualTo(RunSupportAttachmentTransition.None));
            Assert.That(recovered.State, Is.EqualTo(RunSupportAttachmentState.Attached));
            Assert.That(recovered.Transition, Is.EqualTo(RunSupportAttachmentTransition.None));
        }

        [Test]
        public void Evaluate_DetachedAndStillSeparatedFromSameSurface_DoesNotPrematurelyReattach()
        {
            _policy.Evaluate(Supported(Vector3.up), Vector3.zero, Vector3.zero, FixedDeltaTime);
            _policy.Evaluate(Supported(Vector3.up), Vector3.zero, Vector3.up, FixedDeltaTime);
            _policy.Evaluate(Supported(Vector3.up), Vector3.up * 0.1f, Vector3.up, FixedDeltaTime);

            RunSupportAttachmentResult separated = default;

            for (var tick = 0; tick < 4; tick += 1)
            {
                separated = _policy.Evaluate(
                    Supported(Vector3.up),
                    Vector3.up * 0.1f,
                    Vector3.down,
                    FixedDeltaTime);
            }

            Assert.That(separated.State, Is.EqualTo(RunSupportAttachmentState.Detached));
            Assert.That(separated.Transition, Is.EqualTo(RunSupportAttachmentTransition.None));

            _policy.Evaluate(Supported(Vector3.up), Vector3.zero, Vector3.down, FixedDeltaTime);
            var returned = _policy.Evaluate(Supported(Vector3.up), Vector3.zero, Vector3.down, FixedDeltaTime);

            Assert.That(returned.State, Is.EqualTo(RunSupportAttachmentState.Attached));
            Assert.That(returned.Transition, Is.EqualTo(RunSupportAttachmentTransition.Reattached));
        }

        [Test]
        public void Evaluate_UnavailableAfterDetachment_ResetsAttachmentState()
        {
            _policy.Evaluate(Supported(Vector3.up), Vector3.zero, Vector3.zero, FixedDeltaTime);
            _policy.Evaluate(Supported(Vector3.up), Vector3.zero, Vector3.up, FixedDeltaTime);
            _policy.Evaluate(Supported(Vector3.up), Vector3.up * 0.1f, Vector3.up, FixedDeltaTime);

            var reset = _policy.Evaluate(Unavailable(), Vector3.up, Vector3.up, FixedDeltaTime);

            Assert.That(reset.State, Is.EqualTo(RunSupportAttachmentState.Unknown));
            Assert.That(reset.Transition, Is.EqualTo(RunSupportAttachmentTransition.None));
        }

        private RunSupportObservation Supported(Vector3 normal)
        {
            return new RunSupportObservation(
                RunSupportObservationState.Supported,
                _frame,
                new RunSurfaceContext(true, normal, 0f),
                0.01f);
        }

        private RunSupportObservation Missing()
        {
            return new RunSupportObservation(
                RunSupportObservationState.Missing,
                _frame,
                default,
                0f);
        }

        private RunSupportObservation Unavailable()
        {
            return new RunSupportObservation(
                RunSupportObservationState.Unavailable,
                default,
                default,
                0f);
        }
    }
}
