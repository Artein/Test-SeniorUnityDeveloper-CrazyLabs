using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotActivePullNotifier
    {
        event Action<SlingshotActivePullContext> ActivePullChanged;
        event Action ActivePullCleared;
    }

    public interface ISlingshotCaptureLifecycleNotifier
    {
        event Action CaptureEnabled;
        event Action CaptureDisabled;
    }

    public interface ISlingshotGeometrySnapshotSource
    {
        SlingshotGeometrySnapshot CurrentGeometry { get; }
    }

    public interface ISlingshotPresentationContextSource
    {
        SlingshotPresentationContext Current { get; }
    }

    public readonly struct SlingshotActivePullContext
    {
        public float NormalizedPull { get; }
        public float NormalizedPullOffset { get; }

        public SlingshotActivePullContext(float normalizedPull, float normalizedPullOffset)
        {
            NormalizedPull = math.isfinite(normalizedPull) ? Mathf.Clamp01(normalizedPull) : 0f;
            NormalizedPullOffset = math.isfinite(normalizedPullOffset) ? Mathf.Clamp(normalizedPullOffset, -1f, 1f) : 0f;
        }
    }

    public readonly struct SlingshotPresentationContext
    {
        public bool HasActivePull { get; }
        public float NormalizedPull { get; }
        public float NormalizedPullOffset { get; }
        public bool HasLaunchPush { get; }
        public float LaunchPushElapsedSeconds { get; }
        public float NormalizedLaunchPower { get; }
        public float NormalizedLaunchOffset { get; }

        public SlingshotPresentationContext(
            bool hasActivePull,
            float normalizedPull,
            float normalizedPullOffset,
            bool hasLaunchPush,
            float launchPushElapsedSeconds,
            float normalizedLaunchPower,
            float normalizedLaunchOffset)
        {
            HasActivePull = hasActivePull;
            NormalizedPull = hasActivePull && math.isfinite(normalizedPull) ? Mathf.Clamp01(normalizedPull) : 0f;
            NormalizedPullOffset = hasActivePull && math.isfinite(normalizedPullOffset) ? Mathf.Clamp(normalizedPullOffset, -1f, 1f) : 0f;
            HasLaunchPush = hasLaunchPush;
            LaunchPushElapsedSeconds = hasLaunchPush && math.isfinite(launchPushElapsedSeconds) ? Mathf.Max(0f, launchPushElapsedSeconds) : 0f;
            NormalizedLaunchPower = hasLaunchPush && math.isfinite(normalizedLaunchPower) ? Mathf.Clamp01(normalizedLaunchPower) : 0f;
            NormalizedLaunchOffset = hasLaunchPush && math.isfinite(normalizedLaunchOffset) ? Mathf.Clamp(normalizedLaunchOffset, -1f, 1f) : 0f;
        }
    }
}
