using System.Collections;
using System.Collections.Generic;
using System.Text;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;
using static GameplaySceneBandShapePlayModeTestUtils;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneCharacterPresentationVisualTests : BaseGameplayScenePlayModeFixture
{
    private const string PresentationModeParameterName = "PresentationMode";
    private const string PlaybackSpeedParameterName = "PlaybackSpeedMultiplier";
    private const int PostLaunchObservationFrameCount = 180;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_RepresentativePullLaunches_then_CharacterPresentationDoesNotExposeReservedRun()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Character Presentation Visual Smoke Mouse");

        try
        {
            yield return ReloadGameplaySceneWithIsolatedSavesAndContinueToPreLaunch();
            var activeScene = SceneManager.GetActiveScene();
            var context = CreateSceneContext(activeScene);
            var surfaceFrameSource = ResolveSurfaceFrameSource(activeScene);
            var slingshotPresentationContextSource = ResolveSlingshotPresentationContextSource(activeScene);
            var characterAnimator = FindCharacterAnimator(activeScene);
            var visualAnchor = FindGameObjectByName(activeScene, "CharacterVisualAnchor");

            yield return WaitUntilPlayerIsHeld(context);
            yield return null;

            Assert.That(characterAnimator.applyRootMotion, Is.False);
            Assert.That(characterAnimator.transform.IsChildOf(visualAnchor.transform), Is.True);
            Assert.That(visualAnchor.transform.IsChildOf(context.PlayerRigidbody.transform), Is.False);
            AssertPresentationModeIsNotRun(characterAnimator, "pre-launch idle");

            var pullScreenPosition = GetPullScreenPosition(context, pullOffset: 0.35f, pullDepth: 1.25f);
            yield return SendMouse(mouse, context.PressScreenPosition, true);
            yield return SendMouse(mouse, pullScreenPosition, true);
            yield return WaitForPresentationMode(characterAnimator, CharacterPresentationMode.PullAnticipation, "active pull");
            AssertPresentationModeIsNotRun(characterAnimator, "active pull");

            yield return SendMouse(mouse, pullScreenPosition, false);
            yield return WaitUntilPlayerLaunches(context.PlayerRigidbody);

            yield return AssertPostLaunchPresentation(
                characterAnimator,
                context.PlayerRigidbody,
                surfaceFrameSource,
                slingshotPresentationContextSource);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_LadybugAnimator_when_AirborneTransitionReceivesSlideAgain_then_TransitionRedirectsToSlide()
    {
        yield return ReloadGameplaySceneWithIsolatedSavesAndContinueToPreLaunch();
        var activeScene = SceneManager.GetActiveScene();
        var sourceAnimator = FindCharacterAnimator(activeScene);
        var probe = Object.Instantiate(sourceAnimator.gameObject);

        probe.name = "Ladybug Animator Interruption Probe";
        probe.SetActive(false);
        probe.transform.position += Vector3.right * 1000f;

        var animator = probe.GetComponent<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.updateMode = AnimatorUpdateMode.Normal;

        var trace = new StringBuilder();

        try
        {
            probe.SetActive(true);
            animator.Rebind();
            animator.Update(0f);
            animator.Play(CharacterPresentationMode.Slide.ToString(), layer: 0, normalizedTime: 0f);
            SetPresentationMode(animator, CharacterPresentationMode.Slide);
            animator.Update(0f);

            var initial = CaptureAnimatorSnapshot(animator, "initial Slide", trace);
            AssertSnapshotState(initial, CharacterPresentationMode.Slide, hasTransition: false, trace);

            SetPresentationMode(animator, CharacterPresentationMode.Airborne);
            AdvanceAnimator(animator, seconds: 0.02f);

            var airborneRequested = CaptureAnimatorSnapshot(animator, "0.02s after Airborne request", trace);
            AssertSnapshotState(airborneRequested, CharacterPresentationMode.Slide, CharacterPresentationMode.Airborne, trace);

            SetPresentationMode(animator, CharacterPresentationMode.Slide);
            AdvanceAnimator(animator, seconds: 0.02f);

            var slideRequestedDuringBlend = CaptureAnimatorSnapshot(animator, "0.02s after Slide request during Airborne blend", trace);
            AssertSnapshotState(slideRequestedDuringBlend, CharacterPresentationMode.Slide, CharacterPresentationMode.Slide, trace);

            AdvanceAnimator(animator, seconds: 0.12f);

            var recovered = CaptureAnimatorSnapshot(animator, "after recovery time", trace);
            TestContext.Out.WriteLine(trace.ToString());
            AssertSnapshotState(recovered, CharacterPresentationMode.Slide, hasTransition: false, trace);
        }
        finally
        {
            Object.Destroy(probe);
        }
    }

    private static IEnumerator AssertPostLaunchPresentation(
        Animator characterAnimator,
        Rigidbody playerRigidbody,
        IRunSurfaceFrameSource surfaceFrameSource,
        ISlingshotPresentationContextSource slingshotPresentationContextSource)
    {
        var sawLaunchFlight = false;
        var sawSlideAfterLaunchFlight = false;
        var observedMovingGroundedFrames = 0;
        var modeCounts = new Dictionary<CharacterPresentationMode, int>();
        var trace = new StringBuilder();
        var previousMode = CharacterPresentationMode.Idle;

        for (var frameIndex = 0; frameIndex < PostLaunchObservationFrameCount; frameIndex += 1)
        {
            yield return null;

            var mode = GetPresentationMode(characterAnimator);
            var surfaceContext = surfaceFrameSource.Current.ObservedSupport.SurfaceContext;
            var slingshotContext = slingshotPresentationContextSource.Current;

            if (!modeCounts.TryAdd(mode, 1))
                modeCounts[mode] += 1;

            if (frameIndex == 0 || mode != previousMode || frameIndex % 30 == 0)
            {
                _ = trace.Append("frame ")
                    .Append(frameIndex)
                    .Append(": mode=")
                    .Append(mode)
                    .Append(", grounded=")
                    .Append(surfaceContext.IsGrounded)
                    .Append(", hasLaunchPush=")
                    .Append(slingshotContext.HasLaunchPush)
                    .Append(", launchElapsed=")
                    .Append(slingshotContext.LaunchPushElapsedSeconds.ToString("0.000"))
                    .Append(", speed=")
                    .Append(playerRigidbody.linearVelocity.magnitude.ToString("0.000"))
                    .AppendLine();
            }

            previousMode = mode;
            Assert.That(mode, Is.Not.EqualTo(CharacterPresentationMode.Run), $"frame {frameIndex}");

            Assert.That(mode, Is.Not.EqualTo(CharacterPresentationMode.LaunchPush),
                $"normal slingshot launch should go straight to LaunchFlight, frame {frameIndex}");
            Assert.That(characterAnimator.applyRootMotion, Is.False, $"frame {frameIndex}");

            sawLaunchFlight |= mode == CharacterPresentationMode.LaunchFlight;

            if (mode == CharacterPresentationMode.Slide)
            {
                Assert.That(sawLaunchFlight, Is.True, $"Slide should not appear before LaunchFlight after slingshot release, frame {frameIndex}");
                Assert.That(characterAnimator.GetFloat(PlaybackSpeedParameterName), Is.InRange(0.5f, 1.5f), $"frame {frameIndex}");
                sawSlideAfterLaunchFlight |= sawLaunchFlight;
            }

            if (surfaceContext.IsGrounded && playerRigidbody.linearVelocity.sqrMagnitude > 0.25f)
            {
                observedMovingGroundedFrames += 1;

                if (mode != CharacterPresentationMode.LaunchFlight)
                    Assert.That(mode, Is.EqualTo(CharacterPresentationMode.Slide), $"frame {frameIndex}");
            }
        }

        var diagnostics = CreatePostLaunchDiagnostics(modeCounts, trace);

        Assert.That(
            sawLaunchFlight,
            Is.True,
            $"Expected slingshot-fired airborne motion to use LaunchFlight before first landing.{diagnostics}");

        Assert.That(
            sawSlideAfterLaunchFlight,
            Is.True,
            $"Expected LaunchFlight to resolve back to Slide after valid landing.{diagnostics}");

        TestContext.Out.WriteLine(
            $"Observed {observedMovingGroundedFrames} moving grounded presentation frames during post-launch smoke.");
    }

    private static string CreatePostLaunchDiagnostics(
        Dictionary<CharacterPresentationMode, int> modeCounts,
        StringBuilder trace)
    {
        var message = new StringBuilder();
        _ = message.AppendLine().Append("Mode counts:");

        foreach (var pair in modeCounts)
            _ = message.Append("  ").Append(pair.Key).Append(": ").Append(pair.Value).AppendLine();

        return message.Append("Trace:").AppendLine().Append(trace).ToString();
    }

    private static IEnumerator WaitForPresentationMode(Animator animator, CharacterPresentationMode expectedMode, string phase)
    {
        for (var frameIndex = 0; frameIndex < 10; frameIndex += 1)
        {
            yield return null;

            if (GetPresentationMode(animator) == expectedMode)
                yield break;
        }

        Assert.Fail($"Expected Character Presentation mode {expectedMode} during {phase}, but saw {GetPresentationMode(animator)}.");
    }

    private static IEnumerator WaitUntilPlayerLaunches(Rigidbody playerRigidbody)
    {
        for (var frameIndex = 0; frameIndex < 60; frameIndex += 1)
        {
            if (!playerRigidbody.isKinematic && playerRigidbody.linearVelocity.sqrMagnitude > 0.0001f)
                yield break;

            yield return null;
        }

        Assert.Fail("Expected Slingshot pull release to launch the Player.");
    }

    private static Vector2 GetPullScreenPosition(GameplaySceneBandShapePlayModeTestContext context, float pullOffset, float pullDepth)
    {
        var pullWorldPosition = context.Geometry.RestPoint
                                + (context.Geometry.LaunchFrameRight * pullOffset)
                                - (context.Geometry.LaunchFrameForward * pullDepth);

        return GetScreenPosition(context.InputCamera, pullWorldPosition);
    }

    private IRunSurfaceFrameSource ResolveSurfaceFrameSource(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        return lifetimeScope.Container.Resolve<IRunSurfaceFrameSource>();
    }

    private ISlingshotPresentationContextSource ResolveSlingshotPresentationContextSource(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        return lifetimeScope.Container.Resolve<ISlingshotPresentationContextSource>();
    }

    private Animator FindCharacterAnimator(Scene scene)
    {
        var presentationView = FindSingleInScene<CharacterPresentationView>(scene, "CharacterPresentationView");
        var animator = presentationView.GetComponentInChildren<Animator>(true);

        Assert.That(animator, Is.Not.Null, "CharacterPresentationView should own the visible character Animator.");
        return animator;
    }

    private static CharacterPresentationMode GetPresentationMode(Animator animator)
    {
        return (CharacterPresentationMode)animator.GetInteger(PresentationModeParameterName);
    }

    private static void SetPresentationMode(Animator animator, CharacterPresentationMode mode)
    {
        animator.SetInteger(PresentationModeParameterName, (int)mode);
    }

    private static void AssertPresentationModeIsNotRun(Animator animator, string phase)
    {
        Assert.That(GetPresentationMode(animator), Is.Not.EqualTo(CharacterPresentationMode.Run), phase);
    }

    private static void AdvanceAnimator(Animator animator, float seconds)
    {
        const float StepSeconds = 1f / 60f;
        var remainingSeconds = seconds;

        while (remainingSeconds > 0f)
        {
            var deltaTime = Mathf.Min(StepSeconds, remainingSeconds);
            animator.Update(deltaTime);
            remainingSeconds -= deltaTime;
        }
    }

    private static AnimatorTransitionProbeSnapshot CaptureAnimatorSnapshot(
        Animator animator,
        string label,
        StringBuilder trace)
    {
        var isInTransition = animator.IsInTransition(layerIndex: 0);
        var currentState = animator.GetCurrentAnimatorStateInfo(layerIndex: 0);

        var nextState = isInTransition
            ? animator.GetNextAnimatorStateInfo(layerIndex: 0)
            : default;

        var transitionInfo = isInTransition
            ? animator.GetAnimatorTransitionInfo(layerIndex: 0)
            : default;

        var snapshot = new AnimatorTransitionProbeSnapshot(
            label,
            isInTransition,
            currentState,
            nextState,
            transitionInfo.normalizedTime,
            transitionInfo.anyState);

        _ = trace.AppendLine(snapshot.ToString());
        return snapshot;
    }

    private static void AssertSnapshotState(
        AnimatorTransitionProbeSnapshot snapshot,
        CharacterPresentationMode expectedCurrent,
        bool hasTransition,
        StringBuilder trace)
    {
        Assert.That(snapshot.IsInTransition, Is.EqualTo(hasTransition), trace.ToString());
        Assert.That(StateMatches(snapshot.CurrentState, expectedCurrent), Is.True, trace.ToString());
    }

    private static void AssertSnapshotState(
        AnimatorTransitionProbeSnapshot snapshot,
        CharacterPresentationMode expectedCurrent,
        CharacterPresentationMode expectedNext,
        StringBuilder trace)
    {
        Assert.That(snapshot.IsInTransition, Is.True, trace.ToString());
        Assert.That(StateMatches(snapshot.CurrentState, expectedCurrent), Is.True, trace.ToString());
        Assert.That(StateMatches(snapshot.NextState, expectedNext), Is.True, trace.ToString());
    }

    private static bool StateMatches(AnimatorStateInfo stateInfo, CharacterPresentationMode mode)
    {
        return stateInfo.shortNameHash == Animator.StringToHash(mode.ToString());
    }

    private readonly struct AnimatorTransitionProbeSnapshot
    {
        public string Label { get; }
        public bool IsInTransition { get; }
        public AnimatorStateInfo CurrentState { get; }
        public AnimatorStateInfo NextState { get; }
        public float TransitionNormalizedTime { get; }
        public bool IsAnyStateTransition { get; }

        public AnimatorTransitionProbeSnapshot(
            string label,
            bool isInTransition,
            AnimatorStateInfo currentState,
            AnimatorStateInfo nextState,
            float transitionNormalizedTime,
            bool isAnyStateTransition)
        {
            Label = label;
            IsInTransition = isInTransition;
            CurrentState = currentState;
            NextState = nextState;
            TransitionNormalizedTime = transitionNormalizedTime;
            IsAnyStateTransition = isAnyStateTransition;
        }

        public override string ToString()
        {
            return $"{Label}: inTransition={IsInTransition}, current={ResolveStateName(CurrentState)}, next={ResolveStateName(NextState)}, " +
                   $"transitionNormalizedTime={TransitionNormalizedTime:0.000}, anyState={IsAnyStateTransition}";
        }
    }

    private static string ResolveStateName(AnimatorStateInfo stateInfo)
    {
        if (StateMatches(stateInfo, CharacterPresentationMode.Idle))
            return CharacterPresentationMode.Idle.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.PullAnticipation))
            return CharacterPresentationMode.PullAnticipation.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.LaunchPush))
            return CharacterPresentationMode.LaunchPush.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.Slide))
            return CharacterPresentationMode.Slide.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.Run))
            return CharacterPresentationMode.Run.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.Airborne))
            return CharacterPresentationMode.Airborne.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.Victory))
            return CharacterPresentationMode.Victory.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.Defeat))
            return CharacterPresentationMode.Defeat.ToString();

        if (StateMatches(stateInfo, CharacterPresentationMode.LaunchFlight))
            return CharacterPresentationMode.LaunchFlight.ToString();

        return $"hash:{stateInfo.shortNameHash}";
    }
}
