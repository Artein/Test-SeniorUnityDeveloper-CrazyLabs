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
        private readonly RunDiagnosticsOverlayTextFormatter _textFormatter = new();

        private IRunMotionSource _motionSource;
        private IRunSurfaceContextSource _surfaceContextSource;
        private IRunSteeringFrameSource _steeringFrameSource;
        private ICharacterVisualTargetPoseSource _visualTargetPoseSource;
        private ICharacterVisualFollowView _visualFollowView;
        private ICharacterVisualFollowTuning _visualFollowTuning;
        private IRunCameraAnchor _cameraAnchor;
        private IRunBodySpeedDiagnosticsSource _speedDiagnosticsSource;

        private RunDiagnosticsOverlayBuffer _samples;
        private Texture2D _pixelTexture;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _smallLabelStyle;
        private GUIStyle _diagnosticStyle;

        private Vector3 _previousMotionPosition;
        private Vector3 _previousVisualTargetPosition;
        private Vector3 _previousCameraPosition;
        private Vector3 _previousRawGroundNormal;
        private Vector3 _previousSteeringUp;
        private Quaternion _previousVisualTargetRotation = Quaternion.identity;
        private Quaternion _previousVisualRotation = Quaternion.identity;
        private Quaternion _previousCameraRotation = Quaternion.identity;
        private bool _hasPreviousMotionPosition;
        private bool _hasPreviousVisualTargetPosition;
        private bool _hasPreviousCameraPosition;
        private bool _hasPreviousRawGroundNormal;
        private bool _hasPreviousSteeringUp;
        private bool _hasPreviousVisualTargetRotation;
        private bool _hasPreviousVisualRotation;
        private bool _hasPreviousCameraRotation;
        private int _fixedStepsSinceLastSample;

        [Inject]
        private void Construct(
            IRunMotionSource motionSource,
            IRunSurfaceContextSource surfaceContextSource,
            IRunSteeringFrameSource steeringFrameSource,
            ICharacterVisualTargetPoseSource visualTargetPoseSource,
            ICharacterVisualFollowView visualFollowView,
            ICharacterVisualFollowTuning visualFollowTuning,
            IRunCameraAnchor cameraAnchor,
            IRunBodySpeedDiagnosticsSource speedDiagnosticsSource)
        {
            _motionSource = motionSource;
            _surfaceContextSource = surfaceContextSource;
            _steeringFrameSource = steeringFrameSource;
            _visualTargetPoseSource = visualTargetPoseSource;
            _visualFollowView = visualFollowView;
            _visualFollowTuning = visualFollowTuning;
            _cameraAnchor = cameraAnchor;
            _speedDiagnosticsSource = speedDiagnosticsSource;
        }

        private void Awake()
        {
            EnsureSampleBuffer();
            EnsurePixelTexture();
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

        private void FixedUpdate()
        {
            _fixedStepsSinceLastSample += 1;
        }

        private void OnGUI()
        {
            if (!_visible)
                return;

            EnsurePixelTexture();
            EnsureStyles();

            var panelRect = GetPanelRect();
            DrawRect(panelRect, new Color(0f, 0f, 0f, 0.72f));

            if (!HasDependencies)
            {
                DrawText(new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, 28f),
                    "RUN DIAG waiting for dependencies",
                    Color.white,
                    _titleStyle);
                return;
            }

            if (_samples == null || _samples.Count <= 0)
            {
                DrawText(new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, 28f),
                    "RUN DIAG waiting for samples",
                    Color.white,
                    _titleStyle);
                return;
            }

            DrawOverlay(panelRect, _samples.Latest);
        }

        private void OnDestroy()
        {
            if (_pixelTexture != null)
                Destroy(_pixelTexture);
        }

        private bool HasDependencies =>
            _motionSource != null
            && _surfaceContextSource != null
            && _steeringFrameSource != null
            && _visualTargetPoseSource != null
            && _visualFollowView != null
            && _visualFollowTuning != null
            && _cameraAnchor != null
            && _speedDiagnosticsSource != null;

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
            var surfaceContext = _surfaceContextSource.Current;

            var rawGroundNormal = surfaceContext.IsGrounded && surfaceContext.HasValidGroundNormal
                ? surfaceContext.GroundNormal
                : Vector3.up;
            var steeringUp = _steeringFrameSource.GetUpDirection(rawGroundNormal);

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
            var rawNormalDelta = _math.CalculateDirectionDelta(rawGroundNormal, ref _previousRawGroundNormal, ref _hasPreviousRawGroundNormal);
            var steeringUpDelta = _math.CalculateDirectionDelta(steeringUp, ref _previousSteeringUp, ref _hasPreviousSteeringUp);
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
            var cameraRotationDelta = _math.CalculateRotationDelta(cameraRotation, ref _previousCameraRotation, ref _hasPreviousCameraRotation);

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
                rawNormalDelta,
                steeringUpDelta,
                visualLag,
                cameraStep,
                targetToMotion,
                targetRotationDelta,
                visualRotationDelta,
                cameraRotationDelta,
                estimatedSnapReason,
                fixedStepsThisFrame,
                surfaceContext.IsGrounded,
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
            var titleRect = new Rect(contentX, panelRect.y + 7f, contentWidth, 22f);
            var detailRect = new Rect(contentX, panelRect.y + 29f, contentWidth, 18f);
            var speedDiagnosticsRect = new Rect(contentX, panelRect.y + 47f, contentWidth, 44f);
            var assistDiagnosticsRect = new Rect(contentX, panelRect.y + 91f, contentWidth, 30f);
            var chartTop = panelRect.y + 126f;
            var chartHeight = Mathf.Max(1f, panelRect.height - 134f);
            var rowCount = 10f;
            var rowHeight = Mathf.Max(1f, chartHeight / rowCount);

            DrawText(titleRect, "RUN DIAG | high-speed shake source", Color.white, _titleStyle);

            DrawText(detailRect,
                _textFormatter.FormatMotionSummary(latest),
                new Color(0.78f, 0.84f, 0.88f, 1f),
                _smallLabelStyle);

            DrawText(speedDiagnosticsRect,
                _textFormatter.FormatRunBodySpeed(latest.SpeedDiagnostics),
                new Color(0.65f, 0.9f, 1f, 1f),
                _diagnosticStyle);

            DrawText(assistDiagnosticsRect,
                _textFormatter.FormatLowSpeedAssist(latest.SpeedDiagnostics),
                new Color(0.72f, 1f, 0.72f, 1f),
                _diagnosticStyle);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 0f), contentWidth, rowHeight - 2f),
                0,
                "rb v",
                "m/s",
                ResolveScale(_speedScaleMetersPerSecond, 80f),
                new Color(1f, 1f, 1f, 0.95f),
                latest.SpeedMetersPerSecond);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 1f), contentWidth, rowHeight - 2f),
                1,
                "rb step",
                "m/s",
                ResolveScale(_speedScaleMetersPerSecond, 80f),
                new Color(1f, 0.38f, 0.32f, 0.95f),
                latest.MotionStepMetersPerSecond);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 2f), contentWidth, rowHeight - 2f),
                2,
                "tgt step",
                "m/s",
                ResolveScale(_speedScaleMetersPerSecond, 80f),
                new Color(1f, 0.58f, 0.16f, 0.95f),
                latest.VisualTargetStepMetersPerSecond);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 3f), contentWidth, rowHeight - 2f),
                3,
                "rawN d",
                "deg",
                ResolveScale(_normalDeltaScaleDegrees, 25f),
                new Color(1f, 0.88f, 0.16f, 0.95f),
                latest.RawGroundNormalDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 4f), contentWidth, rowHeight - 2f),
                4,
                "steer d",
                "deg",
                ResolveScale(_normalDeltaScaleDegrees, 25f),
                new Color(0.35f, 1f, 0.45f, 0.95f),
                latest.SteeringUpDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 5f), contentWidth, rowHeight - 2f),
                5,
                "snap",
                "",
                1f,
                new Color(1f, 0.95f, 0.22f, 0.95f),
                latest.HasEstimatedVisualSnap ? 1f : 0f);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 6f), contentWidth, rowHeight - 2f),
                6,
                "tgt rot",
                "deg",
                ResolveScale(_normalDeltaScaleDegrees, 25f),
                new Color(0.52f, 0.82f, 1f, 0.95f),
                latest.VisualTargetRotationDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 7f), contentWidth, rowHeight - 2f),
                7,
                "vis rot",
                "deg",
                ResolveScale(_normalDeltaScaleDegrees, 25f),
                new Color(0.62f, 0.65f, 1f, 0.95f),
                latest.VisualRotationDeltaDegrees);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 8f), contentWidth, rowHeight - 2f),
                8,
                "vis lag",
                "cm",
                ResolveScale(_visualLagScaleCentimeters, 15f),
                new Color(0.22f, 0.9f, 1f, 0.95f),
                latest.VisualLagCentimeters);

            DrawMetricRow(
                new Rect(contentX, chartTop + (rowHeight * 9f), contentWidth, rowHeight - 2f),
                9,
                "cam step",
                "m/s",
                ResolveScale(_speedScaleMetersPerSecond, 80f),
                new Color(1f, 0.35f, 1f, 0.95f),
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
            var labelWidth = Mathf.Min(122f, rowRect.width * 0.34f);
            var labelRect = new Rect(rowRect.x, rowRect.y + 1f, labelWidth, rowRect.height - 2f);

            var chartRect = new Rect(
                rowRect.x + labelWidth + 6f,
                rowRect.y + 3f,
                Mathf.Max(1f, rowRect.width - labelWidth - 6f),
                Mathf.Max(1f, rowRect.height - 6f));

            DrawText(labelRect, $"{label} {currentValue:0.0}{unit}", color, _labelStyle);
            DrawRect(chartRect, new Color(1f, 1f, 1f, 0.08f));
            DrawBars(chartRect, metricIndex, scaleMax, color);
        }

        private void DrawBars(Rect chartRect, int metricIndex, float scaleMax, Color color)
        {
            if (_samples == null || _samples.Count <= 0)
                return;

            var sampleCount = _samples.Count;
            var barWidth = Mathf.Max(1f, chartRect.width / Mathf.Max(1, _samples.Capacity));

            for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex += 1)
            {
                var sample = _samples.GetChronological(sampleIndex);
                var normalizedValue = Mathf.Clamp01(sample.SelectMetric(metricIndex) / scaleMax);
                var barHeight = Mathf.Max(1f, normalizedValue * chartRect.height);

                var barRect = new Rect(
                    chartRect.x + (sampleIndex * barWidth),
                    chartRect.yMax - barHeight,
                    Mathf.Max(1f, barWidth - 1f),
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
            var resolvedCapacity = Mathf.Clamp(_sampleCapacity, 32, 600);

            if (_samples == null || _samples.Capacity != resolvedCapacity)
                _samples = new RunDiagnosticsOverlayBuffer(resolvedCapacity);
        }

        private void EnsurePixelTexture()
        {
            if (_pixelTexture != null)
                return;

            _pixelTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _pixelTexture.SetPixel(0, 0, Color.white);
            _pixelTexture.Apply(false, false);
        }

        private void EnsureStyles()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.Max(10, _titleFontSize),
                    fontStyle = FontStyle.Bold,
                    clipping = TextClipping.Clip
                };
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.Max(9, _labelFontSize),
                    clipping = TextClipping.Clip
                };
            }

            if (_smallLabelStyle == null)
            {
                _smallLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.Max(8, _labelFontSize - 2),
                    clipping = TextClipping.Clip
                };
            }

            if (_diagnosticStyle == null)
            {
                _diagnosticStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.Max(8, _labelFontSize - 3),
                    wordWrap = true,
                    clipping = TextClipping.Clip
                };
            }
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
