using System;
using System.Collections.Generic;
using Game.Utils.Mathematics;
using UnityEngine;

namespace Game.Gameplay
{
    public interface IGameplaySlingshotLaunchConfig
    {
        float MinimumForwardImpulse { get; }
        float MaximumForwardImpulse { get; }
        AnimationCurve PullStrengthCurve { get; }
        float MaximumLateralLaunchAngleDegrees { get; }
        AnimationCurve LateralAngleCurve { get; }
        float UpwardImpulse { get; }
        bool HasMinimumTotalImpulse { get; }
        float MinimumTotalImpulse { get; }
        bool HasMaximumTotalImpulse { get; }
        float MaximumTotalImpulse { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(GameplaySlingshotLaunchConfig),
        menuName = "Game/Gameplay/Gameplay Slingshot Launch Config")]
    public sealed partial class GameplaySlingshotLaunchConfig : ScriptableObject, IGameplaySlingshotLaunchConfig
    {
        [SerializeField, Min(0f),
         Tooltip("Controls: Forward Launch Impulse used at the lowest accepted Pull Strength."
                 + "\n\nImpact: Higher values make weak valid pulls launch farther; lower values make weak pulls softer."
                 + "\n\nTypical: Non-negative impulse. Must stay less than or equal to maximum forward impulse.")]
        private float _minimumForwardImpulse = 4f;

        [SerializeField, Min(0f),
         Tooltip("Controls: Forward Launch Impulse used at full Pull Strength."
                 + "\n\nImpact: Higher values increase maximum launch distance and run entry speed; lower values cap full pulls sooner."
                 + "\n\nTypical: Non-negative impulse. Must stay greater than or equal to minimum forward impulse.")]
        private float _maximumForwardImpulse = 12f;

        [SerializeField,
         Tooltip("Controls: Mapping from normalized Pull Strength to forward Launch Impulse between the minimum and maximum values."
                 + "\n\nImpact: A steeper curve rewards deeper pulls more aggressively; a flatter curve makes pull depth less sensitive."
                 + "\n\nTypical: Normalized 0 to 1 input and output. Avoid implying new balance unless the curve is intentionally retuned.")]
        private AnimationCurve _pullStrengthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField, Range(0f, 85f),
         Tooltip("Controls: Maximum angle that lateral Pull Offset can bend the accepted Launch Impulse direction."
                 + "\n\nImpact: Higher values allow sharper side launches; lower values keep launch direction closer to straight forward."
                 + "\n\nTypical: Degrees, constrained by the Inspector range. This is launch aiming, not Run Steering Control.")]
        private float _maximumLateralLaunchAngleDegrees = 35f;

        [SerializeField,
         Tooltip("Controls: Mapping from normalized lateral Pull Offset to launch angle fraction."
                 + "\n\nImpact: Curve shape changes how quickly side pull becomes lateral launch angle."
                 + "\n\nTypical: Normalized 0 to 1 input and output, multiplied by maximum lateral launch angle.")]
        private AnimationCurve _lateralAngleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField, Min(0f),
         Tooltip("Controls: Upward Launch Impulse added to the calculated forward and lateral launch."
                 + "\n\nImpact: Higher values add lift, arc, airtime, and a softer run entry; lower values keep launches flatter."
                 + "\n\nTypical: Non-negative impulse. Tune with forward impulse so launch arcs still land into the Run cleanly.")]
        private float _upwardImpulse = 1.5f;

        [SerializeField,
         Tooltip(
             "Enables a defensive lower bound on total Launch Impulse magnitude. Leave off when minimum and maximum forward impulse already define the intended floor.")]
        private bool _hasMinimumTotalImpulse;

        [SerializeField, Min(0f),
         Tooltip("Controls: Minimum total Launch Impulse magnitude when the lower-bound clamp is enabled."
                 + "\n\nImpact: Higher values prevent unusually soft launches after all direction components are combined."
                 + "\n\nTypical: Non-negative impulse. Used only when the matching enable toggle is on.")]
        private float _minimumTotalImpulse;

        [SerializeField,
         Tooltip(
             "Enables a defensive upper bound on total Launch Impulse magnitude. Leave off unless combined impulse components need an explicit safety cap.")]
        private bool _hasMaximumTotalImpulse;

        [SerializeField, Min(0f),
         Tooltip("Controls: Maximum total Launch Impulse magnitude when the upper-bound clamp is enabled."
                 + "\n\nImpact: Lower values cap extreme launches after all direction components are combined."
                 + "\n\nTypical: Non-negative impulse. Used only when the matching enable toggle is on and must not be below the minimum clamp.")]
        private float _maximumTotalImpulse;

        public float MinimumForwardImpulse => _minimumForwardImpulse;
        public float MaximumForwardImpulse => _maximumForwardImpulse;
        public AnimationCurve PullStrengthCurve => _pullStrengthCurve;
        public float MaximumLateralLaunchAngleDegrees => _maximumLateralLaunchAngleDegrees;
        public AnimationCurve LateralAngleCurve => _lateralAngleCurve;
        public float UpwardImpulse => _upwardImpulse;
        public bool HasMinimumTotalImpulse => _hasMinimumTotalImpulse;
        public float MinimumTotalImpulse => _minimumTotalImpulse;
        public bool HasMaximumTotalImpulse => _hasMaximumTotalImpulse;
        public float MaximumTotalImpulse => _maximumTotalImpulse;

        private void OnValidate()
        {
            var validator = new GameplaySlingshotLaunchConfigValidator();

            foreach (var error in validator.Validate(this))
            {
                Debug.LogWarning(error, this);
            }
        }
    }

    internal sealed class GameplaySlingshotLaunchConfigValidator
    {
        public IEnumerable<string> Validate(IGameplaySlingshotLaunchConfig config)
        {
            if (config is null)
            {
                yield return "Gameplay Slingshot Launch config is missing.";
                yield break;
            }

            if (!config.MinimumForwardImpulse.IsFinitePositive())
                yield return $"Gameplay Slingshot Launch {nameof(config.MinimumForwardImpulse)} must be a finite non-negative value.";

            if (!config.MaximumForwardImpulse.IsFinitePositive())
                yield return $"Gameplay Slingshot Launch {nameof(config.MaximumForwardImpulse)} must be a finite non-negative value.";

            if (config.MaximumForwardImpulse < config.MinimumForwardImpulse)
                yield return "Gameplay Slingshot Launch maximum forward impulse must be greater than or equal to minimum forward impulse.";

            if (config.PullStrengthCurve is not { length: > 0 })
                yield return "Gameplay Slingshot Launch pull strength curve must contain at least one key.";

            if (float.IsNaN(config.MaximumLateralLaunchAngleDegrees)
                || float.IsInfinity(config.MaximumLateralLaunchAngleDegrees)
                || config.MaximumLateralLaunchAngleDegrees < 0f)
                yield return $"Gameplay Slingshot Launch {nameof(config.MaximumLateralLaunchAngleDegrees)} must be a finite non-negative value.";

            if (config.LateralAngleCurve is not { length: > 0 })
                yield return "Gameplay Slingshot Launch lateral angle curve must contain at least one key.";

            if (!config.UpwardImpulse.IsFinitePositive())
                yield return $"Gameplay Slingshot Launch {nameof(config.UpwardImpulse)} must be a finite non-negative value.";

            if (config.HasMinimumTotalImpulse && !config.MinimumTotalImpulse.IsFinitePositive())
                yield return $"Gameplay Slingshot Launch {nameof(config.MinimumTotalImpulse)} must be a finite non-negative value when enabled.";

            if (config.HasMaximumTotalImpulse && !config.MaximumTotalImpulse.IsFinitePositive())
                yield return $"Gameplay Slingshot Launch {nameof(config.MaximumTotalImpulse)} must be a finite non-negative value when enabled.";

            if (config.HasMinimumTotalImpulse
                && config.HasMaximumTotalImpulse
                && config.MaximumTotalImpulse < config.MinimumTotalImpulse)
            {
                yield return "Gameplay Slingshot Launch maximum total impulse must be greater than or equal to minimum total impulse.";
            }
        }
    }
}
