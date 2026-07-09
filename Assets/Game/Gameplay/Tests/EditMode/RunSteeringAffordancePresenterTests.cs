using System.Collections.Generic;
using Game.Foundation.Time;
using Game.Gameplay;
using NUnit.Framework;
using UnityEngine;
using VContainer.Unity;

// ReSharper disable once CheckNamespace
public sealed class RunSteeringAffordancePresenterTests
{
    private const float HiddenScale = 0.86f;

    [Test]
    public void Initialize_Always_ResetsViewImmediately()
    {
        var fixture = CreateFixture();

        ((IInitializable)fixture.Presenter).Initialize();

        Assert.That(fixture.View.AnimationFrames, Has.Count.EqualTo(1));
        AssertAnimationFrame(fixture.View.AnimationFrames[0], 0f, HiddenScale);
        Assert.That(fixture.View.DeactivateCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Show_VisibleState_PresentsAndStartsScaleAnimation()
    {
        var fixture = CreateFixture(showSeconds: 0.2f);
        InitializeAndClear(fixture);

        ((IRunSteeringAffordanceView)fixture.Presenter).Show(CreateState(knobX: 120f));

        Assert.That(fixture.View.PresentedStates, Has.Count.EqualTo(1));
        Assert.That(fixture.View.PresentedStates[0].KnobScreenPosition.x, Is.EqualTo(120f));
        Assert.That(fixture.View.AnimationFrames, Has.Count.EqualTo(1));
        AssertAnimationFrame(fixture.View.AnimationFrames[0], 1f, HiddenScale);
        Assert.That(fixture.View.DeactivateCallCount, Is.Zero);
    }

    [Test]
    public void Tick_ShowAnimation_UsesScaledDeltaTimeToReachVisibleScale()
    {
        var fixture = CreateFixture(showSeconds: 0.2f);
        InitializeAndClear(fixture);
        ((IRunSteeringAffordanceView)fixture.Presenter).Show(CreateState());

        fixture.Time.DeltaTime = 0.1f;
        ((ITickable)fixture.Presenter).Tick();

        AssertAnimationFrame(fixture.View.AnimationFrames[^1], 1f, 0.93f);

        ((ITickable)fixture.Presenter).Tick();

        AssertAnimationFrame(fixture.View.AnimationFrames[^1], 1f, 1f);
        Assert.That(fixture.View.DeactivateCallCount, Is.Zero);
    }

    [Test]
    public void Hide_VisibleState_FadesAndDeactivatesOnlyAfterDuration()
    {
        var fixture = CreateFixture(hideSeconds: 0.2f);
        InitializeAndClear(fixture);
        ((IRunSteeringAffordanceView)fixture.Presenter).Show(CreateState());
        fixture.View.Clear();

        ((IRunSteeringAffordanceView)fixture.Presenter).Hide(CreateState(knobX: 180f));

        Assert.That(fixture.View.PresentedStates, Has.Count.EqualTo(1));
        Assert.That(fixture.View.PresentedStates[0].KnobScreenPosition.x, Is.EqualTo(180f));
        AssertAnimationFrame(fixture.View.AnimationFrames[^1], 1f, 1f);
        Assert.That(fixture.View.DeactivateCallCount, Is.Zero);

        fixture.Time.DeltaTime = 0.1f;
        ((ITickable)fixture.Presenter).Tick();

        AssertAnimationFrame(fixture.View.AnimationFrames[^1], 0.5f, 0.93f);
        Assert.That(fixture.View.DeactivateCallCount, Is.Zero);

        ((ITickable)fixture.Presenter).Tick();

        AssertAnimationFrame(fixture.View.AnimationFrames[^1], 0f, HiddenScale);
        Assert.That(fixture.View.DeactivateCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Reset_HideAnimationInProgress_DeactivatesImmediatelyAndStopsAnimation()
    {
        var fixture = CreateFixture(hideSeconds: 0.2f);
        InitializeAndClear(fixture);
        ((IRunSteeringAffordanceView)fixture.Presenter).Show(CreateState());
        ((IRunSteeringAffordanceView)fixture.Presenter).Hide(CreateState());

        fixture.Time.DeltaTime = 0.1f;
        ((ITickable)fixture.Presenter).Tick();
        fixture.View.Clear();

        ((IRunSteeringAffordanceView)fixture.Presenter).Reset();

        Assert.That(fixture.View.AnimationFrames, Has.Count.EqualTo(1));
        AssertAnimationFrame(fixture.View.AnimationFrames[0], 0f, HiddenScale);
        Assert.That(fixture.View.DeactivateCallCount, Is.EqualTo(1));

        ((ITickable)fixture.Presenter).Tick();

        Assert.That(fixture.View.AnimationFrames, Has.Count.EqualTo(1));
        Assert.That(fixture.View.DeactivateCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Show_HideAnimationInProgress_InterruptsHideAndKeepsViewActive()
    {
        var fixture = CreateFixture(showSeconds: 0.1f, hideSeconds: 0.2f);
        InitializeAndClear(fixture);
        ((IRunSteeringAffordanceView)fixture.Presenter).Show(CreateState());
        ((IRunSteeringAffordanceView)fixture.Presenter).Hide(CreateState());

        fixture.Time.DeltaTime = 0.1f;
        ((ITickable)fixture.Presenter).Tick();
        fixture.View.Clear();

        ((IRunSteeringAffordanceView)fixture.Presenter).Show(CreateState(knobX: 140f));
        ((ITickable)fixture.Presenter).Tick();

        Assert.That(fixture.View.PresentedStates, Has.Count.EqualTo(1));
        AssertAnimationFrame(fixture.View.AnimationFrames[^1], 1f, 1f);
        Assert.That(fixture.View.DeactivateCallCount, Is.Zero);
    }

    [Test]
    public void Update_ShowAnimationInProgress_PresentsWithoutRestartingAnimation()
    {
        var fixture = CreateFixture(showSeconds: 0.2f);
        InitializeAndClear(fixture);
        ((IRunSteeringAffordanceView)fixture.Presenter).Show(CreateState());

        fixture.Time.DeltaTime = 0.1f;
        ((ITickable)fixture.Presenter).Tick();
        ((IRunSteeringAffordanceView)fixture.Presenter).Update(CreateState(knobX: 160f));
        ((ITickable)fixture.Presenter).Tick();

        Assert.That(fixture.View.PresentedStates[^1].KnobScreenPosition.x, Is.EqualTo(160f));
        AssertAnimationFrame(fixture.View.AnimationFrames[^1], 1f, 1f);
    }

    private static Fixture CreateFixture(float showSeconds = 0f, float hideSeconds = 0.08f)
    {
        var view = new FakePresentationView();
        var tuning = new FakeTuning(HiddenScale, showSeconds, hideSeconds);
        var time = new FakeTime();
        var presenter = new RunSteeringAffordancePresenter(view, tuning, time);
        return new Fixture(presenter, view, time);
    }

    private static void InitializeAndClear(Fixture fixture)
    {
        ((IInitializable)fixture.Presenter).Initialize();
        fixture.View.Clear();
    }

    private static RunSteeringAffordancePresentationState CreateState(float knobX = 100f)
    {
        return new RunSteeringAffordancePresentationState(
            isVisible: true,
            originScreenPosition: new Vector2(100f, 200f),
            knobScreenPosition: new Vector2(knobX, 200f),
            leftRangeEndScreenPosition: new Vector2(20f, 200f),
            rightRangeEndScreenPosition: new Vector2(180f, 200f),
            deadzoneDiameterPixels: 40f);
    }

    private static void AssertAnimationFrame(AnimationFrame frame, float expectedAlpha, float expectedScale)
    {
        Assert.That(frame.Alpha, Is.EqualTo(expectedAlpha).Within(0.001f));
        Assert.That(frame.Scale, Is.EqualTo(expectedScale).Within(0.001f));
    }

    private readonly struct Fixture
    {
        public RunSteeringAffordancePresenter Presenter { get; }
        public FakePresentationView View { get; }
        public FakeTime Time { get; }

        public Fixture(RunSteeringAffordancePresenter presenter, FakePresentationView view, FakeTime time)
        {
            Presenter = presenter;
            View = view;
            Time = time;
        }
    }

    private readonly struct AnimationFrame
    {
        public float Alpha { get; }
        public float Scale { get; }

        public AnimationFrame(float alpha, float scale)
        {
            Alpha = alpha;
            Scale = scale;
        }
    }

    private sealed class FakePresentationView : IRunSteeringAffordancePresentationView
    {
        public List<RunSteeringAffordancePresentationState> PresentedStates { get; } = new();
        public List<AnimationFrame> AnimationFrames { get; } = new();
        public int DeactivateCallCount { get; private set; }

        public void Present(RunSteeringAffordancePresentationState state)
        {
            PresentedStates.Add(state);
        }

        public void ApplyAnimation(float alpha, float scale)
        {
            AnimationFrames.Add(new AnimationFrame(alpha, scale));
        }

        public void Deactivate()
        {
            DeactivateCallCount += 1;
        }

        public void Clear()
        {
            PresentedStates.Clear();
            AnimationFrames.Clear();
            DeactivateCallCount = 0;
        }
    }

    private sealed class FakeTuning : IRunSteeringAffordanceTuning
    {
        public float HiddenScale { get; }
        public float ShowDurationSeconds { get; }
        public float HideDurationSeconds { get; }

        public FakeTuning(float hiddenScale, float showDurationSeconds, float hideDurationSeconds)
        {
            HiddenScale = hiddenScale;
            ShowDurationSeconds = showDurationSeconds;
            HideDurationSeconds = hideDurationSeconds;
        }
    }

    private sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime => DeltaTime;
    }
}
