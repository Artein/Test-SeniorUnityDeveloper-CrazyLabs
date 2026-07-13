using System;
using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using Game.Gameplay.Economy;
using Game.Gameplay.GameplayState;
using Game.Gameplay.Tests.Common;
using Game.Gameplay.Tests.PlayMode;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public sealed class RunSurfaceAirTimePlayModeTests : BaseGameplayTestAssetsFixture
{
    private readonly List<Object> _createdObjects = new();
    private readonly Vector3 _testOrigin = new(x: 1200f, y: 0f, z: 1200f);
    private readonly List<RunAirTimeTracker> _trackers = new();

    [TearDown]
    public void OnTearDown()
    {
        for (var i = 0; i < _trackers.Count; i += 1)
        {
            ((IDisposable)_trackers[i]).Dispose();
        }

        _trackers.Clear();

        for (var i = 0; i < _createdObjects.Count; i += 1)
        {
            if (_createdObjects[i] != null)
                Object.DestroyImmediate(_createdObjects[i]);
        }

        _createdObjects.Clear();
    }

    [TestCase(arg1: 0.01f, arg2: 6)]
    [TestCase(arg1: 0.02f, arg2: 3)]
    public void given_BriefSupportGap_when_PipelineAndAirTimeTick_then_NoAirTimeIsAwarded(
        float fixedDeltaTime,
        int supportLossTick)
    {
        var fixture = CreateFixture(fixedDeltaTime);
        fixture.ShowSurface();

        var acquired = fixture.Tick();

        Assert.That(acquired.Transition, Is.EqualTo(RunSurfaceTransition.SupportAcquired));
        Assert.That(fixture.AirTimeSeconds, Is.Zero);

        fixture.HideSurface();

        for (var missTick = 1; missTick < supportLossTick; missTick += 1)
        {
            var held = fixture.Tick();

            Assert.That(
                held.ObservedSupport.State,
                Is.EqualTo(RunSupportObservationState.Missing),
                fixture.FailureContext(scenario: "brief gap", missTick));

            Assert.That(
                held.StableSupport.IsGrounded,
                Is.True,
                fixture.FailureContext(scenario: "brief gap", missTick));

            Assert.That(
                held.IsMissingSupportHeld,
                Is.True,
                fixture.FailureContext(scenario: "brief gap", missTick));

            Assert.That(
                fixture.AirTimeSeconds,
                Is.Zero,
                fixture.FailureContext(scenario: "brief gap", missTick));
        }

        fixture.ShowSurface();
        var recovered = fixture.Tick();

        Assert.That(recovered.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Supported));
        Assert.That(recovered.StableSupport.IsGrounded, Is.True);
        Assert.That(recovered.Transition, Is.EqualTo(RunSurfaceTransition.None));
        Assert.That(fixture.AirTimeSeconds, Is.Zero);
    }

    [TestCase(arg1: 0.01f, arg2: 6, arg3: false)]
    [TestCase(arg1: 0.02f, arg2: 3, arg3: false)]
    [TestCase(arg1: 0.01f, arg2: 6, arg3: true)]
    [TestCase(arg1: 0.02f, arg2: 3, arg3: true)]
    public void given_SustainedWalkOffOrJump_when_PipelineAndAirTimeTick_then_RewardStartsAtLossAndStopsOnLanding(
        float fixedDeltaTime,
        int supportLossTick,
        bool moveBodyForJump)
    {
        var fixture = CreateFixture(fixedDeltaTime);
        fixture.ShowSurface();
        fixture.Tick();

        if (moveBodyForJump)
            fixture.RaiseBodyBeyondProbe();
        else
            fixture.HideSurface();

        for (var missTick = 1; missTick < supportLossTick; missTick += 1)
        {
            var held = fixture.Tick();

            Assert.That(
                held.StableSupport.IsGrounded,
                Is.True,
                fixture.FailureContext(moveBodyForJump ? "jump" : "walk-off", missTick));

            Assert.That(
                fixture.AirTimeSeconds,
                Is.Zero,
                fixture.FailureContext(moveBodyForJump ? "jump" : "walk-off", missTick));
        }

        var lost = fixture.Tick();

        Assert.That(lost.Transition, Is.EqualTo(RunSurfaceTransition.SupportLost));
        Assert.That(lost.StableSupport.IsGrounded, Is.False);
        Assert.That(fixture.AirTimeSeconds, Is.EqualTo(fixedDeltaTime).Within(amount: 0.000001f));

        var airborne = fixture.Tick();
        var expectedAirTime = fixedDeltaTime * 2f;

        Assert.That(airborne.Transition, Is.EqualTo(RunSurfaceTransition.None));
        Assert.That(airborne.StableSupport.IsGrounded, Is.False);
        Assert.That(fixture.AirTimeSeconds, Is.EqualTo(expectedAirTime).Within(amount: 0.000001f));

        if (moveBodyForJump)
            fixture.LandBody();
        else
            fixture.ShowSurface();

        var landed = fixture.Tick();

        Assert.That(landed.Transition, Is.EqualTo(RunSurfaceTransition.SupportAcquired));
        Assert.That(landed.StableSupport.IsGrounded, Is.True);
        Assert.That(fixture.AirTimeSeconds, Is.EqualTo(expectedAirTime).Within(amount: 0.000001f));

        var grounded = fixture.Tick();

        Assert.That(grounded.Transition, Is.EqualTo(RunSurfaceTransition.None));
        Assert.That(fixture.AirTimeSeconds, Is.EqualTo(expectedAirTime).Within(amount: 0.000001f));
        AssertAirTimeReward(fixture.AirTimeSeconds, expectedAirTime);
    }

    [Test]
    public void given_UnavailableProgressFrame_when_PipelineAndAirTimeTick_then_HardResetCannotAwardAirTime()
    {
        var fixture = CreateFixture(fixedDeltaTime: 0.02f);
        fixture.ShowSurface();
        fixture.Tick();

        fixture.IsProgressFrameAvailable = false;

        var reset = fixture.Tick();
        var repeatedReset = fixture.Tick();

        Assert.That(reset.ObservedSupport.State, Is.EqualTo(RunSupportObservationState.Unavailable));
        Assert.That(reset.Transition, Is.EqualTo(RunSurfaceTransition.HardReset));
        Assert.That(repeatedReset.Transition, Is.EqualTo(RunSurfaceTransition.HardReset));
        Assert.That(fixture.AirTimeSeconds, Is.Zero);

        fixture.IsProgressFrameAvailable = true;
        var reacquired = fixture.Tick();

        Assert.That(reacquired.Transition, Is.EqualTo(RunSurfaceTransition.SupportAcquired));
        Assert.That(fixture.AirTimeSeconds, Is.Zero);
    }

    private SurfaceAirTimeFixture CreateFixture(float fixedDeltaTime)
    {
        var body = CreateGameObject(name: "Run Surface Air Time Body");
        body.transform.position = _testOrigin + Vector3.up * 0.4f;
        var supportCollider = body.AddComponent<SphereCollider>();
        supportCollider.radius = 0.05f;

        var surface = CreateGameObject(name: "Run Surface Air Time Ground");
        surface.layer = GetSingleLayer(TestAssets.RunSurfaceLayerMask, description: "Run Surface");
        surface.transform.position = _testOrigin;
        surface.transform.localScale = new Vector3(x: 4f, y: 0.1f, z: 4f);
        surface.AddComponent<BoxCollider>();
        surface.AddComponent<RunContact>().SetCategoryForTests(RunContactCategory.Surface);
        surface.SetActive(value: false);

        var time = new FakeTime { FixedDeltaTime = fixedDeltaTime };
        var progressFrameSource = new FakeRunProgressFrameSource();
        var slopeCalculator = new RunSurfaceSlopeCalculator();

        var pipeline = new RunSurfaceFramePipeline(
            progressFrameSource,
            new PhysicsRunSupportProbe(
                supportCollider,
                new RunSupportColliderProbeFactory(),
                new RunSurfaceProbeConfig(
                    distance: 0.4f,
                    skinWidth: 0.02f,
                    TestAssets.RunSurfaceLayerMask,
                    minimumSupportNormalDot: 0.17f,
                    footprintSampleOffsetScale: 0.6f,
                    footprintNormalClusterAngleDegrees: 8f),
                slopeCalculator),
            new SurfaceAirTimeRunMotionSource(body.transform),
            new RunSupportAttachmentPolicy(
                new RunSupportAttachmentConfig(
                    maximumAttachedSurfaceNormalLiftSpeed: 0.35f,
                    sameSurfaceReattachmentSeparationMeters: 0.08f,
                    minimumReattachmentNormalChangeDegrees: 30f,
                    transitionConfirmationSeconds: 0.04f)),
            new RunSurfaceStabilityPolicy(
                new RunSurfaceStabilityConfig(
                    supportLossConfirmationSeconds: 0.06f,
                    discontinuousNormalThresholdDegrees: 45f,
                    discontinuousNormalConfirmationSeconds: 0.04f,
                    candidateCoherenceDegrees: 8f),
                slopeCalculator),
            new RunSteeringFramePolicy(new RunSteeringFrameConfig(normalSlewDegreesPerSecond: 180f, airborneUpRetentionSeconds: 0.08f)),
            time);

        ((IRunSteeringFrameResetter)pipeline).Reset(Vector3.up);

        var runPreparationStateId = CreateStateId(stateName: "Run Preparation");
        var runningStateId = CreateStateId(stateName: "Running");
        var stateService = new RunBodyContactPhysicsStateService(runningStateId);

        var tracker = new RunAirTimeTracker(
            stateService,
            pipeline,
            time,
            runPreparationStateId,
            runningStateId);

        ((IInitializable)tracker).Initialize();
        _trackers.Add(tracker);

        return new SurfaceAirTimeFixture(
            body,
            surface,
            progressFrameSource,
            pipeline,
            tracker);
    }

    private void AssertAirTimeReward(float actualAirTime, float expectedAirTime)
    {
        var coinDefinition = ScriptableObject.CreateInstance<CurrencyDefinition>();
        _createdObjects.Add(coinDefinition);

        var contributor = new AirTimeBonusRunRewardContributor(
            coinDefinition,
            new RunRewardSourceCatalog(),
            new FixedRunRewardConfig());

        var amounts = contributor.CreateSourceAmounts(
            new RunRewardContributorContext(
                RunEndReason.Finished,
                elapsedTime: 1f,
                distanceTravelled: 0f,
                Vector3.zero,
                finalSpeed: 0f,
                actualAirTime));

        Assert.That(amounts.Count, Is.EqualTo(expected: 1));

        Assert.That(
            amounts[index: 0].Amount,
            Is.EqualTo(Mathf.FloorToInt(expectedAirTime * FixedRunRewardConfig.CoinsPerSecond)));
    }

    private GameplayStateId CreateStateId(string stateName)
    {
        var stateId = ScriptableObject.CreateInstance<GameplayStateId>();
        stateId.name = stateName;
        _createdObjects.Add(stateId);
        return stateId;
    }

    private int GetSingleLayer(LayerMask layerMask, string description)
    {
        Assert.That(layerMask.value, Is.Not.EqualTo(expected: 0), description);
        Assert.That(layerMask.value & (layerMask.value - 1), Is.Zero, description);
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, p: 2f));
    }

    private GameObject CreateGameObject(string name)
    {
        var gameObject = new GameObject(name);
        _createdObjects.Add(gameObject);
        return gameObject;
    }

    private sealed class SurfaceAirTimeFixture
    {
        private readonly GameObject _body;
        private readonly Vector3 _landedBodyPosition;
        private readonly RunSurfaceFramePipeline _pipeline;
        private readonly FakeRunProgressFrameSource _progressFrameSource;
        private readonly GameObject _surface;
        private readonly RunAirTimeTracker _tracker;
        private int _tick;

        public float AirTimeSeconds => _tracker.CurrentRunAirTimeSeconds;

        public bool IsProgressFrameAvailable
        {
            set => _progressFrameSource.IsAvailable = value;
        }

        public SurfaceAirTimeFixture(
            GameObject body,
            GameObject surface,
            FakeRunProgressFrameSource progressFrameSource,
            RunSurfaceFramePipeline pipeline,
            RunAirTimeTracker tracker)
        {
            _body = body;
            _surface = surface;
            _landedBodyPosition = body.transform.position;
            _progressFrameSource = progressFrameSource;
            _pipeline = pipeline;
            _tracker = tracker;
        }

        public void ShowSurface()
        {
            _surface.SetActive(value: true);
        }

        public void HideSurface()
        {
            _surface.SetActive(value: false);
        }

        public void RaiseBodyBeyondProbe()
        {
            _body.transform.position = _landedBodyPosition + Vector3.up;
        }

        public void LandBody()
        {
            _body.transform.position = _landedBodyPosition;
        }

        public RunSurfaceFrameSnapshot Tick()
        {
            _tick += 1;
            Physics.SyncTransforms();
            ((IRunSurfaceFrameFixedStep)_pipeline).UpdateSurfaceFrame();
            _tracker.UpdateAirTime();
            return _pipeline.Current;
        }

        public string FailureContext(string scenario, int scenarioTick)
        {
            var snapshot = _pipeline.Current;

            return $"trace tick {_tick}, {scenario} tick {scenarioTick}: "
                   + $"observed={snapshot.ObservedSupport.State}, "
                   + $"stable={snapshot.StableSupport.IsGrounded}, "
                   + $"transition={snapshot.Transition}, "
                   + $"airTime={AirTimeSeconds:0.000}";
        }
    }

    private sealed class FakeRunProgressFrameSource : IRunProgressFrameSource
    {
        public bool IsAvailable { get; set; } = true;

        public bool TryCreateSnapshot(
            Vector3 origin,
            out RunProgressFrameSnapshot snapshot,
            out string error)
        {
            if (!IsAvailable)
            {
                snapshot = default;
                error = "Run Surface air-time progress frame unavailable.";
                return false;
            }

            return RunProgressFrameSnapshot.TryCreate(
                origin,
                Vector3.forward,
                Vector3.up,
                out snapshot,
                out error);
        }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }

    private sealed class SurfaceAirTimeRunMotionSource : IRunMotionSource
    {
        private readonly Transform _transform;
        public Vector3 LinearVelocity => Vector3.zero;

        public Vector3 Position => _transform.position;

        public SurfaceAirTimeRunMotionSource(Transform transform)
        {
            _transform = transform;
        }
    }

    private sealed class FixedRunRewardConfig : IRunRewardConfig
    {
        public const float CoinsPerSecond = 100f;
        public float AirTimeBonusCoinsPerSecond => CoinsPerSecond;

        public float DistanceBonusCoinsPerMeter => 0f;
    }
}
