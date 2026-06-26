using System.Collections.Generic;

namespace Game.Gameplay.GameplayState
{
    public interface IGameplayStateValidator
    {
        IReadOnlyList<GameplayStateValidationError> Validate(IGameplayStateConfig config);
    }

    public sealed class GameplayStateValidator : IGameplayStateValidator
    {
        public IReadOnlyList<GameplayStateValidationError> Validate(IGameplayStateConfig config)
        {
            var errors = new List<GameplayStateValidationError>();

            if (config is null)
            {
                errors.Add(new GameplayStateValidationError(
                    GameplayStateValidationErrorCode.NullConfig,
                    null,
                    "Gameplay State config is missing."));

                return errors;
            }

            if (config.InitialStateId == null)
            {
                errors.Add(new GameplayStateValidationError(
                    GameplayStateValidationErrorCode.MissingInitialState,
                    null,
                    "Gameplay State config is missing an initial state id."));
            }

            ValidateTransitions(config.AllowedTransitions, errors);

            return errors;
        }

        private void ValidateTransitions(
            IReadOnlyList<GameplayStateTransition> transitions,
            List<GameplayStateValidationError> errors)
        {
            if (transitions is null)
            {
                errors.Add(new GameplayStateValidationError(
                    GameplayStateValidationErrorCode.NullTransition,
                    null,
                    "Gameplay State config has a missing transition list."));

                return;
            }

            var acceptedTransitions = new List<GameplayStateTransition>();

            foreach (var transition in transitions)
            {
                if (transition == null)
                {
                    errors.Add(new GameplayStateValidationError(
                        GameplayStateValidationErrorCode.NullTransition,
                        null,
                        "Gameplay State config contains a null transition."));

                    continue;
                }

                if (!ValidateTransitionStateIds(transition, errors))
                    continue;

                if (ReferenceEquals(transition.FromStateId, transition.ToStateId))
                {
                    errors.Add(new GameplayStateValidationError(
                        GameplayStateValidationErrorCode.SelfTransition,
                        transition,
                        $"Gameplay State transition '{transition.name}' points to the same state."));

                    continue;
                }

                if (ContainsTransitionPair(
                        acceptedTransitions,
                        transition.FromStateId,
                        transition.ToStateId))
                {
                    errors.Add(new GameplayStateValidationError(
                        GameplayStateValidationErrorCode.DuplicateTransition,
                        transition,
                        $"Gameplay State transition '{transition.name}' duplicates another allowed transition."));

                    continue;
                }

                acceptedTransitions.Add(transition);
            }
        }

        private bool ValidateTransitionStateIds(
            GameplayStateTransition transition,
            List<GameplayStateValidationError> errors)
        {
            var isValid = true;

            if (transition.FromStateId == null)
            {
                errors.Add(new GameplayStateValidationError(
                    GameplayStateValidationErrorCode.MissingTransitionFromState,
                    transition,
                    $"Gameplay State transition '{transition.name}' is missing a from state id."));

                isValid = false;
            }

            if (transition.ToStateId == null)
            {
                errors.Add(new GameplayStateValidationError(
                    GameplayStateValidationErrorCode.MissingTransitionToState,
                    transition,
                    $"Gameplay State transition '{transition.name}' is missing a to state id."));

                isValid = false;
            }

            return isValid;
        }

        private bool ContainsTransitionPair(
            IReadOnlyList<GameplayStateTransition> transitions,
            GameplayStateId fromStateId,
            GameplayStateId toStateId)
        {
            foreach (var transition in transitions)
            {
                if (ReferenceEquals(transition.FromStateId, fromStateId)
                    && ReferenceEquals(transition.ToStateId, toStateId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
