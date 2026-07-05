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
        [SerializeField] private LayerMask _surfaceMask = Physics.DefaultRaycastLayers;

        public override void Install([NotNull] IContainerBuilder builder)
        {
            ThrowIfInvalidReferences();

            builder.Register<PhysicsRunSurfaceContextSource>(Lifetime.Singleton)
                .WithParameter(_supportCollider)
                .WithParameter(_supportProbeDistance)
                .WithParameter(_surfaceMask)
                .AsImplementedInterfaces();
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
        }
    }
}
