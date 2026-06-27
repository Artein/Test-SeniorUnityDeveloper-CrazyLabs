using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ILaunchTargetSilhouetteSource
    {
        bool TryWriteSilhouetteSamples(LaunchTargetSilhouetteQuery query, Vector3[] outputSamples, out int sampleCount);
    }

    public readonly struct LaunchTargetSilhouetteQuery
    {
        public Vector3 PlaneOrigin { get; }
        public Vector3 LaunchFrameRight { get; }
        public Vector3 LaunchFrameForward { get; }
        public Vector3 LaunchFrameUp { get; }
        public int SampleCount { get; }

        public LaunchTargetSilhouetteQuery(
            Vector3 planeOrigin,
            Vector3 launchFrameRight,
            Vector3 launchFrameForward,
            Vector3 launchFrameUp,
            int sampleCount)
        {
            PlaneOrigin = planeOrigin;
            LaunchFrameRight = launchFrameRight;
            LaunchFrameForward = launchFrameForward;
            LaunchFrameUp = launchFrameUp;
            SampleCount = sampleCount;
        }
    }
}
