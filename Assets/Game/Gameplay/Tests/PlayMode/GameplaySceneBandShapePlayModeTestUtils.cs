using System;
using System.Collections;
using System.IO;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using VContainer;

// ReSharper disable once CheckNamespace
internal static class GameplaySceneBandShapePlayModeTestUtils
{
    public static IEnumerator WaitUntilPlayerIsHeld(GameplaySceneBandShapePlayModeTestContext context)
    {
        for (var frameIndex = 0; frameIndex < 10; frameIndex += 1)
        {
            if (context.PlayerRigidbody != null && context.PlayerRigidbody.isKinematic)
                yield break;

            yield return null;
        }

        Assert.Fail("Expected Player to be held by the Slingshot.");
    }

    public static GameplaySceneBandShapePlayModeTestContext CreateSceneContext(Scene scene)
    {
        var slingshotView = FindSingleInScene<SlingshotView>(scene, "SlingshotView");
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(scene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(scene, "Input Camera");
        var bandCenter = FindGameObjectByName(scene, "Band Center");
        var geometry = slingshotView.CreateGeometrySnapshot();

        return new GameplaySceneBandShapePlayModeTestContext(
            inputCamera,
            slingshotView.GetComponent<LineRenderer>(),
            GetSingleTargetCollider(launchTarget),
            launchTarget.GetComponent<Rigidbody>(),
            bandCenter,
            lifetimeScope.Container.Resolve<ISlingshotConfig>(),
            geometry,
            GetScreenPosition(inputCamera, geometry.RestPoint));
    }

    public static Vector2 GetScreenPosition(Camera camera, Vector3 worldPosition)
    {
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        Assert.That(screenPosition.z, Is.GreaterThan(0f));
        return new Vector2(screenPosition.x, screenPosition.y);
    }

    public static IEnumerator SendMouse(Mouse mouse, Vector2 screenPosition, bool isPressed)
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

    public static Vector3[] ReadWorldLinePositions(LineRenderer lineRenderer)
    {
        var positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);

        if (lineRenderer.useWorldSpace)
            return positions;

        for (var i = 0; i < positions.Length; i += 1)
        {
            positions[i] = lineRenderer.transform.TransformPoint(positions[i]);
        }

        return positions;
    }

    public static float GetMaximumRenderedBandRadius(LineRenderer lineRenderer)
    {
        var maximumWidth = Mathf.Max(lineRenderer.startWidth, lineRenderer.endWidth);

        foreach (var key in lineRenderer.widthCurve.keys)
        {
            maximumWidth = Mathf.Max(maximumWidth, key.value);
        }

        return maximumWidth * lineRenderer.widthMultiplier * 0.5f;
    }

    public static Vector2 ProjectToPullPlane(Vector3 vector, SlingshotGeometrySnapshot geometry)
    {
        return new Vector2(
            Vector3.Dot(vector, geometry.LaunchFrameRight),
            Vector3.Dot(vector, -geometry.LaunchFrameForward));
    }

    public static float ProjectOffset(Vector3 position, SlingshotGeometrySnapshot geometry)
    {
        return Vector3.Dot(position - geometry.RestPoint, geometry.LaunchFrameRight);
    }

    public static float ProjectDepth(Vector3 position, SlingshotGeometrySnapshot geometry)
    {
        return Vector3.Dot(position - geometry.RestPoint, -geometry.LaunchFrameForward);
    }

    public static string DescribePoint(Vector3 position, SlingshotGeometrySnapshot geometry)
    {
        return $"({ProjectOffset(position, geometry):0.###}, {ProjectDepth(position, geometry):0.###})";
    }

    public static string TrySaveCameraCapture(Camera camera, string fileName)
    {
        try
        {
            return SaveCameraCapture(camera, fileName);
        }
        catch (Exception exception)
        {
            return $"capture failed: {exception.GetType().Name} {exception.Message}";
        }
    }

    private static string SaveCameraCapture(Camera camera, string fileName)
    {
        if (!SystemInfo.supportsRendering || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
        {
            return
                $"capture skipped: batchMode={Application.isBatchMode} supportsRendering={SystemInfo.supportsRendering} graphicsDeviceType={SystemInfo.graphicsDeviceType}";
        }

        const int width = 540;
        const int height = 960;
        var directory = Path.Combine(Application.temporaryCachePath, "slingshot-direct-max-pull-diagnostics");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        var previousTargetTexture = camera.targetTexture;
        var previousActiveRenderTexture = RenderTexture.active;
        var renderTexture = new RenderTexture(width, height, 24);
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        try
        {
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
            texture.Apply(false, false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTargetTexture;
            RenderTexture.active = previousActiveRenderTexture;
            UnityEngine.Object.Destroy(renderTexture);
            UnityEngine.Object.Destroy(texture);
        }

        return path;
    }

    private static T FindSingleInScene<T>(Scene scene, string objectDescription)
        where T : Component
    {
        var results = FindComponentsInScene<T>(scene);

        Assert.That(results, Has.Length.EqualTo(1), objectDescription);
        return results[0];
    }

    private static T[] FindComponentsInScene<T>(Scene scene)
        where T : Component
    {
        return scene.GetRootGameObjects()
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<T>(true))
            .ToArray();
    }

    private static GameObject FindGameObjectByName(Scene scene, string objectName)
    {
        if (TryFindGameObjectByName(scene, objectName, out var gameObject))
            return gameObject;

        Assert.Fail($"Expected scene object '{objectName}' to exist.");
        return null;
    }

    private static bool TryFindGameObjectByName(Scene scene, string objectName, out GameObject gameObject)
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

    private static Collider GetSingleTargetCollider(RigidbodyLaunchTarget launchTarget)
    {
        var colliders = launchTarget.GetComponentsInChildren<Collider>(true);

        Assert.That(colliders, Has.Length.EqualTo(1));
        return colliders[0];
    }
}

// ReSharper disable once CheckNamespace
internal sealed class GameplaySceneBandShapePlayModeTestContext
{
    public Camera InputCamera { get; }
    public LineRenderer BandLineRenderer { get; }
    public Collider TargetCollider { get; }
    public Rigidbody PlayerRigidbody { get; }
    public GameObject BandCenter { get; }
    public ISlingshotConfig SlingshotConfig { get; }
    public SlingshotGeometrySnapshot Geometry { get; }
    public Vector2 PressScreenPosition { get; }

    public GameplaySceneBandShapePlayModeTestContext(
        Camera inputCamera,
        LineRenderer bandLineRenderer,
        Collider targetCollider,
        Rigidbody playerRigidbody,
        GameObject bandCenter,
        ISlingshotConfig slingshotConfig,
        SlingshotGeometrySnapshot geometry,
        Vector2 pressScreenPosition)
    {
        InputCamera = inputCamera;
        BandLineRenderer = bandLineRenderer;
        TargetCollider = targetCollider;
        PlayerRigidbody = playerRigidbody;
        BandCenter = bandCenter;
        SlingshotConfig = slingshotConfig;
        Geometry = geometry;
        PressScreenPosition = pressScreenPosition;
    }
}
