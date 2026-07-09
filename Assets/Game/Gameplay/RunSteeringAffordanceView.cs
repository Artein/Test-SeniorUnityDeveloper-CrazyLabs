using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class RunSteeringAffordanceView : MonoBehaviour, IRunSteeringAffordanceView
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

        private float _animationElapsedSeconds;
        private float _animationDurationSeconds;
        private float _animationStartAlpha;
        private float _animationEndAlpha;
        private float _animationStartScale;
        private float _animationEndScale;
        private bool _isAnimating;
        private bool _hideWhenAnimationCompletes;

        void IRunSteeringAffordanceView.Show(RunSteeringAffordancePresentationState state)
        {
            Show(state);
        }

        void IRunSteeringAffordanceView.Update(RunSteeringAffordancePresentationState state)
        {
            Update(state);
        }

        void IRunSteeringAffordanceView.Hide(RunSteeringAffordancePresentationState state)
        {
            Hide(state);
        }

        private void Show(RunSteeringAffordancePresentationState state)
        {
            if (IsDestroyed())
                return;

            EnsureReferences();

            if (!state.IsVisible)
            {
                Reset();
                return;
            }

            if (_root != null)
                _root.gameObject.SetActive(true);

            ApplyLayout(state);
            ApplyNonInteractiveSettings();
            BeginAnimation(1f, 1f, Mathf.Clamp01(_hiddenScale), 1f, Mathf.Max(0f, _showSeconds), false);
        }

        private void Update(RunSteeringAffordancePresentationState state)
        {
            if (IsDestroyed())
                return;

            EnsureReferences();

            if (!state.IsVisible)
            {
                Reset();
                return;
            }

            if (_root != null)
                _root.gameObject.SetActive(true);

            ApplyLayout(state);
            ApplyNonInteractiveSettings();
        }

        private void Hide(RunSteeringAffordancePresentationState state)
        {
            if (IsDestroyed())
                return;

            EnsureReferences();

            if (state.IsVisible)
                ApplyLayout(state);

            ApplyNonInteractiveSettings();
            BeginAnimation(GetAlpha(), 0f, GetVisualScale(), Mathf.Clamp01(_hiddenScale), Mathf.Max(0f, _hideSeconds), true);
        }

        public void Reset()
        {
            if (IsDestroyed())
                return;

            EnsureReferences();
            _isAnimating = false;
            SetAlpha(0f);
            SetVisualScale(Mathf.Clamp01(_hiddenScale));
            ApplyNonInteractiveSettings();

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

            if (_root != null && !_root.gameObject.activeSelf)
                Reset();
        }

        private void Update()
        {
            TickAnimation(Time.unscaledDeltaTime);
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
        }

        private bool IsDestroyed()
        {
            return this == null;
        }

        private void BeginAnimation(
            float startAlpha,
            float endAlpha,
            float startScale,
            float endScale,
            float durationSeconds,
            bool hideWhenAnimationCompletes)
        {
            _animationElapsedSeconds = 0f;
            _animationDurationSeconds = durationSeconds;
            _animationStartAlpha = startAlpha;
            _animationEndAlpha = endAlpha;
            _animationStartScale = startScale;
            _animationEndScale = endScale;
            _hideWhenAnimationCompletes = hideWhenAnimationCompletes;

            if (durationSeconds <= 0f)
            {
                CompleteAnimation();
                return;
            }

            _isAnimating = true;
            SetAlpha(startAlpha);
            SetVisualScale(startScale);
        }

        private void TickAnimation(float deltaSeconds)
        {
            if (!_isAnimating)
                return;

            _animationElapsedSeconds += Mathf.Max(0f, deltaSeconds);
            var progress = _animationDurationSeconds > 0f
                ? Mathf.Clamp01(_animationElapsedSeconds / _animationDurationSeconds)
                : 1f;

            SetAlpha(Mathf.Lerp(_animationStartAlpha, _animationEndAlpha, progress));
            SetVisualScale(Mathf.Lerp(_animationStartScale, _animationEndScale, progress));

            if (progress >= 1f)
                CompleteAnimation();
        }

        private void CompleteAnimation()
        {
            _isAnimating = false;
            SetAlpha(_animationEndAlpha);
            SetVisualScale(_animationEndScale);

            if (_hideWhenAnimationCompletes && _root != null)
                _root.gameObject.SetActive(false);
        }

        private void ApplyLayout(RunSteeringAffordancePresentationState state)
        {
            SetAnchoredPosition(_knobRoot, state.KnobScreenPosition);
            SetAnchoredPosition(_leftRangeEndRoot, state.LeftRangeEndScreenPosition);
            SetAnchoredPosition(_rightRangeEndRoot, state.RightRangeEndScreenPosition);

            if (_deadzoneRoot != null)
            {
                _deadzoneRoot.gameObject.SetActive(_deadzoneHintEnabled && state.DeadzoneDiameterPixels > 0f);
                _deadzoneRoot.anchoredPosition = state.OriginScreenPosition;
                _deadzoneRoot.sizeDelta = new Vector2(state.DeadzoneDiameterPixels, state.DeadzoneDiameterPixels);
            }

            ApplyImageSettings();
        }

        private void ApplyImageSettings()
        {
            ApplyImageSettings(_knobImage, _knobTint);
            ApplyImageSettings(_leftRangeEndImage, _rangeEndTint);
            ApplyImageSettings(_rightRangeEndImage, _rangeEndTint);
            ApplyImageSettings(_deadzoneImage, _deadzoneTint);
        }

        private void ApplyImageSettings(Image image, Color tint)
        {
            if (image == null)
                return;

            image.color = tint;
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

            ApplyImageSettings();
        }

        private void SetAnchoredPosition(RectTransform target, Vector2 screenPosition)
        {
            if (target != null)
                target.anchoredPosition = screenPosition;
        }

        private float GetAlpha()
        {
            return _canvasGroup != null ? _canvasGroup.alpha : 0f;
        }

        private void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        private float GetVisualScale()
        {
            return _knobRoot != null ? _knobRoot.localScale.x : 1f;
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
