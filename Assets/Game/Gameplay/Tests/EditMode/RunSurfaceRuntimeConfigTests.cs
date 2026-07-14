using System;
using NUnit.Framework;

namespace Game.Gameplay.Tests.EditMode
{
    public sealed class RunSurfaceRuntimeConfigTests
    {
        [Test]
        public void ProbeConfig_ValidValues_PreservesEveryAuthoredValue()
        {
            var config = new RunSurfaceProbeConfig(
                distance: 0.08f,
                skinWidth: 0.02f,
                1 << 7,
                minimumSupportNormalDot: 0.17f,
                footprintSampleOffsetScale: 0.6f,
                footprintNormalClusterAngleDegrees: 8f);

            Assert.That(config.Distance, Is.EqualTo(expected: 0.08f));
            Assert.That(config.SkinWidth, Is.EqualTo(expected: 0.02f));
            Assert.That(config.SurfaceMask.value, Is.EqualTo(1 << 7));
            Assert.That(config.MinimumSupportNormalDot, Is.EqualTo(expected: 0.17f));
            Assert.That(config.FootprintSampleOffsetScale, Is.EqualTo(expected: 0.6f));
            Assert.That(config.FootprintNormalClusterAngleDegrees, Is.EqualTo(expected: 8f));
        }

        [TestCase(float.NaN)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(arg: -0.01f)]
        public void ProbeConfig_InvalidDistance_Throws(float value)
        {
            Assert.That(() => CreateProbeConfig(value), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(float.NaN)]
        [TestCase(float.NegativeInfinity)]
        [TestCase(arg: -0.01f)]
        public void ProbeConfig_InvalidSkinWidth_Throws(float value)
        {
            Assert.That(() => CreateProbeConfig(skinWidth: value), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ProbeConfig_EmptySurfaceMask_Throws()
        {
            Assert.That(() => CreateProbeConfig(surfaceMask: 0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(float.NaN)]
        [TestCase(arg: -1.01f)]
        [TestCase(arg: 1.01f)]
        public void ProbeConfig_InvalidMinimumSupportNormalDot_Throws(float value)
        {
            Assert.That(() => CreateProbeConfig(minimumSupportNormalDot: value), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(float.NaN)]
        [TestCase(arg: -0.01f)]
        [TestCase(arg: 1.01f)]
        public void ProbeConfig_InvalidFootprintSampleOffsetScale_Throws(float value)
        {
            Assert.That(() => CreateProbeConfig(footprintSampleOffsetScale: value), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(float.NaN)]
        [TestCase(arg: -0.01f)]
        [TestCase(arg: 180.01f)]
        public void ProbeConfig_InvalidFootprintClusterAngle_Throws(float value)
        {
            Assert.That(() => CreateProbeConfig(footprintNormalClusterAngleDegrees: value), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AttachmentConfig_ValidValues_PreservesEveryAuthoredValue()
        {
            var config = new RunSupportAttachmentConfig(
                maximumAttachedSurfaceNormalLiftSpeed: 0.35f,
                sameSurfaceReattachmentSeparationMeters: 0.08f,
                minimumReattachmentNormalChangeDegrees: 30f,
                transitionConfirmationSeconds: 0.04f);

            Assert.That(config.MaximumAttachedSurfaceNormalLiftSpeed, Is.EqualTo(expected: 0.35f));
            Assert.That(config.SameSurfaceReattachmentSeparationMeters, Is.EqualTo(expected: 0.08f));
            Assert.That(config.MinimumReattachmentNormalChangeDegrees, Is.EqualTo(expected: 30f));
            Assert.That(config.TransitionConfirmationSeconds, Is.EqualTo(expected: 0.04f));
        }

        [TestCase(float.NaN, 0.08f, 30f, 0.04f)]
        [TestCase(0.35f, -0.01f, 30f, 0.04f)]
        [TestCase(0.35f, 0.08f, 180.01f, 0.04f)]
        [TestCase(0.35f, 0.08f, 30f, -0.01f)]
        public void AttachmentConfig_InvalidValue_Throws(
            float maximumLiftSpeed,
            float separationMeters,
            float minimumNormalChangeDegrees,
            float confirmationSeconds)
        {
            Assert.That(
                () => new RunSupportAttachmentConfig(
                    maximumLiftSpeed,
                    separationMeters,
                    minimumNormalChangeDegrees,
                    confirmationSeconds),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        private static RunSurfaceProbeConfig CreateProbeConfig(
            float distance = 0.08f,
            float skinWidth = 0.02f,
            int surfaceMask = 1 << 7,
            float minimumSupportNormalDot = 0.17f,
            float footprintSampleOffsetScale = 0.6f,
            float footprintNormalClusterAngleDegrees = 8f)
        {
            return new RunSurfaceProbeConfig(
                distance,
                skinWidth,
                surfaceMask,
                minimumSupportNormalDot,
                footprintSampleOffsetScale,
                footprintNormalClusterAngleDegrees);
        }
    }
}
