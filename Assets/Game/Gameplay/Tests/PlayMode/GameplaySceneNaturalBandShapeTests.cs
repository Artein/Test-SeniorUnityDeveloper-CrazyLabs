using System.Collections;
using System.Linq;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneNaturalBandShapeTests
{
    // TODO - AI Note: We should load scene via SceneRefernce + EditorAssetProvider instead of scene build index
    private readonly int _gameplaySceneBuildIndex = 0;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_LoadedIntoPrelaunch_then_CaptureIdleBandShapeStaysOutsideTargetCollider()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var targetCollider = GetSingleTargetCollider(launchTarget);
        var geometry = slingshotView.CreateGeometrySnapshot();

        var captureIdleBandPositions = ReadWorldLinePositions(bandLineRenderer);

        Assert.That(captureIdleBandPositions, Has.Length.GreaterThan(3));
        AssertBandShapeMatchesRawTwoSpan(captureIdleBandPositions, geometry, geometry.RestPoint, 0.01f, "Capture Idle");
        AssertBandSamplesStayOutsideCollider(captureIdleBandPositions, bandLineRenderer, targetCollider, "Capture Idle");
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_MousePressesRestWithoutPull_then_BandShapeStaysNearRestAndOutsideTargetCollider()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Rest Press Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var targetCollider = GetSingleTargetCollider(launchTarget);
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

            yield return SendMouse(mouse, pressScreenPosition, true);

            var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
            var visualCenterPoint = activeBandPositions[(activeBandPositions.Length - 1) / 2];

            Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

            AssertSimpleBandVisualCenterMatchesBandCenter(
                visualCenterPoint,
                bandCenter.transform.position,
                bandLineRenderer,
                geometry,
                "Rest Press");
            AssertBandShapeMatchesRawTwoSpan(activeBandPositions, geometry, visualCenterPoint, 0.01f, "Rest Press");
            AssertBandSamplesStayOutsideCollider(activeBandPositions, bandLineRenderer, targetCollider, "Rest Press");

            yield return SendMouse(mouse, pressScreenPosition, false);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_MouseTinyPullsNearRest_then_BandShapeStaysNearPullPointAndOutsideTargetCollider()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Tiny Pull Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return WaitUntilPlayerIsHeld(activeScene);

            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var targetCollider = GetSingleTargetCollider(launchTarget);
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var geometry = slingshotView.CreateGeometrySnapshot();
            var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

            var tinyPullWorldPosition = geometry.RestPoint
                                        + (geometry.LaunchFrameRight * 0.02f)
                                        - (geometry.LaunchFrameForward * 0.02f);
            var tinyPullScreenPosition = GetScreenPosition(inputCamera, tinyPullWorldPosition);

            yield return SendMouse(mouse, pressScreenPosition, true);
            yield return SendMouse(mouse, tinyPullScreenPosition, true);

            var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
            var visualCenterPoint = activeBandPositions[(activeBandPositions.Length - 1) / 2];

            Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

            AssertSimpleBandVisualCenterMatchesBandCenter(
                visualCenterPoint,
                bandCenter.transform.position,
                bandLineRenderer,
                geometry,
                "Tiny Pull");
            AssertBandShapeMatchesRawTwoSpan(activeBandPositions, geometry, visualCenterPoint, 0.01f, "Tiny Pull");
            AssertBandSamplesStayOutsideCollider(activeBandPositions, bandLineRenderer, targetCollider, "Tiny Pull");

            yield return SendMouse(mouse, tinyPullScreenPosition, false);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_MouseShallowLateralPullsLeftAndRight_then_BandShapeUsesCleanTwoSpan()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Shallow Lateral Pull Mouse");

        try
        {
            yield return AssertShallowLateralBandShape(mouse, 0.75f, 0.02f);
            yield return AssertShallowLateralBandShape(mouse, -0.75f, 0.02f);
            yield return AssertShallowLateralBandShape(mouse, 0.75f, 0.15f);
            yield return AssertShallowLateralBandShape(mouse, -0.75f, 0.15f);
            yield return AssertShallowLateralBandShape(mouse, 0.75f, 0.35f);
            yield return AssertShallowLateralBandShape(mouse, -0.75f, 0.35f);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsLeftAndRight_then_BandShapeWrapsPulledSideAndStaysOutsideTargetCollider()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Natural Band Mouse");

        try
        {
            yield return AssertPulledBandShape(mouse, 0.75f);
            yield return AssertPulledBandShape(mouse, -0.75f);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsBeyondAnchorSpan_then_TargetColliderStaysInsideAnchorSpan()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Anchor Span Clamp Mouse");

        try
        {
            yield return AssertTargetColliderClampsInsideAnchorSpan(mouse, 1f);
            yield return AssertTargetColliderClampsInsideAnchorSpan(mouse, -1f);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsNearAnchorSides_then_BandStaysInsideRenderedAnchorSpan()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Rendered Anchor Span Mouse");

        try
        {
            yield return AssertRenderedAnchorSpan(mouse, 0.85f, 0.35f, "Right Shallow Side Pull");
            yield return AssertRenderedAnchorSpan(mouse, -0.85f, 0.35f, "Left Shallow Side Pull");
            yield return AssertRenderedAnchorSpan(mouse, 0.85f, 1.25f, "Right Deep Side Pull");
            yield return AssertRenderedAnchorSpan(mouse, -0.85f, 1.25f, "Left Deep Side Pull");
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsSideways_then_PulledWrapStaysAlignedWithBandCenter()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Band Center Wrap Mouse");

        try
        {
            yield return AssertPulledWrapAlignsWithBandCenter(mouse, 0.75f, 1.25f, "Right Deep Pull");
            yield return AssertPulledWrapAlignsWithBandCenter(mouse, -0.75f, 1.25f, "Left Deep Pull");
            yield return AssertPulledWrapAlignsWithBandCenter(mouse, 0.75f, 2.25f, "Right Long Pull");
            yield return AssertPulledWrapAlignsWithBandCenter(mouse, -0.75f, 2.25f, "Left Long Pull");
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsSideways_then_AnchorSideBandSpanDoesNotWrapAroundOppositeSide()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Anchor Side Span Mouse");

        try
        {
            yield return AssertAnchorSideSpanDoesNotBacktrack(mouse, 0.75f, 0.35f, "Right Shallow Pull");
            yield return AssertAnchorSideSpanDoesNotBacktrack(mouse, -0.75f, 0.35f, "Left Shallow Pull");
            yield return AssertAnchorSideSpanDoesNotBacktrack(mouse, 0.75f, 1.25f, "Right Deep Pull");
            yield return AssertAnchorSideSpanDoesNotBacktrack(mouse, -0.75f, 1.25f, "Left Deep Pull");
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsSidewaysNearTarget_then_BandPathDoesNotBacktrackAcrossTarget()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Side Kink Mouse");

        try
        {
            yield return AssertBandPathDoesNotBacktrackAcrossTarget(mouse, 0.75f, 0.5f, "Right First Taut Pull");
            yield return AssertBandPathDoesNotBacktrackAcrossTarget(mouse, -0.75f, 0.5f, "Left First Taut Pull");
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator AssertBandPathDoesNotBacktrackAcrossTarget(Mouse mouse, float pullOffset, float pullDepth, string phase)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var targetCollider = GetSingleTargetCollider(launchTarget);
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

        AssertBandShapeDoesNotMatchRawTwoSpan(activeBandPositions, geometry, phase);
        AssertBandPathOffsetsAreMonotonic(activeBandPositions, bandLineRenderer, geometry, phase);
        AssertBandPathHasNoSharpFolds(activeBandPositions, geometry, phase);
        AssertBandSamplesStayOutsideCollider(activeBandPositions, bandLineRenderer, targetCollider, phase);

        yield return SendMouse(mouse, pullScreenPosition, false);
    }

    private IEnumerator AssertAnchorSideSpanDoesNotBacktrack(Mouse mouse, float pullOffset, float pullDepth, string phase)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var bandCenter = FindGameObjectByName(activeScene, "Band Center");
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

        AssertAnchorSideHalfStaysOnPulledSide(
            activeBandPositions,
            bandLineRenderer,
            bandCenter.transform.position,
            geometry,
            pullOffset,
            phase);

        yield return SendMouse(mouse, pullScreenPosition, false);
    }

    private IEnumerator AssertPulledWrapAlignsWithBandCenter(Mouse mouse, float pullOffset, float pullDepth, string phase)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var bandCenter = FindGameObjectByName(activeScene, "Band Center");
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(activeBandPositions.Length % 2, Is.EqualTo(1), $"{phase} Band Shape should have an explicit middle wrap point.");

        var actualBandCenterOffset = Vector3.Dot(bandCenter.transform.position - geometry.RestPoint, geometry.LaunchFrameRight);
        var actualBandCenterDepth = Vector3.Dot(bandCenter.transform.position - geometry.RestPoint, -geometry.LaunchFrameForward);
        var middleWrapPoint = activeBandPositions[(activeBandPositions.Length - 1) / 2];
        var middleWrapOffset = Vector3.Dot(middleWrapPoint - geometry.RestPoint, geometry.LaunchFrameRight);
        var middleWrapDepth = Vector3.Dot(middleWrapPoint - geometry.RestPoint, -geometry.LaunchFrameForward);
        var renderedTolerance = GetMaximumRenderedBandRadius(bandLineRenderer) + 0.04f;

        Assert.That(
            Mathf.Abs(middleWrapOffset - actualBandCenterOffset),
            Is.LessThanOrEqualTo(renderedTolerance),
            $"{phase} pulled Band wrap should stay laterally aligned with the authored Band Center.");

        Assert.That(
            Mathf.Abs(middleWrapDepth - actualBandCenterDepth),
            Is.LessThanOrEqualTo(0.15f),
            $"{phase} pulled Band wrap should stay near the authored Band Center depth.");

        yield return SendMouse(mouse, pullScreenPosition, false);
    }

    private IEnumerator AssertShallowLateralBandShape(Mouse mouse, float pullOffset, float pullDepth)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var bandCenter = FindGameObjectByName(activeScene, "Band Center");
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

        var visualCenterPoint = activeBandPositions[(activeBandPositions.Length - 1) / 2];

        AssertSimpleBandVisualCenterMatchesBandCenter(
            visualCenterPoint,
            bandCenter.transform.position,
            bandLineRenderer,
            geometry,
            $"Shallow Lateral Pull offset {pullOffset} depth {pullDepth}");

        AssertBandShapeMatchesRawTwoSpan(activeBandPositions, geometry, visualCenterPoint, 0.01f,
            $"Shallow Lateral Pull offset {pullOffset} depth {pullDepth}");

        yield return SendMouse(mouse, pullScreenPosition, false);
    }

    private IEnumerator AssertTargetColliderClampsInsideAnchorSpan(Mouse mouse, float lateralSign)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var bandCenter = FindGameObjectByName(activeScene, "Band Center");
        var targetCollider = GetSingleTargetCollider(launchTarget);
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);
        var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var sideAnchorOffset = lateralSign > 0f ? rightAnchorOffset : leftAnchorOffset;

        var beyondAnchorWorldPosition = geometry.RestPoint
                                        + (geometry.LaunchFrameRight * (sideAnchorOffset + lateralSign))
                                        - (geometry.LaunchFrameForward * 1.25f);
        var beyondAnchorScreenPosition = GetScreenPosition(inputCamera, beyondAnchorWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, beyondAnchorScreenPosition, true);

        var bandCenterOffset = Vector3.Dot(bandCenter.transform.position - geometry.RestPoint, geometry.LaunchFrameRight);

        if (lateralSign > 0f)
            Assert.That(bandCenterOffset, Is.LessThan(rightAnchorOffset - 0.05f));
        else
            Assert.That(bandCenterOffset, Is.GreaterThan(leftAnchorOffset + 0.05f));

        AssertTargetColliderWithinAnchorSpan(targetCollider, geometry, $"Lateral Clamp {lateralSign}");

        yield return SendMouse(mouse, beyondAnchorScreenPosition, false);
    }

    private IEnumerator AssertRenderedAnchorSpan(Mouse mouse, float pullOffset, float pullDepth, string phase)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var bandCenter = FindGameObjectByName(activeScene, "Band Center");
        var targetCollider = GetSingleTargetCollider(launchTarget);
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

        AssertTargetColliderWithinRenderedAnchorSpan(
            targetCollider,
            bandCenter.transform.position,
            bandLineRenderer,
            geometry,
            phase);
        AssertBandCenterlineWithinAnchorSpan(activeBandPositions, geometry, phase);
        AssertBandSamplesStayOutsideCollider(activeBandPositions, bandLineRenderer, targetCollider, phase);

        yield return SendMouse(mouse, pullScreenPosition, false);
    }

    private IEnumerator AssertPulledBandShape(Mouse mouse, float pullOffset)
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return WaitUntilPlayerIsHeld(activeScene);

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var targetCollider = GetSingleTargetCollider(launchTarget);
        var geometry = slingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * 1.25f);
        var pullScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));
        AssertPulledSideWrap(activeBandPositions, pullWorldPosition, geometry.LaunchFrameRight, pullOffset);
        AssertBandSamplesStayOutsideCollider(activeBandPositions, bandLineRenderer, targetCollider, "Active Pull");

        yield return SendMouse(mouse, pullScreenPosition, false);
        yield return null;

        var recoilBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(recoilBandPositions, Has.Length.GreaterThan(3));
        AssertBandSamplesStayOutsideCollider(recoilBandPositions, bandLineRenderer, targetCollider, "Band Release Recoil");
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

    private Collider GetSingleTargetCollider(RigidbodyLaunchTarget launchTarget)
    {
        var colliders = launchTarget.GetComponentsInChildren<Collider>(true);

        Assert.That(colliders, Has.Length.EqualTo(1));
        return colliders[0];
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
        yield return null;
        yield return null;
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

    private Vector3[] ReadWorldLinePositions(LineRenderer lineRenderer)
    {
        var positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);

        if (lineRenderer.useWorldSpace)
            return positions;

        for (var positionIndex = 0; positionIndex < positions.Length; positionIndex += 1)
        {
            positions[positionIndex] = lineRenderer.transform.TransformPoint(positions[positionIndex]);
        }

        return positions;
    }

    private void AssertPulledSideWrap(Vector3[] bandPositions, Vector3 pullWorldPosition, Vector3 launchFrameRight, float pullOffset)
    {
        var pullSign = Mathf.Sign(pullOffset);

        var strongestPulledSidePoint = bandPositions
            .Skip(1)
            .Take(bandPositions.Length - 2)
            .Max(position => Vector3.Dot(position - pullWorldPosition, launchFrameRight) * pullSign);

        Assert.That(strongestPulledSidePoint, Is.GreaterThan(0.05f));
    }

    private void AssertBandSamplesStayOutsideCollider(
        Vector3[] bandPositions,
        LineRenderer bandLineRenderer,
        Collider targetCollider,
        string phase)
    {
        const float samplingSafetyMargin = 0.002f;
        var requiredClearance = GetMaximumRenderedBandRadius(bandLineRenderer) + samplingSafetyMargin;

        for (var pointIndex = 0; pointIndex < bandPositions.Length - 1; pointIndex += 1)
        {
            for (var sampleIndex = 0; sampleIndex <= 24; sampleIndex += 1)
            {
                var samplePoint = Vector3.Lerp(bandPositions[pointIndex], bandPositions[pointIndex + 1], sampleIndex / 24f);
                var closestPoint = targetCollider.ClosestPoint(samplePoint);
                var surfaceDistance = Vector3.Distance(samplePoint, closestPoint);

                Assert.That(
                    surfaceDistance,
                    Is.GreaterThan(requiredClearance),
                    $"{phase} Band rendered radius intersects the Launch Target Collider.");
            }
        }
    }

    private float GetMaximumRenderedBandRadius(LineRenderer lineRenderer)
    {
        var maximumWidth = Mathf.Max(lineRenderer.startWidth, lineRenderer.endWidth);

        foreach (var key in lineRenderer.widthCurve.keys)
        {
            maximumWidth = Mathf.Max(maximumWidth, key.value);
        }

        return maximumWidth * lineRenderer.widthMultiplier * 0.5f;
    }

    private void AssertTargetColliderWithinAnchorSpan(Collider targetCollider, SlingshotGeometrySnapshot geometry, string phase)
    {
        GetProjectedColliderSpan(targetCollider, geometry.RestPoint, geometry.LaunchFrameRight, out var targetMinimumOffset,
            out var targetMaximumOffset);

        var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var tolerance = 0.02f;

        Assert.That(
            targetMinimumOffset,
            Is.GreaterThanOrEqualTo(leftAnchorOffset - tolerance),
            $"{phase} target collider should stay inside the left Slingshot Anchor span.");

        Assert.That(
            targetMaximumOffset,
            Is.LessThanOrEqualTo(rightAnchorOffset + tolerance),
            $"{phase} target collider should stay inside the right Slingshot Anchor span.");
    }

    private void AssertTargetColliderWithinRenderedAnchorSpan(
        Collider targetCollider,
        Vector3 bandCenterPosition,
        LineRenderer bandLineRenderer,
        SlingshotGeometrySnapshot geometry,
        string phase)
    {
        GetProjectedColliderSpan(targetCollider, geometry.RestPoint, geometry.LaunchFrameRight, out var targetMinimumOffset,
            out var targetMaximumOffset);

        var bandCenterOffset = Vector3.Dot(bandCenterPosition - geometry.RestPoint, geometry.LaunchFrameRight);

        var targetHalfWidth = Mathf.Max(
            Mathf.Abs(bandCenterOffset - targetMinimumOffset),
            Mathf.Abs(targetMaximumOffset - bandCenterOffset));
        var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var renderedClearance = GetMaximumRenderedBandRadius(bandLineRenderer) + 0.02f;
        var minimumBandCenterOffset = leftAnchorOffset + targetHalfWidth + renderedClearance;
        var maximumBandCenterOffset = rightAnchorOffset - targetHalfWidth - renderedClearance;

        Assert.That(
            minimumBandCenterOffset,
            Is.LessThanOrEqualTo(maximumBandCenterOffset),
            $"{phase} Slingshot Anchor span should fit the target collider plus rendered band clearance.");

        Assert.That(
            bandCenterOffset,
            Is.GreaterThanOrEqualTo(minimumBandCenterOffset),
            $"{phase} Band Center should leave rendered band clearance inside the left Slingshot Anchor.");

        Assert.That(
            bandCenterOffset,
            Is.LessThanOrEqualTo(maximumBandCenterOffset),
            $"{phase} Band Center should leave rendered band clearance inside the right Slingshot Anchor.");
    }

    private void AssertBandCenterlineWithinAnchorSpan(Vector3[] bandPositions, SlingshotGeometrySnapshot geometry, string phase)
    {
        var leftAnchorOffset = Vector3.Dot(geometry.LeftAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var rightAnchorOffset = Vector3.Dot(geometry.RightAnchorPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        const float tolerance = 0.002f;

        for (var pointIndex = 0; pointIndex < bandPositions.Length; pointIndex += 1)
        {
            var offset = Vector3.Dot(bandPositions[pointIndex] - geometry.RestPoint, geometry.LaunchFrameRight);

            Assert.That(
                offset,
                Is.GreaterThanOrEqualTo(leftAnchorOffset - tolerance),
                $"{phase} Band point {pointIndex} should not escape past the left Slingshot Anchor.");

            Assert.That(
                offset,
                Is.LessThanOrEqualTo(rightAnchorOffset + tolerance),
                $"{phase} Band point {pointIndex} should not escape past the right Slingshot Anchor.");
        }
    }

    private void AssertAnchorSideHalfStaysOnPulledSide(
        Vector3[] bandPositions,
        LineRenderer lineRenderer,
        Vector3 bandCenterPosition,
        SlingshotGeometrySnapshot geometry,
        float pullOffset,
        string phase)
    {
        var middleIndex = (bandPositions.Length - 1) / 2;
        var bandCenterOffset = Vector3.Dot(bandCenterPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var tolerance = GetMaximumRenderedBandRadius(lineRenderer) + 0.03f;

        if (pullOffset > 0f)
        {
            for (var pointIndex = middleIndex; pointIndex < bandPositions.Length; pointIndex += 1)
            {
                var pointOffset = Vector3.Dot(bandPositions[pointIndex] - geometry.RestPoint, geometry.LaunchFrameRight);

                Assert.That(
                    pointOffset,
                    Is.GreaterThanOrEqualTo(bandCenterOffset - tolerance),
                    $"{phase} right anchor-side Band span should not wrap around the left side of the target. {DescribeBandOffsets(bandPositions, geometry)}");
            }

            return;
        }

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var pointOffset = Vector3.Dot(bandPositions[pointIndex] - geometry.RestPoint, geometry.LaunchFrameRight);

            Assert.That(
                pointOffset,
                Is.LessThanOrEqualTo(bandCenterOffset + tolerance),
                $"{phase} left anchor-side Band span should not wrap around the right side of the target. {DescribeBandOffsets(bandPositions, geometry)}");
        }
    }

    private void AssertBandPathOffsetsAreMonotonic(
        Vector3[] bandPositions,
        LineRenderer lineRenderer,
        SlingshotGeometrySnapshot geometry,
        string phase)
    {
        var tolerance = GetMaximumRenderedBandRadius(lineRenderer) + 0.01f;
        var previousOffset = Vector3.Dot(bandPositions[0] - geometry.RestPoint, geometry.LaunchFrameRight);

        for (var pointIndex = 1; pointIndex < bandPositions.Length; pointIndex += 1)
        {
            var offset = Vector3.Dot(bandPositions[pointIndex] - geometry.RestPoint, geometry.LaunchFrameRight);

            Assert.That(
                offset,
                Is.GreaterThanOrEqualTo(previousOffset - tolerance),
                $"{phase} Band path should not laterally backtrack across the target. {DescribeBandOffsets(bandPositions, geometry)}");

            previousOffset = Mathf.Max(previousOffset, offset);
        }
    }

    private void AssertBandPathHasNoSharpFolds(Vector3[] bandPositions, SlingshotGeometrySnapshot geometry, string phase)
    {
        const float minimumAdjacentDirectionDot = 0.2f;
        const float minimumSegmentLengthSquared = 0.000001f;

        for (var pointIndex = 1; pointIndex < bandPositions.Length - 1; pointIndex += 1)
        {
            var previousSegment = ProjectToPullPlane(bandPositions[pointIndex] - bandPositions[pointIndex - 1], geometry);
            var nextSegment = ProjectToPullPlane(bandPositions[pointIndex + 1] - bandPositions[pointIndex], geometry);

            if (previousSegment.sqrMagnitude <= minimumSegmentLengthSquared
                || nextSegment.sqrMagnitude <= minimumSegmentLengthSquared)
            {
                continue;
            }

            var directionDot = Vector2.Dot(previousSegment.normalized, nextSegment.normalized);

            Assert.That(
                directionDot,
                Is.GreaterThanOrEqualTo(minimumAdjacentDirectionDot),
                $"{phase} Band path should not form a sharp inward fold at point {pointIndex}. {DescribeBandOffsets(bandPositions, geometry)}");
        }
    }

    private Vector2 ProjectToPullPlane(Vector3 vector, SlingshotGeometrySnapshot geometry)
    {
        return new Vector2(
            Vector3.Dot(vector, geometry.LaunchFrameRight),
            Vector3.Dot(vector, -geometry.LaunchFrameForward));
    }

    private void AssertBandShapeDoesNotMatchRawTwoSpan(Vector3[] bandPositions, SlingshotGeometrySnapshot geometry, string phase)
    {
        var middleIndex = (bandPositions.Length - 1) / 2;
        var lastIndex = bandPositions.Length - 1;
        var centerPoint = bandPositions[middleIndex];
        var maximumDistanceFromRawTwoSpan = 0f;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
            var expectedPoint = Vector3.Lerp(geometry.LeftAnchorPosition, centerPoint, progress);
            maximumDistanceFromRawTwoSpan = Mathf.Max(maximumDistanceFromRawTwoSpan, Vector3.Distance(bandPositions[pointIndex], expectedPoint));
        }

        for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
        {
            var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
            var expectedPoint = Vector3.Lerp(centerPoint, geometry.RightAnchorPosition, progress);
            maximumDistanceFromRawTwoSpan = Mathf.Max(maximumDistanceFromRawTwoSpan, Vector3.Distance(bandPositions[pointIndex], expectedPoint));
        }

        Assert.That(
            maximumDistanceFromRawTwoSpan,
            Is.GreaterThan(0.01f),
            $"{phase} Band Shape should use taut target wrap instead of raw two-span geometry. {DescribeBandOffsets(bandPositions, geometry)}");
    }

    private string DescribeBandOffsets(Vector3[] bandPositions, SlingshotGeometrySnapshot geometry)
    {
        return "Band offsets: " + string.Join(", ", bandPositions.Select(position => { return DescribePoint(position, geometry); }));
    }

    private string DescribePoint(Vector3 position, SlingshotGeometrySnapshot geometry)
    {
        var offset = Vector3.Dot(position - geometry.RestPoint, geometry.LaunchFrameRight);
        var depth = Vector3.Dot(position - geometry.RestPoint, -geometry.LaunchFrameForward);
        return $"({offset:0.###}, {depth:0.###})";
    }

    private void AssertSimpleBandVisualCenterMatchesBandCenter(
        Vector3 visualCenterPoint,
        Vector3 bandCenterPosition,
        LineRenderer lineRenderer,
        SlingshotGeometrySnapshot geometry,
        string phase)
    {
        var visualCenterOffset = Vector3.Dot(visualCenterPoint - geometry.RestPoint, geometry.LaunchFrameRight);
        var visualCenterDepth = Vector3.Dot(visualCenterPoint - geometry.RestPoint, -geometry.LaunchFrameForward);
        var bandCenterOffset = Vector3.Dot(bandCenterPosition - geometry.RestPoint, geometry.LaunchFrameRight);
        var bandCenterDepth = Vector3.Dot(bandCenterPosition - geometry.RestPoint, -geometry.LaunchFrameForward);

        Assert.That(
            visualCenterOffset,
            Is.EqualTo(bandCenterOffset).Within(0.01f),
            $"{phase} simple Band visual center should stay laterally aligned with the authored Band Center.");

        Assert.That(
            visualCenterDepth,
            Is.GreaterThanOrEqualTo(bandCenterDepth + GetMaximumRenderedBandRadius(lineRenderer) + 0.005f),
            $"{phase} simple Band visual center should leave rendered-band clearance from the authored Band Center.");

        Assert.That(
            visualCenterDepth,
            Is.LessThanOrEqualTo(bandCenterDepth + 0.08f),
            $"{phase} simple Band visual center should only use a small contact stand-off.");
    }

    private void GetProjectedColliderSpan(Collider targetCollider, Vector3 origin, Vector3 axis, out float minimumOffset, out float maximumOffset)
    {
        var bounds = targetCollider.bounds;
        var extents = bounds.extents;
        minimumOffset = float.PositiveInfinity;
        maximumOffset = float.NegativeInfinity;

        for (var xSign = -1; xSign <= 1; xSign += 2)
        {
            for (var ySign = -1; ySign <= 1; ySign += 2)
            {
                for (var zSign = -1; zSign <= 1; zSign += 2)
                {
                    var corner = bounds.center + new Vector3(extents.x * xSign, extents.y * ySign, extents.z * zSign);
                    var offset = Vector3.Dot(corner - origin, axis);
                    minimumOffset = Mathf.Min(minimumOffset, offset);
                    maximumOffset = Mathf.Max(maximumOffset, offset);
                }
            }
        }
    }

    private void AssertBandShapeMatchesRawTwoSpan(
        Vector3[] bandPositions,
        SlingshotGeometrySnapshot geometry,
        Vector3 centerPoint,
        float tolerance,
        string phase)
    {
        var middleIndex = (bandPositions.Length - 1) / 2;
        var lastIndex = bandPositions.Length - 1;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
            var expectedPoint = Vector3.Lerp(geometry.LeftAnchorPosition, centerPoint, progress);

            Assert.That(
                Vector3.Distance(bandPositions[pointIndex], expectedPoint),
                Is.LessThanOrEqualTo(tolerance),
                $"{phase} Band Shape should use clean two-span geometry near rest.");
        }

        for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
        {
            var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
            var expectedPoint = Vector3.Lerp(centerPoint, geometry.RightAnchorPosition, progress);

            Assert.That(
                Vector3.Distance(bandPositions[pointIndex], expectedPoint),
                Is.LessThanOrEqualTo(tolerance),
                $"{phase} Band Shape should use clean two-span geometry near rest.");
        }
    }
}
