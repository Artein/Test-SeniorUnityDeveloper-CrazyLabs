using Game.Gameplay.Slingshot;
using SaintsField;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public sealed partial class PullHintView : MonoBehaviour, IPullHintView, IPullHintTuning
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _fingerRoot;
        [SerializeField] private Image _fingerImage;
        [SerializeField] private Animator _animator;

        [SerializeField, AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Trigger)]
        private string _playTriggerName = "PlayPullHint";

        [SerializeField, Min(0f)] private float _initialIdleDelaySeconds = 2f;
        [SerializeField, Min(0f)] private float _playbackDurationSeconds = 1.25f;
        [SerializeField, Min(0f)] private float _repeatCooldownSeconds = 4f;
        [SerializeField] private Vector2 _screenOffset;

        private bool _isShowing;

        public float InitialIdleDelaySeconds => Mathf.Max(0f, _initialIdleDelaySeconds);
        public float PlaybackDurationSeconds => Mathf.Max(0f, _playbackDurationSeconds);
        public float RepeatCooldownSeconds => Mathf.Max(0f, _repeatCooldownSeconds);

        void IPullHintView.ShowAt(Vector2 screenPosition)
        {
            EnsureReferences();

            if (_root == null)
                return;

            var anchoredPosition = screenPosition + _screenOffset;
            var currentPosition = _root.position;
            _root.position = new Vector3(anchoredPosition.x, anchoredPosition.y, currentPosition.z);
            _isShowing = true;
            _root.gameObject.SetActive(true);
            ResetToHiddenStartState();
        }

        void IPullHintView.Play()
        {
            EnsureReferences();

            if (_animator != null && _animator.runtimeAnimatorController != null && _playTriggerName != null && _playTriggerName.Length > 0)
                _animator.SetTrigger(_playTriggerName);
        }

        void IPullHintView.Hide()
        {
            EnsureReferences();
            _isShowing = false;
            SetAlpha(0f);

            if (_root != null)
                _root.gameObject.SetActive(false);
        }

        private void Awake()
        {
            EnsureReferences();
            ResetToHiddenStartState();

            if (!_isShowing)
                ((IPullHintView)this).Hide();
        }

        private void OnValidate()
        {
            EnsureReferences();
            ApplyNonInteractiveSettings();
        }

        private void EnsureReferences()
        {
            if (_root == null)
                _root = transform as RectTransform;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_fingerRoot == null)
                _fingerRoot = transform.Find("Finger") as RectTransform;

            if (_fingerImage == null && _fingerRoot != null)
                _fingerImage = _fingerRoot.GetComponent<Image>();

            if (_fingerImage == null)
                _fingerImage = GetComponentInChildren<Image>(true);

            if (_animator == null)
                _animator = GetComponent<Animator>();

            ApplyNonInteractiveSettings();
        }

        private void ResetToHiddenStartState()
        {
            SetAlpha(0f);

            if (_fingerRoot != null)
            {
                _fingerRoot.anchoredPosition = Vector2.zero;
                _fingerRoot.localScale = Vector3.one;
            }

            ApplyNonInteractiveSettings();
        }

        private void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = alpha;
        }

        private void ApplyNonInteractiveSettings()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_fingerImage != null)
                _fingerImage.raycastTarget = false;
        }
    }
}
