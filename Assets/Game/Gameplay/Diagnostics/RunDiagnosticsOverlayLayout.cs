using UnityEngine;

namespace Game.Gameplay.Diagnostics
{
    internal sealed class RunDiagnosticsOverlayLayout
    {
        private readonly float _horizontalMargin;
        private readonly float _topMargin;
        private readonly float _bottomMargin;
        private readonly float _heightScreenRatio;
        private readonly float _minimumHeight;
        private readonly float _maximumHeight;

        public RunDiagnosticsOverlayLayout()
            : this(
                horizontalMargin: 0f,
                topMargin: 6f,
                bottomMargin: 8f,
                heightScreenRatio: 0.26f,
                minimumHeight: 320f,
                maximumHeight: 400f)
        {
        }

        internal RunDiagnosticsOverlayLayout(
            float horizontalMargin,
            float topMargin,
            float bottomMargin,
            float heightScreenRatio,
            float minimumHeight,
            float maximumHeight)
        {
            _horizontalMargin = Mathf.Max(0f, horizontalMargin);
            _topMargin = Mathf.Max(0f, topMargin);
            _bottomMargin = Mathf.Max(0f, bottomMargin);
            _heightScreenRatio = float.IsFinite(heightScreenRatio) && heightScreenRatio > 0f ? heightScreenRatio : 0.26f;
            _minimumHeight = Mathf.Max(1f, minimumHeight);
            _maximumHeight = Mathf.Max(_minimumHeight, maximumHeight);
        }

        public Rect CreatePanelRect(float screenWidth, float screenHeight)
        {
            var resolvedScreenWidth = Mathf.Max(1f, screenWidth);
            var resolvedScreenHeight = Mathf.Max(1f, screenHeight);
            var width = Mathf.Max(1f, resolvedScreenWidth - (_horizontalMargin * 2f));
            var availableHeight = Mathf.Max(1f, resolvedScreenHeight - _topMargin - _bottomMargin);
            var preferredHeight = Mathf.Clamp(resolvedScreenHeight * _heightScreenRatio, _minimumHeight, _maximumHeight);
            var height = Mathf.Min(preferredHeight, availableHeight);

            return new Rect(_horizontalMargin, _topMargin, width, height);
        }
    }
}
