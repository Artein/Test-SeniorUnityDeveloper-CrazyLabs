using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    internal interface IRunSteeringAffordanceView
    {
        void Present(RunSteeringAffordancePresentationState state);
        void ApplyAnimation(float alpha, float scale);
        void Deactivate();
    }

    internal interface IRunSteeringAffordanceTuning
    {
        float HiddenScale { get; }
        float ShowDurationSeconds { get; }
        float HideDurationSeconds { get; }
    }

    public sealed partial class RunSteeringAffordanceView :
        MonoBehaviour,
        IRunSteeringAffordanceView,
        IRunSteeringAffordanceTuning
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _knobRoot;
        [SerializeField] private Image _knobImage;
        [SerializeField] private RectTransform _leftRangeEndRoot;
        [SerializeField] private Image _leftRangeEndImage;
        [SerializeField] private RectTransform _rightRangeEndRoot;
        [SerializeField] private Image _rightRangeEndImage;
        [SerializeField] private RectTransform _deadzoneRoot;
        [SerializeField] private Image _deadzoneImage;
        [SerializeField] private bool _deadzoneHintEnabled = true;
        [SerializeField] private Color _knobTint = Color.white;
        [SerializeField] private Color _rangeEndTint = new(1f, 1f, 1f, 0.58f);
        [SerializeField] private Color _deadzoneTint = new(1f, 1f, 1f, 0.24f);
        [SerializeField, Range(0f, 1f)] private float _hiddenScale = 0.86f;
        [SerializeField, Min(0f)] private float _showSeconds = 0.08f;
        [SerializeField, Min(0f)] private float _hideSeconds = 0.08f;

        private Canvas _canvas;

        float IRunSteeringAffordanceTuning.HiddenScale => Mathf.Clamp01(_hiddenScale);
        float IRunSteeringAffordanceTuning.ShowDurationSeconds => Mathf.Max(0f, _showSeconds);
        float IRunSteeringAffordanceTuning.HideDurationSeconds => Mathf.Max(0f, _hideSeconds);

        void IRunSteeringAffordanceView.Present(RunSteeringAffordancePresentationState state)
        {
            if (IsDestroyed())
                return;

            EnsureReferences();

            if (_root != null)
                _root.gameObject.SetActive(true);

            ApplyLayout(state);
            ApplyNonInteractiveSettings();
        }

        void IRunSteeringAffordanceView.ApplyAnimation(float alpha, float scale)
        {
            if (IsDestroyed())
                return;

            EnsureReferences();
            SetAlpha(alpha);
            SetVisualScale(scale);
            ApplyNonInteractiveSettings();
        }

        void IRunSteeringAffordanceView.Deactivate()
        {
            if (IsDestroyed())
                return;

            EnsureReferences();

            if (_root != null)
                _root.gameObject.SetActive(false);
        }

        internal IReadOnlyList<string> GetReferenceValidationErrors()
        {
            EnsureReferences();

            var errors = new List<string>();

            if (_root == null)
                errors.Add("RunSteeringAffordanceView requires a Root RectTransform reference.");

            if (_canvasGroup == null)
                errors.Add("RunSteeringAffordanceView requires a CanvasGroup reference.");

            if (_knobRoot == null || _knobImage == null)
                errors.Add("RunSteeringAffordanceView requires Knob RectTransform and Image references.");

            if (_leftRangeEndRoot == null || _leftRangeEndImage == null || _rightRangeEndRoot == null || _rightRangeEndImage == null)
                errors.Add("RunSteeringAffordanceView requires Left and Right Range End RectTransform and Image references.");

            if (_deadzoneHintEnabled && (_deadzoneRoot == null || _deadzoneImage == null))
                errors.Add("RunSteeringAffordanceView requires Deadzone RectTransform and Image references.");

            return errors;
        }

        private void Awake()
        {
            EnsureReferences();
            ApplyNonInteractiveSettings();
        }

        private void OnValidate()
        {
            EnsureReferences();
            ApplyNonInteractiveSettings();
        }

        private void EnsureReferences()
        {
            if (IsDestroyed())
                return;

            if (_root == null)
                _root = transform as RectTransform;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_knobRoot == null)
                _knobRoot = transform.Find("Knob") as RectTransform;

            if (_knobImage == null && _knobRoot != null)
                _knobImage = _knobRoot.GetComponent<Image>();

            if (_leftRangeEndRoot == null)
                _leftRangeEndRoot = transform.Find("Left Range End Hint") as RectTransform;

            if (_leftRangeEndImage == null && _leftRangeEndRoot != null)
                _leftRangeEndImage = _leftRangeEndRoot.GetComponent<Image>();

            if (_rightRangeEndRoot == null)
                _rightRangeEndRoot = transform.Find("Right Range End Hint") as RectTransform;

            if (_rightRangeEndImage == null && _rightRangeEndRoot != null)
                _rightRangeEndImage = _rightRangeEndRoot.GetComponent<Image>();

            if (_deadzoneRoot == null)
                _deadzoneRoot = transform.Find("Deadzone Hint") as RectTransform;

            if (_deadzoneImage == null && _deadzoneRoot != null)
                _deadzoneImage = _deadzoneRoot.GetComponent<Image>();

            _canvas = _root != null
                ? _root.GetComponentInParent<Canvas>(true)
                : GetComponentInParent<Canvas>(true);
        }

        private bool IsDestroyed()
        {
            return this == null;
        }

        private void ApplyLayout(RunSteeringAffordancePresentationState state)
        {
            SetScreenPosition(_knobRoot, state.KnobScreenPosition);
            SetScreenPosition(_leftRangeEndRoot, state.LeftRangeEndScreenPosition);
            SetScreenPosition(_rightRangeEndRoot, state.RightRangeEndScreenPosition);

            if (_deadzoneRoot != null)
            {
                _deadzoneRoot.gameObject.SetActive(_deadzoneHintEnabled && state.DeadzoneDiameterPixels > 0f);
                SetScreenPosition(_deadzoneRoot, state.OriginScreenPosition);

                var deadzoneDiameter = ScreenPixelsToCanvasUnits(state.DeadzoneDiameterPixels);
                _deadzoneRoot.sizeDelta = new Vector2(deadzoneDiameter, deadzoneDiameter);
            }

            ApplyImageSettings(state);
        }

        private void ApplyImageSettings(RunSteeringAffordancePresentationState state)
        {
            ApplyImageSettings(_knobImage, _knobTint);
            ApplyImageSettings(_leftRangeEndImage, CreateRangeEndTint(state.LeftRangeEndAlphaMultiplier));
            ApplyImageSettings(_rightRangeEndImage, CreateRangeEndTint(state.RightRangeEndAlphaMultiplier));
            ApplyImageSettings(_deadzoneImage, _deadzoneTint);
        }

        private void ApplyImageSettings(Image image, Color tint)
        {
            if (image == null)
                return;

            image.color = tint;
            ApplyNonInteractiveImageSettings(image);
        }

        private Color CreateRangeEndTint(float alphaMultiplier)
        {
            var tint = _rangeEndTint;
            tint.a *= Mathf.Clamp01(alphaMultiplier);
            return tint;
        }

        private void ApplyNonInteractiveImageSettings(Image image)
        {
            if (image == null)
                return;

            image.raycastTarget = false;
            image.preserveAspect = true;
        }

        private void ApplyNonInteractiveSettings()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            ApplyNonInteractiveImageSettings(_knobImage);
            ApplyNonInteractiveImageSettings(_leftRangeEndImage);
            ApplyNonInteractiveImageSettings(_rightRangeEndImage);
            ApplyNonInteractiveImageSettings(_deadzoneImage);
        }

        private void SetScreenPosition(RectTransform target, Vector2 screenPosition)
        {
            if (target == null)
                return;

            if (!TryGetParentLocalPosition(target, screenPosition, out var localPosition))
            {
                target.anchoredPosition = screenPosition;
                return;
            }

            var currentLocalPosition = target.localPosition;
            target.localPosition = new Vector3(localPosition.x, localPosition.y, currentLocalPosition.z);
        }

        private bool TryGetParentLocalPosition(RectTransform target, Vector2 screenPosition, out Vector2 localPosition)
        {
            localPosition = Vector2.zero;
            var parent = target.parent as RectTransform;

            if (parent == null || _canvas == null)
                return false;

            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return TryGetOverlayParentLocalPosition(parent, screenPosition, out localPosition);

            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, screenPosition, GetCanvasCamera(), out var worldPosition))
                return false;

            var parentLocalPosition = parent.InverseTransformPoint(worldPosition);
            localPosition = new Vector2(parentLocalPosition.x, parentLocalPosition.y);
            return true;
        }

        private bool TryGetOverlayParentLocalPosition(RectTransform parent, Vector2 screenPosition, out Vector2 localPosition)
        {
            localPosition = Vector2.zero;

            if (_canvas == null || _canvas.transform is not RectTransform canvasTransform)
                return false;

            var scaleFactor = _canvas.scaleFactor;

            if (scaleFactor <= 0f)
                return false;

            // CanvasScaler maps physical screen pixels into Canvas-local UI units via scaleFactor.
            var canvasRect = canvasTransform.rect;

            var canvasLocalPosition = new Vector3(
                canvasRect.xMin + (screenPosition.x / scaleFactor),
                canvasRect.yMin + (screenPosition.y / scaleFactor),
                0f);
            var worldPosition = canvasTransform.TransformPoint(canvasLocalPosition);
            var parentLocalPosition = parent.InverseTransformPoint(worldPosition);
            localPosition = new Vector2(parentLocalPosition.x, parentLocalPosition.y);
            return true;
        }

        private float ScreenPixelsToCanvasUnits(float screenPixels)
        {
            if (_canvas == null || _canvas.scaleFactor <= 0f)
                return screenPixels;

            return screenPixels / _canvas.scaleFactor;
        }

        private Camera GetCanvasCamera()
        {
            if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return _canvas.worldCamera;
        }

        private void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        private void SetVisualScale(float scale)
        {
            var clampedScale = Mathf.Max(0f, scale);
            var localScale = Vector3.one * clampedScale;

            if (_knobRoot != null)
                _knobRoot.localScale = localScale;

            if (_leftRangeEndRoot != null)
                _leftRangeEndRoot.localScale = localScale;

            if (_rightRangeEndRoot != null)
                _rightRangeEndRoot.localScale = localScale;

            if (_deadzoneRoot != null)
                _deadzoneRoot.localScale = localScale;
        }
    }
}
