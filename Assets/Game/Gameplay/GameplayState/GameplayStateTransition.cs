using UnityEngine;

namespace Game.Gameplay.GameplayState
{
    [CreateAssetMenu(
        fileName = nameof(GameplayStateTransition),
        menuName = "Game/Gameplay/Gameplay State Transition")]
    public sealed partial class GameplayStateTransition : ScriptableObject
    {
        [SerializeField] private GameplayStateId _fromStateId;
        [SerializeField] private GameplayStateId _toStateId;

        public GameplayStateId FromStateId => _fromStateId;
        public GameplayStateId ToStateId => _toStateId;
    }
}
