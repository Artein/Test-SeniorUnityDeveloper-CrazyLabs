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
        [SerializeField, Min(0f)] private float _minimumForwardImpulse = 4f;
        [SerializeField, Min(0f)] private float _maximumForwardImpulse = 12f;
        [SerializeField] private AnimationCurve _pullStrengthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Range(0f, 85f)] private float _maximumLateralLaunchAngleDegrees = 35f;
        [SerializeField] private AnimationCurve _lateralAngleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Min(0f)] private float _upwardImpulse = 1.5f;
        [SerializeField] private bool _hasMinimumTotalImpulse;
        [SerializeField, Min(0f)] private float _minimumTotalImpulse;
        [SerializeField] private bool _hasMaximumTotalImpulse;
        [SerializeField, Min(0f)] private float _maximumTotalImpulse;

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
