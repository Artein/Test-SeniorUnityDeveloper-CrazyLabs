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
        AssertBandSamplesStayOutsideCollider(activeBandPositions, targetCollider, "Active Pull");

        yield return SendMouse(mouse, pullScreenPosition, false);
        yield return null;

        var recoilBandPositions = ReadWorldLinePositions(bandLineRenderer);
        Assert.That(recoilBandPositions, Has.Length.GreaterThan(3));
        AssertBandSamplesStayOutsideCollider(recoilBandPositions, targetCollider, "Band Release Recoil");
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

    private void AssertBandSamplesStayOutsideCollider(Vector3[] bandPositions, Collider targetCollider, string phase)
    {
        for (var pointIndex = 0; pointIndex < bandPositions.Length - 1; pointIndex += 1)
        {
            for (var sampleIndex = 0; sampleIndex <= 6; sampleIndex += 1)
            {
                var samplePoint = Vector3.Lerp(bandPositions[pointIndex], bandPositions[pointIndex + 1], sampleIndex / 6f);
                var closestPoint = targetCollider.ClosestPoint(samplePoint);
                var surfaceDistance = Vector3.Distance(samplePoint, closestPoint);

                Assert.That(surfaceDistance, Is.GreaterThan(0.002f), $"{phase} Band sample intersects the Launch Target Collider.");
            }
        }
    }
}
