using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Upgrades
{
    [Serializable]
    public sealed class UpgradeProgression
    {
        [SerializeField] private float _minimumValue;
        [SerializeField] private float _maximumValue = 1f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private UpgradeProgressionRoundingMode _roundingMode;
        [SerializeField, Min(0f)] private float _stepSize;
        [SerializeField] private List<UpgradeLevelValueOverride> _exactOverrides = new();

        public float MinimumValue => _minimumValue;
        public float MaximumValue => _maximumValue;
        public AnimationCurve Curve => _curve;
        public UpgradeProgressionRoundingMode RoundingMode => _roundingMode;
        public float StepSize => _stepSize;
        public IReadOnlyList<UpgradeLevelValueOverride> ExactOverrides => _exactOverrides;

        public UpgradeProgression()
        {
        }

        public UpgradeProgression(
            float minimumValue,
            float maximumValue,
            AnimationCurve curve,
            UpgradeProgressionRoundingMode roundingMode,
            float stepSize,
            IReadOnlyList<UpgradeLevelValueOverride> exactOverrides = null)
        {
            _minimumValue = minimumValue;
            _maximumValue = maximumValue;
            _curve = curve;
            _roundingMode = roundingMode;
            _stepSize = stepSize;
            _exactOverrides = new List<UpgradeLevelValueOverride>();

            if (exactOverrides == null)
                return;

            foreach (var exactOverride in exactOverrides)
            {
                _exactOverrides.Add(exactOverride);
            }
        }

        public float Evaluate(int level, int minimumLevel, int maximumLevel)
        {
            if (maximumLevel < minimumLevel)
                throw new ArgumentOutOfRangeException(nameof(maximumLevel), maximumLevel,
                    "Maximum level must be greater than or equal to minimum level.");

            if (level < minimumLevel || level > maximumLevel)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level is outside the progression range.");

            if (TryGetExactOverride(level, out var exactValue))
                return exactValue;

            if (_curve is not { length: > 0 })
                throw new InvalidOperationException("Upgrade progression curve must contain at least one key.");

            var normalizedLevel = GetNormalizedLevel(level, minimumLevel, maximumLevel);
            var curveValue = _curve.Evaluate(normalizedLevel);
            var projectedValue = Lerp(_minimumValue, _maximumValue, curveValue);
            var steppedValue = ApplyStep(projectedValue);
            return ApplyRounding(steppedValue);
        }

        private bool TryGetExactOverride(int level, out float value)
        {
            foreach (var exactOverride in _exactOverrides)
            {
                if (exactOverride.Level != level)
                    continue;

                value = exactOverride.Value;
                return true;
            }

            value = 0f;
            return false;
        }

        private float GetNormalizedLevel(int level, int minimumLevel, int maximumLevel)
        {
            if (maximumLevel == minimumLevel)
                return 1f;

            return (float)(level - minimumLevel) / (maximumLevel - minimumLevel);
        }

        private float Lerp(float minimumValue, float maximumValue, float curveValue)
        {
            return minimumValue + (maximumValue - minimumValue) * curveValue;
        }

        private float ApplyStep(float value)
        {
            if (_stepSize <= 0f)
                return value;

            return (float)(Math.Round(value / _stepSize, MidpointRounding.AwayFromZero) * _stepSize);
        }

        private float ApplyRounding(float value)
        {
            return _roundingMode switch
            {
                UpgradeProgressionRoundingMode.Floor => (float)Math.Floor(value),
                UpgradeProgressionRoundingMode.Ceiling => (float)Math.Ceiling(value),
                UpgradeProgressionRoundingMode.Nearest => (float)Math.Round(value, MidpointRounding.AwayFromZero),
                _ => value
            };
        }
    }
}
