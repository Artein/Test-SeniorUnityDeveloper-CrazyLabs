using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utils.Invocation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay
{
    public interface IRunEndedView
    {
        event Action AcknowledgeRequested;

        void Apply(RunEndedViewState state);
    }

    public sealed partial class RunEndedUIView : MonoBehaviour, IRunEndedView
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _earnedCoinsText;
        [SerializeField] private Graphic[] _earnedCoinsRevealGraphics;
        [SerializeField] private TMP_Text _reachedDistanceText;
        [SerializeField] private Transform _rewardSourceRowsRoot;
        [SerializeField] private RunEndedRewardSourceRowUIView _rewardSourceRowPrefab;
        [SerializeField] private GameObject _bestImprovementRoot;
        [SerializeField] private TMP_Text _bestImprovementText;
        [SerializeField] private GameObject _tapToContinueRoot;
        [SerializeField] private Button _acknowledgeTouchAreaButton;
        [SerializeField, Min(0f)] private float _labelFadeDuration = 0.18f;
        [SerializeField, Min(0f)] private float _counterDuration = 0.35f;
        [SerializeField, Min(0f)] private float _stepDelay = 0.05f;

        private readonly List<RunEndedRewardSourceRowUIView> _rewardSourceRows = new();
        private Coroutine _revealCoroutine;
        private RunEndedViewState _currentState;
        private bool _isRevealRunning;
        private bool _canAcknowledge;

        private GameObject RootObject => _root != null ? _root : gameObject;

        public event Action AcknowledgeRequested;

        public void Apply(RunEndedViewState state)
        {
            ValidateSerializedReferences();
            StopRevealCoroutine();
            _currentState = state;

            RootObject.SetActive(state.IsVisible);

            if (!state.IsVisible)
            {
                ClearRewardSourceRows();
                _canAcknowledge = false;
                _isRevealRunning = false;
                _titleText.text = string.Empty;
                _earnedCoinsText.text = string.Empty;
                _reachedDistanceText.text = string.Empty;
                _bestImprovementRoot.SetActive(false);
                _bestImprovementText.text = string.Empty;
                SetEarnedCoinsRevealAlpha(0f);
                _tapToContinueRoot.SetActive(false);
                _acknowledgeTouchAreaButton.interactable = false;
                return;
            }

            BuildRewardSourceRows(state);
            _titleText.SetText(state.TitleText);
            _earnedCoinsText.text = state.EarnedCoinsText;
            _reachedDistanceText.text = state.ReachedDistanceText;
            _bestImprovementRoot.SetActive(state is { IsVisible: true, HasBestImprovement: true });
            _bestImprovementText.text = state.BestImprovementText;
            _acknowledgeTouchAreaButton.interactable = true;

            PrepareRevealFrame(state);

            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                CompleteReveal();
                return;
            }

            // TODO: Consider migrating this reveal sequence to UniTask if async UI flows become common.
            _revealCoroutine = StartCoroutine(PlayRevealSequence());
        }

        private void Awake()
        {
            ValidateSerializedReferences();
        }

        private void OnEnable()
        {
            if (_acknowledgeTouchAreaButton != null)
                _acknowledgeTouchAreaButton.onClick.AddListener(OnAcknowledgeTouchAreaClicked);
        }

        private void OnDisable()
        {
            if (_acknowledgeTouchAreaButton != null)
                _acknowledgeTouchAreaButton.onClick.RemoveListener(OnAcknowledgeTouchAreaClicked);

            StopRevealCoroutine();
        }

        private void ValidateSerializedReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_titleText, $"{nameof(RunEndedUIView)} requires a title text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_earnedCoinsText, $"{nameof(RunEndedUIView)} requires an earned coins text reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_earnedCoinsRevealGraphics,
                $"{nameof(RunEndedUIView)} requires earned coins reveal graphic references.");

            UnityEngine.Assertions.Assert.IsTrue(_earnedCoinsRevealGraphics.Length > 0,
                $"{nameof(RunEndedUIView)} requires at least one earned coins reveal graphic reference.");

            foreach (var earnedCoinsRevealGraphic in _earnedCoinsRevealGraphics)
            {
                UnityEngine.Assertions.Assert.IsNotNull(earnedCoinsRevealGraphic,
                    $"{nameof(RunEndedUIView)} has a missing earned coins reveal graphic reference.");
            }

            UnityEngine.Assertions.Assert.IsNotNull(_reachedDistanceText, $"{nameof(RunEndedUIView)} requires a reached distance text reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_rewardSourceRowsRoot,
                $"{nameof(RunEndedUIView)} requires a reward source rows root reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_rewardSourceRowPrefab,
                $"{nameof(RunEndedUIView)} requires a reward source row prefab reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_bestImprovementRoot, $"{nameof(RunEndedUIView)} requires a best improvement root reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_bestImprovementText, $"{nameof(RunEndedUIView)} requires a best improvement text reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_tapToContinueRoot,
                $"{nameof(RunEndedUIView)} requires a tap to continue root reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_acknowledgeTouchAreaButton,
                $"{nameof(RunEndedUIView)} requires an acknowledge touch area button reference.");
        }

        private void OnAcknowledgeTouchAreaClicked()
        {
            if (_isRevealRunning)
            {
                CompleteReveal();
                return;
            }

            if (!_canAcknowledge)
                return;

            AcknowledgeRequested?.InvokeSafely();
        }

        private void BuildRewardSourceRows(RunEndedViewState state)
        {
            ClearRewardSourceRows();

            // TODO: Pool source row instances if the result screen starts rebuilding frequently.
            foreach (var rewardSourceRow in state.RewardSourceRows)
            {
                var row = Instantiate(_rewardSourceRowPrefab, _rewardSourceRowsRoot);
                row.gameObject.SetActive(true);
                row.Apply(rewardSourceRow);
                row.ApplyRevealFrame(0f, 0);
                _rewardSourceRows.Add(row);
            }
        }

        private void ClearRewardSourceRows()
        {
            foreach (var rewardSourceRow in _rewardSourceRows)
            {
                if (rewardSourceRow == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(rewardSourceRow.gameObject);
                else
                    DestroyImmediate(rewardSourceRow.gameObject);
            }

            _rewardSourceRows.Clear();
        }

        private IEnumerator PlayRevealSequence()
        {
            _isRevealRunning = true;
            _canAcknowledge = false;

            yield return AnimateTextAlpha(_titleText, _labelFadeDuration);
            yield return WaitForStepDelay();
            yield return AnimateDistanceCounter(_currentState);
            yield return WaitForStepDelay();

            if (_currentState.HasBestImprovement)
            {
                yield return AnimateTextAlpha(_bestImprovementText, _labelFadeDuration);
                yield return WaitForStepDelay();
            }

            foreach (var rewardSourceRow in _rewardSourceRows)
            {
                yield return AnimateRewardSourceRow(rewardSourceRow);
                yield return WaitForStepDelay();
            }

            yield return AnimateEarnedCoinsRevealAlpha(_labelFadeDuration);

            _revealCoroutine = null;
            ApplyCompletedRevealFrame();
        }

        private IEnumerator AnimateTextAlpha(TMP_Text text, float duration)
        {
            if (duration <= 0f)
            {
                SetTextAlpha(text, 1f);
                yield break;
            }

            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetTextAlpha(text, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            SetTextAlpha(text, 1f);
        }

        private IEnumerator AnimateGraphicsAlpha(IReadOnlyList<Graphic> graphics, float duration)
        {
            if (duration <= 0f)
            {
                SetGraphicsAlpha(graphics, 1f);
                yield break;
            }

            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetGraphicsAlpha(graphics, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            SetGraphicsAlpha(graphics, 1f);
        }

        private IEnumerator AnimateEarnedCoinsRevealAlpha(float duration)
        {
            if (duration <= 0f)
            {
                SetEarnedCoinsRevealAlpha(1f);
                yield break;
            }

            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetEarnedCoinsRevealAlpha(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            SetEarnedCoinsRevealAlpha(1f);
        }

        private IEnumerator AnimateDistanceCounter(RunEndedViewState state)
        {
            if (_counterDuration <= 0f)
            {
                _reachedDistanceText.text = state.ReachedDistanceText;
                SetTextAlpha(_reachedDistanceText, 1f);
                yield break;
            }

            var elapsed = 0f;

            while (elapsed < _counterDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / _counterDuration);
                var displayedMeters = Mathf.RoundToInt(Mathf.Lerp(0f, state.ReachedMeters, progress));
                _reachedDistanceText.text = FormatReachedDistanceText(displayedMeters);
                SetTextAlpha(_reachedDistanceText, progress);
                yield return null;
            }

            _reachedDistanceText.text = state.ReachedDistanceText;
            SetTextAlpha(_reachedDistanceText, 1f);
        }

        private IEnumerator AnimateRewardSourceRow(RunEndedRewardSourceRowUIView row)
        {
            if (row == null)
                yield break;

            var rowIndex = _rewardSourceRows.IndexOf(row);

            if (rowIndex < 0 || rowIndex >= _currentState.RewardSourceRows.Count)
                yield break;

            var rowState = _currentState.RewardSourceRows[rowIndex];

            if (_counterDuration <= 0f)
            {
                row.Apply(rowState);
                yield break;
            }

            var elapsed = 0f;

            while (elapsed < _counterDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / _counterDuration);
                var displayedAmount = Mathf.RoundToInt(Mathf.Lerp(0f, rowState.Amount, progress));
                row.ApplyRevealFrame(progress, displayedAmount);
                yield return null;
            }

            row.Apply(rowState);
        }

        private IEnumerator WaitForStepDelay()
        {
            if (_stepDelay > 0f)
                yield return new WaitForSecondsRealtime(_stepDelay);
        }

        private void PrepareRevealFrame(RunEndedViewState state)
        {
            _isRevealRunning = false;
            _canAcknowledge = false;
            _tapToContinueRoot.SetActive(false);

            _titleText.text = state.TitleText;
            _earnedCoinsText.text = state.EarnedCoinsText;
            _reachedDistanceText.text = FormatReachedDistanceText(0);
            _bestImprovementRoot.SetActive(state.HasBestImprovement);
            _bestImprovementText.text = state.BestImprovementText;

            SetTextAlpha(_titleText, 0f);
            SetEarnedCoinsRevealAlpha(0f);
            SetTextAlpha(_reachedDistanceText, 0f);
            SetTextAlpha(_bestImprovementText, 0f);

            for (var i = 0; i < _rewardSourceRows.Count; i += 1)
            {
                _rewardSourceRows[i].Apply(state.RewardSourceRows[i]);
                _rewardSourceRows[i].ApplyRevealFrame(0f, 0);
            }
        }

        private void CompleteReveal()
        {
            StopRevealCoroutine();
            ApplyCompletedRevealFrame();
        }

        private void ApplyCompletedRevealFrame()
        {
            _isRevealRunning = false;
            _canAcknowledge = _currentState.IsVisible;
            _tapToContinueRoot.SetActive(_currentState.IsVisible);

            _titleText.text = _currentState.TitleText;
            _earnedCoinsText.text = _currentState.EarnedCoinsText;
            _reachedDistanceText.text = _currentState.ReachedDistanceText;
            _bestImprovementRoot.SetActive(_currentState.HasBestImprovement);
            _bestImprovementText.text = _currentState.BestImprovementText;

            SetTextAlpha(_titleText, 1f);
            SetEarnedCoinsRevealAlpha(1f);
            SetTextAlpha(_reachedDistanceText, 1f);
            SetTextAlpha(_bestImprovementText, _currentState.HasBestImprovement ? 1f : 0f);

            for (var rowIndex = 0; rowIndex < _rewardSourceRows.Count; rowIndex += 1)
            {
                _rewardSourceRows[rowIndex].Apply(_currentState.RewardSourceRows[rowIndex]);
            }
        }

        private void StopRevealCoroutine()
        {
            if (_revealCoroutine == null)
                return;

            StopCoroutine(_revealCoroutine);
            _revealCoroutine = null;
        }

        private void SetEarnedCoinsRevealAlpha(float alpha)
        {
            SetTextAlpha(_earnedCoinsText, alpha);
            SetGraphicsAlpha(_earnedCoinsRevealGraphics, alpha);
        }

        private static void SetTextAlpha(TMP_Text text, float alpha)
        {
            var color = text.color;
            color.a = Mathf.Clamp01(alpha);
            text.color = color;
        }

        private static void SetGraphicsAlpha(IReadOnlyList<Graphic> graphics, float alpha)
        {
            foreach (var graphic in graphics)
            {
                var color = graphic.color;
                color.a = Mathf.Clamp01(alpha);
                graphic.color = color;
            }
        }

        private static string FormatReachedDistanceText(int reachedMeters)
        {
            return "DISTANCE " + Mathf.Max(0, reachedMeters) + " m";
        }
    }
}
