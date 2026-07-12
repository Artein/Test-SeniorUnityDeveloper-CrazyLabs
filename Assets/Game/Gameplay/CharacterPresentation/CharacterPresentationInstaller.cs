using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.CharacterPresentation
{
    public sealed class CharacterPresentationInstaller : IInstaller
    {
        private readonly AnimatedContactSensorPoseSyncView _animatedContactSensorPoseSyncView;
        private readonly CharacterPresentationView _view;
        private readonly Transform _visualTarget;

        public CharacterPresentationInstaller(
            CharacterPresentationView view,
            Transform visualTarget,
            AnimatedContactSensorPoseSyncView animatedContactSensorPoseSyncView)
        {
            _view = view != null ? view : throw new ArgumentNullException(nameof(view));
            _visualTarget = visualTarget != null ? visualTarget : throw new ArgumentNullException(nameof(visualTarget));

            _animatedContactSensorPoseSyncView = animatedContactSensorPoseSyncView != null
                ? animatedContactSensorPoseSyncView
                : throw new ArgumentNullException(nameof(animatedContactSensorPoseSyncView));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

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

            builder.RegisterEntryPoint<CharacterVisualFollower>();
            builder.RegisterEntryPoint<AnimatedContactSensorPoseSync>();
            builder.RegisterEntryPoint<CharacterPresenter>();
        }
    }
}
