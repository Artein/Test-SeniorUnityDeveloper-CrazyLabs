using System.Collections;
using Game.Gameplay;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneBandVisibilityTests : BaseGameplayScenePlayModeFixture
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_PlayerPullsBandNearHeldTarget_then_BandCenterlineStaysVisibleFromGameplayCamera()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Band Visibility Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return ContinueToPreLaunch(activeScene);
            var context = CreateSceneContext(activeScene);
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, 0f, 0.2f, "Center Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, 0.35f, 0.35f, "Right Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, -0.35f, 0.35f, "Left Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, 0.45f, 0.2f, "Right Shallow Side Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, -0.45f, 0.2f, "Left Shallow Side Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, 0.75f, 0.35f, "Right Edge Near Held Pull");
            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, -0.75f, 0.35f, "Left Edge Near Held Pull");

            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, context.SlingshotConfig.MaximumLateralPull,
                context.SlingshotConfig.MaximumPullDistance, phase: "Right Maximum Side Pull");

            yield return AssertBandCenterlineIsVisibleFromGameplayCamera(context, mouse, -context.SlingshotConfig.MaximumLateralPull,
                context.SlingshotConfig.MaximumPullDistance, phase: "Left Maximum Side Pull");

            yield return SendMouse(mouse, context.PressScreenPosition, false);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator AssertBandCenterlineIsVisibleFromGameplayCamera(
        GameplaySceneContext context,
        Mouse mouse,
        float pullOffset,
        float pullDepth,
        string phase)
    {
        var pullWorldPosition = context.Geometry.RestPoint
                                + (context.Geometry.LaunchFrameRight * pullOffset)
                                - (context.Geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(context.InputCamera, pullWorldPosition);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

        AssertBandCenterlineVisibleFromCamera(activeBandPositions, context.InputCamera, context.TargetCollider, context.Geometry,
            context.BandCenter.transform.position, phase);
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

        var geometry = slingshotViews[0].CreateGeometrySnapshot();
        return Vector3.Distance(bandCenter.transform.position, geometry.RestPoint) <= 0.05f;
    }

    private IEnumerator WaitUntilPlayerIsHeld(GameplaySceneContext context)
    {
        for (var frameIndex = 0; frameIndex < 10; frameIndex += 1)
        {
            if (context.PlayerRigidbody != null && context.PlayerRigidbody.isKinematic)
                yield break;

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

    private GameplaySceneContext CreateSceneContext(Scene scene)
    {
        var slingshotView = FindSingleInScene<SlingshotView>(scene, "SlingshotView");
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(scene, "RigidbodyLaunchTarget");
        var inputCamera = FindSingleInScene<Camera>(scene, "Input Camera");
        var bandCenter = FindGameObjectByName(scene, "Band Center");
        var geometry = slingshotView.CreateGeometrySnapshot();

        return new GameplaySceneContext(
            inputCamera,
            slingshotView.GetComponent<LineRenderer>(),
            GetSingleTargetCollider(launchTarget),
            launchTarget.GetComponent<Rigidbody>(),
            bandCenter,
            lifetimeScope.Container.Resolve<ISlingshotConfig>(),
            geometry,
            GetScreenPosition(inputCamera, geometry.RestPoint));
    }

    private sealed class GameplaySceneContext
    {
        public Camera InputCamera { get; }
        public LineRenderer BandLineRenderer { get; }
        public Collider TargetCollider { get; }
        public Rigidbody PlayerRigidbody { get; }
        public GameObject BandCenter { get; }
        public ISlingshotConfig SlingshotConfig { get; }
        public SlingshotGeometrySnapshot Geometry { get; }
        public Vector2 PressScreenPosition { get; }

        public GameplaySceneContext(
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
}
