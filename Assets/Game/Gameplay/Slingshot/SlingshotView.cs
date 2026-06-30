using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotView
    {
        float VisibleBandRadius { get; }
        SlingshotGeometrySnapshot CreateGeometrySnapshot();
        void ShowInactiveIdle(SlingshotBandShape bandShape);
        void ShowCaptureIdle(SlingshotBandShape bandShape);
        void ShowLoadedRelease(SlingshotBandShape bandShape);
        void ShowActivePull(SlingshotPullVisual pullVisual);
    }

    public sealed partial class SlingshotView : MonoBehaviour, ISlingshotView
    {
        [SerializeField] private Transform _leftAnchor;
        [SerializeField] private Transform _rightAnchor;
        [SerializeField] private Transform _restPoint;
        [SerializeField] private Transform _launchFrame;
        [SerializeField] private LineRenderer _bandLineRenderer;
        [SerializeField] private GameObject _touchIndicatorObject;
        [SerializeField] private SlingshotConfig _gizmoConfig;
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField, Min(0.05f)] private float _gizmoFrameAxisLength = 0.75f;
        [SerializeField, Min(0.01f)] private float _gizmoTouchTargetWorldRadius = 0.25f;

        public float VisibleBandRadius
        {
            get
            {
                ThrowIfInvalidReferences();
                return GetMaximumRenderedBandRadius();
            }
        }

        public SlingshotGeometrySnapshot CreateGeometrySnapshot()
        {
            ThrowIfInvalidReferences();
            return CreateGeometrySnapshotFromTransforms();
        }

        public void ShowInactiveIdle(SlingshotBandShape bandShape)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(bandShape);
            SetTouchIndicatorActive(false);
        }

        public void ShowCaptureIdle(SlingshotBandShape bandShape)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(bandShape);
            SetTouchIndicatorActive(false);
        }

        public void ShowLoadedRelease(SlingshotBandShape bandShape)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(bandShape);
            SetTouchIndicatorActive(false);
        }

        public void ShowActivePull(SlingshotPullVisual pullVisual)
        {
            ThrowIfInvalidReferences();
            ApplyBandShape(pullVisual.BandShape);
            SetTouchIndicatorScreenPosition(pullVisual.TouchIndicatorScreenPosition);
            SetTouchIndicatorActive(true);
        }

        private void OnValidate()
        {
            foreach (var error in GetReferenceValidationErrors())
            {
                Debug.LogWarning(error, this);
            }
        }

        private void ApplyBandShape(SlingshotBandShape bandShape)
        {
            var points = bandShape.Points;
            _bandLineRenderer.positionCount = points.Count;

            for (var i = 0; i < points.Count; i += 1)
            {
                _bandLineRenderer.SetPosition(i, points[i]);
            }
        }

        private void SetTouchIndicatorActive(bool active)
        {
            _touchIndicatorObject.SetActive(active);
        }

        private void SetTouchIndicatorScreenPosition(Vector2 screenPosition)
        {
            var indicatorTransform = _touchIndicatorObject.transform;
            var currentPosition = indicatorTransform.position;
            indicatorTransform.position = new Vector3(screenPosition.x, screenPosition.y, currentPosition.z);
        }

        private float GetMaximumRenderedBandRadius()
        {
            var maximumWidth = Mathf.Max(_bandLineRenderer.startWidth, _bandLineRenderer.endWidth);

            foreach (var key in _bandLineRenderer.widthCurve.keys)
            {
                maximumWidth = Mathf.Max(maximumWidth, key.value);
            }

            return maximumWidth * _bandLineRenderer.widthMultiplier * 0.5f;
        }

        private SlingshotGeometrySnapshot CreateGeometrySnapshotFromTransforms()
        {
            return new SlingshotGeometrySnapshot(
                _leftAnchor.position,
                _rightAnchor.position,
                _restPoint.position,
                _launchFrame.right,
                _launchFrame.forward,
                _launchFrame.up);
        }

        private void ThrowIfInvalidReferences()
        {
            var errors = GetReferenceValidationErrors().ToList();

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" ", errors));
        }

        private IEnumerable<string> GetReferenceValidationErrors()
        {
            if (_leftAnchor == null)
                yield return "SlingshotView requires a Left Anchor reference.";

            if (_rightAnchor == null)
                yield return "SlingshotView requires a Right Anchor reference.";

            if (_restPoint == null)
                yield return "SlingshotView requires a Rest Point reference.";

            if (_launchFrame == null)
                yield return "SlingshotView requires a Launch Frame reference.";

            if (_bandLineRenderer == null)
                yield return "SlingshotView requires a Band Line Renderer reference.";

            if (_touchIndicatorObject == null)
                yield return "SlingshotView requires a Touch Indicator object reference.";
        }
    }
}
