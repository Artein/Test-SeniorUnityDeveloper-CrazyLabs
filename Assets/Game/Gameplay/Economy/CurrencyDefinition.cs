using System;
using UnityEngine;

namespace Game.Gameplay.Economy
{
    [CreateAssetMenu(
        fileName = nameof(CurrencyDefinition),
        menuName = "Game/Gameplay/Economy/Currency Definition")]
    public sealed partial class CurrencyDefinition : ScriptableObject
    {
        [SerializeField] private string _saveId;
        [SerializeField] private Sprite _icon;

        public string SaveId => _saveId;
        public Sprite Icon => _icon;

        public static class Serialization
        {
            public const string SaveId = nameof(_saveId);
            public const string Icon = nameof(_icon);
        }

        private void OnValidate()
        {
            EnsureSaveId();
        }

        private void EnsureSaveId()
        {
            if (!string.IsNullOrWhiteSpace(_saveId))
                return;

            _saveId = "currency-" + Guid.NewGuid().ToString("N");
        }
    }
}
