using UnityEngine;

namespace Game.Gameplay.CharacterPresentation
{
    public readonly struct CharacterPresentationClassificationInput
    {
        public CharacterPresentationMode CurrentMode { get; }
        public float CurrentModeElapsedSeconds { get; }
        public float UngroundedElapsedSeconds { get; }
        public bool IsPreLaunch { get; }
        public bool IsRunActive { get; }
        public bool HasAcceptedRunResult { get; }
        public bool AcceptedRunResultSucceeded { get; }
        public bool HasActivePull { get; }
        public bool HasLaunchPush { get; }
        public float LaunchPushElapsedSeconds { get; }
        public RunSurfaceContext SurfaceContext { get; }
        public float CoursePlanarSpeed { get; }
        public float CourseForwardSpeed { get; }
        public Vector3 LinearVelocity { get; }

        public CharacterPresentationClassificationInput(
            CharacterPresentationMode currentMode,
            float currentModeElapsedSeconds,
            float ungroundedElapsedSeconds,
            bool isPreLaunch,
            bool isRunActive,
            bool hasAcceptedRunResult,
            bool acceptedRunResultSucceeded,
            bool hasActivePull,
            bool hasLaunchPush,
            float launchPushElapsedSeconds,
            RunSurfaceContext surfaceContext,
            float coursePlanarSpeed,
            float courseForwardSpeed,
            Vector3 linearVelocity)
        {
            CurrentMode = currentMode;
            CurrentModeElapsedSeconds = Mathf.Max(0f, currentModeElapsedSeconds);
            UngroundedElapsedSeconds = Mathf.Max(0f, ungroundedElapsedSeconds);
            IsPreLaunch = isPreLaunch;
            IsRunActive = isRunActive;
            HasAcceptedRunResult = hasAcceptedRunResult;
            AcceptedRunResultSucceeded = acceptedRunResultSucceeded;
            HasActivePull = hasActivePull;
            HasLaunchPush = hasLaunchPush;
            LaunchPushElapsedSeconds = float.IsFinite(launchPushElapsedSeconds) ? Mathf.Max(0f, launchPushElapsedSeconds) : 0f;
            SurfaceContext = surfaceContext;
            CoursePlanarSpeed = float.IsFinite(coursePlanarSpeed) ? coursePlanarSpeed : 0f;
            CourseForwardSpeed = float.IsFinite(courseForwardSpeed) ? courseForwardSpeed : 0f;
            LinearVelocity = linearVelocity;
        }
    }
}
