using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;
using static GameplaySceneBandShapePlayModeTestUtils;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneFinishPresentationFlowTests : BaseGameplayScenePlayModeFixture
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_SuccessfulFinishIsAccepted_then_FinishPresentationPlaysOnceAndResets()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Finish Presentation Success Mouse");
        var acceptedResults = new List<RunResult>();
        IRunResultNotifier runResultNotifier = null;

        try
        {
            yield return ReloadGameplaySceneWithIsolatedSavesAndContinueToPreLaunch();
            var activeScene = SceneManager.GetActiveScene();
            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var context = CreateSceneContext(activeScene);
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var runEndCandidateReceiver = lifetimeScope.Container.Resolve<IRunEndCandidateReceiver>();
            var acknowledgeCommand = lifetimeScope.Container.Resolve<IRunResultAcknowledgeCommand>();
            var finishPresentationView = FindSingleInScene<FinishPresentationView>(activeScene, "FinishPresentationView");
            var characterPresentationView = FindSingleInScene<CharacterPresentationView>(activeScene, "CharacterPresentationView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var runCameraLens = lifetimeScope.Container.Resolve<IRunCameraLens>();
            runResultNotifier = lifetimeScope.Container.Resolve<IRunResultNotifier>();
            runResultNotifier.RunResultAccepted += acceptedResults.Add;

            AssertFinishPresentationReady(finishPresentationView);

            yield return LaunchFromPreLaunch(mouse, context, stateService);
            yield return WaitUntilPlayerMovesAwayFrom(context.PlayerRigidbody, context.Geometry.RestPoint, 0.5f, 60);

            LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=Finished, IsSuccess=True"));
            runEndCandidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
            yield return WaitUntilStateName(stateService, "RunEndedStateId", 60);
            runEndCandidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.Finished));
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.That(acceptedResults, Has.Count.EqualTo(1));
            Assert.That(acceptedResults[0].Reason, Is.EqualTo(RunEndReason.Finished));
            Assert.That(acceptedResults[0].IsSuccess, Is.True);
            Assert.That(acknowledgeCommand.TryAcknowledge(), Is.False);
            yield return WaitUntilAnySuccessParticleIsPlaying(finishPresentationView, 10);

            yield return WaitUntilVisualFacesRunCamera(
                characterPresentationView.VisualAnchorForTests,
                launchTarget.transform,
                runCameraLens,
                45f,
                60);
            yield return WaitUntilThresholdHidden(finishPresentationView, 90);

            yield return AcknowledgeRunEndAfterGuard(acknowledgeCommand, 30);
            yield return WaitUntilStateName(stateService, "RunPreparationStateId", 120);
            AssertFinishPresentationReady(finishPresentationView);
            yield return WaitUntilVisualMatchesRunBody(characterPresentationView.VisualAnchorForTests, launchTarget.transform, 10);
        }
        finally
        {
            if (runResultNotifier != null)
                runResultNotifier.RunResultAccepted -= acceptedResults.Add;

            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_FailedFinishResultIsAccepted_then_FinishPresentationStaysQuiet()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Finish Presentation Failure Mouse");
        var acceptedResults = new List<RunResult>();
        IRunResultNotifier runResultNotifier = null;

        try
        {
            yield return ReloadGameplaySceneWithIsolatedSavesAndContinueToPreLaunch();
            var activeScene = SceneManager.GetActiveScene();
            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var context = CreateSceneContext(activeScene);
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var runEndCandidateReceiver = lifetimeScope.Container.Resolve<IRunEndCandidateReceiver>();
            var finishPresentationView = FindSingleInScene<FinishPresentationView>(activeScene, "FinishPresentationView");
            runResultNotifier = lifetimeScope.Container.Resolve<IRunResultNotifier>();
            runResultNotifier.RunResultAccepted += acceptedResults.Add;

            AssertFinishPresentationReady(finishPresentationView);

            yield return LaunchFromPreLaunch(mouse, context, stateService);
            yield return WaitUntilPlayerMovesAwayFrom(context.PlayerRigidbody, context.Geometry.RestPoint, 0.5f, 60);

            LogAssert.Expect(LogType.Log, new Regex("Run Result: Reason=ObstacleHit, IsSuccess=False"));
            runEndCandidateReceiver.SubmitCandidate(new RunEndCandidate(RunEndReason.ObstacleHit));
            yield return WaitUntilStateName(stateService, "RunEndedStateId", 60);
            yield return AssertFinishPresentationStaysReady(finishPresentationView, 12);

            Assert.That(acceptedResults, Has.Count.EqualTo(1));
            Assert.That(acceptedResults[0].Reason, Is.EqualTo(RunEndReason.ObstacleHit));
            Assert.That(acceptedResults[0].IsSuccess, Is.False);
        }
        finally
        {
            if (runResultNotifier != null)
                runResultNotifier.RunResultAccepted -= acceptedResults.Add;

            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator LaunchFromPreLaunch(
        Mouse mouse,
        GameplaySceneBandShapePlayModeTestContext context,
        IGameplayStateService stateService)
    {
        yield return WaitUntilStateName(stateService, "PreLaunchStateId", 10);
        yield return WaitUntilPlayerIsHeld(context);

        var pullWorldPosition = context.Geometry.RestPoint
                                + (context.Geometry.LaunchFrameRight * 0.35f)
                                - (context.Geometry.LaunchFrameForward * 1.25f);
        var releaseScreenPosition = GetScreenPosition(context.InputCamera, pullWorldPosition);

        yield return SendMouse(mouse, context.PressScreenPosition, true);
        yield return SendMouse(mouse, releaseScreenPosition, true);
        Assert.That(context.BandLineRenderer.positionCount, Is.GreaterThan(3));
        yield return SendMouse(mouse, releaseScreenPosition, false);
        yield return WaitUntilStateName(stateService, "RunningStateId", 60);
        yield return WaitUntilPlayerLaunches(context.PlayerRigidbody);
    }

    private IEnumerator WaitUntilStateName(IGameplayStateService stateService, string stateName, int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (stateService.CurrentStateId != null && stateService.CurrentStateId.name == stateName)
                yield break;

            yield return null;
        }

        Assert.Fail($"Expected Gameplay State '{stateName}', but current state is '{stateService.CurrentStateId?.name ?? "<null>"}'.");
    }

    private IEnumerator WaitUntilPlayerLaunches(Rigidbody playerRigidbody)
    {
        for (var frameIndex = 0; frameIndex < 60; frameIndex += 1)
        {
            if (!playerRigidbody.isKinematic && playerRigidbody.linearVelocity.sqrMagnitude > 0.0001f)
                yield break;

            yield return null;
        }

        Assert.Fail("Expected Slingshot pull release to launch the Player.");
    }

    private IEnumerator WaitUntilPlayerMovesAwayFrom(
        Rigidbody playerRigidbody,
        Vector3 origin,
        float minimumDistance,
        int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (Vector3.Distance(playerRigidbody.position, origin) >= minimumDistance)
                yield break;

            yield return null;
        }

        Assert.Fail($"Expected Player to move at least {minimumDistance:0.###} meters away from the origin.");
    }

    private IEnumerator WaitUntilAnySuccessParticleIsPlaying(FinishPresentationView finishPresentationView, int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (finishPresentationView.SuccessParticlesForTests.Any(IsParticlePlayingOrAlive))
                yield break;

            yield return null;
        }

        Assert.Fail("Expected Finish Presentation success particles to play after accepted successful finish.");
    }

    private IEnumerator WaitUntilThresholdHidden(FinishPresentationView finishPresentationView, int frameLimit)
    {
        var thresholdRenderer = finishPresentationView.ThresholdRendererForTests;

        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (thresholdRenderer != null && !thresholdRenderer.enabled)
                yield break;

            yield return null;
        }

        Assert.Fail("Expected Finish Presentation threshold renderer to fade out after accepted successful finish.");
    }

    private IEnumerator WaitUntilVisualFacesRunCamera(
        Transform visualAnchor,
        Transform runBody,
        IRunCameraLens runCameraLens,
        float maximumAngleDegrees,
        int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            var cameraFacingRotation = GetCameraFacingRotation(runBody, runCameraLens);

            if (Quaternion.Angle(visualAnchor.rotation, cameraFacingRotation) <= maximumAngleDegrees)
                yield break;

            yield return null;
        }

        Assert.Fail("Expected Character visual anchor to blend toward the Run Camera after accepted successful finish.");
    }

    private IEnumerator WaitUntilVisualMatchesRunBody(Transform visualAnchor, Transform runBody, int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (Quaternion.Angle(visualAnchor.rotation, runBody.rotation) <= 0.5f
                && Vector3.Distance(visualAnchor.position, runBody.position) <= 0.01f)
            {
                yield break;
            }

            yield return null;
        }

        Assert.Fail("Expected Character visual anchor to reset to the Run Body pose in RunPreparation.");
    }

    private IEnumerator AcknowledgeRunEndAfterGuard(IRunResultAcknowledgeCommand acknowledgeCommand, int fixedFrameLimit)
    {
        for (var frameIndex = 0; frameIndex < fixedFrameLimit; frameIndex += 1)
        {
            if (acknowledgeCommand.TryAcknowledge())
                yield break;

            yield return new WaitForFixedUpdate();
        }

        Assert.Fail("Expected Run End acknowledgement guard to elapse.");
    }

    private IEnumerator SendMouse(Mouse mouse, Vector2 screenPosition, bool isPressed)
    {
        mouse.MakeCurrent();

        var mouseState = new MouseState
        {
            position = screenPosition
        }.WithButton(MouseButton.Left, isPressed);

        InputSystem.QueueStateEvent(mouse, mouseState);
        InputSystem.Update();
        yield break;
    }

    private IEnumerator AssertFinishPresentationStaysReady(FinishPresentationView finishPresentationView, int frameCount)
    {
        for (var frameIndex = 0; frameIndex < frameCount; frameIndex += 1)
        {
            AssertFinishPresentationReady(finishPresentationView);
            yield return null;
        }
    }

    private void AssertFinishPresentationReady(FinishPresentationView finishPresentationView)
    {
        var thresholdRenderer = finishPresentationView.ThresholdRendererForTests;
        Assert.That(thresholdRenderer, Is.Not.Null);
        Assert.That(thresholdRenderer.enabled, Is.True);

        foreach (var particle in finishPresentationView.SuccessParticlesForTests)
        {
            Assert.That(particle, Is.Not.Null);
            Assert.That(IsParticlePlayingOrAlive(particle), Is.False, particle.name);
        }
    }

    private static bool IsParticlePlayingOrAlive(ParticleSystem particle)
    {
        return particle != null && (particle.isPlaying || particle.particleCount > 0);
    }

    private static Quaternion GetCameraFacingRotation(Transform runBody, IRunCameraLens runCameraLens)
    {
        var up = runBody.rotation * Vector3.up;
        var directionToCamera = Vector3.ProjectOnPlane(runCameraLens.Position - runBody.position, up);

        Assert.That(directionToCamera.sqrMagnitude, Is.GreaterThan(Mathf.Epsilon));
        return Quaternion.LookRotation(directionToCamera.normalized, up);
    }
}
