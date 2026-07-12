using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Game.Gameplay;
using Game.Gameplay.CharacterPresentation;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneCompositionTests : BaseGameplayScenePlayModeFixture
{
    private const string FinishThresholdMaterialName = "FinishThresholdCheckered";
    private const string FinishThresholdShaderName = "Game/Level/Finish Threshold Tiled Fade";
    private const string FinishThresholdBaseMapPropertyName = "_BaseMap";
    private const string FinishThresholdBottomAlphaPropertyName = "_BottomAlpha";
    private const string FinishThresholdTopAlphaPropertyName = "_TopAlpha";
    private const string FinishThresholdFadeExponentPropertyName = "_FadeExponent";

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_SlingshotPrelaunchCompositionIsReady()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return ContinueToPreLaunch(activeScene);
        yield return WaitUntilPlayerIsHeld(activeScene);

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, objectDescription: "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, objectDescription: "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, objectDescription: "RigidbodyLaunchTarget");

        var runBodyMovementTarget = FindSingleInScene<RigidbodyRunBodyMovementTarget>(
            activeScene,
            objectDescription: "RigidbodyRunBodyMovementTarget");

        var runBodyMovementTargets = FindComponentsInScene<RigidbodyRunBodyMovementTarget>(activeScene);

        var legacyMovementWriters = FindComponentsInScene<MonoBehaviour>(activeScene)
            .Where(component => component.GetType().Name == "PlayerSteeringController")
            .ToArray();

        var runCameraSource = FindSingleInScene<RigidbodyRunCameraSource>(activeScene, objectDescription: "RigidbodyRunCameraSource");
        var contactNotifier = FindSingleInScene<RigidbodyContactNotifier>(activeScene, objectDescription: "RigidbodyContactNotifier");
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(activeScene, objectDescription: "RunProgressFrameSource");

        var runSurfaceInstaller =
            FindSingleInScene<GameplayPhysicsSceneCompositionMonoInstaller>(
                activeScene,
                objectDescription: "GameplayPhysicsSceneCompositionMonoInstaller");

        var characterPresentationView =
            FindSingleInScene<CharacterPresentationView>(activeScene, objectDescription: "CharacterPresentationView");

        var runCameraAnchor = FindSingleInScene<TransformRunCameraAnchor>(activeScene, objectDescription: "Run Camera Anchor");
        var runCameraRig = FindSingleInScene<CinemachineRunCameraRig>(activeScene, objectDescription: "Run Camera Rig");
        var mainCamera = FindSingleInScene<Camera>(activeScene, objectDescription: "Main Camera");
        var brain = mainCamera.GetComponent<CinemachineBrain>();
        var runPreparationCamera = FindGameObjectByName(activeScene, objectName: "Run Preparation Camera").GetComponent<CinemachineCamera>();
        var preLaunchCamera = FindGameObjectByName(activeScene, objectName: "Pre-Launch Camera").GetComponent<CinemachineCamera>();
        var runCamera = FindGameObjectByName(activeScene, objectName: "Run Camera").GetComponent<CinemachineCamera>();
        var runPreparationHardLookAt = runPreparationCamera.GetComponent<CinemachineHardLookAt>();
        var preLaunchHardLookAt = preLaunchCamera.GetComponent<CinemachineHardLookAt>();
        var thirdPersonFollow = runCamera.GetComponent<CinemachineThirdPersonFollow>();
        var decollider = runCamera.GetComponent<CinemachineDecollider>();
        var deoccluder = runCamera.GetComponent<CinemachineDeoccluder>();
        var surface = FindGameObjectByName(activeScene, objectName: "Surface");
        var surfaceContact = surface.GetComponent<RunContact>();
        var runContactsRoot = FindGameObjectByName(activeScene, objectName: "Run Contacts");
        TryFindGameObjectByName(activeScene, objectName: "Run Finish", out var legacyRunFinish);
        var runSafetyNet = FindGameObjectByName(activeScene, objectName: "Run Safety Net");
        var runObstacle = FindGameObjectByName(activeScene, objectName: "Run Obstacle");
        var authoritativeRunFinish = FindGameObjectByName(activeScene, objectName: "Band 5 Run Finish");
        var finishPresentationRoot = FindGameObjectByName(activeScene, objectName: "Finish Presentation");
        var finishThresholdVisual = FindGameObjectByName(activeScene, objectName: "Finish Threshold Visual");
        var finishPresentationView = FindSingleInScene<FinishPresentationView>(activeScene, objectDescription: "FinishPresentationView");
        var canvas = FindSingleInScene<Canvas>(activeScene, objectDescription: "Gameplay UI Canvas");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var targetCollider = GetSingleTargetCollider(launchTarget);
        var geometry = slingshotView.CreateGeometrySnapshot();
        var bandCenter = FindGameObjectByName(activeScene, objectName: "Band Center");
        var launchTargetColliderRoot = FindGameObjectByName(activeScene, objectName: "LaunchTargetColliderRoot");
        var runBodyContactColliderRoot = FindGameObjectByName(activeScene, objectName: "RunBodyContactColliderRoot");
        var runBodyContactCollider = runBodyContactColliderRoot.GetComponent<SphereCollider>();
        var characterVisualAnchor = FindGameObjectByName(activeScene, objectName: "CharacterVisualAnchor");
        var playerCameraLookTarget = FindGameObjectByName(activeScene, objectName: "Player Camera Look Target");
        var ladybugCharacter = FindGameObjectByName(activeScene, objectName: "LadybugCharacter");
        var characterAnimator = ladybugCharacter.GetComponent<Animator>();

        var characterAnimationEventReceiver = ladybugCharacter.GetComponent<CharacterAnimationEventReceiver>();
        var preLaunchRigPoseRoot = FindGameObjectByName(activeScene, objectName: "Pre-Launch Rig Pose");
        var preLaunchSlingshotRigPose = FindGameObjectByName(activeScene, objectName: "Pre-Launch Slingshot Rig Pose");
        var preLaunchLaunchTargetPose = FindGameObjectByName(activeScene, objectName: "Pre-Launch Launch Target Pose");
        var pullHint = FindGameObjectByName(activeScene, objectName: "Pull Hint");
        var pullHintView = pullHint.GetComponent<PullHintView>();
        var pullHintRectTransform = pullHint.GetComponent<RectTransform>();
        var pullHintCanvasGroup = pullHint.GetComponent<CanvasGroup>();
        var pullHintAnimator = pullHint.GetComponent<Animator>();
        var pullHintFinger = pullHint.transform.Find(n: "Finger");
        var pullHintFingerRectTransform = pullHintFinger != null ? pullHintFinger.GetComponent<RectTransform>() : null;
        var pullHintFingerImage = pullHintFinger != null ? pullHintFinger.GetComponent<Image>() : null;
        var runSteeringAffordance = FindGameObjectByName(activeScene, objectName: "Run Steering Affordance");
        var runSteeringAffordanceView = runSteeringAffordance.GetComponent<RunSteeringAffordanceView>();
        var runSteeringAffordanceRectTransform = runSteeringAffordance.GetComponent<RectTransform>();
        var runSteeringAffordanceCanvasGroup = runSteeringAffordance.GetComponent<CanvasGroup>();
        var runSteeringKnob = runSteeringAffordance.transform.Find(n: "Knob");
        var runSteeringLeftRangeEnd = runSteeringAffordance.transform.Find(n: "Left Range End Hint");
        var runSteeringRightRangeEnd = runSteeringAffordance.transform.Find(n: "Right Range End Hint");
        var runSteeringDeadzone = runSteeringAffordance.transform.Find(n: "Deadzone Hint");
        var runSteeringKnobRectTransform = runSteeringKnob != null ? runSteeringKnob.GetComponent<RectTransform>() : null;
        var runSteeringKnobImage = runSteeringKnob != null ? runSteeringKnob.GetComponent<Image>() : null;
        var runSteeringLeftRangeEndRectTransform = runSteeringLeftRangeEnd != null ? runSteeringLeftRangeEnd.GetComponent<RectTransform>() : null;
        var runSteeringLeftRangeEndImage = runSteeringLeftRangeEnd != null ? runSteeringLeftRangeEnd.GetComponent<Image>() : null;
        var runSteeringRightRangeEndRectTransform = runSteeringRightRangeEnd != null ? runSteeringRightRangeEnd.GetComponent<RectTransform>() : null;
        var runSteeringRightRangeEndImage = runSteeringRightRangeEnd != null ? runSteeringRightRangeEnd.GetComponent<Image>() : null;
        var runSteeringDeadzoneRectTransform = runSteeringDeadzone != null ? runSteeringDeadzone.GetComponent<RectTransform>() : null;
        var runSteeringDeadzoneImage = runSteeringDeadzone != null ? runSteeringDeadzone.GetComponent<Image>() : null;
        var touchIndicator = FindGameObjectByName(activeScene, objectName: "Touch Indicator");

        var bandShapeProvider = lifetimeScope.Container.Resolve<ISlingshotBandShapeProvider>();
        var preLaunchRigPoseResetter = lifetimeScope.Container.Resolve<IPreLaunchRigPoseResetter>();
        var resolvedRunBodyMovementTarget = lifetimeScope.Container.Resolve<IRunBodyMovementTarget>();
        var resolvedRunSteeringInputSource = lifetimeScope.Container.Resolve<IRunSteeringInputSource>();
        var resolvedRunBodySpeedEvaluator = lifetimeScope.Container.Resolve<IRunBodySpeedEvaluator>();
        var resolvedRunBodySpeedDiagnostics = lifetimeScope.Container.Resolve<IRunBodySpeedDiagnosticsSource>();
        var resolvedRunSteeringEvaluator = lifetimeScope.Container.Resolve<IRunSteeringEvaluator>();
        var resolvedRunLaunchLandingStabilizer = lifetimeScope.Container.Resolve<IRunLaunchLandingStabilizer>();
        var gameplaySlingshotLaunchConfig = lifetimeScope.Container.Resolve<IGameplaySlingshotLaunchConfig>();
        var runBodySpeedConfig = lifetimeScope.Container.Resolve<IRunBodySpeedConfig>();
        var runBodyMovementValidityConfig = lifetimeScope.Container.Resolve<IRunBodyMovementValidityConfig>();
        var runLaunchLandingStabilizationConfig = lifetimeScope.Container.Resolve<IRunLaunchLandingStabilizationConfig>();
        var runSteeringConfig = lifetimeScope.Container.Resolve<IRunSteeringConfig>();
        var runSurfaceStabilityConfig = lifetimeScope.Container.Resolve<RunSurfaceStabilityConfig>();
        var runSupportAttachmentConfig = lifetimeScope.Container.Resolve<RunSupportAttachmentConfig>();
        var runSteeringFrameConfig = lifetimeScope.Container.Resolve<RunSteeringFrameConfig>();
        var resolvedRunCameraSource = lifetimeScope.Container.Resolve<IRunCameraSource>();
        var resolvedRunMotionSource = lifetimeScope.Container.Resolve<IRunMotionSource>();
        var resolvedRunProgressFrameSource = lifetimeScope.Container.Resolve<IRunProgressFrameSource>();
        var resolvedRunSurfaceFrameSource = lifetimeScope.Container.Resolve<IRunSurfaceFrameSource>();
        var resolvedRunSteeringFrameSource = lifetimeScope.Container.Resolve<IRunSteeringFrameSource>();
        var resolvedContactNotifier = lifetimeScope.Container.Resolve<IRigidbodyContactNotifier>();
        var resolvedRunEndConfig = lifetimeScope.Container.Resolve<IRunEndConfig>();
        var resolvedRunProgressService = lifetimeScope.Container.Resolve<IRunProgressService>();
        var resolvedRunContactClassifier = lifetimeScope.Container.Resolve<IRunContactClassifier>();
        var resolvedCharacterPresentationView = lifetimeScope.Container.Resolve<ICharacterPresentationView>();
        var resolvedCharacterPresentationTuning = lifetimeScope.Container.Resolve<ICharacterPresentationTuning>();
        var resolvedCharacterVisualTargetPoseSource = lifetimeScope.Container.Resolve<ICharacterVisualTargetPoseSource>();
        var resolvedCharacterVisualFollowView = lifetimeScope.Container.Resolve<ICharacterVisualFollowView>();
        var resolvedCharacterVisualFollowTuning = lifetimeScope.Container.Resolve<ICharacterVisualFollowTuning>();
        var resolvedFinishPresentationView = lifetimeScope.Container.Resolve<IFinishPresentationView>();
        var resolvedPullHintView = lifetimeScope.Container.Resolve<IPullHintView>();
        var resolvedPullHintTuning = lifetimeScope.Container.Resolve<IPullHintTuning>();
        var resolvedRunSteeringAffordancePresenter = lifetimeScope.Container.Resolve<IRunSteeringAffordancePresenter>();
        var resolvedRunSteeringAffordanceView = lifetimeScope.Container.Resolve<IRunSteeringAffordanceView>();
        var resolvedRunSteeringAffordanceTuning = lifetimeScope.Container.Resolve<IRunSteeringAffordanceTuning>();
        var resolvedRunSteeringPointerPressGuard = lifetimeScope.Container.Resolve<IRunSteeringPointerPressGuard>();
        var resolvedCharacterPresentationModeClassifier = lifetimeScope.Container.Resolve<ICharacterPresentationModeClassifier>();
        var resolvedSlingshotActivePullNotifier = lifetimeScope.Container.Resolve<ISlingshotActivePullNotifier>();
        var resolvedSlingshotCaptureLifecycleNotifier = lifetimeScope.Container.Resolve<ISlingshotCaptureLifecycleNotifier>();
        var resolvedSlingshotPresentationContextSource = lifetimeScope.Container.Resolve<ISlingshotPresentationContextSource>();
        var resolvedSlingshotPullOffsetNormalizer = lifetimeScope.Container.Resolve<ISlingshotPullOffsetNormalizer>();
        var resolvedRunEndCandidateReceiver = lifetimeScope.Container.Resolve<IRunEndCandidateReceiver>();
        var resolvedRunCameraAnchor = lifetimeScope.Container.Resolve<IRunCameraAnchor>();
        var resolvedRunCameraRig = lifetimeScope.Container.Resolve<IRunCameraRig>();
        var resolvedLaunchTargetPreLaunchReset = lifetimeScope.Container.Resolve<ILaunchTargetPreLaunchReset>();
        var runCameraConfig = lifetimeScope.Container.Resolve<IRunCameraConfig>();
        var assignedRunBodyMovementConfigs = GetAssignedRunBodyMovementConfigs(activeScene);
        var assignedRunCameraConfigs = GetAssignedRunCameraConfigs(activeScene);
        var assignedRunEndConfigs = GetAssignedRunEndConfigs(activeScene);
        var assignedRunProgressFrameSources = GetAssignedRunProgressFrameSources(activeScene);
        var cameraTerrainLayerMask = TestAssets.CameraTerrainLayerMask;
        var cameraObstacleLayerMask = TestAssets.CameraObstacleLayerMask;
        var cameraTerrainLayer = GetSingleLayer(cameraTerrainLayerMask, description: "Camera Terrain");
        var cameraObstacleLayer = GetSingleLayer(cameraObstacleLayerMask, description: "Camera Obstacle");
        var bandShapeOutput = new Vector3[bandShapeProvider.BandShapePointCount];

        var bandShapeSolved = bandShapeProvider.TryCreateBandShape(
            new SlingshotBandShapeQuery(
                geometry.LeftAnchorPosition,
                geometry.RightAnchorPosition,
                geometry.RestPoint,
                geometry.RestPoint,
                geometry.LaunchFrameRight,
                geometry.LaunchFrameForward,
                geometry.LaunchFrameUp),
            bandShapeOutput,
            out var bandShapePointCount);

        Assert.That(activeScene.path, Is.EqualTo(TestAssets.GameplaySceneRef.Path));
        Assert.That(lifetimeScope, Is.Not.Null);
        Assert.That(runCameraSource, Is.Not.Null);
        Assert.That(contactNotifier, Is.Not.Null);
        Assert.That(runProgressFrameSource, Is.Not.Null);
        Assert.That(runSurfaceInstaller, Is.Not.Null);
        Assert.That(typeof(Component).IsAssignableFrom(typeof(PhysicsRunSupportProbe)), Is.False);
        Assert.That(typeof(Component).IsAssignableFrom(typeof(RunSurfaceFramePipeline)), Is.False);
        Assert.That(characterPresentationView, Is.Not.Null);
        Assert.That(finishPresentationView, Is.Not.Null);
        Assert.That(runCameraAnchor, Is.Not.Null);
        Assert.That(runCameraRig, Is.Not.Null);
        Assert.That(brain, Is.Not.Null);
        Assert.That(brain.UpdateMethod, Is.EqualTo(CinemachineBrain.UpdateMethods.SmartUpdate));
        Assert.That(runPreparationCamera, Is.Not.Null);
        Assert.That(preLaunchCamera, Is.Not.Null);
        Assert.That(runCamera, Is.Not.Null);
        Assert.That(preLaunchCamera.Priority.Value, Is.GreaterThan(runPreparationCamera.Priority.Value));
        Assert.That(preLaunchCamera.Priority.Value, Is.GreaterThan(runCamera.Priority.Value));
        Assert.That(playerCameraLookTarget.transform.IsChildOf(preLaunchLaunchTargetPose.transform), Is.True);
        Assert.That(playerCameraLookTarget.transform.IsChildOf(characterVisualAnchor.transform), Is.False);
        Assert.That(playerCameraLookTarget.transform.IsChildOf(launchTarget.transform), Is.False);
        Assert.That(playerCameraLookTarget.GetComponent<Renderer>(), Is.Null);
        Assert.That(playerCameraLookTarget.GetComponent<Collider>(), Is.Null);
        Assert.That(runPreparationCamera.Target.TrackingTarget, Is.SameAs(playerCameraLookTarget.transform));
        Assert.That(preLaunchCamera.Target.TrackingTarget, Is.SameAs(playerCameraLookTarget.transform));
        Assert.That(runPreparationCamera.Target.CustomLookAtTarget, Is.False);
        Assert.That(preLaunchCamera.Target.CustomLookAtTarget, Is.False);
        Assert.That(runPreparationHardLookAt, Is.Not.Null);
        Assert.That(preLaunchHardLookAt, Is.Not.Null);
        Assert.That(runPreparationCamera.BlendHint, Is.EqualTo(CinemachineCore.BlendHints.CylindricalPosition));
        Assert.That(preLaunchCamera.BlendHint, Is.EqualTo(CinemachineCore.BlendHints.CylindricalPosition));

        Assert.That(
            Vector3.Dot(runPreparationCamera.transform.position - playerCameraLookTarget.transform.position, Vector3.right),
            Is.GreaterThan(expected: 0.25f));

        Assert.That(
            Mathf.Abs(Vector3.Dot(preLaunchCamera.transform.position - playerCameraLookTarget.transform.position, Vector3.right)),
            Is.LessThan(expected: 0.15f));

        Assert.That(runCamera.Target.TrackingTarget, Is.SameAs(runCameraAnchor.transform));
        Assert.That(thirdPersonFollow, Is.Not.Null);
        Assert.That(decollider, Is.Not.Null);
        Assert.That(deoccluder, Is.Null);
        Assert.That(cameraTerrainLayer, Is.GreaterThanOrEqualTo(expected: 0));
        Assert.That(cameraObstacleLayer, Is.GreaterThanOrEqualTo(expected: 0));
        Assert.That(surface.layer, Is.EqualTo(cameraTerrainLayer));
        Assert.That(runObstacle.layer, Is.EqualTo(cameraObstacleLayer));
        Assert.That(surfaceContact, Is.Not.Null);
        Assert.That(surfaceContact.Category, Is.EqualTo(RunContactCategory.Surface));
        AssertRunSurfacePhysicsMaterial(activeScene, materialName: "LadybugHalfTubeCompletionGlide", expectedFriction: 0.10f);
        AssertRunSurfacePhysicsMaterial(activeScene, materialName: "LadybugHalfTubeEarlyReachPressure", expectedFriction: 0.15f);
        Assert.That(decollider.TerrainResolution.Enabled, Is.True);
        Assert.That(decollider.TerrainResolution.TerrainLayers.value, Is.EqualTo(cameraTerrainLayerMask.value));
        Assert.That(decollider.Decollision.Enabled, Is.True);
        Assert.That(decollider.Decollision.ObstacleLayers.value, Is.EqualTo(cameraTerrainLayerMask.value | cameraObstacleLayerMask.value));
        Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
        Assert.That(bandLineRenderer, Is.Not.Null);
        Assert.That(preLaunchRigPoseRoot, Is.Not.Null);
        Assert.That(preLaunchSlingshotRigPose.transform.IsChildOf(preLaunchRigPoseRoot.transform), Is.True);
        Assert.That(preLaunchLaunchTargetPose.transform.IsChildOf(preLaunchRigPoseRoot.transform), Is.True);
        Assert.That(bandLineRenderer.sharedMaterial, Is.Not.Null);
        Assert.That(bandLineRenderer.positionCount, Is.GreaterThanOrEqualTo(expected: 3));
        Assert.That(geometry.LeftAnchorPosition.x, Is.LessThan(geometry.RightAnchorPosition.x));
        Assert.That(Vector3.Dot(geometry.LaunchFrameForward, Vector3.forward), Is.GreaterThan(expected: 0.99f));
        Assert.That(playerRigidbody, Is.Not.Null);
        Assert.That(runCameraSource.GetComponent<Rigidbody>(), Is.SameAs(playerRigidbody));
        Assert.That(runBodyMovementTarget.GetComponent<Rigidbody>(), Is.SameAs(playerRigidbody));
        Assert.That(runBodyMovementTargets, Has.Length.EqualTo(expected: 1));
        Assert.That(legacyMovementWriters, Is.Empty);
        Assert.That(contactNotifier.GetComponent<Rigidbody>(), Is.SameAs(playerRigidbody));
        Assert.That(preLaunchRigPoseResetter, Is.Not.Null);
        Assert.That(resolvedLaunchTargetPreLaunchReset, Is.SameAs(launchTarget));
        Assert.That(resolvedRunBodyMovementTarget, Is.SameAs(runBodyMovementTarget));
        Assert.That(resolvedRunSteeringInputSource, Is.TypeOf<RunSteeringInputController>());
        Assert.That(resolvedRunBodySpeedEvaluator, Is.TypeOf<DefaultRunBodySpeedEvaluator>());
        Assert.That(resolvedRunBodySpeedDiagnostics, Is.Not.Null);
        Assert.That(resolvedRunSteeringEvaluator, Is.TypeOf<DefaultRunSteeringEvaluator>());
        Assert.That(resolvedRunLaunchLandingStabilizer, Is.TypeOf<RunLaunchLandingStabilizer>());
        Assert.That(resolvedRunCameraSource, Is.SameAs(runCameraSource));
        Assert.That(resolvedRunMotionSource, Is.SameAs(runCameraSource));
        Assert.That(resolvedRunProgressFrameSource, Is.SameAs(runProgressFrameSource));
        Assert.That(resolvedRunSurfaceFrameSource, Is.TypeOf<RunSurfaceFramePipeline>());
        Assert.That(resolvedRunSurfaceFrameSource, Is.Not.SameAs(runSurfaceInstaller));
        Assert.That(resolvedRunSteeringFrameSource, Is.Not.Null);
        Assert.That(resolvedContactNotifier, Is.SameAs(contactNotifier));
        Assert.That(resolvedRunEndConfig, Is.SameAs(assignedRunEndConfigs[0]));
        Assert.That(resolvedRunProgressService, Is.Not.Null);
        Assert.That(resolvedRunContactClassifier, Is.Not.Null);
        Assert.That(resolvedCharacterPresentationView, Is.SameAs(characterPresentationView));
        Assert.That(resolvedCharacterPresentationTuning, Is.SameAs(characterPresentationView));

        Assert.That(
            resolvedCharacterPresentationTuning.FallEnterMinimumUngroundedSeconds,
            Is.EqualTo(expected: 0.3f).Within(amount: 0.0001f));

        Assert.That(resolvedCharacterPresentationTuning.FallEnterMinimumDownwardSpeed, Is.EqualTo(expected: 1.5f).Within(amount: 0.0001f));

        Assert.That(
            resolvedCharacterPresentationTuning.FallEnterMinimumVerticalSeparation,
            Is.EqualTo(expected: 0.18f).Within(amount: 0.0001f));

        Assert.That(resolvedCharacterPresentationTuning.FallEnterHardUngroundedSeconds, Is.EqualTo(expected: 0.65f).Within(amount: 0.0001f));

        Assert.That(
            resolvedCharacterPresentationTuning.MeaningfulGroundedMovementThreshold,
            Is.EqualTo(expected: 0.5f).Within(amount: 0.0001f));

        Assert.That(resolvedCharacterPresentationTuning.MinimumLocomotionModeDuration, Is.EqualTo(expected: 0.35f).Within(amount: 0.0001f));
        Assert.That(resolvedCharacterPresentationTuning.LaunchPushMinimumSeconds, Is.EqualTo(expected: 0.25f).Within(amount: 0.0001f));

        Assert.That(
            resolvedCharacterPresentationTuning.LaunchFlightMaximumGroundedWaitSeconds,
            Is.EqualTo(expected: 0.35f).Within(amount: 0.0001f));

        Assert.That(
            resolvedCharacterPresentationTuning.PresentationSupportMaximumSurfaceLiftSpeed,
            Is.EqualTo(expected: 0.35f).Within(amount: 0.0001f));

        Assert.That(
            resolvedCharacterPresentationTuning.PresentationSupportReacquireSeconds,
            Is.EqualTo(expected: 0.08f).Within(amount: 0.0001f));

        Assert.That(resolvedCharacterPresentationTuning.SlideReferenceSpeed, Is.EqualTo(expected: 8f).Within(amount: 0.0001f));
        Assert.That(resolvedCharacterVisualTargetPoseSource, Is.Not.Null);
        Assert.That(resolvedCharacterVisualTargetPoseSource.CurrentPose.Position, Is.EqualTo(launchTarget.transform.position));

        Assert.That(
            Quaternion.Angle(resolvedCharacterVisualTargetPoseSource.CurrentPose.Rotation, launchTarget.transform.rotation),
            Is.EqualTo(expected: 0f).Within(amount: 0.0001f));

        Assert.That(resolvedCharacterVisualFollowView, Is.SameAs(characterPresentationView));
        Assert.That(resolvedCharacterVisualFollowTuning, Is.SameAs(characterPresentationView));
        Assert.That(resolvedCharacterVisualFollowTuning.VisualPositionResponseRate, Is.EqualTo(expected: 60f).Within(amount: 0.0001f));
        Assert.That(resolvedCharacterVisualFollowTuning.VisualHeadingResponseRate, Is.EqualTo(expected: 45f).Within(amount: 0.0001f));
        Assert.That(resolvedCharacterVisualFollowTuning.VisualUpTiltResponseRate, Is.EqualTo(expected: 18f).Within(amount: 0.0001f));
        Assert.That(resolvedCharacterVisualFollowTuning.VisualMaxPositionLag, Is.EqualTo(expected: 0.06f).Within(amount: 0.0001f));
        Assert.That(resolvedCharacterVisualFollowTuning.VisualSnapDistance, Is.EqualTo(expected: 0.75f).Within(amount: 0.0001f));
        Assert.That(resolvedCharacterVisualFollowTuning.VisualSnapAngleDegrees, Is.EqualTo(expected: 45f).Within(amount: 0.0001f));
        Assert.That(resolvedFinishPresentationView, Is.SameAs(finishPresentationView));
        Assert.That(resolvedPullHintView, Is.SameAs(pullHintView));
        Assert.That(resolvedPullHintTuning, Is.SameAs(pullHintView));
        Assert.That(resolvedRunSteeringAffordancePresenter, Is.TypeOf<RunSteeringAffordancePresenter>());
        Assert.That(resolvedRunSteeringAffordancePresenter, Is.InstanceOf<ITickable>());
        Assert.That(resolvedRunSteeringAffordanceView, Is.SameAs(runSteeringAffordanceView));
        Assert.That(resolvedRunSteeringAffordanceTuning, Is.SameAs(runSteeringAffordanceView));
        Assert.That(resolvedRunSteeringPointerPressGuard, Is.TypeOf<UnityEventSystemRunSteeringPointerPressGuard>());
        Assert.That(resolvedCharacterPresentationModeClassifier, Is.Not.Null);
        Assert.That(resolvedSlingshotActivePullNotifier, Is.Not.Null);
        Assert.That(resolvedSlingshotCaptureLifecycleNotifier, Is.Not.Null);
        Assert.That(resolvedSlingshotPresentationContextSource, Is.Not.Null);
        Assert.That(resolvedSlingshotPullOffsetNormalizer, Is.Not.Null);
        Assert.That(resolvedRunEndCandidateReceiver, Is.Not.Null);
        Assert.That(resolvedRunCameraAnchor, Is.SameAs(runCameraAnchor));
        Assert.That(resolvedRunCameraRig, Is.SameAs(runCameraRig));
        Assert.That(((IRunBodyMovementTarget)runBodyMovementTarget).LinearVelocity, Is.EqualTo(playerRigidbody.linearVelocity));
        Assert.That(runCameraSource.Position, Is.EqualTo(playerRigidbody.transform.position));
        Assert.That(runCameraSource.LinearVelocity, Is.EqualTo(playerRigidbody.linearVelocity));
        Assert.That(targetCollider, Is.Not.Null);
        Assert.That(targetCollider.transform, Is.SameAs(launchTargetColliderRoot.transform));
        Assert.That(runBodyContactCollider, Is.Not.Null);
        Assert.That(runBodyContactCollider.isTrigger, Is.False);
        Assert.That(runBodyContactCollider.transform, Is.SameAs(runBodyContactColliderRoot.transform));
        Assert.That(runSurfaceInstaller.SupportColliderForTests, Is.SameAs(runBodyContactCollider));
        Assert.That(runSurfaceInstaller.SupportProbeDistanceForTests, Is.LessThanOrEqualTo(expected: 0.25f));
        Assert.That(runSurfaceInstaller.SurfaceMaskForTests.value, Is.EqualTo(TestAssets.RunSurfaceLayerMask.value));
        Assert.That(launchTargetColliderRoot.transform.IsChildOf(launchTarget.transform), Is.True);
        Assert.That(runBodyContactColliderRoot.transform.IsChildOf(launchTarget.transform), Is.True);
        Assert.That(launchTargetColliderRoot.GetComponent<MeshRenderer>(), Is.Null);
        Assert.That(launchTargetColliderRoot.GetComponent<MeshFilter>(), Is.Null);
        Assert.That(characterPresentationView.VisualAnchorForTests, Is.SameAs(characterVisualAnchor.transform));
        Assert.That(characterVisualAnchor.transform.IsChildOf(launchTarget.transform), Is.False);
        Assert.That(ladybugCharacter.transform.IsChildOf(characterVisualAnchor.transform), Is.True);
        Assert.That(characterPresentationView.transform, Is.SameAs(ladybugCharacter.transform));
        Assert.That(characterAnimator, Is.Not.Null);
        Assert.That(characterAnimator.runtimeAnimatorController, Is.Not.Null);
        Assert.That(characterAnimator.avatar, Is.Not.Null);
        Assert.That(characterAnimator.applyRootMotion, Is.False);
        AssertAnimatorParameter(characterAnimator, parameterName: "PresentationMode", AnimatorControllerParameterType.Int);

        AssertAnimatorParameter(
            characterAnimator,
            parameterName: "PlaybackSpeedMultiplier",
            AnimatorControllerParameterType.Float);

        AssertAnimatorParameter(characterAnimator, parameterName: "NormalizedPull", AnimatorControllerParameterType.Float);

        AssertAnimatorParameter(
            characterAnimator,
            parameterName: "NormalizedLaunchPower",
            AnimatorControllerParameterType.Float);

        AssertAnimatorParameter(
            characterAnimator,
            parameterName: "NormalizedPullOffset",
            AnimatorControllerParameterType.Float);

        AssertAnimatorParameter(
            characterAnimator,
            parameterName: "NormalizedLaunchOffset",
            AnimatorControllerParameterType.Float);

#if UNITY_EDITOR
        AssertAnyStateTransitionDuration(characterAnimator, CharacterPresentationMode.LaunchFlight, expectedDuration: 0.08f);
        AssertAnyStateTransitionDuration(characterAnimator, CharacterPresentationMode.Airborne, expectedDuration: 1f);
        AssertAnyStateTransitionSupportsInterruption(characterAnimator, CharacterPresentationMode.Airborne);
        AssertAnyStateTransitionDuration(characterAnimator, CharacterPresentationMode.Victory, expectedDuration: 0.24f);
        AssertAnyStateTransitionDuration(characterAnimator, CharacterPresentationMode.Defeat, expectedDuration: 0.2f);
        AssertLaunchPushUsesSlideMotion(characterAnimator);
        AssertLaunchFlightUsesDistinctMotion(characterAnimator);
#endif

        Assert.That(characterAnimationEventReceiver, Is.Not.Null);
        Assert.That(characterAnimationEventReceiver.transform, Is.SameAs(characterAnimator.transform));

        Assert.That(
            characterAnimationEventReceiver.GetType().GetMethod(
                name: "StepOnGround",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                Type.EmptyTypes,
                modifiers: null),
            Is.Not.Null);

        Assert.That(ladybugCharacter.GetComponentsInChildren<Collider>(includeInactive: true), Is.Empty);
        Assert.That(ladybugCharacter.GetComponentsInChildren<Rigidbody>(includeInactive: true), Is.Empty);
        Assert.That(ladybugCharacter.GetComponentsInChildren<Joint>(includeInactive: true), Is.Empty);
        Assert.That(ladybugCharacter.GetComponentsInChildren<CharacterController>(includeInactive: true), Is.Empty);
        Assert.That(bandShapeSolved, Is.True);
        Assert.That(bandShapePointCount, Is.EqualTo(bandShapeProvider.BandShapePointCount));
        Assert.That(bandShapePointCount, Is.GreaterThan(expected: 3));
        Assert.That(bandShapeProvider, Is.TypeOf<SlingshotBandShapeProvider>());
        Assert.That(((SlingshotBandShapeProvider)bandShapeProvider).UsesLaunchTargetBandShapeClearanceSourceForTests, Is.True);
        Assert.That(assignedRunBodyMovementConfigs, Has.Length.EqualTo(expected: 1));
        Assert.That(assignedRunCameraConfigs, Has.Length.EqualTo(expected: 1));
        Assert.That(assignedRunEndConfigs, Has.Length.EqualTo(expected: 1));
        Assert.That(assignedRunProgressFrameSources, Has.Length.EqualTo(expected: 1));
        Assert.That(runBodySpeedConfig, Is.SameAs(assignedRunBodyMovementConfigs[0]));
        Assert.That(runBodyMovementValidityConfig, Is.SameAs(assignedRunBodyMovementConfigs[0]));
        Assert.That(runLaunchLandingStabilizationConfig, Is.SameAs(assignedRunBodyMovementConfigs[0]));
        Assert.That(runSteeringConfig, Is.SameAs(assignedRunBodyMovementConfigs[0]));
        Assert.That(runSurfaceStabilityConfig.SupportLossConfirmationSeconds, Is.EqualTo(expected: 0.12f).Within(amount: 0.0001f));
        Assert.That(runSurfaceStabilityConfig.DiscontinuousNormalThresholdDegrees, Is.EqualTo(expected: 60f).Within(amount: 0.0001f));
        Assert.That(runSurfaceStabilityConfig.DiscontinuousNormalConfirmationSeconds, Is.EqualTo(expected: 0.6f).Within(amount: 0.0001f));
        Assert.That(runSurfaceStabilityConfig.CandidateCoherenceDegrees, Is.EqualTo(expected: 1f).Within(amount: 0.0001f));
        Assert.That(runSupportAttachmentConfig.MaximumAttachedSurfaceNormalLiftSpeed, Is.EqualTo(expected: 0.35f).Within(amount: 0.0001f));
        Assert.That(runSupportAttachmentConfig.SameSurfaceReattachmentSeparationMeters, Is.EqualTo(expected: 0.08f).Within(amount: 0.0001f));
        Assert.That(runSupportAttachmentConfig.MinimumReattachmentNormalChangeDegrees, Is.EqualTo(expected: 30f).Within(amount: 0.0001f));
        Assert.That(runSupportAttachmentConfig.TransitionConfirmationSeconds, Is.EqualTo(expected: 0.04f).Within(amount: 0.0001f));
        Assert.That(runCameraConfig, Is.SameAs(assignedRunCameraConfigs[0]));
        Assert.That(runBodySpeedConfig.DownhillAcceleration, Is.EqualTo(expected: 8f).Within(amount: 0.0001f));
        Assert.That(runBodySpeedConfig.SurfaceSlowdown, Is.EqualTo(expected: 0.5f).Within(amount: 0.0001f));
        Assert.That(runBodySpeedConfig.LowSpeedAssistTargetSpeed, Is.EqualTo(expected: 5f).Within(amount: 0.0001f));
        Assert.That(runBodySpeedConfig.LowSpeedAssistAcceleration, Is.EqualTo(expected: 8f).Within(amount: 0.0001f));
        Assert.That(runBodySpeedConfig.BaseSoftMaximumSpeed, Is.EqualTo(expected: 20f).Within(amount: 0.0001f));
        Assert.That(runBodySpeedConfig.AboveMaximumSpeedResistance, Is.EqualTo(expected: 12f).Within(amount: 0.0001f));
        Assert.That(runBodyMovementValidityConfig.MaximumSupportedSurfaceNormalLiftSpeed, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        Assert.That(runBodyMovementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond, Is.EqualTo(expected: 250f).Within(amount: 0.0001f));

        Assert.That(
            runLaunchLandingStabilizationConfig.LaunchLandingStabilizationSeconds,
            Is.EqualTo(expected: 0.3f).Within(amount: 0.0001f));

        Assert.That(runLaunchLandingStabilizationConfig.LaunchLandingMaximumLiftSpeed, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        Assert.That(runSteeringConfig.MaximumTurnDegreesPerSecond, Is.EqualTo(expected: 30f).Within(amount: 0.0001f));
        Assert.That(runSteeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond, Is.EqualTo(expected: 12f).Within(amount: 0.0001f));

        Assert.That(
            runSteeringConfig.RunAirSteeringMaximumTurnDegreesPerSecond,
            Is.LessThan(runSteeringConfig.MaximumTurnDegreesPerSecond));

        Assert.That(runSteeringFrameConfig.NormalSlewDegreesPerSecond, Is.EqualTo(expected: 120f).Within(amount: 0.0001f));
        Assert.That(runSteeringFrameConfig.AirborneUpRetentionSeconds, Is.EqualTo(expected: 0.12f).Within(amount: 0.0001f));
        Assert.That(runCameraConfig, Is.Not.Null);
        Assert.That(resolvedRunEndConfig, Is.Not.Null);
        Assert.That(gameplaySlingshotLaunchConfig.MinimumForwardImpulse, Is.EqualTo(expected: 10f).Within(amount: 0.0001f));
        Assert.That(gameplaySlingshotLaunchConfig.MaximumForwardImpulse, Is.EqualTo(expected: 25f).Within(amount: 0.0001f));
        Assert.That(gameplaySlingshotLaunchConfig.UpwardImpulse, Is.EqualTo(expected: 3f).Within(amount: 0.0001f));

        Assert.That(
            runBodyMovementValidityConfig.RunBodySpeedSanityGuardMetersPerSecond,
            Is.GreaterThan(gameplaySlingshotLaunchConfig.MaximumForwardImpulse * 4f));

        Assert.That(gameplaySlingshotLaunchConfig.MaximumLateralLaunchAngleDegrees, Is.EqualTo(expected: 35f).Within(amount: 0.0001f));
        Assert.That(gameplaySlingshotLaunchConfig.HasMinimumTotalImpulse, Is.False);
        Assert.That(gameplaySlingshotLaunchConfig.HasMaximumTotalImpulse, Is.False);
        Assert.That(resolvedRunEndConfig.LostMomentumLaunchGraceDuration, Is.EqualTo(expected: 1.25f).Within(amount: 0.0001f));
        Assert.That(resolvedRunEndConfig.LostMomentumDuration, Is.EqualTo(expected: 0.5f).Within(amount: 0.0001f));
        Assert.That(resolvedRunEndConfig.LostMomentumPlanarSpeedThreshold, Is.EqualTo(expected: 1f).Within(amount: 0.0001f));
        Assert.That(resolvedRunEndConfig.LostMomentumProgressThreshold, Is.EqualTo(expected: 0.5f).Within(amount: 0.0001f));

        Assert.That(
            ((IRunProgressFrameSource)runProgressFrameSource).TryCreateSnapshot(
                playerRigidbody.position,
                out var frameSnapshot,
                out var frameError),
            Is.True,
            frameError);

        Assert.That(frameSnapshot.ForwardDirection.sqrMagnitude, Is.EqualTo(expected: 1f).Within(amount: 0.0001f));
        Assert.That(frameSnapshot.RightDirection.sqrMagnitude, Is.EqualTo(expected: 1f).Within(amount: 0.0001f));
        Assert.That(frameSnapshot.UpDirection.sqrMagnitude, Is.EqualTo(expected: 1f).Within(amount: 0.0001f));
        AssertLegacyRunFinishIsNonAuthoritative(runContactsRoot, legacyRunFinish);

        AssertRunContactPlaceholder(
            runContactsRoot,
            runSafetyNet,
            RunContactCategory.SafetyNet,
            expectTrigger: true);

        AssertRunContactPlaceholder(
            runContactsRoot,
            runObstacle,
            RunContactCategory.Obstacle,
            expectTrigger: false);

        AssertFinishPresentationContracts(
            finishPresentationRoot,
            finishThresholdVisual,
            authoritativeRunFinish,
            finishPresentationView);

        Assert.That(Enum.GetNames(typeof(RunContactCategory)), Does.Not.Contain(expected: "Boundary"));
        Assert.That(Enum.GetNames(typeof(RunContactCategory)), Does.Not.Contain(expected: "Ramp"));
        Assert.That(playerRigidbody.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode.ContinuousDynamic));
        Assert.That(playerRigidbody.isKinematic, Is.True);
        Assert.That(playerRigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.None));
        Assert.That(launchTarget.HasPreviousStateForTests, Is.True);
        Assert.That(launchTarget.PreviousInterpolationForTests, Is.EqualTo(RigidbodyInterpolation.Interpolate));
        Assert.That(bandCenter.transform.IsChildOf(launchTarget.transform), Is.True);
        Assert.That(bandCenter.transform.position.x, Is.EqualTo(geometry.RestPoint.x).Within(amount: 0.01f));
        Assert.That(bandCenter.transform.position.y, Is.EqualTo(geometry.RestPoint.y).Within(amount: 0.01f));
        Assert.That(bandCenter.transform.position.z, Is.EqualTo(geometry.RestPoint.z).Within(amount: 0.01f));

        Assert.That(
            Quaternion.Angle(preLaunchLaunchTargetPose.transform.rotation, playerRigidbody.transform.rotation),
            Is.EqualTo(expected: 0f).Within(amount: 0.01f));

        Assert.That(pullHint.transform.IsChildOf(canvas.transform), Is.True);
        Assert.That(pullHintView, Is.Not.Null);
        Assert.That(pullHintRectTransform, Is.Not.Null);
        Assert.That(pullHintRectTransform.sizeDelta.x, Is.EqualTo(expected: 192f).Within(amount: 0.001f));
        Assert.That(pullHintRectTransform.sizeDelta.y, Is.EqualTo(expected: 320f).Within(amount: 0.001f));
        Assert.That(pullHintCanvasGroup, Is.Not.Null);
        Assert.That(pullHintCanvasGroup.alpha, Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(pullHintCanvasGroup.interactable, Is.False);
        Assert.That(pullHintCanvasGroup.blocksRaycasts, Is.False);
        Assert.That(pullHintAnimator, Is.Not.Null);
        Assert.That(pullHintAnimator.runtimeAnimatorController, Is.Not.Null);
        AssertAnimatorParameter(pullHintAnimator, parameterName: "PlayPullHint", AnimatorControllerParameterType.Trigger);
        Assert.That(pullHintFinger, Is.Not.Null);
        Assert.That(pullHintFingerRectTransform, Is.Not.Null);
        Assert.That(pullHintFingerRectTransform.sizeDelta.x, Is.EqualTo(expected: 160f).Within(amount: 0.001f));
        Assert.That(pullHintFingerRectTransform.sizeDelta.y, Is.EqualTo(expected: 160f).Within(amount: 0.001f));
        Assert.That(pullHintFingerImage, Is.Not.Null);
        Assert.That(pullHintFingerImage.sprite, Is.Not.Null);
        Assert.That(pullHintFingerImage.raycastTarget, Is.False);
        Assert.That(pullHint.activeSelf, Is.False);
        resolvedPullHintView.ShowAt(Vector2.zero);
        Assert.That(pullHint.activeSelf, Is.True);
        Assert.That(pullHintCanvasGroup.alpha, Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(pullHintFingerRectTransform.anchoredPosition.x, Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(pullHintFingerRectTransform.anchoredPosition.y, Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(pullHintFingerImage.sprite, Is.Not.Null);
        resolvedPullHintView.Hide();
        Assert.That(pullHint.activeSelf, Is.False);
        Assert.That(pullHintCanvasGroup.alpha, Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(runSteeringAffordance.transform.IsChildOf(canvas.transform), Is.True);
        Assert.That(runSteeringAffordanceView, Is.Not.Null);
        Assert.That(runSteeringAffordanceRectTransform, Is.Not.Null);
        Assert.That(runSteeringAffordanceRectTransform.anchorMin, Is.EqualTo(Vector2.zero));
        Assert.That(runSteeringAffordanceRectTransform.anchorMax, Is.EqualTo(Vector2.one));
        Assert.That(runSteeringAffordanceCanvasGroup, Is.Not.Null);
        Assert.That(runSteeringAffordanceCanvasGroup.alpha, Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(runSteeringAffordanceCanvasGroup.interactable, Is.False);
        Assert.That(runSteeringAffordanceCanvasGroup.blocksRaycasts, Is.False);
        Assert.That(runSteeringKnobRectTransform, Is.Not.Null);
        Assert.That(runSteeringLeftRangeEndRectTransform, Is.Not.Null);
        Assert.That(runSteeringRightRangeEndRectTransform, Is.Not.Null);
        Assert.That(runSteeringDeadzoneRectTransform, Is.Not.Null);
        AssertRunSteeringAffordanceImage(runSteeringKnobImage, label: "Run Steering Knob");
        AssertRunSteeringAffordanceImage(runSteeringLeftRangeEndImage, label: "Run Steering Left Range End");
        AssertRunSteeringAffordanceImage(runSteeringRightRangeEndImage, label: "Run Steering Right Range End");
        AssertRunSteeringAffordanceImage(runSteeringDeadzoneImage, label: "Run Steering Deadzone");
        Assert.That(TryFindGameObjectByName(activeScene, objectName: "Run Steering Track", out _), Is.False);
        Assert.That(runSteeringAffordance.activeSelf, Is.False);

        var affordanceLayout = new RunSteeringAffordanceLayout();

        var affordanceStartState = affordanceLayout.Create(
            new RunSteeringAffordanceSnapshot(
                isActive: true,
                pointerId: 1,
                new Vector2(x: 500f, y: 700f),
                new Vector2(x: 560f, y: 1200f),
                capturedRangePixels: 100f,
                capturedDeadzoneFraction: 0.25f));

        resolvedRunSteeringAffordancePresenter.Show(affordanceStartState);
        Assert.That(runSteeringAffordance.activeSelf, Is.True);
        Assert.That(canvas.scaleFactor, Is.GreaterThan(expected: 0f));
        Assert.That(runSteeringKnobRectTransform.anchoredPosition.y, Is.EqualTo(700f / canvas.scaleFactor).Within(amount: 0.001f));

        var affordanceMovedState = affordanceLayout.Create(
            new RunSteeringAffordanceSnapshot(
                isActive: true,
                pointerId: 1,
                new Vector2(x: 500f, y: 700f),
                new Vector2(x: 650f, y: 1200f),
                capturedRangePixels: 100f,
                capturedDeadzoneFraction: 0.25f));

        resolvedRunSteeringAffordancePresenter.Update(affordanceMovedState);
        Assert.That(runSteeringKnobRectTransform.anchoredPosition.x, Is.EqualTo(600f / canvas.scaleFactor).Within(amount: 0.001f));
        Assert.That(runSteeringKnobRectTransform.anchoredPosition.y, Is.EqualTo(700f / canvas.scaleFactor).Within(amount: 0.001f));
        resolvedRunSteeringAffordancePresenter.Reset();
        Assert.That(runSteeringAffordance.activeSelf, Is.False);
        Assert.That(runSteeringAffordanceCanvasGroup.alpha, Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(touchIndicator.transform.IsChildOf(canvas.transform), Is.True);
        Assert.That(touchIndicator.activeSelf, Is.False);
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_ReturnsToPrelaunchInSameSession_then_PreLaunchRigPoseIsRestored()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return ContinueToPreLaunch(activeScene);
        yield return WaitUntilPlayerIsHeld(activeScene);

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, objectDescription: "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, objectDescription: "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, objectDescription: "RigidbodyLaunchTarget");
        var bandCenter = FindGameObjectByName(activeScene, objectName: "Band Center");
        var preLaunchSlingshotRigPose = FindGameObjectByName(activeScene, objectName: "Pre-Launch Slingshot Rig Pose");
        var preLaunchLaunchTargetPose = FindGameObjectByName(activeScene, objectName: "Pre-Launch Launch Target Pose");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();

        Assert.That(stateService.TryTransitionTo(lifetimeScope.RunningStateIdForTests), Is.True);
        slingshotView.transform.SetPositionAndRotation(new Vector3(x: 1f, y: 0.25f, z: 0.5f), Quaternion.Euler(x: 0f, y: 15f, z: 0f));
        playerRigidbody.isKinematic = false;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        playerRigidbody.linearVelocity = new Vector3(x: 3f, y: 0f, z: 2f);
        playerRigidbody.angularVelocity = new Vector3(x: 1f, y: 2f, z: 3f);
        playerRigidbody.transform.SetPositionAndRotation(new Vector3(x: 4f, y: 2f, z: -3f), Quaternion.Euler(x: 10f, y: 80f, z: 25f));
        playerRigidbody.position = playerRigidbody.transform.position;
        playerRigidbody.rotation = playerRigidbody.transform.rotation;

        Assert.That(stateService.TryTransitionTo(lifetimeScope.RunEndedStateIdForTests), Is.True);
        Assert.That(stateService.TryTransitionTo(lifetimeScope.RunPreparationStateIdForTests), Is.True);
        yield return ContinueToPreLaunch(activeScene);

        var geometry = slingshotView.CreateGeometrySnapshot();

        Assert.That(slingshotView.transform.position, Is.EqualTo(preLaunchSlingshotRigPose.transform.position));

        Assert.That(
            Quaternion.Angle(preLaunchSlingshotRigPose.transform.rotation, slingshotView.transform.rotation),
            Is.EqualTo(expected: 0f).Within(amount: 0.0001f));

        Assert.That(
            Quaternion.Angle(preLaunchLaunchTargetPose.transform.rotation, playerRigidbody.rotation),
            Is.EqualTo(expected: 0f).Within(amount: 0.0001f));

        AssertPlayerIsHeld(playerRigidbody);
        Assert.That(playerRigidbody.constraints, Is.EqualTo(RigidbodyConstraints.None));
        Assert.That(Vector3.Distance(bandCenter.transform.position, geometry.RestPoint), Is.LessThan(expected: 0.01f));
        AssertBandLineCenterEqualsRest(bandLineRenderer, geometry);
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_RepeatedLaunchRestartsInSameSession_then_PreLaunchRigPoseRemainsCentered()
    {
        var mouse = InputSystem.AddDevice<Mouse>(name: "Gameplay Scene Restart Alignment Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return ContinueToPreLaunch(activeScene);
            yield return WaitUntilPlayerIsHeld(activeScene);

            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, objectDescription: "GameplayLifetimeScope");
            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, objectDescription: "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, objectDescription: "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, objectDescription: "Input Camera");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var bandCenter = FindGameObjectByName(activeScene, objectName: "Band Center");
            var preLaunchLaunchTargetPose = FindGameObjectByName(activeScene, objectName: "Pre-Launch Launch Target Pose");

            var launchScenarios = new[]
            {
                new Vector2(x: 0f, y: 0.45f),
                new Vector2(x: 0.75f, y: 1.25f),
                new Vector2(x: -0.75f, y: 1.25f),
                new Vector2(x: 0.25f, y: 1.75f)
            };

            for (var scenarioIndex = 0; scenarioIndex < launchScenarios.Length; scenarioIndex += 1)
            {
                var geometry = slingshotView.CreateGeometrySnapshot();
                var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

                var pullWorldPosition = geometry.RestPoint
                                        + geometry.LaunchFrameRight * launchScenarios[scenarioIndex].x
                                        - geometry.LaunchFrameForward * launchScenarios[scenarioIndex].y;

                var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

                yield return SendMouse(mouse, pressScreenPosition, isPressed: true);
                yield return SendMouse(mouse, releaseScreenPosition, isPressed: true);
                yield return SendMouse(mouse, releaseScreenPosition, isPressed: false);
                yield return WaitUntilPlayerLaunches(playerRigidbody);

                DisturbLaunchedPlayerPose(playerRigidbody, scenarioIndex);

                Assert.That(stateService.TryTransitionTo(lifetimeScope.RunEndedStateIdForTests), Is.True);
                Assert.That(stateService.TryTransitionTo(lifetimeScope.RunPreparationStateIdForTests), Is.True);
                yield return ContinueToPreLaunch(activeScene);

                AssertPreLaunchRigPoseRestored(
                    slingshotView,
                    bandLineRenderer,
                    playerRigidbody,
                    bandCenter.transform,
                    preLaunchLaunchTargetPose.transform,
                    $"restart scenario {scenarioIndex}");
            }
        }
        finally
        {
            InputSystem.RemoveDevice(mouse);
        }
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_SlingshotPolesFrameBandAnchors()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, objectDescription: "SlingshotView");
        var geometry = slingshotView.CreateGeometrySnapshot();
        var leftAnchor = FindGameObjectByName(activeScene, objectName: "Left Anchor");
        var rightAnchor = FindGameObjectByName(activeScene, objectName: "Right Anchor");
        var leftPole = FindGameObjectByName(activeScene, objectName: "Left Slingshot Pole");
        var rightPole = FindGameObjectByName(activeScene, objectName: "Right Slingshot Pole");

        AssertPoleFramesAnchor(
            leftPole,
            leftAnchor.transform,
            geometry.LeftAnchorPosition,
            poleName: "Left Slingshot Pole");

        AssertPoleFramesAnchor(
            rightPole,
            rightAnchor.transform,
            geometry.RightAnchorPosition,
            poleName: "Right Slingshot Pole");
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

        if (!TryFindGameObjectByName(scene, objectName: "Band Center", out var bandCenter))
            return false;

        if (!TryFindGameObjectByName(scene, objectName: "Pull Hint", out var pullHint) || pullHint.activeSelf)
            return false;

        if (!TryFindGameObjectByName(scene, objectName: "Run Steering Affordance", out var runSteeringAffordance) ||
            runSteeringAffordance.activeSelf)
            return false;

        if (!TryFindGameObjectByName(scene, objectName: "Touch Indicator", out var touchIndicator) || touchIndicator.activeSelf)
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

        Assert.Fail(message: "Expected Player to be held by the Slingshot.");
    }

    private IEnumerator ContinueToPreLaunch(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, objectDescription: "GameplayLifetimeScope");
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
        continueCommand.TryContinue();
        yield return null;
    }

    private RunBodyMovementConfig[] GetAssignedRunBodyMovementConfigs(Scene scene)
    {
        return FindComponentsInScene<GameplayLifetimeScope>(scene)
            .Select(lifetimeScope => lifetimeScope.RunBodyMovementConfigForTests)
            .Where(config => config != null)
            .Distinct()
            .ToArray();
    }

    private RunCameraConfig[] GetAssignedRunCameraConfigs(Scene scene)
    {
        return FindComponentsInScene<GameplayLifetimeScope>(scene)
            .Select(lifetimeScope => lifetimeScope.RunCameraConfigForTests)
            .Where(config => config != null)
            .Distinct()
            .ToArray();
    }

    private RunEndConfig[] GetAssignedRunEndConfigs(Scene scene)
    {
        return FindComponentsInScene<GameplayLifetimeScope>(scene)
            .Select(lifetimeScope => lifetimeScope.RunEndConfigForTests)
            .Where(config => config != null)
            .Distinct()
            .ToArray();
    }

    private RunProgressFrameSource[] GetAssignedRunProgressFrameSources(Scene scene)
    {
        return FindComponentsInScene<GameplayLifetimeScope>(scene)
            .Select(lifetimeScope => lifetimeScope.RunProgressFrameSourceForTests)
            .Where(source => source != null)
            .Distinct()
            .ToArray();
    }

    private Collider GetSingleTargetCollider(RigidbodyLaunchTarget launchTarget)
    {
        var collider = launchTarget.BandContactColliderForTests;

        Assert.That(collider, Is.Not.Null);
        return collider;
    }

    private void AssertRunContactPlaceholder(
        GameObject runContactsRoot,
        GameObject placeholder,
        RunContactCategory expectedCategory,
        bool expectTrigger)
    {
        Assert.That(placeholder.transform.IsChildOf(runContactsRoot.transform), Is.True, placeholder.name);

        var collider = placeholder.GetComponent<Collider>();
        var contact = placeholder.GetComponent<RunContact>();

        Assert.That(collider, Is.Not.Null, placeholder.name);
        Assert.That(collider.isTrigger, Is.EqualTo(expectTrigger), placeholder.name);
        Assert.That(contact, Is.Not.Null, placeholder.name);
        Assert.That(contact.Category, Is.EqualTo(expectedCategory), placeholder.name);
        Assert.That(contact.gameObject, Is.SameAs(collider.gameObject), placeholder.name);
    }

    private void AssertLegacyRunFinishIsNonAuthoritative(GameObject runContactsRoot, GameObject legacyRunFinish)
    {
        if (legacyRunFinish == null)
            return;

        Assert.That(legacyRunFinish.transform.IsChildOf(runContactsRoot.transform), Is.True, legacyRunFinish.name);
        Assert.That(legacyRunFinish.activeInHierarchy, Is.False, message: "Legacy Run Contacts/Run Finish must stay disabled.");

        var collider = legacyRunFinish.GetComponent<Collider>();
        var contact = legacyRunFinish.GetComponent<RunContact>();

        if (collider != null)
            Assert.That(collider.enabled && legacyRunFinish.activeInHierarchy, Is.False, legacyRunFinish.name);

        if (contact != null)
            Assert.That(contact.enabled && legacyRunFinish.activeInHierarchy, Is.False, legacyRunFinish.name);
    }

    private void AssertFinishPresentationContracts(
        GameObject finishPresentationRoot,
        GameObject finishThresholdVisual,
        GameObject authoritativeRunFinish,
        FinishPresentationView finishPresentationView)
    {
        Assert.That(finishPresentationView.transform.IsChildOf(finishPresentationRoot.transform), Is.True);
        Assert.That(finishThresholdVisual.transform.IsChildOf(finishPresentationRoot.transform), Is.True);
        Assert.That(finishThresholdVisual.GetComponent<Collider>(), Is.Null);
        Assert.That(finishThresholdVisual.GetComponent<RunContact>(), Is.Null);

        var thresholdRenderer = finishThresholdVisual.GetComponent<Renderer>();
        Assert.That(thresholdRenderer, Is.Not.Null);
        Assert.That(thresholdRenderer.enabled, Is.True);
        Assert.That(thresholdRenderer.sharedMaterial, Is.Not.Null);
        AssertFinishThresholdMaterial(thresholdRenderer.sharedMaterial);
        Assert.That(finishPresentationView.ThresholdRendererForTests, Is.SameAs(thresholdRenderer));
        Assert.That(finishPresentationView.SuccessParticlesForTests, Has.Length.GreaterThanOrEqualTo(expected: 1));
        Assert.That(finishPresentationView.SuccessParticlesForTests.All(particle => particle != null), Is.True);
        AssertLegacyFinishMarkerInactive(finishPresentationRoot, markerName: "Finish Marker Top Bar");
        AssertLegacyFinishMarkerInactive(finishPresentationRoot, markerName: "Finish Marker Left Post");
        AssertLegacyFinishMarkerInactive(finishPresentationRoot, markerName: "Finish Marker Right Post");

        Assert.That(
            finishPresentationView.SuccessParticlesForTests.Any(particle => particle.name.Contains(value: "Confetti_Hearts")),
            Is.True,
            message: "Finish Celebration should use Ladybug Confetti_Hearts as the first-pass VFX source.");

        Assert.That(
            Mathf.Abs(finishThresholdVisual.transform.position.z - authoritativeRunFinish.transform.position.z),
            Is.LessThanOrEqualTo(expected: 0.5f));
    }

    private void AssertFinishThresholdMaterial(Material material)
    {
        Assert.That(material.name, Is.EqualTo(FinishThresholdMaterialName));
        Assert.That(material.shader, Is.Not.Null);
        Assert.That(material.shader.name, Is.EqualTo(FinishThresholdShaderName));
        Assert.That(material.HasProperty(FinishThresholdBaseMapPropertyName), Is.True);
        Assert.That(material.HasProperty(FinishThresholdBottomAlphaPropertyName), Is.True);
        Assert.That(material.HasProperty(FinishThresholdTopAlphaPropertyName), Is.True);
        Assert.That(material.HasProperty(FinishThresholdFadeExponentPropertyName), Is.True);

        var baseMap = material.GetTexture(FinishThresholdBaseMapPropertyName);
        var baseMapScale = material.GetTextureScale(FinishThresholdBaseMapPropertyName);

        Assert.That(baseMap, Is.Not.Null);
        Assert.That(baseMap.width, Is.EqualTo(expected: 128));
        Assert.That(baseMap.height, Is.EqualTo(expected: 128));
        Assert.That(baseMap.mipmapCount, Is.GreaterThan(expected: 1));
        Assert.That(baseMap.wrapModeU, Is.EqualTo(TextureWrapMode.Repeat));
        Assert.That(baseMap.wrapModeV, Is.EqualTo(TextureWrapMode.Repeat));
        Assert.That(baseMapScale.x, Is.GreaterThan(expected: 1f));
        Assert.That(baseMapScale.x, Is.EqualTo(expected: 20f).Within(amount: 0.001f));
        Assert.That(baseMapScale.y, Is.EqualTo(expected: 2f).Within(amount: 0.001f));
        Assert.That(material.GetFloat(FinishThresholdBottomAlphaPropertyName), Is.EqualTo(expected: 0.5f).Within(amount: 0.001f));
        Assert.That(material.GetFloat(FinishThresholdTopAlphaPropertyName), Is.EqualTo(expected: 0f).Within(amount: 0.001f));
        Assert.That(material.GetFloat(FinishThresholdFadeExponentPropertyName), Is.EqualTo(expected: 3f).Within(amount: 0.001f));

#if UNITY_EDITOR
        AssertFinishThresholdTextureImporter(baseMap);
#endif
    }

    private void AssertRunSteeringAffordanceImage(Image image, string label)
    {
        Assert.That(image, Is.Not.Null, label);
        Assert.That(image.sprite, Is.Not.Null, label);
        Assert.That(image.raycastTarget, Is.False, label);

#if UNITY_EDITOR
        AssertRunSteeringAffordanceSpriteImporter(image.sprite, label);
#endif
    }

    private void AssertLegacyFinishMarkerInactive(GameObject finishPresentationRoot, string markerName)
    {
        var marker = finishPresentationRoot.transform.Find(markerName);

        if (marker == null)
            return;

        Assert.That(
            marker.gameObject.activeInHierarchy,
            Is.False,
            $"{markerName} must not render over the checkered finish threshold.");
    }

    private void AssertRunSurfacePhysicsMaterial(Scene scene, string materialName, float expectedFriction)
    {
        var materials = FindComponentsInScene<Collider>(scene)
            .Where(collider => collider.sharedMaterial != null && collider.sharedMaterial.name == materialName)
            .Select(collider => collider.sharedMaterial)
            .Distinct()
            .ToArray();

        Assert.That(materials, Has.Length.EqualTo(expected: 1), materialName);

        var material = materials[0];
        Assert.That(material.dynamicFriction, Is.EqualTo(expectedFriction).Within(amount: 0.0001f), materialName);
        Assert.That(material.staticFriction, Is.EqualTo(expectedFriction).Within(amount: 0.0001f), materialName);
        Assert.That(material.bounciness, Is.EqualTo(expected: 0f).Within(amount: 0.0001f), materialName);
        Assert.That(material.frictionCombine, Is.EqualTo(PhysicsMaterialCombine.Minimum), materialName);
        Assert.That(material.bounceCombine, Is.EqualTo(PhysicsMaterialCombine.Minimum), materialName);
    }

    private void AssertAnimatorParameter(Animator animator, string parameterName, AnimatorControllerParameterType parameterType)
    {
        Assert.That(
            animator.parameters.Any(parameter => parameter.name == parameterName && parameter.type == parameterType),
            Is.True,
            $"{animator.name} Animator should contain {parameterType} parameter '{parameterName}'.");
    }

    private void AssertPoleFramesAnchor(GameObject pole, Transform anchor, Vector3 anchorPosition, string poleName)
    {
        var meshRenderer = pole.GetComponent<MeshRenderer>();
        var meshFilter = pole.GetComponent<MeshFilter>();
        var colliders = pole.GetComponentsInChildren<Collider>(includeInactive: true);

        Assert.That(pole.transform.IsChildOf(anchor), Is.True, poleName);
        Assert.That(meshRenderer, Is.Not.Null, poleName);
        Assert.That(meshRenderer.enabled, Is.True, poleName);
        Assert.That(meshRenderer.sharedMaterial, Is.Not.Null, poleName);
        Assert.That(meshFilter, Is.Not.Null, poleName);
        Assert.That(meshFilter.sharedMesh, Is.Not.Null, poleName);
        Assert.That(colliders, Is.Empty, poleName);
        Assert.That(pole.transform.position.x, Is.EqualTo(anchorPosition.x).Within(amount: 0.01f), poleName);
        Assert.That(pole.transform.position.z, Is.EqualTo(anchorPosition.z).Within(amount: 0.01f), poleName);
        Assert.That(pole.transform.position.y, Is.LessThan(anchorPosition.y), poleName);
    }

    private Vector2 GetScreenPosition(Camera camera, Vector3 worldPosition)
    {
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        Assert.That(screenPosition.z, Is.GreaterThan(expected: 0f));
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

    private int GetSingleLayer(LayerMask layerMask, string description)
    {
        Assert.That(layerMask.value, Is.Not.EqualTo(expected: 0), description);
        Assert.That(layerMask.value & (layerMask.value - 1), Is.Zero, description);
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, p: 2f));
    }

    private IEnumerator WaitUntilPlayerLaunches(Rigidbody playerRigidbody)
    {
        for (var frameIndex = 0; frameIndex < 60; frameIndex += 1)
        {
            if (!playerRigidbody.isKinematic && playerRigidbody.linearVelocity.sqrMagnitude > 0.0001f)
                yield break;

            yield return null;
        }

        Assert.Fail(message: "Expected Slingshot pull release to launch the Player.");
    }

    private void AssertPlayerIsHeld(Rigidbody playerRigidbody)
    {
        Assert.That(playerRigidbody.isKinematic, Is.True);
        Assert.That(playerRigidbody.linearVelocity.sqrMagnitude, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
        Assert.That(playerRigidbody.angularVelocity.sqrMagnitude, Is.EqualTo(expected: 0f).Within(amount: 0.0001f));
    }

    private void DisturbLaunchedPlayerPose(Rigidbody playerRigidbody, int scenarioIndex)
    {
        var position = new Vector3(2f + scenarioIndex, 0.75f + scenarioIndex * 0.25f, -1f - scenarioIndex);
        var rotation = Quaternion.Euler(25f + scenarioIndex * 7f, 45f + scenarioIndex * 13f, 15f + scenarioIndex * 5f);

        playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        playerRigidbody.linearVelocity = new Vector3(3f + scenarioIndex, y: 0.5f, z: 2f);
        playerRigidbody.angularVelocity = new Vector3(x: 1f, 2f + scenarioIndex, z: 3f);
        playerRigidbody.transform.SetPositionAndRotation(position, rotation);
        playerRigidbody.position = position;
        playerRigidbody.rotation = rotation;
    }

    private void AssertPreLaunchRigPoseRestored(
        SlingshotView slingshotView,
        LineRenderer bandLineRenderer,
        Rigidbody playerRigidbody,
        Transform bandCenter,
        Transform preLaunchLaunchTargetPose,
        string assertionScope)
    {
        var geometry = slingshotView.CreateGeometrySnapshot();

        Assert.That(
            Quaternion.Angle(preLaunchLaunchTargetPose.rotation, playerRigidbody.rotation),
            Is.EqualTo(expected: 0f).Within(amount: 0.0001f),
            assertionScope);

        AssertPlayerIsHeld(playerRigidbody);
        Assert.That(playerRigidbody.constraints, Is.EqualTo(RigidbodyConstraints.None), assertionScope);
        Assert.That(Vector3.Distance(bandCenter.position, geometry.RestPoint), Is.LessThan(expected: 0.01f), assertionScope);
        AssertBandLineCenterEqualsRest(bandLineRenderer, geometry);
    }

    private void AssertBandLineCenterEqualsRest(LineRenderer bandLineRenderer, SlingshotGeometrySnapshot geometry)
    {
        Assert.That(bandLineRenderer.positionCount, Is.GreaterThanOrEqualTo(expected: 3));

        var middleIndex = (bandLineRenderer.positionCount - 1) / 2;

        Assert.That(Vector3.Distance(bandLineRenderer.GetPosition(middleIndex), geometry.RestPoint), Is.LessThan(expected: 0.01f));
        Assert.That(Vector3.Distance(bandLineRenderer.GetPosition(index: 0), geometry.LeftAnchorPosition), Is.LessThan(expected: 0.01f));

        Assert.That(
            Vector3.Distance(bandLineRenderer.GetPosition(bandLineRenderer.positionCount - 1), geometry.RightAnchorPosition),
            Is.LessThan(expected: 0.01f));
    }

#if UNITY_EDITOR
    private void AssertRunSteeringAffordanceSpriteImporter(Sprite sprite, string label)
    {
        var texturePath = AssetDatabase.GetAssetPath(sprite);
        Assert.That(texturePath, Is.Not.Empty, label);

        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        Assert.That(importer, Is.Not.Null, label);
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), label);
        Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), label);
        Assert.That(importer.mipmapEnabled, Is.False, label);
        Assert.That(importer.alphaIsTransparency, Is.True, label);
        Assert.That(importer.maxTextureSize, Is.EqualTo(expected: 512), label);
    }

    private void AssertFinishThresholdTextureImporter(Texture texture)
    {
        var texturePath = AssetDatabase.GetAssetPath(texture);
        Assert.That(texturePath, Is.Not.Empty);

        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        Assert.That(importer, Is.Not.Null);
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Default));
        Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.None));
        Assert.That(importer.mipmapEnabled, Is.True);
        Assert.That(importer.wrapModeU, Is.EqualTo(TextureWrapMode.Repeat));
        Assert.That(importer.wrapModeV, Is.EqualTo(TextureWrapMode.Repeat));
    }
#endif

#if UNITY_EDITOR
    private void AssertAnyStateTransitionDuration(Animator animator, CharacterPresentationMode mode, float expectedDuration)
    {
        var transition = FindPresentationModeAnyStateTransition(animator, mode);

        Assert.That(transition.hasFixedDuration, Is.True, $"{mode} transition should use fixed-duration seconds.");
        Assert.That(transition.duration, Is.EqualTo(expectedDuration).Within(amount: 0.0001f), mode.ToString());
    }

    private void AssertAnyStateTransitionSupportsInterruption(Animator animator, CharacterPresentationMode mode)
    {
        var transition = FindPresentationModeAnyStateTransition(animator, mode);

        Assert.That(
            transition.interruptionSource,
            Is.Not.EqualTo(TransitionInterruptionSource.None),
            $"{mode} transition should support interruption.");
    }

    private AnimatorStateTransition FindPresentationModeAnyStateTransition(Animator animator, CharacterPresentationMode mode)
    {
        var controller = animator.runtimeAnimatorController as AnimatorController;

        Assert.That(controller, Is.Not.Null, $"{animator.name} should use an AnimatorController asset.");

        var transition = controller.layers
            .SelectMany(layer => layer.stateMachine.anyStateTransitions)
            .SingleOrDefault(candidate => IsPresentationModeAnyStateTransition(candidate, mode));

        Assert.That(transition, Is.Not.Null, $"{controller.name} should contain an AnyState transition to {mode}.");

        return transition;
    }

    private bool IsPresentationModeAnyStateTransition(AnimatorStateTransition transition, CharacterPresentationMode mode)
    {
        return transition.destinationState != null
               && transition.destinationState.name == mode.ToString()
               && transition.conditions.Any(condition =>
                   condition is { parameter: "PresentationMode", mode: AnimatorConditionMode.Equals }
                   && Mathf.Approximately(condition.threshold, (int)mode));
    }

    private void AssertLaunchPushUsesSlideMotion(Animator animator)
    {
        var launchPushMotion = GetPresentationModeMotion(animator, CharacterPresentationMode.LaunchPush);
        var slideMotion = GetPresentationModeMotion(animator, CharacterPresentationMode.Slide);
        var airborneMotion = GetPresentationModeMotion(animator, CharacterPresentationMode.Airborne);

        Assert.That(
            launchPushMotion,
            Is.SameAs(slideMotion),
            message: "LaunchPush should visually continue Slide instead of replaying the fall motion.");

        Assert.That(
            launchPushMotion,
            Is.Not.SameAs(airborneMotion),
            message: "LaunchPush and Airborne should not share a motion because Airborne is reserved for real falls.");
    }

    private void AssertLaunchFlightUsesDistinctMotion(Animator animator)
    {
        var launchFlightMotion = GetPresentationModeMotion(animator, CharacterPresentationMode.LaunchFlight);
        var slideMotion = GetPresentationModeMotion(animator, CharacterPresentationMode.Slide);
        var airborneMotion = GetPresentationModeMotion(animator, CharacterPresentationMode.Airborne);

        Assert.That(
            launchFlightMotion,
            Is.Not.SameAs(slideMotion),
            message: "LaunchFlight should use a distinct fired-through-air motion instead of grounded Slide.");

        Assert.That(
            launchFlightMotion,
            Is.Not.SameAs(airborneMotion),
            message: "LaunchFlight and Airborne should not share a motion because Airborne is reserved for real falls.");
    }

    private Motion GetPresentationModeMotion(Animator animator, CharacterPresentationMode mode)
    {
        var controller = animator.runtimeAnimatorController as AnimatorController;

        Assert.That(controller, Is.Not.Null, $"{animator.name} should use an AnimatorController asset.");

        var state = controller.layers
            .SelectMany(layer => layer.stateMachine.states)
            .Select(childState => childState.state)
            .SingleOrDefault(candidate => candidate.name == mode.ToString());

        Assert.That(state, Is.Not.Null, $"{controller.name} should contain a {mode} state.");
        Assert.That(state.motion, Is.Not.Null, $"{mode} should have a motion assigned.");

        return state.motion;
    }
#endif // UNITY_EDITOR
}
