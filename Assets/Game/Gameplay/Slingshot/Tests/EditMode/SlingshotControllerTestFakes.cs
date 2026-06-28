using System;
using System.Collections.Generic;
using Game.Foundation.Input;
using Game.Foundation.Time;
using Game.Utils.Invocation;
using UnityEngine;

namespace Game.Gameplay.Slingshot.Tests.EditMode
{
    internal sealed class FakeSlingshotConfig : ISlingshotConfig
    {
        public float TouchTargetRadiusPixels { get; set; }
        public float MinimumPullDistance { get; set; }
        public float MaximumPullDistance { get; set; }
        public float MaximumLateralPull { get; set; }
        public float MaximumLaunchAngleDegrees { get; set; }
        public float MinimumLaunchSpeed { get; set; }
        public float MaximumLaunchSpeed { get; set; }
        public AnimationCurve LaunchSpeedCurve { get; set; }
        public float LaunchUpSpeed { get; set; }
        public float BandContactPadding { get; set; }
        public int BandSilhouetteSampleCount { get; set; }
        public int BandWrapSampleCount { get; set; }
        public float BandRecoilDuration { get; set; }
        public AnimationCurve BandRecoilCurve { get; set; }
    }

    internal sealed class FakeUnityInput : IUnityInput
    {
        private readonly List<string> _observations;

        public int ActiveHandleCount { get; private set; }

        public event Action<PointerInput> PointerPressed;
        public event Action<PointerInput> PointerMoved;
        public event Action<PointerInput> PointerReleased;
        public event Action<PointerInput> PointerCanceled;

        public FakeUnityInput(List<string> observations)
        {
            _observations = observations;
        }

        public IDisposable Enable()
        {
            ActiveHandleCount += 1;
            _observations.Add("input-enable");
            return new EnableHandle(this);
        }

        public void Press(int pointerId, Vector2 screenPosition)
        {
            PointerPressed?.InvokeSafely(new PointerInput(pointerId, screenPosition));
        }

        public void Move(int pointerId, Vector2 screenPosition)
        {
            PointerMoved?.InvokeSafely(new PointerInput(pointerId, screenPosition));
        }

        public void Release(int pointerId, Vector2 screenPosition)
        {
            PointerReleased?.InvokeSafely(new PointerInput(pointerId, screenPosition));
        }

        public void Cancel(int pointerId, Vector2 screenPosition)
        {
            PointerCanceled?.InvokeSafely(new PointerInput(pointerId, screenPosition));
        }

        private void ReleaseHandle()
        {
            ActiveHandleCount -= 1;
            _observations.Add("input-disable");
        }

        private sealed class EnableHandle : IDisposable
        {
            private FakeUnityInput _owner;

            public EnableHandle(FakeUnityInput owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                var owner = _owner;

                if (owner is null)
                    return;

                _owner = null;
                owner.ReleaseHandle();
            }
        }
    }

    internal sealed class FakeSlingshotView : ISlingshotView
    {
        private readonly List<string> _observations;

        public SlingshotGeometrySnapshot Geometry { get; }
        public SlingshotBandShape LastBandShape { get; private set; }
        public SlingshotPullVisual LastActivePullVisual { get; private set; }
        public List<SlingshotPullVisual> ActivePullVisuals { get; } = new();

        public FakeSlingshotView(List<string> observations)
        {
            _observations = observations;

            Geometry = new SlingshotGeometrySnapshot(
                new Vector3(-1f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                Vector3.zero,
                Vector3.right,
                Vector3.forward,
                Vector3.up);
        }

        public SlingshotGeometrySnapshot CreateGeometrySnapshot()
        {
            return Geometry;
        }

        public void ShowInactiveIdle(SlingshotBandShape bandShape)
        {
            LastBandShape = bandShape;
            _observations.Add("view-inactive-idle");
        }

        public void ShowCaptureIdle(SlingshotBandShape bandShape)
        {
            LastBandShape = bandShape;
            _observations.Add("view-capture-idle");
        }

        public void ShowLoadedRelease(SlingshotBandShape bandShape)
        {
            LastBandShape = bandShape;
            _observations.Add("view-loaded-release");
        }

        public void ShowActivePull(SlingshotPullVisual pullVisual)
        {
            LastActivePullVisual = pullVisual;
            LastBandShape = pullVisual.BandShape;
            ActivePullVisuals.Add(pullVisual);
            _observations.Add("view-active-pull");
        }
    }

    internal sealed class FakeHeldLaunchTarget : IHeldLaunchTarget
    {
        private readonly List<string> _observations;

        public List<Vector3> HeldPositions { get; } = new();

        public FakeHeldLaunchTarget(List<string> observations)
        {
            _observations = observations;
        }

        public void SetHeldPosition(Vector3 heldPosition)
        {
            HeldPositions.Add(heldPosition);
            _observations.Add("target-position");
        }
    }

    internal sealed class FakeLaunchTarget : ILaunchTarget
    {
        private readonly List<string> _observations;

        public int HoldCallCount { get; private set; }
        public List<Vector3> LaunchVelocities { get; } = new();

        public FakeLaunchTarget(List<string> observations)
        {
            _observations = observations;
        }

        public void Hold()
        {
            HoldCallCount += 1;
            _observations.Add("target-hold");
        }

        public void Launch(Vector3 velocity)
        {
            LaunchVelocities.Add(velocity);
            _observations.Add("target-launch");
        }
    }

    internal sealed class FakeSlingshotBandShapeProvider : ISlingshotBandShapeProvider
    {
        private readonly List<string> _observations;

        public int BandShapePointCount => ShapePoints.Length;

        public List<SlingshotBandShapeQuery> Queries { get; } = new();
        public bool ShouldFail { get; set; }
        public bool ShouldFailActivePullOnly { get; set; }

        public Vector3[] ShapePoints { get; set; } =
        {
            new(-1f, 0f, 0f),
            new(-0.75f, 0f, -0.1f),
            new(-0.5f, 0f, -0.35f),
            new(-0.25f, 0f, -0.55f),
            new(0f, 0f, -0.7f),
            new(0.25f, 0f, -0.55f),
            new(0.5f, 0f, -0.35f),
            new(0.75f, 0f, -0.1f),
            new(1f, 0f, 0f)
        };

        public FakeSlingshotBandShapeProvider(List<string> observations)
        {
            _observations = observations;
        }

        public bool TryCreateBandShape(SlingshotBandShapeQuery query, Vector3[] outputPoints, out int pointCount)
        {
            Queries.Add(query);
            _observations.Add("band-shape");

            if (ShouldFail && (!ShouldFailActivePullOnly || query.PullPoint != query.RestPoint))
            {
                pointCount = 0;
                return false;
            }

            if (outputPoints is null)
                throw new ArgumentNullException(nameof(outputPoints));

            if (outputPoints.Length < ShapePoints.Length)
                throw new ArgumentException("Output buffer is too small.", nameof(outputPoints));

            for (var pointIndex = 0; pointIndex < ShapePoints.Length; pointIndex += 1)
            {
                outputPoints[pointIndex] = ShapePoints[pointIndex];
            }

            pointCount = ShapePoints.Length;
            return true;
        }
    }

    internal sealed class FakeSlingshotLaunchAppliedNotifier : ISlingshotLaunchAppliedNotifier
    {
        public event Action<SlingshotLaunchRequest> LaunchApplied;

        public void Apply(SlingshotLaunchRequest request)
        {
            LaunchApplied?.InvokeSafely(request);
        }
    }

    internal sealed class FakeTime : ITime
    {
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
    }

    internal sealed class FakeSlingshotInputProjector : ISlingshotInputProjector
    {
        private readonly List<ScreenWorldProjection> _screenToWorldProjections = new();
        private readonly List<WorldScreenProjection> _worldToScreenProjections = new();

        public void SetScreenToWorld(Vector2 screenPosition, Vector3 worldPosition)
        {
            _screenToWorldProjections.Add(new ScreenWorldProjection(screenPosition, true, worldPosition));
        }

        public void SetScreenProjectionFailure(Vector2 screenPosition)
        {
            _screenToWorldProjections.Add(new ScreenWorldProjection(screenPosition, false, Vector3.zero));
        }

        public void SetWorldToScreen(Vector3 worldPosition, Vector2 screenPosition)
        {
            _worldToScreenProjections.Add(new WorldScreenProjection(worldPosition, true, screenPosition));
        }

        public bool TryProjectScreenToPullPlane(Vector2 screenPosition, SlingshotGeometrySnapshot geometry, out Vector3 worldPosition)
        {
            for (var i = _screenToWorldProjections.Count - 1; i >= 0; i -= 1)
            {
                var projection = _screenToWorldProjections[i];

                if (projection.ScreenPosition != screenPosition)
                    continue;

                worldPosition = projection.WorldPosition;
                return projection.Succeeded;
            }

            worldPosition = geometry.RestPoint;
            return true;
        }

        public bool TryProjectWorldToScreen(Vector3 worldPosition, out Vector2 screenPosition)
        {
            for (var i = _worldToScreenProjections.Count - 1; i >= 0; i -= 1)
            {
                var projection = _worldToScreenProjections[i];

                if (projection.WorldPosition != worldPosition)
                    continue;

                screenPosition = projection.ScreenPosition;
                return projection.Succeeded;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        private readonly struct ScreenWorldProjection
        {
            public Vector2 ScreenPosition { get; }
            public bool Succeeded { get; }
            public Vector3 WorldPosition { get; }

            public ScreenWorldProjection(Vector2 screenPosition, bool succeeded, Vector3 worldPosition)
            {
                ScreenPosition = screenPosition;
                Succeeded = succeeded;
                WorldPosition = worldPosition;
            }
        }

        private readonly struct WorldScreenProjection
        {
            public Vector3 WorldPosition { get; }
            public bool Succeeded { get; }
            public Vector2 ScreenPosition { get; }

            public WorldScreenProjection(Vector3 worldPosition, bool succeeded, Vector2 screenPosition)
            {
                WorldPosition = worldPosition;
                Succeeded = succeeded;
                ScreenPosition = screenPosition;
            }
        }
    }
}
