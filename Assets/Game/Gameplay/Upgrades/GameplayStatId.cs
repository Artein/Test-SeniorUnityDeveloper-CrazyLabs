using UnityEngine;

namespace Game.Gameplay.Upgrades
{
    [CreateAssetMenu(
        fileName = nameof(GameplayStatId),
        menuName = "Game/Gameplay/Upgrades/Gameplay Stat Id")]
    public sealed partial class GameplayStatId : ScriptableObject
    {
        [SerializeField] private string _id;

        public string Id => _id;
    }
}
