using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    public sealed partial class GameplayPhysicsSceneCompositionMonoInstaller : BaseSceneCompositionMonoInstaller
    {
        [SerializeField] private Collider _supportCollider;
        [SerializeField] private float _supportProbeDistance = 0.08f;
        [SerializeField] private float _supportProbeSkinWidth = 0.02f;
        [SerializeField] private LayerMask _surfaceMask = Physics.DefaultRaycastLayers;

        [SerializeField, Range(min: -1f, max: 1f)]
        private float _minimumSupportNormalDot = 0.17f;

        [SerializeField, Range(min: 0f, max: 1f)]
        private float _footprintSampleOffsetScale = 0.6f;

        [SerializeField, Range(min: 0f, max: 180f)]
        private float _footprintNormalClusterAngleDegrees = 8f;

        private void Reset()
        {
            _supportCollider = GetComponentInChildren<Collider>();
        }

        private void OnValidate()
        {
            _supportProbeDistance = Mathf.Max(a: 0f, _supportProbeDistance);
            _supportProbeSkinWidth = Mathf.Max(a: 0f, _supportProbeSkinWidth);
            _minimumSupportNormalDot = Mathf.Clamp(_minimumSupportNormalDot, min: -1f, max: 1f);
            _footprintSampleOffsetScale = Mathf.Clamp01(_footprintSampleOffsetScale);
            _footprintNormalClusterAngleDegrees = Mathf.Clamp(_footprintNormalClusterAngleDegrees, min: 0f, max: 180f);
        }

        public override void Install([NotNull] IContainerBuilder builder)
        {
            ThrowIfInvalidReferences();

            var probeConfig = new RunSurfaceProbeConfig(
                _supportProbeDistance,
                _supportProbeSkinWidth,
                _surfaceMask,
                _minimumSupportNormalDot,
                _footprintSampleOffsetScale,
                _footprintNormalClusterAngleDegrees);

            builder.RegisterInstance(probeConfig);
            builder.Register<IRunSupportColliderProbeFactory, RunSupportColliderProbeFactory>(Lifetime.Singleton);

            builder.Register<IRunSupportProbe, PhysicsRunSupportProbe>(Lifetime.Singleton)
                .WithParameter(_supportCollider);
        }

        internal override IEnumerable<string> GetReferenceValidationErrors()
        {
            if (_supportCollider == null)
                yield return "GameplayPhysicsSceneCompositionMonoInstaller requires a Support Collider reference.";
        }

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToArray();

            if (errors.Length > 0)
                throw new InvalidOperationException(string.Join(separator: "\n", errors));
        }
    }
}
