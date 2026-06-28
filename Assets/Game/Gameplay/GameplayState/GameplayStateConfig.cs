using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.GameplayState
{
    public interface IGameplayStateConfig
    {
        GameplayStateId InitialStateId { get; }
        IReadOnlyList<GameplayStateTransition> AllowedTransitions { get; }
    }

    [CreateAssetMenu(
        fileName = nameof(GameplayStateConfig),
        menuName = "Game/Gameplay/Gameplay State Config")]
    public sealed partial class GameplayStateConfig : ScriptableObject, IGameplayStateConfig
    {
        [SerializeField] private GameplayStateId _initialStateId;
        [SerializeField] private List<GameplayStateTransition> _allowedTransitions = new();

        public GameplayStateId InitialStateId => _initialStateId;

        public IReadOnlyList<GameplayStateTransition> AllowedTransitions => _allowedTransitions;

        private void OnValidate()
        {
            var validator = new GameplayStateValidator();
            var errors = validator.Validate(this);

            foreach (var error in errors)
            {
                Debug.LogWarning(error.Message);
            }
        }
    }
}
