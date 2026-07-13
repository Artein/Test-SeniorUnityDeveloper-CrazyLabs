using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    public sealed partial class RunMovementSceneCompositionMonoInstaller : BaseSceneCompositionMonoInstaller
    {
        [SerializeField] private RunBodyMovementConfig _config;
        [SerializeField] private RigidbodyRunBodyMovementTarget _movementTarget;
        [SerializeField] private RigidbodyRunCameraSource _runCameraSource;
        [SerializeField] private RunProgressFrameSource _progressFrameSource;
        [SerializeField] private RigidbodyContactNotifier _contactNotifier;

        internal RunBodyMovementConfig Config => _config;
        internal RunProgressFrameSource ProgressFrameSource => _progressFrameSource;

        public override void Install([NotNull] IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            ThrowIfInvalidReferences();

            RegisterPorts(builder);
            RegisterConfigs(builder);
            RegisterPolicies(builder);

            builder.Register<RunSurfaceFramePipeline>(Lifetime.Singleton)
                .As<IRunSurfaceFrameSource, IRunSteeringFrameSource, IRunSteeringFrameResetter, IRunSurfaceFrameFixedStep>();

            builder.Register<RunBodySpeedEnvelopeValidator>(Lifetime.Singleton);

            builder.Register<RunBodyMovementController>(Lifetime.Singleton)
                .AsSelf()
                .As<IRunBodyMovementFixedStep, IInitializable>();
        }

        internal override IEnumerable<string> GetReferenceValidationErrors()
        {
            if (_config == null)
            {
                yield return "RunMovementSceneCompositionMonoInstaller requires a Run Body Movement Config reference.";
            }
            else
            {
                var validator = new RunBodyMovementConfigValidator();

                foreach (var error in validator.Validate(_config))
                {
                    yield return $"RunMovementSceneCompositionMonoInstaller Run Body Movement Config is invalid: {error}";
                }
            }

            if (_movementTarget == null)
                yield return "RunMovementSceneCompositionMonoInstaller requires a Run Body Movement Target reference.";

            if (_runCameraSource == null)
                yield return "RunMovementSceneCompositionMonoInstaller requires a Run Camera Source reference.";

            if (_progressFrameSource == null)
                yield return "RunMovementSceneCompositionMonoInstaller requires a Run Progress Frame Source reference.";

            if (_contactNotifier == null)
                yield return "RunMovementSceneCompositionMonoInstaller requires a Rigidbody Contact Notifier reference.";
        }

        private void RegisterPorts(IContainerBuilder builder)
        {
            builder.RegisterInstance<IRunBodyMovementTarget>(_movementTarget);
            builder.RegisterInstance<IRunCameraSource>(_runCameraSource);
            builder.RegisterInstance<IRunMotionSource>(_runCameraSource);
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

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToArray();

            if (errors.Length > 0)
                throw new InvalidOperationException(string.Join(separator: "\n", errors));
        }
    }
}
