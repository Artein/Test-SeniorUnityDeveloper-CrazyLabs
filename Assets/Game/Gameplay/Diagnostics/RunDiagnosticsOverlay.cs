using Game.Gameplay.CharacterPresentation;
using Game.Utils.Mathematics;
using UnityEngine;
using VContainer;

namespace Game.Gameplay.Diagnostics
{
    public sealed class RunDiagnosticsOverlay : MonoBehaviour
    {
        [SerializeField] private bool _visible = true;
        [SerializeField] private int _sampleCapacity = 180;
        [SerializeField] private int _titleFontSize = 18;
        [SerializeField] private int _labelFontSize = 14;
        [SerializeField] private float _speedScaleMetersPerSecond = 80f;
        [SerializeField] private float _normalDeltaScaleDegrees = 25f;
        [SerializeField] private float _visualLagScaleCentimeters = 15f;

        private readonly RunDiagnosticsOverlayLayout _layout = new();
        private readonly RunDiagnosticsOverlayMath _math = new();
        private readonly RunDiagnosticsOverlaySnapEstimator _snapEstimator = new();
        private IRunCameraAnchor _cameraAnchor;
        private GUIStyle _diagnosticStyle;
        private int _fixedStepsSinceLastSample;
        private bool _hasPreviousCameraPosition;
        private bool _hasPreviousCameraRotation;
        private bool _hasPreviousMotionPosition;
        private bool _hasPreviousObservedGroundNormal;
        private bool _hasPreviousSteeringUp;
        private bool _hasPreviousVisualRotation;
        private bool _hasPreviousVisualTargetPosition;
        private bool _hasPreviousVisualTargetRotation;
        private GUIStyle _labelStyle;

        private IRunMotionSource _motionSource;
        private Texture2D _pixelTexture;
        private Vector3 _previousCameraPosition;
        private Quaternion _previousCameraRotation = Quaternion.identity;

        private Vector3 _previousMotionPosition;
        private Vector3 _previousObservedGroundNormal;
        private Vector3 _previousSteeringUp;
        private Quaternion _previousVisualRotation = Quaternion.identity;
        private Vector3 _previousVisualTargetPosition;
        private Quaternion _previousVisualTargetRotation = Quaternion.identity;

        private RunDiagnosticsOverlayBuffer _samples;
        private GUIStyle _smallLabelStyle;
        private IRunBodySpeedDiagnosticsSource _speedDiagnosticsSource;
        private IRunSurfaceFrameSource _surfaceFrameSource;
        private GUIStyle _titleStyle;
        private ICharacterVisualFollowTuning _visualFollowTuning;
        private ICharacterVisualFollowView _visualFollowView;
        private ICharacterVisualTargetPoseSource _visualTargetPoseSource;

        private bool HasDependencies =>
            _motionSource != null
            && _surfaceFrameSource != null
            && _visualTargetPoseSource != null
            && _visualFollowView != null
            && _visualFollowTuning != null
            && _cameraAnchor != null
            && _speedDiagnosticsSource != null;

        private void Awake()
        {
            EnsureSampleBuffer();
            EnsurePixelTexture();
        }

        private void FixedUpdate()
        {
            _fixedStepsSinceLastSample += 1;
        }

        private void LateUpdate()
        {
            if (!_visible || !HasDependencies)
            {
                _fixedStepsSinceLastSample = 0;
                return;
            }

            EnsureSampleBuffer();
            _samples.Add(CreateSample(ResolveDeltaTime()));
        }

        private void OnDestroy()
        {
            if (_pixelTexture != null)
                Destroy(_pixelTexture);
        }

        private void OnGUI()
        {
            if (!_visible)
                return;

            EnsurePixelTexture();
            EnsureStyles();

            var panelRect = GetPanelRect();
            DrawRect(panelRect, new Color(r: 0f, g: 0f, b: 0f, a: 0.72f));

            if (!HasDependencies)
            {
                DrawText(
                    new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, height: 28f),
                    text: "RUN DIAG waiting for dependencies",
                    Color.white,
                    _titleStyle);

                return;
            }

            if (_samples is not { Count: > 0 })
            {
                DrawText(
                    new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, height: 28f),
                    text: "RUN DIAG waiting for samples",
                    Color.white,
                    _titleStyle);

                return;
            }

            DrawOverlay(panelRect, _samples.Latest);
        }

        [Inject]
        private void Construct(
            IRunMotionSource motionSource,
            IRunSurfaceFrameSource surfaceFrameSource,
            ICharacterVisualTargetPoseSource visualTargetPoseSource,
            ICharacterVisualFollowView visualFollowView,
            ICharacterVisualFollowTuning visualFollowTuning,
            IRunCameraAnchor cameraAnchor,
            IRunBodySpeedDiagnosticsSource speedDiagnosticsSource)
        {
            _motionSource = motionSource;
            _surfaceFrameSource = surfaceFrameSource;
            _visualTargetPoseSource = visualTargetPoseSource;
            _visualFollowView = visualFollowView;
            _visualFollowTuning = visualFollowTuning;
            _cameraAnchor = cameraAnchor;
            _speedDiagnosticsSource = speedDiagnosticsSource;
        }

        private RunDiagnosticsOverlaySample CreateSample(float deltaTime)
        {
            var fixedStepsThisFrame = _fixedStepsSinceLastSample;
            _fixedStepsSinceLastSample = 0;
            var motionPosition = _motionSource.Position;
            var motionVelocity = _motionSource.LinearVelocity;
            var targetPose = _visualTargetPoseSource.CurrentPose;
            var visualPose = _visualFollowView.CurrentVisualPose;
            var cameraPosition = _cameraAnchor.Position;
            var cameraRotation = _cameraAnchor.Rotation;
            var surfaceFrame = _surfaceFrameSource.Current;
            var observedSupport = surfaceFrame.ObservedSupport;
            var observedSurfaceContext = observedSupport.SurfaceContext;

            var observedGroundNormal = observedSupport.State == RunSupportObservationState.Supported
                                       && observedSurfaceContext.HasValidGroundNormal
                ? observedSurfaceContext.GroundNormal
                : Vector3.up;

            var steeringUp = surfaceFrame.SteeringFrame.IsValid
                ? surfaceFrame.SteeringFrame.UpDirection
                : observedGroundNormal;

            var speed = motionVelocity.IsFinite() ? motionVelocity.magnitude : 0f;

            var motionStep = _math.CalculateStepSpeed(
                motionPosition,
                ref _previousMotionPosition,
                ref _hasPreviousMotionPosition,
                deltaTime,
                out _);

            var visualTargetStep = _math.CalculateStepSpeed(
                targetPose.Position,
                ref _previousVisualTargetPosition,
                ref _hasPreviousVisualTargetPosition,
                deltaTime,
                out var visualTargetStepMeters);

            var cameraStep = _math.CalculateStepSpeed(
                cameraPosition,
                ref _previousCameraPosition,
                ref _hasPreviousCameraPosition,
                deltaTime,
                out _);

            var observedNormalDelta = _math.CalculateDirectionDelta(
                observedGroundNormal,
                ref _previousObservedGroundNormal,
                ref _hasPreviousObservedGroundNormal);

            var steeringUpDelta = _math.CalculateDirectionDelta(
                steeringUp,
                ref _previousSteeringUp,
                ref _hasPreviousSteeringUp);

            var visualLag = _math.CalculateDistanceCentimeters(targetPose.Position, visualPose.Position);
            var targetToMotion = _math.CalculateDistanceCentimeters(targetPose.Position, motionPosition);

            var targetRotationDelta = _math.CalculateRotationDelta(
                targetPose.Rotation,
                ref _previousVisualTargetRotation,
                ref _hasPreviousVisualTargetRotation);

            var visualRotationDelta = _math.CalculateRotationDelta(
                visualPose.Rotation,
                ref _previousVisualRotation,
                ref _hasPreviousVisualRotation);

            var cameraRotationDelta = _math.CalculateRotationDelta(
                cameraRotation,
                ref _previousCameraRotation,
                ref _hasPreviousCameraRotation);

            var estimatedSnapReason = _snapEstimator.Estimate(
                visualTargetStepMeters,
                targetRotationDelta,
                visualLag,
                _visualFollowTuning);

            return new RunDiagnosticsOverlaySample(
                speed,
                motionStep,
                visualTargetStep,
                visualTargetStepMeters,
                observedNormalDelta,
                steeringUpDelta,
                visualLag,
                cameraStep,
                targetToMotion,
                targetRotationDelta,
                visualRotationDelta,
                cameraRotationDelta,
                estimatedSnapReason,
                fixedStepsThisFrame,
                surfaceFrame,
                _speedDiagnosticsSource.Current);
        }

        private float ResolveDeltaTime()
        {
            var deltaTime = Time.unscaledDeltaTime;
            return float.IsFinite(deltaTime) && deltaTime > 0.0001f ? deltaTime : 0.0001f;
        }

        private void DrawOverlay(Rect panelRect, RunDiagnosticsOverlaySample latest)
        {
            var contentX = panelRect.x + 10f;
            var contentWidth = panelRect.width - 20f;
            var titleRect = new Rect(contentX, panelRect.y + 7f, contentWidth, height: 22f);
            var detailRect = new Rect(contentX, panelRect.y + 29f, contentWidth, height: 18f);
            var speedDiagnosticsRect = new Rect(contentX, panelRect.y + 47f, contentWidth, height: 44f);
            var assistDiagnosticsRect = new Rect(contentX, panelRect.y + 91f, contentWidth, height: 30f);
            var chartTop = panelRect.y + 126f;
            var chartHeight = Mathf.Max(a: 1f, panelRect.height - 134f);
            var rowCount = 10f;
            var rowHeight = Mathf.Max(a: 1f, chartHeight / rowCount);

            DrawText(titleRect, text: "RUN DIAG | high-speed shake source", Color.white, _titleStyle);

            DrawText(
                detailRect,
                RunDiagnosticsOverlayTextFormatter.FormatMotionSummary(latest),
                new Color(r: 0.78f, g: 0.84f, b: 0.88f, a: 1f),
                _smallLabelStyle);

            DrawText(
                speedDiagnosticsRect,
                RunDiagnosticsOverlayTextFormatter.FormatRunBodySpeed(latest.SpeedDiagnostics),
                new Color(r: 0.65f, g: 0.9f, b: 1f, a: 1f),
                _diagnosticStyle);

            DrawText(
                assistDiagnosticsRect,
                RunDiagnosticsOverlayTextFormatter.FormatLowSpeedAssist(latest.SpeedDiagnostics),
                new Color(r: 0.72f, g: 1f, b: 0.72f, a: 1f),
                _diagnosticStyle);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 0f, contentWidth, rowHeight - 2f),
                metricIndex: 0,
                label: "rb v",
                unit: "m/s",
                ResolveScale(_speedScaleMetersPerSecond, fallbackScale: 80f),
                new Color(r: 1f, g: 1f, b: 1f, a: 0.95f),
                latest.SpeedMetersPerSecond);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 1f, contentWidth, rowHeight - 2f),
                metricIndex: 1,
                label: "rb step",
                unit: "m/s",
                ResolveScale(_speedScaleMetersPerSecond, fallbackScale: 80f),
                new Color(r: 1f, g: 0.38f, b: 0.32f, a: 0.95f),
                latest.MotionStepMetersPerSecond);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 2f, contentWidth, rowHeight - 2f),
                metricIndex: 2,
                label: "tgt step",
                unit: "m/s",
                ResolveScale(_speedScaleMetersPerSecond, fallbackScale: 80f),
                new Color(r: 1f, g: 0.58f, b: 0.16f, a: 0.95f),
                latest.VisualTargetStepMetersPerSecond);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 3f, contentWidth, rowHeight - 2f),
                metricIndex: 3,
                label: "obsN d",
                unit: "deg",
                ResolveScale(_normalDeltaScaleDegrees, fallbackScale: 25f),
                new Color(r: 1f, g: 0.88f, b: 0.16f, a: 0.95f),
                latest.ObservedGroundNormalDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 4f, contentWidth, rowHeight - 2f),
                metricIndex: 4,
                label: "steer d",
                unit: "deg",
                ResolveScale(_normalDeltaScaleDegrees, fallbackScale: 25f),
                new Color(r: 0.35f, g: 1f, b: 0.45f, a: 0.95f),
                latest.SteeringUpDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 5f, contentWidth, rowHeight - 2f),
                metricIndex: 5,
                label: "snap",
                unit: "",
                scaleMax: 1f,
                new Color(r: 1f, g: 0.95f, b: 0.22f, a: 0.95f),
                latest.HasEstimatedVisualSnap ? 1f : 0f);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 6f, contentWidth, rowHeight - 2f),
                metricIndex: 6,
                label: "tgt rot",
                unit: "deg",
                ResolveScale(_normalDeltaScaleDegrees, fallbackScale: 25f),
                new Color(r: 0.52f, g: 0.82f, b: 1f, a: 0.95f),
                latest.VisualTargetRotationDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 7f, contentWidth, rowHeight - 2f),
                metricIndex: 7,
                label: "vis rot",
                unit: "deg",
                ResolveScale(_normalDeltaScaleDegrees, fallbackScale: 25f),
                new Color(r: 0.62f, g: 0.65f, b: 1f, a: 0.95f),
                latest.VisualRotationDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 8f, contentWidth, rowHeight - 2f),
                metricIndex: 8,
                label: "vis lag",
                unit: "cm",
                ResolveScale(_visualLagScaleCentimeters, fallbackScale: 15f),
                new Color(r: 0.22f, g: 0.9f, b: 1f, a: 0.95f),
                latest.VisualLagCentimeters);

            DrawMetricRow(
                new Rect(contentX, chartTop + rowHeight * 9f, contentWidth, rowHeight - 2f),
                metricIndex: 9,
                label: "cam step",
                unit: "m/s",
                ResolveScale(_speedScaleMetersPerSecond, fallbackScale: 80f),
                new Color(r: 1f, g: 0.35f, b: 1f, a: 0.95f),
                latest.CameraStepMetersPerSecond);
        }

        private void DrawMetricRow(
            Rect rowRect,
            int metricIndex,
            string label,
            string unit,
            float scaleMax,
            Color color,
            float currentValue)
        {
            var labelWidth = Mathf.Min(a: 122f, rowRect.width * 0.34f);
            var labelRect = new Rect(rowRect.x, rowRect.y + 1f, labelWidth, rowRect.height - 2f);

            var chartRect = new Rect(
                rowRect.x + labelWidth + 6f,
                rowRect.y + 3f,
                Mathf.Max(a: 1f, rowRect.width - labelWidth - 6f),
                Mathf.Max(a: 1f, rowRect.height - 6f));

            DrawText(labelRect, $"{label} {currentValue:0.0}{unit}", color, _labelStyle);
            DrawRect(chartRect, new Color(r: 1f, g: 1f, b: 1f, a: 0.08f));
            DrawBars(chartRect, metricIndex, scaleMax, color);
        }

        private void DrawBars(Rect chartRect, int metricIndex, float scaleMax, Color color)
        {
            if (_samples is not { Count: > 0 })
                return;

            var sampleCount = _samples.Count;
            var barWidth = Mathf.Max(a: 1f, chartRect.width / Mathf.Max(a: 1, _samples.Capacity));

            for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex += 1)
            {
                var sample = _samples.GetChronological(sampleIndex);
                var normalizedValue = Mathf.Clamp01(sample.SelectMetric(metricIndex) / scaleMax);
                var barHeight = Mathf.Max(a: 1f, normalizedValue * chartRect.height);

                var barRect = new Rect(
                    chartRect.x + sampleIndex * barWidth,
                    chartRect.yMax - barHeight,
                    Mathf.Max(a: 1f, barWidth - 1f),
                    barHeight);

                DrawRect(barRect, color);
            }
        }

        private Rect GetPanelRect()
        {
            return _layout.CreatePanelRect(Screen.width, Screen.height);
        }

        private float ResolveScale(float authoredScale, float fallbackScale)
        {
            return float.IsFinite(authoredScale) && authoredScale > 0.0001f ? authoredScale : fallbackScale;
        }

        private void EnsureSampleBuffer()
        {
            var resolvedCapacity = Mathf.Clamp(_sampleCapacity, min: 32, max: 600);

            if (_samples is null || _samples.Capacity != resolvedCapacity)
                _samples = new RunDiagnosticsOverlayBuffer(resolvedCapacity);
        }

        private void EnsurePixelTexture()
        {
            if (_pixelTexture != null)
                return;

            _pixelTexture = new Texture2D(width: 1, height: 1, TextureFormat.RGBA32, mipChain: false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _pixelTexture.SetPixel(x: 0, y: 0, Color.white);
            _pixelTexture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        }

        private void EnsureStyles()
        {
            _titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(a: 10, _titleFontSize),
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip
            };

            _labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(a: 9, _labelFontSize),
                clipping = TextClipping.Clip
            };

            _smallLabelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(a: 8, _labelFontSize - 2),
                clipping = TextClipping.Clip
            };

            _diagnosticStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(a: 8, _labelFontSize - 3),
                wordWrap = true,
                clipping = TextClipping.Clip
            };
        }

        private void DrawRect(Rect rect, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _pixelTexture);
            GUI.color = previousColor;
        }

        private void DrawText(Rect rect, string text, Color color, GUIStyle style)
        {
            var previousColor = style.normal.textColor;
            style.normal.textColor = color;
            GUI.Label(rect, text, style);
            style.normal.textColor = previousColor;
        }
    }
}
