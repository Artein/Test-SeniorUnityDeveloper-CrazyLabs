using System;
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
        [SerializeField] private TMP_Text _reachedDistanceText;
        [SerializeField] private GameObject _bestImprovementRoot;
        [SerializeField] private TMP_Text _bestImprovementText;
        [SerializeField] private Button _acknowledgeTouchAreaButton;

        private GameObject RootObject => _root != null ? _root : gameObject;

        public event Action AcknowledgeRequested;

        public void Apply(RunEndedViewState state)
        {
            ValidateSerializedReferences();

            RootObject.SetActive(state.IsVisible);
            _titleText.SetText(state.TitleText);
            _earnedCoinsText.text = state.EarnedCoinsText;
            _reachedDistanceText.text = state.ReachedDistanceText;
            _bestImprovementRoot.SetActive(state is { IsVisible: true, HasBestImprovement: true });
            _bestImprovementText.text = state.BestImprovementText;
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
        }

        private void ValidateSerializedReferences()
        {
            UnityEngine.Assertions.Assert.IsNotNull(_titleText, $"{nameof(RunEndedUIView)} requires a title text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_earnedCoinsText, $"{nameof(RunEndedUIView)} requires an earned coins text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_reachedDistanceText, $"{nameof(RunEndedUIView)} requires a reached distance text reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_bestImprovementRoot, $"{nameof(RunEndedUIView)} requires a best improvement root reference.");
            UnityEngine.Assertions.Assert.IsNotNull(_bestImprovementText, $"{nameof(RunEndedUIView)} requires a best improvement text reference.");

            UnityEngine.Assertions.Assert.IsNotNull(_acknowledgeTouchAreaButton,
                $"{nameof(RunEndedUIView)} requires an acknowledge touch area button reference.");
        }

        private void OnAcknowledgeTouchAreaClicked()
        {
            AcknowledgeRequested?.InvokeSafely();
        }
    }
}
