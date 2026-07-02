using System.Collections;
using System.Linq;
using System.Reflection;
using Game.Gameplay;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.CharacterPresentation;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;

// ReSharper disable once CheckNamespace
public sealed class GameplaySceneCompositionTests : BaseGameplayScenePlayModeFixture
{
    [UnityTest]
    public IEnumerator given_GameplayScene_when_Loaded_then_SlingshotPrelaunchCompositionIsReady()
    {
        yield return LoadGameplayScene();
        var activeScene = SceneManager.GetActiveScene();
        yield return ContinueToPreLaunch(activeScene);
        yield return WaitUntilPlayerIsHeld(activeScene);

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var playerSteeringTarget = FindSingleInScene<RigidbodyPlayerSteeringTarget>(activeScene, "RigidbodyPlayerSteeringTarget");
        var runCameraSource = FindSingleInScene<RigidbodyRunCameraSource>(activeScene, "RigidbodyRunCameraSource");
        var contactNotifier = FindSingleInScene<RigidbodyContactNotifier>(activeScene, "RigidbodyContactNotifier");
        var runProgressFrameSource = FindSingleInScene<RunProgressFrameSource>(activeScene, "RunProgressFrameSource");
        var runSurfaceContextSource = FindSingleInScene<PhysicsRunSurfaceContextSource>(activeScene, "PhysicsRunSurfaceContextSource");
        var characterPresentationView = FindSingleInScene<CharacterPresentationView>(activeScene, "CharacterPresentationView");
        var runCameraAnchor = FindSingleInScene<TransformRunCameraAnchor>(activeScene, "Run Camera Anchor");
        var runCameraRig = FindSingleInScene<CinemachineRunCameraRig>(activeScene, "Run Camera Rig");
        var mainCamera = FindSingleInScene<Camera>(activeScene, "Main Camera");
        var brain = mainCamera.GetComponent<CinemachineBrain>();
        var runPreparationCamera = FindGameObjectByName(activeScene, "Run Preparation Camera").GetComponent<CinemachineCamera>();
        var preLaunchCamera = FindGameObjectByName(activeScene, "Pre-Launch Camera").GetComponent<CinemachineCamera>();
        var runCamera = FindGameObjectByName(activeScene, "Run Camera").GetComponent<CinemachineCamera>();
        var runPreparationHardLookAt = runPreparationCamera.GetComponent<CinemachineHardLookAt>();
        var preLaunchHardLookAt = preLaunchCamera.GetComponent<CinemachineHardLookAt>();
        var thirdPersonFollow = runCamera.GetComponent<CinemachineThirdPersonFollow>();
        var decollider = runCamera.GetComponent<CinemachineDecollider>();
        var deoccluder = runCamera.GetComponent<CinemachineDeoccluder>();
        var surface = FindGameObjectByName(activeScene, "Surface");
        var surfaceContact = surface.GetComponent<RunContact>();
        var runContactsRoot = FindGameObjectByName(activeScene, "Run Contacts");
        var runFinish = FindGameObjectByName(activeScene, "Run Finish");
        var runSafetyNet = FindGameObjectByName(activeScene, "Run Safety Net");
        var runObstacle = FindGameObjectByName(activeScene, "Run Obstacle");
        var canvas = FindSingleInScene<Canvas>(activeScene, "Gameplay UI Canvas");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var targetCollider = GetSingleTargetCollider(launchTarget);
        var geometry = slingshotView.CreateGeometrySnapshot();
        var bandCenter = FindGameObjectByName(activeScene, "Band Center");
        var launchTargetColliderRoot = FindGameObjectByName(activeScene, "LaunchTargetColliderRoot");
        var characterVisualAnchor = FindGameObjectByName(activeScene, "CharacterVisualAnchor");
        var playerCameraLookTarget = FindGameObjectByName(activeScene, "Player Camera Look Target");
        var ladybugCharacter = FindGameObjectByName(activeScene, "LadybugCharacter");
        var characterAnimator = ladybugCharacter.GetComponent<Animator>();

        var characterAnimationEventReceiver = ladybugCharacter.GetComponent<CharacterAnimationEventReceiver>();
        var preLaunchRigPoseRoot = FindGameObjectByName(activeScene, "Pre-Launch Rig Pose");
        var preLaunchSlingshotRigPose = FindGameObjectByName(activeScene, "Pre-Launch Slingshot Rig Pose");
        var preLaunchLaunchTargetPose = FindGameObjectByName(activeScene, "Pre-Launch Launch Target Pose");
        var pullHint = FindGameObjectByName(activeScene, "Pull Hint");
        var pullHintView = pullHint.GetComponent<PullHintView>();
        var pullHintRectTransform = pullHint.GetComponent<RectTransform>();
        var pullHintCanvasGroup = pullHint.GetComponent<CanvasGroup>();
        var pullHintAnimator = pullHint.GetComponent<Animator>();
        var pullHintFinger = pullHint.transform.Find("Finger");

        var pullHintFingerRectTransform = pullHintFinger != null
            ? pullHintFinger.GetComponent<RectTransform>()
            : null;

        var pullHintFingerImage = pullHintFinger != null
            ? pullHintFinger.GetComponent<Image>()
            : null;

        var touchIndicator = FindGameObjectByName(activeScene, "Touch Indicator");

        var bandShapeProvider = lifetimeScope.Container.Resolve<ISlingshotBandShapeProvider>();
        var preLaunchRigPoseResetter = lifetimeScope.Container.Resolve<IPreLaunchRigPoseResetter>();
        var resolvedPlayerSteeringTarget = lifetimeScope.Container.Resolve<IPlayerSteeringTarget>();
        var gameplaySlingshotLaunchConfig = lifetimeScope.Container.Resolve<IGameplaySlingshotLaunchConfig>();
        var playerSteeringConfig = lifetimeScope.Container.Resolve<IPlayerSteeringConfig>();
        var resolvedRunCameraSource = lifetimeScope.Container.Resolve<IRunCameraSource>();
        var resolvedRunMotionSource = lifetimeScope.Container.Resolve<IRunMotionSource>();
        var resolvedRunProgressFrameSource = lifetimeScope.Container.Resolve<IRunProgressFrameSource>();
        var resolvedRunSurfaceContextSource = lifetimeScope.Container.Resolve<IRunSurfaceContextSource>();
        var resolvedRunSteeringFrameSource = lifetimeScope.Container.Resolve<IRunSteeringFrameSource>();
        var resolvedContactNotifier = lifetimeScope.Container.Resolve<IRigidbodyContactNotifier>();
        var resolvedRunEndConfig = lifetimeScope.Container.Resolve<IRunEndConfig>();
        var resolvedRunProgressService = lifetimeScope.Container.Resolve<IRunProgressService>();
        var resolvedRunContactClassifier = lifetimeScope.Container.Resolve<IRunContactClassifier>();
        var resolvedCharacterPresentationView = lifetimeScope.Container.Resolve<ICharacterPresentationView>();
        var resolvedCharacterPresentationTuning = lifetimeScope.Container.Resolve<ICharacterPresentationTuning>();
        var resolvedPullHintView = lifetimeScope.Container.Resolve<IPullHintView>();
        var resolvedPullHintTuning = lifetimeScope.Container.Resolve<IPullHintTuning>();
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
        var assignedPlayerSteeringConfigs = GetAssignedPlayerSteeringConfigs(activeScene);
        var assignedRunCameraConfigs = GetAssignedRunCameraConfigs(activeScene);
        var assignedRunEndConfigs = GetAssignedRunEndConfigs(activeScene);
        var assignedRunProgressFrameSources = GetAssignedRunProgressFrameSources(activeScene);
        var cameraTerrainLayerMask = TestAssets.CameraTerrainLayerMask;
        var cameraObstacleLayerMask = TestAssets.CameraObstacleLayerMask;
        var cameraTerrainLayer = GetSingleLayer(cameraTerrainLayerMask, "Camera Terrain");
        var cameraObstacleLayer = GetSingleLayer(cameraObstacleLayerMask, "Camera Obstacle");
        var bandShapeOutput = new Vector3[bandShapeProvider.BandShapePointCount];

        var bandShapeSolved = bandShapeProvider.TryCreateBandShape(new SlingshotBandShapeQuery(
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
        Assert.That(runSurfaceContextSource, Is.Not.Null);
        Assert.That(characterPresentationView, Is.Not.Null);
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
            Is.GreaterThan(0.25f));

        Assert.That(
            Mathf.Abs(Vector3.Dot(preLaunchCamera.transform.position - playerCameraLookTarget.transform.position, Vector3.right)),
            Is.LessThan(0.15f));
        Assert.That(runCamera.Target.TrackingTarget, Is.SameAs(runCameraAnchor.transform));
        Assert.That(thirdPersonFollow, Is.Not.Null);
        Assert.That(decollider, Is.Not.Null);
        Assert.That(deoccluder, Is.Null);
        Assert.That(cameraTerrainLayer, Is.GreaterThanOrEqualTo(0));
        Assert.That(cameraObstacleLayer, Is.GreaterThanOrEqualTo(0));
        Assert.That(surface.layer, Is.EqualTo(cameraTerrainLayer));
        Assert.That(runObstacle.layer, Is.EqualTo(cameraObstacleLayer));
        Assert.That(surfaceContact, Is.Not.Null);
        Assert.That(surfaceContact.Category, Is.EqualTo(RunContactCategory.Surface));
        AssertRunSurfacePhysicsMaterial(activeScene, "LadybugHalfTubeCompletionGlide", 0.16f);
        AssertRunSurfacePhysicsMaterial(activeScene, "LadybugHalfTubeEarlyReachPressure", 0.25f);
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
        Assert.That(bandLineRenderer.positionCount, Is.GreaterThanOrEqualTo(3));
        Assert.That(geometry.LeftAnchorPosition.x, Is.LessThan(geometry.RightAnchorPosition.x));
        Assert.That(Vector3.Dot(geometry.LaunchFrameForward, Vector3.forward), Is.GreaterThan(0.99f));
        Assert.That(playerRigidbody, Is.Not.Null);
        Assert.That(runCameraSource.GetComponent<Rigidbody>(), Is.SameAs(playerRigidbody));
        Assert.That(playerSteeringTarget.GetComponent<Rigidbody>(), Is.SameAs(playerRigidbody));
        Assert.That(contactNotifier.GetComponent<Rigidbody>(), Is.SameAs(playerRigidbody));
        Assert.That(preLaunchRigPoseResetter, Is.Not.Null);
        Assert.That(resolvedLaunchTargetPreLaunchReset, Is.SameAs(launchTarget));
        Assert.That(resolvedPlayerSteeringTarget, Is.SameAs(playerSteeringTarget));
        Assert.That(resolvedRunCameraSource, Is.SameAs(runCameraSource));
        Assert.That(resolvedRunMotionSource, Is.SameAs(runCameraSource));
        Assert.That(resolvedRunProgressFrameSource, Is.SameAs(runProgressFrameSource));
        Assert.That(resolvedRunSurfaceContextSource, Is.SameAs(runSurfaceContextSource));
        Assert.That(resolvedRunSteeringFrameSource, Is.Not.Null);
        Assert.That(resolvedContactNotifier, Is.SameAs(contactNotifier));
        Assert.That(resolvedRunEndConfig, Is.SameAs(assignedRunEndConfigs[0]));
        Assert.That(resolvedRunProgressService, Is.Not.Null);
        Assert.That(resolvedRunContactClassifier, Is.Not.Null);
        Assert.That(resolvedCharacterPresentationView, Is.SameAs(characterPresentationView));
        Assert.That(resolvedCharacterPresentationTuning, Is.SameAs(characterPresentationView));
        Assert.That(resolvedCharacterPresentationTuning.AirborneDelaySeconds, Is.EqualTo(0.12f).Within(0.0001f));
        Assert.That(resolvedCharacterPresentationTuning.MeaningfulGroundedMovementThreshold, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(resolvedCharacterPresentationTuning.MinimumLocomotionModeDuration, Is.EqualTo(0.35f).Within(0.0001f));
        Assert.That(resolvedCharacterPresentationTuning.LaunchPushMinimumSeconds, Is.EqualTo(0.25f).Within(0.0001f));
        Assert.That(resolvedCharacterPresentationTuning.SlideReferenceSpeed, Is.EqualTo(8f).Within(0.0001f));
        Assert.That(resolvedPullHintView, Is.SameAs(pullHintView));
        Assert.That(resolvedPullHintTuning, Is.SameAs(pullHintView));
        Assert.That(resolvedCharacterPresentationModeClassifier, Is.Not.Null);
        Assert.That(resolvedSlingshotActivePullNotifier, Is.Not.Null);
        Assert.That(resolvedSlingshotCaptureLifecycleNotifier, Is.Not.Null);
        Assert.That(resolvedSlingshotPresentationContextSource, Is.Not.Null);
        Assert.That(resolvedSlingshotPullOffsetNormalizer, Is.Not.Null);
        Assert.That(resolvedRunEndCandidateReceiver, Is.Not.Null);
        Assert.That(resolvedRunCameraAnchor, Is.SameAs(runCameraAnchor));
        Assert.That(resolvedRunCameraRig, Is.SameAs(runCameraRig));
        Assert.That(((IPlayerSteeringTarget)playerSteeringTarget).LinearVelocity, Is.EqualTo(playerRigidbody.linearVelocity));
        Assert.That(((IRunMotionSource)runCameraSource).Position, Is.EqualTo(playerRigidbody.position));
        Assert.That(((IRunMotionSource)runCameraSource).LinearVelocity, Is.EqualTo(playerRigidbody.linearVelocity));
        Assert.That(targetCollider, Is.Not.Null);
        Assert.That(targetCollider.transform, Is.SameAs(launchTargetColliderRoot.transform));
        Assert.That(runSurfaceContextSource.SupportColliderForTests, Is.SameAs(targetCollider));
        Assert.That(runSurfaceContextSource.SupportProbeDistanceForTests, Is.LessThanOrEqualTo(0.25f));
        Assert.That(runSurfaceContextSource.SurfaceMaskForTests.value, Is.EqualTo(TestAssets.RunSurfaceLayerMask.value));
        Assert.That(launchTargetColliderRoot.transform.IsChildOf(launchTarget.transform), Is.True);
        Assert.That(launchTargetColliderRoot.GetComponent<MeshRenderer>(), Is.Null);
        Assert.That(launchTargetColliderRoot.GetComponent<MeshFilter>(), Is.Null);
        Assert.That(characterVisualAnchor.transform.IsChildOf(launchTarget.transform), Is.True);
        Assert.That(ladybugCharacter.transform.IsChildOf(characterVisualAnchor.transform), Is.True);
        Assert.That(characterPresentationView.transform, Is.SameAs(ladybugCharacter.transform));
        Assert.That(characterAnimator, Is.Not.Null);
        Assert.That(characterAnimator.runtimeAnimatorController, Is.Not.Null);
        Assert.That(characterAnimator.avatar, Is.Not.Null);
        Assert.That(characterAnimator.applyRootMotion, Is.False);
        AssertAnimatorParameter(characterAnimator, "PresentationMode", AnimatorControllerParameterType.Int);
        AssertAnimatorParameter(characterAnimator, "PlaybackSpeedMultiplier", AnimatorControllerParameterType.Float);
        AssertAnimatorParameter(characterAnimator, "NormalizedPull", AnimatorControllerParameterType.Float);
        AssertAnimatorParameter(characterAnimator, "NormalizedLaunchPower", AnimatorControllerParameterType.Float);
        AssertAnimatorParameter(characterAnimator, "NormalizedPullOffset", AnimatorControllerParameterType.Float);
        AssertAnimatorParameter(characterAnimator, "NormalizedLaunchOffset", AnimatorControllerParameterType.Float);
        Assert.That(characterAnimationEventReceiver, Is.Not.Null);
        Assert.That(characterAnimationEventReceiver.transform, Is.SameAs(characterAnimator.transform));

        Assert.That(
            characterAnimationEventReceiver.GetType().GetMethod(
                "StepOnGround",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: System.Type.EmptyTypes,
                modifiers: null),
            Is.Not.Null);
        Assert.That(ladybugCharacter.GetComponentsInChildren<Collider>(true), Is.Empty);
        Assert.That(ladybugCharacter.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
        Assert.That(ladybugCharacter.GetComponentsInChildren<Joint>(true), Is.Empty);
        Assert.That(ladybugCharacter.GetComponentsInChildren<CharacterController>(true), Is.Empty);
        Assert.That(bandShapeSolved, Is.True);
        Assert.That(bandShapePointCount, Is.EqualTo(bandShapeProvider.BandShapePointCount));
        Assert.That(bandShapePointCount, Is.GreaterThan(3));
        Assert.That(assignedPlayerSteeringConfigs, Has.Length.EqualTo(1));
        Assert.That(assignedRunCameraConfigs, Has.Length.EqualTo(1));
        Assert.That(assignedRunEndConfigs, Has.Length.EqualTo(1));
        Assert.That(assignedRunProgressFrameSources, Has.Length.EqualTo(1));
        Assert.That(playerSteeringConfig, Is.SameAs(assignedPlayerSteeringConfigs[0]));
        Assert.That(runCameraConfig, Is.SameAs(assignedRunCameraConfigs[0]));
        Assert.That(playerSteeringConfig, Is.Not.Null);
        Assert.That(runCameraConfig, Is.Not.Null);
        Assert.That(resolvedRunEndConfig, Is.Not.Null);
        Assert.That(gameplaySlingshotLaunchConfig.MinimumForwardImpulse, Is.EqualTo(15f).Within(0.0001f));
        Assert.That(gameplaySlingshotLaunchConfig.MaximumForwardImpulse, Is.EqualTo(30f).Within(0.0001f));
        Assert.That(gameplaySlingshotLaunchConfig.UpwardImpulse, Is.EqualTo(1.5f).Within(0.0001f));
        Assert.That(gameplaySlingshotLaunchConfig.MaximumLateralLaunchAngleDegrees, Is.EqualTo(35f).Within(0.0001f));
        Assert.That(gameplaySlingshotLaunchConfig.HasMinimumTotalImpulse, Is.False);
        Assert.That(gameplaySlingshotLaunchConfig.HasMaximumTotalImpulse, Is.False);
        Assert.That(resolvedRunEndConfig.LostMomentumLaunchGraceDuration, Is.EqualTo(1.25f).Within(0.0001f));
        Assert.That(resolvedRunEndConfig.LostMomentumDuration, Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(resolvedRunEndConfig.LostMomentumPlanarSpeedThreshold, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(resolvedRunEndConfig.LostMomentumProgressThreshold, Is.EqualTo(0.5f).Within(0.0001f));

        Assert.That(runProgressFrameSource.TryCreateSnapshot(playerRigidbody.position, out var frameSnapshot, out var frameError), Is.True,
            frameError);
        Assert.That(frameSnapshot.ForwardDirection.sqrMagnitude, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(frameSnapshot.RightDirection.sqrMagnitude, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(frameSnapshot.UpDirection.sqrMagnitude, Is.EqualTo(1f).Within(0.0001f));
        AssertRunContactPlaceholder(runContactsRoot, runFinish, RunContactCategory.Finish, true);
        AssertRunContactPlaceholder(runContactsRoot, runSafetyNet, RunContactCategory.SafetyNet, true);
        AssertRunContactPlaceholder(runContactsRoot, runObstacle, RunContactCategory.Obstacle, false);
        Assert.That(System.Enum.GetNames(typeof(RunContactCategory)), Does.Not.Contain("Boundary"));
        Assert.That(System.Enum.GetNames(typeof(RunContactCategory)), Does.Not.Contain("Ramp"));
        Assert.That(playerRigidbody.collisionDetectionMode, Is.EqualTo(CollisionDetectionMode.ContinuousDynamic));
        Assert.That(playerRigidbody.interpolation, Is.EqualTo(RigidbodyInterpolation.Interpolate));
        Assert.That(playerRigidbody.isKinematic, Is.True);
        Assert.That(bandCenter.transform.IsChildOf(launchTarget.transform), Is.True);
        Assert.That(bandCenter.transform.position.x, Is.EqualTo(geometry.RestPoint.x).Within(0.01f));
        Assert.That(bandCenter.transform.position.y, Is.EqualTo(geometry.RestPoint.y).Within(0.01f));
        Assert.That(bandCenter.transform.position.z, Is.EqualTo(geometry.RestPoint.z).Within(0.01f));

        Assert.That(Quaternion.Angle(preLaunchLaunchTargetPose.transform.rotation, playerRigidbody.transform.rotation),
            Is.EqualTo(0f).Within(0.01f));
        Assert.That(pullHint.transform.IsChildOf(canvas.transform), Is.True);
        Assert.That(pullHintView, Is.Not.Null);
        Assert.That(pullHintRectTransform, Is.Not.Null);
        Assert.That(pullHintRectTransform.sizeDelta.x, Is.EqualTo(192f).Within(0.001f));
        Assert.That(pullHintRectTransform.sizeDelta.y, Is.EqualTo(320f).Within(0.001f));
        Assert.That(pullHintCanvasGroup, Is.Not.Null);
        Assert.That(pullHintCanvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
        Assert.That(pullHintCanvasGroup.interactable, Is.False);
        Assert.That(pullHintCanvasGroup.blocksRaycasts, Is.False);
        Assert.That(pullHintAnimator, Is.Not.Null);
        Assert.That(pullHintAnimator.runtimeAnimatorController, Is.Not.Null);
        AssertAnimatorParameter(pullHintAnimator, "PlayPullHint", AnimatorControllerParameterType.Trigger);
        Assert.That(pullHintFinger, Is.Not.Null);
        Assert.That(pullHintFingerRectTransform, Is.Not.Null);
        Assert.That(pullHintFingerRectTransform.sizeDelta.x, Is.EqualTo(160f).Within(0.001f));
        Assert.That(pullHintFingerRectTransform.sizeDelta.y, Is.EqualTo(160f).Within(0.001f));
        Assert.That(pullHintFingerImage, Is.Not.Null);
        Assert.That(pullHintFingerImage.sprite, Is.Not.Null);
        Assert.That(pullHintFingerImage.raycastTarget, Is.False);
        Assert.That(pullHint.activeSelf, Is.False);
        resolvedPullHintView.ShowAt(Vector2.zero);
        Assert.That(pullHint.activeSelf, Is.True);
        Assert.That(pullHintCanvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
        Assert.That(pullHintFingerRectTransform.anchoredPosition.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(pullHintFingerRectTransform.anchoredPosition.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(pullHintFingerImage.sprite, Is.Not.Null);
        resolvedPullHintView.Hide();
        Assert.That(pullHint.activeSelf, Is.False);
        Assert.That(pullHintCanvasGroup.alpha, Is.EqualTo(0f).Within(0.001f));
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

        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
        var bandCenter = FindGameObjectByName(activeScene, "Band Center");
        var preLaunchSlingshotRigPose = FindGameObjectByName(activeScene, "Pre-Launch Slingshot Rig Pose");
        var preLaunchLaunchTargetPose = FindGameObjectByName(activeScene, "Pre-Launch Launch Target Pose");
        var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
        var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
        var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();

        Assert.That(stateService.TryTransitionTo(lifetimeScope.RunningStateIdForTests), Is.True);
        slingshotView.transform.SetPositionAndRotation(new Vector3(1f, 0.25f, 0.5f), Quaternion.Euler(0f, 15f, 0f));
        playerRigidbody.isKinematic = false;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        playerRigidbody.linearVelocity = new Vector3(3f, 0f, 2f);
        playerRigidbody.angularVelocity = new Vector3(1f, 2f, 3f);
        playerRigidbody.transform.SetPositionAndRotation(new Vector3(4f, 2f, -3f), Quaternion.Euler(10f, 80f, 25f));
        playerRigidbody.position = playerRigidbody.transform.position;
        playerRigidbody.rotation = playerRigidbody.transform.rotation;

        Assert.That(stateService.TryTransitionTo(lifetimeScope.RunEndedStateIdForTests), Is.True);
        Assert.That(stateService.TryTransitionTo(lifetimeScope.RunPreparationStateIdForTests), Is.True);
        yield return ContinueToPreLaunch(activeScene);

        var geometry = slingshotView.CreateGeometrySnapshot();

        Assert.That(slingshotView.transform.position, Is.EqualTo(preLaunchSlingshotRigPose.transform.position));

        Assert.That(Quaternion.Angle(preLaunchSlingshotRigPose.transform.rotation, slingshotView.transform.rotation),
            Is.EqualTo(0f).Within(0.0001f));

        Assert.That(Quaternion.Angle(preLaunchLaunchTargetPose.transform.rotation, playerRigidbody.rotation),
            Is.EqualTo(0f).Within(0.0001f));
        AssertPlayerIsHeld(playerRigidbody);
        Assert.That(playerRigidbody.constraints, Is.EqualTo(RigidbodyConstraints.None));
        Assert.That(Vector3.Distance(bandCenter.transform.position, geometry.RestPoint), Is.LessThan(0.01f));
        AssertBandLineCenterEqualsRest(bandLineRenderer, geometry);
    }

    [UnityTest]
    public IEnumerator given_GameplayScene_when_RepeatedLaunchRestartsInSameSession_then_PreLaunchRigPoseRemainsCentered()
    {
        var mouse = InputSystem.AddDevice<Mouse>("Gameplay Scene Restart Alignment Mouse");

        try
        {
            yield return LoadGameplayScene();
            var activeScene = SceneManager.GetActiveScene();
            yield return ContinueToPreLaunch(activeScene);
            yield return WaitUntilPlayerIsHeld(activeScene);

            var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(activeScene, "GameplayLifetimeScope");
            var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
            var launchTarget = FindSingleInScene<RigidbodyLaunchTarget>(activeScene, "RigidbodyLaunchTarget");
            var inputCamera = FindSingleInScene<Camera>(activeScene, "Input Camera");
            var bandLineRenderer = slingshotView.GetComponent<LineRenderer>();
            var playerRigidbody = launchTarget.GetComponent<Rigidbody>();
            var stateService = lifetimeScope.Container.Resolve<IGameplayStateService>();
            var bandCenter = FindGameObjectByName(activeScene, "Band Center");
            var preLaunchLaunchTargetPose = FindGameObjectByName(activeScene, "Pre-Launch Launch Target Pose");

            var launchScenarios = new[]
            {
                new Vector2(0f, 0.45f),
                new Vector2(0.75f, 1.25f),
                new Vector2(-0.75f, 1.25f),
                new Vector2(0.25f, 1.75f)
            };

            for (var scenarioIndex = 0; scenarioIndex < launchScenarios.Length; scenarioIndex += 1)
            {
                var geometry = slingshotView.CreateGeometrySnapshot();
                var pressScreenPosition = GetScreenPosition(inputCamera, geometry.RestPoint);

                var pullWorldPosition = geometry.RestPoint
                                        + (geometry.LaunchFrameRight * launchScenarios[scenarioIndex].x)
                                        - (geometry.LaunchFrameForward * launchScenarios[scenarioIndex].y);
                var releaseScreenPosition = GetScreenPosition(inputCamera, pullWorldPosition);

                yield return SendMouse(mouse, pressScreenPosition, true);
                yield return SendMouse(mouse, releaseScreenPosition, true);
                yield return SendMouse(mouse, releaseScreenPosition, false);
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

        var slingshotView = FindSingleInScene<SlingshotView>(activeScene, "SlingshotView");
        var geometry = slingshotView.CreateGeometrySnapshot();
        var leftAnchor = FindGameObjectByName(activeScene, "Left Anchor");
        var rightAnchor = FindGameObjectByName(activeScene, "Right Anchor");
        var leftPole = FindGameObjectByName(activeScene, "Left Slingshot Pole");
        var rightPole = FindGameObjectByName(activeScene, "Right Slingshot Pole");

        AssertPoleFramesAnchor(leftPole, leftAnchor.transform, geometry.LeftAnchorPosition, "Left Slingshot Pole");
        AssertPoleFramesAnchor(rightPole, rightAnchor.transform, geometry.RightAnchorPosition, "Right Slingshot Pole");
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

        if (!TryFindGameObjectByName(scene, "Pull Hint", out var pullHint) || pullHint.activeSelf)
            return false;

        if (!TryFindGameObjectByName(scene, "Touch Indicator", out var touchIndicator) || touchIndicator.activeSelf)
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

        Assert.Fail("Expected Player to be held by the Slingshot.");
    }

    private IEnumerator ContinueToPreLaunch(Scene scene)
    {
        var lifetimeScope = FindSingleInScene<GameplayLifetimeScope>(scene, "GameplayLifetimeScope");
        var continueCommand = lifetimeScope.Container.Resolve<IRunPreparationContinueCommand>();
        continueCommand.TryContinue();
        yield return null;
    }

    private PlayerSteeringConfig[] GetAssignedPlayerSteeringConfigs(Scene scene)
    {
        return FindComponentsInScene<GameplayLifetimeScope>(scene)
            .Select(lifetimeScope => lifetimeScope.PlayerSteeringConfigForTests)
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
        var colliders = launchTarget.GetComponentsInChildren<Collider>(true);

        Assert.That(colliders, Has.Length.EqualTo(1));
        return colliders[0];
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

    private void AssertRunSurfacePhysicsMaterial(Scene scene, string materialName, float expectedFriction)
    {
        var materials = FindComponentsInScene<Collider>(scene)
            .Where(collider => collider.sharedMaterial != null && collider.sharedMaterial.name == materialName)
            .Select(collider => collider.sharedMaterial)
            .Distinct()
            .ToArray();

        Assert.That(materials, Has.Length.EqualTo(1), materialName);

        var material = materials[0];
        Assert.That(material.dynamicFriction, Is.EqualTo(expectedFriction).Within(0.0001f), materialName);
        Assert.That(material.staticFriction, Is.EqualTo(expectedFriction).Within(0.0001f), materialName);
        Assert.That(material.bounciness, Is.EqualTo(0f).Within(0.0001f), materialName);
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
        var colliders = pole.GetComponentsInChildren<Collider>(true);

        Assert.That(pole.transform.IsChildOf(anchor), Is.True, poleName);
        Assert.That(meshRenderer, Is.Not.Null, poleName);
        Assert.That(meshRenderer.enabled, Is.True, poleName);
        Assert.That(meshRenderer.sharedMaterial, Is.Not.Null, poleName);
        Assert.That(meshFilter, Is.Not.Null, poleName);
        Assert.That(meshFilter.sharedMesh, Is.Not.Null, poleName);
        Assert.That(colliders, Is.Empty, poleName);
        Assert.That(pole.transform.position.x, Is.EqualTo(anchorPosition.x).Within(0.01f), poleName);
        Assert.That(pole.transform.position.z, Is.EqualTo(anchorPosition.z).Within(0.01f), poleName);
        Assert.That(pole.transform.position.y, Is.LessThan(anchorPosition.y), poleName);
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

    private int GetSingleLayer(LayerMask layerMask, string description)
    {
        Assert.That(layerMask.value, Is.Not.EqualTo(0), description);
        Assert.That(layerMask.value & (layerMask.value - 1), Is.Zero, description);
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, 2f));
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

    private void DisturbLaunchedPlayerPose(Rigidbody playerRigidbody, int scenarioIndex)
    {
        var position = new Vector3(2f + scenarioIndex, 0.75f + (scenarioIndex * 0.25f), -1f - scenarioIndex);
        var rotation = Quaternion.Euler(25f + (scenarioIndex * 7f), 45f + (scenarioIndex * 13f), 15f + (scenarioIndex * 5f));

        playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        playerRigidbody.linearVelocity = new Vector3(3f + scenarioIndex, 0.5f, 2f);
        playerRigidbody.angularVelocity = new Vector3(1f, 2f + scenarioIndex, 3f);
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

        Assert.That(Quaternion.Angle(preLaunchLaunchTargetPose.rotation, playerRigidbody.rotation),
            Is.EqualTo(0f).Within(0.0001f),
            assertionScope);
        AssertPlayerIsHeld(playerRigidbody);
        Assert.That(playerRigidbody.constraints, Is.EqualTo(RigidbodyConstraints.None), assertionScope);
        Assert.That(Vector3.Distance(bandCenter.position, geometry.RestPoint), Is.LessThan(0.01f), assertionScope);
        AssertBandLineCenterEqualsRest(bandLineRenderer, geometry);
    }

    private void AssertBandLineCenterEqualsRest(LineRenderer bandLineRenderer, SlingshotGeometrySnapshot geometry)
    {
        Assert.That(bandLineRenderer.positionCount, Is.GreaterThanOrEqualTo(3));

        var middleIndex = (bandLineRenderer.positionCount - 1) / 2;

        Assert.That(Vector3.Distance(bandLineRenderer.GetPosition(middleIndex), geometry.RestPoint), Is.LessThan(0.01f));
        Assert.That(Vector3.Distance(bandLineRenderer.GetPosition(0), geometry.LeftAnchorPosition), Is.LessThan(0.01f));

        Assert.That(Vector3.Distance(bandLineRenderer.GetPosition(bandLineRenderer.positionCount - 1), geometry.RightAnchorPosition),
            Is.LessThan(0.01f));
    }
}
