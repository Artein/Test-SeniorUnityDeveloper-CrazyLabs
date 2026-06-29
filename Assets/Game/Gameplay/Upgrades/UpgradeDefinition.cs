using UnityEngine;

namespace Game.Gameplay.Upgrades
{
    public interface IUpgradeDefinition
    {
        string StableId { get; }
        string DisplayName { get; }
        string ShortDisplayName { get; }
        string Description { get; }
        Sprite Icon { get; }
        GameplayStatId TargetStatId { get; }
        int MaxLevel { get; }
        UpgradeProgression CostProgression { get; }
        UpgradeProgression EffectProgression { get; }
        UpgradeOperationType OperationType { get; }
        UpgradeValueFormat ValueFormat { get; }
        int DisplayDecimalPlaces { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(UpgradeDefinition),
        menuName = "Game/Gameplay/Upgrades/Upgrade Definition")]
    public sealed partial class UpgradeDefinition : ScriptableObject, IUpgradeDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private string _displayName;
        [SerializeField] private string _shortDisplayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameplayStatId _targetStatId;
        [SerializeField, Min(1)] private int _maxLevel = 1;
        [SerializeField] private UpgradeProgression _costProgression = new();
        [SerializeField] private UpgradeProgression _effectProgression = new();
        [SerializeField] private UpgradeOperationType _operationType = UpgradeOperationType.MultiplicativeFactor;
        [SerializeField] private UpgradeValueFormat _valueFormat = UpgradeValueFormat.Multiplier;
        [SerializeField, Min(0)] private int _displayDecimalPlaces = 1;

        public string StableId => _stableId;
        public string DisplayName => _displayName;
        public string ShortDisplayName => string.IsNullOrWhiteSpace(_shortDisplayName) ? _displayName : _shortDisplayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameplayStatId TargetStatId => _targetStatId;
        public int MaxLevel => _maxLevel;
        public UpgradeProgression CostProgression => _costProgression;
        public UpgradeProgression EffectProgression => _effectProgression;
        public UpgradeOperationType OperationType => _operationType;
        public UpgradeValueFormat ValueFormat => _valueFormat;
        public int DisplayDecimalPlaces => _displayDecimalPlaces;

        private void OnValidate()
        {
            var validator = new UpgradeDefinitionValidator(new UpgradeDefinitionEvaluator());
            var errors = validator.Validate(this);

            foreach (var error in errors)
            {
                Debug.LogWarning(error.Message);
            }
        }
    }
}
