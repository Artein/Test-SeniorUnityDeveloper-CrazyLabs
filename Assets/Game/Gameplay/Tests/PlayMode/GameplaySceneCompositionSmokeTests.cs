using System;
using System.Collections;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneCompositionSmokeTests
{
    // TODO - AI Note: We should load scene via SceneRefernce + EditorAssetProvider instead of scene build index
    private readonly int _gameplaySceneBuildIndex = 0;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_SlingshotPrelaunchCompositionIsReady()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var canvas = FindSingleInScene<Canvas>(activeScene, "Gameplay UI Canvas");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
        var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");

        Assert.That(activeScene.buildIndex, Is.EqualTo(_gameplaySceneBuildIndex));
        Assert.That(lifetimeScope, Is.Not.Null);
        Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
        Assert.That(bandLineRenderer, Is.Not.Null);
        Assert.That(bandLineRenderer.sharedMaterial, Is.Not.Null);
        Assert.That(bandLineRenderer.positionCount, Is.EqualTo(3));
        Assert.That(geometry.LeftAnchorPosition.x, Is.LessThan(geometry.RightAnchorPosition.x));
        Assert.That(Vector3.Dot(geometry.LaunchFrameForward, Vector3.forward), Is.GreaterThan(0.99f));
        Assert.That(playerRigidbody, Is.Not.Null);
        Assert.That(playerRigidbody.isKinematic, Is.True);
        Assert.That(pullHint.transform.IsChildOf(canvas.transform), Is.True);
        Assert.That(pullHint.activeInHierarchy, Is.True);
        Assert.That(touchIndicator.transform.IsChildOf(canvas.transform), Is.True);
        Assert.That(touchIndicator.activeSelf, Is.False);
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsSlingshot_then_PlayerLaunches()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Smoke Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

            var pullWorldPosition = geometry.RestPoint
                                    + (geometry.LaunchFrameRight * 0.35f)
                                    - (geometry.LaunchFrameForward * 1.25f);
            var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

            QueueMouse(mouse, pressScreenPosition, true);
            yield return null;

            QueueMouse(mouse, releaseScreenPosition, true);
            yield return null;

            Assert.That(pullHint.activeSelf, Is.False);
            Assert.That(touchIndicator.activeSelf, Is.True);
            Assert.That(bandLineRenderer.GetPosition(1).x, Is.EqualTo(pullWorldPosition.x).Within(0.05f));
            Assert.That(bandLineRenderer.GetPosition(1).z, Is.EqualTo(pullWorldPosition.z).Within(0.05f));

            QueueMouse(mouse, releaseScreenPosition, false);
            yield return WaitUntilPlayerLaunches(playerRigidbody);

            Assert.That(playerRigidbody.isKinematic, Is.False);
            Assert.That(playerRigidbody.linearVelocity.magnitude, Is.GreaterThan(4f));
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

            QueueMouse(mouse, outsideBandScreenPosition, true);
            yield return null;

            QueueMouse(mouse, validPullScreenPosition, true);
            yield return null;

            Assert.That(pullHint.activeSelf, Is.True);
            Assert.That(touchIndicator.activeSelf, Is.False);

            QueueMouse(mouse, validPullScreenPosition, false);
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
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
            var weakPullWorldPosition = geometry.RestPoint - (geometry.LaunchFrameForward * 0.1f);
            var weakPullScreenPosition = GetScreenPosition(inputCamera, weakPullWorldPosition);

            QueueMouse(mouse, pressScreenPosition, true);
            yield return null;

            QueueMouse(mouse, weakPullScreenPosition, true);
            yield return null;

            Assert.That(touchIndicator.activeSelf, Is.True);

            QueueMouse(mouse, weakPullScreenPosition, false);
            yield return WaitFrames(10);

            AssertPlayerIsHeld(playerRigidbody);
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
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
            var forwardPullWorldPosition = geometry.RestPoint + (geometry.LaunchFrameForward * 1f);
            var forwardPullScreenPosition = GetScreenPosition(inputCamera, forwardPullWorldPosition);

            QueueMouse(mouse, pressScreenPosition, true);
            yield return null;

            QueueMouse(mouse, forwardPullScreenPosition, true);
            yield return null;

            Assert.That(bandLineRenderer.GetPosition(1).x, Is.EqualTo(geometry.RestPoint.x).Within(0.05f));
            Assert.That(bandLineRenderer.GetPosition(1).z, Is.EqualTo(geometry.RestPoint.z).Within(0.05f));

            QueueMouse(mouse, forwardPullScreenPosition, false);
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
        var loadOperation = SceneManager.LoadSceneAsync(_gameplaySceneBuildIndex, LoadSceneMode.Single);

        Assert.That(loadOperation, Is.Not.Null);

        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator WaitUntilPlayerIsHeld(Scene scene)
    {
        for (var frameIndex = 0; frameIndex < 30; frameIndex += 1)
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
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            var transforms = rootGameObject.GetComponentsInChildren<Transform>(true);

            foreach (var childTransform in transforms)
            {
                if (childTransform.name == objectName)
                    return childTransform.gameObject;
            }
        }

        Assert.Fail($"Expected scene object '{objectName}' to exist.");
        return null;
    }

    private Vector2 GetScreenPosition(Camera camera, Vector3 worldPosition)
    {
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        Assert.That(screenPosition.z, Is.GreaterThan(0f));
        return new Vector2(screenPosition.x, screenPosition.y);
    }

    private void QueueMouse(Mouse mouse, Vector2 screenPosition, bool isPressed)
    {
        var mouseState = new MouseState
        {
            position = screenPosition
        }.WithButton(MouseButton.Left, isPressed);

        InputSystem.QueueStateEvent(mouse, mouseState);
    }

    private IEnumerator LaunchAndCaptureVelocity(Mouse mouse, float pullOffset, float pullDistance, Action<Vector3> captureVelocity)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
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

        QueueMouse(mouse, pressScreenPosition, true);
        yield return null;

        QueueMouse(mouse, releaseScreenPosition, true);
        yield return null;

        QueueMouse(mouse, releaseScreenPosition, false);
        yield return WaitUntilPlayerLaunches(playerRigidbody);

        captureVelocity.Invoke(playerRigidbody.linearVelocity);
    }

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
