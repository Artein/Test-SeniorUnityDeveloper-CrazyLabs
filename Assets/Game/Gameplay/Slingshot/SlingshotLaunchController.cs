using System;
using Game.Utils.Invocation;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotLaunchAppliedNotifier
    {
        event Action<SlingshotLaunchAppliedEvent> LaunchApplied;
    }

    public sealed class SlingshotLaunchController : ISlingshotLaunchAppliedNotifier, ISlingshotLaunchAppliedPublisher
    {
        public event Action<SlingshotLaunchAppliedEvent> LaunchApplied;

        public void Publish(SlingshotLaunchAppliedEvent appliedEvent)
        {
            LaunchApplied?.InvokeSafely(appliedEvent);
        }
    }
}
