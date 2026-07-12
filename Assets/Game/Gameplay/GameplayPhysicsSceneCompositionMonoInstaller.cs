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
        [SerializeField, Range(-1f, 1f)] private float _minimumSupportNormalDot = 0.17f;
        [SerializeField, Range(0f, 1f)] private float _footprintSampleOffsetScale = 0.6f;
        [SerializeField, Range(0f, 180f)] private float _footprintNormalClusterAngleDegrees = 8f;

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
                throw new InvalidOperationException(string.Join("\n", errors));
        }

        private void Reset()
        {
            _supportCollider = GetComponentInChildren<Collider>();
        }

        private void OnValidate()
        {
            _supportProbeDistance = Mathf.Max(0f, _supportProbeDistance);
            _supportProbeSkinWidth = Mathf.Max(0f, _supportProbeSkinWidth);
            _minimumSupportNormalDot = Mathf.Clamp(_minimumSupportNormalDot, -1f, 1f);
            _footprintSampleOffsetScale = Mathf.Clamp01(_footprintSampleOffsetScale);
            _footprintNormalClusterAngleDegrees = Mathf.Clamp(_footprintNormalClusterAngleDegrees, 0f, 180f);
        }
    }
}
