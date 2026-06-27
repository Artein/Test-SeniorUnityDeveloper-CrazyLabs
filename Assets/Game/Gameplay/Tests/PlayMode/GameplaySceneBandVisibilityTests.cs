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
public sealed class GameplaySceneBandVisibilityTests
{
    // TODO - AI Note: We should load scene via SceneRefernce + EditorAssetProvider instead of scene build index.
    private readonly int _gameplaySceneBuildIndex = 0;

    [UnityTest]
    public IEnumerator given_GameplayScene_when_PlayerPullsBandNearHeldTarget_then_BandCenterlineStaysVisibleFromGameplayCamera()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Band Visibility Mouse");

        try
        {
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(mouse, 0f, 0.2f, "Center Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(mouse, 0.35f, 0.35f, "Right Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(mouse, -0.35f, 0.35f, "Left Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(mouse, 0.45f, 0.2f, "Right Shallow Side Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(mouse, -0.45f, 0.2f, "Left Shallow Side Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(mouse, 0.75f, 0.35f, "Right Edge Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(mouse, -0.75f, 0.35f, "Left Edge Near Held Pull");
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator AssertBandCenterlineIsVisibleFromGameplayCamera(Mouse mouse, float pullOffset, float pullDepth, string phase)
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

        var pullWorldPosition = geometry.RestPoint
                                + (geometry.LaunchFrameRight * pullOffset)
                                - (geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));
        AssertBandCenterlineVisibleFromCamera(activeBandPositions, inputCamera, targetCollider, geometry, bandCenter.transform.position, phase);

        yield return SendMouse(mouse, pullScreenPosition, false);
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

        Assert.Fail("Expected Player to be held by the Slingshot.");
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

    private void AssertBandCenterlineVisibleFromCamera(
        Vector3[] bandPositions,
        Camera camera,
        Collider targetCollider,
        SlingshotGeometrySnapshot geometry,
        Vector3 bandCenter,
        string phase)
    {
        var cameraPosition = camera.transform.position;
        var diagnostics = CreateDiagnostics(bandPositions, targetCollider, geometry, bandCenter, phase);

        for (var pointIndex = 0; pointIndex < bandPositions.Length - 1; pointIndex += 1)
        {
            for (var sampleIndex = 0; sampleIndex <= 24; sampleIndex += 1)
            {
                var samplePoint = Vector3.Lerp(bandPositions[pointIndex], bandPositions[pointIndex + 1], sampleIndex / 24f);
                var rayDirection = samplePoint - cameraPosition;
                var sampleDistance = rayDirection.magnitude;

                if (sampleDistance <= 0.0001f)
                    continue;

                var ray = new Ray(cameraPosition, rayDirection / sampleDistance);

                Assert.That(
                    targetCollider.Raycast(ray, out _, sampleDistance - 0.002f),
                    Is.False,
                    diagnostics +
                    $"\n{phase} Band segment {pointIndex} sample {sampleIndex} is hidden behind the Launch Target from the gameplay camera.");
            }
        }
    }

    private string CreateDiagnostics(
        Vector3[] bandPositions,
        Collider targetCollider,
        SlingshotGeometrySnapshot geometry,
        Vector3 bandCenter,
        string phase)
    {
        var targetCenter = targetCollider.bounds.center;

        var message = $"{phase} "
                      + $"BandCenter=({ProjectOffsetDepth(bandCenter, geometry)}) "
                      + $"ColliderCenter=({ProjectOffsetDepth(targetCenter, geometry)})";

        for (var pointIndex = 0; pointIndex < bandPositions.Length; pointIndex += 1)
        {
            message += $"\n{phase} Point[{pointIndex}]=({ProjectOffsetDepth(bandPositions[pointIndex], geometry)})";
        }

        return message;
    }

    private string ProjectOffsetDepth(Vector3 point, SlingshotGeometrySnapshot geometry)
    {
        var offset = Vector3.Dot(point - geometry.RestPoint, geometry.LaunchFrameRight);
        var depth = Vector3.Dot(point - geometry.RestPoint, -geometry.LaunchFrameForward);
        return $"offset={offset:0.0000}, depth={depth:0.0000}";
    }
}
