#if UNITY_INCLUDE_TESTS

using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public sealed partial class SlingshotView
    {
        internal void SetReferencesForTests(
            Transform leftAnchor,
            Transform rightAnchor,
            Transform restPoint,
            Transform launchFrame,
            LineRenderer bandLineRenderer,
            GameObject pullHintObject,
            GameObject touchIndicatorObject,
            SlingshotConfig gizmoConfig)
        {
            _leftAnchor = leftAnchor;
            _rightAnchor = rightAnchor;
            _restPoint = restPoint;
            _launchFrame = launchFrame;
            _bandLineRenderer = bandLineRenderer;
            _pullHintObject = pullHintObject;
            _touchIndicatorObject = touchIndicatorObject;
            _gizmoConfig = gizmoConfig;
        }
    }
}

#endif // UNITY_INCLUDE_TESTS
