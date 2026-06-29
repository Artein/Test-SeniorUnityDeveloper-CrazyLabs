using System;
using Game.Gameplay.Slingshot;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IPreLaunchRigPoseResetter
    {
        void ResetToPreLaunchRigPose();
    }

    public sealed class PreLaunchRigPoseResetter : IPreLaunchRigPoseResetter
    {
        private readonly Transform _slingshotRig;
        private readonly Transform _slingshotRigPose;
        private readonly ILaunchTargetPreLaunchReset _launchTarget;
        private readonly Transform _launchTargetPose;

        public PreLaunchRigPoseResetter(
            Transform slingshotRig,
            Transform slingshotRigPose,
            ILaunchTargetPreLaunchReset launchTarget,
            Transform launchTargetPose)
        {
            _slingshotRig = slingshotRig != null ? slingshotRig : throw new ArgumentNullException(nameof(slingshotRig));
            _slingshotRigPose = slingshotRigPose != null ? slingshotRigPose : throw new ArgumentNullException(nameof(slingshotRigPose));
            _launchTarget = launchTarget ?? throw new ArgumentNullException(nameof(launchTarget));
            _launchTargetPose = launchTargetPose != null ? launchTargetPose : throw new ArgumentNullException(nameof(launchTargetPose));
        }

        void IPreLaunchRigPoseResetter.ResetToPreLaunchRigPose()
        {
            _slingshotRig.SetPositionAndRotation(_slingshotRigPose.position, _slingshotRigPose.rotation);
            _launchTarget.ResetToPreLaunchPose(_launchTargetPose.position, _launchTargetPose.rotation);
        }
    }
}
