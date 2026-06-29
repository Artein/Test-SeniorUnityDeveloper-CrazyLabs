using UnityEngine;

namespace Game.Gameplay.Economy
{
    [CreateAssetMenu(
        fileName = nameof(CurrencyDefinition),
        menuName = "Game/Gameplay/Economy/Currency Definition")]
    public sealed partial class CurrencyDefinition : ScriptableObject
    {
        [SerializeField] private Sprite _icon;

        public Sprite Icon => _icon;
    }
}
