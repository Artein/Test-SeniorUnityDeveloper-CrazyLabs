using System.Collections;
using System.Linq;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed partial class LadybugHalfTubeRunCourseSceneTests
{
    private const float ReferenceRunPullDistance = 2.95f;
    private const float ReferenceRunTimeoutSeconds = 75f;
    private const float ReferenceRouteLookAheadMeters = 14f;
    private const float ReferenceRouteSteeringGain = 0.45f;
    private const int ReferenceLaunchPowerLevel = 10;
    private const int ReferenceMaxSpeedLevel = 10;
    private const int ReferenceSteeringLevel = 10;

    private RunPlaybackContext CreateRunPlaybackContext(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(scene, "RigidbodyLaunchTarget");

        return new RunPlaybackContext(
            FindGameObjectByName(scene, _courseRootName),
            lifetimeScope,
            lifetimeScope.Container.Resolve<IGameplayStateService>(),
            lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>(),
            lifetimeScope.Container.Resolve<IRunResultNotifier>(),
            lifetimeScope.Container.Resolve<IRunProgressService>(),
            lifetimeScope.Container.Resolve<IUpgradeCatalog>(),
            launchTarget.GetComponent<Rigidbody>(),
            FindSingleInScene<SlingshotView>(scene, "SlingshotView"),
            FindSingleInScene<Camera>(scene, "Input Camera"),
            lifetimeScope.Container.Resolve<ISlingshotInputProjector>(),
            lifetimeScope.Container.Resolve<ISlingshotConfig>());
    }

    private IEnumerator PlayReferenceRouteRun(
        Mouse mouse,
        RunPlaybackContext context,
        RunPlaybackCapture capture,
        string runLabel)
    {
        void OnRunResultAccepted(RunResult result)
        {
            capture.AcceptedResult = result;
        }

        context.RunResultNotifier.RunResultAccepted += OnRunResultAccepted;

        try
        {
            Assert.That(context.StateService.CurrentStateId.name, Is.EqualTo("RunPreparationStateId"));
            Assert.That(context.ContinueCommand.TryContinue(), Is.True);

            yield return PullAndReleaseSlingshot(mouse, context);
            yield return WaitUntilStateName(context.StateService, "RunningStateId", 120);
            yield return WaitUntilPlayerLaunches(context.PlayerRigidbody);

            var routeAnchors = CreateReferenceSafeRouteAnchors();
            var fixedFrameLimit = Mathf.CeilToInt(ReferenceRunTimeoutSeconds / Mathf.Max(0.0001f, Time.fixedDeltaTime));

            for (var fixedFrameIndex = 0; fixedFrameIndex < fixedFrameLimit; fixedFrameIndex += 1)
            {
                if (capture.AcceptedResult.HasValue)
                    break;

                var steeringScreenPosition = GetReferenceRouteSteeringScreenPosition(context, routeAnchors);

                yield return SendMouse(mouse, steeringScreenPosition, true);
                yield return new WaitForFixedUpdate();
            }

            if (!capture.AcceptedResult.HasValue)
            {
                Assert.Fail(
                    $"{runLabel} timed out after {ReferenceRunTimeoutSeconds:0.#}s. "
                    + $"progress={context.ProgressService.MaximumForwardProgress:0.#}, "
                    + $"position={context.PlayerRigidbody.position}, "
                    + $"speed={context.PlayerRigidbody.linearVelocity.magnitude:0.##}");
            }
        }
        finally
        {
            context.RunResultNotifier.RunResultAccepted -= OnRunResultAccepted;
        }

        yield return SendMouse(mouse, GetSteeringScreenPosition(0f), false);
    }

    private IEnumerator PullAndReleaseSlingshot(Mouse mouse, RunPlaybackContext context)
    {
        var geometry = context.SlingshotView.CreateGeometrySnapshot();
        var pressScreenPosition = GetScreenPosition(context.InputCamera, geometry.RestPoint);
        var pullWorldPosition = geometry.RestPoint - (geometry.LaunchFrameForward * ReferenceRunPullDistance);
        var releaseScreenPosition = GetScreenPosition(context.InputCamera, pullWorldPosition);

        Assert.That(
            context.SlingshotInputProjector.TryProjectScreenToPullPlane(releaseScreenPosition, geometry, out _),
            Is.True);
        Assert.That(context.SlingshotConfig.MinimumPullDistance, Is.LessThan(ReferenceRunPullDistance));

        yield return SendMouse(mouse, pressScreenPosition, true);
        yield return SendMouse(mouse, releaseScreenPosition, true);
        yield return SendMouse(mouse, releaseScreenPosition, false);
    }

    private IEnumerator WaitUntilStateName(IGameplayStateService stateService, string stateName, int frameLimit)
    {
        for (var frameIndex = 0; frameIndex < frameLimit; frameIndex += 1)
        {
            if (stateService.CurrentStateId != null && stateService.CurrentStateId.name == stateName)
                yield break;

            yield return null;
        }

        Assert.Fail($"Expected Gameplay State '{stateName}', but current state is '{stateService.CurrentStateId?.name ?? "<null>"}'.");
    }

    private IEnumerator WaitUntilPlayerLaunches(Rigidbody playerRigidbody)
    {
        for (var frameIndex = 0; frameIndex < 60; frameIndex += 1)
        {
            if (!playerRigidbody.isKinematic && playerRigidbody.linearVelocity.sqrMagnitude > 0.0001f)
                yield break;

            yield return null;
        }

        Assert.Fail("Expected reference slingshot pull release to launch the Player.");
    }

    private Vector2 GetReferenceRouteSteeringScreenPosition(
        RunPlaybackContext context,
        (float Progress, float Lateral, string Description)[] routeAnchors)
    {
        var progress = context.ProgressService.HasValidSnapshot
            ? context.ProgressService.CurrentForwardProgress
            : context.PlayerRigidbody.position.z;
        var targetLateral = GetTargetLateral(routeAnchors, progress + ReferenceRouteLookAheadMeters);
        var lateralError = targetLateral - context.PlayerRigidbody.position.x;

        var desiredSteer = Mathf.Abs(lateralError) < 0.2f
            ? 0f
            : Mathf.Clamp(lateralError * ReferenceRouteSteeringGain, -1f, 1f);

        return GetSteeringScreenPosition(desiredSteer);
    }

    private float GetTargetLateral(
        (float Progress, float Lateral, string Description)[] routeAnchors,
        float progress)
    {
        Assert.That(routeAnchors, Has.Length.GreaterThan(1));

        if (progress <= routeAnchors[0].Progress)
            return routeAnchors[0].Lateral;

        for (var anchorIndex = 0; anchorIndex < routeAnchors.Length - 1; anchorIndex += 1)
        {
            var start = routeAnchors[anchorIndex];
            var end = routeAnchors[anchorIndex + 1];

            if (progress < start.Progress || progress > end.Progress)
                continue;

            var interpolation = Mathf.InverseLerp(start.Progress, end.Progress, progress);
            return Mathf.Lerp(start.Lateral, end.Lateral, interpolation);
        }

        return routeAnchors[^1].Lateral;
    }

    private Vector2 GetSteeringScreenPosition(float desiredSteer)
    {
        var screenWidth = Mathf.Max(1, Screen.width);
        var screenHeight = Mathf.Max(1, Screen.height);
        var screenX = screenWidth * (0.5f + (Mathf.Clamp(desiredSteer, -1f, 1f) * 0.5f));

        return new Vector2(screenX, screenHeight * 0.5f);
    }

    private Vector2 GetScreenPosition(Camera camera, Vector3 worldPosition)
    {
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        Assert.That(screenPosition.z, Is.GreaterThan(0f));
        return new Vector2(screenPosition.x, screenPosition.y);
    }

    private IEnumerator SendMouse(Mouse mouse, Vector2 screenPosition, bool isPressed)
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

    private void SetReferenceCompletionUpgradeLevels(RunPlaybackContext context)
    {
        SetReferenceUpgradeLevels(context, ReferenceLaunchPowerLevel, ReferenceMaxSpeedLevel, ReferenceSteeringLevel);
    }

    private void ResetReferenceUpgradeLevels(RunPlaybackContext context)
    {
        SetReferenceUpgradeLevels(context, 0, 0, 0);
    }

    private void SetReferenceUpgradeLevels(
        RunPlaybackContext context,
        int launchPowerLevel,
        int maxSpeedLevel,
        int steeringLevel)
    {
        var progressStorage = context.LifetimeScope.Container.Resolve<IUpgradeProgressStorage>();

        SetUpgradeLevel(context.Catalog, progressStorage, "slingshot-launch-power", launchPowerLevel);
        SetUpgradeLevel(context.Catalog, progressStorage, "player-max-speed", maxSpeedLevel);
        SetUpgradeLevel(context.Catalog, progressStorage, "player-steering-responsiveness", steeringLevel);
    }

    private void SetUpgradeLevel(
        IUpgradeCatalog catalog,
        IUpgradeProgressStorage progressStorage,
        string stableId,
        int level)
    {
        var definition = catalog.UpgradeDefinitions.Single(candidate => candidate.StableId == stableId);

        progressStorage.SetLevel(definition, level);

        Assert.That(progressStorage.GetLevel(definition), Is.EqualTo(level), stableId);
    }

    private (float Progress, float Lateral, string Description)[] CreateReferenceSafeRouteAnchors()
    {
        return new[]
        {
            (Progress: 8f, Lateral: 0f, Description: "launch settle"),
            (Progress: 38f, Lateral: 0f, Description: "Band 1 center settle"),
            (Progress: 66f, Lateral: 3.6f, Description: "pre-first-blocker right setup"),
            (Progress: 88f, Lateral: 3.6f, Description: "first center blocker right gap"),
            (Progress: 108f, Lateral: 0f, Description: "Band 2 recovery trough"),
            (Progress: 122f, Lateral: 3.6f, Description: "offset blocker safe-side setup"),
            (Progress: 146f, Lateral: 3.6f, Description: "offset blocker right gap"),
            (Progress: 160f, Lateral: 0f, Description: "required ramp setup"),
            (Progress: 176f, Lateral: 0f, Description: "required ramp takeoff"),
            (Progress: 194f, Lateral: 0f, Description: "required ramp landing"),
            (Progress: 216f, Lateral: 0f, Description: "post-ramp center fallback"),
            (Progress: 236f, Lateral: 0f, Description: "Band 3 pressure center fallback"),
            (Progress: 264f, Lateral: 0f, Description: "Band 4 transfer fallback"),
            (Progress: 278f, Lateral: 0f, Description: "right billboard bypass"),
            (Progress: 300f, Lateral: 3.8f, Description: "optional-ramp setup"),
            (Progress: 318f, Lateral: 4.05f, Description: "optional-ramp climb"),
            (Progress: 342f, Lateral: 4.05f, Description: "optional-ramp reward line"),
            (Progress: 356f, Lateral: 2.4f, Description: "finish approach coin transfer"),
            (Progress: 388f, Lateral: 0f, Description: "final funnel recovery"),
            (Progress: 404f, Lateral: 0f, Description: "final funnel"),
            (Progress: 416f, Lateral: 0f, Description: "finish gate"),
        };
    }

    private RunResult AssertCapturedRunResult(RunPlaybackCapture capture)
    {
        Assert.That(capture.AcceptedResult.HasValue, Is.True);
        return capture.AcceptedResult.Value;
    }

    private sealed class RunPlaybackCapture
    {
        public RunResult? AcceptedResult { get; set; }
    }

    private sealed class RunPlaybackContext
    {
        public RunPlaybackContext(
            GameObject courseRoot,
            GameplayLifetimeScope lifetimeScope,
            IGameplayStateService stateService,
            IRunPreparationContinueCommand continueCommand,
            IRunResultNotifier runResultNotifier,
            IRunProgressService progressService,
            IUpgradeCatalog catalog,
            Rigidbody playerRigidbody,
            SlingshotView slingshotView,
            Camera inputCamera,
            ISlingshotInputProjector slingshotInputProjector,
            ISlingshotConfig slingshotConfig)
        {
            CourseRoot = courseRoot;
            LifetimeScope = lifetimeScope;
            StateService = stateService;
            ContinueCommand = continueCommand;
            RunResultNotifier = runResultNotifier;
            ProgressService = progressService;
            Catalog = catalog;
            PlayerRigidbody = playerRigidbody;
            SlingshotView = slingshotView;
            InputCamera = inputCamera;
            SlingshotInputProjector = slingshotInputProjector;
            SlingshotConfig = slingshotConfig;
        }

        public GameObject CourseRoot { get; }
        public GameplayLifetimeScope LifetimeScope { get; }
        public IGameplayStateService StateService { get; }
        public IRunPreparationContinueCommand ContinueCommand { get; }
        public IRunResultNotifier RunResultNotifier { get; }
        public IRunProgressService ProgressService { get; }
        public IUpgradeCatalog Catalog { get; }
        public Rigidbody PlayerRigidbody { get; }
        public SlingshotView SlingshotView { get; }
        public Camera InputCamera { get; }
        public ISlingshotInputProjector SlingshotInputProjector { get; }
        public ISlingshotConfig SlingshotConfig { get; }
    }
}
