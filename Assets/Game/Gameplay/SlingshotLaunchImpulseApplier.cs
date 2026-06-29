using System;
using Game.Gameplay.Slingshot;
using JetBrains.Annotations;

namespace Game.Gameplay
{
    public interface ILaunchImpulseApplier
    {
        void Apply(SlingshotLaunchRequest request, LaunchImpulse impulse);
    }

    [UsedImplicitly]
    public sealed class SlingshotLaunchImpulseApplier : ILaunchImpulseApplier
    {
        private readonly ILaunchTarget _launchTarget;
        private readonly IHeldLaunchTarget _heldLaunchTarget;

        public SlingshotLaunchImpulseApplier(ILaunchTarget launchTarget, IHeldLaunchTarget heldLaunchTarget)
        {
            _launchTarget = launchTarget ?? throw new ArgumentNullException(nameof(launchTarget));
            _heldLaunchTarget = heldLaunchTarget ?? throw new ArgumentNullException(nameof(heldLaunchTarget));
        }

        public void Apply(SlingshotLaunchRequest request, LaunchImpulse impulse)
        {
            _heldLaunchTarget.SetHeldPosition(request.FinalPullPoint);
            _launchTarget.Launch(impulse.VelocityChange);
        }
    }
}
