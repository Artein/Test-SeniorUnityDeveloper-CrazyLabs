using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.CharacterPresentation
{
    public sealed partial class CharacterPresentationSceneCompositionMonoInstaller : BaseSceneCompositionMonoInstaller
    {
        [SerializeField] private CharacterPresentationView _view;
        [SerializeField] private Transform _visualTarget;
        [SerializeField] private AnimatedContactSensorPoseSyncView _animatedContactSensorPoseSyncView;

        internal Transform VisualTarget => _visualTarget;

        public override void Install([NotNull] IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            ThrowIfInvalidReferences();

            builder.RegisterInstance<ICharacterPresentationView>(_view);
            builder.RegisterInstance<ICharacterPresentationTuning>(_view);
            builder.RegisterInstance<ICharacterVisualFollowView>(_view);
            builder.RegisterInstance<ICharacterVisualFollowTuning>(_view);
            builder.RegisterInstance<ICharacterVisualTargetPoseSource>(
                new TransformCharacterVisualTargetPoseSource(_visualTarget));
            builder.RegisterInstance<IAnimatedContactSensorPoseSyncView>(_animatedContactSensorPoseSyncView);
            builder.Register<ICharacterPresentationModeClassifier, CharacterPresentationModeClassifier>(Lifetime.Singleton);
            builder.Register<ICharacterPresentationSupportTracker, CharacterPresentationSupportTracker>(Lifetime.Singleton);
            builder.Register<ICharacterVisualPoseSmoother, CharacterVisualPoseSmoother>(Lifetime.Transient);
            builder.Register<CharacterVisualFollower>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AnimatedContactSensorPoseSync>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterEntryPoint<CharacterPresenter>();
        }

        internal override IEnumerable<string> GetReferenceValidationErrors()
        {
            if (_view == null)
                yield return "CharacterPresentationSceneCompositionMonoInstaller requires a Character Presentation View reference.";

            if (_visualTarget == null)
                yield return "CharacterPresentationSceneCompositionMonoInstaller requires a Visual Target reference.";

            if (_animatedContactSensorPoseSyncView == null)
            {
                yield return
                    "CharacterPresentationSceneCompositionMonoInstaller requires an Animated Contact Sensor Pose Sync View reference.";
            }
            else
            {
                var validator = new AnimatedContactSensorPoseSyncReferenceValidator();

                foreach (var error in validator.GetReferenceValidationErrors(
                             _animatedContactSensorPoseSyncView.RootRigidbody,
                             _animatedContactSensorPoseSyncView.Bindings))
                {
                    yield return error;
                }
            }
        }

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToArray();

            if (errors.Length > 0)
                throw new InvalidOperationException(string.Join(separator: "\n", errors));
        }
    }
}
