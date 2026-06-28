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
using VContainer;

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
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertShallowLateralBandShape(context, mouse, 0.75f, 0.02f);
            yield return AssertShallowLateralBandShape(context, mouse, -0.75f, 0.02f);
            yield return AssertShallowLateralBandShape(context, mouse, 0.75f, 0.15f);
            yield return AssertShallowLateralBandShape(context, mouse, -0.75f, 0.15f);
            yield return AssertShallowLateralBandShape(context, mouse, 0.75f, 0.35f);
            yield return AssertShallowLateralBandShape(context, mouse, -0.75f, 0.35f);

            yield return SendMouse(mouse, context.PressScreenPosition, false);
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
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertPulledBandShape(context, mouse, 0.75f, false);
            yield return AssertPulledBandShape(context, mouse, -0.75f, true);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_LowValidPullReleases_then_RecoilBandStaysOutsideTargetColliderAcrossFrames()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Low Valid Recoil Mouse");

        try
        {
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            var lowValidPullDepth = context.SlingshotConfig.MinimumPullDistance + 0.02f;

            var pullWorldPosition = context.Geometry.RestPoint
                                    + (context.Geometry.LaunchFrameRight * 0.35f)
                                    - (context.Geometry.LaunchFrameForward * lowValidPullDepth);
            var pullScreenPosition = GetScreenPosition(context.InputCamera, pullWorldPosition);
            yield return SendMouse(mouse, pullScreenPosition, true);

            var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
            Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

            AssertBandSamplesStayOutsideCollider(
                activeBandPositions,
                context.BandLineRenderer,
                context.TargetCollider,
                "Low Valid Active Pull");

            yield return SendMouse(mouse, pullScreenPosition, false);
            Assert.That(context.PlayerRigidbody.isKinematic, Is.False);
            var blockedRestBandColliderCenter = context.Geometry.RestPoint;
            PinTargetColliderCenter(context, blockedRestBandColliderCenter);

            Assert.That(
                Vector3.Distance(
                    blockedRestBandColliderCenter,
                    context.TargetCollider.ClosestPoint(blockedRestBandColliderCenter)),
                Is.LessThanOrEqualTo(GetMaximumRenderedBandRadius(context.BandLineRenderer)),
                "Test setup should keep the Launch Target Collider blocking the detached rest Band Shape.");

            for (var frameIndex = 0; frameIndex < 8; frameIndex += 1)
            {
                PinTargetColliderCenter(context, blockedRestBandColliderCenter);
                yield return null;
                PinTargetColliderCenter(context, blockedRestBandColliderCenter);

                var recoilBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
                Assert.That(recoilBandPositions, Has.Length.GreaterThan(3));

                AssertBandSamplesStayOutsideCollider(
                    recoilBandPositions,
                    context.BandLineRenderer,
                    context.TargetCollider,
                    $"Low Valid Band Release Recoil frame {frameIndex}");
            }
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_MaxPullReleasesAndRestBandClears_then_RecoilDetachesToRestBandShape()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Max Pull Recoil Mouse");

        try
        {
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            var pullWorldPosition = context.Geometry.RestPoint
                                    - (context.Geometry.LaunchFrameForward * context.SlingshotConfig.MaximumPullDistance);
            var pullScreenPosition = GetScreenPosition(context.InputCamera, pullWorldPosition);
            yield return SendMouse(mouse, pullScreenPosition, true);

            var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
            Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

            AssertBandSamplesStayOutsideCollider(
                activeBandPositions,
                context.BandLineRenderer,
                context.TargetCollider,
                "Max Pull Active Pull");

            yield return SendMouse(mouse, pullScreenPosition, false);
            Assert.That(context.PlayerRigidbody.isKinematic, Is.False);

            var restBandPositions = CreateRawTwoSpanBandPositions(
                context.Geometry,
                context.Geometry.RestPoint,
                context.BandLineRenderer.positionCount);

            var observedRestBandDetach = false;

            for (var frameIndex = 0; frameIndex < 32; frameIndex += 1)
            {
                yield return null;

                var recoilBandPositions = ReadWorldLinePositions(context.BandLineRenderer);

                var isRestBandClear = IsBandShapeClearOfCollider(
                    restBandPositions,
                    context.BandLineRenderer,
                    context.TargetCollider);

                Assert.That(recoilBandPositions, Has.Length.GreaterThan(3));

                AssertBandPathOffsetsAreMonotonic(
                    recoilBandPositions,
                    context.BandLineRenderer,
                    context.Geometry,
                    $"Max Pull Band Release Recoil frame {frameIndex}");

                AssertBandPathHasNoSharpFolds(
                    recoilBandPositions,
                    context.Geometry,
                    $"Max Pull Band Release Recoil frame {frameIndex}");

                AssertBandSamplesStayOutsideCollider(
                    recoilBandPositions,
                    context.BandLineRenderer,
                    context.TargetCollider,
                    $"Max Pull Band Release Recoil frame {frameIndex}");

                var visibleBandMatchesRest = DoesBandShapeMatchRawTwoSpan(
                    recoilBandPositions,
                    context.Geometry,
                    context.Geometry.RestPoint,
                    0.01f);

                if (!isRestBandClear && visibleBandMatchesRest)
                {
                    Assert.Fail(
                        "Max Pull Band Release Recoil detached to rest while the launched Target blocks the rest Band Shape at frame "
                        + $"{frameIndex} targetCenter={DescribePoint(context.TargetCollider.bounds.center, context.Geometry)} "
                        + DescribeBandOffsets(recoilBandPositions, context.Geometry));
                }

                if (observedRestBandDetach && !visibleBandMatchesRest)
                {
                    Assert.Fail(
                        "Max Pull Band Release Recoil returned to a live wrap after already detaching to rest at frame "
                        + $"{frameIndex} targetCenter={DescribePoint(context.TargetCollider.bounds.center, context.Geometry)} "
                        + DescribeBandOffsets(recoilBandPositions, context.Geometry));
                }

                if (!visibleBandMatchesRest)
                    continue;

                observedRestBandDetach = true;

                AssertBandShapeMatchesRawTwoSpan(
                    recoilBandPositions,
                    context.Geometry,
                    context.Geometry.RestPoint,
                    0.01f,
                    "Max Pull Band Release Recoil cleared frame "
                    + $"{frameIndex} targetCenter={DescribePoint(context.TargetCollider.bounds.center, context.Geometry)} "
                    + DescribeBandOffsets(recoilBandPositions, context.Geometry));
            }

            Assert.That(
                observedRestBandDetach,
                Is.True,
                "Expected maximum launch recoil to detach to the rest Band Shape within the observed recoil frames.");
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
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertTargetColliderClampsInsideAnchorSpan(context, mouse, 1f);
            yield return AssertTargetColliderClampsInsideAnchorSpan(context, mouse, -1f);

            yield return SendMouse(mouse, context.PressScreenPosition, false);
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
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertRenderedAnchorSpan(context, mouse, 0.85f, 0.35f, "Right Shallow Side Pull");
            yield return AssertRenderedAnchorSpan(context, mouse, -0.85f, 0.35f, "Left Shallow Side Pull");
            yield return AssertRenderedAnchorSpan(context, mouse, 0.85f, 1.25f, "Right Deep Side Pull");
            yield return AssertRenderedAnchorSpan(context, mouse, -0.85f, 1.25f, "Left Deep Side Pull");

            yield return SendMouse(mouse, context.PressScreenPosition, false);
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
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertPulledWrapAlignsWithBandCenter(context, mouse, 0.75f, 1.25f, "Right Deep Pull");
            yield return AssertPulledWrapAlignsWithBandCenter(context, mouse, -0.75f, 1.25f, "Left Deep Pull");
            yield return AssertPulledWrapAlignsWithBandCenter(context, mouse, 0.75f, 2.25f, "Right Long Pull");
            yield return AssertPulledWrapAlignsWithBandCenter(context, mouse, -0.75f, 2.25f, "Left Long Pull");

            yield return SendMouse(mouse, context.PressScreenPosition, false);
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
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertAnchorSideSpanDoesNotBacktrack(context, mouse, 0.75f, 0.35f, "Right Shallow Pull");
            yield return AssertAnchorSideSpanDoesNotBacktrack(context, mouse, -0.75f, 0.35f, "Left Shallow Pull");
            yield return AssertAnchorSideSpanDoesNotBacktrack(context, mouse, 0.75f, 1.25f, "Right Deep Pull");
            yield return AssertAnchorSideSpanDoesNotBacktrack(context, mouse, -0.75f, 1.25f, "Left Deep Pull");

            yield return SendMouse(mouse, context.PressScreenPosition, false);
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
            yield return LoadGameplayScene();
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            yield return AssertBandPathDoesNotBacktrackAcrossTarget(context, mouse, 0.75f, 0.5f, "Right First Taut Pull");
            yield return AssertBandPathDoesNotBacktrackAcrossTarget(context, mouse, -0.75f, 0.5f, "Left First Taut Pull");

            yield return SendMouse(mouse, context.PressScreenPosition, false);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    private IEnumerator AssertBandPathDoesNotBacktrackAcrossTarget(
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

        AssertBandShapeDoesNotMatchRawTwoSpan(activeBandPositions, context.Geometry, phase);
        AssertBandPathOffsetsAreMonotonic(activeBandPositions, context.BandLineRenderer, context.Geometry, phase);
        AssertBandPathHasNoSharpFolds(activeBandPositions, context.Geometry, phase);
        AssertBandSamplesStayOutsideCollider(activeBandPositions, context.BandLineRenderer, context.TargetCollider, phase);
    }

    private IEnumerator AssertAnchorSideSpanDoesNotBacktrack(
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

        AssertAnchorSideHalfStaysOnPulledSide(
            activeBandPositions,
            context.BandLineRenderer,
            context.BandCenter.transform.position,
            context.Geometry,
            pullOffset,
            phase);
    }

    private IEnumerator AssertPulledWrapAlignsWithBandCenter(
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
        Assert.That(activeBandPositions.Length % 2, Is.EqualTo(1), $"{phase} Band Shape should have an explicit middle wrap point.");

        var actualBandCenterOffset = Vector3.Dot(context.BandCenter.transform.position - context.Geometry.RestPoint,
            context.Geometry.LaunchFrameRight);

        var actualBandCenterDepth = Vector3.Dot(context.BandCenter.transform.position - context.Geometry.RestPoint,
            -context.Geometry.LaunchFrameForward);
        var middleWrapPoint = activeBandPositions[(activeBandPositions.Length - 1) / 2];
        var middleWrapOffset = Vector3.Dot(middleWrapPoint - context.Geometry.RestPoint, context.Geometry.LaunchFrameRight);
        var middleWrapDepth = Vector3.Dot(middleWrapPoint - context.Geometry.RestPoint, -context.Geometry.LaunchFrameForward);
        var renderedTolerance = GetMaximumRenderedBandRadius(context.BandLineRenderer) + 0.04f;

        Assert.That(
            Mathf.Abs(middleWrapOffset - actualBandCenterOffset),
            Is.LessThanOrEqualTo(renderedTolerance),
            $"{phase} pulled Band wrap should stay laterally aligned with the authored Band Center.");

        Assert.That(
            Mathf.Abs(middleWrapDepth - actualBandCenterDepth),
            Is.LessThanOrEqualTo(0.15f),
            $"{phase} pulled Band wrap should stay near the authored Band Center depth.");
    }

    private IEnumerator AssertShallowLateralBandShape(GameplaySceneContext context, Mouse mouse, float pullOffset, float pullDepth)
    {
        var pullWorldPosition = context.Geometry.RestPoint
                                + (context.Geometry.LaunchFrameRight * pullOffset)
                                - (context.Geometry.LaunchFrameForward * pullDepth);
        var pullScreenPosition = GetScreenPosition(context.InputCamera, pullWorldPosition);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));

        var visualCenterPoint = activeBandPositions[(activeBandPositions.Length - 1) / 2];

        AssertSimpleBandVisualCenterMatchesBandCenter(
            visualCenterPoint,
            context.BandCenter.transform.position,
            context.BandLineRenderer,
            context.Geometry,
            $"Shallow Lateral Pull offset {pullOffset} depth {pullDepth}");

        AssertBandShapeMatchesRawTwoSpan(activeBandPositions, context.Geometry, visualCenterPoint, 0.01f,
            $"Shallow Lateral Pull offset {pullOffset} depth {pullDepth}");
    }

    private IEnumerator AssertTargetColliderClampsInsideAnchorSpan(GameplaySceneContext context, Mouse mouse, float lateralSign)
    {
        var leftAnchorOffset = Vector3.Dot(
            context.Geometry.LeftAnchorPosition - context.Geometry.RestPoint,
            context.Geometry.LaunchFrameRight);

        var rightAnchorOffset = Vector3.Dot(
            context.Geometry.RightAnchorPosition - context.Geometry.RestPoint,
            context.Geometry.LaunchFrameRight);
        var sideAnchorOffset = lateralSign > 0f ? rightAnchorOffset : leftAnchorOffset;

        var beyondAnchorWorldPosition = context.Geometry.RestPoint
                                        + (context.Geometry.LaunchFrameRight * (sideAnchorOffset + lateralSign))
                                        - (context.Geometry.LaunchFrameForward * 1.25f);
        var beyondAnchorScreenPosition = GetScreenPosition(context.InputCamera, beyondAnchorWorldPosition);

        yield return SendMouse(mouse, beyondAnchorScreenPosition, true);

        var bandCenterOffset = Vector3.Dot(
            context.BandCenter.transform.position - context.Geometry.RestPoint,
            context.Geometry.LaunchFrameRight);

        if (lateralSign > 0f)
            Assert.That(bandCenterOffset, Is.LessThan(rightAnchorOffset - 0.05f));
        else
            Assert.That(bandCenterOffset, Is.GreaterThan(leftAnchorOffset + 0.05f));

        AssertTargetColliderWithinAnchorSpan(context.TargetCollider, context.Geometry, $"Lateral Clamp {lateralSign}");
    }

    private IEnumerator AssertRenderedAnchorSpan(
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

        AssertTargetColliderWithinRenderedAnchorSpan(
            context.TargetCollider,
            context.BandCenter.transform.position,
            context.BandLineRenderer,
            context.Geometry,
            phase);
        AssertBandCenterlineWithinAnchorSpan(activeBandPositions, context.Geometry, phase);
        AssertBandSamplesStayOutsideCollider(activeBandPositions, context.BandLineRenderer, context.TargetCollider, phase);
    }

    private IEnumerator AssertPulledBandShape(
        GameplaySceneContext context,
        Mouse mouse,
        float pullOffset,
        bool releaseAndAssertRecoil)
    {
        var pullWorldPosition = context.Geometry.RestPoint
                                + (context.Geometry.LaunchFrameRight * pullOffset)
                                - (context.Geometry.LaunchFrameForward * 1.25f);
        var pullScreenPosition = GetScreenPosition(context.InputCamera, pullWorldPosition);
        yield return SendMouse(mouse, pullScreenPosition, true);

        var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
        Assert.That(activeBandPositions, Has.Length.GreaterThan(3));
        AssertPulledSideWrap(activeBandPositions, pullWorldPosition, context.Geometry.LaunchFrameRight, pullOffset);
        AssertBandSamplesStayOutsideCollider(activeBandPositions, context.BandLineRenderer, context.TargetCollider, "Active Pull");

        if (!releaseAndAssertRecoil)
            yield break;

        yield return SendMouse(mouse, pullScreenPosition, false);
        yield return null;

        var recoilBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
        Assert.That(recoilBandPositions, Has.Length.GreaterThan(3));
        AssertBandSamplesStayOutsideCollider(recoilBandPositions, context.BandLineRenderer, context.TargetCollider, "Band Release Recoil");
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

        var geometry = slingshotViews[0].CreateGeometrySnapshot();
        return Vector3.Distance(bandCenter.transform.position, geometry.RestPoint) <= 0.05f;
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

    private bool IsBandShapeClearOfCollider(Vector3[] bandPositions, LineRenderer bandLineRenderer, Collider targetCollider)
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

                if (surfaceDistance <= requiredClearance)
                    return false;
            }
        }

        return true;
    }

    private void PinTargetColliderCenter(GameplaySceneContext context, Vector3 colliderCenterPosition)
    {
        var rigidbodyToColliderCenter = context.TargetCollider.bounds.center - context.PlayerRigidbody.position;
        context.PlayerRigidbody.position = colliderCenterPosition - rigidbodyToColliderCenter;
        context.PlayerRigidbody.linearVelocity = context.Geometry.LaunchFrameForward * 0.05f;
        context.PlayerRigidbody.angularVelocity = Vector3.zero;
        Physics.SyncTransforms();
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
        Assert.That(
            GetMaximumDistanceFromRawTwoSpan(bandPositions, geometry, centerPoint),
            Is.LessThanOrEqualTo(tolerance),
            $"{phase} Band Shape should use clean two-span geometry near rest.");
    }

    private bool DoesBandShapeMatchRawTwoSpan(
        Vector3[] bandPositions,
        SlingshotGeometrySnapshot geometry,
        Vector3 centerPoint,
        float tolerance)
    {
        return GetMaximumDistanceFromRawTwoSpan(bandPositions, geometry, centerPoint) <= tolerance;
    }

    private float GetMaximumDistanceFromRawTwoSpan(Vector3[] bandPositions, SlingshotGeometrySnapshot geometry, Vector3 centerPoint)
    {
        var middleIndex = (bandPositions.Length - 1) / 2;
        var lastIndex = bandPositions.Length - 1;
        var maximumDistance = 0f;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
            var expectedPoint = Vector3.Lerp(geometry.LeftAnchorPosition, centerPoint, progress);
            maximumDistance = Mathf.Max(maximumDistance, Vector3.Distance(bandPositions[pointIndex], expectedPoint));
        }

        for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
        {
            var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
            var expectedPoint = Vector3.Lerp(centerPoint, geometry.RightAnchorPosition, progress);
            maximumDistance = Mathf.Max(maximumDistance, Vector3.Distance(bandPositions[pointIndex], expectedPoint));
        }

        return maximumDistance;
    }

    private Vector3[] CreateRawTwoSpanBandPositions(SlingshotGeometrySnapshot geometry, Vector3 centerPoint, int pointCount)
    {
        var positions = new Vector3[pointCount];
        var middleIndex = (positions.Length - 1) / 2;
        var lastIndex = positions.Length - 1;

        for (var pointIndex = 0; pointIndex <= middleIndex; pointIndex += 1)
        {
            var progress = middleIndex <= 0 ? 1f : (float)pointIndex / middleIndex;
            positions[pointIndex] = Vector3.Lerp(geometry.LeftAnchorPosition, centerPoint, progress);
        }

        for (var pointIndex = middleIndex + 1; pointIndex <= lastIndex; pointIndex += 1)
        {
            var progress = (float)(pointIndex - middleIndex) / (lastIndex - middleIndex);
            positions[pointIndex] = Vector3.Lerp(centerPoint, geometry.RightAnchorPosition, progress);
        }

        return positions;
    }

    private GameplaySceneContext CreateSceneContext(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(scene, "SlingshotView");
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
