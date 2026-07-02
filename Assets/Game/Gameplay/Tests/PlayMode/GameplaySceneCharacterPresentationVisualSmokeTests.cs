using System.Collections;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;
using static GameplaySceneBandShapePlayModeTestUtils;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneCharacterPresentationVisualSmokeTests : BaseGameplayScenePlayModeFixture
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
            var surfaceContextSource = ResolveSurfaceContextSource(activeScene);
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
            yield return AssertPostLaunchPresentation(characterAnimator, context.PlayerRigidbody, surfaceContextSource);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private static IEnumerator AssertPostLaunchPresentation(
        Animator characterAnimator,
        Rigidbody playerRigidbody,
        IRunSurfaceContextSource surfaceContextSource)
    {
        var sawLaunchPush = false;
        var observedMovingGroundedFrames = 0;

        for (var frameIndex = 0; frameIndex < PostLaunchObservationFrameCount; frameIndex += 1)
        {
            yield return null;

            var mode = GetPresentationMode(characterAnimator);
            Assert.That(mode, Is.Not.EqualTo(CharacterPresentationMode.Run), $"frame {frameIndex}");
            Assert.That(characterAnimator.applyRootMotion, Is.False, $"frame {frameIndex}");

            sawLaunchPush |= mode == CharacterPresentationMode.LaunchPush;

            if (mode == CharacterPresentationMode.Slide)
                Assert.That(characterAnimator.GetFloat(PlaybackSpeedParameterName), Is.InRange(0.5f, 1.5f), $"frame {frameIndex}");

            if (surfaceContextSource.Current.IsGrounded && playerRigidbody.linearVelocity.sqrMagnitude > 0.25f)
            {
                observedMovingGroundedFrames += 1;
                if (mode != CharacterPresentationMode.LaunchPush)
                    Assert.That(mode, Is.EqualTo(CharacterPresentationMode.Slide), $"frame {frameIndex}");
            }
        }

        Assert.That(sawLaunchPush, Is.True, "Expected launch release to drive the launch-push presentation before sliding.");

        TestContext.Out.WriteLine(
            $"Observed {observedMovingGroundedFrames} moving grounded presentation frames during post-launch smoke.");
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

    private IRunSurfaceContextSource ResolveSurfaceContextSource(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        return lifetimeScope.Container.Resolve<IRunSurfaceContextSource>();
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

    private static void AssertPresentationModeIsNotRun(Animator animator, string phase)
    {
        Assert.That(GetPresentationMode(animator), Is.Not.EqualTo(CharacterPresentationMode.Run), phase);
    }
}
