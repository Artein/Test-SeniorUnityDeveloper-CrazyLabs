using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public readonly struct SlingshotLaunchAppliedEvent
    {
        public SlingshotLaunchRequest Request { get; }
        public Vector3 VelocityChange { get; }
        public Vector3 LaunchDirection { get; }
        public Vector3 LaunchUpDirection { get; }
        
        public SlingshotLaunchAppliedEvent(
            SlingshotLaunchRequest request,
            Vector3 velocityChange,
            Vector3 launchDirection,
            Vector3 launchUpDirection)
        {
            Request = request;
            VelocityChange = velocityChange;
            LaunchDirection = launchDirection;
            LaunchUpDirection = launchUpDirection;
        }
    }

    public interface ISlingshotLaunchAppliedPublisher
    {
        void Publish(SlingshotLaunchAppliedEvent appliedEvent);
    }
}
