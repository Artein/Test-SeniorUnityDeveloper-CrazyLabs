using System;
using System.Collections;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Slingshot;
using Game.Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace Game.Gameplay.Tests.PlayMode
{
    internal readonly struct RunBodyContactPhysicsStep
    {
        public Vector3 SampledVelocity { get; }
        public Vector3 WrittenVelocity { get; }
        public Vector3 PostSolverVelocity { get; }
        public Vector3 SampledPosition { get; }
        public Vector3 PostSolverPosition { get; }
        public RunSurfaceContext SurfaceContext { get; }
        public RunBodySpeedDiagnosticsSnapshot Diagnostics { get; }
        public int MovementWriteCount { get; }

        public RunBodyContactPhysicsStep(
            Vector3 sampledVelocity,
            Vector3 writtenVelocity,
            Vector3 postSolverVelocity,
            Vector3 sampledPosition,
            Vector3 postSolverPosition,
            RunSurfaceContext surfaceContext,
            RunBodySpeedDiagnosticsSnapshot diagnostics,
            int movementWriteCount)
        {
            SampledVelocity = sampledVelocity;
            WrittenVelocity = writtenVelocity;
            PostSolverVelocity = postSolverVelocity;
            SampledPosition = sampledPosition;
            PostSolverPosition = postSolverPosition;
            SurfaceContext = surfaceContext;
            Diagnostics = diagnostics;
            MovementWriteCount = movementWriteCount;
        }
    }

    internal sealed class RunBodyContactPhysicsScenario : IDisposable
    {
        private readonly List<UnityEngine.Object> _createdObjects = new();
        private readonly HashSet<Collider> _collidedWith = new();
        private readonly RunBodyContactPhysicsConfig _config;
        private readonly SlingshotLaunchController _launchEvents;
        private readonly RunSurfaceFramePipeline _surfaceFramePipeline;
        private readonly RunBodySpeedDiagnostics _diagnostics;
        private readonly RecordingRunBodyMovementTarget _recordingMovementTarget;
        private readonly RigidbodyContactNotifier _contactNotifier;
        private readonly RunBodyMovementController _controller;
        private readonly int _runSurfaceLayer;
        private bool _isDisposed;
        private bool _isRunActive;

        public Rigidbody Body { get; }
        public SphereCollider BodyCollider { get; }
        public float BodyRadius => BodyCollider.radius;
        public RunBodyContactPhysicsStep CurrentStep { get; private set; }

        public RunBodyContactPhysicsScenario(
            LayerMask runSurfaceLayerMask,
            Vector3 origin,
            RunBodyContactPhysicsConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _runSurfaceLayer = ResolveSingleLayer(runSurfaceLayerMask);

            var bodyObject = Track(new GameObject("Run Body Contact Physics Test Body"));
            bodyObject.transform.position = origin + Vector3.up * 2f;

            BodyCollider = bodyObject.AddComponent<SphereCollider>();
            BodyCollider.radius = 0.5f;

            BodyCollider.sharedMaterial = CreatePhysicsMaterial(
                "Run Body Contact Physics Body Material",
                _config.BodyBounciness);

            Body = bodyObject.AddComponent<Rigidbody>();
            Body.mass = 1f;
            Body.useGravity = true;
            Body.constraints = RigidbodyConstraints.FreezeRotation;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Body.interpolation = RigidbodyInterpolation.None;

            _contactNotifier = bodyObject.AddComponent<RigidbodyContactNotifier>();
            _contactNotifier.CollisionEntered += OnCollisionEntered;

            var rigidbodyMovementTarget = bodyObject.AddComponent<RigidbodyRunBodyMovementTarget>();
            rigidbodyMovementTarget.SetRigidbodyForTests(Body);
            _recordingMovementTarget = new RecordingRunBodyMovementTarget(rigidbodyMovementTarget);

            var runningStateId = Track(ScriptableObject.CreateInstance<GameplayStateId>());
            runningStateId.name = "Running";
            var playerMaxSpeedStatId = Track(ScriptableObject.CreateInstance<GameplayStatId>());
            playerMaxSpeedStatId.name = "PlayerMaxSpeed";

            var progressContext = new FixedRunProgressContext(origin, Vector3.forward, Vector3.up);
            var clock = new UnityTime();

            var slopeCalculator = new RunSurfaceSlopeCalculator();

            var supportProbe = new PhysicsRunSupportProbe(
                BodyCollider,
                new RunSupportColliderProbeFactory(),
                new RunSurfaceProbeConfig(
                    _config.SupportProbeDistance,
                    0.02f,
                    runSurfaceLayerMask,
                    0.17f,
                    0.6f,
                    8f),
                slopeCalculator);

            _surfaceFramePipeline = new RunSurfaceFramePipeline(
                progressContext,
                supportProbe,
                new RunSurfaceStabilityPolicy(
                    new RunSurfaceStabilityConfig(
                        _config.RunSurfaceSupportLossConfirmationSeconds,
                        _config.RunSurfaceDiscontinuousNormalThresholdDegrees,
                        _config.RunSurfaceDiscontinuousNormalConfirmationSeconds,
                        _config.RunSurfaceCandidateCoherenceDegrees),
                    slopeCalculator),
                new RunSteeringFramePolicy(
                    new RunSteeringFrameConfig(
                        _config.RunSteeringFrameNormalSlewDegreesPerSecond,
                        _config.RunSteeringFrameAirborneUpRetentionSeconds)),
                clock);

            _diagnostics = new RunBodySpeedDiagnostics();
            _launchEvents = new SlingshotLaunchController();

            _controller = new RunBodyMovementController(
                new RunBodyContactPhysicsStateService(runningStateId),
                _launchEvents,
                _recordingMovementTarget,
                new NeutralRunSteeringInputSource(),
                new DefaultRunBodySpeedEvaluator(_config),
                new DefaultRunSteeringEvaluator(),
                new RunLaunchLandingStabilizer(_config),
                _surfaceFramePipeline,
                _surfaceFramePipeline,
                _surfaceFramePipeline,
                progressContext,
                new FixedRunGameplayStatResolver(_config.BaseSoftMaximumSpeed),
                _diagnostics,
                _config,
                _config,
                _config,
                new RunBodySpeedEnvelopeValidator(_config),
                clock,
                playerMaxSpeedStatId,
                runningStateId);

            ((IInitializable)_controller).Initialize();
        }

        public Collider CreateRunSurface(
            string name,
            Vector3 topPoint,
            Quaternion rotation,
            Vector3 size,
            float bounciness = 0f)
        {
            var surfaceObject = Track(new GameObject(name));
            surfaceObject.layer = _runSurfaceLayer;

            surfaceObject.transform.SetPositionAndRotation(
                topPoint - rotation * Vector3.up * (size.y * 0.5f),
                rotation);
            surfaceObject.transform.localScale = size;

            var collider = surfaceObject.AddComponent<BoxCollider>();
            collider.sharedMaterial = CreatePhysicsMaterial(name + " Material", bounciness);
            surfaceObject.AddComponent<RunContact>().SetCategoryForTests(RunContactCategory.Surface);
            return collider;
        }

        public Collider CreateWall(string name, Vector3 center, Vector3 size)
        {
            var wallObject = Track(new GameObject(name));
            wallObject.layer = ResolveNonSurfaceLayer();
            wallObject.transform.SetPositionAndRotation(center, Quaternion.identity);
            wallObject.transform.localScale = size;

            var collider = wallObject.AddComponent<BoxCollider>();
            collider.sharedMaterial = CreatePhysicsMaterial(name + " Material", 0f);
            wallObject.AddComponent<RunContact>().SetCategoryForTests(RunContactCategory.Obstacle);
            return collider;
        }

        public Rigidbody CreateProjectile(
            string name,
            Vector3 position,
            Vector3 velocity,
            float radius,
            float mass,
            out Collider projectileCollider)
        {
            var projectileObject = Track(new GameObject(name));
            projectileObject.layer = ResolveNonSurfaceLayer();
            projectileObject.transform.position = position;

            var sphereCollider = projectileObject.AddComponent<SphereCollider>();
            sphereCollider.radius = radius;
            sphereCollider.sharedMaterial = CreatePhysicsMaterial(name + " Material", 0f);
            projectileCollider = sphereCollider;

            var projectileBody = projectileObject.AddComponent<Rigidbody>();
            projectileBody.mass = mass;
            projectileBody.useGravity = false;
            projectileBody.constraints = RigidbodyConstraints.FreezeRotation;
            projectileBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            projectileBody.interpolation = RigidbodyInterpolation.None;
            projectileBody.position = position;
            projectileBody.linearVelocity = velocity;
            return projectileBody;
        }

        public void SetBodyPose(Vector3 position, Vector3 velocity)
        {
            Body.transform.SetPositionAndRotation(position, Quaternion.identity);
            Body.position = position;
            Body.rotation = Quaternion.identity;
            Body.linearVelocity = velocity;
            Body.angularVelocity = Vector3.zero;
            Body.WakeUp();
        }

        public void SetBodyVelocity(Vector3 velocity)
        {
            Body.linearVelocity = velocity;
            Body.angularVelocity = Vector3.zero;
            Body.WakeUp();
        }

        public void SynchronizeTransforms()
        {
            Physics.SyncTransforms();
        }

        public void ActivateRun()
        {
            if (_isRunActive)
                return;

            _isRunActive = true;
            _launchEvents.Publish(CreateLaunchAppliedEvent());
        }

        public bool HasCollisionWith(Collider collider)
        {
            return collider != null && _collidedWith.Contains(collider);
        }

        public IEnumerator Step()
        {
            ((IFixedTickable)_surfaceFramePipeline).FixedTick();

            _recordingMovementTarget.BeginStep();
            var sampledVelocity = Body.linearVelocity;
            var sampledPosition = Body.position;
            var surfaceContext = _surfaceFramePipeline.Current.StableSupport;

            ((IFixedTickable)_controller).FixedTick();

            var writtenVelocity = _recordingMovementTarget.HasLastTargetState
                ? _recordingMovementTarget.LastTargetState.LinearVelocity
                : sampledVelocity;
            var diagnostics = _diagnostics.Current;
            var movementWriteCount = _recordingMovementTarget.StepWriteCount;

            yield return new WaitForFixedUpdate();

            CurrentStep = new RunBodyContactPhysicsStep(
                sampledVelocity,
                writtenVelocity,
                Body.linearVelocity,
                sampledPosition,
                Body.position,
                surfaceContext,
                diagnostics,
                movementWriteCount);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _contactNotifier.CollisionEntered -= OnCollisionEntered;
            ((IDisposable)_controller).Dispose();

            for (var index = _createdObjects.Count - 1; index >= 0; index -= 1)
            {
                if (_createdObjects[index] != null)
                    UnityEngine.Object.DestroyImmediate(_createdObjects[index]);
            }

            _createdObjects.Clear();
            _collidedWith.Clear();
        }

        private void OnCollisionEntered(RigidbodyCollisionNotification notification)
        {
            if (notification?.OtherCollider != null)
                _collidedWith.Add(notification.OtherCollider);
        }

        private PhysicsMaterial CreatePhysicsMaterial(string name, float bounciness)
        {
            var material = Track(new PhysicsMaterial(name));
            material.dynamicFriction = 0f;
            material.staticFriction = 0f;
            material.bounciness = Mathf.Clamp01(bounciness);
            material.frictionCombine = PhysicsMaterialCombine.Minimum;
            material.bounceCombine = PhysicsMaterialCombine.Maximum;
            return material;
        }

        private SlingshotLaunchAppliedEvent CreateLaunchAppliedEvent()
        {
            var request = new SlingshotLaunchRequest(
                1f,
                1f,
                0f,
                0f,
                Body.position,
                Vector3.forward,
                Vector3.up);

            return new SlingshotLaunchAppliedEvent(
                request,
                Body.linearVelocity,
                Vector3.forward,
                Vector3.up);
        }

        private int ResolveSingleLayer(LayerMask layerMask)
        {
            var value = layerMask.value;

            if (value <= 0 || (value & (value - 1)) != 0)
                throw new ArgumentException("Expected exactly one Run Surface layer.", nameof(layerMask));

            var layer = 0;

            while ((value >>= 1) > 0)
                layer += 1;

            return layer;
        }

        private int ResolveNonSurfaceLayer()
        {
            for (var layer = 0; layer < 32; layer += 1)
            {
                if (layer != _runSurfaceLayer)
                    return layer;
            }

            throw new InvalidOperationException("Could not resolve a non-surface physics layer.");
        }

        private T Track<T>(T createdObject) where T : UnityEngine.Object
        {
            _createdObjects.Add(createdObject);
            return createdObject;
        }
    }
}
