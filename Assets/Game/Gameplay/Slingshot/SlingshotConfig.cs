using UnityEngine;

namespace Game.Gameplay.Slingshot
{
    public interface ISlingshotConfig
    {
        float TouchTargetRadiusPixels { get; }
        float MinimumPullDistance { get; }
        float MaximumPullDistance { get; }
        float MaximumLateralPull { get; }
        float BandContactPadding { get; }
        int BandSilhouetteSampleCount { get; }
        int BandWrapSampleCount { get; }
        float BandRecoilDuration { get; }
        AnimationCurve BandRecoilCurve { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(SlingshotConfig),
        menuName = "Game/Gameplay/Slingshot Config")]
    public sealed class SlingshotConfig : ScriptableObject, ISlingshotConfig
    {
        [SerializeField, Min(1f),
         Tooltip("Controls: Screen-space radius around the visible Slingshot Band path that can begin a Slingshot Pull."
                 + "\n\nImpact: Larger values make capture more forgiving; smaller values require more precise touch or pointer placement."
                 + "\n\nTypical: Pixels. Keep large enough for mobile touch comfort without capturing unrelated screen input.")]
        private float _touchTargetRadiusPixels = 80f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Backward Pull distance before the Slingshot treats the held target as meaningfully pulled."
                 + "\n\nImpact: Higher values reject tiny pulls and jitter; lower values make weak pulls enter launch preparation sooner."
                 + "\n\nTypical: Unity world meters from rest along the Launch Frame backward axis.")]
        private float _minimumPullDistance = 0.25f;

        [SerializeField, Min(0.01f),
         Tooltip("Controls: Backward Pull distance that maps to full Pull Strength."
                 + "\n\nImpact: Higher values require a deeper pull for maximum Launch Impulse; lower values reach full strength sooner."
                 + "\n\nTypical: Unity world meters. Must stay greater than the minimum pull distance.")]
        private float _maximumPullDistance = 3f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Maximum side-to-side Pull Offset allowed from the Launch Frame center line."
                 + "\n\nImpact: Higher values allow stronger lateral launch aiming; lower values constrain side aiming before launch."
                 + "\n\nTypical: Unity world meters. This is Slingshot Pull Offset, not Run Steering Control.")]
        private float _maximumLateralPull = 1.5f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Visual clearance between Band Shape contact points and the held target silhouette."
                 + "\n\nImpact: Higher values keep the rendered Band farther from the target; lower values can make the Band appear to clip or overlap."
                 + "\n\nTypical: Unity world meters. Visual-only; it does not change Pull Strength or Launch Impulse.")]
        private float _bandContactPadding = 0.05f;

        [SerializeField, Range(8, 64),
         Tooltip("Controls: Number of samples used to approximate the held target silhouette for Band Shape wrapping."
                 + "\n\nImpact: Higher values can improve visual contact accuracy at higher cost; lower values are cheaper but rougher."
                 + "\n\nTypical: Integer sample count constrained by the Inspector range. Visual-only; it does not change launch force.")]
        private int _bandSilhouetteSampleCount = 32;

        [SerializeField, Range(3, 31),
         Tooltip("Controls: Number of points used to render wrapped Band Shape spans around the held target."
                 + "\n\nImpact: Higher values make curved wraps smoother at higher cost; lower values make wraps more angular."
                 + "\n\nTypical: Odd integer sample count constrained by the Inspector range. Visual-only; it does not change Pull Strength.")]
        private int _bandWrapSampleCount = 13;

        [SerializeField, Min(0.01f),
         Tooltip("Controls: Duration of Band Release Recoil after a valid launch."
                 + "\n\nImpact: Higher values make the Band recover more slowly; lower values make the visual snap back sooner."
                 + "\n\nTypical: Seconds. Visual-only recovery timing after release.")]
        private float _bandRecoilDuration = 0.18f;

        [SerializeField,
         Tooltip("Controls: Normalized easing curve for Band Release Recoil over its duration."
                 + "\n\nImpact: Curve shape changes the visual recovery feel without changing launch force or Pull interpretation."
                 + "\n\nTypical: Time and value are normalized from 0 to 1.")]
        private AnimationCurve _bandRecoilCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float TouchTargetRadiusPixels => _touchTargetRadiusPixels;
        public float MinimumPullDistance => _minimumPullDistance;
        public float MaximumPullDistance => _maximumPullDistance;
        public float MaximumLateralPull => _maximumLateralPull;
        public float BandContactPadding => _bandContactPadding;
        public int BandSilhouetteSampleCount => _bandSilhouetteSampleCount;
        public int BandWrapSampleCount => _bandWrapSampleCount;
        public float BandRecoilDuration => _bandRecoilDuration;
        public AnimationCurve BandRecoilCurve => _bandRecoilCurve;

        private void OnValidate()
        {
            var validator = new SlingshotConfigValidator();
            var errors = validator.Validate(this);

            foreach (var error in errors)
            {
                Debug.LogWarning(error);
            }
        }
    }
}
