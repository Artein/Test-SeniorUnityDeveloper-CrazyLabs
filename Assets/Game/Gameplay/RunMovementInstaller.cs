using System;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed class RunMovementInstaller : IInstaller
    {
        private readonly RunBodyMovementConfig _config;
        private readonly IRigidbodyContactNotifier _contactNotifier;
        private readonly IRunBodyMovementTarget _movementTarget;
        private readonly IRunMotionSource _motionSource;
        private readonly IRunProgressFrameSource _progressFrameSource;

        public RunMovementInstaller(
            RunBodyMovementConfig config,
            IRunBodyMovementTarget movementTarget,
            IRunMotionSource motionSource,
            IRunProgressFrameSource progressFrameSource,
            IRigidbodyContactNotifier contactNotifier)
        {
            _config = config != null ? config : throw new ArgumentNullException(nameof(config));
            _movementTarget = movementTarget ?? throw new ArgumentNullException(nameof(movementTarget));
            _motionSource = motionSource ?? throw new ArgumentNullException(nameof(motionSource));
            _progressFrameSource = progressFrameSource ?? throw new ArgumentNullException(nameof(progressFrameSource));
            _contactNotifier = contactNotifier ?? throw new ArgumentNullException(nameof(contactNotifier));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            RegisterPorts(builder);
            RegisterConfigs(builder);
            RegisterPolicies(builder);

            builder.Register<RunSurfaceFramePipeline>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RunBodySpeedEnvelopeValidator>(Lifetime.Singleton);
            builder.RegisterEntryPoint<RunBodyMovementController>();
        }

        private void RegisterPorts(IContainerBuilder builder)
        {
            builder.RegisterInstance<IRunBodyMovementTarget>(_movementTarget);
            builder.RegisterInstance<IRunMotionSource>(_motionSource);
            builder.RegisterInstance<IRunProgressFrameSource>(_progressFrameSource);
            builder.RegisterInstance<IRigidbodyContactNotifier>(_contactNotifier);
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance<IRunBodySpeedConfig>(_config);
            builder.RegisterInstance<IRunBodyMovementValidityConfig>(_config);
            builder.RegisterInstance<IRunLaunchLandingStabilizationConfig>(_config);
            builder.RegisterInstance<IRunSteeringConfig>(_config);

            var surfaceStabilityAuthoringConfig = (IRunSurfaceStabilityAuthoringConfig)_config;
            var supportAttachmentAuthoringConfig = (IRunSupportAttachmentAuthoringConfig)_config;
            var steeringFrameAuthoringConfig = (IRunSteeringFrameAuthoringConfig)_config;

            builder.RegisterInstance(
                new RunSupportAttachmentConfig(
                    supportAttachmentAuthoringConfig.MaximumAttachedSurfaceNormalLiftSpeed,
                    supportAttachmentAuthoringConfig.SameSurfaceReattachmentSeparationMeters,
                    supportAttachmentAuthoringConfig.MinimumReattachmentNormalChangeDegrees,
                    supportAttachmentAuthoringConfig.TransitionConfirmationSeconds));

            builder.RegisterInstance(
                new RunSurfaceStabilityConfig(
                    surfaceStabilityAuthoringConfig.SupportLossConfirmationSeconds,
                    surfaceStabilityAuthoringConfig.DiscontinuousNormalThresholdDegrees,
                    surfaceStabilityAuthoringConfig.DiscontinuousNormalConfirmationSeconds,
                    surfaceStabilityAuthoringConfig.CandidateCoherenceDegrees));

            builder.RegisterInstance(
                new RunSteeringFrameConfig(
                    steeringFrameAuthoringConfig.NormalSlewDegreesPerSecond,
                    steeringFrameAuthoringConfig.AirborneUpRetentionSeconds));
        }

        private void RegisterPolicies(IContainerBuilder builder)
        {
            builder.Register<IRunBodySpeedEvaluator, DefaultRunBodySpeedEvaluator>(Lifetime.Singleton);
            builder.Register<IRunBodySpeedDiagnosticsSource, IRunBodySpeedDiagnosticsSink, RunBodySpeedDiagnostics>(Lifetime.Singleton);
            builder.Register<IRunSteeringEvaluator, DefaultRunSteeringEvaluator>(Lifetime.Singleton);
            builder.Register<IRunLaunchLandingStabilizer, RunLaunchLandingStabilizer>(Lifetime.Singleton);
            builder.Register<IRunContactClassifier, RunContactClassifier>(Lifetime.Singleton);
            builder.Register<IRunSurfaceSlopeCalculator, RunSurfaceSlopeCalculator>(Lifetime.Singleton);
            builder.Register<RunSupportAttachmentPolicy>(Lifetime.Singleton);
            builder.Register<RunSurfaceStabilityPolicy>(Lifetime.Singleton);
            builder.Register<RunSteeringFramePolicy>(Lifetime.Singleton);
        }
    }
}
