using System;
using System.Collections;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Slingshot;
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
public sealed class GameplaySceneSlingshotInputTests
{
    // TODO - AI Note: We should load scene via SceneRefernce + EditorAssetProvider instead of scene build index
    private readonly int _gameplaySceneBuildIndex = 0;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsSlingshot_then_PlayerLaunches()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Smoke Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return ContinueToPreLaunch(activeScene);
            yield return WaitUntilPlayerIsHeld(activeScene);

            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
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
            var slingshotInputProjector = lifetimeScope.Container.Resolve<ISlingshotInputProjector>();
            var runCameraConfig = lifetimeScope.Container.Resolve<IRunCameraConfig>();

            var pullWorldPosition = geometry.RestPoint
                                    + (geometry.LaunchFrameRight * 0.35f)
                                    - (geometry.LaunchFrameForward * 1.25f);
            var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

            var expectedPullPoint = GetExpectedClampedPullPoint(
                slingshotInputProjector,
                releaseScreenPosition,
                geometry,
                slingshotConfig);

            var pointerPressedCount = 0;
            var unityInput = lifetimeScope.Container.Resolve<IUnityInput>();
            unityInput.PointerPressed += _ => pointerPressedCount += 1;

            yield return SendMouse(mouse, pressScreenPosition, true);

            Assert.That(pointerPressedCount, Is.EqualTo(1));

            yield return SendMouse(mouse, releaseScreenPosition, true);

            Assert.That(pullHint.activeSelf, Is.False);
            Assert.That(touchIndicator.activeSelf, Is.True);
            Assert.That(bandLineRenderer.positionCount, Is.GreaterThan(3));
            Assert.That(bandCenter.transform.position.x, Is.EqualTo(expectedPullPoint.x).Within(0.05f));
            Assert.That(bandCenter.transform.position.z, Is.EqualTo(expectedPullPoint.z).Within(0.05f));

            yield return SendMouse(mouse, releaseScreenPosition, false);
            yield return WaitUntilPlayerLaunches(playerRigidbody);

            Assert.That(playerRigidbody.isKinematic, Is.False);
            Assert.That(playerRigidbody.linearVelocity.magnitude, Is.GreaterThan(4f));
            Assert.That(runCamera.Priority.Value, Is.GreaterThan(preLaunchCamera.Priority.Value));

            Assert.That(Vector3.Distance(
                    runCameraAnchor.transform.position,
                    playerRigidbody.position + runCameraConfig.AnchorOffset),
                Is.LessThan(0.75f));
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
            yield return WaitFrames(10);

            AssertPlayerIsHeld(playerRigidbody);
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
            yield return WaitFrames(10);

            AssertPlayerIsHeld(playerRigidbody);
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
            yield return WaitFrames(10);

            AssertPlayerIsHeld(playerRigidbody);
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
        var rightPullVelocity = Vector3.zero;
        var leftPullVelocity = Vector3.zero;

        try
        {
            yield return LaunchAndCaptureVelocity(mouse, 0.75f, 1.25f, velocity => rightPullVelocity = velocity);
            yield return LaunchAndCaptureVelocity(mouse, -0.75f, 1.25f, velocity => leftPullVelocity = velocity);

            Assert.That(rightPullVelocity.x, Is.LessThan(-0.5f));
            Assert.That(leftPullVelocity.x, Is.GreaterThan(0.5f));
            Assert.That(rightPullVelocity.magnitude, Is.EqualTo(leftPullVelocity.magnitude).Within(0.2f));
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator LoadGameplayScene()
    {
        if (CanReuseGameplayScene(SceneManager.GetActiveScene()))
            yield break;

        SceneManager.LoadScene(_gameplaySceneBuildIndex, LoadSceneMode.Single);
        yield break;
    }

    private bool CanReuseGameplayScene(Scene scene)
    {
        if (!scene.IsValid() || scene.buildIndex != _gameplaySceneBuildIndex)
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

    private T FindSingleInScene<T>(Scene scene, string objectDescription)
        where T : Component
    {
        var results = FindComponentsInScene<T>(scene);

        Assert.That(results, Has.Length.EqualTo(1), objectDescription);
        return results[0];
    }

    private T[] FindComponentsInScene<T>(Scene scene)
        where T : Component
    {
        return scene.GetRootGameObjects()
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<T>(true))
            .ToArray();
    }

    private GameObject FindGameObjectByName(Scene scene, string objectName)
    {
        if (TryFindGameObjectByName(scene, objectName, out var gameObject))
            return gameObject;

        Assert.Fail($"Expected scene object '{objectName}' to exist.");
        return null;
    }

    private bool TryFindGameObjectByName(Scene scene, string objectName, out GameObject gameObject)
    {
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            var transforms = rootGameObject.GetComponentsInChildren<Transform>(true);

            foreach (var childTransform in transforms)
            {
                if (childTransform.name == objectName)
                {
                    gameObject = childTransform.gameObject;
                    return true;
                }
            }
        }

        gameObject = null;
        return false;
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

    private IEnumerator LaunchAndCaptureVelocity(Mouse mouse, float pullOffset, float pullDistance, Action<Vector3> captureVelocity)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return ContinueToPreLaunch(activeScene);
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDistance);
        var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);

        yield return SendMouse(mouse, releaseScreenPosition, true);

        yield return SendMouse(mouse, releaseScreenPosition, false);
        yield return WaitUntilPlayerLaunches(playerRigidbody);

        captureVelocity.Invoke(playerRigidbody.linearVelocity);
    }

    private Vector3 GetExpectedClampedPullPoint(
        ISlingshotInputProjector slingshotInputProjector,
        Vector2 screenPosition,
        SlingshotGeometrySnapshot geometry,
        ISlingshotConfig slingshotConfig)
    {
        Assert.That(slingshotInputProjector.TryProjectScreenToPullPlane(screenPosition, geometry, out var rawPullPoint), Is.True);

        var delta = rawPullPoint - geometry.RestPoint;

        var pullDistance = Mathf.Clamp(
            -Vector3.Dot(delta, geometry.LaunchFrameForward),
            0f,
            slingshotConfig.MaximumPullDistance);
        var pullOffset = GetClampedPullOffset(Vector3.Dot(delta, geometry.LaunchFrameRight), pullDistance, geometry, slingshotConfig);

        return geometry.RestPoint
               + (geometry.LaunchFrameRight * pullOffset)
               - (geometry.LaunchFrameForward * pullDistance);
    }

    private float GetClampedPullOffset(
        float rawPullOffset,
        float pullDistance,
        SlingshotGeometrySnapshot geometry,
        ISlingshotConfig slingshotConfig)
    {
        var fullLateralPullDistance = Mathf.Max(0.02f, slingshotConfig.MinimumPullDistance + (slingshotConfig.BandContactPadding * 2f))
                                      + (slingshotConfig.BandContactPadding * 2f);

        var lateralPullScale = fullLateralPullDistance <= 0.000001f
            ? 1f
            : Mathf.Clamp01(pullDistance / fullLateralPullDistance);

        return Mathf.Clamp(
            rawPullOffset,
            GetMinimumAllowedPullOffset(geometry, slingshotConfig) * lateralPullScale,
            GetMaximumAllowedPullOffset(geometry, slingshotConfig) * lateralPullScale);
    }

    private float GetMinimumAllowedPullOffset(SlingshotGeometrySnapshot geometry, ISlingshotConfig slingshotConfig)
    {
        var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var minimumAnchorOffset = Mathf.Min(leftAnchorOffset, rightAnchorOffset);
        return Mathf.Max(-slingshotConfig.MaximumLateralPull, minimumAnchorOffset);
    }

    private float GetMaximumAllowedPullOffset(SlingshotGeometrySnapshot geometry, ISlingshotConfig slingshotConfig)
    {
        var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var maximumAnchorOffset = Mathf.Max(leftAnchorOffset, rightAnchorOffset);
        return Mathf.Min(slingshotConfig.MaximumLateralPull, maximumAnchorOffset);
    }

    // TODO: Fix this
    private IEnumerator WaitFrames(int frameCount)
    {
        for (var frameIndex = 0; frameIndex < frameCount; frameIndex += 1)
        {
            yield return null;
        }
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

    private void AssertPlayerIsHeld(Rigidbody playerRigidbody)
    {
        Assert.That(playerRigidbody.isKinematic, Is.True);
        Assert.That(playerRigidbody.linearVelocity.sqrMagnitude, Is.EqualTo(0f).Within(0.0001f));
        Assert.That(playerRigidbody.angularVelocity.sqrMagnitude, Is.EqualTo(0f).Within(0.0001f));
    }
}
