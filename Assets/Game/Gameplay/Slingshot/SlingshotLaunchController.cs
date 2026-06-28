using System;
using Game.Utils.Invocation;
using Game.Utils.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotLauncher
    {
        void Launch(SlingshotLaunchRequest request);
    }

    public interface ISlingshotLaunchAppliedNotifier
    {
        event Action<SlingshotLaunchRequest> LaunchApplied;
    }

    public sealed class SlingshotLaunchController : ISlingshotLauncher, ISlingshotLaunchAppliedNotifier
    {
        private readonly ILaunchTarget _launchTarget;
        private readonly IHeldLaunchTarget _heldLaunchTarget;

        public event Action<SlingshotLaunchRequest> LaunchApplied;

        public SlingshotLaunchController(
            ILaunchTarget launchTarget,
            IHeldLaunchTarget heldLaunchTarget)
        {
            _launchTarget = launchTarget ?? throw new ArgumentNullException(nameof(launchTarget));
            _heldLaunchTarget = heldLaunchTarget ?? throw new ArgumentNullException(nameof(heldLaunchTarget));
        }

        public void Launch(SlingshotLaunchRequest request)
        {
            if (!IsValidRequest(request))
            {
                Debug.LogWarning("Invalid Slingshot launch request. Launch skipped.");
                return;
            }

            var finalVelocity = (request.LaunchDirection * request.LaunchSpeed)
                                + (request.LaunchUpDirection * request.LaunchUpSpeed);

            if (!finalVelocity.IsFinite() || finalVelocity.sqrMagnitude <= 0.000001f)
            {
                Debug.LogWarning("Invalid Slingshot final velocity. Launch skipped.");
                return;
            }

            _heldLaunchTarget.SetHeldPosition(request.FinalPullPoint);
            _launchTarget.Launch(finalVelocity);
            LaunchApplied?.InvokeSafely(request);
        }

        private bool IsValidRequest(SlingshotLaunchRequest request)
        {
            return request.LaunchDirection.IsFinite()
                   && request.LaunchDirection.IsApproximatelyUnit()
                   && request.FinalPullPoint.IsFinite()
                   && request.LaunchUpDirection.IsFinite()
                   && request.LaunchUpDirection.IsApproximatelyUnit()
                   && math.isfinite(request.LaunchSpeed)
                   && request.LaunchSpeed >= 0f
                   && math.isfinite(request.LaunchUpSpeed)
                   && request.LaunchUpSpeed >= 0f;
        }
    }
}
