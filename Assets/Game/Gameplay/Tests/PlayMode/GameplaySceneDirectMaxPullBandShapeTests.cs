using System.Collections;
using Game.Gameplay.Tests.Common;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static GameplaySceneDirectMaxPullBandShapeAssertions;
using static GameplaySceneBandShapePlayModeTestUtils;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneDirectMaxPullBandShapeTests : BaseGameplayTestAssetsFixture
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_EditorMousePullsDirectlyToMaximumDistance_then_BandPathStaysVisibleSymmetricAndStableAcrossFrames()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Direct Max Pull Mouse");

        try
        {
            yield return LoadGameplayScene(TestAssets.GameplaySceneRef);
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            var renderedBandRadius = GetMaximumRenderedBandRadius(context.BandLineRenderer);
            var nearCenteredJitterOffset = Mathf.Max(renderedBandRadius * 0.75f, 0.01f);

            var observedPullOffsets = new[]
            {
                0f,
                0f,
                nearCenteredJitterOffset,
                -nearCenteredJitterOffset,
                nearCenteredJitterOffset * 1.5f,
                -nearCenteredJitterOffset * 1.5f,
                0f,
                0f
            };
            Vector3[] previousFrameBandPositions = null;
            var lastMaximumPullScreenPosition = context.PressScreenPosition;

            for (var frameIndex = 0; frameIndex < observedPullOffsets.Length; frameIndex += 1)
            {
                var requestedPullOffset = observedPullOffsets[frameIndex];

                var maximumPullWorldPosition = context.Geometry.RestPoint
                                               + (context.Geometry.LaunchFrameRight * requestedPullOffset)
                                               - (context.Geometry.LaunchFrameForward * context.SlingshotConfig.MaximumPullDistance);
                var maximumPullScreenPosition = GetScreenPosition(context.InputCamera, maximumPullWorldPosition);
                lastMaximumPullScreenPosition = maximumPullScreenPosition;

                yield return SendMouse(mouse, maximumPullScreenPosition, true);
                yield return null;

                var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
                var screenshotPath = TrySaveCameraCapture(context.InputCamera, $"direct-max-pull-frame-{frameIndex:00}.png");
                var diagnostics = CreateFrameDiagnostics(context, activeBandPositions, frameIndex, requestedPullOffset, screenshotPath);
                Debug.Log(diagnostics);

                Assert.That(activeBandPositions, Has.Length.GreaterThan(3), diagnostics);

                AssertDirectMaxPullBandUsesCenteredTautWrap(
                    context,
                    activeBandPositions,
                    $"Direct Maximum Pull frame {frameIndex}",
                    diagnostics);

                if (Mathf.Abs(requestedPullOffset) <= 0.0001f)
                {
                    AssertBandShapeIsSymmetricAroundBandCenter(
                        activeBandPositions,
                        context,
                        $"Direct Maximum Pull frame {frameIndex}",
                        diagnostics);
                }

                if (previousFrameBandPositions != null)
                {
                    AssertBandShapeStableFromPreviousFrame(
                        context,
                        previousFrameBandPositions,
                        activeBandPositions,
                        requestedPullOffset - observedPullOffsets[frameIndex - 1],
                        $"Direct Maximum Pull frame {frameIndex}",
                        diagnostics);
                }

                previousFrameBandPositions = activeBandPositions;
            }

            yield return SendMouse(mouse, lastMaximumPullScreenPosition, false);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator
        given_GameplayScene_when_EditorMouseDragsStraightDownPastMaximumDistance_then_BandPathStaysVisibleCenteredAndStableAcrossFrames()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Direct Max Pull Screen Mouse");

        try
        {
            yield return LoadGameplayScene(TestAssets.GameplaySceneRef);
            var context = CreateSceneContext(SceneManager.GetActiveScene());
            var inputProjector = new SlingshotInputProjector(context.InputCamera);
            yield return WaitUntilPlayerIsHeld(context);
            yield return SendMouse(mouse, context.PressScreenPosition, true);

            var maximumPullWorldPosition = context.Geometry.RestPoint
                                           - (context.Geometry.LaunchFrameForward * context.SlingshotConfig.MaximumPullDistance);
            var maximumPullScreenPosition = GetScreenPosition(context.InputCamera, maximumPullWorldPosition);
            var maximumPullScreenDeltaY = maximumPullScreenPosition.y - context.PressScreenPosition.y;

            Assert.That(
                Mathf.Abs(maximumPullScreenDeltaY),
                Is.GreaterThan(1f),
                "Expected direct max-pull to move vertically enough on screen for a straight-down drag diagnostic.");

            var screenPullProgresses = new[]
            {
                1f,
                1.15f,
                1.3f,
                1.45f,
                1.3f,
                1.15f,
                1f
            };
            Vector3[] previousFrameBandPositions = null;
            Vector3 previousRawProjectedPullPoint = maximumPullWorldPosition;
            var lastScreenPosition = context.PressScreenPosition;

            for (var frameIndex = 0; frameIndex < screenPullProgresses.Length; frameIndex += 1)
            {
                var screenPosition = new Vector2(
                    context.PressScreenPosition.x,
                    context.PressScreenPosition.y + (maximumPullScreenDeltaY * screenPullProgresses[frameIndex]));
                lastScreenPosition = screenPosition;

                Assert.That(
                    ((ISlingshotInputProjector)inputProjector).TryProjectScreenToPullPlane(screenPosition, context.Geometry,
                        out var rawProjectedPullPoint),
                    Is.True,
                    $"Expected screen drag frame {frameIndex} to project onto the Slingshot pull plane.");

                yield return SendMouse(mouse, screenPosition, true);
                yield return null;

                var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);
                var screenshotPath = TrySaveCameraCapture(context.InputCamera, $"direct-max-screen-drag-frame-{frameIndex:00}.png");

                var diagnostics = CreateScreenDragFrameDiagnostics(
                    context,
                    activeBandPositions,
                    frameIndex,
                    screenPosition,
                    rawProjectedPullPoint,
                    screenshotPath);
                Debug.Log(diagnostics);

                Assert.That(activeBandPositions, Has.Length.GreaterThan(3), diagnostics);

                AssertRawProjectedPullReachedMaximumDepth(context, rawProjectedPullPoint, $"Screen Direct Maximum Pull frame {frameIndex}",
                    diagnostics);

                AssertDirectMaxPullBandUsesCenteredTautWrap(
                    context,
                    activeBandPositions,
                    $"Screen Direct Maximum Pull frame {frameIndex}",
                    diagnostics);

                AssertBandShapeIsSymmetricAroundBandCenter(
                    activeBandPositions,
                    context,
                    $"Screen Direct Maximum Pull frame {frameIndex}",
                    diagnostics);

                if (previousFrameBandPositions != null)
                {
                    var rawPullOffsetDelta = ProjectOffset(rawProjectedPullPoint, context.Geometry)
                                             - ProjectOffset(previousRawProjectedPullPoint, context.Geometry);

                    AssertBandShapeStableFromPreviousFrame(
                        context,
                        previousFrameBandPositions,
                        activeBandPositions,
                        rawPullOffsetDelta,
                        $"Screen Direct Maximum Pull frame {frameIndex}",
                        diagnostics);
                }

                previousFrameBandPositions = activeBandPositions;
                previousRawProjectedPullPoint = rawProjectedPullPoint;
            }

            yield return SendMouse(mouse, lastScreenPosition, false);
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator
        given_GameplayScene_when_DirectMaximumPullRepeatsAcrossSceneReloadsWithScreenJitter_then_BandTopologyStaysStable()
    {
        var screenJitterPixels = new[]
        {
            0f,
            0.25f,
            -0.25f,
            1f,
            -1f,
            2f,
            -2f,
            0f
        };
        Vector3[][] referenceBandPositionsByJitter = null;

        for (var reloadAttempt = 0; reloadAttempt < 4; reloadAttempt += 1)
        {
            var mouse = InputSystem.AddDevice<Mouse>($"Gameplay Scene Direct Max Pull Reload Mouse {reloadAttempt}");

            try
            {
                yield return ReloadGameplayScene(TestAssets.GameplaySceneRef);
                var context = CreateSceneContext(SceneManager.GetActiveScene());
                var inputProjector = new SlingshotInputProjector(context.InputCamera);
                yield return WaitUntilPlayerIsHeld(context);
                yield return SendMouse(mouse, context.PressScreenPosition, true);

                var maximumPullWorldPosition = context.Geometry.RestPoint
                                               - (context.Geometry.LaunchFrameForward * context.SlingshotConfig.MaximumPullDistance);
                var maximumPullScreenPosition = GetScreenPosition(context.InputCamera, maximumPullWorldPosition);
                var maximumPullScreenDeltaY = maximumPullScreenPosition.y - context.PressScreenPosition.y;
                var lastScreenPosition = context.PressScreenPosition;
                Vector3[] previousFrameBandPositions = null;
                Vector3 previousRawProjectedPullPoint = maximumPullWorldPosition;

                if (referenceBandPositionsByJitter == null)
                    referenceBandPositionsByJitter = new Vector3[screenJitterPixels.Length][];

                for (var jitterIndex = 0; jitterIndex < screenJitterPixels.Length; jitterIndex += 1)
                {
                    var screenPosition = new Vector2(
                        maximumPullScreenPosition.x + screenJitterPixels[jitterIndex],
                        context.PressScreenPosition.y + (maximumPullScreenDeltaY * 1.3f));
                    lastScreenPosition = screenPosition;

                    Assert.That(
                        ((ISlingshotInputProjector)inputProjector).TryProjectScreenToPullPlane(screenPosition, context.Geometry,
                            out var rawProjectedPullPoint),
                        Is.True,
                        $"Expected reload attempt {reloadAttempt} jitter {jitterIndex} to project onto the Slingshot pull plane.");

                    yield return SendMouse(mouse, screenPosition, true);
                    yield return null;

                    var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);

                    var screenshotPath = TrySaveCameraCapture(
                        context.InputCamera,
                        $"direct-max-reload-{reloadAttempt:00}-jitter-{jitterIndex:00}.png");
                    var frameIndex = (reloadAttempt * screenJitterPixels.Length) + jitterIndex;

                    var diagnostics = CreateScreenDragFrameDiagnostics(
                        context,
                        activeBandPositions,
                        frameIndex,
                        screenPosition,
                        rawProjectedPullPoint,
                        screenshotPath);
                    Debug.Log(diagnostics);

                    Assert.That(activeBandPositions, Has.Length.GreaterThan(3), diagnostics);

                    AssertRawProjectedPullReachedMaximumDepth(
                        context,
                        rawProjectedPullPoint,
                        $"Reloaded Direct Maximum Pull attempt {reloadAttempt} jitter {jitterIndex}",
                        diagnostics);

                    AssertDirectMaxPullBandUsesCenteredTautWrap(
                        context,
                        activeBandPositions,
                        $"Reloaded Direct Maximum Pull attempt {reloadAttempt} jitter {jitterIndex}",
                        diagnostics);

                    if (Mathf.Abs(ProjectOffset(rawProjectedPullPoint, context.Geometry)) <= 0.01f)
                    {
                        AssertBandShapeIsSymmetricAroundBandCenter(
                            activeBandPositions,
                            context,
                            $"Reloaded Direct Maximum Pull attempt {reloadAttempt} jitter {jitterIndex}",
                            diagnostics);
                    }

                    if (previousFrameBandPositions != null)
                    {
                        var rawPullOffsetDelta = ProjectOffset(rawProjectedPullPoint, context.Geometry)
                                                 - ProjectOffset(previousRawProjectedPullPoint, context.Geometry);

                        AssertBandShapeStableFromPreviousFrame(
                            context,
                            previousFrameBandPositions,
                            activeBandPositions,
                            rawPullOffsetDelta,
                            $"Reloaded Direct Maximum Pull attempt {reloadAttempt} jitter {jitterIndex}",
                            diagnostics);
                    }

                    if (reloadAttempt == 0)
                    {
                        referenceBandPositionsByJitter[jitterIndex] = activeBandPositions;
                    }
                    else
                    {
                        AssertBandShapeStableFromPreviousFrame(
                            context,
                            referenceBandPositionsByJitter[jitterIndex],
                            activeBandPositions,
                            0f,
                            $"Reloaded Direct Maximum Pull attempt {reloadAttempt} jitter {jitterIndex}",
                            diagnostics);
                    }

                    previousFrameBandPositions = activeBandPositions;
                    previousRawProjectedPullPoint = rawProjectedPullPoint;
                }

                yield return SendMouse(mouse, lastScreenPosition, false);
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
            }
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_MaximumPullRepeatsAtSideLimitsWithScreenJitter_then_BandPathStaysVisible()
    {
        var sideSigns = new[]
        {
            1f,
            -1f
        };

        var screenJitterPixels = new[]
        {
            0f,
            1f,
            -1f,
            2f,
            -2f,
            0f
        };
        Vector3[][][] referenceBandPositionsBySideAndJitter = null;

        for (var reloadAttempt = 0; reloadAttempt < 4; reloadAttempt += 1)
        {
            var mouse = InputSystem.AddDevice<Mouse>($"Gameplay Scene Maximum Side Pull Reload Mouse {reloadAttempt}");

            try
            {
                yield return ReloadGameplayScene(TestAssets.GameplaySceneRef);
                var context = CreateSceneContext(SceneManager.GetActiveScene());
                var inputProjector = new SlingshotInputProjector(context.InputCamera);
                yield return WaitUntilPlayerIsHeld(context);
                yield return SendMouse(mouse, context.PressScreenPosition, true);
                var lastHeldScreenPosition = context.PressScreenPosition;

                if (referenceBandPositionsBySideAndJitter == null)
                {
                    referenceBandPositionsBySideAndJitter = new Vector3[sideSigns.Length][][];

                    for (var sideIndex = 0; sideIndex < sideSigns.Length; sideIndex += 1)
                    {
                        referenceBandPositionsBySideAndJitter[sideIndex] = new Vector3[screenJitterPixels.Length][];
                    }
                }

                for (var sideIndex = 0; sideIndex < sideSigns.Length; sideIndex += 1)
                {
                    var sideSign = sideSigns[sideIndex];

                    var maximumSidePullWorldPosition = context.Geometry.RestPoint
                                                       + (context.Geometry.LaunchFrameRight
                                                          * context.SlingshotConfig.MaximumLateralPull
                                                          * sideSign)
                                                       - (context.Geometry.LaunchFrameForward
                                                          * context.SlingshotConfig.MaximumPullDistance);
                    var maximumSidePullScreenPosition = GetScreenPosition(context.InputCamera, maximumSidePullWorldPosition);
                    var maximumSidePullScreenDelta = maximumSidePullScreenPosition - context.PressScreenPosition;
                    Vector3[] previousFrameBandPositions = null;
                    Vector3 previousRawProjectedPullPoint = maximumSidePullWorldPosition;

                    for (var jitterIndex = 0; jitterIndex < screenJitterPixels.Length; jitterIndex += 1)
                    {
                        var screenPosition = context.PressScreenPosition
                                             + (maximumSidePullScreenDelta * 1.3f)
                                             + new Vector2(screenJitterPixels[jitterIndex], 0f);
                        lastHeldScreenPosition = screenPosition;

                        Assert.That(
                            ((ISlingshotInputProjector)inputProjector).TryProjectScreenToPullPlane(screenPosition, context.Geometry,
                                out var rawProjectedPullPoint),
                            Is.True,
                            $"Expected reload attempt {reloadAttempt} side {sideSign} jitter {jitterIndex} to project onto the Slingshot pull plane.");

                        yield return SendMouse(mouse, screenPosition, true);
                        yield return null;

                        var activeBandPositions = ReadWorldLinePositions(context.BandLineRenderer);

                        var screenshotPath = TrySaveCameraCapture(
                            context.InputCamera,
                            $"max-side-reload-{reloadAttempt:00}-side-{sideIndex:00}-jitter-{jitterIndex:00}.png");
                        var frameIndex = ((reloadAttempt * sideSigns.Length) + sideIndex) * screenJitterPixels.Length + jitterIndex;

                        var diagnostics = CreateScreenDragFrameDiagnostics(
                            context,
                            activeBandPositions,
                            frameIndex,
                            screenPosition,
                            rawProjectedPullPoint,
                            screenshotPath);
                        Debug.Log(diagnostics);

                        Assert.That(activeBandPositions, Has.Length.GreaterThan(3), diagnostics);

                        AssertRawProjectedPullReachedMaximumDepth(
                            context,
                            rawProjectedPullPoint,
                            $"Reloaded Maximum Side Pull attempt {reloadAttempt} side {sideSign} jitter {jitterIndex}",
                            diagnostics);

                        AssertMaximumPullBandUsesTautClearVisibleWrap(
                            context,
                            activeBandPositions,
                            $"Reloaded Maximum Side Pull attempt {reloadAttempt} side {sideSign} jitter {jitterIndex}",
                            diagnostics);

                        if (previousFrameBandPositions != null)
                        {
                            var rawPullOffsetDelta = ProjectOffset(rawProjectedPullPoint, context.Geometry)
                                                     - ProjectOffset(previousRawProjectedPullPoint, context.Geometry);

                            AssertBandShapeStableFromPreviousFrame(
                                context,
                                previousFrameBandPositions,
                                activeBandPositions,
                                rawPullOffsetDelta,
                                $"Reloaded Maximum Side Pull attempt {reloadAttempt} side {sideSign} jitter {jitterIndex}",
                                diagnostics);
                        }

                        if (reloadAttempt == 0)
                        {
                            referenceBandPositionsBySideAndJitter[sideIndex][jitterIndex] = activeBandPositions;
                        }
                        else
                        {
                            AssertBandShapeStableFromPreviousFrame(
                                context,
                                referenceBandPositionsBySideAndJitter[sideIndex][jitterIndex],
                                activeBandPositions,
                                0f,
                                $"Reloaded Maximum Side Pull attempt {reloadAttempt} side {sideSign} jitter {jitterIndex}",
                                diagnostics);
                        }

                        previousFrameBandPositions = activeBandPositions;
                        previousRawProjectedPullPoint = rawProjectedPullPoint;
                    }

                    yield return SendMouse(mouse, context.PressScreenPosition, true);
                    yield return null;
                }

                yield return SendMouse(mouse, lastHeldScreenPosition, false);
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
            }
        }
    }
}
