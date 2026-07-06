using System;
using VContainer.Unity;

namespace Game.Gameplay.CharacterPresentation
{
    public sealed class AnimatedContactSensorPoseSync : ILateTickable
    {
        private readonly IAnimatedContactSensorPoseSyncView _view;

        public AnimatedContactSensorPoseSync(IAnimatedContactSensorPoseSyncView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        void ILateTickable.LateTick()
        {
            CopySourcePosesToTargets();
        }

        internal int CopySourcePosesToTargets()
        {
            var copiedCount = 0;
            var bindings = _view.Bindings ?? Array.Empty<AnimatedContactSensorPoseBinding>();

            foreach (var binding in bindings)
            {
                if (binding.Source == null || binding.Target == null)
                    continue;

                binding.Target.SetPositionAndRotation(binding.Source.position, binding.Source.rotation);
                binding.Target.localScale = binding.Source.lossyScale;
                copiedCount += 1;
            }

            return copiedCount;
        }
    }
}
