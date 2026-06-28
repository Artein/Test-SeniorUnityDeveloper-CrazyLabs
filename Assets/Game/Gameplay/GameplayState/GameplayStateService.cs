using System;
using System.Linq;
using Game.Utils.Invocation;
using UnityEngine;

namespace Game.Gameplay.GameplayState
{
    public interface IGameplayStateService
    {
        GameplayStateId CurrentStateId { get; }
        
        event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        bool IsCurrent(GameplayStateId stateId);
        bool TryTransitionTo(GameplayStateId nextStateId);
    }

    public sealed class GameplayStateService : IGameplayStateService
    {
        private readonly IGameplayStateConfig _config;
        private readonly IGameplayStateModel _model;

        public GameplayStateId CurrentStateId => _model.CurrentStateId;
        
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanging;
        public event Action<GameplayStateId, GameplayStateId> GameplayStateChanged;

        internal GameplayStateService(
            IGameplayStateConfig config,
            IGameplayStateValidator validator,
            IGameplayStateModel model)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _model = model ?? throw new ArgumentNullException(nameof(model));

            if (validator is null)
                throw new ArgumentNullException(nameof(validator));

            ValidateConfig(validator);
            _model.CurrentStateId = _config.InitialStateId;
        }

        public bool IsCurrent(GameplayStateId stateId)
        {
            return ReferenceEquals(_model.CurrentStateId, stateId);
        }

        public bool TryTransitionTo(GameplayStateId nextStateId)
        {
            var previousStateId = _model.CurrentStateId;

            if (ReferenceEquals(previousStateId, nextStateId))
                return false;

            if (!CanTransitionTo(previousStateId, nextStateId))
            {
                Debug.LogWarning(
                    "Invalid Gameplay State transition from "
                    + $"'{GetStateName(previousStateId)}' to '{GetStateName(nextStateId)}'.");

                return false;
            }

            GameplayStateChanging?.InvokeSafely(nextStateId, previousStateId);
            _model.CurrentStateId = nextStateId;
            GameplayStateChanged?.InvokeSafely(nextStateId, previousStateId);

            return true;
        }

        private void ValidateConfig(IGameplayStateValidator validator)
        {
            var errors = validator.Validate(_config);

            if (errors is null)
            {
                throw new ArgumentException(
                    "Gameplay State validator returned no validation result.",
                    nameof(validator));
            }

            if (errors.Count <= 0)
                return;

            var messages = errors.Select(error => error.Message);

            throw new ArgumentException(
                "Invalid Gameplay State config: " + string.Join(" ", messages),
                nameof(_config));
        }

        private bool CanTransitionTo(GameplayStateId previousStateId, GameplayStateId nextStateId)
        {
            var transitions = _config.AllowedTransitions;

            if (transitions is null)
                return false;

            foreach (var transition in transitions)
            {
                if (transition == null)
                    continue;

                if (ReferenceEquals(transition.FromStateId, previousStateId)
                    && ReferenceEquals(transition.ToStateId, nextStateId))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetStateName(GameplayStateId stateId)
        {
            if (stateId == null)
                return "<null>";

            return stateId.name;
        }
    }
}
