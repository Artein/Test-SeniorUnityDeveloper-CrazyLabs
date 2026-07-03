using System;
using System.Collections;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using Game.Foundation.Input;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneSlingshotInputTests : BaseGameplayScenePlayModeFixture
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsSlingshot_then_PlayerLaunches()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Smoke Mouse");

        try
        {
            yield return PrepareFreshPreLaunchScene();
            var activeScene = SceneManager.GetActiveScene();

            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var runPreparationCamera = FindGameObjectByName(activeScene, "Run Preparation Camera").GetComponent<CinemachineCamera>();
            var preLaunchCamera = FindGameObjectByName(activeScene, "Pre-Launch Camera").GetComponent<CinemachineCamera>();
            var runCamera = FindGameObjectByName(activeScene, "Run Camera").GetComponent<CinemachineCamera>();
            var runCameraAnchor = FindSingleInScene<TransformRunCameraAnchor>(activeScene, "Run Camera Anchor");
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
            var slingshotConfig = lifetimeScope.Container.Resolve<ISlingshotConfig>();
            var runCameraConfig = lifetimeScope.Container.Resolve<IRunCameraConfig>();
            var launchAppliedNotifier = lifetimeScope.Container.Resolve<ISlingshotLaunchAppliedNotifier>();

            var pullWorldPosition = geometry.RestPoint
                                    + (geometry.LaunchFrameRight * 0.35f)
                                    - (geometry.LaunchFrameForward * 1.25f);
            var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

            var pointerPressedCount = 0;
            var unityInput = lifetimeScope.Container.Resolve<IUnityInput>();
            var hasLaunchApplied = false;
            var capturedLaunch = default(SlingshotLaunchAppliedEvent);

            void OnPointerPressed(PointerInput _)
            {
                pointerPressedCount += 1;
            }

            void OnLaunchApplied(SlingshotLaunchAppliedEvent launch)
            {
                hasLaunchApplied = true;
                capturedLaunch = launch;
            }

            unityInput.PointerPressed += OnPointerPressed;
            launchAppliedNotifier.LaunchApplied += OnLaunchApplied;

            try
            {
                yield return SendMouse(mouse, pressScreenPosition, true);

                Assert.That(pointerPressedCount, Is.EqualTo(1));

                yield return SendMouse(mouse, releaseScreenPosition, true);

                Assert.That(pullHint.activeSelf, Is.False);
                Assert.That(touchIndicator.activeSelf, Is.True);
                Assert.That(bandLineRenderer.positionCount, Is.GreaterThan(3));

                AssertActivePullPresentation(
                    bandCenter.transform.position,
                    geometry,
                    slingshotConfig,
                    expectedPullSide: 1f);

                yield return SendMouse(mouse, releaseScreenPosition, false);
                yield return WaitUntilPlayerLaunches(playerRigidbody);

                Assert.That(hasLaunchApplied, Is.True, "Expected Slingshot launch to publish its applied launch payload.");
                Assert.That(capturedLaunch.Request.PullDistance, Is.GreaterThan(slingshotConfig.MinimumPullDistance));
                Assert.That(capturedLaunch.VelocityChange.magnitude, Is.GreaterThan(6f));
                Assert.That(playerRigidbody.isKinematic, Is.False);
                Assert.That(runCamera.Priority.Value, Is.GreaterThan(runPreparationCamera.Priority.Value));
                Assert.That(runCamera.Priority.Value, Is.GreaterThan(preLaunchCamera.Priority.Value));

                AssertRunCameraAnchorTracksLaunchedPlayer(runCameraAnchor, playerRigidbody, runCameraConfig);
            }
            finally
            {
                unityInput.PointerPressed -= OnPointerPressed;
                launchAppliedNotifier.LaunchApplied -= OnLaunchApplied;
            }
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePressesOutsideBand_then_PlayerStaysHeld()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Outside Band Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return ContinueToPreLaunch(activeScene);
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var geometry = slingshotView.CreateGeometrySnapshot();
            var outsideBandScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint) + new Vector2(0f, 220f);
            var validPullWorldPosition = geometry.RestPoint - (geometry.LaunchFrameForward * 1.25f);
            var validPullScreenPosition = GetScreenPosition(inputCamera, validPullWorldPosition);

            yield return SendMouse(mouse, outsideBandScreenPosition, true);

            yield return SendMouse(mouse, validPullScreenPosition, true);

            Assert.That(pullHint.activeSelf, Is.False);
            Assert.That(touchIndicator.activeSelf, Is.False);

            yield return SendMouse(mouse, validPullScreenPosition, false);
            yield return AssertPlayerRemainsHeld(playerRigidbody, 10);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMouseReleasesWeakPull_then_PlayerStaysHeld()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Weak Pull Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return ContinueToPreLaunch(activeScene);
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
            var weakPullWorldPosition = geometry.RestPoint - (geometry.LaunchFrameForward * 0.1f);
            var weakPullScreenPosition = GetScreenPosition(inputCamera, weakPullWorldPosition);

            yield return SendMouse(mouse, pressScreenPosition, true);

            yield return SendMouse(mouse, weakPullScreenPosition, true);

            Assert.That(touchIndicator.activeSelf, Is.True);

            yield return SendMouse(mouse, weakPullScreenPosition, false);
            yield return AssertPlayerRemainsHeld(playerRigidbody, 10);

            Assert.That(bandCenter.transform.position.x, Is.EqualTo(geometry.RestPoint.x).Within(0.05f));
            Assert.That(bandCenter.transform.position.z, Is.EqualTo(geometry.RestPoint.z).Within(0.05f));
            Assert.That(touchIndicator.activeSelf, Is.False);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsForward_then_PlayerStaysHeld()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Forward Pull Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return ContinueToPreLaunch(activeScene);
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
            var forwardPullWorldPosition = geometry.RestPoint + (geometry.LaunchFrameForward * 1f);
            var forwardPullScreenPosition = GetScreenPosition(inputCamera, forwardPullWorldPosition);

            yield return SendMouse(mouse, pressScreenPosition, true);

            yield return SendMouse(mouse, forwardPullScreenPosition, true);

            Assert.That(bandLineRenderer.positionCount, Is.GreaterThan(3));
            Assert.That(bandCenter.transform.position.x, Is.EqualTo(geometry.RestPoint.x).Within(0.05f));
            Assert.That(bandCenter.transform.position.z, Is.EqualTo(geometry.RestPoint.z).Within(0.05f));

            yield return SendMouse(mouse, forwardPullScreenPosition, false);
            yield return AssertPlayerRemainsHeld(playerRigidbody, 10);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsLeftAndRight_then_LaunchDirectionSteersWithoutPowerChange()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Steering Mouse");
        var rightPullLaunch = default(SlingshotLaunchAppliedEvent);
        var leftPullLaunch = default(SlingshotLaunchAppliedEvent);

        try
        {
            yield return LaunchAndCaptureAppliedLaunch(mouse, 0.75f, 1.25f, launch => rightPullLaunch = launch);
            yield return LaunchAndCaptureAppliedLaunch(mouse, -0.75f, 1.25f, launch => leftPullLaunch = launch);

            Assert.That(rightPullLaunch.Request.PullDistance, Is.EqualTo(leftPullLaunch.Request.PullDistance).Within(0.01f));
            Assert.That(rightPullLaunch.VelocityChange.x, Is.LessThan(-0.5f));
            Assert.That(leftPullLaunch.VelocityChange.x, Is.GreaterThan(0.5f));
            Assert.That(rightPullLaunch.VelocityChange.magnitude, Is.EqualTo(leftPullLaunch.VelocityChange.magnitude).Within(0.2f));
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator LoadGameplayScene()
    {
        yield return LoadGameplaySceneWithIsolatedSaves(CanReuseGameplayScene);
    }

    private bool CanReuseGameplayScene(Scene scene)
    {
        if (!scene.IsValid() || scene.path != TestAssets.GameplaySceneRef.Path)
            return false;

        var slingshotViews = FindComponentsInScene<SlingshotView>(scene);
        var launchTargets = FindComponentsInScene<RigidbodyLaunchTarget>(scene);

        if (slingshotViews.Length != 1 || launchTargets.Length != 1)
            return false;

        var playerRigidbody = launchTargets[0].GetComponent<Rigidbody>();

        if (playerRigidbody == null || !playerRigidbody.isKinematic)
            return false;

        if (!TryFindGameObjectByName(scene, "Band Center", out var bandCenter))
            return false;

        if (!TryFindGameObjectByName(scene, "Pull Hint", out var pullHint) || pullHint.activeSelf)
            return false;

        if (!TryFindGameObjectByName(scene, "Touch Indicator", out var touchIndicator) || touchIndicator.activeSelf)
            return false;

        var geometry = slingshotViews[0].CreateGeometrySnapshot();
        return Vector3.Distance(bandCenter.transform.position, geometry.RestPoint) <= 0.05f;
    }

    private IEnumerator PrepareFreshPreLaunchScene()
    {
        yield return ReloadGameplaySceneWithIsolatedSaves();

        var activeScene = SceneManager.GetActiveScene();
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var gameplayStateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
        var snapshotProvider = lifetimeScope.Container.Resolve<IRunModifierSnapshotProvider>();
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();

        Assert.That(gameplayStateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
        Assert.That(snapshotProvider.CurrentSnapshot.Modifiers, Is.Empty);
        Assert.That(continueCommand.TryContinue(), Is.True);

        yield return null;

        Assert.That(gameplayStateService.CurrentStateId.name, Is.EqualTo("PreLaunchStateId"));
        Assert.That(snapshotProvider.CurrentSnapshot.Modifiers, Is.Empty);

        yield return WaitUntilPlayerIsHeld(activeScene);
        AssertCleanSlingshotRestState(activeScene);
    }

    private void AssertCleanSlingshotRestState(Scene scene)
    {
        var slingshotView = FindSingleInScene<SlingshotView>(scene, "SlingshotView");
        var bandCenter = FindGameObjectByName(scene, "Band Center");
        var geometry = slingshotView.CreateGeometrySnapshot();

        Assert.That(Vector3.Distance(bandCenter.transform.position, geometry.RestPoint), Is.LessThanOrEqualTo(0.05f));
    }

    private IEnumerator WaitUntilPlayerIsHeld(Scene scene)
    {
        for (var frameIndex = 0; frameIndex < 10; frameIndex += 1)
        {
            var launchTargets = FindComponentsInScene<RigidbodyLaunchTarget>(scene);

            if (launchTargets.Length == 1)
            {
                var rigidbody = launchTargets[0].GetComponent<Rigidbody>();

                if (rigidbody != null && rigidbody.isKinematic)
                    yield break;
            }

            yield return null;
        }

        Assert.Fail("Expected Player to be held by the Slingshot.");
    }

    private IEnumerator ContinueToPreLaunch(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
        continueCommand.TryContinue();
        yield return null;
    }

    private Vector2 GetScreenPosition(Camera camera, Vector3 worldPosition)
    {
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        Assert.That(screenPosition.z, Is.GreaterThan(0f));
        return new Vector2(screenPosition.x, screenPosition.y);
    }

    private IEnumerator SendMouse(Mouse mouse, Vector2 screenPosition, bool isPressed)
    {
        QueueMouse(mouse, screenPosition, isPressed);
        yield break;
    }

    private void QueueMouse(Mouse mouse, Vector2 screenPosition, bool isPressed)
    {
        mouse.MakeCurrent();

        var mouseState = new MouseState
        {
            position = screenPosition
        }.WithButton(MouseButton.Left, isPressed);

        InputSystem.QueueStateEvent(mouse, mouseState);
        InputSystem.Update();
    }

    private IEnumerator LaunchAndCaptureAppliedLaunch(
        Mouse mouse,
        float pullOffset,
        float pullDistance,
        Action<SlingshotLaunchAppliedEvent> captureLaunch)
    {
        yield return PrepareFreshPreLaunchScene();
        var activeScene = SceneManager.GetActiveScene();

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var launchAppliedNotifier = lifetimeScope.Container.Resolve<ISlingshotLaunchAppliedNotifier>();
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
        var hasLaunchApplied = false;
        var capturedLaunch = default(SlingshotLaunchAppliedEvent);

        void OnLaunchApplied(SlingshotLaunchAppliedEvent launch)
        {
            hasLaunchApplied = true;
            capturedLaunch = launch;
        }

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDistance);
        var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        launchAppliedNotifier.LaunchApplied += OnLaunchApplied;

        try
        {
            yield return SendMouse(mouse, pressScreenPosition, true);

            yield return SendMouse(mouse, releaseScreenPosition, true);

            yield return SendMouse(mouse, releaseScreenPosition, false);
            yield return WaitUntilPlayerLaunches(playerRigidbody);

            Assert.That(hasLaunchApplied, Is.True, "Expected Slingshot launch to publish its applied launch payload.");
            captureLaunch.Invoke(capturedLaunch);
        }
        finally
        {
            launchAppliedNotifier.LaunchApplied -= OnLaunchApplied;
        }
    }

    private void AssertActivePullPresentation(
        Vector3 actualBandCenter,
        SlingshotGeometrySnapshot geometry,
        ISlingshotConfig slingshotConfig,
        float expectedPullSide)
    {
        var actualOffset = Vector3.Dot(actualBandCenter - geometry.RestPoint, geometry.LaunchFrameRight);
        var actualDepth = Vector3.Dot(actualBandCenter - geometry.RestPoint, -geometry.LaunchFrameForward);

        Assert.That(
            actualDepth,
            Is.GreaterThan(slingshotConfig.MinimumPullDistance),
            "Active pull should move Band Center behind the rest point past the launch threshold.");

        Assert.That(
            actualDepth,
            Is.LessThanOrEqualTo(slingshotConfig.MaximumPullDistance + 0.05f),
            "Active pull should stay within the configured maximum pull depth.");

        Assert.That(
            Mathf.Abs(actualOffset),
            Is.GreaterThan(0.05f),
            "Active pull should visibly move Band Center sideways.");

        Assert.That(
            Mathf.Sign(actualOffset),
            Is.EqualTo(Mathf.Sign(expectedPullSide)),
            "Active pull should move Band Center to the requested side.");
    }

    private IEnumerator AssertPlayerRemainsHeld(Rigidbody playerRigidbody, int frameCount)
    {
        for (var frameIndex = 0; frameIndex < frameCount; frameIndex += 1)
        {
            AssertPlayerIsHeld(playerRigidbody);
            yield return null;
        }

        AssertPlayerIsHeld(playerRigidbody);
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

    private void AssertRunCameraAnchorTracksLaunchedPlayer(
        TransformRunCameraAnchor runCameraAnchor,
        Rigidbody playerRigidbody,
        IRunCameraConfig runCameraConfig)
    {
        var targetPosition = playerRigidbody.position + runCameraConfig.AnchorOffset;
        var distance = Vector3.Distance(runCameraAnchor.transform.position, targetPosition);
        var launchFrameDuration = Mathf.Max(Time.fixedDeltaTime, Time.deltaTime);
        var launchMotionAllowance = playerRigidbody.linearVelocity.magnitude * launchFrameDuration;
        var allowedDistance = 0.75f + launchMotionAllowance;

        Assert.That(
            distance,
            Is.LessThan(allowedDistance),
            "Run camera anchor should be within the legacy tracking tolerance plus one launched frame of motion.");
    }

    private void AssertPlayerIsHeld(Rigidbody playerRigidbody)
    {
        Assert.That(playerRigidbody.isKinematic, Is.True);
        Assert.That(playerRigidbody.linearVelocity.sqrMagnitude, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(playerRigidbody.angularVelocity.sqrMagnitude, Is.EqualTo(0f).Within(0.0001f));
    }
}
